using System.ComponentModel.DataAnnotations;
using LoanApplications.Domain;

namespace LoanApplications.Api;

public sealed class ValidSsnAttribute() : ValidationAttribute("Enter a valid Social Security number, such as 123-45-6789.")
{
    public override bool IsValid(object? value) => value is not string ssn || Ssn.IsValid(ssn);
}
