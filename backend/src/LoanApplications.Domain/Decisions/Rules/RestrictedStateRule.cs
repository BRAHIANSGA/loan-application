namespace LoanApplications.Domain.Decisions.Rules;

public sealed class RestrictedStateRule : IDenialRule
{
    public static readonly DenialReason Reason = new("STATE_NOT_ELIGIBLE");

    private const string RestrictedState = "NY";

    public Task<DenialReason?> EvaluateAsync(LoanRequest request, CancellationToken cancellationToken) =>
        Task.FromResult(request.Address.State == RestrictedState ? Reason : null);
}
