namespace ShiftManager.Application.Abstractions;

public interface IHolidayCatalog
{
    // Null means the external source did not answer.
    Task<IReadOnlyList<PublicHoliday>?> GetAsync(string countryCode, int year, CancellationToken cancellationToken);
}
