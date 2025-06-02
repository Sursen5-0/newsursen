using Domain.Interfaces.Repositories;
using Domain.Models;
using Infrastructure.Persistance.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistance.Repositories
{
    public class JobExecutionRepository(SursenContext _db) : IJobExecutionRepository
    {
        public void InsertJobExecution(JobExecutionDTO job)
        {
            var jobExecution = new JobExecution
            {
                Id = Guid.NewGuid(),
                JobName = job.JobName,
                StartTimeDate = job.StartTimeDate,
                CompletionDate = job.CompletionDate,
                ErrorMessage = job.ErrorMessage,
                IsSuccess = job.IsSuccess
            };
            _db.JobExecutions.Add(jobExecution);
            _db.SaveChanges();
        }
    }
}
