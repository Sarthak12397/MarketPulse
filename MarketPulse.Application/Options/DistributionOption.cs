using MarketPulse.Domain.Enums;

namespace MarketPulse.Application.Options;

public class DistributionOptions
{
    public List<ChannelConfig> Channels { get; set; } = new();
}

public class ChannelConfig
{
    public DistributionChannel Channel    { get; set; }
    public string WebhookEndpoint        { get; set; } = string.Empty;
    public int MaxAttempts               { get; set; } = 5;
    public bool IsEnabled                { get; set; } = true;
}