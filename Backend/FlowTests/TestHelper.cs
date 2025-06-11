using Microsoft.Extensions.DependencyInjection;
using Hangfire;
using Application;
using Infrastructure;
using System.Diagnostics;
using Serilog;
using Hangfire.MemoryStorage;

namespace FlowTests
{
    internal static class TestHelper
    {
        public static async Task WaitForJobToComplete(JobStorage jobstorage, string jobId, int timeoutSeconds = 600)
        {
            var monitor = jobstorage.GetMonitoringApi();
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
        public static ServiceCollection GetServiceCollection()
        {
            var token = Environment.GetEnvironmentVariable("DOPPLER_KEY");
            var environment = Environment.GetEnvironmentVariable("ENVIRONMENT");

            var services = new ServiceCollection();

            services.AddSerilog();
            services.AddHangfire((provider, config) =>
            {
                config.UseMemoryStorage();
                config.UseSerilogLogProvider();
                config.UseActivator(new TestJobActivator(provider));
            });
            services.RegisterInfrastructureServices(token, environment, true);
            services.RegisterApplication();
            services.RegisterJobs();
            return services;
        }
    }
}
