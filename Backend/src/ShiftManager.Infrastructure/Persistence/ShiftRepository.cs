using Microsoft.EntityFrameworkCore;
using ShiftManager.Application.Abstractions;
using ShiftManager.Domain.Shifts;

namespace ShiftManager.Infrastructure.Persistence;

public sealed class ShiftRepository(AppDbContext db) : IShiftRepository
{
    public async Task<IReadOnlyList<Shift>> ListByCollaboratorAndDateAsync(
        Guid collaboratorId, DateOnly date, CancellationToken cancellationToken) =>
        await db.Shifts
            .AsNoTracking()
            .Where(s => s.CollaboratorId == collaboratorId && s.Date == date)
            .ToListAsync(cancellationToken);

    public void Add(Shift shift) => db.Shifts.Add(shift);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => db.SaveChangesAsync(cancellationToken);
}
