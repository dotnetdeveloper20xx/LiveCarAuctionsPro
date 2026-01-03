# LiveCarAuctionsPro

An enterprise-grade real-time car auction platform built with .NET 8 and Angular 18, featuring live bidding, payment processing, and comprehensive auction management.

![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)
![Angular](https://img.shields.io/badge/Angular-18-DD0031?logo=angular)
![License](https://img.shields.io/badge/License-MIT-green)

## Overview

LiveCarAuctionsPro is a full-stack auction platform designed for B2B and B2C car sales. The system supports multiple auction types, real-time bidding via WebSockets, proxy bidding, anti-sniping protection, and complete payment/invoicing workflows.

## Demo

| Auction List | Live Bidding |
|--------------|--------------|
| Browse active auctions with filtering | Real-time bid updates via SignalR |

**API Swagger UI**: `http://localhost:5298/swagger`
**Angular Frontend**: `http://localhost:4200`

---

## Features

### Auction Management
- **Multiple Auction Types**: Timed auctions, live auctions, and Buy Now options
- **Auction Lifecycle**: Draft → Scheduled → Active → Completed/Cancelled
- **Anti-Sniping Protection**: Automatic time extension when bids are placed near closing
- **Reserve Prices**: Optional minimum sale prices with "Reserve Not Met" indicators
- **Dealer-Only Auctions**: Restrict certain auctions to verified dealers

### Real-Time Bidding
- **Live Updates**: WebSocket-based real-time bid notifications via SignalR
- **Proxy Bidding**: Set maximum bids and let the system bid incrementally on your behalf
- **Credit Limit Enforcement**: Buyers can only bid within their approved credit limits
- **Bid History**: Complete audit trail of all bids placed

### Vehicle Management
- **VIN Validation**: 17-character VIN validation with check digit verification
- **Condition Reports**: Detailed inspection reports with grading (Grade 1-5)
- **Multiple Images**: Support for exterior, interior, and damage photos
- **Title Status Tracking**: Clean, salvage, rebuilt, lemon, and flood titles

### User Management
- **Role-Based Access**: Admin, Buyer, Seller, Dealer, and Inspector roles
- **KYC Verification**: Know Your Customer verification workflow
- **Credit Limits**: Configurable bidding limits per user
- **Dealer Profiles**: License verification and dealership information

### Payments & Invoicing
- **Payment Processing**: Integrated payment gateway (mock for development)
- **Automatic Invoicing**: Invoice generation with buyer premiums and fees
- **Refund Support**: Process refunds for cancelled transactions
- **Audit Logging**: Complete financial audit trail

---

## Technology Stack

### Backend (.NET 8)

| Technology | Purpose |
|------------|---------|
| **ASP.NET Core 8** | Web API framework |
| **Entity Framework Core 8** | ORM with In-Memory/SQL Server providers |
| **MediatR** | CQRS pattern implementation |
| **FluentValidation** | Request validation |
| **AutoMapper** | Object mapping |
| **SignalR** | Real-time WebSocket communication |
| **Serilog** | Structured logging |
| **ErrorOr** | Result pattern for error handling |
| **Ardalis.GuardClauses** | Defensive programming |

### Frontend (Angular 18)

| Technology | Purpose |
|------------|---------|
| **Angular 18** | SPA framework with standalone components |
| **Angular Material** | UI component library |
| **Angular Signals** | Reactive state management |
| **SignalR Client** | Real-time updates |
| **RxJS** | Reactive programming |

### Architecture Patterns

- **Clean Architecture**: Separation of concerns with Domain, Application, Infrastructure, and Presentation layers
- **Domain-Driven Design (DDD)**: Aggregates, Value Objects, Domain Events, and Domain Services
- **CQRS**: Command Query Responsibility Segregation via MediatR
- **Repository Pattern**: Abstracted data access with Unit of Work
- **Strongly-Typed IDs**: Type-safe entity identifiers (AuctionId, VehicleId, etc.)

---

## Project Structure

```
CarAuctions/
├── src/
│   ├── CarAuctions.Domain/           # Core domain logic (zero dependencies)
│   │   ├── Aggregates/               # Auction, Vehicle, Bid, User, Payment
│   │   │   ├── Auctions/             # Auction aggregate root + events
│   │   │   ├── Vehicles/             # Vehicle aggregate + VIN validation
│   │   │   ├── Bids/                 # Bid aggregate + proxy bidding
│   │   │   ├── Users/                # User aggregate + credit limits
│   │   │   └── Payments/             # Payment aggregate + invoicing
│   │   ├── Common/                   # Base classes, Value Objects (Money, etc.)
│   │   └── Services/                 # Domain services (BiddingService)
│   │
│   ├── CarAuctions.Application/      # Use cases and business logic
│   │   ├── Common/
│   │   │   ├── Interfaces/           # Repository & service contracts
│   │   │   ├── Behaviors/            # MediatR pipeline (validation, logging, audit)
│   │   │   └── Models/               # DTOs and result types
│   │   └── Features/                 # CQRS Commands & Queries by feature
│   │       ├── Auctions/
│   │       ├── Vehicles/
│   │       ├── Bids/
│   │       ├── Users/
│   │       └── Payments/
│   │
│   ├── CarAuctions.Infrastructure/   # External service implementations
│   │   ├── Services/                 # Email, DateTime, CurrentUser, Audit
│   │   └── Payment/                  # Payment gateway (Mock implementation)
│   │
│   ├── CarAuctions.Persistence/      # Database and repositories
│   │   ├── Configurations/           # EF Core Fluent API entity configs
│   │   ├── Repositories/             # Repository implementations
│   │   ├── Interceptors/             # Auditable entity interceptor
│   │   └── Seeding/                  # Development seed data
│   │
│   ├── CarAuctions.Api/              # HTTP API layer
│   │   ├── Controllers/              # REST endpoints
│   │   ├── Hubs/                     # SignalR hubs (AuctionHub)
│   │   ├── Middleware/               # Exception handling, correlation IDs
│   │   └── Authorization/            # RBAC policies
│   │
│   └── CarAuctions.Worker/           # Background job processing
│       └── Services/                 # Auction timer, starter services
│
├── frontend/
│   └── carauctions-web/              # Angular 18 application
│       └── src/app/
│           ├── core/                 # Services, State (Signals), Interceptors
│           ├── features/             # Auction list, detail, dashboard, auth
│           └── shared/               # Reusable components, pipes, directives
│
├── tests/
│   ├── CarAuctions.Domain.Tests/
│   ├── CarAuctions.Application.Tests/
│   ├── CarAuctions.Integration.Tests/
│   └── CarAuctions.Architecture.Tests/
│
└── docker/
    └── docker-compose.yml            # SQL Server & Azurite containers
```

---

## Key Domain Models

### Auction Aggregate
```csharp
public sealed class Auction : AggregateRoot<AuctionId>
{
    public string Title { get; }
    public AuctionType Type { get; }              // Timed, Live, BuyNow
    public AuctionStatus Status { get; }          // Draft, Scheduled, Active, Completed
    public VehicleId VehicleId { get; }
    public UserId SellerId { get; }
    public Money StartingPrice { get; }
    public Money? ReservePrice { get; }
    public Money CurrentHighBid { get; }
    public AuctionSettings Settings { get; }      // Anti-sniping, proxy bidding config
    public DateTime StartTime { get; }
    public DateTime EndTime { get; }

    // Domain methods
    public ErrorOr<Success> Schedule();
    public ErrorOr<Success> Start(DateTime currentTime);
    public ErrorOr<Bid> PlaceBid(UserId bidderId, Money amount);
    public ErrorOr<Success> Close();
}
```

### Value Objects
```csharp
// Immutable monetary amount with currency
public sealed class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    public static Money Create(decimal amount, string currency = "USD");
    public Money Add(Money other);
    public Money Subtract(Money other);
}

// Validated 17-character Vehicle Identification Number
public sealed class VIN : ValueObject
{
    public string Value { get; }

    public static ErrorOr<VIN> Create(string value);  // Validates format & check digit
}

// Credit limit with reservation tracking
public sealed class CreditLimit : ValueObject
{
    public Money TotalLimit { get; }
    public Money UsedAmount { get; }
    public Money AvailableAmount => TotalLimit - UsedAmount;

    public ErrorOr<CreditLimit> Reserve(Money amount);
    public CreditLimit Release(Money amount);
}
```

---

## Challenges & Solutions

### 1. EF Core Value Object Materialization

**Challenge**: EF Core couldn't materialize complex value objects with owned type parameters in constructors.

**Solution**: Added private parameterless constructors and changed properties to private setters, allowing EF Core to use reflection-based materialization while maintaining domain immutability from external code.

```csharp
public sealed class Money : ValueObject
{
    public decimal Amount { get; private set; }
    public string Currency { get; private set; }

    private Money() { } // EF Core constructor

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }
}
```

### 2. Repository Save Pattern

**Challenge**: Commands were creating entities but changes weren't being persisted to the database.

**Solution**: Added `SaveChangesAsync` to the repository interface and ensured all command handlers explicitly save after modifications, following the Unit of Work pattern.

```csharp
public interface IRepository<TEntity, TId>
{
    Task<TEntity> AddAsync(TEntity entity, CancellationToken ct);
    Task UpdateAsync(TEntity entity, CancellationToken ct);
    Task<int> SaveChangesAsync(CancellationToken ct);  // Added
}
```

### 3. Data Seeding Order Dependencies

**Challenge**: Related entities (Users → Vehicles → Auctions) failed to seed because foreign key relationships weren't established.

**Solution**: Modified the seeder to save changes after each entity type, ensuring IDs are generated and available for subsequent relationships.

```csharp
public async Task SeedAsync(CancellationToken ct)
{
    await SeedUsersAsync(ct);
    await _context.SaveChangesAsync(ct);  // Commit users first

    await SeedVehiclesAsync(ct);          // Now can reference user IDs
    await _context.SaveChangesAsync(ct);

    await SeedAuctionsAsync(ct);          // Now can reference vehicle IDs
    await _context.SaveChangesAsync(ct);
}
```

### 4. Real-Time Bid Synchronization

**Challenge**: Multiple users bidding simultaneously could cause race conditions and stale data.

**Solution**: Implemented SignalR hub for instant bid broadcasts, combined with optimistic concurrency in the domain model and automatic UI refresh on bid events.

```csharp
// SignalR Hub
public class AuctionHub : Hub
{
    public async Task JoinAuction(string auctionId)
        => await Groups.AddToGroupAsync(Context.ConnectionId, auctionId);

    public async Task BroadcastBid(string auctionId, BidDto bid)
        => await Clients.Group(auctionId).SendAsync("BidPlaced", bid);
}
```

### 5. Anti-Sniping Implementation

**Challenge**: Preventing last-second bids from unfairly winning auctions.

**Solution**: Domain logic automatically extends auction end time when bids are placed within the anti-sniping window (configurable, default 2 minutes), with a maximum extension limit.

```csharp
public ErrorOr<Success> ApplyAntiSniping(DateTime currentTime)
{
    var timeRemaining = EndTime - currentTime;

    if (timeRemaining <= Settings.AntiSnipingWindow && _extensionCount < Settings.MaxExtensions)
    {
        EndTime = EndTime.Add(Settings.AntiSnipingExtension);
        _extensionCount++;
    }

    return Result.Success;
}
```

### 6. Credit Limit Enforcement

**Challenge**: Ensuring buyers can't exceed their approved credit across multiple active bids.

**Solution**: CreditLimit value object tracks both total limit and reserved amount, with atomic reservation during bid placement and release on outbid.

---

## Getting Started

### Prerequisites

- .NET 8 SDK
- Node.js 18+ and npm
- SQL Server (optional - uses In-Memory by default)

### Running the API

```bash
# Navigate to project root
cd carauctions

# Restore and build
dotnet restore
dotnet build

# Run the API (uses In-Memory database by default)
dotnet run --project src/CarAuctions.Api/CarAuctions.Api.csproj
```

The API will start at `http://localhost:5298` with Swagger UI at `http://localhost:5298/swagger`.

### Running the Frontend

```bash
# Navigate to frontend
cd frontend/carauctions-web

# Install dependencies
npm install

# Start development server
npm start
```

The Angular app will start at `http://localhost:4200`.

### Seed Data

The application automatically seeds development data on startup:

| Entity | Count | Examples |
|--------|-------|----------|
| Users | 5 | Admin, Dealer, Buyer, Seller, Inspector |
| Vehicles | 5 | Toyota Camry, Honda Accord, BMW 330i, Mercedes C300, Ford Mustang |

### Creating a Test Auction

```bash
# 1. Get available vehicles
curl http://localhost:5298/api/vehicles

# 2. Create an auction (use vehicleId and sellerId from step 1)
curl -X POST http://localhost:5298/api/auctions \
  -H "Content-Type: application/json" \
  -d '{
    "title": "2023 BMW 330i - Low Miles",
    "type": "Timed",
    "vehicleId": "<vehicle-id>",
    "sellerId": "<seller-id>",
    "startingPrice": 35000,
    "currency": "USD",
    "startTime": "2026-01-04T12:00:00Z",
    "endTime": "2026-01-10T12:00:00Z",
    "reservePrice": 40000,
    "buyNowPrice": 45000
  }'

# 3. Schedule the auction
curl -X POST http://localhost:5298/api/auctions/<auction-id>/schedule

# 4. Start the auction (after start time has passed)
curl -X POST http://localhost:5298/api/auctions/<auction-id>/start
```

---

## API Endpoints

### Auctions

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/auctions` | List auctions with filtering |
| GET | `/api/auctions/{id}` | Get auction details |
| POST | `/api/auctions` | Create new auction |
| POST | `/api/auctions/{id}/schedule` | Schedule a draft auction |
| POST | `/api/auctions/{id}/start` | Start a scheduled auction |
| POST | `/api/auctions/{id}/close` | Close an active auction |

### Vehicles

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/vehicles` | List vehicles |
| GET | `/api/vehicles/{id}` | Get vehicle details |
| POST | `/api/vehicles` | Register a vehicle |

### Bids

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/bids` | Place a bid |
| GET | `/api/bids/auction/{id}` | Get bid history for auction |

### Payments

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/payments/initiate` | Initiate payment for won auction |
| POST | `/api/payments/{id}/confirm` | Confirm payment completion |
| GET | `/api/payments/{id}/invoice` | Download invoice PDF |

### Admin

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/admin/audit-logs` | View audit logs (Admin only) |

---

## Configuration

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=CarAuctionsDb;Trusted_Connection=True"
  },
  "UseInMemoryDatabase": true,
  "Cors": {
    "AllowedOrigins": ["http://localhost:4200"]
  },
  "Serilog": {
    "MinimumLevel": "Information",
    "WriteTo": [
      { "Name": "Console" }
    ]
  }
}
```

---

## Implementation Phases

- [x] **Phase 1**: Foundation & Solution Setup
- [x] **Phase 2**: Core Domain Models (Aggregates, Value Objects, Events)
- [x] **Phase 3**: Persistence & EF Core (Configurations, Repositories, Seeding)
- [x] **Phase 4**: CQRS Infrastructure (MediatR, Behaviors, Validation)
- [x] **Phase 5**: API Layer (Controllers, SignalR Hub, Middleware)
- [x] **Phase 6**: Bidding Engine (Event Handlers, Background Services)
- [x] **Phase 7**: Angular Frontend (Material UI, Signals State, Real-time)
- [x] **Phase 8**: Payments & Compliance (Invoicing, Audit Logging, RBAC)

---

## Future Enhancements

- [ ] Azure Service Bus integration for distributed events
- [ ] Azure Blob Storage for vehicle images
- [ ] Stripe/PayPal payment gateway integration
- [ ] SMS/Push notification support
- [ ] Advanced search with Elasticsearch
- [ ] Mobile application (React Native/Flutter)
- [ ] Admin dashboard with analytics
- [ ] Multi-currency support
- [ ] Internationalization (i18n)

---

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

---

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

Built with Clean Architecture, Domain-Driven Design, and modern .NET practices.
