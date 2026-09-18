using System.ComponentModel.DataAnnotations;
using LoanApplications.Domain;

namespace LoanApplications.Api;

public sealed class UsZipCodeAttribute() : ValidationAttribute("Enter a ZIP code such as 12345 or 12345-6789.")
{
    public override bool IsValid(object? value) => value is not string zipCode || Address.IsValidZipCode(zipCode);
}
