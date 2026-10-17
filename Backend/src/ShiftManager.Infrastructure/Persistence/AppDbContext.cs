using Microsoft.EntityFrameworkCore;
using ShiftManager.Domain.Collaborators;

namespace ShiftManager.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Collaborator> Collaborators => Set<Collaborator>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
}
