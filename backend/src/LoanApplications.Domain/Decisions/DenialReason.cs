namespace LoanApplications.Domain.Decisions;

// A string code instead of an enum, so adding a rule never requires editing a shared type.
public sealed record DenialReason(string Code);
