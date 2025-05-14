using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Infrastructure.Secrets
{
    public class SecretResponse
    {
        [JsonPropertyName("secrets")]
        public Dictionary<string, Secret> Secrets { get; set; }

        [JsonPropertyName("success")]
        public bool Success { get; set; }
    }
}
