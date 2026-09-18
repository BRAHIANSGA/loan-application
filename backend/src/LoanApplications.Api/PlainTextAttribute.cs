using System.ComponentModel.DataAnnotations;
using LoanApplications.Domain;

namespace LoanApplications.Api;

public sealed class PlainTextAttribute() : ValidationAttribute("Remove line breaks and other control characters.")
{
    public override bool IsValid(object? value) => value is not string text || PlainText.IsValid(text);
}
