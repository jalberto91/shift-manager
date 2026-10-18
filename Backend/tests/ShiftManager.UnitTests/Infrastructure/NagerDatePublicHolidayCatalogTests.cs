using System.Net;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using ShiftManager.Application.Abstractions;
using ShiftManager.Infrastructure.Countries;

namespace ShiftManager.UnitTests.Infrastructure;

public class NagerDatePublicHolidayCatalogTests
{
    private const string HolidaysJson = """
        [{"date":"2026-01-01","name":"New Year's Day","countryCode":"ES","nationalHoliday":true,"subdivisionCodes":null},
         {"date":"2026-02-28","name":"Day of Andalucía","countryCode":"ES","nationalHoliday":false,"subdivisionCodes":["ES-AN"]}]
        """;

    private static NagerDatePublicHolidayCatalog CatalogReturning(
        Func<HttpResponseMessage> response, out StubHandler handler)
    {
        handler = new StubHandler(response);
        var http = new HttpClient(handler) { BaseAddress = new Uri("https://nager.test/") };
        return new NagerDatePublicHolidayCatalog(
            http, new MemoryCache(new MemoryCacheOptions()), NullLogger<NagerDatePublicHolidayCatalog>.Instance);
    }

    private static HttpResponseMessage Json(string body) => new(HttpStatusCode.OK) { Content = new StringContent(body) };

    [Fact]
    public async Task Maps_global_and_regional_holidays()
    {
        var catalog = CatalogReturning(() => Json(HolidaysJson), out _);

        var holidays = await catalog.GetAsync("ES", 2026, CancellationToken.None);

        Assert.Equal(
            [
                new PublicHoliday(new DateOnly(2026, 1, 1), "New Year's Day", true),
                new PublicHoliday(new DateOnly(2026, 2, 28), "Day of Andalucía", false)
            ],
            holidays);
    }

    [Fact]
    public async Task Requests_the_year_and_country_from_the_service()
    {
        var catalog = CatalogReturning(() => Json(HolidaysJson), out var handler);

        await catalog.GetAsync("ES", 2026, CancellationToken.None);

        Assert.Equal("/api/v4/Holidays/ES/2026", handler.LastPath);
    }

    [Fact]
    public async Task Calls_the_service_once_per_country_and_year_while_cached()
    {
        var catalog = CatalogReturning(() => Json(HolidaysJson), out var handler);

        await catalog.GetAsync("ES", 2026, CancellationToken.None);
        await catalog.GetAsync("ES", 2026, CancellationToken.None);
        await catalog.GetAsync("ES", 2027, CancellationToken.None);

        Assert.Equal(2, handler.Calls);
    }

    [Fact]
    public async Task Returns_null_when_the_service_fails()
    {
        var catalog = CatalogReturning(() => new HttpResponseMessage(HttpStatusCode.ServiceUnavailable), out _);

        Assert.Null(await catalog.GetAsync("ES", 2026, CancellationToken.None));
    }

    [Fact]
    public async Task Returns_null_when_the_body_is_not_valid_json()
    {
        var catalog = CatalogReturning(() => Json("not json"), out _);

        Assert.Null(await catalog.GetAsync("ES", 2026, CancellationToken.None));
    }

    [Fact]
    public async Task Does_not_cache_a_failure()
    {
        var catalog = CatalogReturning(() => new HttpResponseMessage(HttpStatusCode.ServiceUnavailable), out var handler);

        await catalog.GetAsync("ES", 2026, CancellationToken.None);
        await catalog.GetAsync("ES", 2026, CancellationToken.None);

        Assert.Equal(2, handler.Calls);
    }
}
