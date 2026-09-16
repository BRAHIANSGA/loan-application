namespace LoanApplications.Domain;

public sealed record LoanRequest(
    string FirstName,
    string LastName,
    Address Address,
    string CompanyName,
    decimal RequestedAmount,
    Ssn Ssn);
