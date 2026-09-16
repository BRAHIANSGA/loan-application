using LoanApplications.Application;
using LoanApplications.Domain;
using Microsoft.EntityFrameworkCore;

namespace LoanApplications.Infrastructure.Persistence;

internal sealed class CustomerRepository(LoanApplicationsDbContext dbContext) : ICustomerRepository
{
    public Task<Customer?> FindBySsnAsync(Ssn ssn, CancellationToken cancellationToken) =>
        dbContext.Customers
            .Include(c => c.LoanApplication)
            .SingleOrDefaultAsync(c => c.Ssn == ssn, cancellationToken);

    public void Add(Customer customer) => dbContext.Customers.Add(customer);
}
