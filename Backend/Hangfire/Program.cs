using System;
using Application.Secrets;
using Hangfire;
using Hangfire.Common;
using Hangfire.Jobs;
using Infrastructure.Entra;
using Infrastructure.Secrets;
using Infrastructure.Severa;
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
builder.Services.AddSerilog();

builder.Services.AddHangfire(config =>
{
    config.UseInMemoryStorage();
    config.UseSerilogLogProvider();
});
builder.Services.AddHangfireServer();

builder.Services.AddHttpClient();

builder.Services.AddScoped<ISecretClient, DopplerClient>(provider =>
{
    var httpClient = provider.GetRequiredService<HttpClient>();
    return new DopplerClient(httpClient, token, environment);
});

builder.Services.AddScoped<SeveraClient>(provider =>
{
    var secretClient = provider.GetRequiredService<ISecretClient>();
    var logger = provider.GetRequiredService<ILogger<RetryHandler>>();
    var httpClient = new HttpClient(new RetryHandler(new HttpClientHandler(), logger));
    return new SeveraClient(secretClient, httpClient);
});

builder.Services.AddTransient<EntraRetryHandler>();

builder.Services.AddScoped<EntraClient>(provider =>
{
    var secretClient = provider.GetRequiredService<ISecretClient>();
    var retryLogger = provider.GetRequiredService<ILogger<EntraRetryHandler>>();
    return new EntraClient(secretClient, retryLogger);
});

builder.Services.AddScoped<TestJob>();
builder.Services.AddScoped<TestEntraJob>();
builder.Services.AddScoped<FetchGraphUsersJob>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var jobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();

    jobManager.AddOrUpdate<TestJob>(
        "my-recurring-job",
        job => job.WriteTest(),
        Cron.Minutely);

    jobManager.AddOrUpdate<TestEntraJob>(
        "entra-token-job",
        job => job.WriteTestToken(),
        Cron.Minutely);

    jobManager.AddOrUpdate<FetchGraphUsersJob>(
        "graph-users-job",
        job => job.WriteGraphUsers(),
        Cron.Minutely);
}

app.UseHangfireDashboard();
app.Run();




