using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using ShiftManager.Application.Abstractions;

namespace ShiftManager.Infrastructure.Countries;

public sealed class NagerDateCountryCatalog(HttpClient http, IMemoryCache cache, ILogger<NagerDateCountryCatalog> logger)
    : ICountryCatalog
{
    private const string CacheKey = "nager-date:available-countries";

    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(24);

    public async Task<bool?> IsKnownAsync(string countryCode, CancellationToken cancellationToken)
    {
        var codes = await GetAvailableCodesAsync(cancellationToken);
        return codes?.Contains(countryCode);
    }

    private async Task<HashSet<string>?> GetAvailableCodesAsync(CancellationToken cancellationToken)
    {
        if (cache.TryGetValue(CacheKey, out HashSet<string>? cached))
        {
            return cached;
        }

        try
        {
            var countries = await http.GetFromJsonAsync<List<CountryResponse>>("api/v3/AvailableCountries", cancellationToken)
                ?? throw new JsonException("Nager.Date returned an empty body.");

            var codes = countries.Select(c => c.CountryCode.ToUpperInvariant()).ToHashSet();
            cache.Set(CacheKey, codes, CacheDuration);
            return codes;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException
                                   && !cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(ex, "Nager.Date did not return the available countries; the country code is not verified");
            return null;
        }
    }

    private sealed record CountryResponse(string CountryCode);
}
