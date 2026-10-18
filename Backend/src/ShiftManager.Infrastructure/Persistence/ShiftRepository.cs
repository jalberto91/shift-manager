using Microsoft.EntityFrameworkCore;
using ShiftManager.Application.Abstractions;
using ShiftManager.Domain.Shifts;

namespace ShiftManager.Infrastructure.Persistence;

public sealed class ShiftRepository(AppDbContext db) : IShiftRepository
{
    public async Task<IReadOnlyList<Shift>> ListByCollaboratorAndDateAsync(
        Guid collaboratorId, DateOnly date, Guid? excludingId, CancellationToken cancellationToken) =>
        await db.Shifts
            .AsNoTracking()
            .Where(s => s.CollaboratorId == collaboratorId && s.Date == date && s.Id != excludingId)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Shift>> ListAsync(
        DateOnly? from, DateOnly? to, CancellationToken cancellationToken) =>
        await db.Shifts
            .AsNoTracking()
            .Where(s => (from == null || s.Date >= from) && (to == null || s.Date <= to))
            .OrderBy(s => s.Date)
            .ThenBy(s => s.StartTime)
            .ToListAsync(cancellationToken);

    public Task<Shift?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        db.Shifts.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public void Add(Shift shift) => db.Shifts.Add(shift);

    public void Remove(Shift shift) => db.Shifts.Remove(shift);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => db.SaveChangesAsync(cancellationToken);
}
