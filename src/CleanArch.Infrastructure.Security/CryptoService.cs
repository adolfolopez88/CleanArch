using CleanArch.Domain.Interfaces;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace CleanArch.Infrastructure.Security
{
    public class CryptoService : ICryptoService
    {
        private readonly ILogger<CryptoService> _logger;
        private readonly SecurityConfiguration _securityConfig;

        public CryptoService(ILogger<CryptoService> logger, SecurityConfiguration securityConfig)
        {
            _logger = logger;
            _securityConfig = securityConfig;
        }

        public string HashPassword(string password)
        {
            try
            {
                // Generate a random salt
                byte[] salt = new byte[128 / 8];
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(salt);
                }

                // Derive a 256-bit subkey (use HMACSHA256 with 10,000 iterations)
                string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                    password: password,
                    salt: salt,
                    prf: KeyDerivationPrf.HMACSHA256,
                    iterationCount: 10000,
                    numBytesRequested: 256 / 8));

                // Format: {salt}:{hashed}
                return $"{Convert.ToBase64String(salt)}:{hashed}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error hashing password");
                throw;
            }
        }

        public bool VerifyPassword(string hashedPassword, string providedPassword)
        {
            try
            {
                // Extract the salt and the hashed password
                var parts = hashedPassword.Split(':');
                if (parts.Length != 2)
                {
                    return false;
                }

                var salt = Convert.FromBase64String(parts[0]);
                var originalHashedPassword = parts[1];

                // Hash the incoming password with the same salt
                string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                    password: providedPassword,
                    salt: salt,
                    prf: KeyDerivationPrf.HMACSHA256,
                    iterationCount: 10000,
                    numBytesRequested: 256 / 8));

                // Compare the hashed passwords
                return hashed == originalHashedPassword;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying password");
                return false;
            }
        }

        public string Encrypt(string text)
        {
            try
            {
                byte[] iv = new byte[16];
                byte[] array;

                using (Aes aes = Aes.Create())
                {
                    aes.Key = Encoding.UTF8.GetBytes(_securityConfig.EncryptionKey);
                    aes.IV = iv;

                    ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                        {
                            using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
                            {
                                streamWriter.Write(text);
                            }

                            array = memoryStream.ToArray();
                        }
                    }
                }

                return Convert.ToBase64String(array);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error encrypting text");
                throw;
            }
        }

        public string Decrypt(string cipherText)
        {
            try
            {
                byte[] iv = new byte[16];
                byte[] buffer = Convert.FromBase64String(cipherText);

                using (Aes aes = Aes.Create())
                {
                    aes.Key = Encoding.UTF8.GetBytes(_securityConfig.EncryptionKey);
                    aes.IV = iv;
                    ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                    using (MemoryStream memoryStream = new MemoryStream(buffer))
                    {
                        using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader streamReader = new StreamReader(cryptoStream))
                            {
                                return streamReader.ReadToEnd();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error decrypting text");
                throw;
            }
        }
    }
}