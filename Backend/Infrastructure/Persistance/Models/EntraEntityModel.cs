using System;
using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace Infrastructure.Persistance.Models
{
    internal class EntraEntityModel
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("givenName")]
        public string? FirstName { get; set; }

        [JsonPropertyName("surname")]
        public string? LastName { get; set; }

        [JsonPropertyName("mail")]
        public string? Email { get; set; }

        [JsonPropertyName("employeeHireDate")]
        public DateTime? HireDate { get; set; }

        [JsonPropertyName("employeeLeaveDateTime")]
        public DateTime? LeaveDate { get; set; }

        [JsonPropertyName("businessPhones")]
        public List<string>? BusinessPhones { get; set; }

        [JsonPropertyName("mobilePhone")]
        public string? PersonalPhoneNumber { get; set; }


        [JsonPropertyName("manager")]
        public EntraManager? Manager { get; set; }

        internal class EntraManager
        {
            [JsonPropertyName("id")]
            public Guid? Id { get; set; }

            [JsonPropertyName("displayName")]
            public string? DisplayName { get; set; }
        }

    }
}
