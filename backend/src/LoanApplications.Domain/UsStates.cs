using System.Collections.Frozen;

namespace LoanApplications.Domain;

public static class UsStates
{
    private static readonly FrozenSet<string> Codes = new[]
    {
        "AL", "AK", "AZ", "AR", "CA", "CO", "CT", "DE", "DC", "FL", "GA", "HI", "ID", "IL", "IN", "IA",
        "KS", "KY", "LA", "ME", "MD", "MA", "MI", "MN", "MS", "MO", "MT", "NE", "NV", "NH", "NJ", "NM",
        "NY", "NC", "ND", "OH", "OK", "OR", "PA", "RI", "SC", "SD", "TN", "TX", "UT", "VT", "VA", "WA",
        "WV", "WI", "WY",
    }.ToFrozenSet();

    public static string Normalize(string code) => code.Trim().ToUpperInvariant();

    public static bool IsValid(string code) => Codes.Contains(Normalize(code));
}
