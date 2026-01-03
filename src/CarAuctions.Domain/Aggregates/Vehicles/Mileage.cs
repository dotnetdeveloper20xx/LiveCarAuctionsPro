using CarAuctions.Domain.Common;
using ErrorOr;

namespace CarAuctions.Domain.Aggregates.Vehicles;

/// <summary>
/// Value object representing vehicle mileage with unit.
/// </summary>
public sealed class Mileage : ValueObject, IComparable<Mileage>
{
    public int Value { get; }
    public MileageUnit Unit { get; }

    private Mileage(int value, MileageUnit unit)
    {
        Value = value;
        Unit = unit;
    }

    public static ErrorOr<Mileage> Create(int value, MileageUnit unit = MileageUnit.Miles)
    {
        if (value < 0)
            return Error.Validation("Mileage.Negative", "Mileage cannot be negative.");

        if (value > 1_000_000)
            return Error.Validation("Mileage.TooHigh", "Mileage exceeds maximum allowed value.");

        return new Mileage(value, unit);
    }

    public static Mileage Zero(MileageUnit unit = MileageUnit.Miles) => new(0, unit);

    public int ToMiles() => Unit == MileageUnit.Kilometers
        ? (int)(Value * 0.621371)
        : Value;

    public int ToKilometers() => Unit == MileageUnit.Miles
        ? (int)(Value * 1.60934)
        : Value;

    public bool IsHighMileage() => ToMiles() > 100_000;

    public int CompareTo(Mileage? other)
    {
        if (other is null) return 1;
        return ToMiles().CompareTo(other.ToMiles());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return ToMiles();
    }

    public override string ToString() => $"{Value:N0} {Unit}";
}

public enum MileageUnit
{
    Miles,
    Kilometers
}
