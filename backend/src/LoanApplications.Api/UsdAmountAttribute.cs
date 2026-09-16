using System.ComponentModel.DataAnnotations;

namespace LoanApplications.Api;

public sealed class UsdAmountAttribute()
    : ValidationAttribute("The {0} field must be between $1 and $1,000,000,000 with at most two decimals.")
{
    private const decimal Minimum = 1m;

    // Far above any credit limit. It only keeps the value inside the numeric(18,2) column.
    private const decimal Maximum = 1_000_000_000m;

    public override bool IsValid(object? value) =>
        value is not decimal amount || (amount is >= Minimum and <= Maximum && decimal.Round(amount, 2) == amount);
}
