using CleanArch.Domain.Entities;
using CleanArch.Domain.Models;
using Microsoft.AspNetCore.Identity;

namespace CleanArch.Application.Interfaces
{
    public interface IUserService
    {
        Task<(IdentityResult Result, string UserId)> RegisterUserAsync(RegisterUserDto model, string role = "User");
        Task<AuthResponse?> LoginAsync(LoginUserDto model);
        Task<AuthResponse?> RefreshTokenAsync(RefreshTokenDto model);
        Task<bool> LogoutAsync(string userId);
        Task<List<UserDto>> GetAllUsersAsync();
        Task<UserDto?> GetUserByIdAsync(string userId);
        Task<UserDto?> GetUserByEmailAsync(string email);
        Task<IdentityResult> UpdateUserAsync(string userId, UpdateUserDto model);
        Task<IdentityResult> ChangePasswordAsync(string userId, ChangePasswordDto model);
        Task<IdentityResult> DeleteUserAsync(string userId);
        Task<(bool Succeeded, string Token)> ForgetPasswordAsync(string email);
        Task<IdentityResult> ResetPasswordAsync(ResetPasswordDto model);
        Task<List<string>> GetUserRolesAsync(string userId);
        Task<IdentityResult> AddUserToRoleAsync(string userId, string role);
        Task<IdentityResult> RemoveUserFromRoleAsync(string userId, string role);
        Task<bool> CheckEmailExistsAsync(string email);
        Task<bool> CheckUsernameExistsAsync(string username);
    }
}