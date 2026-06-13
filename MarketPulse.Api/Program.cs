using Hangfire;
using Hangfire.PostgreSql;
using MarketPulse.Api.Middleware;
using MarketPulse.Application.Interfaces;
using MarketPulse.Application.Jobs;
using MarketPulse.Application.Options;
using MarketPulse.Application.Services;
using MarketPulse.Domain.Enums;
using MarketPulse.Infrastructure.BackgroundJobs;
using MarketPulse.Infrastructure.ExternalClients;
using MarketPulse.Infrastructure.Persistence;
using MarketPulse.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ── Database ──────────────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── Unit of Work ──────────────────────────────────────────────────────────
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// ── Repositories ──────────────────────────────────────────────────────────
builder.Services.AddScoped<IAssetRepository,              AssetRepository>();
builder.Services.AddScoped<ICandleRepository,             CandleRepository>();
builder.Services.AddScoped<ISignalJobRepository,          SignalJobRepository>();
builder.Services.AddScoped<ITradingSignalRepository,      TradingSignalRepository>();
builder.Services.AddScoped<ISignalOutcomeRepository,      SignalOutcomeRepository>();
builder.Services.AddScoped<IDistributionRecordRepository, DistributionRecordRepository>();

// ── External Clients ──────────────────────────────────────────────────────
builder.Services.AddHttpClient<IAISignalEngineClient, PythonAISignalEngineClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["PythonAI:BaseUrl"]!);
    client.Timeout     = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient<IWebhookDispatcherClient, HttpWebhookDispatcherClient>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(10);
});

builder.Services.AddHttpClient<IPriceDataClient, BinancePriceDataClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Binance:BaseUrl"]!);
    client.Timeout     = TimeSpan.FromSeconds(10);
});

builder.Services.AddHttpClient<IMarketDataClient, BinanceMarketDataClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Binance:BaseUrl"]!);
    client.Timeout     = TimeSpan.FromSeconds(15);
});

// ── Application Services ──────────────────────────────────────────────────
builder.Services.AddScoped<CandleIngestionService>();
builder.Services.AddScoped<SignalOrchestrationService>();
builder.Services.AddScoped<SignalDistributionService>();
builder.Services.AddScoped<OutcomeEvaluationService>();

// ── Background Jobs ───────────────────────────────────────────────────────
builder.Services.AddScoped<FetchAndGenerateJob>();
builder.Services.AddScoped<DispatchDistributionsJob>();
builder.Services.AddScoped<RetryFailedDistributionsJob>();
builder.Services.AddScoped<EvaluateOutcomesJob>();

// ── Hangfire ──────────────────────────────────────────────────────────────
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(c =>
        c.UseNpgsqlConnection(
            builder.Configuration.GetConnectionString("DefaultConnection"))));

builder.Services.AddHangfireServer(options =>
{
    options.Queues = new[]
    {
        "signal-pipeline", "distribution", "distribution-retry", "evaluation", "default"
    };
});

builder.Services.AddScoped<IDistributionJobScheduler, HangfireJobScheduler>();

// ── Options ───────────────────────────────────────────────────────────────
builder.Services.Configure<DistributionOptions>(
    builder.Configuration.GetSection("Distribution"));

// ── Controllers ───────────────────────────────────────────────────────────
builder.Services.AddControllers();

// ── Build ─────────────────────────────────────────────────────────────────
var app = builder.Build();

// ── Middleware ────────────────────────────────────────────────────────────
app.UseMiddleware<ApiKeyAuthMiddleware>();

app.UseHangfireDashboard("/hangfire");

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

// ── Apply DB migrations ───────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// ── Register recurring jobs ───────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var jobs = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();

    jobs.AddOrUpdate<FetchAndGenerateJob>("fetch-m5",
        j => j.ExecuteForTimeFrameAsync(TimeFrame.M5, CancellationToken.None),
        "*/5 * * * *");

    jobs.AddOrUpdate<FetchAndGenerateJob>("fetch-h1",
        j => j.ExecuteForTimeFrameAsync(TimeFrame.H1, CancellationToken.None),
        "0 * * * *");

    jobs.AddOrUpdate<FetchAndGenerateJob>("fetch-h4",
        j => j.ExecuteForTimeFrameAsync(TimeFrame.H4, CancellationToken.None),
        "0 */4 * * *");

    jobs.AddOrUpdate<FetchAndGenerateJob>("fetch-d1",
        j => j.ExecuteForTimeFrameAsync(TimeFrame.D1, CancellationToken.None),
        "5 0 * * *");

    jobs.AddOrUpdate<DispatchDistributionsJob>("dispatch-distributions",
        j => j.ExecuteAsync(CancellationToken.None),
        "*/5 * * * *");

    jobs.AddOrUpdate<RetryFailedDistributionsJob>("retry-failed",
        j => j.ExecuteAsync(CancellationToken.None),
        "*/15 * * * *");

    jobs.AddOrUpdate<EvaluateOutcomesJob>("evaluate-outcomes",
        j => j.ExecuteAsync(CancellationToken.None),
        "0 * * * *");
}

app.Run();