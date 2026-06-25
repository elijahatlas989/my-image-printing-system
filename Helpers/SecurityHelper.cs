using System.Security.Cryptography;
using System.Text;

namespace MyImagePrinting.Helpers;

// Contains a basic password hashing method for this college project.
public static class SecurityHelper
{
    public static string HashPassword(string password)
    {
        // Convert the password into a one-way SHA-256 hash before saving it.
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes);
    }
}
