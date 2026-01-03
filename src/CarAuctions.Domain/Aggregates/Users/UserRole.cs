namespace CarAuctions.Domain.Aggregates.Users;

[Flags]
public enum UserRole
{
    None = 0,
    Buyer = 1,
    Seller = 2,
    Dealer = 4,
    Inspector = 8,
    Auctioneer = 16,
    Finance = 32,
    Compliance = 64,
    Support = 128,
    Admin = 256,
    SuperAdmin = 512
}
