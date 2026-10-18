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

        var holidaysByCountryYear = new Dictionary<(string Country, int Year), IReadOnlyList<PublicHoliday>?>();
        var keys = list.Select(s => (Country: countries[s.CollaboratorId], s.Date.Year)).Distinct();
        foreach (var (country, year) in keys)
        {
            holidaysByCountryYear[(country, year)] = await holidayCatalog.GetAsync(country, year, cancellationToken);
        }

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

        var holiday = holidays.FirstOrDefault(h => h.Date == date);

        return holiday is null ? HolidayInfo.None : HolidayInfo.From(holiday);
    }
}
