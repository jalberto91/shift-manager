using Microsoft.EntityFrameworkCore;
using ShiftManager.Application.Abstractions;
using ShiftManager.Domain.Collaborators;

namespace ShiftManager.Infrastructure.Persistence;

public sealed class CollaboratorRepository(AppDbContext db) : ICollaboratorRepository
{
    public async Task<IReadOnlyList<Collaborator>> ListAsync(CancellationToken cancellationToken) =>
        await db.Collaborators
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ThenBy(c => c.Email)
            .ToListAsync(cancellationToken);
}
