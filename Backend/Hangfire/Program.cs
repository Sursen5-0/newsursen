using Application.Secrets;
using Hangfire;
using Hangfire.Common;
using Hangfire.Jobs;
using Infrastructure.Entra;
using Infrastructure.Secrets;
using Infrastructure.Severa;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;

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

builder.Services.AddHangfire(config =>
{
    config.UseInMemoryStorage(); // Use in-memory storage for demo purposes
    config.UseSerilogLogProvider();
});
builder.Services.AddHangfireServer();
builder.Services.AddHttpClient();

builder.Services.AddScoped<TestJob>();
builder.Services.AddScoped<ISecretClient, DopplerClient>(provider =>
{
    var httpclient = provider.GetRequiredService<HttpClient>();
    return new DopplerClient(httpclient, token, environment);
});
builder.Services.AddScoped<SeveraClient>(provider =>
{
    var secretClient = provider.GetRequiredService<ISecretClient>();
    var logger = provider.GetRequiredService<ILogger<RetryHandler>>();
    var httpclient = new HttpClient(new RetryHandler(new HttpClientHandler(), logger));
    return new SeveraClient(secretClient, httpclient);
});

// Register EntraClient using secrets for tenant, client ID and secret
builder.Services.AddScoped<EntraClient>(provider =>
{
    var secretClient = provider.GetRequiredService<ISecretClient>();
    var httpClient = provider.GetRequiredService<HttpClient>();
    return new EntraClient(secretClient, httpClient);
});

// Register the test Entra job
builder.Services.AddScoped<TestEntraJob>();

builder.Services.AddLogging(loggingbuilder =>
{
    loggingbuilder.ClearProviders()
        .SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace)
        .AddConsole();
});
builder.Services.AddSerilog();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var jobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();

    // Existing recurring job for TestJob
    var service = scope.ServiceProvider.GetRequiredService<TestJob>();
    jobManager.AddOrUpdate(
        "my-recurring-job",
        () => service.WriteTest(),
        Cron.Minutely);

    // Recurring job for TestEntraJob
    jobManager.AddOrUpdate<TestEntraJob>(
      "entra-token-job",
      job => job.WriteTest(),
      Cron.Minutely);
}

app.UseHangfireDashboard();
app.Run();
