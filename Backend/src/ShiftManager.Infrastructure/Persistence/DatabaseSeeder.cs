using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ShiftManager.Domain.Collaborators;

namespace ShiftManager.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    private static readonly (string Name, string Email, string CountryCode)[] SeedCollaborators =
    [
        ("Ana Pérez", "ana.perez@example.com", "ES"),
        ("Bruno Silva", "bruno.silva@example.com", "BR"),
        ("Carla Gómez", "carla.gomez@example.com", "AR"),
        ("Diego Hernández", "diego.hernandez@example.com", "MX"),
        ("Elena Rossi", "elena.rossi@example.com", "IT")
    ];

    public static async Task SeedAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (await db.Collaborators.AnyAsync(cancellationToken))
        {
            return;
        }

        foreach (var (name, email, countryCode) in SeedCollaborators)
        {
            var result = Collaborator.Create(name, email, countryCode);
            if (result.IsFailure)
            {
                throw new InvalidOperationException($"Invalid seed collaborator '{email}': {result.Errors[0].Message}");
            }

            db.Collaborators.Add(result.Value);
        }

        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Seeded {Count} collaborators", SeedCollaborators.Length);
    }
}
