using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Infrastructure.Severa.Models
{
    public class SeveraActivityModel
    {
        public Guid Id
        {
            get
            {
                if (Guid.TryParse(Identifier, out var result))
                {
                    return result;
                }
                // If multiple IDs are present (fx "guid1;guid2"), parse the last one. since the first might be the id of a reccuring id from severa,
                // and the last one is the unique id for the activity.

                else
                {
                    var splitGuid = Identifier.Split(';');

                    if (splitGuid.Length > 0 && Guid.TryParse(splitGuid.Last(), out result))
                    {
                        return result;
                    }
                }


                throw new Exception($"unable to parse the Id of absence from this string: '{Identifier}'");
            }
        }
        [JsonPropertyName("guid")]
        public required string Identifier { get; set; }
        [JsonPropertyName("startDateTime")]
        public DateTime FromDate { get; set; }
        [JsonPropertyName("endDateTime")]
        public DateTime ToDate { get; set; }
        public string? Type => ActivityType?.Name ?? null;
        public Guid? SeveraEmployeeId => Owner?.Id ?? null;

        //Only used for mapping
        [JsonPropertyName("activityType")]
        public required ActivityType ActivityType { get; set; }
        [JsonPropertyName("ownerUser")]
        public required SimpleUserModel Owner { get; set; }

    }
    public class ActivityType
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;
    }
}
