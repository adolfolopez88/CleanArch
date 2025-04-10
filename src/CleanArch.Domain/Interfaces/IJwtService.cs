namespace CleanArch.Domain.Interfaces
{
    public interface IJwtService
    {
        /// <summary>
        /// Generate a JWT token for the given user ID and roles
        /// </summary>
        /// <param name="userId">User's unique identifier</param>
        /// <param name="email">User's email address</param>
        /// <param name="roles">User's roles</param>
        /// <returns>The generated JWT token</returns>
        string GenerateJwtToken(string userId, string email, IList<string> roles);

        /// <summary>
        /// Generate a refresh token
        /// </summary>
        /// <returns>A refresh token string</returns>
        string GenerateRefreshToken();

        /// <summary>
        /// Validate the JWT token
        /// </summary>
        /// <param name="token">The JWT token to validate</param>
        /// <returns>True if the token is valid, false otherwise</returns>
        bool ValidateToken(string token);

        /// <summary>
        /// Get the user ID from the JWT token
        /// </summary>
        /// <param name="token">The JWT token</param>
        /// <returns>The user ID</returns>
        string GetUserIdFromToken(string token);
    }
}