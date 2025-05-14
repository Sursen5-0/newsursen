using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Infrastructure.Secrets
{
    public class Secret
    {
        [JsonPropertyName("raw")]
        public required string Value { get; set; }

    }
}
