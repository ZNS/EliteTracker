using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ZNS.EliteTracker.Models.Documents
{
    public class Station
    {
        public string Guid { get; set; }
        public FactionRef Faction { get; set; }
        public string Name { get; set; }
        public List<StationEconomy> Economy { get; set; }
        public StationType Type { get; set; }
        public int Distance { get; set; }
        public bool Main { get; set; }
    }
}