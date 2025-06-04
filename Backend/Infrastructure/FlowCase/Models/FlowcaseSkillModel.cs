using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Infrastructure.FlowCase.Models
{
    public class FlowcaseSkillModel
    {
        [JsonPropertyName("_id")]
        public string SkillId { get; set; } = null!;

        [JsonPropertyName("tags")]
        public FlowcaseSkillName Tags { get; set; } = new();

        [JsonPropertyName("values")]
        public FlowcaseSkillName Values { get; set; } = new();

        [JsonPropertyName("proficiency")]
        public byte Proficiency { get; set; }

        [JsonPropertyName("total_duration_in_years")]
        public byte TotalDurationInYears { get; set; }

        [JsonPropertyName("technologies")]
        public List<TechnologyModel> Technologies { get; set; } = new();
    }
    public class TechnologyModel

    {
        [JsonPropertyName("technology_skills")]
        public List<FlowcaseSkillModel> TechnologySkills { get; set; } = new();
    }

    /// <summary>
    /// Used when taking name from masterdata endpoint
    /// </summary>
    public class FlowcaseSkillName
    {
        [JsonPropertyName("dk")]
        public string? Dk { get; set; }

        [JsonPropertyName("int")]
        public string? En { get; set; }

        public string? Name
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(Dk))
                {
                    return Dk;
                }
                else if (!string.IsNullOrWhiteSpace(En))
                {
                    return En;
                }
                return null;
            }
        }
    }

}