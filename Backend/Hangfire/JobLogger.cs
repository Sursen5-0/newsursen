using Domain.Models;
using System.Reflection;

namespace Hangfire
{
    public static class JobLogger
    {
        public static async Task<JobExecutionDTO> LogJobExecutionAsync(string jobName, Task action)
        {
            var jobExecution = new JobExecutionDTO()
            {
                JobName = MethodBase.GetCurrentMethod()?.Name ?? "Unknown method",
                StartTimeDate = DateTime.UtcNow,
            };
            try
            {
                await action;
                jobExecution.IsSuccess = true;
            }
            catch (Exception ex)
            {
                jobExecution.ErrorMessage = ex.Message;
                jobExecution.IsSuccess = false;
                throw;
            }
            jobExecution.CompletionDate = DateTime.UtcNow;
            return jobExecution;

        }
    }
}
