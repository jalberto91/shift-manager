using ShiftManager.Application.Shifts.DeleteShift;
using ShiftManager.Domain.Common;
using ShiftManager.Domain.Shifts;

namespace ShiftManager.UnitTests.Application;

public class DeleteShiftHandlerTests
{
    private readonly FakeShiftRepository _shifts = new();

    private Task<Result> Delete(Guid id) =>
        new DeleteShiftHandler(_shifts).Handle(new DeleteShiftCommand(id), CancellationToken.None).AsTask();

    [Fact]
    public async Task Removes_and_saves_the_shift()
    {
        var shift = _shifts.Seed(Guid.NewGuid(), new DateOnly(2026, 3, 10), "09:00", "13:00");

        var result = await Delete(shift.Id);

        Assert.True(result.IsSuccess);
        Assert.Empty(_shifts.Stored);
        Assert.Equal(1, _shifts.SaveCount);
    }

    [Fact]
    public async Task Returns_not_found_for_an_unknown_id()
    {
        var result = await Delete(Guid.NewGuid());

        Assert.Equal([ShiftErrors.NotFound], result.Errors);
        Assert.Equal(0, _shifts.SaveCount);
    }
}
