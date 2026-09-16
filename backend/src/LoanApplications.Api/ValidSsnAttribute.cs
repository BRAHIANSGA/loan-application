using System.ComponentModel.DataAnnotations;
using LoanApplications.Domain;

namespace LoanApplications.Api;

public sealed class ValidSsnAttribute() : ValidationAttribute("The {0} field must be a valid SSN such as 123-45-6789.")
{
    // A missing value is left to [Required], like the built-in attributes do.
    public override bool IsValid(object? value) => value is not string ssn || Ssn.IsValid(ssn);
}
