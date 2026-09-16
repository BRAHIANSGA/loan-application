namespace LoanApplications.Application.CustomerSync;

public interface IExternalCustomerClient
{
    Task CreateAsync(CustomerSnapshot customer, CancellationToken cancellationToken);

    Task UpdateAsync(CustomerSnapshot customer, CancellationToken cancellationToken);
}
