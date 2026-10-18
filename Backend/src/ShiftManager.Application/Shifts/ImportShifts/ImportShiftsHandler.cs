using Mediator;
using ShiftManager.Application.Abstractions;
using ShiftManager.Domain.Common;
using ShiftManager.Domain.Shifts;

namespace ShiftManager.Application.Shifts.ImportShifts;

public sealed class ImportShiftsHandler(
    IShiftSheetReader reader,
    ICollaboratorRepository collaborators,
    ShiftPlacementChecker placement,
    IShiftRepository shifts)
    : ICommandHandler<ImportShiftsCommand, Result<ImportReportDto>>
{
    public async ValueTask<Result<ImportReportDto>> Handle(
        ImportShiftsCommand command, CancellationToken cancellationToken)
    {
        var read = reader.Read(command.Content);
        if (read.IsFailure)
        {
            return Result<ImportReportDto>.Failure(read.Errors);
        }

        var rows = read.Value;
        var emails = rows
            .Where(row => row.CellError is null)
            .Select(row => NormalizeEmail(row.Email))
            .Distinct()
            .ToList();
        var collaboratorIds = await collaborators.GetIdsByEmailsAsync(emails, cancellationToken);

        var accepted = new List<Shift>();
        var importedRows = new List<int>();
        var failedRows = new List<ImportFailureDto>();

        foreach (var row in rows)
        {
            var built = await BuildShiftAsync(row, collaboratorIds, accepted, cancellationToken);
            if (built.IsFailure)
            {
                failedRows.Add(ImportFailureDto.From(row.RowNumber, built.Errors));
                continue;
            }

            accepted.Add(built.Value);
            importedRows.Add(row.RowNumber);
        }

        if (accepted.Count > 0)
        {
            accepted.ForEach(shifts.Add);
            await shifts.SaveChangesAsync(cancellationToken);
        }

        return new ImportReportDto(importedRows, failedRows);
    }

    private async Task<Result<Shift>> BuildShiftAsync(
        ShiftSheetRow row,
        IReadOnlyDictionary<string, Guid> collaboratorIds,
        List<Shift> accepted,
        CancellationToken cancellationToken)
    {
        if (row.CellError is not null)
        {
            return row.CellError;
        }

        if (!collaboratorIds.TryGetValue(NormalizeEmail(row.Email), out var collaboratorId))
        {
            return ShiftErrors.CollaboratorUnavailable;
        }

        var created = Shift.Create(collaboratorId, row.Date, row.StartTime, row.EndTime);
        if (created.IsFailure)
        {
            return created;
        }

        var shift = created.Value;
        if (accepted.Any(earlier => shift.OverlapsWith(earlier)))
        {
            return ShiftErrors.OverlapsEarlierRow;
        }

        var placementError = await placement.CheckAsync(shift, excludingId: null, cancellationToken);

        if (placementError is not null)
        {
            return placementError;
        }

        return shift;
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
