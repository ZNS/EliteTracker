using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ZNS.EliteTracker.Models.Documents
{
    public class SolarSystemStatus : BaseDocument
    {
        public int SolarSystem { get; set; }
        public List<FactionStatus> FactionStatus {get; set;}
        public DateTime Date { get; set; }
    }
}