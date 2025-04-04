namespace CleanArch.Domain.Interfaces
{
    public interface ICryptoService
    {
        string HashPassword(string password);
        bool VerifyPassword(string hashedPassword, string providedPassword);
        string Encrypt(string text);
        string Decrypt(string cipherText);
    }
}