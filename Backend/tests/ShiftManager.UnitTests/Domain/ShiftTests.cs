using ShiftManager.Domain.Shifts;

namespace ShiftManager.UnitTests.Domain;

public class ShiftTests
{
    private static readonly Guid Collaborator = Guid.NewGuid();
    private static readonly DateOnly Day = new(2026, 3, 10);

    private static Shift ShiftAt(string start, string end, Guid? collaboratorId = null, DateOnly? date = null) =>
        Shift.Create(collaboratorId ?? Collaborator, date ?? Day, TimeOnly.Parse(start), TimeOnly.Parse(end)).Value;

    [Fact]
    public void Create_keeps_the_given_values_and_generates_an_id()
    {
        var result = Shift.Create(Collaborator, Day, new TimeOnly(9, 0), new TimeOnly(13, 0));

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value.Id);
        Assert.Equal(Collaborator, result.Value.CollaboratorId);
        Assert.Equal(Day, result.Value.Date);
        Assert.Equal(new TimeOnly(9, 0), result.Value.StartTime);
        Assert.Equal(new TimeOnly(13, 0), result.Value.EndTime);
    }

    [Theory]
    [InlineData("09:00", "09:00")]
    [InlineData("13:00", "09:00")]
    public void Create_rejects_an_end_that_is_not_after_the_start(string start, string end)
    {
        var result = Shift.Create(Collaborator, Day, TimeOnly.Parse(start), TimeOnly.Parse(end));

        Assert.Equal([ShiftErrors.InvalidTimeRange], result.Errors);
    }

    [Fact]
    public void Create_rejects_a_missing_date()
    {
        var result = Shift.Create(Collaborator, default, new TimeOnly(9, 0), new TimeOnly(13, 0));

        Assert.Equal([ShiftErrors.InvalidDate], result.Errors);
    }

    [Fact]
    public void Create_reports_every_error_at_once()
    {
        var result = Shift.Create(Collaborator, default, new TimeOnly(13, 0), new TimeOnly(9, 0));

        Assert.Equal([ShiftErrors.InvalidDate, ShiftErrors.InvalidTimeRange], result.Errors);
    }

    [Theory]
    [InlineData("09:00", "13:00", "13:00", "17:00")]
    [InlineData("13:00", "17:00", "09:00", "13:00")]
    public void Shifts_that_only_touch_at_a_boundary_do_not_overlap(string startA, string endA, string startB, string endB)
    {
        Assert.False(ShiftAt(startA, endA).OverlapsWith(ShiftAt(startB, endB)));
        Assert.False(ShiftAt(startB, endB).OverlapsWith(ShiftAt(startA, endA)));
    }

    [Theory]
    [InlineData("09:00", "13:00", "12:00", "16:00")]
    [InlineData("12:00", "16:00", "09:00", "13:00")]
    [InlineData("09:00", "17:00", "11:00", "12:00")]
    [InlineData("11:00", "12:00", "09:00", "17:00")]
    [InlineData("09:00", "13:00", "09:00", "13:00")]
    public void Shifts_sharing_part_of_the_interval_overlap(string startA, string endA, string startB, string endB)
    {
        Assert.True(ShiftAt(startA, endA).OverlapsWith(ShiftAt(startB, endB)));
        Assert.True(ShiftAt(startB, endB).OverlapsWith(ShiftAt(startA, endA)));
    }

    [Fact]
    public void The_same_hours_on_another_day_do_not_overlap()
    {
        var other = ShiftAt("09:00", "13:00", date: Day.AddDays(1));

        Assert.False(ShiftAt("09:00", "13:00").OverlapsWith(other));
    }

    [Fact]
    public void The_same_hours_for_another_collaborator_do_not_overlap()
    {
        var other = ShiftAt("09:00", "13:00", collaboratorId: Guid.NewGuid());

        Assert.False(ShiftAt("09:00", "13:00").OverlapsWith(other));
    }

    [Fact]
    public void Overlap_error_carries_the_conflicting_shift_id()
    {
        var id = Guid.NewGuid();

        var error = ShiftErrors.Overlap(id);

        Assert.Equal("shift.overlap", error.Code);
        Assert.Equal(id, error.Extensions!["conflictingShiftId"]);
    }
}
