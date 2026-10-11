using System.Net;
using System.Text;

namespace Nameless.Auth.Support;

/// <summary>Snapshot of an HTTP request received by the stub (the original is disposed after sending).</summary>
internal sealed record CapturedRequest(
    HttpMethod Method,
    Uri? Uri,
    string? Authorization,
    string? ContentType,
    string? Accept,
    string? Body)
{
    /// <summary>Body parsed as application/x-www-form-urlencoded.</summary>
    public IReadOnlyDictionary<string, string> Form => (Body ?? string.Empty)
        .Split('&', StringSplitOptions.RemoveEmptyEntries)
        .Select(pair => pair.Split('=', 2))
        .ToDictionary(
            p => WebUtility.UrlDecode(p[0]),
            p => p.Length > 1 ? WebUtility.UrlDecode(p[1]) : string.Empty);
}

/// <summary>Ready-made HTTP responses for the tests.</summary>
internal static class Responses
{
    public static HttpResponseMessage Json(string json, HttpStatusCode status = HttpStatusCode.OK)
    {
        return new HttpResponseMessage(status) { Content = new StringContent(json, Encoding.UTF8, "application/json") };
    }

    public static HttpResponseMessage Text(string text, HttpStatusCode status)
    {
        return new HttpResponseMessage(status) { Content = new StringContent(text, Encoding.UTF8, "text/plain") };
    }
}

/// <summary>Handler that records requests and delegates the response to a delegate.</summary>
internal sealed class StubHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<CapturedRequest, HttpResponseMessage> _responder;
    private readonly List<CapturedRequest> _requests = [];

    public StubHttpMessageHandler(Func<CapturedRequest, HttpResponseMessage> responder)
    {
        _responder = responder;
    }

    public IReadOnlyList<CapturedRequest> Requests
    {
        get { lock (_requests) { return [.. _requests]; } }
    }

    /// <summary>Shortcut: always responds with the same JSON.</summary>
    public static StubHttpMessageHandler RespondingWith(string json, HttpStatusCode status = HttpStatusCode.OK)
    {
        return new StubHttpMessageHandler(_ => Responses.Json(json, status));
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var body = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
        var captured = new CapturedRequest(
            request.Method,
            request.RequestUri,
            request.Headers.Authorization?.ToString(),
            request.Content?.Headers.ContentType?.MediaType,
            request.Headers.Accept.ToString(),
            body);

        lock (_requests) { _requests.Add(captured); }
        return _responder(captured);
    }
}

/// <summary>Minimal IHttpClientFactory that always returns clients over the same handler.</summary>
internal sealed class StubHttpClientFactory : IHttpClientFactory
{
    private readonly HttpMessageHandler _handler;

    public StubHttpClientFactory(HttpMessageHandler handler)
    {
        _handler = handler;
    }

    public HttpClient CreateClient(string name)
    {
        return new HttpClient(_handler, disposeHandler: false);
    }
}
