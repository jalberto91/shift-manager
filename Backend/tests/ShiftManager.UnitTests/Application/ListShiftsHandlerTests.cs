using ShiftManager.Application.Abstractions;
using ShiftManager.Application.Shifts.ListShifts;
using ShiftManager.Domain.Collaborators;
using ShiftManager.Domain.Common;
using ShiftManager.Domain.Shifts;

namespace ShiftManager.UnitTests.Application;

public class ListShiftsHandlerTests
{
    private static readonly DateOnly Day = new(2026, 3, 10);

    private readonly FakeShiftRepository _shifts = new();
    private readonly FakeCollaboratorRepository _collaborators = new();
    private readonly FakeHolidayCatalog _holidays = new();
    private readonly Collaborator _collaborator;

    public ListShiftsHandlerTests()
    {
        _collaborator = _collaborators.Seed("Ana", "ana@example.com", "ES");
    }

    private Task<Result<IReadOnlyList<ShiftListItemDto>>> List(DateOnly? from = null, DateOnly? to = null) =>
        new ListShiftsHandler(_shifts, _collaborators, _holidays)
            .Handle(new ListShiftsQuery(from, to), CancellationToken.None).AsTask();

    private Shift SeedOn(DateOnly date, Collaborator? collaborator = null) =>
        _shifts.Seed((collaborator ?? _collaborator).Id, date, "09:00", "13:00");

    [Fact]
    public async Task Returns_every_shift_without_filter()
    {
        SeedOn(Day);
        SeedOn(Day.AddDays(30));

        var result = await List();

        Assert.Equal(2, result.Value.Count);
    }

    [Fact]
    public async Task Filters_by_from_only()
    {
        SeedOn(Day.AddDays(-1));
        var kept = SeedOn(Day);

        var result = await List(from: Day);

        Assert.Equal([kept.Id], result.Value.Select(s => s.Id));
    }

    [Fact]
    public async Task Filters_by_to_only()
    {
        var kept = SeedOn(Day);
        SeedOn(Day.AddDays(1));

        var result = await List(to: Day);

        Assert.Equal([kept.Id], result.Value.Select(s => s.Id));
    }

    [Fact]
    public async Task Includes_both_ends_of_the_range()
    {
        SeedOn(Day.AddDays(-1));
        var first = SeedOn(Day);
        var last = SeedOn(Day.AddDays(2));
        SeedOn(Day.AddDays(3));

        var result = await List(Day, Day.AddDays(2));

        Assert.Equal([first.Id, last.Id], result.Value.Select(s => s.Id));
    }

    [Fact]
    public async Task Returns_a_single_day_when_from_equals_to()
    {
        var kept = SeedOn(Day);
        SeedOn(Day.AddDays(1));

        var result = await List(Day, Day);

        Assert.Equal([kept.Id], result.Value.Select(s => s.Id));
    }

    [Fact]
    public async Task Rejects_a_from_date_later_than_the_to_date()
    {
        SeedOn(Day);

        var result = await List(Day.AddDays(1), Day);

        Assert.Equal([ShiftErrors.DateRange], result.Errors);
    }

    [Fact]
    public async Task Marks_a_shift_on_a_nationwide_holiday()
    {
        SeedOn(Day);
        _holidays.Answer("ES", 2026, new PublicHoliday(Day, "Saint Joseph", true));

        var result = await List();

        Assert.Equal(new HolidayInfo(HolidayStatus.Holiday, "Saint Joseph", true), result.Value.Single().Holiday);
    }

    [Fact]
    public async Task Marks_a_shift_on_a_regional_holiday_as_not_global()
    {
        SeedOn(Day);
        _holidays.Answer("ES", 2026, new PublicHoliday(Day, "Regional day", false));

        var result = await List();

        Assert.Equal(new HolidayInfo(HolidayStatus.Holiday, "Regional day", false), result.Value.Single().Holiday);
    }

    [Fact]
    public async Task Marks_a_shift_on_an_ordinary_day_as_none()
    {
        SeedOn(Day);
        _holidays.Answer("ES", 2026, new PublicHoliday(Day.AddDays(1), "Other day", true));

        var result = await List();

        Assert.Equal(HolidayInfo.None, result.Value.Single().Holiday);
    }

    [Fact]
    public async Task Marks_shifts_as_unknown_when_the_holiday_source_does_not_answer()
    {
        var shift = SeedOn(Day);
        _holidays.Fail("ES", 2026);

        var result = await List();

        Assert.True(result.IsSuccess);
        Assert.Equal(shift.Id, result.Value.Single().Id);
        Assert.Equal(HolidayInfo.Unknown, result.Value.Single().Holiday);
    }

    [Fact]
    public async Task Uses_the_country_of_each_collaborator()
    {
        var bruno = _collaborators.Seed("Bruno", "bruno@example.com", "DE");
        SeedOn(Day);
        SeedOn(Day, bruno);
        _holidays.Answer("DE", 2026, new PublicHoliday(Day, "German day", true));

        var result = await List();

        Assert.Equal(
            [HolidayStatus.None, HolidayStatus.Holiday],
            result.Value.OrderBy(s => s.CollaboratorId == bruno.Id).Select(s => s.Holiday.Status));
    }

    [Fact]
    public async Task Asks_once_per_country_and_year()
    {
        SeedOn(Day);
        SeedOn(Day.AddDays(1));
        SeedOn(new DateOnly(2027, 1, 5));

        await List();

        Assert.Equal([("ES", 2026), ("ES", 2027)], _holidays.Calls.OrderBy(c => c.Year));
    }

    [Fact]
    public async Task Marks_shifts_of_deactivated_collaborators()
    {
        SeedOn(Day);
        _collaborator.Deactivate();
        _holidays.Answer("ES", 2026, new PublicHoliday(Day, "Saint Joseph", true));

        var result = await List();

        Assert.Equal(HolidayStatus.Holiday, result.Value.Single().Holiday.Status);
    }

    [Fact]
    public async Task Does_not_call_the_holiday_source_without_shifts()
    {
        var result = await List();

        Assert.Empty(result.Value);
        Assert.Empty(_holidays.Calls);
    }
}
