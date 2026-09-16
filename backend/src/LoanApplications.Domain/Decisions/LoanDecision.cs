namespace LoanApplications.Domain.Decisions;

public sealed record LoanDecision(IReadOnlyList<DenialReason> DenialReasons)
{
    public bool IsApproved => DenialReasons.Count == 0;
}
