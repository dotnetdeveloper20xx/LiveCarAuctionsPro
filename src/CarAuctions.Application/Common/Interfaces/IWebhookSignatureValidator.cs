namespace CarAuctions.Application.Common.Interfaces;

public interface IWebhookSignatureValidator
{
    bool ValidateSignature(string payload, string signature, string secret);
    string ComputeSignature(string payload, string secret);
}
