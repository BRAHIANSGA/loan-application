using System.Text.Json;
using LoanApplications.Application.CustomerSync;
using LoanApplications.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LoanApplications.Infrastructure.Outbox;

public sealed class OutboxDispatcher(
    LoanApplicationsDbContext dbContext,
    IExternalCustomerClient externalCustomerClient,
    TimeProvider timeProvider,
    ILogger<OutboxDispatcher> logger)
{
    public const int MaxAttempts = 5;
    private const int BatchSize = 20;

    public async Task ProcessPendingAsync(CancellationToken cancellationToken)
    {
        var pendingMessages = await dbContext.OutboxMessages
            .Where(m => m.ProcessedAt == null && m.Attempts < MaxAttempts)
            .OrderBy(m => m.OccurredAt)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);

        foreach (var message in pendingMessages)
        {
            try
            {
                await SendAsync(message, cancellationToken);
                message.MarkAsProcessed(timeProvider.GetUtcNow());
            }
            catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
            {
                message.RecordFailure(exception.Message);
                logger.LogWarning(exception, "Outbox message {MessageId} failed on attempt {Attempt}", message.Id, message.Attempts);

                // Stop here so an Update never reaches the external service before its Create.
                break;
            }
            finally
            {
                await dbContext.SaveChangesAsync(cancellationToken);
            }
        }
    }

    private Task SendAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        var customer = JsonSerializer.Deserialize<CustomerSnapshot>(message.Payload, JsonSerializerOptions.Web)!;

        return message.Operation == CustomerSyncOperation.Create
            ? externalCustomerClient.CreateAsync(customer, cancellationToken)
            : externalCustomerClient.UpdateAsync(customer, cancellationToken);
    }
}
