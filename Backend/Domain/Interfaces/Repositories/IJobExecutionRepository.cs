using Domain.Models;

namespace Domain.Interfaces.Repositories
{
    public interface IJobExecutionRepository
    {
        void InsertJobExecution(JobExecutionDTO job);
    }
}