using Mediator;
using ShiftManager.Application.Abstractions;
using ShiftManager.Domain.Common;
using ShiftManager.Domain.Shifts;

namespace ShiftManager.Application.Shifts.UpdateShift;

public sealed class UpdateShiftHandler(ShiftPlacementChecker placement, IShiftRepository shifts)
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

        var placementError = await placement.CheckAsync(shift, excludingId: shift.Id, cancellationToken);
        if (placementError is not null)
        {
            return placementError;
        }

        await shifts.SaveChangesAsync(cancellationToken);

        return ShiftDto.From(shift);
    }
}
