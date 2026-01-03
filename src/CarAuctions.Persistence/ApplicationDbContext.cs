using CarAuctions.Application.Common.Interfaces;
using CarAuctions.Domain.Aggregates.Auctions;
using CarAuctions.Domain.Aggregates.Bids;
using CarAuctions.Domain.Aggregates.Users;
using CarAuctions.Domain.Aggregates.Vehicles;
using Microsoft.EntityFrameworkCore;

namespace CarAuctions.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Auction> Auctions => Set<Auction>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<Bid> Bids => Set<Bid>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
