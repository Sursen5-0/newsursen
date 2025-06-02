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
                JobName = jobName ?? "Unknown method",
                StartTimeDate = DateTime.UtcNow,
            };
            try
            {
                await action;
                jobExecution.IsSuccess = true;
            }
            catch (Exception ex)
            {
                jobExecution.ErrorMessage = ex.Message.ToString();
                jobExecution.IsSuccess = false;
                jobExecution.Exception = ex;
            }
            finally
            {
                jobExecution.CompletionDate = DateTime.UtcNow;
            }
            return jobExecution;

        }
        public static void ThrowIfFailed(JobExecutionDTO jobExecution)
        {
            if (!jobExecution.IsSuccess && jobExecution.Exception != null)
            { 
                throw jobExecution.Exception;
            }
        }
    }
}
