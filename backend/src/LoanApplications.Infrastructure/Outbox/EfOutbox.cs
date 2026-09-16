using System.Text.Json;
using LoanApplications.Application.CustomerSync;
using LoanApplications.Infrastructure.Persistence;

namespace LoanApplications.Infrastructure.Outbox;

internal sealed class EfOutbox(LoanApplicationsDbContext dbContext, TimeProvider timeProvider) : IOutbox
{
    public void Enqueue(CustomerSyncOperation operation, CustomerSnapshot customer)
    {
        var payload = JsonSerializer.Serialize(customer, JsonSerializerOptions.Web);
        dbContext.OutboxMessages.Add(new OutboxMessage(operation, payload, timeProvider.GetUtcNow()));
    }
}
