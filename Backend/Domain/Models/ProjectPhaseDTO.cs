using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class ProjectPhaseDTO
    {
        public Guid? Id { get; set; }
        public required string Name { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? DeadLine { get; set; }
        public Guid ProjectId { get; set; }
        public Guid ExternalProjectId { get; set; }
        public Guid? ExternalParentPhaseId { get; set; }
        public Guid? ParentPhaseId { get; set; }
        public Guid ExternalId { get; set; }
    }
}
