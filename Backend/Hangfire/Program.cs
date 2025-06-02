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
using Infrastructure.Entra;
using Infrastructure.FlowCase;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using System.Net.Http.Headers;
using Infrastructure.Common;
using HangFire.Jobs;

var token = Environment.GetEnvironmentVariable("DOPPLER_KEY");
var environment = Environment.GetEnvironmentVariable("ENVIRONMENT");
ArgumentNullException.ThrowIfNull(token);
ArgumentNullException.ThrowIfNull(environment);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore.Hosting", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore.Routing", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore.Server.Kestrel", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore.Http", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Extensions.Http", LogEventLevel.Warning)
    .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Infrastructure", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Model.Validation", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Query", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Update", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .MinimumLevel.Override("System.Data.SqlClient", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Data.SqlClient", LogEventLevel.Warning).Enrich.FromLogContext()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder
        .ClearProviders()
        .SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace)
        .AddConsole();
});
builder.Services.AddSerilog();

builder.Services.AddHangfire(config =>
{
    config.UseInMemoryStorage();
    config.UseSerilogLogProvider();
});
builder.Services.AddHangfireServer();

builder.Services.AddHttpClient();
builder.Services.AddScoped<TestJob>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<SeveraJobs>();
builder.Services.AddScoped<EntraJobs>();
builder.Services.AddScoped<FlowCaseJob>();
builder.Services.AddScoped<ISkillService, SkillService>();
builder.Services.AddScoped<ISkillRepository, SkillRepository>();
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
    return new DopplerClient(httpClient, token, environment);
});
builder.Services.AddHttpClient<ISeveraClient,SeveraClient>(client =>
{
    client.BaseAddress = new Uri("https://api.severa.visma.com/rest-api/v1.0/");
})
    .ConfigurePrimaryHttpMessageHandler(provider =>
    {
        var logger = provider.GetRequiredService<ILogger<RetryHandler>>();
        return new RetryHandler(new HttpClientHandler(), logger);
    });


builder.Services.AddHttpClient<IEntraClient, EntraClient>()
    .ConfigurePrimaryHttpMessageHandler(provider =>
    {
        var logger = provider.GetRequiredService<ILogger<RetryHandler>>();
        return new RetryHandler(new HttpClientHandler(), logger);
    });


builder.Services.AddHttpClient<IFlowCaseClient, FlowCaseClient>(client =>
{
    client.BaseAddress = new Uri("https://twoday.flowcase.com");
})
    .ConfigurePrimaryHttpMessageHandler(provider =>
    {
        var logger = provider.GetRequiredService<ILogger<RetryHandler>>();
        return new RetryHandler(new HttpClientHandler(), logger);
    });


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var jobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();

    var testJob = scope.ServiceProvider.GetRequiredService<TestJob>();
    jobManager.AddOrUpdate("my-recurring-job", () => testJob.WriteTest(), Cron.Minutely);

    jobManager.AddOrUpdate(
        "SynchronizeEmployees",
        () => scope.ServiceProvider.GetRequiredService<SeveraJobs>().SynchronizeEmployees(),"0 0 31 2 *");
    jobManager.AddOrUpdate(
        "SynchronizeContracts",
        () => scope.ServiceProvider.GetRequiredService<SeveraJobs>().SynchronizeContracts(), "0 0 31 2 *");
    jobManager.AddOrUpdate(
        "SynchronizeAbsence",
        () => scope.ServiceProvider.GetRequiredService<SeveraJobs>().SynchronizeAbsence(), "0 0 31 2 *");
    jobManager.AddOrUpdate(
        "SynchronizeProjects",
        () => scope.ServiceProvider.GetRequiredService<SeveraJobs>().SynchronizeProjects(), "0 0 31 2 *");
    jobManager.AddOrUpdate(
        "SynchronizePhases",
        () => scope.ServiceProvider.GetRequiredService<SeveraJobs>().SynchronizePhases(), "0 0 31 2 *");

    jobManager.AddOrUpdate(
        "SynchronizeEntraEmployees",
        () => scope.ServiceProvider.GetRequiredService<EntraJobs>().GetAllEmployeesEntra(),
        Cron.Daily);
    jobManager.AddOrUpdate(
        "SynchronizeSkillsToSkillsTable",
        () => scope.ServiceProvider.GetRequiredService<FlowCaseJob>().SynchronizeSkillsToSkillsTable(), "0 0 31 2 *");
    jobManager.AddOrUpdate(
        "SynchronizeEmployeesWithFlowcaseIdsAsync",
        () => scope.ServiceProvider.GetRequiredService<FlowCaseJob>().SynchronizeEmployeesWithFlowcaseIdsAsync(), "0 0 31 2 *");
}

app.UseHangfireDashboard();
app.Run();
