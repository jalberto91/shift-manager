using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ShiftManager.Infrastructure.Persistence;

public static class DatabaseMigrator
{
    public static async Task ApplyMigrationsAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        logger.LogInformation("Applying database migrations");
        await db.Database.MigrateAsync(cancellationToken);
    }
}
