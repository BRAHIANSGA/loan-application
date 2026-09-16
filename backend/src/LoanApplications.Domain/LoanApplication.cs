namespace LoanApplications.Domain;

public sealed class LoanApplication
{
    public Guid Id { get; private set; }
    public decimal RequestedAmount { get; private set; }
    public Guid CustomerId { get; private set; }

    private LoanApplication()
    {
    }

    internal static LoanApplication Create(Guid customerId, decimal requestedAmount)
    {
        var application = new LoanApplication { Id = Guid.CreateVersion7(), CustomerId = customerId };
        application.ChangeRequestedAmount(requestedAmount);
        return application;
    }

    internal void ChangeRequestedAmount(decimal requestedAmount)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(requestedAmount);
        if (decimal.Round(requestedAmount, 2) != requestedAmount)
        {
            throw new ArgumentException("Requested amount cannot have more than two decimals.", nameof(requestedAmount));
        }

        RequestedAmount = requestedAmount;
    }
}
