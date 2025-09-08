
using System.Security.Cryptography;
using System.Text;

namespace ClientID_SecretAuth.Api.Helpers;

public static class SecretHasher
{
    public static (string hash, string salt) HashSecret(string secret)
    {
        var saltBytes = RandomNumberGenerator.GetBytes(16);
        var salt = Convert.ToBase64String(saltBytes);

        using var sha256 = SHA256.Create();
        var combined = Encoding.UTF8.GetBytes(secret + salt);
        var hash = Convert.ToBase64String(sha256.ComputeHash(combined));

        return (hash, salt);
    }

    public static bool VerifySecret(string secret, string salt, string storedHash)
    {
        using var sha256 = SHA256.Create();
        var combined = Encoding.UTF8.GetBytes(secret + salt);
        var computedHash = Convert.ToBase64String(sha256.ComputeHash(combined));

        return storedHash == computedHash;
    }
}
