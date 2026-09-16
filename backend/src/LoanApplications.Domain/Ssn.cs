using System.Text.RegularExpressions;

namespace LoanApplications.Domain;

public sealed partial record Ssn
{
    public string Value { get; }

    private Ssn(string value) => Value = value;

    public static Ssn Parse(string input) =>
        TryNormalize(input, out var digits)
            ? new Ssn(digits)
            : throw new ArgumentException("SSN must look like 123-45-6789 and be a number the SSA issues.", nameof(input));

    public static bool IsValid(string input) => TryNormalize(input, out _);

    // Masked so a full SSN never leaks through logs or string interpolation.
    public override string ToString() => $"***-**-{Value[^4..]}";

    // The SSA never issues area 000, 666 or 900-999, group 00 or serial 0000.
    private static bool TryNormalize(string input, out string digits)
    {
        var trimmed = input.Trim();
        digits = trimmed.Replace("-", string.Empty);

        return SsnFormat().IsMatch(trimmed)
            && digits[..3] is not ("000" or "666")
            && digits[0] != '9'
            && digits[3..5] != "00"
            && digits[5..] != "0000";
    }

    [GeneratedRegex(@"^\d{3}-?\d{2}-?\d{4}$")]
    private static partial Regex SsnFormat();
}
