using CarAuctions.Domain.Aggregates.Auctions;
using CarAuctions.Domain.Aggregates.Bids;
using CarAuctions.Domain.Aggregates.Users;
using CarAuctions.Domain.Aggregates.Vehicles;
using CarAuctions.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarAuctions.Persistence.Configurations;

public class AuctionConfiguration : IEntityTypeConfiguration<Auction>
{
    public void Configure(EntityTypeBuilder<Auction> builder)
    {
        builder.ToTable("Auctions");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasConversion(
                id => id.Value,
                value => AuctionId.Create(value))
            .HasColumnName("Id");

        builder.Property(a => a.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.Description)
            .HasMaxLength(4000);

        builder.Property(a => a.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(a => a.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(a => a.VehicleId)
            .HasConversion(
                id => id.Value,
                value => VehicleId.Create(value))
            .IsRequired();

        builder.Property(a => a.SellerId)
            .HasConversion(
                id => id.Value,
                value => UserId.Create(value))
            .IsRequired();

        builder.OwnsOne(a => a.StartingPrice, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("StartingPriceAmount")
                .HasPrecision(18, 2)
                .IsRequired();
            money.Property(m => m.Currency)
                .HasColumnName("StartingPriceCurrency")
                .HasMaxLength(3)
                .IsRequired();
        });

        builder.OwnsOne(a => a.ReservePrice, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("ReservePriceAmount")
                .HasPrecision(18, 2);
            money.Property(m => m.Currency)
                .HasColumnName("ReservePriceCurrency")
                .HasMaxLength(3);
        });

        builder.OwnsOne(a => a.BuyNowPrice, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("BuyNowPriceAmount")
                .HasPrecision(18, 2);
            money.Property(m => m.Currency)
                .HasColumnName("BuyNowPriceCurrency")
                .HasMaxLength(3);
        });

        builder.OwnsOne(a => a.CurrentHighBid, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("CurrentHighBidAmount")
                .HasPrecision(18, 2)
                .IsRequired();
            money.Property(m => m.Currency)
                .HasColumnName("CurrentHighBidCurrency")
                .HasMaxLength(3)
                .IsRequired();
        });

        builder.Property(a => a.WinningBidId)
            .HasConversion(
                id => id != null ? id.Value : (Guid?)null,
                value => value.HasValue ? BidId.Create(value.Value) : null);

        builder.Property(a => a.WinningBidderId)
            .HasConversion(
                id => id != null ? id.Value : (Guid?)null,
                value => value.HasValue ? UserId.Create(value.Value) : null);

        builder.OwnsOne(a => a.Settings, settings =>
        {
            settings.Property(s => s.AntiSnipingWindow)
                .HasColumnName("AntiSnipingWindowMinutes")
                .HasConversion(
                    ts => ts.TotalMinutes,
                    minutes => TimeSpan.FromMinutes(minutes));

            settings.Property(s => s.AntiSnipingExtension)
                .HasColumnName("AntiSnipingExtensionMinutes")
                .HasConversion(
                    ts => ts.TotalMinutes,
                    minutes => TimeSpan.FromMinutes(minutes));

            settings.OwnsOne(s => s.MinimumBidIncrement, money =>
            {
                money.Property(m => m.Amount)
                    .HasColumnName("MinBidIncrementAmount")
                    .HasPrecision(18, 2);
                money.Property(m => m.Currency)
                    .HasColumnName("MinBidIncrementCurrency")
                    .HasMaxLength(3);
            });

            settings.Property(s => s.AllowProxyBidding)
                .HasColumnName("AllowProxyBidding");

            settings.Property(s => s.MaxExtensions)
                .HasColumnName("MaxExtensions");

            settings.Property(s => s.RequireDeposit)
                .HasColumnName("RequireDeposit");

            settings.OwnsOne(s => s.DepositAmount, money =>
            {
                money.Property(m => m.Amount)
                    .HasColumnName("DepositAmount")
                    .HasPrecision(18, 2);
                money.Property(m => m.Currency)
                    .HasColumnName("DepositCurrency")
                    .HasMaxLength(3);
            });
        });

        builder.Property(a => a.StartTime).IsRequired();
        builder.Property(a => a.EndTime).IsRequired();
        builder.Property(a => a.ActualEndTime);
        builder.Property(a => a.IsDealerOnly);
        builder.Property(a => a.BidCount);
        builder.Property(a => a.CreatedAt).IsRequired();
        builder.Property(a => a.UpdatedAt);

        builder.Ignore(a => a.BidIds);
        builder.Ignore(a => a.DomainEvents);

        builder.HasIndex(a => a.Status);
        builder.HasIndex(a => a.VehicleId);
        builder.HasIndex(a => a.SellerId);
        builder.HasIndex(a => a.StartTime);
        builder.HasIndex(a => a.EndTime);
    }
}
