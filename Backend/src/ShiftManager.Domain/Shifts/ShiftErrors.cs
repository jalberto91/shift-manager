using ShiftManager.Domain.Common;

namespace ShiftManager.Domain.Shifts;

public static class ShiftErrors
{
    public static readonly Error InvalidDate = Error.Validation(
        "shift.date", "Date is required.");

    public static readonly Error InvalidTimeRange = Error.Validation(
        "shift.time-range", "End time must be later than start time on the same day.");

    public static readonly Error CollaboratorUnavailable = Error.Validation(
        "shift.collaborator-unavailable", "Collaborator does not exist or is inactive.");

    public static readonly Error DateRange = Error.Validation(
        "shift.date-range", "From date must not be later than to date.");

    public static readonly Error NotFound = Error.NotFound(
        "shift.not-found", "Shift not found.");

    public static readonly Error OverlapsEarlierRow = Error.Conflict(
        "shift.overlap", "The shift overlaps an earlier row of the same file.");

    public static Error InvalidCell(string column) => Error.Validation(
        "shift.import.cell", $"Column '{column}' is missing or has an unreadable value.");

    public static Error Overlap(Guid conflictingShiftId) => Error.Conflict(
        "shift.overlap",
        "The shift overlaps another shift of the same collaborator.",
        new Dictionary<string, object?> { ["conflictingShiftId"] = conflictingShiftId });
}
