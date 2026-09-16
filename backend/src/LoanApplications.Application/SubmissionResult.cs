using LoanApplications.Domain.Decisions;

namespace LoanApplications.Application;

public abstract record SubmissionResult
{
    public sealed record Approved(Guid CustomerId, Guid LoanApplicationId, bool IsReturningCustomer) : SubmissionResult;

    public sealed record Denied(IReadOnlyList<DenialReason> Reasons) : SubmissionResult;
}
