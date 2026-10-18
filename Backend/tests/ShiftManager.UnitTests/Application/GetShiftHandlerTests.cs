using ShiftManager.Application.Shifts;
using ShiftManager.Application.Shifts.GetShift;
using ShiftManager.Domain.Common;
using ShiftManager.Domain.Shifts;

namespace ShiftManager.UnitTests.Application;

public class GetShiftHandlerTests
{
    private readonly FakeShiftRepository _shifts = new();

    private Task<Result<ShiftDto>> Get(Guid id) =>
        new GetShiftHandler(_shifts).Handle(new GetShiftQuery(id), CancellationToken.None).AsTask();

    [Fact]
    public async Task Returns_the_shift()
    {
        var shift = _shifts.Seed(Guid.NewGuid(), new DateOnly(2026, 3, 10), "09:00", "13:00");

        var result = await Get(shift.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(shift.Id, result.Value.Id);
    }

    [Fact]
    public async Task Returns_not_found_for_an_unknown_id()
    {
        var result = await Get(Guid.NewGuid());

        Assert.Equal([ShiftErrors.NotFound], result.Errors);
    }
}
