namespace LoanApplications.Domain;

public sealed record Address(string Street, string City, string State, string ZipCode)
{
    public string State { get; private init; } = State.Trim().ToUpperInvariant();
}
