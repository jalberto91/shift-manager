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

    public Guid CollaboratorId { get; }

    public DateOnly Date { get; }

    public TimeOnly StartTime { get; }

    public TimeOnly EndTime { get; }

    public static Result<Shift> Create(Guid collaboratorId, DateOnly date, TimeOnly startTime, TimeOnly endTime)
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

        return errors.Count > 0
            ? Result<Shift>.Failure(errors)
            : new Shift(Guid.NewGuid(), collaboratorId, date, startTime, endTime);
    }

    // Half-open intervals [start, end): shifts that only touch at a boundary do not overlap.
    public bool OverlapsWith(Shift other) =>
        CollaboratorId == other.CollaboratorId
        && Date == other.Date
        && StartTime < other.EndTime
        && other.StartTime < EndTime;
}
