//Hibrael Andre Cidade Xavier
using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;

namespace AcademiaDoZe.Application.Security
{
    public static class PasswordHasher
    {
        private const int Iterations = 3;
        private const int MemorySize = 64 * 1024;
        private const int DegreeOfParallelism = 1;
        private const int HashLength = 32;

        public static string HashPassword(string password)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(password);

            var salt = RandomNumberGenerator.GetBytes(16);
            var hash = ComputeHash(password, salt);

            return Convert.ToBase64String(salt) + ":" + Convert.ToBase64String(hash);
        }

        public static bool VerifyPassword(string password, string storedHash)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(password);
            ArgumentException.ThrowIfNullOrWhiteSpace(storedHash);

            var parts = storedHash.Split(':', 2);
            if (parts.Length != 2)
                return false;

            try
            {
                var salt = Convert.FromBase64String(parts[0]);
                var expectedHash = Convert.FromBase64String(parts[1]);
                var actualHash = ComputeHash(password, salt);

                return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private static byte[] ComputeHash(string password, byte[] salt)
        {
            var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
            {
                DegreeOfParallelism = DegreeOfParallelism,
                Iterations = Iterations,
                MemorySize = MemorySize,
                Salt = salt,
            };

            return argon2.GetBytes(HashLength);
        }
    }
}
