using System.Net;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using ShiftManager.Infrastructure.Countries;

namespace ShiftManager.UnitTests.Infrastructure;

public class NagerDateCountryCatalogTests
{
    private const string CountriesJson = """[{"countryCode":"ES","name":"Spain"},{"countryCode":"BR","name":"Brazil"}]""";

    private static NagerDateCountryCatalog CatalogReturning(Func<HttpResponseMessage> response, out StubHandler handler)
    {
        handler = new StubHandler(response);
        var http = new HttpClient(handler) { BaseAddress = new Uri("https://nager.test/") };
        return new NagerDateCountryCatalog(http, new MemoryCache(new MemoryCacheOptions()), NullLogger<NagerDateCountryCatalog>.Instance);
    }

    private static HttpResponseMessage Json(string body) => new(HttpStatusCode.OK) { Content = new StringContent(body) };

    [Fact]
    public async Task Knows_a_listed_country_and_rejects_an_unlisted_one()
    {
        var catalog = CatalogReturning(() => Json(CountriesJson), out _);

        Assert.True(await catalog.IsKnownAsync("ES", CancellationToken.None));
        Assert.False(await catalog.IsKnownAsync("ZZ", CancellationToken.None));
    }

    [Fact]
    public async Task Calls_the_service_once_while_the_answer_is_cached()
    {
        var catalog = CatalogReturning(() => Json(CountriesJson), out var handler);

        await catalog.IsKnownAsync("ES", CancellationToken.None);
        await catalog.IsKnownAsync("BR", CancellationToken.None);

        Assert.Equal(1, handler.Calls);
    }

    [Fact]
    public async Task Returns_null_when_the_service_fails()
    {
        var catalog = CatalogReturning(() => new HttpResponseMessage(HttpStatusCode.ServiceUnavailable), out _);

        Assert.Null(await catalog.IsKnownAsync("ES", CancellationToken.None));
    }

    [Fact]
    public async Task Does_not_cache_a_failure()
    {
        var catalog = CatalogReturning(() => new HttpResponseMessage(HttpStatusCode.ServiceUnavailable), out var handler);

        await catalog.IsKnownAsync("ES", CancellationToken.None);
        await catalog.IsKnownAsync("ES", CancellationToken.None);

        Assert.Equal(2, handler.Calls);
    }
}
