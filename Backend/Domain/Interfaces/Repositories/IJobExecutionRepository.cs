using Domain.Models;

namespace Domain.Interfaces.Repositories
{
    public interface IJobExecutionRepository
    {
        /// <summary>
        /// Inserts a new job execution record into the database.
        /// </summary>
        /// <param name="job">A <see cref="JobExecutionDTO"/> object containing the job execution details to insert.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation.
        /// </returns>
        Task InsertJobExecution(JobExecutionDTO job);
        /// <summary>
        /// Retrieves the completion date of the latest successful job execution with the specified job name.
        /// </summary>
        /// <param name="name">The name of the job to search for. If null or empty, <c>null</c> is returned.</param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> representing the asynchronous operation, with the completion date of the latest successful job execution if found; otherwise, <c>null</c>.
        /// </returns>
        Task<DateTime?> GetLatestSuccessfulJobExecutionByName(string name);
    }
}