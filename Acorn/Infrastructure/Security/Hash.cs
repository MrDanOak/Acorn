using System.Security.Cryptography;

namespace Acorn.Infrastructure.Security;

internal static class Hash
{
    public static string HashPassword(string username, string password, out byte[] salt)
    {
        // Generate a 16-byte salt using RandomNumberGenerator
        salt = new byte[16];
        var rng = RandomNumberGenerator.Create();
        rng.GetBytes(salt);

        // Combine username and password
        var combined = username + password;

        // Hash the combined string using PBKDF2
        using var pbkdf2 = new Rfc2898DeriveBytes(combined, salt, 10000, HashAlgorithmName.SHA256);
        var hash = pbkdf2.GetBytes(32); // Generate a 32-byte hash
        return Convert.ToBase64String(hash);
    }

    public static bool VerifyPassword(string username, string password, byte[] salt, string storedHash)
    {
        // Combine username and password
        var combined = username + password;

        // Hash the combined string using PBKDF2 with the same salt
        using var pbkdf2 = new Rfc2898DeriveBytes(combined, salt, 10000, HashAlgorithmName.SHA256);
        var hash = pbkdf2.GetBytes(32); // Generate a 32-byte hash
        var hashString = Convert.ToBase64String(hash);
        return hashString == storedHash;
    }
}