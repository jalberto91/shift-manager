using ShiftManager.Application.Abstractions;
using ShiftManager.Domain.Common;
using ShiftManager.Domain.Shifts;

namespace ShiftManager.Application.Shifts;

public sealed class ShiftPlacementChecker(ICollaboratorRepository collaborators, IShiftRepository shifts)
{
    public async Task<Error?> CheckAsync(Shift shift, Guid? excludingId, CancellationToken cancellationToken)
    {
        var collaborator = await collaborators.GetByIdAsync(shift.CollaboratorId, cancellationToken);
        if (collaborator is not { IsActive: true })
        {
            return ShiftErrors.CollaboratorUnavailable;
        }

        var sameDay = await shifts.ListByCollaboratorAndDateAsync(
            shift.CollaboratorId, shift.Date, excludingId, cancellationToken);
        var conflicting = sameDay.FirstOrDefault(existing => shift.OverlapsWith(existing));

        return conflicting is null ? null : ShiftErrors.Overlap(conflicting.Id);
    }
}
