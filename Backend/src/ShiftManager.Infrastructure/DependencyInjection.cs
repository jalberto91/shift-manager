using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ShiftManager.Application.Abstractions;
using ShiftManager.Infrastructure.Persistence;

namespace ShiftManager.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<ICollaboratorRepository, CollaboratorRepository>();
        services.AddHealthChecks().AddDbContextCheck<AppDbContext>("database");

        return services;
    }
}
