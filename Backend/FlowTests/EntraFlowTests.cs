using Hangfire;
using Hangfire.Jobs;
using Infrastructure.Persistance;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowTests
{

    public class EntraFlowTests
    {
        private readonly IBackgroundJobClient _jobClient;
        private readonly JobStorage _jobStorage;
        private readonly ServiceProvider _provider;
        public EntraFlowTests()
        {
            var services = TestHelper.GetServiceCollection();
            _provider = services.BuildServiceProvider();
            _jobStorage = _provider.GetRequiredService<JobStorage>();

            _jobClient = _provider.GetRequiredService<IBackgroundJobClient>();
            var _server = new BackgroundJobServer(new BackgroundJobServerOptions
            {
                WorkerCount = 1
            }, _jobStorage);

        }
        [Fact]
        public async Task EntraHangfireJobs_Synchronizes_Employees()
        {
            // Arrange 
            var scope = _provider.CreateScope();
            var entraJobs = scope.ServiceProvider.GetRequiredService<EntraJobs>();

            // Act
            var jobId = _jobClient.Enqueue(() => entraJobs.GetAllEmployeesEntra());
            await TestHelper.WaitForJobToComplete(_jobStorage, jobId);

            // Assert
            var db = scope.ServiceProvider.GetRequiredService<SursenContext>();
            Assert.NotEmpty(db.Employees);
        }

    }
}
