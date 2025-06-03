using Domain.Interfaces.Repositories;
using Domain.Models;
using Infrastructure.Persistance.Models;
using Microsoft.EntityFrameworkCore;
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
        public async Task<DateTime?> GetLatestSuccessfulJobExecutionByName(string name)
        {
            if(string.IsNullOrEmpty(name))
            {
                return null;
            }
            var jobExecution = await _db.JobExecutions.Where(x => x.JobName == name && x.IsSuccess)
                .OrderByDescending(x => x.CompletionDate)
                .FirstOrDefaultAsync();

            if(jobExecution == null)
            {
                return null;
            }
            else
            {
                return jobExecution.CompletionDate;
            }

        }

        public async Task InsertJobExecution(JobExecutionDTO job)
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
            await _db.SaveChangesAsync();
        }
    }
}
