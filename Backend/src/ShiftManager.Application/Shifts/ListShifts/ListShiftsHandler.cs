using Mediator;
using ShiftManager.Application.Abstractions;
using ShiftManager.Domain.Common;
using ShiftManager.Domain.Shifts;

namespace ShiftManager.Application.Shifts.ListShifts;

public sealed class ListShiftsHandler(IShiftRepository shifts)
    : IQueryHandler<ListShiftsQuery, Result<IReadOnlyList<ShiftDto>>>
{
    public async ValueTask<Result<IReadOnlyList<ShiftDto>>> Handle(
        ListShiftsQuery query, CancellationToken cancellationToken)
    {
        if (query.From > query.To)
        {
            return ShiftErrors.DateRange;
        }

        var list = await shifts.ListAsync(query.From, query.To, cancellationToken);

        return list.Select(ShiftDto.From).ToList();
    }
}
