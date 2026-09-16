using System.Net;
using System.Net.Http.Json;
using LoanApplications.Application.CustomerSync;
using LoanApplications.Domain;
using LoanApplications.Domain.Decisions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LoanApplications.IntegrationTests;

[Collection(nameof(ApiCollection))]
public sealed class LoanApplicationEndpointTests(ApiFactory factory) : IAsyncLifetime
{
    public async ValueTask InitializeAsync() => await factory.ResetStateAsync();

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task Submit_NewApplicant_StoresCustomerApplicationAndCreateMessage()
    {
        var response = await factory.CreateClient().SubmitAsync(TestRequests.Valid());

        Assert.Equal("Approved", response.Decision);
        Assert.False(response.IsReturningCustomer);

        var application = await factory.QueryDatabaseAsync((db, token) => db.LoanApplications.SingleAsync(token));
        Assert.Equal(response.LoanApplicationId, application.Id);
        Assert.Equal(response.CustomerId, application.CustomerId);

        var message = await factory.QueryDatabaseAsync((db, token) => db.OutboxMessages.SingleAsync(token));
        Assert.Equal(CustomerSyncOperation.Create, message.Operation);
    }

    [Theory]
    [InlineData("NY", "123-45-6789", "STATE_NOT_ELIGIBLE")]
    [InlineData("TX", "111-11-1111", "SSN_BLACKLISTED")]
    public async Task Submit_DeniedApplicant_ReturnsReasonAndStoresNothing(string state, string ssn, string expectedReason)
    {
        var response = await factory.CreateClient().SubmitAsync(TestRequests.Valid(ssn, state));

        Assert.Equal("Denied", response.Decision);
        Assert.Equal(new[] { expectedReason }, response.DenialReasons);
        Assert.Equal(0, await factory.QueryDatabaseAsync((db, token) => db.Customers.CountAsync(token)));
        Assert.Equal(0, await factory.QueryDatabaseAsync((db, token) => db.OutboxMessages.CountAsync(token)));
    }

    [Fact]
    public async Task Submit_ReturningApplicant_UpdatesTheSameCustomerAndApplication()
    {
        var client = factory.CreateClient();
        var first = await client.SubmitAsync(TestRequests.Valid());

        var second = await client.SubmitAsync(
            TestRequests.Valid() with { CompanyName = "Doe Catering LLC", RequestedAmount = 75_000m });

        Assert.True(second.IsReturningCustomer);
        Assert.Equal(first.CustomerId, second.CustomerId);
        Assert.Equal(first.LoanApplicationId, second.LoanApplicationId);

        var customer = await factory.QueryDatabaseAsync((db, token) =>
            db.Customers.Include(c => c.LoanApplication).SingleAsync(token));
        Assert.Equal("Doe Catering LLC", customer.CompanyName);
        Assert.Equal(75_000m, customer.LoanApplication.RequestedAmount);
        Assert.Equal(1, await factory.QueryDatabaseAsync((db, token) => db.LoanApplications.CountAsync(token)));

        var operations = await factory.QueryDatabaseAsync((db, token) =>
            db.OutboxMessages.OrderBy(m => m.OccurredAt).Select(m => m.Operation).ToListAsync(token));
        Assert.Equal(new[] { CustomerSyncOperation.Create, CustomerSyncOperation.Update }, operations);
    }

    [Fact]
    public async Task Submit_OutboxFails_StoresNothing()
    {
        var client = factory
            .WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
                services.AddScoped<IOutbox, UnavailableOutbox>()))
            .CreateClient();

        using var response = await client.PostAsJsonAsync(
            TestRequests.Endpoint, TestRequests.Valid(), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal(0, await factory.QueryDatabaseAsync((db, token) => db.Customers.CountAsync(token)));
        Assert.Equal(0, await factory.QueryDatabaseAsync((db, token) => db.LoanApplications.CountAsync(token)));
    }

    [Fact]
    public async Task Submit_InvalidPayload_ReturnsValidationErrors()
    {
        using var response = await factory.CreateClient().PostAsJsonAsync(
            TestRequests.Endpoint, TestRequests.Valid(ssn: "12-34", state: ""), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>(
            TestContext.Current.CancellationToken);
        Assert.Equal(2, problem!.Errors.Count);
    }

    [Fact]
    public async Task Submit_UnknownState_ReturnsValidationError()
    {
        using var response = await factory.CreateClient().PostAsJsonAsync(
            TestRequests.Endpoint, TestRequests.Valid(state: "ZZ"), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Submit_RuleRegisteredOnlyInDependencyInjection_DeniesWithItsReason()
    {
        var client = factory
            .WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
                services.AddScoped<IDenialRule, ExcludedCompanyRule>()))
            .CreateClient();

        var response = await client.SubmitAsync(TestRequests.Valid() with { CompanyName = ExcludedCompanyRule.CompanyName });

        Assert.Equal("Denied", response.Decision);
        Assert.Equal(new[] { ExcludedCompanyRule.Reason.Code }, response.DenialReasons);
    }

    private sealed class UnavailableOutbox : IOutbox
    {
        public void Enqueue(CustomerSyncOperation operation, CustomerSnapshot customer) =>
            throw new InvalidOperationException("Outbox unavailable.");
    }

    private sealed class ExcludedCompanyRule : IDenialRule
    {
        public const string CompanyName = "Acme Shell Co";
        public static readonly DenialReason Reason = new("COMPANY_NOT_ELIGIBLE");

        public Task<DenialReason?> EvaluateAsync(LoanRequest request, CancellationToken cancellationToken) =>
            Task.FromResult(request.CompanyName == CompanyName ? Reason : null);
    }
}
