namespace RealEstate.Domain.ValueObjects;

public record PhoneNumber
{
    public string Value { get; }

    public PhoneNumber(string value)
    {
        if (!IsValid(value))
            throw new ArgumentException($"Invalid phone number: {value}", nameof(value));

        Value = value;
    }

    public static bool IsValid(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return false;

        return phoneNumber.StartsWith('+') &&
               phoneNumber.Length >= 11 &&
               phoneNumber.Length <= 16 &&
               phoneNumber[1..].All(char.IsDigit);
    }

    public override string ToString() => Value;

    public static implicit operator string(PhoneNumber phone) => phone.Value;
}
