using Application.Secrets;
using Hangfire;
using Hangfire.Jobs;
using Infrastructure.Persistance;
using Infrastructure.Secrets;
using Infrastructure.Severa;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Entra;
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

// Serilog integration
builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder
        .ClearProviders()
        .SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace)
        .AddConsole();
});
builder.Services.AddSerilog();

// Hangfire setup
builder.Services.AddHangfire(config =>
{
    config.UseInMemoryStorage();   // Use in-memory storage for demo purposes
    config.UseSerilogLogProvider();
});
builder.Services.AddHangfireServer();

// HTTP client and secret store
builder.Services.AddHttpClient();
builder.Services.AddScoped<TestJob>();
builder.Services.AddDbContext<SursenContext>((services, options) =>
{
    var secretClient = services.GetRequiredService<ISecretClient>();
    var connectionString = secretClient.GetSecretAsync("CONNECTIONSTRING").Result;
    options.UseSqlServer(connectionString,
    b => b.MigrationsAssembly("Infrastructure"));
});

builder.Services.AddScoped<ISecretClient, DopplerClient>(provider =>
{
    var httpClient = provider.GetRequiredService<HttpClient>();
    return new DopplerClient(httpClient, token, environment);
});

// SeveraClient with retry handler
builder.Services.AddScoped<SeveraClient>(provider =>
{
    var secretClient = provider.GetRequiredService<ISecretClient>();
    var logger = provider.GetRequiredService<ILogger<RetryHandler>>();
    var httpClient = new HttpClient(new RetryHandler(new HttpClientHandler(), logger));
    return new SeveraClient(secretClient, httpClient);
});

// Entra retry handler and client
builder.Services.AddTransient<EntraRetryHandler>();
builder.Services.AddScoped<EntraClient>(provider =>
{
    var secretClient = provider.GetRequiredService<ISecretClient>();
    var retryLogger = provider.GetRequiredService<ILogger<EntraRetryHandler>>();
    return new EntraClient(secretClient, retryLogger);
});

// Jobs
builder.Services.AddScoped<TestJob>();

var app = builder.Build();

// Register recurring jobs
using (var scope = app.Services.CreateScope())
{
    var jobs = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();

    // TestJob
    var testJob = scope.ServiceProvider.GetRequiredService<TestJob>();
    jobs.AddOrUpdate(
        "my-recurring-job",
        () => testJob.WriteTest(),
        Cron.Minutely);

}

app.UseHangfireDashboard();
app.Run();
