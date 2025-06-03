using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class EmployeeSkillDTO
    {
        public Guid EmployeeId { get; set; }
        public Guid SkillId { get; set; }
        public byte YearsOfExperience { get; set; }
    }
}
