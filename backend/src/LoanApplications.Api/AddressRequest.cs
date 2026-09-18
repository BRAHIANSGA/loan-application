using System.ComponentModel.DataAnnotations;
using LoanApplications.Domain;

namespace LoanApplications.Api;

public sealed record AddressRequest(
    [Required, StringLength(Address.MaxStreetLength), PlainText] string Street,
    [Required, StringLength(Address.MaxCityLength), PlainText] string City,
    [Required, UsStateCode] string State,
    [Required, UsZipCode] string ZipCode);
