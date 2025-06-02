using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class ProjectDTO
    {
        public Guid Id { get; set; }
        public required string Name { get; set; } = null!;
        public string? Description { get; set; } = null!;
        public bool IsClosed { get; set; }
        public Guid ExternalId { get; set; }
        public Guid ExternalOwnerId { get; set; }
        public Guid? OwnerId { get; set; }
    }
}
