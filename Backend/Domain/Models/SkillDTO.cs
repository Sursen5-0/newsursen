using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class SkillDTO
    {
        public Guid Id { get; set; }
        public string ExternalId { get; set; } = null!;
        public string SkillName { get; set; } = null!;
    }
}
