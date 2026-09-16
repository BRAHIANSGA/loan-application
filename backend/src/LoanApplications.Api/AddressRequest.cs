using System.ComponentModel.DataAnnotations;

namespace LoanApplications.Api;

public sealed record AddressRequest(
    [Required, StringLength(200)] string Street,
    [Required, StringLength(100)] string City,
    [Required, RegularExpression("^[A-Za-z]{2}$")] string State,
    [Required, RegularExpression(@"^\d{5}(-\d{4})?$")] string ZipCode);
