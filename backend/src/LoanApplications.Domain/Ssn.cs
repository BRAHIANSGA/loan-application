using System.Text.RegularExpressions;

namespace LoanApplications.Domain;

public sealed partial record Ssn
{
    public string Value { get; }

    private Ssn(string value) => Value = value;

    public static Ssn Parse(string input)
    {
        var trimmed = input.Trim();
        if (!SsnFormat().IsMatch(trimmed))
        {
            throw new ArgumentException("SSN must have the format 123-45-6789 or 123456789.", nameof(input));
        }

        return new Ssn(trimmed.Replace("-", string.Empty));
    }

    // Masked so a full SSN never leaks through logs or string interpolation.
    public override string ToString() => $"***-**-{Value[^4..]}";

    [GeneratedRegex(@"^\d{3}-?\d{2}-?\d{4}$")]
    private static partial Regex SsnFormat();
}
