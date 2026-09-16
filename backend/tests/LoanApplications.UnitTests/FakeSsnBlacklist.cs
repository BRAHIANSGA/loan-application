using LoanApplications.Domain;
using LoanApplications.Domain.Decisions.Rules;

namespace LoanApplications.UnitTests;

internal sealed class FakeSsnBlacklist(params string[] ssns) : ISsnBlacklist
{
    private readonly HashSet<Ssn> _ssns = [.. ssns.Select(Ssn.Parse)];

    public Task<bool> ContainsAsync(Ssn ssn, CancellationToken cancellationToken) =>
        Task.FromResult(_ssns.Contains(ssn));
}
