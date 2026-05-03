using System.Security.Cryptography;
using System.Text;

namespace FitServiceCRM.Services;

public static class PasswordService
{
    private const string Salt = "FitServiceCRM-2026-Diploma";

    public static string Hash(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes($"{Salt}|{password}"));
        return Convert.ToHexString(bytes);
    }

    public static bool Verify(string password, string hash) =>
        string.Equals(Hash(password), hash, StringComparison.OrdinalIgnoreCase);
}
