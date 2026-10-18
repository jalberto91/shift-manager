using Mediator;
using ShiftManager.Application.Abstractions;
using ShiftManager.Domain.Common;
using ShiftManager.Domain.Shifts;

namespace ShiftManager.Application.Shifts.CreateShift;

public sealed class CreateShiftHandler(ICollaboratorRepository collaborators, IShiftRepository shifts)
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

        var collaborator = await collaborators.GetByIdAsync(shift.CollaboratorId, cancellationToken);
        if (collaborator is not { IsActive: true })
        {
            return ShiftErrors.CollaboratorUnavailable;
        }

        var sameDay = await shifts.ListByCollaboratorAndDateAsync(
            shift.CollaboratorId, shift.Date, excludingId: null, cancellationToken);
        var conflicting = sameDay.FirstOrDefault(existing => shift.OverlapsWith(existing));
        if (conflicting is not null)
        {
            return ShiftErrors.Overlap(conflicting.Id);
        }

        shifts.Add(shift);
        await shifts.SaveChangesAsync(cancellationToken);

        return ShiftDto.From(shift);
    }
}
