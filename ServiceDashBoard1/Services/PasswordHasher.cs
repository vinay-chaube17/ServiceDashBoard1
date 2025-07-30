using System.Security.Cryptography;

using System.Text;

namespace ServiceDashBoard1.Services
{
    // This class handles password hashing and verification using SHA256.
    // HashPassword() converts plain text password into a hashed format.
    // VerifyPassword() checks if entered password matches the stored hashed password.
    // SHA256 is one-way hashing, so original password can't be reversed.
    // Note: SHA256 is okay for basic use, but in real apps use stronger methods like BCrypt.

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

