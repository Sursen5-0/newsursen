using Hangfire.Jobs;
using Hangfire;
using Infrastructure.Persistance;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowTests
{
    public class FlowcaseFlowTests
    {
        private readonly IBackgroundJobClient _jobClient;
        private readonly JobStorage _jobStorage;
        private readonly ServiceProvider _provider;
        public FlowcaseFlowTests()
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
        public async Task FlowcaseHangfireJob_Synchronizes_Skills()
        {
            // Arrange 
            var scope = _provider.CreateScope();
            var flowcaseJobs = scope.ServiceProvider.GetRequiredService<FlowCaseJobs>();

            // Act
            var jobId = _jobClient.Enqueue(() => flowcaseJobs.SynchronizeSkillsToSkillsTable());
            await TestHelper.WaitForJobToComplete(_jobStorage, jobId);

            // Assert
            var db = scope.ServiceProvider.GetRequiredService<SursenContext>();
            Assert.NotEmpty(db.Skills);
        }

        [Fact]
        public async Task FlowcaseHangfireJob_Synchronizes_EmployeeSkills()
        {
            // Arrange 
            var scope = _provider.CreateScope();
            var flowcaseJobs = scope.ServiceProvider.GetRequiredService<FlowCaseJobs>();
            var entraJobs = scope.ServiceProvider.GetRequiredService<EntraJobs>();
            var jobId1 = _jobClient.Enqueue(() => entraJobs.GetAllEmployeesEntra());
            await TestHelper.WaitForJobToComplete(_jobStorage, jobId1);
            var jobId2 = _jobClient.Enqueue(() => flowcaseJobs.SynchronizeEmployeesWithFlowcaseIdsAsync());
            await TestHelper.WaitForJobToComplete(_jobStorage, jobId2);
            var jobId3 = _jobClient.Enqueue(() => flowcaseJobs.SynchronizeSkillsToSkillsTable());
            await TestHelper.WaitForJobToComplete(_jobStorage, jobId3);

            // Act
            var jobId4 = _jobClient.Enqueue(() => flowcaseJobs.SynchronizeEmployeeSkillsAsync());
            await TestHelper.WaitForJobToComplete(_jobStorage, jobId4);

            // Assert
            var db = scope.ServiceProvider.GetRequiredService<SursenContext>();
            var employeeSkills = db.EmployeeSkills.ToList();
            Assert.NotEmpty(employeeSkills);
        }
        [Fact]
        public async Task FlowcaseHangfireJob_Synchronizes_Employees()
        {
            // Arrange 
            var scope = _provider.CreateScope();
            var flowcaseJobs = scope.ServiceProvider.GetRequiredService<FlowCaseJobs>();
            var entraJobs = scope.ServiceProvider.GetRequiredService<EntraJobs>();
            var jobId1 = _jobClient.Enqueue(() => entraJobs.GetAllEmployeesEntra());
            await TestHelper.WaitForJobToComplete(_jobStorage, jobId1);

            // Act
            var jobId2 = _jobClient.Enqueue(() => flowcaseJobs.SynchronizeEmployeesWithFlowcaseIdsAsync());
            await TestHelper.WaitForJobToComplete(_jobStorage, jobId2);

            // Assert
            var db = scope.ServiceProvider.GetRequiredService<SursenContext>();
            var employees = db.Employees.ToList();
            Assert.NotEmpty(employees);
            Assert.Contains(employees, x => !string.IsNullOrEmpty(x.FlowCaseId));
        }



    }
}
