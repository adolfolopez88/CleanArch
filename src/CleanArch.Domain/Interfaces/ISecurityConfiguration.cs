namespace CleanArch.Domain.Interfaces
{
    public interface ISecurityConfiguration
    {
        string JwtSecret { get; set; }
        string JwtIssuer { get; set; }
        string JwtAudience { get; set; }
        int JwtExpirationMinutes { get; set; }
        int RefreshTokenExpirationDays { get; set; }
        string EncryptionKey { get; set; }
    }
}