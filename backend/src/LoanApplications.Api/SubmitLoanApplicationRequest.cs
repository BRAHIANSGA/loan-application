using System.ComponentModel.DataAnnotations;
using LoanApplications.Domain;

namespace LoanApplications.Api;

public sealed record SubmitLoanApplicationRequest(
    [Required, StringLength(Customer.MaxNameLength), PlainText] string FirstName,
    [Required, StringLength(Customer.MaxNameLength), PlainText] string LastName,
    [Required] AddressRequest Address,
    [Required, StringLength(Customer.MaxCompanyNameLength), PlainText] string CompanyName,
    [UsdAmount] decimal RequestedAmount,
    [Required, ValidSsn] string Ssn);
