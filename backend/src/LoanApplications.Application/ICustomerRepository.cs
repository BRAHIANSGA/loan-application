using LoanApplications.Domain;

namespace LoanApplications.Application;

public interface ICustomerRepository
{
    Task<Customer?> FindBySsnAsync(Ssn ssn, CancellationToken cancellationToken);

    void Add(Customer customer);
}
