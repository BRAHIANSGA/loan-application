namespace LoanApplications.Domain.Decisions;

public sealed class LoanDecisionEngine(IEnumerable<IDenialRule> rules)
{
    public async Task<LoanDecision> DecideAsync(LoanRequest request, CancellationToken cancellationToken)
    {
        var denialReasons = new List<DenialReason>();

        foreach (var rule in rules)
        {
            if (await rule.EvaluateAsync(request, cancellationToken) is { } reason)
            {
                denialReasons.Add(reason);
            }
        }

        return new LoanDecision(denialReasons);
    }
}
