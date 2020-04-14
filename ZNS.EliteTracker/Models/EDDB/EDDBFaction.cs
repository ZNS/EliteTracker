using System;
using Newtonsoft.Json;
using ZNS.EliteTracker.Models.Json;

namespace ZNS.EliteTracker.Models.EDDB
{
    public class EDDBFaction
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string State { get; set; }
        public string Allegiance { get; set; }
        public string Government { get; set; }
        [JsonConverter(typeof(TimestampJsonConverter))]
        public DateTime Updated_At { get; set; }
    }
}