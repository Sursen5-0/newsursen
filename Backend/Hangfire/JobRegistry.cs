using Hangfire.Jobs;

namespace Hangfire
{
    public class JobRegistry(SeveraJobs severa, EntraJobs entra, FlowCaseJobs flowcase)
    {
        public void RegisterJobs()
        {
            // Register recurring jobs here
            RecurringJob.AddOrUpdate(
                "SynchronizeEmployees",
                () => severa.SynchronizeEmployees(), "0 0 31 2 *");
            RecurringJob.AddOrUpdate(
                "SynchronizeContracts",
                () => severa.SynchronizeContracts(), "0 0 31 2 *");
            RecurringJob.AddOrUpdate(
                "SynchronizeAbsence",
                () => severa.SynchronizeAbsence(), "0 0 31 2 *");
            RecurringJob.AddOrUpdate(
                "SynchronizeProjects",
                () => severa.SynchronizeProjects(), "0 0 31 2 *");
            RecurringJob.AddOrUpdate(
                "SynchronizePhases",
                () => severa.SynchronizePhases(), "0 0 31 2 *");
            RecurringJob.AddOrUpdate(
                "SynchronizeEntraEmployees",
                () => entra.GetAllEmployeesEntra(), "0 0 31 2 *");
            RecurringJob.AddOrUpdate(
                "SynchronizeSkillsToSkillsTable",
                () => flowcase.SynchronizeSkillsToSkillsTable(), "0 0 31 2 *");
            RecurringJob.AddOrUpdate(
                "SynchronizeEmployeesWithFlowcaseIdsAsync",
                () => flowcase.SynchronizeEmployeesWithFlowcaseIdsAsync(), "0 0 31 2 *");
            RecurringJob.AddOrUpdate(
                "SynchronizeEmployeeSkillsAsync",
                () => flowcase.SynchronizeEmployeeSkillsAsync(), "0 0 31 2 *");
        }
    }
}
