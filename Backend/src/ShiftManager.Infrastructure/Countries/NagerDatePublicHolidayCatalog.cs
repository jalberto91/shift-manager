using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using ShiftManager.Application.Abstractions;

namespace ShiftManager.Infrastructure.Countries;

public sealed class NagerDatePublicHolidayCatalog(
    HttpClient http, IMemoryCache cache, ILogger<NagerDatePublicHolidayCatalog> logger) : IHolidayCatalog
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(24);

    public async Task<IReadOnlyList<PublicHoliday>?> GetAsync(
        string countryCode, int year, CancellationToken cancellationToken)
    {
        var cacheKey = $"nager-date:holidays:{countryCode}:{year}";

        if (cache.TryGetValue(cacheKey, out IReadOnlyList<PublicHoliday>? cached))
        {
            return cached;
        }

        try
        {
            var response = await http.GetFromJsonAsync<List<HolidayResponse>>(
                    $"api/v4/Holidays/{countryCode}/{year}", cancellationToken)
                ?? throw new JsonException("Nager.Date returned an empty body.");

            var holidays = response.Select(h => new PublicHoliday(h.Date, h.Name, h.NationalHoliday)).ToList();
            cache.Set(cacheKey, holidays, CacheDuration);
            return holidays;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException
                                   && !cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(ex, "Nager.Date did not return the {Year} holidays for {CountryCode}", year, countryCode);
            return null;
        }
    }

    private sealed record HolidayResponse(DateOnly Date, string Name, bool NationalHoliday);
}
