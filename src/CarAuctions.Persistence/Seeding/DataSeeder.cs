using CarAuctions.Application.Common.Interfaces;
using CarAuctions.Domain.Aggregates.Auctions;
using CarAuctions.Domain.Aggregates.Users;
using CarAuctions.Domain.Aggregates.Vehicles;
using CarAuctions.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CarAuctions.Persistence.Seeding;

public class DataSeeder : IDataSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DataSeeder> _logger;
    private readonly IPasswordHasher _passwordHasher;

    public DataSeeder(
        ApplicationDbContext context,
        ILogger<DataSeeder> logger,
        IPasswordHasher passwordHasher)
    {
        _context = context;
        _logger = logger;
        _passwordHasher = passwordHasher;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await SeedUsersAsync(cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            await SeedVehiclesAsync(cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            await SeedAuctionsAsync(cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Database seeding completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while seeding the database");
            throw;
        }
    }

    private async Task SeedUsersAsync(CancellationToken cancellationToken)
    {
        if (await _context.Users.AnyAsync(cancellationToken))
            return;

        var users = new List<User>();

        // Default password for all seeded users: "Password123!"
        var defaultPasswordHash = _passwordHasher.HashPassword("Password123!");

        // Admin user
        var admin = User.Create(
            "admin@carauctions.com",
            defaultPasswordHash,
            "Admin",
            "User",
            UserRole.Admin | UserRole.Buyer | UserRole.Seller);
        if (!admin.IsError) users.Add(admin.Value);

        // Dealer user
        var dealer = User.Create(
            "dealer@example.com",
            defaultPasswordHash,
            "John",
            "Dealer",
            UserRole.Dealer | UserRole.Buyer | UserRole.Seller,
            "+1234567890");
        if (!dealer.IsError)
        {
            dealer.Value.SetDealerInfo("DL-123456", "Premium Auto Sales");
            dealer.Value.SetCreditLimit(Money.Create(500000, "USD"));
            users.Add(dealer.Value);
        }

        // Regular buyer
        var buyer = User.Create(
            "buyer@example.com",
            defaultPasswordHash,
            "Jane",
            "Buyer",
            UserRole.Buyer);
        if (!buyer.IsError)
        {
            buyer.Value.SetCreditLimit(Money.Create(100000, "USD"));
            users.Add(buyer.Value);
        }

        // Seller
        var seller = User.Create(
            "seller@example.com",
            defaultPasswordHash,
            "Bob",
            "Seller",
            UserRole.Seller);
        if (!seller.IsError) users.Add(seller.Value);

        // Inspector
        var inspector = User.Create(
            "inspector@carauctions.com",
            defaultPasswordHash,
            "Inspector",
            "Smith",
            UserRole.Inspector);
        if (!inspector.IsError) users.Add(inspector.Value);

        foreach (var user in users)
        {
            user.Activate();
        }

        await _context.Users.AddRangeAsync(users, cancellationToken);
        _logger.LogInformation("Seeded {Count} users (default password: Password123!)", users.Count);
    }

    private async Task SeedVehiclesAsync(CancellationToken cancellationToken)
    {
        if (await _context.Vehicles.AnyAsync(cancellationToken))
            return;

        var dealer = await _context.Users.FirstOrDefaultAsync(u => u.IsDealer, cancellationToken);
        var seller = await _context.Users.FirstOrDefaultAsync(u => u.Email == "seller@example.com", cancellationToken);
        var inspector = await _context.Users.FirstOrDefaultAsync(u => u.Roles.HasFlag(UserRole.Inspector), cancellationToken);

        if (dealer is null || seller is null || inspector is null)
            return;

        var vehicles = new List<Vehicle>();

        // Sample vehicles with valid VINs (using test VINs)
        var vehicleData = new[]
        {
            ("11111111111111111", "Toyota", "Camry", 2022, 25000, dealer.Id),
            ("22222222222222222", "Honda", "Accord", 2021, 32000, dealer.Id),
            ("33333333333333333", "BMW", "330i", 2023, 15000, dealer.Id),
            ("44444444444444444", "Mercedes-Benz", "C300", 2022, 28000, seller.Id),
            ("55555555555555555", "Ford", "Mustang", 2023, 8000, seller.Id)
        };

        foreach (var (vin, make, model, year, mileage, ownerId) in vehicleData)
        {
            var vinResult = VIN.Create(vin);
            var mileageResult = Mileage.Create(mileage);

            if (vinResult.IsError || mileageResult.IsError)
                continue;

            var vehicleResult = Vehicle.Create(
                vinResult.Value,
                make,
                model,
                year,
                mileageResult.Value,
                ownerId,
                TitleStatus.Clean,
                "Silver",
                "Black",
                "V6",
                "Automatic",
                "Gasoline");

            if (!vehicleResult.IsError)
            {
                var vehicle = vehicleResult.Value;

                // Add condition report
                var report = ConditionReport.Create(
                    ConditionGrade.Grade4,
                    ConditionGrade.Grade4,
                    ConditionGrade.Grade4,
                    ConditionGrade.Grade4,
                    inspector.Id,
                    DateTime.UtcNow.AddDays(-1),
                    "Vehicle in excellent condition");

                vehicle.AddConditionReport(report);
                vehicle.MarkAsListed();

                vehicles.Add(vehicle);
            }
        }

        await _context.Vehicles.AddRangeAsync(vehicles, cancellationToken);
        _logger.LogInformation("Seeded {Count} vehicles", vehicles.Count);
    }

    private async Task SeedAuctionsAsync(CancellationToken cancellationToken)
    {
        if (await _context.Auctions.AnyAsync(cancellationToken))
            return;

        var vehicles = await _context.Vehicles
            .Where(v => v.Status == VehicleStatus.Listed)
            .ToListAsync(cancellationToken);

        var auctions = new List<Auction>();

        foreach (var vehicle in vehicles.Take(3))
        {
            var startingPrice = Money.Create(15000 + (vehicles.IndexOf(vehicle) * 5000), "USD");
            var reservePrice = Money.Create(startingPrice.Amount + 5000, "USD");

            var auctionResult = Auction.Create(
                $"{vehicle.Year} {vehicle.Make} {vehicle.Model}",
                AuctionType.Timed,
                vehicle.Id,
                vehicle.OwnerId,
                startingPrice,
                DateTime.UtcNow.AddHours(-1),
                DateTime.UtcNow.AddDays(3),
                AuctionSettings.Default("USD"),
                reservePrice,
                buyNowPrice: Money.Create(reservePrice.Amount + 10000, "USD"),
                description: $"Beautiful {vehicle.Year} {vehicle.Make} {vehicle.Model} in excellent condition.");

            if (!auctionResult.IsError)
            {
                var auction = auctionResult.Value;
                auction.Schedule();
                auction.Start(DateTime.UtcNow);
                vehicle.MarkAsInAuction();
                auctions.Add(auction);
            }
        }

        await _context.Auctions.AddRangeAsync(auctions, cancellationToken);
        _logger.LogInformation("Seeded {Count} auctions", auctions.Count);
    }
}
