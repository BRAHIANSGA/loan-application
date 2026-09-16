using System.Net.Http.Json;
using LoanApplications.Api;

namespace LoanApplications.IntegrationTests;

public static class TestRequests
{
    public const string Endpoint = "/api/loan-applications";

    public static SubmitLoanApplicationRequest Valid(string ssn = "123-45-6789", string state = "TX") => new(
        FirstName: "Jane",
        LastName: "Doe",
        Address: new AddressRequest("1 Main St", "Austin", state, "78701"),
        CompanyName: "Doe Bakery LLC",
        RequestedAmount: 50_000m,
        Ssn: ssn);

    public static async Task<SubmitLoanApplicationResponse> SubmitAsync(
        this HttpClient client,
        SubmitLoanApplicationRequest request)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var response = await client.PostAsJsonAsync(Endpoint, request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<SubmitLoanApplicationResponse>(cancellationToken))!;
    }
}
