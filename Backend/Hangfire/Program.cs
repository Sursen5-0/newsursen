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
using Microsoft.Extensions.Configuration;
using Infrastructure;

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
    config.UseSerilogLogProvider();
});
builder.Services.AddHangfireServer();

builder.Services.AddScoped<SeveraJobs>();
builder.Services.AddScoped<EntraJobs>();
builder.Services.AddScoped<FlowCaseJobs>();
builder.Services.AddScoped<JobRegistry>();
builder.Services.AddScoped<ISkillService, SkillService>();
builder.Services.RegisterInfrastructureServices(token, environment);
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var secretClient = scope.ServiceProvider.GetRequiredService<ISecretClient>();
    var hangfireConnection = secretClient.GetSecretAsync("HANGFIRE_CONNECTIONSTRING").Result;
    var jobRegistry = scope.ServiceProvider.GetRequiredService<JobRegistry>();
    GlobalConfiguration.Configuration
        .UseSqlServerStorage(hangfireConnection)
        .UseSerilogLogProvider();
    jobRegistry.RegisterJobs();
}
app.UseHangfireDashboard();
app.Run();
