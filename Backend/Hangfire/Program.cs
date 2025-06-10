using Application;
using Domain.Interfaces.ExternalClients;
using Hangfire;
using Hangfire.Jobs;
using Serilog;
using Serilog.Events;
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
    .MinimumLevel.Override("Microsoft.Data.SqlClient", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

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
builder.Services.RegisterInfrastructureServices(token, environment);
builder.Services.RegisterApplication();
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
