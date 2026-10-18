using Mediator;
using ShiftManager.Application.Abstractions;
using ShiftManager.Domain.Common;
using ShiftManager.Domain.Shifts;

namespace ShiftManager.Application.Shifts.CreateShift;

public sealed class CreateShiftHandler(ShiftPlacementChecker placement, IShiftRepository shifts)
    : ICommandHandler<CreateShiftCommand, Result<ShiftDto>>
{
    public async ValueTask<Result<ShiftDto>> Handle(CreateShiftCommand command, CancellationToken cancellationToken)
    {
        var created = Shift.Create(command.CollaboratorId, command.Date, command.StartTime, command.EndTime);
        if (created.IsFailure)
        {
            return Result<ShiftDto>.Failure(created.Errors);
        }

        var shift = created.Value;

        var placementError = await placement.CheckAsync(shift, excludingId: null, cancellationToken);
        if (placementError is not null)
        {
            return placementError;
        }

        shifts.Add(shift);
        await shifts.SaveChangesAsync(cancellationToken);

        return ShiftDto.From(shift);
    }
}
