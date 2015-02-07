using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace ZNS.EliteTracker.Models
{
    public class Password
    {
        private const int SALT_BYTE_SIZE = 24;

        public static bool Authenticate(string stringPassword, string hashedPassword, string salt)
        {
            return ComputeHash(stringPassword, salt) == hashedPassword;
        }

        public static string HashPassword(string password, string salt)
        {
            return ComputeHash(password, salt);
        }
        
        public static string GenerateSalt()
        {
            RNGCryptoServiceProvider csprng = new RNGCryptoServiceProvider();
            var saltBytes = new byte[SALT_BYTE_SIZE];
            csprng.GetBytes(saltBytes);
            return Convert.ToBase64String(saltBytes);
        }

        private static string ComputeHash(string input, string salt)
        {
            Byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            Byte[] saltBytes = Convert.FromBase64String(salt);

            // Combine salt and input bytes
            Byte[] saltedInput = new Byte[salt.Length + inputBytes.Length];
            saltBytes.CopyTo(saltedInput, 0);
            inputBytes.CopyTo(saltedInput, salt.Length);

            var provider = new SHA256CryptoServiceProvider();
            Byte[] hashedBytes = provider.ComputeHash(saltedInput);

            return Convert.ToBase64String(hashedBytes);
        }
    }
}