using System.Security.Cryptography;
using System.Text;

namespace StudentPayments_API.Utils;
public static class HashHelper
{
    public static string ComputeSha256Hash(string rawData)
    {
        var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
        var builder = new StringBuilder();
        foreach (var b in bytes)
            builder.Append(b.ToString("x2"));
        return builder.ToString();
    }
}