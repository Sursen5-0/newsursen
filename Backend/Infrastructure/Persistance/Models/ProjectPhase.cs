using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistance.Models
{
    public partial class ProjectPhase : BaseEntity
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? DeadLine{ get; set; }
        public Guid ProjectId { get; set; }
        public Guid ExternalId { get; set; }
        public Guid? ParentPhaseId { get; set; }
        public virtual Project Project { get; set; } = null!;
        public virtual ProjectPhase? ParentPhase { get; set; }
        public virtual ICollection<ProjectPhase> UnderPhases { get; set; } = new List<ProjectPhase>();
    }
}
