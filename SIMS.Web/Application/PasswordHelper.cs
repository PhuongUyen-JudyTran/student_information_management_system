using System;
using System.Security.Cryptography;
using System.Text;

namespace SIMS.Application
{
    public static class PasswordHelper
    {
        public static string HashPassword(string password)
        {
            if (password == null) throw new ArgumentNullException(nameof(password));

            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hashBytes = sha256.ComputeHash(bytes);
                
                var sb = new StringBuilder();
                foreach (var b in hashBytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }

        public static bool VerifyPassword(string password, string hash)
        {
            if (password == null || hash == null) return false;
            
            var passwordHash = HashPassword(password);
            return string.Equals(passwordHash, hash, StringComparison.Ordinal);
        }
    }
}
