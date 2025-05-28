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
        public string SkillId { get; set; } = string.Empty;
        public string SkillName { get; set; } = string.Empty;
        public byte SkillProficiency { get; set; } = 0;
        public byte SkillTotalDurationInYears { get; set; } = 0;
        public byte SkillBaseDurationInYears { get; set; } = 0;
        public byte SkillOffsetDurationInYears { get; set; } = 0;
    }
}
