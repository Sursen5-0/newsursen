using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class JobExecutionDTO
    {
        public Guid Id { get; set; }
        public string JobName { get; set; } = null!;
        public DateTime StartTimeDate { get; set; }
        public DateTime CompletionDate { get; set; }
        public string? ErrorMessage { get; set; }
        public bool IsSuccess { get; set; }
    }
}
