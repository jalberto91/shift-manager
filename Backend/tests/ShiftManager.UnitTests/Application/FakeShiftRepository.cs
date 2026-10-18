using ShiftManager.Application.Abstractions;
using ShiftManager.Domain.Shifts;

namespace ShiftManager.UnitTests.Application;

internal sealed class FakeShiftRepository : IShiftRepository
{
    public List<Shift> Stored { get; } = [];

    public int SaveCount { get; private set; }

    public Task<IReadOnlyList<Shift>> ListByCollaboratorAndDateAsync(
        Guid collaboratorId, DateOnly date, Guid? excludingId, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Shift>>(
            Stored.Where(s => s.CollaboratorId == collaboratorId && s.Date == date && s.Id != excludingId).ToList());

    public Task<IReadOnlyList<Shift>> ListAsync(DateOnly? from, DateOnly? to, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Shift>>(
            Stored.Where(s => (from is null || s.Date >= from) && (to is null || s.Date <= to)).ToList());

    public Task<Shift?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(Stored.FirstOrDefault(s => s.Id == id));

    public void Add(Shift shift) => Stored.Add(shift);

    public void Remove(Shift shift) => Stored.Remove(shift);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        SaveCount++;
        return Task.CompletedTask;
    }

    public Shift Seed(Guid collaboratorId, DateOnly date, string start, string end)
    {
        var shift = Shift.Create(collaboratorId, date, TimeOnly.Parse(start), TimeOnly.Parse(end)).Value;
        Stored.Add(shift);
        return shift;
    }
}
