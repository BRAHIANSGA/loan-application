using LoanApplications.Application.CustomerSync;

namespace LoanApplications.IntegrationTests;

public sealed class FakeExternalCustomerClient : IExternalCustomerClient
{
    private readonly List<(CustomerSyncOperation Operation, Guid CustomerId)> _calls = [];

    public IReadOnlyList<(CustomerSyncOperation Operation, Guid CustomerId)> Calls => _calls;

    public bool IsUnavailable { get; set; }

    public Task CreateAsync(CustomerSnapshot customer, CancellationToken cancellationToken) =>
        RecordAsync(CustomerSyncOperation.Create, customer);

    public Task UpdateAsync(CustomerSnapshot customer, CancellationToken cancellationToken) =>
        RecordAsync(CustomerSyncOperation.Update, customer);

    public void Reset()
    {
        _calls.Clear();
        IsUnavailable = false;
    }

    private Task RecordAsync(CustomerSyncOperation operation, CustomerSnapshot customer)
    {
        if (IsUnavailable)
        {
            throw new HttpRequestException("External service unavailable.");
        }

        _calls.Add((operation, customer.Id));
        return Task.CompletedTask;
    }
}
