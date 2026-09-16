namespace LoanApplications.Domain.Decisions.Rules;

public interface ISsnBlacklist
{
    Task<bool> ContainsAsync(Ssn ssn, CancellationToken cancellationToken);
}
