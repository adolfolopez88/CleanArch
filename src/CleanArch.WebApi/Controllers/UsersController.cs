using CleanArch.Application.Interfaces;
using CleanArch.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CleanArch.WebApi.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserService userService, ILogger<UsersController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// Registers a new user
        /// </summary>
        /// <param name="model">User registration data</param>
        /// <returns>The result of the registration operation</returns>
        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Check if email already exists
            if (await _userService.CheckEmailExistsAsync(model.Email))
                return BadRequest(new { message = "Email already exists" });

            // Check if username already exists
            if (await _userService.CheckUsernameExistsAsync(model.UserName))
                return BadRequest(new { message = "Username already exists" });

            var result = await _userService.RegisterUserAsync(model);
            if (!result.Result.Succeeded)
            {
                foreach (var error in result.Result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return BadRequest(ModelState);
            }

            _logger.LogInformation("User registered successfully with ID: {UserId}", result.UserId);
            return CreatedAtAction(nameof(GetUserById), new { id = result.UserId, version = "1.0" }, null);
        }

        /// <summary>
        /// Authenticates a user and returns a JWT token
        /// </summary>
        /// <param name="model">Login credentials</param>
        /// <returns>JWT token and user information</returns>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginUserDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _userService.LoginAsync(model);
            if (result == null)
            {
                _logger.LogWarning("Login failed for user: {UserName}", model.UserNameOrEmail);
                return Unauthorized(new { message = "Invalid username or password" });
            }

            _logger.LogInformation("User logged in successfully: {UserName}", result.UserName);
            return Ok(result);
        }

        /// <summary>
        /// Refreshes an expired JWT token
        /// </summary>
        /// <param name="model">The expired token and refresh token</param>
        /// <returns>New JWT token and refresh token</returns>
        [HttpPost("refresh-token")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _userService.RefreshTokenAsync(model);
            if (result == null)
            {
                _logger.LogWarning("Token refresh failed");
                return Unauthorized(new { message = "Invalid token or refresh token" });
            }

            _logger.LogInformation("Token refreshed successfully for user: {UserName}", result.UserName);
            return Ok(result);
        }

        /// <summary>
        /// Logs out a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Result of the logout operation</returns>
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Logout([FromQuery] string userId)
        {
            var result = await _userService.LogoutAsync(userId);
            if (!result)
            {
                _logger.LogWarning("Logout failed for user ID: {UserId}", userId);
                return NotFound(new { message = "User not found" });
            }

            _logger.LogInformation("User logged out successfully: {UserId}", userId);
            return Ok(new { message = "Logged out successfully" });
        }

        /// <summary>
        /// Gets all users
        /// </summary>
        /// <returns>List of all users</returns>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        /// <summary>
        /// Gets a user by ID
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>User data</returns>
        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserDto>> GetUserById(string id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                _logger.LogWarning("User not found with ID: {UserId}", id);
                return NotFound(new { message = "User not found" });
            }

            // Only allow users to access their own data unless they are an admin
            if (id != User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value && 
                !User.IsInRole("Admin"))
            {
                _logger.LogWarning("Unauthorized access attempt to user data: {UserId}", id);
                return Forbid();
            }

            return Ok(user);
        }

        /// <summary>
        /// Gets a user by email
        /// </summary>
        /// <param name="email">User email</param>
        /// <returns>User data</returns>
        [HttpGet("by-email")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserDto>> GetUserByEmail([FromQuery] string email)
        {
            var user = await _userService.GetUserByEmailAsync(email);
            if (user == null)
            {
                _logger.LogWarning("User not found with email: {Email}", email);
                return NotFound(new { message = "User not found" });
            }

            return Ok(user);
        }

        /// <summary>
        /// Updates a user
        /// </summary>
        /// <param name="id">User ID</param>
        /// <param name="model">Updated user data</param>
        /// <returns>Result of the update operation</returns>
        [HttpPut("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Only allow users to update their own data unless they are an admin
            if (id != User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value && 
                !User.IsInRole("Admin"))
            {
                _logger.LogWarning("Unauthorized update attempt for user: {UserId}", id);
                return Forbid();
            }

            var result = await _userService.UpdateUserAsync(id, model);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return BadRequest(ModelState);
            }

            _logger.LogInformation("User updated successfully: {UserId}", id);
            return Ok(new { message = "User updated successfully" });
        }

        /// <summary>
        /// Changes a user's password
        /// </summary>
        /// <param name="id">User ID</param>
        /// <param name="model">Password change data</param>
        /// <returns>Result of the password change operation</returns>
        [HttpPut("{id}/change-password")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ChangePassword(string id, [FromBody] ChangePasswordDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Only allow users to change their own password
            if (id != User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value)
            {
                _logger.LogWarning("Unauthorized password change attempt for user: {UserId}", id);
                return Forbid();
            }

            var result = await _userService.ChangePasswordAsync(id, model);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Password changed successfully for user: {UserId}", id);
            return Ok(new { message = "Password changed successfully" });
        }

        /// <summary>
        /// Deletes a user
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>Result of the delete operation</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var result = await _userService.DeleteUserAsync(id);
            if (!result.Succeeded)
            {
                _logger.LogWarning("Delete user failed: {UserId}", id);
                return NotFound(new { message = "User not found or could not be deleted" });
            }

            _logger.LogInformation("User deleted successfully: {UserId}", id);
            return Ok(new { message = "User deleted successfully" });
        }

        /// <summary>
        /// Initiates the forgot password process
        /// </summary>
        /// <param name="model">Forgot password data</param>
        /// <returns>Result of the forgot password operation</returns>
        [HttpPost("forgot-password")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _userService.ForgetPasswordAsync(model.Email);
            if (!result.Succeeded)
            {
                // Don't reveal that the user does not exist
                _logger.LogWarning("Forgot password request for non-existent email: {Email}", model.Email);
                return Ok(new { message = "If your email is registered, you will receive a password reset link" });
            }

            // In a real application, you would send an email with the reset token
            // For this example, we'll just return the token
            _logger.LogInformation("Password reset requested for email: {Email}", model.Email);
            return Ok(new { 
                message = "Password reset email sent", 
                token = result.Token // In production, you would not return this to the client
            });
        }

        /// <summary>
        /// Resets a user's password
        /// </summary>
        /// <param name="model">Reset password data</param>
        /// <returns>Result of the reset password operation</returns>
        [HttpPost("reset-password")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _userService.ResetPasswordAsync(model);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Password reset successfully for email: {Email}", model.Email);
            return Ok(new { message = "Password has been reset" });
        }

        /// <summary>
        /// Gets a user's roles
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>List of roles</returns>
        [HttpGet("{id}/roles")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<string>>> GetUserRoles(string id)
        {
            var roles = await _userService.GetUserRolesAsync(id);
            if (roles.Count == 0)
            {
                // This could mean either user not found or user has no roles
                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                {
                    _logger.LogWarning("Get roles failed: User not found with ID: {UserId}", id);
                    return NotFound(new { message = "User not found" });
                }
            }

            return Ok(roles);
        }

        /// <summary>
        /// Adds a role to a user
        /// </summary>
        /// <param name="id">User ID</param>
        /// <param name="role">Role name</param>
        /// <returns>Result of the add role operation</returns>
        [HttpPost("{id}/roles")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddUserToRole(string id, [FromQuery] string role)
        {
            if (string.IsNullOrEmpty(role))
                return BadRequest(new { message = "Role name is required" });

            var result = await _userService.AddUserToRoleAsync(id, role);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return BadRequest(ModelState);
            }

            _logger.LogInformation("User {UserId} added to role {Role}", id, role);
            return Ok(new { message = $"User added to role {role} successfully" });
        }

        /// <summary>
        /// Removes a role from a user
        /// </summary>
        /// <param name="id">User ID</param>
        /// <param name="role">Role name</param>
        /// <returns>Result of the remove role operation</returns>
        [HttpDelete("{id}/roles")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveUserFromRole(string id, [FromQuery] string role)
        {
            if (string.IsNullOrEmpty(role))
                return BadRequest(new { message = "Role name is required" });

            var result = await _userService.RemoveUserFromRoleAsync(id, role);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return BadRequest(ModelState);
            }

            _logger.LogInformation("User {UserId} removed from role {Role}", id, role);
            return Ok(new { message = $"User removed from role {role} successfully" });
        }
    }
}