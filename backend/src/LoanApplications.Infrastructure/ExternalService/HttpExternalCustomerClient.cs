using System.Net;
using System.Net.Http.Json;
using LoanApplications.Application.CustomerSync;

namespace LoanApplications.Infrastructure.ExternalService;

internal sealed class HttpExternalCustomerClient(HttpClient httpClient) : IExternalCustomerClient
{
    public async Task CreateAsync(CustomerSnapshot customer, CancellationToken cancellationToken)
    {
        using var response = await httpClient.PostAsJsonAsync("customers", customer, cancellationToken);

        // 409 means an earlier attempt already created the customer and only its response was lost.
        if (response.StatusCode != HttpStatusCode.Conflict)
        {
            response.EnsureSuccessStatusCode();
        }
    }

    public async Task UpdateAsync(CustomerSnapshot customer, CancellationToken cancellationToken)
    {
        using var response = await httpClient.PutAsJsonAsync($"customers/{customer.Id}", customer, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
