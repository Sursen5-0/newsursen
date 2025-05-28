using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace Infrastructure.FlowCase.Models
{
    public class FlowcaseUserModel
    {
        [JsonPropertyName("user_id")]
        public string UserId { get; set; }

        [JsonPropertyName("default_cv_id")]
        public string DefaultCvId { get; set; }
    }
}
