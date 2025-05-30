using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Infrastructure.Severa.Models
{
    public class SeveraPhaseModel
    {
        [JsonPropertyName("name")]
        public required string Name { get; set; }
        [JsonPropertyName("startDate")]
        public DateTime? StartDate { get; set; }
        [JsonPropertyName("deadline")]
        public DateTime? DeadLine { get; set; }
        [JsonPropertyName("guid")]
        public Guid ExternalId { get; set; }
        [JsonPropertyName("parentPhase")]
        public SimpleReferenceModel? ParentPhase { get; set; }
        [JsonPropertyName("project")]
        public required SimpleReferenceModel Project { get; set; }
        public Guid ExternalProjectId => Project.Id;
        public Guid? ParentPhaseId => ParentPhase?.Id;
    }
}
