using Mediator;
using ShiftManager.Application.Abstractions;
using ShiftManager.Domain.Common;
using ShiftManager.Domain.Shifts;

namespace ShiftManager.Application.Shifts.ExportShifts;

public sealed class ExportShiftsHandler(
    IShiftRepository shifts, ICollaboratorRepository collaborators, IShiftSheetWriter writer)
    : IQueryHandler<ExportShiftsQuery, Result<byte[]>>
{
    public async ValueTask<Result<byte[]>> Handle(ExportShiftsQuery query, CancellationToken cancellationToken)
    {
        if (query.From > query.To)
        {
            return ShiftErrors.DateRange;
        }

        var list = await shifts.ListAsync(query.From, query.To, cancellationToken);

        var emails = await collaborators.GetEmailsAsync(
            list.Select(s => s.CollaboratorId).Distinct().ToList(), cancellationToken);

        var rows = list
            .Select(s => new ShiftExportRow(emails[s.CollaboratorId], s.Date, s.StartTime, s.EndTime))
            .ToList();

        return writer.Write(rows);
    }
}
