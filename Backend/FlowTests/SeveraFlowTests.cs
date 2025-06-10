using Microsoft.Extensions.DependencyInjection;
using Hangfire;
using Application;
using Infrastructure;
using Hangfire.Common;
using Hangfire.Jobs;
using System.Diagnostics;
using Infrastructure.Persistance;
using Application.Services;
using Domain.Interfaces.Services;
using Serilog;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.Identity.Client.Extensions.Msal;
using Hangfire.MemoryStorage;
using Hangfire.Storage;

namespace FlowTests
{
    public class SeveraFlowTests
    {
        private readonly IBackgroundJobClient _jobClient;
        private readonly JobStorage _jobStorage;
        private readonly ServiceProvider _provider;
        private readonly BackgroundJobServer _server;
        public SeveraFlowTests()
        {
            var token = Environment.GetEnvironmentVariable("DOPPLER_KEY");
            var environment = Environment.GetEnvironmentVariable("ENVIRONMENT");

            IServiceCollection services = new ServiceCollection();

            services.AddSerilog();
            services.AddHangfire((provider, config) =>
            {
                config.UseMemoryStorage();
                config.UseSerilogLogProvider();
                config.UseActivator(new TestJobActivator(provider));
            });
            services.RegisterInfrastructureServices(token, environment, true);

            //The specific service for the test needs to be registered after the Hangfire configuration
            services.AddScoped<EmployeeService>();

            services.RegisterJobs();
            _provider = services.BuildServiceProvider();
            _jobStorage = _provider.GetRequiredService<JobStorage>();

            _jobClient = _provider.GetRequiredService<IBackgroundJobClient>();
            _server = new BackgroundJobServer(new BackgroundJobServerOptions
            {
                WorkerCount = 1
            }, _jobStorage);
        }

        [Fact]
        public async Task HangfireJob_Synchronizes_Projects()
        {
            // Arrange 
            var scope = _provider.CreateScope();
            var severaJobs = scope.ServiceProvider.GetRequiredService<EmployeeService>();

            // Act
            var jobId = _jobClient.Enqueue(() => severaJobs.SynchronizeProjects());
            await WaitForJobToComplete(jobId);


            // Assert
            var db = scope.ServiceProvider.GetRequiredService<SursenContext>();
            Assert.NotEmpty(db.Projects);

        }

        private async Task WaitForJobToComplete(string jobId, int timeoutSeconds = 300)
        {
            var monitor = _jobStorage.GetMonitoringApi();
            var sw = Stopwatch.StartNew();
            while (sw.Elapsed < TimeSpan.FromSeconds(timeoutSeconds))
            {
                var succeeded = monitor.SucceededJobs(0, 10);
                if (succeeded.Any(j => j.Key == jobId))
                    return;

                await Task.Delay(1000);
            }

            throw new TimeoutException($"Job {jobId} did not complete in time.");
        }
    }
}
