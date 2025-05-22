using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Infrastructure.Severa.Models
{
    public class SeveraWorkContract
    {
        [JsonPropertyName("guid")]
        public Guid Id { get; set; }
        [JsonPropertyName("title")]
        public required string Title { get; set; }
        [JsonPropertyName("startDate")]
        public DateTime StartDate { get; set; }
        [JsonPropertyName("endDate")]
        public DateTime? EndDate { get; set; }
        [JsonPropertyName("dailyHours")]
        public decimal DailyHours { get; set; }
        [JsonPropertyName("workWeek")]
        public string[]? WorkDays { get; set; }
    }
}
