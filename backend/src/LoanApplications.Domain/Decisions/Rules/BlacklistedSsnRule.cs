namespace LoanApplications.Domain.Decisions.Rules;

public sealed class BlacklistedSsnRule(ISsnBlacklist blacklist) : IDenialRule
{
    public static readonly DenialReason Reason = new("SSN_BLACKLISTED");

    public async Task<DenialReason?> EvaluateAsync(LoanRequest request, CancellationToken cancellationToken) =>
        await blacklist.ContainsAsync(request.Ssn, cancellationToken) ? Reason : null;
}
