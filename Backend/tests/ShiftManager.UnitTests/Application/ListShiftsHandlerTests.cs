using ShiftManager.Application.Shifts;
using ShiftManager.Application.Shifts.ListShifts;
using ShiftManager.Domain.Common;
using ShiftManager.Domain.Shifts;

namespace ShiftManager.UnitTests.Application;

public class ListShiftsHandlerTests
{
    private static readonly DateOnly Day = new(2026, 3, 10);

    private readonly FakeShiftRepository _shifts = new();
    private readonly Guid _collaboratorId = Guid.NewGuid();

    private Task<Result<IReadOnlyList<ShiftDto>>> List(DateOnly? from = null, DateOnly? to = null) =>
        new ListShiftsHandler(_shifts).Handle(new ListShiftsQuery(from, to), CancellationToken.None).AsTask();

    private Shift SeedOn(DateOnly date) => _shifts.Seed(_collaboratorId, date, "09:00", "13:00");

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
}
