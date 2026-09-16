using LoanApplications.Application.CustomerSync;

namespace LoanApplications.Infrastructure.Outbox;

public sealed class OutboxMessage(CustomerSyncOperation operation, string payload, DateTimeOffset occurredAt)
{
    public Guid Id { get; private set; } = Guid.CreateVersion7();
    public CustomerSyncOperation Operation { get; private set; } = operation;
    public string Payload { get; private set; } = payload;
    public DateTimeOffset OccurredAt { get; private set; } = occurredAt;
    public DateTimeOffset? ProcessedAt { get; private set; }
    public int Attempts { get; private set; }
    public string? LastError { get; private set; }

    public void MarkAsProcessed(DateTimeOffset processedAt) => ProcessedAt = processedAt;

    public void RecordFailure(string error)
    {
        Attempts++;
        LastError = error;
    }
}
