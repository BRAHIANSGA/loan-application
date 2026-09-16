using System.Diagnostics;
using LoanApplications.Application;

namespace LoanApplications.Api;

public sealed record SubmitLoanApplicationResponse(
    string Decision,
    IReadOnlyList<string> DenialReasons,
    Guid? CustomerId,
    Guid? LoanApplicationId,
    bool IsReturningCustomer)
{
    public static SubmitLoanApplicationResponse From(SubmissionResult result) => result switch
    {
        SubmissionResult.Approved approved =>
            new("Approved", [], approved.CustomerId, approved.LoanApplicationId, approved.IsReturningCustomer),
        SubmissionResult.Denied denied =>
            new("Denied", [.. denied.Reasons.Select(reason => reason.Code)], null, null, false),
        _ => throw new UnreachableException(),
    };
}
