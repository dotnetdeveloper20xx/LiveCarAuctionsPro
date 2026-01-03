using CarAuctions.Domain.Aggregates.Auctions;
using CarAuctions.Domain.Aggregates.Bids;
using CarAuctions.Domain.Aggregates.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarAuctions.Persistence.Configurations;

public class BidConfiguration : IEntityTypeConfiguration<Bid>
{
    public void Configure(EntityTypeBuilder<Bid> builder)
    {
        builder.ToTable("Bids");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Id)
            .HasConversion(
                id => id.Value,
                value => BidId.Create(value))
            .HasColumnName("Id");

        builder.Property(b => b.AuctionId)
            .HasConversion(
                id => id.Value,
                value => AuctionId.Create(value))
            .IsRequired();

        builder.Property(b => b.BidderId)
            .HasConversion(
                id => id.Value,
                value => UserId.Create(value))
            .IsRequired();

        builder.OwnsOne(b => b.Amount, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("Amount")
                .HasPrecision(18, 2)
                .IsRequired();
            money.Property(m => m.Currency)
                .HasColumnName("Currency")
                .HasMaxLength(3)
                .IsRequired();
        });

        builder.Property(b => b.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(b => b.PlacedAt).IsRequired();
        builder.Property(b => b.IsProxyBid);

        builder.OwnsOne(b => b.MaxProxyAmount, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("MaxProxyAmount")
                .HasPrecision(18, 2);
            money.Property(m => m.Currency)
                .HasColumnName("MaxProxyCurrency")
                .HasMaxLength(3);
        });

        builder.Property(b => b.IpAddress).HasMaxLength(50);
        builder.Property(b => b.UserAgent).HasMaxLength(500);

        builder.Ignore(b => b.DomainEvents);

        builder.HasIndex(b => b.AuctionId);
        builder.HasIndex(b => b.BidderId);
        builder.HasIndex(b => b.Status);
        builder.HasIndex(b => b.PlacedAt);
    }
}
