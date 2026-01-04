using System.Security.Cryptography;
using System.Text;
using CarAuctions.Application.Common.Interfaces;

namespace CarAuctions.Infrastructure.Services;

public class WebhookSignatureValidator : IWebhookSignatureValidator
{
    public bool ValidateSignature(string payload, string signature, string secret)
    {
        if (string.IsNullOrEmpty(payload) || string.IsNullOrEmpty(signature) || string.IsNullOrEmpty(secret))
            return false;

        var expectedSignature = ComputeSignature(payload, secret);

        // Use constant-time comparison to prevent timing attacks
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(signature),
            Encoding.UTF8.GetBytes(expectedSignature));
    }

    public string ComputeSignature(string payload, string secret)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
