using CarAuctions.Domain.Common;

namespace CarAuctions.Domain.Aggregates.Users;

/// <summary>
/// Value object representing a physical address.
/// </summary>
public sealed class Address : ValueObject
{
    public string Street { get; }
    public string? Street2 { get; }
    public string City { get; }
    public string State { get; }
    public string PostalCode { get; }
    public string Country { get; }

    private Address(string street, string? street2, string city, string state, string postalCode, string country)
    {
        Street = street;
        Street2 = street2;
        City = city;
        State = state;
        PostalCode = postalCode;
        Country = country;
    }

    public static Address Create(
        string street,
        string city,
        string state,
        string postalCode,
        string country,
        string? street2 = null)
    {
        return new Address(
            street.Trim(),
            street2?.Trim(),
            city.Trim(),
            state.Trim(),
            postalCode.Trim(),
            country.Trim());
    }

    public string ToSingleLine() => Street2 is null
        ? $"{Street}, {City}, {State} {PostalCode}, {Country}"
        : $"{Street}, {Street2}, {City}, {State} {PostalCode}, {Country}";

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Street;
        yield return Street2;
        yield return City;
        yield return State;
        yield return PostalCode;
        yield return Country;
    }
}
