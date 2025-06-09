using System.Security.Cryptography;

using System.Text;

namespace ServiceDashBoard1.Services
{
    
        public static class PasswordHasher
        {
            // Hash password using SHA256
            public static string HashPassword(string password)
            {
                using (SHA256 sha256 = SHA256.Create())
                {
                    byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                    StringBuilder builder = new StringBuilder();
                    foreach (var b in bytes)
                    {
                        builder.Append(b.ToString("x2")); // convert to hex
                    }
                    return builder.ToString();
                }
            }

            // Verify password by comparing hash
            public static bool VerifyPassword(string enteredPassword, string storedHashedPassword)
            {
                string enteredHashed = HashPassword(enteredPassword);
                return enteredHashed == storedHashedPassword;
            }
        }




    }

