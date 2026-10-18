namespace ShiftManager.UnitTests.Infrastructure;

internal sealed class StubHandler(Func<HttpResponseMessage> response) : HttpMessageHandler
{
    public int Calls { get; private set; }

    public string? LastPath { get; private set; }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Calls++;
        LastPath = request.RequestUri?.AbsolutePath;
        return Task.FromResult(response());
    }
}
