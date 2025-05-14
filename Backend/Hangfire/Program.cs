using Application.Secrets;
using Hangfire;
using Hangfire.Jobs;
using Infrastructure.Secrets;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHangfire(config =>
{
    config.UseInMemoryStorage(); // Use in-memory storage for demo purposes
});
builder.Services.AddHangfireServer();
builder.Services.AddScoped<TestJob>();
builder.Services.AddScoped<ISecretClient,DopplerClient>();
builder.Services.AddLogging(loggingbuilder =>
{
    loggingbuilder.ClearProviders()
    .SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace)
    .AddConsole();
});
builder.Services.AddHttpClient();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var jobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
    var service = scope.ServiceProvider.GetRequiredService<TestJob>();
    jobManager.AddOrUpdate(
        "my-recurring-job",
        () => service.WriteTest(), 
        Cron.Minutely);
}

app.UseHangfireDashboard(); 
app.Run();