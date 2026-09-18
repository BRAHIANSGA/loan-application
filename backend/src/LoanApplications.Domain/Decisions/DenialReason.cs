namespace LoanApplications.Domain.Decisions;

// Not an enum: a new rule must not have to edit a shared type.
public sealed record DenialReason(string Code);
