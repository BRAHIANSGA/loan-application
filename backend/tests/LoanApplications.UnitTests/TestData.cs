using LoanApplications.Domain;

namespace LoanApplications.UnitTests;

internal static class TestData
{
    public static LoanRequest ValidRequest() => new(
        FirstName: "Jane",
        LastName: "Doe",
        Address: AddressIn("TX"),
        CompanyName: "Doe Bakery LLC",
        RequestedAmount: 50_000m,
        Ssn: Ssn.Parse("123-45-6789"));

    public static Address AddressIn(string state) => new("1 Main St", "Springfield", state, "12345");
}
