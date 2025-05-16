using System.Net.Http;

namespace Library.Utils;

/// <summary>
/// A simple wrapper around IHttpClientFactory to centralize HttpClient creation.
/// </summary>
public class HttpClientFactoryWrapper
{
    private readonly IHttpClientFactory _httpClientFactory;

    public HttpClientFactoryWrapper(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    }

    /// <summary>
    /// Creates a new HttpClient instance using a named configuration.
    /// </summary>
    /// <param name="clientName">The name of the configuration. Defaults to "default".</param>
    /// <returns>Configured HttpClient instance.</returns>
    public HttpClient CreateClient(string clientName = "default")
    {
        return _httpClientFactory.CreateClient(clientName);
    }
}