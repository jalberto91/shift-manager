using Mediator;
using ShiftManager.Application.Abstractions;
using ShiftManager.Domain.Common;
using ShiftManager.Domain.Shifts;

namespace ShiftManager.Application.Shifts.UpdateShift;

public sealed class UpdateShiftHandler(ICollaboratorRepository collaborators, IShiftRepository shifts)
    : ICommandHandler<UpdateShiftCommand, Result<ShiftDto>>
{
    public async ValueTask<Result<ShiftDto>> Handle(UpdateShiftCommand command, CancellationToken cancellationToken)
    {
        var shift = await shifts.GetByIdAsync(command.Id, cancellationToken);
        if (shift is null)
        {
            return ShiftErrors.NotFound;
        }

        var updated = shift.Update(command.CollaboratorId, command.Date, command.StartTime, command.EndTime);
        if (updated.IsFailure)
        {
            return Result<ShiftDto>.Failure(updated.Errors);
        }

        var collaborator = await collaborators.GetByIdAsync(shift.CollaboratorId, cancellationToken);
        if (collaborator is not { IsActive: true })
        {
            return ShiftErrors.CollaboratorUnavailable;
        }

        var sameDay = await shifts.ListByCollaboratorAndDateAsync(
            shift.CollaboratorId, shift.Date, excludingId: shift.Id, cancellationToken);
        var conflicting = sameDay.FirstOrDefault(existing => shift.OverlapsWith(existing));
        if (conflicting is not null)
        {
            return ShiftErrors.Overlap(conflicting.Id);
        }

        await shifts.SaveChangesAsync(cancellationToken);

        return ShiftDto.From(shift);
    }
}
