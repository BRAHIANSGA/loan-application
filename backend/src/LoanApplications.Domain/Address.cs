using System.Text.RegularExpressions;

namespace LoanApplications.Domain;

public sealed partial record Address
{
    public Address(string street, string city, string state, string zipCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(street);
        ArgumentException.ThrowIfNullOrWhiteSpace(city);

        if (!UsStates.IsValid(state))
        {
            throw new ArgumentException($"'{state}' is not a US state code.", nameof(state));
        }

        if (!ZipCodeFormat().IsMatch(zipCode.Trim()))
        {
            throw new ArgumentException("ZIP code must have the format 12345 or 12345-6789.", nameof(zipCode));
        }

        Street = street.Trim();
        City = city.Trim();
        State = UsStates.Normalize(state);
        ZipCode = zipCode.Trim();
    }

    public string Street { get; private init; }
    public string City { get; private init; }
    public string State { get; private init; }
    public string ZipCode { get; private init; }

    [GeneratedRegex(@"^\d{5}(-\d{4})?$")]
    private static partial Regex ZipCodeFormat();
}
