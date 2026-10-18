namespace ShiftManager.Application.Abstractions;

public interface IHolidayCatalog
{
    Task<IReadOnlyList<PublicHoliday>?> GetAsync(string countryCode, int year, CancellationToken cancellationToken);
}
