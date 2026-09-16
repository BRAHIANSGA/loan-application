using System.ComponentModel.DataAnnotations;

namespace LoanApplications.Api;

public sealed record SubmitLoanApplicationRequest(
    [Required, StringLength(100)] string FirstName,
    [Required, StringLength(100)] string LastName,
    [Required] AddressRequest Address,
    [Required, StringLength(200)] string CompanyName,
    [UsdAmount] decimal RequestedAmount,
    [Required, ValidSsn] string Ssn);
