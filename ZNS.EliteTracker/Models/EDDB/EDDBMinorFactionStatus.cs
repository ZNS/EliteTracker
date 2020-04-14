using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace ZNS.EliteTracker.Models.EDDB
{
    public class EDDBMinorFactionStatus
    {
        [JsonProperty(PropertyName = "minor_faction_id")]
        public int Id { get; set; }
        public double? Influence { get; set; }
        public string State { get; set; }
    }
}