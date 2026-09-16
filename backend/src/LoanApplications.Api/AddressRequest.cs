using System.ComponentModel.DataAnnotations;

namespace LoanApplications.Api;

public sealed record AddressRequest(
    [Required, StringLength(200)] string Street,
    [Required, StringLength(100)] string City,
    [Required, UsStateCode] string State,
    [Required, RegularExpression(@"^\d{5}(-\d{4})?$", ErrorMessage = "The ZipCode field must look like 12345 or 12345-6789.")]
    string ZipCode);
