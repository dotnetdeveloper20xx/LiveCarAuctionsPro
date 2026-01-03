namespace CarAuctions.Persistence.Seeding;

public interface IDataSeeder
{
    Task SeedAsync(CancellationToken cancellationToken = default);
}
