using Hangfire;
using Hangfire.SqlServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using System.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHangfire(config =>
{
    config.UseInMemoryStorage(); // Use in-memory storage for demo purposes
});
builder.Services.AddHangfireServer();

var app = builder.Build();

// Use Hangfire Dashboard
app.UseHangfireDashboard(); // By default, available at /hangfire

app.MapGet("/", () => "Hangfire Dashboard is available at /hangfire");

app.Run();