using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Infrastructure.Severa.Models
{
    public class SeveraProjectModel
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;
        [JsonPropertyName("description")]
        public string? Description { get; set; }
        [JsonPropertyName("guid")]
        public Guid ExternalId { get; set; }
        [JsonPropertyName("isClosed")]
        public bool IsClosed { get; set; }
        [JsonPropertyName("projectOwner")]
        public SimpleReferenceModel User { get; set; }
        public Guid OwnerId => User.Id;

    }
}
