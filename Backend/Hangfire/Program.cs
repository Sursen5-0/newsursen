using System;
using Application.Secrets;
using Hangfire;
using Hangfire.Common;
using Hangfire.Jobs;
using Infrastructure.Secrets;
using Infrastructure.Severa;
using Infrastructure.Entra;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

var token = Environment.GetEnvironmentVariable("doppler_key", EnvironmentVariableTarget.Machine);
var environment = Environment.GetEnvironmentVariable("Environment", EnvironmentVariableTarget.Machine);
ArgumentNullException.ThrowIfNull(token);
ArgumentNullException.ThrowIfNull(environment);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore.Hosting", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore.Routing", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// — Serilog integration
builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder
        .ClearProviders()
        .SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace)
        .AddConsole();
});
builder.Services.AddSerilog();

// — Hangfire setup
builder.Services.AddHangfire(config =>
{
    config.UseInMemoryStorage();   // demo storage
    config.UseSerilogLogProvider();
});
builder.Services.AddHangfireServer();

// — HTTP client and secret store
builder.Services.AddHttpClient();
builder.Services.AddScoped<ISecretClient, DopplerClient>(provider =>
{
    var httpClient = provider.GetRequiredService<HttpClient>();
    return new DopplerClient(httpClient, token, environment);
});

// — SeveraClient with retry handler
builder.Services.AddScoped<SeveraClient>(provider =>
{
    var secretClient = provider.GetRequiredService<ISecretClient>();
    var logger = provider.GetRequiredService<ILogger<RetryHandler>>();
    var httpClient = new HttpClient(new RetryHandler(new HttpClientHandler(), logger));
    return new SeveraClient(secretClient, httpClient);
});

// — Entra retry handler and client
builder.Services.AddTransient<EntraRetryHandler>();
builder.Services.AddScoped<EntraClient>(provider =>
{
    var secretClient = provider.GetRequiredService<ISecretClient>();
    var retryLogger = provider.GetRequiredService<ILogger<EntraRetryHandler>>();
    return new EntraClient(secretClient, retryLogger);
});

// — Jobs
builder.Services.AddScoped<TestJob>();
builder.Services.AddScoped<TestEntraJob>();
builder.Services.AddScoped<FetchGraphUsersJob>();

var app = builder.Build();

// — Register recurring jobs
using (var scope = app.Services.CreateScope())
{
    var jobs = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();

    // TestJob
    var testJob = scope.ServiceProvider.GetRequiredService<TestJob>();
    jobs.AddOrUpdate(
        "my-recurring-job",
        () => testJob.WriteTest(),
        Cron.Minutely);

    // TestEntraJob
    var entraJob = scope.ServiceProvider.GetRequiredService<TestEntraJob>();
    jobs.AddOrUpdate(
        "entra-token-job",
        () => entraJob.WriteTestToken(),
        Cron.Minutely);

    // FetchGraphUsersJob
    var graphJob = scope.ServiceProvider.GetRequiredService<FetchGraphUsersJob>();
    jobs.AddOrUpdate(
        "graph-users-job",
        () => graphJob.WriteGraphUsers(),
        Cron.Minutely);
}

app.UseHangfireDashboard();
app.Run();
