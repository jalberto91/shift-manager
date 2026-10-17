using ShiftManager.Application.Abstractions;
using ShiftManager.Domain.Shifts;

namespace ShiftManager.UnitTests.Application;

internal sealed class FakeShiftRepository : IShiftRepository
{
    public List<Shift> Stored { get; } = [];

    public int SaveCount { get; private set; }

    public Task<IReadOnlyList<Shift>> ListByCollaboratorAndDateAsync(
        Guid collaboratorId, DateOnly date, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Shift>>(
            Stored.Where(s => s.CollaboratorId == collaboratorId && s.Date == date).ToList());

    public void Add(Shift shift) => Stored.Add(shift);

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
