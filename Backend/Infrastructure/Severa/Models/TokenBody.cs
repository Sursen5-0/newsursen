using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Infrastructure.Severa.Models
{
    public class TokenBody
    {
        [JsonPropertyName("client_id")]
        public string ClientId { get; set; } = null!;
        [JsonPropertyName("client_secret")]
        public string ClientSecret { get; set; } = null!;
        [JsonPropertyName("scope")]
        public string Scope { get; set; } = null!;
    }
}
