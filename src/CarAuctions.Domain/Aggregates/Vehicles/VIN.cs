using CarAuctions.Domain.Common;
using ErrorOr;

namespace CarAuctions.Domain.Aggregates.Vehicles;

/// <summary>
/// Vehicle Identification Number value object with validation.
/// </summary>
public sealed class VIN : ValueObject
{
    private static readonly char[] InvalidCharacters = { 'I', 'O', 'Q' };
    private static readonly int[] Weights = { 8, 7, 6, 5, 4, 3, 2, 10, 0, 9, 8, 7, 6, 5, 4, 3, 2 };
    private static readonly string Transliteration = "0123456789.ABCDEFGH..JKLMN.P.R..STUVWXYZ";

    public string Value { get; }

    private VIN(string value)
    {
        Value = value;
    }

    public static ErrorOr<VIN> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Error.Validation("VIN.Empty", "VIN cannot be empty.");

        var normalized = value.ToUpperInvariant().Trim();

        if (normalized.Length != 17)
            return Error.Validation("VIN.InvalidLength", "VIN must be exactly 17 characters.");

        if (normalized.Any(c => InvalidCharacters.Contains(c)))
            return Error.Validation("VIN.InvalidCharacters", "VIN cannot contain I, O, or Q.");

        if (!normalized.All(c => char.IsLetterOrDigit(c)))
            return Error.Validation("VIN.InvalidFormat", "VIN must contain only letters and numbers.");

        if (!IsValidCheckDigit(normalized))
            return Error.Validation("VIN.InvalidCheckDigit", "VIN check digit is invalid.");

        return new VIN(normalized);
    }

    private static bool IsValidCheckDigit(string vin)
    {
        var sum = 0;
        for (var i = 0; i < 17; i++)
        {
            var c = vin[i];
            int value;

            if (char.IsDigit(c))
            {
                value = c - '0';
            }
            else
            {
                var index = Transliteration.IndexOf(c);
                if (index < 0) return false;
                value = index % 10;
            }

            sum += value * Weights[i];
        }

        var checkDigit = sum % 11;
        var expectedChar = checkDigit == 10 ? 'X' : (char)('0' + checkDigit);

        return vin[8] == expectedChar;
    }

    public int GetModelYear()
    {
        var yearChar = Value[9];
        return yearChar switch
        {
            >= 'A' and <= 'H' => 2010 + (yearChar - 'A'),
            >= 'J' and <= 'N' => 2018 + (yearChar - 'J'),
            'P' => 2023,
            'R' => 2024,
            >= 'S' and <= 'Y' => 2025 + (yearChar - 'S'),
            >= '1' and <= '9' => 2001 + (yearChar - '1'),
            '0' => 2010,
            _ => 0
        };
    }

    public string GetWorldManufacturerIdentifier() => Value[..3];

    public string GetVehicleDescriptorSection() => Value.Substring(3, 5);

    public string GetVehicleIdentifierSection() => Value.Substring(9, 8);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(VIN vin) => vin.Value;
}
