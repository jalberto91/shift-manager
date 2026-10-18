using ShiftManager.Application.Abstractions;

namespace ShiftManager.UnitTests.Application;

internal sealed class FakeHolidayCatalog : IHolidayCatalog
{
    private readonly Dictionary<(string Country, int Year), IReadOnlyList<PublicHoliday>?> _answers = [];

    public List<(string Country, int Year)> Calls { get; } = [];

    public void Answer(string country, int year, params PublicHoliday[] holidays) =>
        _answers[(country, year)] = holidays;

    public void Fail(string country, int year) => _answers[(country, year)] = null;

    public Task<IReadOnlyList<PublicHoliday>?> GetAsync(string countryCode, int year, CancellationToken cancellationToken)
    {
        Calls.Add((countryCode, year));
        return Task.FromResult(_answers.GetValueOrDefault((countryCode, year), []));
    }
}
