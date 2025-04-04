using CleanArch.Application.Interfaces;
using CleanArch.Domain.Entities;
using CleanArch.Domain.Interfaces;
using CleanArch.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace CleanArch.Application.AppServices
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ISecurityConfiguration _securityConfig;
        private readonly ILogger<UserService> _logger;

        public UserService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            SignInManager<ApplicationUser> signInManager,
            ISecurityConfiguration securityConfig,
            ILogger<UserService> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _securityConfig = securityConfig;
            _logger = logger;
        }

        public async Task<(IdentityResult Result, string UserId)> RegisterUserAsync(RegisterUserDto model, string role = "User")
        {
            var user = new ApplicationUser
            {
                UserName = model.UserName,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                PhoneNumber = model.PhoneNumber,
                CreatedAt = DateTimeOffset.UtcNow,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Check if role exists
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    // Create role if it doesn't exist
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }

                // Add user to role
                await _userManager.AddToRoleAsync(user, role);
                
                _logger.LogInformation("User {UserName} ({Email}) registered successfully", user.UserName, user.Email);
                return (result, user.Id);
            }
            
            _logger.LogWarning("User registration failed: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
            return (result, string.Empty);
        }

        public async Task<AuthResponse?> LoginAsync(LoginUserDto model)
        {
            // Find user by username or email
            var user = await _userManager.FindByNameAsync(model.UserNameOrEmail) 
                ?? await _userManager.FindByEmailAsync(model.UserNameOrEmail);
            
            if (user == null)
            {
                _logger.LogWarning("Login failed: User {UserNameOrEmail} not found", model.UserNameOrEmail);
                return null;
            }

            // Check if user is active
            if (!user.IsActive)
            {
                _logger.LogWarning("Login failed: User {UserName} is inactive", user.UserName);
                return null;
            }

            // Check password
            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: true);
            
            if (!result.Succeeded)
            {
                if (result.IsLockedOut)
                    _logger.LogWarning("Login failed: User {UserName} is locked out", user.UserName);
                else 
                    _logger.LogWarning("Login failed: Invalid password for user {UserName}", user.UserName);
                    
                return null;
            }

            // Generate JWT token
            var roles = await _userManager.GetRolesAsync(user);
            var token = GenerateJwtToken(user, roles.ToList());
            var refreshToken = GenerateRefreshToken();
            
            // Update refresh token in database
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_securityConfig.RefreshTokenExpirationDays);
            await _userManager.UpdateAsync(user);
            
            _logger.LogInformation("User {UserName} logged in successfully", user.UserName);

            // Create auth response
            return new AuthResponse
            {
                Token = token,
                RefreshToken = refreshToken,
                Expiration = DateTime.UtcNow.AddMinutes(_securityConfig.JwtExpirationMinutes),
                Roles = roles.ToList(),
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName
            };
        }

        public async Task<AuthResponse?> RefreshTokenAsync(RefreshTokenDto model)
        {
            var principal = GetPrincipalFromExpiredToken(model.Token);
            if (principal == null)
            {
                _logger.LogWarning("Refresh token failed: Invalid token");
                return null;
            }

            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Refresh token failed: User ID not found in token");
                return null;
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || user.RefreshToken != model.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                _logger.LogWarning("Refresh token failed: Invalid refresh token or expired");
                return null;
            }

            var roles = await _userManager.GetRolesAsync(user);
            var newToken = GenerateJwtToken(user, roles.ToList());
            var newRefreshToken = GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_securityConfig.RefreshTokenExpirationDays);
            await _userManager.UpdateAsync(user);

            _logger.LogInformation("User {UserName} refreshed token successfully", user.UserName);

            return new AuthResponse
            {
                Token = newToken,
                RefreshToken = newRefreshToken,
                Expiration = DateTime.UtcNow.AddMinutes(_securityConfig.JwtExpirationMinutes),
                Roles = roles.ToList(),
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName
            };
        }

        public async Task<bool> LogoutAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Logout failed: User {UserId} not found", userId);
                return false;
            }

            // Clear refresh token
            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
            await _userManager.UpdateAsync(user);

            _logger.LogInformation("User {UserName} logged out successfully", user.UserName);
            return true;
        }

        public async Task<List<UserDto>> GetAllUsersAsync()
        {
            var users = _userManager.Users.Where(u => !u.IsDeleted).ToList();
            var userDtos = new List<UserDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userDtos.Add(new UserDto
                {
                    Id = user.Id,
                    UserName = user.UserName ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    PhoneNumber = user.PhoneNumber ?? string.Empty,
                    IsActive = user.IsActive,
                    Roles = roles,
                    CreatedAt = user.CreatedAt
                });
            }

            return userDtos;
        }

        public async Task<UserDto?> GetUserByIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || user.IsDeleted)
            {
                _logger.LogWarning("Get user failed: User {UserId} not found", userId);
                return null;
            }

            var roles = await _userManager.GetRolesAsync(user);
            return new UserDto
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                IsActive = user.IsActive,
                Roles = roles,
                CreatedAt = user.CreatedAt
            };
        }

        public async Task<UserDto?> GetUserByEmailAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null || user.IsDeleted)
            {
                _logger.LogWarning("Get user failed: User with email {Email} not found", email);
                return null;
            }

            var roles = await _userManager.GetRolesAsync(user);
            return new UserDto
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                IsActive = user.IsActive,
                Roles = roles,
                CreatedAt = user.CreatedAt
            };
        }

        public async Task<IdentityResult> UpdateUserAsync(string userId, UpdateUserDto model)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || user.IsDeleted)
            {
                _logger.LogWarning("Update user failed: User {UserId} not found", userId);
                return IdentityResult.Failed(new IdentityError { Description = "User not found" });
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.PhoneNumber = model.PhoneNumber;
            user.ModifiedAt = DateTimeOffset.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
                _logger.LogInformation("User {UserName} updated successfully", user.UserName);
            else
                _logger.LogWarning("Update user failed: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));

            return result;
        }

        public async Task<IdentityResult> ChangePasswordAsync(string userId, ChangePasswordDto model)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || user.IsDeleted)
            {
                _logger.LogWarning("Change password failed: User {UserId} not found", userId);
                return IdentityResult.Failed(new IdentityError { Description = "User not found" });
            }

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (result.Succeeded)
                _logger.LogInformation("User {UserName} changed password successfully", user.UserName);
            else
                _logger.LogWarning("Change password failed: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));

            return result;
        }

        public async Task<IdentityResult> DeleteUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Delete user failed: User {UserId} not found", userId);
                return IdentityResult.Failed(new IdentityError { Description = "User not found" });
            }

            // Soft delete
            user.IsDeleted = true;
            user.ModifiedAt = DateTimeOffset.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
                _logger.LogInformation("User {UserName} deleted successfully", user.UserName);
            else
                _logger.LogWarning("Delete user failed: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));

            return result;
        }

        public async Task<(bool Succeeded, string Token)> ForgetPasswordAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null || user.IsDeleted || !user.IsActive)
            {
                _logger.LogWarning("Forget password failed: User with email {Email} not found or inactive", email);
                return (false, string.Empty);
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            _logger.LogInformation("Password reset token generated for user {UserName}", user.UserName);
            
            return (true, token);
        }

        public async Task<IdentityResult> ResetPasswordAsync(ResetPasswordDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || user.IsDeleted || !user.IsActive)
            {
                _logger.LogWarning("Reset password failed: User with email {Email} not found or inactive", model.Email);
                return IdentityResult.Failed(new IdentityError { Description = "User not found or inactive" });
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
            if (result.Succeeded)
                _logger.LogInformation("User {UserName} reset password successfully", user.UserName);
            else
                _logger.LogWarning("Reset password failed: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));

            return result;
        }

        public async Task<List<string>> GetUserRolesAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || user.IsDeleted)
            {
                _logger.LogWarning("Get user roles failed: User {UserId} not found", userId);
                return new List<string>();
            }

            var roles = await _userManager.GetRolesAsync(user);
            return roles.ToList();
        }

        public async Task<IdentityResult> AddUserToRoleAsync(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || user.IsDeleted)
            {
                _logger.LogWarning("Add user to role failed: User {UserId} not found", userId);
                return IdentityResult.Failed(new IdentityError { Description = "User not found" });
            }

            // Check if role exists
            if (!await _roleManager.RoleExistsAsync(role))
            {
                _logger.LogWarning("Add user to role failed: Role {Role} does not exist", role);
                return IdentityResult.Failed(new IdentityError { Description = $"Role {role} does not exist" });
            }

            // Check if user is already in role
            if (await _userManager.IsInRoleAsync(user, role))
            {
                _logger.LogWarning("Add user to role failed: User {UserName} is already in role {Role}", user.UserName, role);
                return IdentityResult.Failed(new IdentityError { Description = $"User is already in role {role}" });
            }

            var result = await _userManager.AddToRoleAsync(user, role);
            if (result.Succeeded)
                _logger.LogInformation("User {UserName} added to role {Role} successfully", user.UserName, role);
            else
                _logger.LogWarning("Add user to role failed: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));

            return result;
        }

        public async Task<IdentityResult> RemoveUserFromRoleAsync(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || user.IsDeleted)
            {
                _logger.LogWarning("Remove user from role failed: User {UserId} not found", userId);
                return IdentityResult.Failed(new IdentityError { Description = "User not found" });
            }

            // Check if role exists
            if (!await _roleManager.RoleExistsAsync(role))
            {
                _logger.LogWarning("Remove user from role failed: Role {Role} does not exist", role);
                return IdentityResult.Failed(new IdentityError { Description = $"Role {role} does not exist" });
            }

            // Check if user is in role
            if (!await _userManager.IsInRoleAsync(user, role))
            {
                _logger.LogWarning("Remove user from role failed: User {UserName} is not in role {Role}", user.UserName, role);
                return IdentityResult.Failed(new IdentityError { Description = $"User is not in role {role}" });
            }

            var result = await _userManager.RemoveFromRoleAsync(user, role);
            if (result.Succeeded)
                _logger.LogInformation("User {UserName} removed from role {Role} successfully", user.UserName, role);
            else
                _logger.LogWarning("Remove user from role failed: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));

            return result;
        }

        public async Task<bool> CheckEmailExistsAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            return user != null && !user.IsDeleted;
        }

        public async Task<bool> CheckUsernameExistsAsync(string username)
        {
            var user = await _userManager.FindByNameAsync(username);
            return user != null && !user.IsDeleted;
        }

        #region Private Helper Methods

        private string GenerateJwtToken(ApplicationUser user, List<string> roles)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("FirstName", user.FirstName),
                new Claim("LastName", user.LastName)
            };

            // Add roles as claims
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_securityConfig.JwtSecret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddMinutes(_securityConfig.JwtExpirationMinutes);

            var token = new JwtSecurityToken(
                issuer: _securityConfig.JwtIssuer,
                audience: _securityConfig.JwtAudience,
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _securityConfig.JwtIssuer,
                ValidAudience = _securityConfig.JwtAudience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_securityConfig.JwtSecret)),
                ValidateLifetime = false // We don't care about the token's expiration date
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
                if (securityToken is not JwtSecurityToken jwtSecurityToken || 
                    !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    return null;
                }
                return principal;
            }
            catch
            {
                return null;
            }
        }

        #endregion
    }
}