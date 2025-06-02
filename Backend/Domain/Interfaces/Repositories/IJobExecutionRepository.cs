using Domain.Models;

namespace Domain.Interfaces.Repositories
{
    public interface IJobExecutionRepository
    {
        Task InsertJobExecution(JobExecutionDTO job);
        Task<DateTime?> GetLatestSuccessfulJobExecutionByName(string name);
    }
}