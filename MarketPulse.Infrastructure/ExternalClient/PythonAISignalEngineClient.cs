using System.Net.Http.Json;
using MarketPulse.Application.DTOs;
using MarketPulse.Application.Interfaces;
using MarketPulse.Domain.Exceptions;

namespace MarketPulse.Infrastructure.ExternalClients;

public class PythonAISignalEngineClient : IAISignalEngineClient
{
    private readonly HttpClient _http;

    public PythonAISignalEngineClient(HttpClient http)
        => _http = http;

    public async Task<SignalEngineResponse> GenerateSignalAsync(
        SignalEngineRequest request, CancellationToken ct)
    {
        var response = await _http.PostAsJsonAsync("/generate-signal", request, ct);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            throw new AIEngineUnavailableException((int)response.StatusCode, body);
        }

        var result = await response.Content
            .ReadFromJsonAsync<SignalEngineResponse>(cancellationToken: ct);

        if (result is null)
            throw new AIEngineUnavailableException(200, "Empty response from AI engine");

        return result;
    }
}