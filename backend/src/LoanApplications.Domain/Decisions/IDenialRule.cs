namespace LoanApplications.Domain.Decisions;

public interface IDenialRule
{
    Task<DenialReason?> EvaluateAsync(LoanRequest request, CancellationToken cancellationToken);
}
