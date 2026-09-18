using LoanApplications.Application;
using LoanApplications.Application.CustomerSync;
using LoanApplications.Domain.Decisions.Rules;
using LoanApplications.Infrastructure.Blacklist;
using LoanApplications.Infrastructure.ExternalService;
using LoanApplications.Infrastructure.Outbox;
using LoanApplications.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LoanApplications.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<LoanApplicationsDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("LoanApplications")));

        // One scoped DbContext sits behind all three. That is the unit of work.
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<LoanApplicationsDbContext>());
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IOutbox, EfOutbox>();

        // Parsed now: a malformed SSN in configuration should stop the app at startup.
        services.AddSingleton<ISsnBlacklist>(new ConfigurationSsnBlacklist(configuration));
        services.TryAddSingleton(TimeProvider.System);

        var externalServiceUrl = configuration["ExternalService:BaseUrl"]
            ?? throw new InvalidOperationException("ExternalService:BaseUrl is not configured.");

        services.AddHttpClient<IExternalCustomerClient, HttpExternalCustomerClient>(client =>
                client.BaseAddress = new Uri(externalServiceUrl))
            .AddStandardResilienceHandler();

        services.AddScoped<OutboxDispatcher>();
        services.AddHostedService<OutboxWorker>();

        return services;
    }

    public static async Task MigrateDatabaseAsync(this IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<LoanApplicationsDbContext>().Database.MigrateAsync();
    }
}
