namespace LoanApplications.Domain;

public static class PlainText
{
    // PostgreSQL rejects NUL in text columns, and none of these fields is multi-line.
    public static bool IsValid(string value) => !value.Any(char.IsControl);

    internal static string Required(string value, int maxLength, string paramName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, paramName);
        var text = value.Trim();
        ArgumentOutOfRangeException.ThrowIfGreaterThan(text.Length, maxLength, paramName);
        return IsValid(text) ? text : throw new ArgumentException("Value cannot contain control characters.", paramName);
    }
}
