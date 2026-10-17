namespace ShiftManager.Application.Abstractions;

public interface ICountryCatalog
{
    // Null means the external source did not answer.
    Task<bool?> IsKnownAsync(string countryCode, CancellationToken cancellationToken);
}
