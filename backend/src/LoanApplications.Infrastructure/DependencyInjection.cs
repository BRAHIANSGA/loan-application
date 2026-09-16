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

        // These three resolve the same scoped DbContext, which is what makes them commit together.
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<LoanApplicationsDbContext>());
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IOutbox, EfOutbox>();

        services.AddSingleton<ISsnBlacklist, ConfigurationSsnBlacklist>();
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
}
