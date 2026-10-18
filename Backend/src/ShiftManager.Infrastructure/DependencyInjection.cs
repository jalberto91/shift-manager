using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ShiftManager.Application.Abstractions;
using ShiftManager.Infrastructure.Countries;
using ShiftManager.Infrastructure.Persistence;
using ShiftManager.Infrastructure.Spreadsheets;

namespace ShiftManager.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, string connectionString, string nagerDateBaseUrl)
    {
        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<ICollaboratorRepository, CollaboratorRepository>();
        services.AddScoped<IShiftRepository, ShiftRepository>();
        services.AddScoped<IShiftSheetReader, ClosedXmlShiftSheetReader>();
        services.AddHealthChecks().AddDbContextCheck<AppDbContext>("database");

        services.AddMemoryCache();
        var nagerDateAddress = new Uri(nagerDateBaseUrl);
        services.AddHttpClient<ICountryCatalog, NagerDateCountryCatalog>(client =>
        {
            client.BaseAddress = nagerDateAddress;
            client.Timeout = TimeSpan.FromSeconds(5);
        });
        services.AddHttpClient<IHolidayCatalog, NagerDatePublicHolidayCatalog>(client =>
        {
            client.BaseAddress = nagerDateAddress;
            client.Timeout = TimeSpan.FromSeconds(3);
        });

        return services;
    }
}
