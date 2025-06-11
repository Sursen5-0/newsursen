using Microsoft.Extensions.DependencyInjection;
using Hangfire;
using Hangfire.Jobs;
using Infrastructure.Persistance;

namespace FlowTests
{
    public class SeveraFlowTests
    {
        private readonly IBackgroundJobClient _jobClient;
        private readonly JobStorage _jobStorage;
        private readonly ServiceProvider _provider;
        public SeveraFlowTests()
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
        public async Task SeveraHangfireJob_Synchronizes_Projects()
        {
            // Arrange 
            var scope = _provider.CreateScope();
            var severaJobs = scope.ServiceProvider.GetRequiredService<SeveraJobs>();

            // Act
            var jobId = _jobClient.Enqueue(() => severaJobs.SynchronizeProjects());
            await TestHelper.WaitForJobToComplete(_jobStorage, jobId);

            // Assert
            var db = scope.ServiceProvider.GetRequiredService<SursenContext>();
            Assert.NotEmpty(db.Projects);
        }

        [Fact]
        public async Task SeveraHangfireJob_Synchronizes_Phases()
        {
            // Arrange 
            var scope = _provider.CreateScope();
            var severaJobs = scope.ServiceProvider.GetRequiredService<SeveraJobs>();
            var jobId1 = _jobClient.Enqueue(() => severaJobs.SynchronizeProjects());
            await TestHelper.WaitForJobToComplete(_jobStorage, jobId1);
            // Act
            var jobId2 = _jobClient.Enqueue(() => severaJobs.SynchronizePhases());
            await TestHelper.WaitForJobToComplete(_jobStorage, jobId2);

            // Assert
            var db = scope.ServiceProvider.GetRequiredService<SursenContext>();
            var phases  = db.ProjectPhases.ToList();
            Assert.NotEmpty(phases);
            Assert.DoesNotContain(phases, x => x.ProjectId == Guid.Empty);
        }

        [Fact]
        public async Task SeveraHangfireJob_Synchronizes_Employees()
        {
            // Arrange 
            var scope = _provider.CreateScope();
            var severaJobs = scope.ServiceProvider.GetRequiredService<SeveraJobs>();
            var entraJobs = scope.ServiceProvider.GetRequiredService<EntraJobs>();
            var jobId1 = _jobClient.Enqueue(() => entraJobs.GetAllEmployeesEntra());
            await TestHelper.WaitForJobToComplete(_jobStorage, jobId1);
            // Act
            var jobId2 = _jobClient.Enqueue(() => severaJobs.SynchronizeEmployees());
            await TestHelper.WaitForJobToComplete(_jobStorage, jobId2);

            // Assert
            var db = scope.ServiceProvider.GetRequiredService<SursenContext>();
            var employees = db.Employees.ToList();
            Assert.NotEmpty(employees);
            Assert.Contains(employees, x => x.SeveraId != Guid.Empty);
        }
        [Fact]
        public async Task SeveraHangfireJob_Synchronizes_EmployeeContracts()
        {
            // Arrange 
            var scope = _provider.CreateScope();
            var severaJobs = scope.ServiceProvider.GetRequiredService<SeveraJobs>();
            var entraJobs = scope.ServiceProvider.GetRequiredService<EntraJobs>();

            var jobId1 = _jobClient.Enqueue(() => entraJobs.GetAllEmployeesEntra());
            await TestHelper.WaitForJobToComplete(_jobStorage, jobId1);
            var jobId2 = _jobClient.Enqueue(() => severaJobs.SynchronizeEmployees());
            await TestHelper.WaitForJobToComplete(_jobStorage, jobId2);

            // Act
            var jobId3 = _jobClient.Enqueue(() => severaJobs.SynchronizeContracts());
            await TestHelper.WaitForJobToComplete(_jobStorage, jobId3);

            // Assert
            var db = scope.ServiceProvider.GetRequiredService<SursenContext>();
            var employees = db.EmployeeContracts.ToList();
            Assert.NotEmpty(employees);
        }

        [Fact]
        public async Task SeveraHangfireJob_Synchronizes_EmployeeAbsence()
        {
            // Arrange 
            var scope = _provider.CreateScope();
            var severaJobs = scope.ServiceProvider.GetRequiredService<SeveraJobs>();
            var entraJobs = scope.ServiceProvider.GetRequiredService<EntraJobs>();

            var jobId1 = _jobClient.Enqueue(() => entraJobs.GetAllEmployeesEntra());
            await TestHelper.WaitForJobToComplete(_jobStorage, jobId1);
            var jobId2 = _jobClient.Enqueue(() => severaJobs.SynchronizeEmployees());
            await TestHelper.WaitForJobToComplete(_jobStorage, jobId2);

            // Act
            var jobId3 = _jobClient.Enqueue(() => severaJobs.SynchronizeAbsence());
            await TestHelper.WaitForJobToComplete(_jobStorage, jobId3);

            // Assert
            var db = scope.ServiceProvider.GetRequiredService<SursenContext>();
            var absence = db.Absences.ToList();
            Assert.NotEmpty(absence);
        }

    }
}
