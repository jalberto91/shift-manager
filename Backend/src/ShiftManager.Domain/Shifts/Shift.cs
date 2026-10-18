using ShiftManager.Domain.Common;

namespace ShiftManager.Domain.Shifts;

public sealed class Shift
{
    private Shift(Guid id, Guid collaboratorId, DateOnly date, TimeOnly startTime, TimeOnly endTime)
    {
        Id = id;
        CollaboratorId = collaboratorId;
        Date = date;
        StartTime = startTime;
        EndTime = endTime;
    }

    public Guid Id { get; }

    public Guid CollaboratorId { get; private set; }

    public DateOnly Date { get; private set; }

    public TimeOnly StartTime { get; private set; }

    public TimeOnly EndTime { get; private set; }

    public static Result<Shift> Create(Guid collaboratorId, DateOnly date, TimeOnly startTime, TimeOnly endTime)
    {
        var valid = Validate(date, startTime, endTime);

        return valid.IsFailure
            ? Result<Shift>.Failure(valid.Errors)
            : new Shift(Guid.NewGuid(), collaboratorId, date, startTime, endTime);
    }

    public Result Update(Guid collaboratorId, DateOnly date, TimeOnly startTime, TimeOnly endTime)
    {
        var valid = Validate(date, startTime, endTime);

        if (valid.IsFailure)
        {
            return valid;
        }

        CollaboratorId = collaboratorId;
        Date = date;
        StartTime = startTime;
        EndTime = endTime;
        return Result.Success();
    }

    // Half-open intervals [start, end): shifts that only touch at a boundary do not overlap.
    public bool OverlapsWith(Shift other) =>
        CollaboratorId == other.CollaboratorId
        && Date == other.Date
        && StartTime < other.EndTime
        && other.StartTime < EndTime;

    private static Result Validate(DateOnly date, TimeOnly startTime, TimeOnly endTime)
    {
        var errors = new List<Error>();

        if (date == default)
        {
            errors.Add(ShiftErrors.InvalidDate);
        }

        if (endTime <= startTime)
        {
            errors.Add(ShiftErrors.InvalidTimeRange);
        }

        return errors.Count > 0 ? Result.Failure(errors) : Result.Success();
    }
}
