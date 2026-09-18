using System.ComponentModel.DataAnnotations;
using LoanApplications.Domain;

namespace LoanApplications.Api;

public sealed class UsdAmountAttribute()
    : ValidationAttribute("Enter an amount between $1 and $1,000,000,000 with at most two decimals.")
{
    public override bool IsValid(object? value) =>
        value is not decimal amount || LoanApplication.IsValidRequestedAmount(amount);
}
