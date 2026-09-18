using LoanApplications.Api;
using LoanApplications.Application;
using LoanApplications.Domain.Decisions;
using LoanApplications.Domain.Decisions.Rules;
using LoanApplications.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddValidation();

builder.Services.AddScoped<LoanDecisionEngine>();
builder.Services.AddScoped<IDenialRule, RestrictedStateRule>();
builder.Services.AddScoped<IDenialRule, BlacklistedSsnRule>();
builder.Services.AddScoped<SubmitLoanApplication>();

builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();

await app.Services.MigrateDatabaseAsync();

app.MapLoanApplicationEndpoints();

app.Run();
