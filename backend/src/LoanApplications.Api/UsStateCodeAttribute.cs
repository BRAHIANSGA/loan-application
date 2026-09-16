using System.ComponentModel.DataAnnotations;
using LoanApplications.Domain;

namespace LoanApplications.Api;

public sealed class UsStateCodeAttribute() : ValidationAttribute("The {0} field must be a US state code.")
{
    // A missing value is left to [Required], like the built-in attributes do.
    public override bool IsValid(object? value) => value is not string code || UsStates.IsValid(code);
}
