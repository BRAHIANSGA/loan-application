namespace LoanApplications.Domain;

public sealed class LoanApplication
{
    public const decimal MinRequestedAmount = 1m;

    // Sanity bound, not a credit limit. A credit limit would be a denial rule.
    public const decimal MaxRequestedAmount = 1_000_000_000m;

    public Guid Id { get; private set; }
    public decimal RequestedAmount { get; private set; }
    public Guid CustomerId { get; private set; }

    private LoanApplication()
    {
    }

    public static bool IsValidRequestedAmount(decimal amount) =>
        amount is >= MinRequestedAmount and <= MaxRequestedAmount && decimal.Round(amount, 2) == amount;

    internal static LoanApplication Create(Guid customerId, decimal requestedAmount)
    {
        var application = new LoanApplication { Id = Guid.CreateVersion7(), CustomerId = customerId };
        application.ChangeRequestedAmount(requestedAmount);
        return application;
    }

    internal void ChangeRequestedAmount(decimal requestedAmount)
    {
        if (!IsValidRequestedAmount(requestedAmount))
        {
            throw new ArgumentOutOfRangeException(
                nameof(requestedAmount),
                requestedAmount,
                "Requested amount must be between $1 and $1,000,000,000 with at most two decimals.");
        }

        RequestedAmount = requestedAmount;
    }
}
