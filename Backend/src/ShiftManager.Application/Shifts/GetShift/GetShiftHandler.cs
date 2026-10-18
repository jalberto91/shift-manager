using Mediator;
using ShiftManager.Application.Abstractions;
using ShiftManager.Domain.Common;
using ShiftManager.Domain.Shifts;

namespace ShiftManager.Application.Shifts.GetShift;

public sealed class GetShiftHandler(IShiftRepository shifts) : IQueryHandler<GetShiftQuery, Result<ShiftDto>>
{
    public async ValueTask<Result<ShiftDto>> Handle(GetShiftQuery query, CancellationToken cancellationToken)
    {
        var shift = await shifts.GetByIdAsync(query.Id, cancellationToken);

        return shift is null ? ShiftErrors.NotFound : ShiftDto.From(shift);
    }
}
