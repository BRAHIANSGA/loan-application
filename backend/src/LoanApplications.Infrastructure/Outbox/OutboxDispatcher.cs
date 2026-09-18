using System.Text.Json;
using LoanApplications.Application.CustomerSync;
using LoanApplications.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LoanApplications.Infrastructure.Outbox;

internal sealed class OutboxDispatcher(
    LoanApplicationsDbContext dbContext,
    IExternalCustomerClient externalCustomerClient,
    ILogger<OutboxDispatcher> logger)
{
    public const int MaxAttempts = 5;
    private const int BatchSize = 20;

    public async Task ProcessPendingAsync(CancellationToken cancellationToken)
    {
        var pendingMessages = await dbContext.OutboxMessages
            .Where(m => m.Attempts < MaxAttempts)
            .OrderBy(m => m.OccurredAt)
            .ThenBy(m => m.Id)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);

        foreach (var message in pendingMessages)
        {
            try
            {
                await SendAsync(message, cancellationToken);

                // Delivered messages are removed: the payload holds the SSN.
                dbContext.OutboxMessages.Remove(message);
            }
            catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
            {
                message.RecordFailure(exception.Message);

                if (message.Attempts < MaxAttempts)
                {
                    logger.LogWarning(exception, "Outbox message {MessageId} failed on attempt {Attempt}", message.Id, message.Attempts);
                }
                else
                {
                    logger.LogError(exception, "Outbox message {MessageId} gave up after {Attempts} attempts and needs manual attention", message.Id, message.Attempts);
                }

                // An Update must never overtake its Create.
                break;
            }
            finally
            {
                // CancellationToken.None on purpose. Losing this write resends a delivered message after a restart.
                await dbContext.SaveChangesAsync(CancellationToken.None);
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
