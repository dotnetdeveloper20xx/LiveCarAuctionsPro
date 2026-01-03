using CarAuctions.Domain.Aggregates.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarAuctions.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasConversion(
                id => id.Value,
                value => UserId.Create(value))
            .HasColumnName("Id");

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.Phone)
            .HasMaxLength(20);

        builder.Property(u => u.Roles)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(u => u.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.OwnsOne(u => u.Address, address =>
        {
            address.Property(a => a.Street)
                .HasColumnName("Street")
                .HasMaxLength(200);
            address.Property(a => a.Street2)
                .HasColumnName("Street2")
                .HasMaxLength(200);
            address.Property(a => a.City)
                .HasColumnName("City")
                .HasMaxLength(100);
            address.Property(a => a.State)
                .HasColumnName("State")
                .HasMaxLength(100);
            address.Property(a => a.PostalCode)
                .HasColumnName("PostalCode")
                .HasMaxLength(20);
            address.Property(a => a.Country)
                .HasColumnName("Country")
                .HasMaxLength(100);
        });

        builder.OwnsOne(u => u.CreditLimit, credit =>
        {
            credit.OwnsOne(c => c.TotalLimit, money =>
            {
                money.Property(m => m.Amount)
                    .HasColumnName("CreditLimitAmount")
                    .HasPrecision(18, 2);
                money.Property(m => m.Currency)
                    .HasColumnName("CreditLimitCurrency")
                    .HasMaxLength(3);
            });
            credit.OwnsOne(c => c.UsedAmount, money =>
            {
                money.Property(m => m.Amount)
                    .HasColumnName("CreditUsedAmount")
                    .HasPrecision(18, 2);
                money.Property(m => m.Currency)
                    .HasColumnName("CreditUsedCurrency")
                    .HasMaxLength(3);
            });
        });

        builder.Property(u => u.IsDealer);
        builder.Property(u => u.DealerLicenseNumber).HasMaxLength(50);
        builder.Property(u => u.CompanyName).HasMaxLength(200);
        builder.Property(u => u.IsKycVerified);
        builder.Property(u => u.KycVerifiedAt);
        builder.Property(u => u.CreatedAt).IsRequired();
        builder.Property(u => u.UpdatedAt);
        builder.Property(u => u.LastLoginAt);

        builder.Ignore(u => u.FullName);
        builder.Ignore(u => u.DomainEvents);

        builder.HasIndex(u => u.Email).IsUnique();
        builder.HasIndex(u => u.Status);
        builder.HasIndex(u => u.IsDealer);
    }
}
