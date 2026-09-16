using LoanApplications.Domain;
using LoanApplications.Domain.Decisions.Rules;
using Microsoft.Extensions.Configuration;

namespace LoanApplications.Infrastructure.Blacklist;

internal sealed class ConfigurationSsnBlacklist(IConfiguration configuration) : ISsnBlacklist
{
    private readonly HashSet<Ssn> _blacklistedSsns =
        [.. (configuration.GetSection("SsnBlacklist").Get<string[]>() ?? []).Select(Ssn.Parse)];

    public Task<bool> ContainsAsync(Ssn ssn, CancellationToken cancellationToken) =>
        Task.FromResult(_blacklistedSsns.Contains(ssn));
}
