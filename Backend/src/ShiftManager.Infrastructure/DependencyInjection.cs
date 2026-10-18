using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ShiftManager.Application.Abstractions;
using ShiftManager.Infrastructure.Countries;
using ShiftManager.Infrastructure.Persistence;

namespace ShiftManager.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, string connectionString, string nagerDateBaseUrl)
    {
        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<ICollaboratorRepository, CollaboratorRepository>();
        services.AddScoped<IShiftRepository, ShiftRepository>();
        services.AddHealthChecks().AddDbContextCheck<AppDbContext>("database");

        services.AddMemoryCache();
        services.AddHttpClient<ICountryCatalog, NagerDateCountryCatalog>(client =>
        {
            client.BaseAddress = new Uri(nagerDateBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(5);
        });

        return services;
    }
}
