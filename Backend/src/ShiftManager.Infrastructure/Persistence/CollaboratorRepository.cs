using Microsoft.EntityFrameworkCore;
using ShiftManager.Application.Abstractions;
using ShiftManager.Domain.Collaborators;

namespace ShiftManager.Infrastructure.Persistence;

public sealed class CollaboratorRepository(AppDbContext db) : ICollaboratorRepository
{
    public async Task<IReadOnlyList<Collaborator>> ListAsync(CancellationToken cancellationToken) =>
        await db.Collaborators
            .AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ThenBy(c => c.Email)
            .ToListAsync(cancellationToken);

    public Task<Collaborator?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        db.Collaborators.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public Task<bool> EmailExistsAsync(string email, Guid? excludingId, CancellationToken cancellationToken) =>
        db.Collaborators.AnyAsync(c => c.Email == email && c.Id != excludingId, cancellationToken);

    public void Add(Collaborator collaborator) => db.Collaborators.Add(collaborator);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => db.SaveChangesAsync(cancellationToken);
}
