using CarAuctions.Domain.Aggregates.Users;
using CarAuctions.Domain.Aggregates.Vehicles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarAuctions.Persistence.Configurations;

public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.ToTable("Vehicles");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.Id)
            .HasConversion(
                id => id.Value,
                value => VehicleId.Create(value))
            .HasColumnName("Id");

        builder.OwnsOne(v => v.VIN, vin =>
        {
            vin.Property(v => v.Value)
                .HasColumnName("VIN")
                .HasMaxLength(17)
                .IsRequired();

            vin.HasIndex(v => v.Value).IsUnique();
        });

        builder.Property(v => v.Make)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(v => v.Model)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(v => v.Year).IsRequired();

        builder.OwnsOne(v => v.Mileage, mileage =>
        {
            mileage.Property(m => m.Value)
                .HasColumnName("Mileage")
                .IsRequired();
            mileage.Property(m => m.Unit)
                .HasColumnName("MileageUnit")
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();
        });

        builder.Property(v => v.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(v => v.TitleStatus)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(v => v.OwnerId)
            .HasConversion(
                id => id.Value,
                value => UserId.Create(value))
            .IsRequired();

        builder.Property(v => v.ExteriorColor).HasMaxLength(50);
        builder.Property(v => v.InteriorColor).HasMaxLength(50);
        builder.Property(v => v.EngineType).HasMaxLength(100);
        builder.Property(v => v.Transmission).HasMaxLength(50);
        builder.Property(v => v.FuelType).HasMaxLength(50);
        builder.Property(v => v.Description).HasMaxLength(4000);
        builder.Property(v => v.IsSalvage);
        builder.Property(v => v.CreatedAt).IsRequired();
        builder.Property(v => v.UpdatedAt);

        builder.OwnsOne(v => v.ConditionReport, report =>
        {
            report.Property(r => r.OverallGrade)
                .HasColumnName("ConditionOverallGrade")
                .HasConversion<int>();
            report.Property(r => r.ExteriorGrade)
                .HasColumnName("ConditionExteriorGrade")
                .HasConversion<int>();
            report.Property(r => r.InteriorGrade)
                .HasColumnName("ConditionInteriorGrade")
                .HasConversion<int>();
            report.Property(r => r.MechanicalGrade)
                .HasColumnName("ConditionMechanicalGrade")
                .HasConversion<int>();
            report.Property(r => r.Notes)
                .HasColumnName("ConditionNotes")
                .HasMaxLength(4000);
            report.Property(r => r.InspectorId)
                .HasColumnName("InspectorId")
                .HasConversion(
                    id => id.Value,
                    value => UserId.Create(value));
            report.Property(r => r.InspectedAt)
                .HasColumnName("InspectedAt");
            report.Property(r => r.HasStructuralDamage)
                .HasColumnName("HasStructuralDamage");
            report.Property(r => r.HasFrameDamage)
                .HasColumnName("HasFrameDamage");
            report.Property(r => r.HasFloodDamage)
                .HasColumnName("HasFloodDamage");
            report.Property(r => r.AirbagDeployed)
                .HasColumnName("AirbagDeployed");
            report.Property(r => r.OdometerDiscrepancy)
                .HasColumnName("OdometerDiscrepancy");
        });

        builder.OwnsMany(v => v.Images, image =>
        {
            image.ToTable("VehicleImages");
            image.WithOwner().HasForeignKey("VehicleId");
            image.HasKey(i => i.Id);
            image.Property(i => i.Url).IsRequired().HasMaxLength(500);
            image.Property(i => i.Type).HasConversion<string>().HasMaxLength(20);
            image.Property(i => i.DisplayOrder);
            image.Property(i => i.IsPrimary);
            image.Property(i => i.UploadedAt);
        });

        builder.Ignore(v => v.DomainEvents);

        builder.HasIndex(v => v.Status);
        builder.HasIndex(v => v.OwnerId);
        builder.HasIndex(v => v.Make);
        builder.HasIndex(v => v.Year);
    }
}
