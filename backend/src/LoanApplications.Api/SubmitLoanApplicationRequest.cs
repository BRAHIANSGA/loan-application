using System.ComponentModel.DataAnnotations;

namespace LoanApplications.Api;

public sealed record SubmitLoanApplicationRequest(
    [Required, StringLength(100)] string FirstName,
    [Required, StringLength(100)] string LastName,
    [Required] AddressRequest Address,
    [Required, StringLength(200)] string CompanyName,
    [Range(1.0, double.MaxValue)] decimal RequestedAmount,
    [Required, RegularExpression(@"^\d{3}-?\d{2}-?\d{4}$", ErrorMessage = "The Ssn field must look like 123-45-6789.")]
    string Ssn);
