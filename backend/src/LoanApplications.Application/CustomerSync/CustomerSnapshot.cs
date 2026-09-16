using LoanApplications.Domain;

namespace LoanApplications.Application.CustomerSync;

public sealed record CustomerSnapshot(
    Guid Id,
    string FirstName,
    string LastName,
    string CompanyName,
    string Ssn,
    Address Address,
    LoanApplicationSnapshot LoanApplication)
{
    public static CustomerSnapshot From(Customer customer) => new(
        customer.Id,
        customer.FirstName,
        customer.LastName,
        customer.CompanyName,
        customer.Ssn.Value,
        customer.Address,
        new LoanApplicationSnapshot(customer.LoanApplication.Id, customer.LoanApplication.RequestedAmount));
}
