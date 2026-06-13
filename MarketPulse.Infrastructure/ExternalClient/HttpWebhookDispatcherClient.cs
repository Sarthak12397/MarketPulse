using System.Text;
using MarketPulse.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace MarketPulse.Infrastructure.ExternalClients;

public class HttpWebhookDispatcherClient : IWebhookDispatcherClient
{
    private readonly HttpClient _http;
    private readonly ILogger<HttpWebhookDispatcherClient> _logger;

    public HttpWebhookDispatcherClient(
        HttpClient http,
        ILogger<HttpWebhookDispatcherClient> logger)
    {
        _http   = http;
        _logger = logger;
    }

    public async Task<bool> DispatchAsync(
        string endpoint, string payload, CancellationToken ct)
    {
        try
        {
            var content  = new StringContent(payload, Encoding.UTF8, "application/json");
            var response = await _http.PostAsync(endpoint, content, ct);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            // Never throw — caller checks bool return value
            // Endpoint is masked in logs — Law 6
            _logger.LogWarning(ex, "Webhook dispatch failed [endpoint masked]");
            return false;
        }
    }
}