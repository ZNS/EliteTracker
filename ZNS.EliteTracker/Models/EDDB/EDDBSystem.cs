using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using ZNS.EliteTracker.Models.Documents;
using ZNS.EliteTracker.Models.Json;

namespace ZNS.EliteTracker.Models.EDDB
{
    public class EDDBSystem
    {
        public int Id { get; set; }
        public int Id_EDSM { get; set; }
        public string Name { get; set; }
        public float? X { get; set; }
        public float? Y { get; set; }
        public float? Z { get; set; }
        public string Faction { get; set; }
        public long? Population { get; set; }
        public string Government { get; set; }
        public string Allegiance { get; set; }
        public string State { get; set; }
        public string Security { get; set; }
        public string Primary_Economy { get; set; }
        public string Power { get; set; }
        public string Power_State { get; set; }
        public bool? Needs_Permit { get; set; }
        [JsonConverter(typeof(TimestampJsonConverter))]
        public DateTime Updated_At { get; set; }
        public int CCIncome { get; set; }
        [JsonIgnore]
        public double Distance { get; set; }
    }
}