# CarAuctions Platform

Enterprise-grade Car Auction Platform supporting B2B/B2C auction models.

## Technology Stack

- **Backend**: ASP.NET 8, Clean Architecture, CQRS, MediatR
- **Database**: SQL Server (LocalDB for development, In-Memory for testing)
- **Frontend**: Angular 18 with Angular Material (Phase 7)
- **Real-time**: SignalR for live bidding
- **Authentication**: JWT with ASP.NET Identity

## Solution Structure

```
CarAuctions/
├── src/
│   ├── CarAuctions.Domain/          # Core domain logic
│   ├── CarAuctions.Application/     # Use cases, CQRS handlers
│   ├── CarAuctions.Infrastructure/  # External services
│   ├── CarAuctions.Persistence/     # EF Core, repositories
│   ├── CarAuctions.Api/             # ASP.NET 8 Web API
│   └── CarAuctions.Worker/          # Background jobs
├── tests/
│   ├── CarAuctions.Domain.Tests/
│   ├── CarAuctions.Application.Tests/
│   ├── CarAuctions.Integration.Tests/
│   └── CarAuctions.Architecture.Tests/
├── frontend/
│   └── carauctions-web/             # Angular application
└── docker/
    └── docker-compose.yml           # SQL Server & Azurite
```

## Getting Started

### Prerequisites

- .NET 8 SDK
- SQL Server LocalDB (or Docker)
- Node.js 18+ (for Angular frontend)

### Run the API (Development)

```bash
dotnet run --project src/CarAuctions.Api
```

The API will start with In-Memory database by default in Development mode.

### Run with Docker (SQL Server)

```bash
cd docker
docker-compose up -d
```

### API Endpoints

- Health: `GET /health`
- Swagger: `GET /swagger`
- Auctions: `GET /api/auctions`

## Implementation Phases

- [x] Phase 1: Foundation & Solution Setup
- [x] Phase 2: Core Domain Models
- [ ] Phase 3: Persistence & EF Core
- [ ] Phase 4: CQRS Infrastructure
- [ ] Phase 5: API Layer
- [ ] Phase 6: Bidding Engine
- [ ] Phase 7: Angular Frontend
- [ ] Phase 8: Payments & Compliance

## License

Proprietary - All rights reserved.
