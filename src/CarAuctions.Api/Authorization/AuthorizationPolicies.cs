using Microsoft.AspNetCore.Authorization;

namespace CarAuctions.Api.Authorization;

public static class AuthorizationPolicies
{
    public const string RequireAdmin = "RequireAdmin";
    public const string RequireBuyer = "RequireBuyer";
    public const string RequireSeller = "RequireSeller";
    public const string RequireDealer = "RequireDealer";
    public const string RequireKycVerified = "RequireKycVerified";

    public static void AddPolicies(AuthorizationOptions options)
    {
        options.AddPolicy(RequireAdmin, policy =>
            policy.RequireRole("Admin"));

        options.AddPolicy(RequireBuyer, policy =>
            policy.RequireRole("Buyer", "Dealer", "Admin"));

        options.AddPolicy(RequireSeller, policy =>
            policy.RequireRole("Seller", "Dealer", "Admin"));

        options.AddPolicy(RequireDealer, policy =>
            policy.RequireRole("Dealer", "Admin"));

        options.AddPolicy(RequireKycVerified, policy =>
            policy.RequireClaim("kyc_verified", "true"));
    }
}
