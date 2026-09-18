using LoanApplications.Application.CustomerSync;
using LoanApplications.Infrastructure.Outbox;
using LoanApplications.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.PostgreSql;

namespace LoanApplications.IntegrationTests;

public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _database = new PostgreSqlBuilder("postgres:18-alpine").Build();

    public FakeExternalCustomerClient ExternalCustomerClient { get; } = new();

    public async ValueTask InitializeAsync() => await _database.StartAsync();

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await _database.DisposeAsync();
    }

    public async Task ResetStateAsync()
    {
        ExternalCustomerClient.Reset();
        await QueryDatabaseAsync(async (dbContext, cancellationToken) =>
        {
            await dbContext.OutboxMessages.ExecuteDeleteAsync(cancellationToken);
            await dbContext.LoanApplications.ExecuteDeleteAsync(cancellationToken);
            return await dbContext.Customers.ExecuteDeleteAsync(cancellationToken);
        });
    }

    public async Task<T> QueryDatabaseAsync<T>(Func<LoanApplicationsDbContext, CancellationToken, Task<T>> query)
    {
        await using var scope = Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LoanApplicationsDbContext>();
        return await query(dbContext, TestContext.Current.CancellationToken);
    }

    public async Task DispatchOutboxAsync()
    {
        await using var scope = Services.CreateAsyncScope();
        var dispatcher = scope.ServiceProvider.GetRequiredService<OutboxDispatcher>();
        await dispatcher.ProcessPendingAsync(TestContext.Current.CancellationToken);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:LoanApplications", _database.GetConnectionString());
        builder.ConfigureTestServices(services =>
        {
            // No background worker in tests: each test dispatches the outbox itself.
            services.Remove(services.Single(service => service.ImplementationType == typeof(OutboxWorker)));

            services.RemoveAll<IExternalCustomerClient>();
            services.AddSingleton<IExternalCustomerClient>(ExternalCustomerClient);
        });
    }
}
