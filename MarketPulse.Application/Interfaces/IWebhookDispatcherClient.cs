public interface IWebhookDispatcherClient
{
    Task<bool>DispatchAsync(string endpoint, string payload, CancellationToken ct);
}