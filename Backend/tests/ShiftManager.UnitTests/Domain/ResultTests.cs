using ShiftManager.Domain.Common;

namespace ShiftManager.UnitTests.Domain;

public class ResultTests
{
    [Fact]
    public void Success_exposes_the_value()
    {
        Result<int> result = 42;

        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
        Assert.Null(result.Error);
    }

    [Fact]
    public void Failure_exposes_the_error_and_has_no_value()
    {
        var error = Error.Validation("collaborator.email", "Invalid email");

        Result<int> result = error;

        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
        Assert.Throws<InvalidOperationException>(() => result.Value);
    }

    [Fact]
    public void Failure_can_carry_several_errors()
    {
        var name = Error.Validation("collaborator.name", "Name is required");
        var email = Error.Validation("collaborator.email", "Invalid email");

        var result = Result.Failure([name, email]);

        Assert.Equal([name, email], result.Errors);
        Assert.Equal(name, result.Error);
    }

    [Fact]
    public void Failure_without_errors_is_rejected()
    {
        Assert.Throws<ArgumentException>(() => Result.Failure([]));
    }

    [Fact]
    public void Errors_with_same_code_message_and_type_are_equal_even_with_different_extensions()
    {
        var a = Error.Conflict("shift.overlap", "Solape", new Dictionary<string, object?> { ["conflictingShiftId"] = 1 });
        var b = Error.Conflict("shift.overlap", "Solape", new Dictionary<string, object?> { ["conflictingShiftId"] = 1 });

        Assert.Equal(a, b);
    }
}
