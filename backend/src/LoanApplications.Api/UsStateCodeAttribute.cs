using System.ComponentModel.DataAnnotations;
using LoanApplications.Domain;

namespace LoanApplications.Api;

public sealed class UsStateCodeAttribute() : ValidationAttribute("Select a US state.")
{
    public override bool IsValid(object? value) => value is not string code || UsStates.IsValid(code);
}
