using LoanApplications.Application;
using LoanApplications.Domain;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LoanApplications.Api;

public static class LoanApplicationEndpoints
{
    public static void MapLoanApplicationEndpoints(this IEndpointRouteBuilder app) =>
        app.MapPost("/api/loan-applications", SubmitAsync);

    private static async Task<Ok<SubmitLoanApplicationResponse>> SubmitAsync(
        SubmitLoanApplicationRequest request,
        SubmitLoanApplication submitLoanApplication,
        CancellationToken cancellationToken)
    {
        var result = await submitLoanApplication.SubmitAsync(ToLoanRequest(request), cancellationToken);
        return TypedResults.Ok(SubmitLoanApplicationResponse.From(result));
    }

    private static LoanRequest ToLoanRequest(SubmitLoanApplicationRequest request) => new(
        request.FirstName,
        request.LastName,
        new Address(request.Address.Street, request.Address.City, request.Address.State, request.Address.ZipCode),
        request.CompanyName,
        request.RequestedAmount,
        Ssn.Parse(request.Ssn));
}
