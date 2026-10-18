using Mediator;
using ShiftManager.Application.Abstractions;
using ShiftManager.Domain.Common;
using ShiftManager.Domain.Shifts;

namespace ShiftManager.Application.Shifts.ListShifts;

public sealed class ListShiftsHandler(
    IShiftRepository shifts, ICollaboratorRepository collaborators, IHolidayCatalog holidayCatalog)
    : IQueryHandler<ListShiftsQuery, Result<IReadOnlyList<ShiftListItemDto>>>
{
    public async ValueTask<Result<IReadOnlyList<ShiftListItemDto>>> Handle(
        ListShiftsQuery query, CancellationToken cancellationToken)
    {
        if (query.From > query.To)
        {
            return ShiftErrors.DateRange;
        }

        var list = await shifts.ListAsync(query.From, query.To, cancellationToken);

        var countries = await collaborators.GetCountryCodesAsync(
            list.Select(s => s.CollaboratorId).Distinct().ToList(), cancellationToken);

        var keys = list.Select(s => (Country: countries[s.CollaboratorId], s.Date.Year)).Distinct().ToList();
        var answers = await Task.WhenAll(
            keys.Select(k => holidayCatalog.GetAsync(k.Country, k.Year, cancellationToken)));
        var holidaysByCountryYear = keys.Zip(answers).ToDictionary(pair => pair.First, pair => pair.Second);

        return list
            .Select(s => ShiftListItemDto.From(
                s, HolidayOn(s.Date, holidaysByCountryYear[(countries[s.CollaboratorId], s.Date.Year)])))
            .ToList();
    }

    private static HolidayInfo HolidayOn(DateOnly date, IReadOnlyList<PublicHoliday>? holidays)
    {
        if (holidays is null)
        {
            return HolidayInfo.Unknown;
        }

        var holiday = holidays
            .Where(h => h.Date == date)
            .OrderByDescending(h => h.IsGlobal)
            .FirstOrDefault();

        return holiday is null ? HolidayInfo.None : HolidayInfo.From(holiday);
    }
}
