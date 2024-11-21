using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MyMasterpiece.Infrastructure.helper
{
    public static class PasswordHelper
    {
        // Method to create password hash and salt
        public static void CreatePasswordHash(string password, out string passwordHash, out string passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = Convert.ToBase64String(hmac.Key); // Create the salt
                passwordHash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(password))); // Hash the password
            }
        }

        // Method to verify the password with hash and salt
        public static bool VerifyPassword(string password, string storedHash, string storedSalt)
        {
            using (var hmac = new HMACSHA512(Convert.FromBase64String(storedSalt))) // Use stored salt for hashing
            {
                var computedHash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(password))); // Hash the provided password
                return computedHash == storedHash; // Compare the computed hash with the stored hash
            }
        }
    }
}
