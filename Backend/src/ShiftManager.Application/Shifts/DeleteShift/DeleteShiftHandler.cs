using Mediator;
using ShiftManager.Application.Abstractions;
using ShiftManager.Domain.Common;
using ShiftManager.Domain.Shifts;

namespace ShiftManager.Application.Shifts.DeleteShift;

public sealed class DeleteShiftHandler(IShiftRepository shifts) : ICommandHandler<DeleteShiftCommand, Result>
{
    public async ValueTask<Result> Handle(DeleteShiftCommand command, CancellationToken cancellationToken)
    {
        var shift = await shifts.GetByIdAsync(command.Id, cancellationToken);
        if (shift is null)
        {
            return Result.Failure(ShiftErrors.NotFound);
        }

        shifts.Remove(shift);
        await shifts.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
