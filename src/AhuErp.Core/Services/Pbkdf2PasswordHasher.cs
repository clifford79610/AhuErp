using System;
using System.Security.Cryptography;

namespace AhuErp.Core.Services
{
    /// <summary>
    /// PBKDF2-SHA256 реализация <see cref="IPasswordHasher"/>. Формат хэша:
    /// <c>{iterations}.{base64(salt)}.{base64(hash)}</c>. Сравнение —
    /// constant-time, чтобы не давать ориентиров таймингового канала.
    /// </summary>
    public sealed class Pbkdf2PasswordHasher : IPasswordHasher
    {
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int DefaultIterations = 100_000;

        public int Iterations { get; }

        public Pbkdf2PasswordHasher(int iterations = DefaultIterations)
        {
            if (iterations < 1_000)
                throw new ArgumentOutOfRangeException(nameof(iterations),
                    "Минимум 1000 итераций PBKDF2.");
            Iterations = iterations;
        }

        public string Hash(string password)
        {
            if (password == null) throw new ArgumentNullException(nameof(password));

            var salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            var hash = DeriveKey(password, salt, Iterations, HashSize);
            return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
        }

        public bool Verify(string password, string hash)
        {
            if (password == null) throw new ArgumentNullException(nameof(password));
            if (string.IsNullOrEmpty(hash)) return false;

            var parts = hash.Split('.');
            if (parts.Length != 3) return false;

            if (!int.TryParse(parts[0], out var iterations) || iterations < 1_000)
                return false;

            byte[] salt, expected;
            try
            {
                salt = Convert.FromBase64String(parts[1]);
                expected = Convert.FromBase64String(parts[2]);
            }
            catch (FormatException)
            {
                return false;
            }

            var actual = DeriveKey(password, salt, iterations, expected.Length);
            return ConstantTimeEquals(actual, expected);
        }

        private static byte[] DeriveKey(string password, byte[] salt, int iterations, int length)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256))
            {
                return pbkdf2.GetBytes(length);
            }
        }

        private static bool ConstantTimeEquals(byte[] a, byte[] b)
        {
            if (a.Length != b.Length) return false;
            var result = 0;
            for (var i = 0; i < a.Length; i++) result |= a[i] ^ b[i];
            return result == 0;
        }
    }
}
