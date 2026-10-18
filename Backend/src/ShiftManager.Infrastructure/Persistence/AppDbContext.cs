using Microsoft.EntityFrameworkCore;
using ShiftManager.Domain.Collaborators;
using ShiftManager.Domain.Shifts;

namespace ShiftManager.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Collaborator> Collaborators => Set<Collaborator>();

    public DbSet<Shift> Shifts => Set<Shift>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
}
