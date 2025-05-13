using Hangfire;
using Hangfire.Jobs;
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

using (var scope = app.Services.CreateScope())
{
    var jobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();

    jobManager.AddOrUpdate(
        "my-recurring-job",
        () => TestJob.WriteTest(), // this will fail if you use instance methods like this
        Cron.Minutely);
}

app.UseHangfireDashboard(); 
app.Run();