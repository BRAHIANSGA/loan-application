using System.Text.RegularExpressions;

namespace LoanApplications.Domain;

public sealed partial record Address
{
    public const int MaxStreetLength = 200;
    public const int MaxCityLength = 100;

    public Address(string street, string city, string state, string zipCode)
    {
        if (!UsStates.IsValid(state))
        {
            throw new ArgumentException($"'{state}' is not a US state code.", nameof(state));
        }

        if (!IsValidZipCode(zipCode))
        {
            throw new ArgumentException("ZIP code must have the format 12345 or 12345-6789.", nameof(zipCode));
        }

        Street = PlainText.Required(street, MaxStreetLength, nameof(street));
        City = PlainText.Required(city, MaxCityLength, nameof(city));
        State = UsStates.Normalize(state);
        ZipCode = zipCode.Trim();
    }

    public string Street { get; private init; }
    public string City { get; private init; }
    public string State { get; private init; }
    public string ZipCode { get; private init; }

    public static bool IsValidZipCode(string zipCode) => ZipCodeFormat().IsMatch(zipCode.Trim());

    [GeneratedRegex(@"^\d{5}(-\d{4})?$")]
    private static partial Regex ZipCodeFormat();
}
