using Application.Services;
using Domain.Interfaces.ExternalClients;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services;
using Hangfire;
using Hangfire.Jobs;
using Infrastructure.Persistance;
using Infrastructure.Persistance.Repositories;
using Infrastructure.Secrets;
using Infrastructure.Severa;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using System.Net.Http.Headers;

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
    config.UseInMemoryStorage(); // Use in-memory storage for demo purposes
    config.UseSerilogLogProvider();
});
builder.Services.AddHangfireServer();
builder.Services.AddHttpClient();
builder.Services.AddScoped<TestJob>();
builder.Services.AddScoped<IEmployeeService,EmployeeService>(); 
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>(); 
builder.Services.AddScoped<SeveraJobs>();
builder.Services.AddDbContext<SursenContext>((services, options) =>
{
    var secretClient = services.GetRequiredService<ISecretClient>();
    var connectionString = secretClient.GetSecretAsync("CONNECTIONSTRING").Result;
    options.UseSqlServer(connectionString,
    b => b.MigrationsAssembly("Infrastructure"));
});


builder.Services.AddScoped<ISecretClient, DopplerClient>(provider =>
{
    var clientFactory = provider.GetRequiredService<IHttpClientFactory>();
    var httpClient = clientFactory.CreateClient("doppler");
    httpClient.BaseAddress = new Uri("https://api.doppler.com/v3/");
    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    return new DopplerClient(httpClient, token, environment);
});
builder.Services.AddHttpClient<ISeveraClient,SeveraClient>(client =>
{
    client.BaseAddress = new Uri("https://api.severa.visma.com/rest-api/v1.0/"); // Replace with your actual base URL
})
    .ConfigurePrimaryHttpMessageHandler(provider =>
    {
        var logger = provider.GetRequiredService<ILogger<RetryHandler>>();
        return new RetryHandler(new HttpClientHandler(), logger);
    });
builder.Services.AddLogging(loggingbuilder =>
{
    loggingbuilder.ClearProviders()
    .SetMinimumLevel(LogLevel.Trace)
    .AddConsole();
});
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var jobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();

    jobManager.AddOrUpdate(
        "TestJob",
        () => scope.ServiceProvider.GetRequiredService<TestJob>().WriteTest(),
        Cron.Minutely);
    jobManager.AddOrUpdate(
        "SynchronizeEmployees",
        () => scope.ServiceProvider.GetRequiredService<SeveraJobs>().SynchronizeEmployees(), "0 0 31 2 *");
    jobManager.AddOrUpdate(
        "SynchronizeContracts",
        () => scope.ServiceProvider.GetRequiredService<SeveraJobs>().SynchronizeContracts(), "0 0 31 2 *");
    jobManager.AddOrUpdate(
    "SynchronizeAbsence",
    () => scope.ServiceProvider.GetRequiredService<SeveraJobs>().SynchronizeAbsence(), "0 0 31 2 *");

}

app.UseHangfireDashboard();
app.Run();