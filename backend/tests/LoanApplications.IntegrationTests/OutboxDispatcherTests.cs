using LoanApplications.Application.CustomerSync;
using Microsoft.EntityFrameworkCore;

namespace LoanApplications.IntegrationTests;

[Collection(nameof(ApiCollection))]
public sealed class OutboxDispatcherTests(ApiFactory factory) : IAsyncLifetime
{
    public async ValueTask InitializeAsync() => await factory.ResetStateAsync();

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task ProcessPendingAsync_ServiceAvailable_SendsMessagesInOrderAndRemovesThem()
    {
        var client = factory.CreateClient();
        var created = await client.SubmitAsync(TestRequests.Valid());
        await client.SubmitAsync(TestRequests.Valid() with { RequestedAmount = 80_000m });

        await factory.DispatchOutboxAsync();

        var customerId = created.CustomerId!.Value;
        Assert.Equal(
            new[] { (CustomerSyncOperation.Create, customerId), (CustomerSyncOperation.Update, customerId) },
            factory.ExternalCustomerClient.Calls);
        Assert.Equal(0, await factory.QueryDatabaseAsync((db, token) => db.OutboxMessages.CountAsync(token)));
    }

    [Fact]
    public async Task ProcessPendingAsync_ServiceUnavailable_KeepsMessagesAndStopsAtTheFirstFailure()
    {
        var client = factory.CreateClient();
        await client.SubmitAsync(TestRequests.Valid(ssn: "123-45-6789"));
        await client.SubmitAsync(TestRequests.Valid(ssn: "234-56-7890"));
        factory.ExternalCustomerClient.IsUnavailable = true;

        await factory.DispatchOutboxAsync();

        var messages = await factory.QueryDatabaseAsync((db, token) =>
            db.OutboxMessages.OrderBy(m => m.OccurredAt).ToListAsync(token));
        Assert.Equal(new[] { 1, 0 }, messages.Select(message => message.Attempts));
        Assert.Empty(factory.ExternalCustomerClient.Calls);
    }
}
