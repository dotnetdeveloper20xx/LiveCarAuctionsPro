using CarAuctions.Application.Common.Interfaces;
using CarAuctions.Domain.Aggregates.Auctions;
using CarAuctions.Domain.Aggregates.Bids;
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

    // Store references for cross-seeding
    private readonly Dictionary<string, User> _users = new();
    private readonly List<Vehicle> _vehicles = new();
    private readonly List<Auction> _auctions = new();

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

            await SeedBidsAsync(cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Database seeding completed successfully");
            _logger.LogInformation("=== TEST CREDENTIALS === All users password: Password123!");
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
        var defaultPasswordHash = _passwordHasher.HashPassword("Password123!");

        // ============ ADMIN USER ============
        var admin = User.Create(
            "admin@carauctions.com",
            defaultPasswordHash,
            "Admin",
            "Administrator",
            UserRole.Admin | UserRole.Buyer | UserRole.Seller);
        if (!admin.IsError)
        {
            admin.Value.Activate();
            admin.Value.VerifyKyc();
            users.Add(admin.Value);
            _users["admin"] = admin.Value;
        }

        // ============ DEALERS ============
        var dealer1 = User.Create(
            "dealer@example.com",
            defaultPasswordHash,
            "John",
            "Premium",
            UserRole.Dealer | UserRole.Buyer | UserRole.Seller,
            "+1-555-0101");
        if (!dealer1.IsError)
        {
            dealer1.Value.SetDealerInfo("DL-2024-001", "Premium Auto Sales");
            dealer1.Value.SetCreditLimit(Money.Create(500000, "USD"));
            dealer1.Value.Activate();
            dealer1.Value.VerifyKyc();
            users.Add(dealer1.Value);
            _users["dealer1"] = dealer1.Value;
        }

        var dealer2 = User.Create(
            "luxurycars@example.com",
            defaultPasswordHash,
            "Sarah",
            "Luxury",
            UserRole.Dealer | UserRole.Buyer | UserRole.Seller,
            "+1-555-0102");
        if (!dealer2.IsError)
        {
            dealer2.Value.SetDealerInfo("DL-2024-002", "Luxury Motors Inc");
            dealer2.Value.SetCreditLimit(Money.Create(1000000, "USD"));
            dealer2.Value.Activate();
            dealer2.Value.VerifyKyc();
            users.Add(dealer2.Value);
            _users["dealer2"] = dealer2.Value;
        }

        // ============ BUYERS ============
        var buyer1 = User.Create(
            "buyer@example.com",
            defaultPasswordHash,
            "Jane",
            "Smith",
            UserRole.Buyer,
            "+1-555-0201");
        if (!buyer1.IsError)
        {
            buyer1.Value.SetCreditLimit(Money.Create(100000, "USD"));
            buyer1.Value.Activate();
            buyer1.Value.VerifyKyc();
            users.Add(buyer1.Value);
            _users["buyer1"] = buyer1.Value;
        }

        var buyer2 = User.Create(
            "mike.johnson@example.com",
            defaultPasswordHash,
            "Mike",
            "Johnson",
            UserRole.Buyer,
            "+1-555-0202");
        if (!buyer2.IsError)
        {
            buyer2.Value.SetCreditLimit(Money.Create(75000, "USD"));
            buyer2.Value.Activate();
            users.Add(buyer2.Value);
            _users["buyer2"] = buyer2.Value;
        }

        var buyer3 = User.Create(
            "emily.davis@example.com",
            defaultPasswordHash,
            "Emily",
            "Davis",
            UserRole.Buyer,
            "+1-555-0203");
        if (!buyer3.IsError)
        {
            buyer3.Value.SetCreditLimit(Money.Create(50000, "USD"));
            buyer3.Value.Activate();
            buyer3.Value.VerifyKyc();
            users.Add(buyer3.Value);
            _users["buyer3"] = buyer3.Value;
        }

        // ============ SELLERS ============
        var seller1 = User.Create(
            "seller@example.com",
            defaultPasswordHash,
            "Bob",
            "Williams",
            UserRole.Seller | UserRole.Buyer,
            "+1-555-0301");
        if (!seller1.IsError)
        {
            seller1.Value.Activate();
            seller1.Value.VerifyKyc();
            users.Add(seller1.Value);
            _users["seller1"] = seller1.Value;
        }

        var seller2 = User.Create(
            "carseller@example.com",
            defaultPasswordHash,
            "Alice",
            "Brown",
            UserRole.Seller | UserRole.Buyer,
            "+1-555-0302");
        if (!seller2.IsError)
        {
            seller2.Value.Activate();
            users.Add(seller2.Value);
            _users["seller2"] = seller2.Value;
        }

        // ============ INSPECTOR ============
        var inspector = User.Create(
            "inspector@carauctions.com",
            defaultPasswordHash,
            "David",
            "Inspector",
            UserRole.Inspector);
        if (!inspector.IsError)
        {
            inspector.Value.Activate();
            users.Add(inspector.Value);
            _users["inspector"] = inspector.Value;
        }

        // ============ PENDING USER (for testing) ============
        var pendingUser = User.Create(
            "pending@example.com",
            defaultPasswordHash,
            "Pending",
            "User",
            UserRole.Buyer);
        if (!pendingUser.IsError)
        {
            // Don't activate - leave as pending
            users.Add(pendingUser.Value);
            _users["pending"] = pendingUser.Value;
        }

        await _context.Users.AddRangeAsync(users, cancellationToken);
        _logger.LogInformation("Seeded {Count} users", users.Count);
    }

    private async Task SeedVehiclesAsync(CancellationToken cancellationToken)
    {
        if (await _context.Vehicles.AnyAsync(cancellationToken))
            return;

        var inspector = _users.GetValueOrDefault("inspector");
        if (inspector is null) return;

        // Vehicle data without VINs - we'll generate valid VINs
        var vehicleData = new (string Make, string Model, int Year, int Mileage, string OwnerKey, string ExtColor, string IntColor, string Engine, string Trans, string Fuel, TitleStatus Title, ConditionGrade Grade)[]
        {
            // Dealer 1 vehicles - Luxury/Sports (0-4)
            ("Toyota", "Camry", 2023, 12500, "dealer1", "Pearl White", "Beige", "2.5L I4", "Automatic", "Gasoline", TitleStatus.Clean, ConditionGrade.Grade5),
            ("Honda", "Accord", 2022, 28000, "dealer1", "Midnight Blue", "Black", "1.5L Turbo", "CVT", "Gasoline", TitleStatus.Clean, ConditionGrade.Grade4),
            ("BMW", "330i", 2023, 8500, "dealer1", "Alpine White", "Cognac", "2.0L Turbo", "Automatic", "Gasoline", TitleStatus.Clean, ConditionGrade.Grade5),
            ("Mercedes-Benz", "C300", 2022, 22000, "dealer1", "Obsidian Black", "Black", "2.0L Turbo", "Automatic", "Gasoline", TitleStatus.Clean, ConditionGrade.Grade4),
            ("Audi", "A4", 2023, 15000, "dealer1", "Glacier White", "Black", "2.0L TFSI", "S-Tronic", "Gasoline", TitleStatus.Clean, ConditionGrade.Grade4),

            // Dealer 2 vehicles - High-end Luxury (5-8)
            ("Porsche", "911 Carrera", 2022, 9800, "dealer2", "Guards Red", "Black", "3.0L Twin-Turbo", "PDK", "Gasoline", TitleStatus.Clean, ConditionGrade.Grade5),
            ("Tesla", "Model S", 2023, 18000, "dealer2", "Pearl White", "White", "Dual Motor", "Single-Speed", "Electric", TitleStatus.Clean, ConditionGrade.Grade5),
            ("Lexus", "ES 350", 2022, 31000, "dealer2", "Caviar Black", "Parchment", "3.5L V6", "Automatic", "Gasoline", TitleStatus.Clean, ConditionGrade.Grade4),
            ("Range Rover", "Sport", 2021, 42000, "dealer2", "Santorini Black", "Ebony", "3.0L I6", "Automatic", "Gasoline", TitleStatus.Clean, ConditionGrade.Grade3),

            // Seller 1 vehicles - Regular cars (9-11)
            ("Ford", "Mustang GT", 2023, 5200, "seller1", "Race Red", "Ebony", "5.0L V8", "Manual", "Gasoline", TitleStatus.Clean, ConditionGrade.Grade5),
            ("Chevrolet", "Camaro SS", 2022, 18500, "seller1", "Summit White", "Jet Black", "6.2L V8", "Automatic", "Gasoline", TitleStatus.Clean, ConditionGrade.Grade4),
            ("Dodge", "Challenger", 2021, 28000, "seller1", "Hellraisin", "Black", "5.7L V8", "Automatic", "Gasoline", TitleStatus.Rebuilt, ConditionGrade.Grade3),

            // Seller 2 vehicles - Economy/Family (12-14)
            ("Hyundai", "Sonata", 2023, 8900, "seller2", "Shimmering Silver", "Gray", "2.5L I4", "Automatic", "Gasoline", TitleStatus.Clean, ConditionGrade.Grade4),
            ("Kia", "Telluride", 2022, 35000, "seller2", "Everlasting Silver", "Black", "3.8L V6", "Automatic", "Gasoline", TitleStatus.Clean, ConditionGrade.Grade4),
            ("Mazda", "CX-5", 2023, 12000, "seller2", "Soul Red", "Parchment", "2.5L Turbo", "Automatic", "Gasoline", TitleStatus.Clean, ConditionGrade.Grade5),

            // Additional vehicles for variety (15-19)
            ("Nissan", "Altima", 2020, 52000, "dealer1", "Gun Metallic", "Charcoal", "2.5L I4", "CVT", "Gasoline", TitleStatus.Clean, ConditionGrade.Grade3),
            ("Subaru", "Outback", 2022, 24000, "dealer1", "Crystal White", "Slate Black", "2.5L Boxer", "CVT", "Gasoline", TitleStatus.Clean, ConditionGrade.Grade4),
            ("Volkswagen", "Golf GTI", 2023, 6500, "dealer2", "Tornado Red", "Titan Black", "2.0L TSI", "DSG", "Gasoline", TitleStatus.Clean, ConditionGrade.Grade5),
            ("Jeep", "Wrangler", 2021, 38000, "seller1", "Firecracker Red", "Black", "3.6L V6", "Manual", "Gasoline", TitleStatus.Clean, ConditionGrade.Grade3),
            ("GMC", "Sierra 1500", 2022, 29000, "seller2", "Summit White", "Jet Black", "5.3L V8", "Automatic", "Gasoline", TitleStatus.Clean, ConditionGrade.Grade4),
        };

        var vehicleIndex = 0;
        foreach (var (make, model, year, mileage, ownerKey, extColor, intColor, engine, trans, fuel, title, grade) in vehicleData)
        {
            var owner = _users.GetValueOrDefault(ownerKey);
            if (owner is null) continue;

            var generatedVin = GenerateVin(vehicleIndex++);
            var vinResult = VIN.Create(generatedVin);
            var mileageResult = Mileage.Create(mileage);

            if (vinResult.IsError || mileageResult.IsError)
                continue;

            var vehicleResult = Vehicle.Create(
                vinResult.Value,
                make,
                model,
                year,
                mileageResult.Value,
                owner.Id,
                title,
                extColor,
                intColor,
                engine,
                trans,
                fuel);

            if (!vehicleResult.IsError)
            {
                var vehicle = vehicleResult.Value;

                var report = ConditionReport.Create(
                    grade, grade, grade, grade,
                    inspector.Id,
                    DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 14)),
                    GetConditionDescription(grade));

                vehicle.AddConditionReport(report);

                // Add vehicle image (cycle through 6 available images)
                var imageNumber = (_vehicles.Count % 6) + 1;
                var imageUrl = $"assets/images/vehicles/{imageNumber}.png";
                vehicle.AddImage(imageUrl, ImageType.Exterior, isPrimary: true);

                vehicle.MarkAsListed();
                _vehicles.Add(vehicle);
            }
        }

        await _context.Vehicles.AddRangeAsync(_vehicles, cancellationToken);
        _logger.LogInformation("Seeded {Count} vehicles", _vehicles.Count);
    }

    private async Task SeedAuctionsAsync(CancellationToken cancellationToken)
    {
        if (await _context.Auctions.AnyAsync(cancellationToken))
            return;

        var now = DateTime.UtcNow;

        // ============ ACTIVE AUCTIONS (currently running) ============

        // Active auction ending soon (1 hour left)
        await CreateAuction(
            _vehicles[0], // Toyota Camry
            AuctionType.Timed,
            startingPrice: 22000,
            reservePrice: 26000,
            buyNowPrice: 32000,
            startTime: now.AddHours(-23),
            endTime: now.AddHours(1),
            status: "Active",
            isDealerOnly: false,
            description: "Low mileage 2023 Toyota Camry in pristine condition. Full service history available.");

        // Active auction ending in 2 days
        await CreateAuction(
            _vehicles[1], // Honda Accord
            AuctionType.Timed,
            startingPrice: 18000,
            reservePrice: 22000,
            buyNowPrice: 28000,
            startTime: now.AddDays(-1),
            endTime: now.AddDays(2),
            status: "Active",
            isDealerOnly: false,
            description: "Well-maintained Honda Accord with turbocharged engine. Great fuel economy!");

        // Active DEALER-ONLY auction
        await CreateAuction(
            _vehicles[5], // Porsche 911
            AuctionType.Timed,
            startingPrice: 85000,
            reservePrice: 95000,
            buyNowPrice: 115000,
            startTime: now.AddHours(-12),
            endTime: now.AddDays(5),
            status: "Active",
            isDealerOnly: true,
            description: "DEALER ONLY - 2022 Porsche 911 Carrera. Exceptional condition with ceramic brakes.");

        // Active auction with high activity
        await CreateAuction(
            _vehicles[9], // Ford Mustang GT
            AuctionType.Timed,
            startingPrice: 42000,
            reservePrice: 48000,
            buyNowPrice: 55000,
            startTime: now.AddDays(-2),
            endTime: now.AddDays(1),
            status: "Active",
            isDealerOnly: false,
            description: "2023 Ford Mustang GT with 5.0L V8! Low miles, garage kept. Manual transmission!");

        // Active Live auction
        await CreateAuction(
            _vehicles[2], // BMW 330i
            AuctionType.Live,
            startingPrice: 38000,
            reservePrice: 42000,
            buyNowPrice: 48000,
            startTime: now.AddHours(-2),
            endTime: now.AddHours(4),
            status: "Active",
            isDealerOnly: false,
            description: "Live Auction! 2023 BMW 330i with M Sport Package. Don't miss this opportunity!");

        // Active auction ending in 3 days
        await CreateAuction(
            _vehicles[6], // Tesla Model S
            AuctionType.Timed,
            startingPrice: 65000,
            reservePrice: 72000,
            buyNowPrice: 82000,
            startTime: now.AddHours(-6),
            endTime: now.AddDays(3),
            status: "Active",
            isDealerOnly: false,
            description: "2023 Tesla Model S Long Range. Full self-driving capability included!");

        // ============ SCHEDULED AUCTIONS (upcoming) ============

        // Starting tomorrow
        await CreateAuction(
            _vehicles[3], // Mercedes C300
            AuctionType.Timed,
            startingPrice: 35000,
            reservePrice: 40000,
            buyNowPrice: 48000,
            startTime: now.AddDays(1),
            endTime: now.AddDays(4),
            status: "Scheduled",
            isDealerOnly: false,
            description: "Coming Soon! 2022 Mercedes-Benz C300 with AMG Line package.");

        // Starting in 3 days
        await CreateAuction(
            _vehicles[4], // Audi A4
            AuctionType.Timed,
            startingPrice: 32000,
            reservePrice: 36000,
            buyNowPrice: 42000,
            startTime: now.AddDays(3),
            endTime: now.AddDays(6),
            status: "Scheduled",
            isDealerOnly: false,
            description: "Upcoming: 2023 Audi A4 with Premium Plus package. Virtual cockpit included!");

        // Scheduled dealer-only
        await CreateAuction(
            _vehicles[7], // Lexus ES 350
            AuctionType.Timed,
            startingPrice: 28000,
            reservePrice: 32000,
            buyNowPrice: 38000,
            startTime: now.AddDays(2),
            endTime: now.AddDays(5),
            status: "Scheduled",
            isDealerOnly: true,
            description: "DEALER ONLY - 2022 Lexus ES 350. Mark Levinson audio system!");

        // ============ COMPLETED AUCTIONS (with winners) ============

        await CreateAuction(
            _vehicles[10], // Chevrolet Camaro
            AuctionType.Timed,
            startingPrice: 35000,
            reservePrice: 40000,
            buyNowPrice: null,
            startTime: now.AddDays(-7),
            endTime: now.AddDays(-4),
            status: "Completed",
            isDealerOnly: false,
            description: "SOLD - 2022 Chevrolet Camaro SS. Congratulations to the winner!");

        await CreateAuction(
            _vehicles[14], // Mazda CX-5
            AuctionType.Timed,
            startingPrice: 28000,
            reservePrice: 31000,
            buyNowPrice: null,
            startTime: now.AddDays(-10),
            endTime: now.AddDays(-7),
            status: "Completed",
            isDealerOnly: false,
            description: "SOLD - 2023 Mazda CX-5 Turbo. Great SUV!");

        // ============ ENDED NO SALE (reserve not met) ============

        await CreateAuction(
            _vehicles[11], // Dodge Challenger (Rebuilt title)
            AuctionType.Timed,
            startingPrice: 25000,
            reservePrice: 35000, // High reserve
            buyNowPrice: null,
            startTime: now.AddDays(-5),
            endTime: now.AddDays(-2),
            status: "EndedNoSale",
            isDealerOnly: false,
            description: "Reserve Not Met - 2021 Dodge Challenger. Rebuilt title.");

        // ============ CANCELLED AUCTION ============

        await CreateAuction(
            _vehicles[8], // Range Rover Sport
            AuctionType.Timed,
            startingPrice: 55000,
            reservePrice: 62000,
            buyNowPrice: 72000,
            startTime: now.AddDays(-3),
            endTime: now.AddDays(0),
            status: "Cancelled",
            isDealerOnly: false,
            description: "CANCELLED - Vehicle no longer available.");

        await _context.Auctions.AddRangeAsync(_auctions, cancellationToken);
        _logger.LogInformation("Seeded {Count} auctions", _auctions.Count);
    }

    private async Task CreateAuction(
        Vehicle vehicle,
        AuctionType type,
        decimal startingPrice,
        decimal reservePrice,
        decimal? buyNowPrice,
        DateTime startTime,
        DateTime endTime,
        string status,
        bool isDealerOnly,
        string description)
    {
        var settings = AuctionSettings.Default("USD");

        // Use CreateForSeeding to bypass date validation for test data
        var auction = Auction.CreateForSeeding(
            $"{vehicle.Year} {vehicle.Make} {vehicle.Model}",
            type,
            vehicle.Id,
            vehicle.OwnerId,
            Money.Create(startingPrice, "USD"),
            startTime,
            endTime,
            settings,
            Money.Create(reservePrice, "USD"),
            buyNowPrice.HasValue ? Money.Create(buyNowPrice.Value, "USD") : null,
            description,
            isDealerOnly);

        switch (status)
        {
            case "Scheduled":
                auction.Schedule();
                break;
            case "Active":
                auction.Schedule();
                auction.Start(startTime);
                vehicle.MarkAsInAuction();
                break;
            case "Completed":
                auction.Schedule();
                auction.Start(startTime);
                vehicle.MarkAsInAuction();
                // Will add bids and close in SeedBidsAsync
                break;
            case "EndedNoSale":
                auction.Schedule();
                auction.Start(startTime);
                vehicle.MarkAsInAuction();
                // Will close without meeting reserve
                break;
            case "Cancelled":
                auction.Schedule();
                auction.Cancel("Seller withdrew the vehicle");
                break;
        }

        _auctions.Add(auction);
        await Task.CompletedTask;
    }

    private async Task SeedBidsAsync(CancellationToken cancellationToken)
    {
        if (await _context.Bids.AnyAsync(cancellationToken))
            return;

        var bids = new List<Bid>();
        var now = DateTime.UtcNow;

        var buyer1 = _users.GetValueOrDefault("buyer1");
        var buyer2 = _users.GetValueOrDefault("buyer2");
        var buyer3 = _users.GetValueOrDefault("buyer3");
        var dealer1 = _users.GetValueOrDefault("dealer1");
        var dealer2 = _users.GetValueOrDefault("dealer2");

        if (buyer1 is null || buyer2 is null || buyer3 is null) return;

        // Add bids to active auctions
        foreach (var auction in _auctions.Where(a => a.Status == AuctionStatus.Active))
        {
            // Skip dealer-only auctions for regular buyers
            var eligibleBidders = auction.IsDealerOnly
                ? new[] { dealer1, dealer2 }.Where(d => d != null && d.Id != auction.SellerId).ToArray()
                : new[] { buyer1, buyer2, buyer3, dealer1, dealer2 }.Where(b => b != null && b.Id != auction.SellerId).ToArray();

            if (eligibleBidders.Length == 0) continue;

            var currentBid = auction.StartingPrice.Amount;
            var bidTime = auction.StartTime.AddMinutes(30);
            var bidCount = Random.Shared.Next(3, 8);

            for (int i = 0; i < bidCount && bidTime < now; i++)
            {
                var bidder = eligibleBidders[i % eligibleBidders.Length];
                if (bidder is null) continue;

                var increment = Random.Shared.Next(100, 500) * 10; // $1000 - $5000
                currentBid += increment;

                var bid = Bid.Place(
                    auction.Id,
                    bidder.Id,
                    Money.Create(currentBid, "USD"),
                    bidTime);

                var placeBidResult = auction.PlaceBid(bid.Id, bidder.Id, Money.Create(currentBid, "USD"), bidTime);
                if (!placeBidResult.IsError)
                {
                    bids.Add(bid);
                }

                bidTime = bidTime.AddMinutes(Random.Shared.Next(30, 180));
            }
        }

        // Add bids to completed auctions and close them
        foreach (var auction in _auctions.Where(a => a.Status == AuctionStatus.Active &&
            a.EndTime < now && _vehicles.Any(v => v.Id == a.VehicleId)))
        {
            var eligibleBidders = new[] { buyer1, buyer2, buyer3 }.Where(b => b != null && b.Id != auction.SellerId).ToArray();
            if (eligibleBidders.Length == 0) continue;

            var currentBid = auction.StartingPrice.Amount;
            var bidTime = auction.StartTime.AddHours(1);

            // Add enough bids to meet reserve
            while (currentBid < auction.ReservePrice?.Amount)
            {
                var bidder = eligibleBidders[Random.Shared.Next(eligibleBidders.Length)];
                if (bidder is null) continue;

                currentBid += Random.Shared.Next(5, 20) * 100;

                var bid = Bid.Place(
                    auction.Id,
                    bidder.Id,
                    Money.Create(currentBid, "USD"),
                    bidTime);

                var placeBidResult = auction.PlaceBid(bid.Id, bidder.Id, Money.Create(currentBid, "USD"), bidTime);
                if (!placeBidResult.IsError)
                {
                    bids.Add(bid);
                }

                bidTime = bidTime.AddMinutes(Random.Shared.Next(60, 240));
            }

            // Close the auction
            auction.Close(auction.EndTime);
        }

        // Close ended-no-sale auctions (bids below reserve)
        foreach (var auction in _auctions.Where(a => a.Status == AuctionStatus.Active))
        {
            // Find auctions that were supposed to end without meeting reserve
            var vehicle = _vehicles.FirstOrDefault(v => v.Id == auction.VehicleId);
            if (vehicle?.Make == "Dodge" && vehicle?.Model == "Challenger")
            {
                // Add some bids below reserve
                var currentBid = auction.StartingPrice.Amount;
                var bidTime = auction.StartTime.AddHours(2);

                for (int i = 0; i < 3; i++)
                {
                    var bidder = new[] { buyer1, buyer2 }[i % 2];
                    if (bidder is null) continue;

                    currentBid += 1000;

                    var bid = Bid.Place(
                        auction.Id,
                        bidder.Id,
                        Money.Create(currentBid, "USD"),
                        bidTime);

                    var placeBidResult = auction.PlaceBid(bid.Id, bidder.Id, Money.Create(currentBid, "USD"), bidTime);
                    if (!placeBidResult.IsError)
                    {
                        bids.Add(bid);
                    }

                    bidTime = bidTime.AddHours(6);
                }

                // Close without meeting reserve
                auction.Close(auction.EndTime);
            }
        }

        await _context.Bids.AddRangeAsync(bids, cancellationToken);
        _logger.LogInformation("Seeded {Count} bids", bids.Count);
    }

    private static string GetConditionDescription(ConditionGrade grade)
    {
        return grade switch
        {
            ConditionGrade.Grade5 => "Exceptional condition. Like new with minimal wear.",
            ConditionGrade.Grade4 => "Excellent condition. Minor wear consistent with age.",
            ConditionGrade.Grade3 => "Good condition. Normal wear and minor imperfections.",
            ConditionGrade.Grade2 => "Fair condition. Visible wear and some repairs needed.",
            ConditionGrade.Grade1 => "Poor condition. Significant wear and repairs required.",
            _ => "Condition not specified."
        };
    }

    /// <summary>
    /// Generates a valid 17-character VIN with correct check digit for testing purposes.
    /// </summary>
    private static string GenerateVin(int index)
    {
        // Use a base pattern and compute correct check digit
        // WMI (3 chars) + VDS (5 chars) + Check (1) + VIS (8 chars)
        var wmiCodes = new[] { "1G1", "1HG", "2HG", "3FA", "5YJ", "WBA", "WDB", "WAU", "JHM", "JTD", "KND", "5NP", "3VW", "1C4", "1GT", "5N1", "JN1", "4T1" };
        var wmi = wmiCodes[index % wmiCodes.Length];

        // Generate VDS (5 chars) and partial VIS (8 chars, but check digit at position 9 will be calculated)
        var alphanumeric = "ABCDEFGHJKLMNPRSTUVWXYZ0123456789"; // Excluding I, O, Q
        var seededRandom = new Random(index * 12345);

        var vds = string.Concat(Enumerable.Range(0, 5).Select(_ => alphanumeric[seededRandom.Next(alphanumeric.Length)]));
        var vis = string.Concat(Enumerable.Range(0, 8).Select(_ => alphanumeric[seededRandom.Next(alphanumeric.Length)]));

        // Build VIN without check digit (placeholder at position 9)
        var vinWithoutCheck = wmi + vds + "0" + vis;

        // Calculate check digit
        var weights = new[] { 8, 7, 6, 5, 4, 3, 2, 10, 0, 9, 8, 7, 6, 5, 4, 3, 2 };
        var transliteration = "0123456789.ABCDEFGH..JKLMN.P.R..STUVWXYZ";

        var sum = 0;
        for (var i = 0; i < 17; i++)
        {
            var c = vinWithoutCheck[i];
            int value;

            if (char.IsDigit(c))
            {
                value = c - '0';
            }
            else
            {
                var idx = transliteration.IndexOf(c);
                value = idx % 10;
            }

            sum += value * weights[i];
        }

        var checkDigit = sum % 11;
        var checkChar = checkDigit == 10 ? 'X' : (char)('0' + checkDigit);

        return wmi + vds + checkChar + vis;
    }
}
