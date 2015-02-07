using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ZNS.EliteTracker.Models.Documents
{
    public class FactionStatus
    {
        public FactionRef Faction { get; set; }
        public double Influence { get; set; }
        public FactionState State { get; set; }
        public List<FactionState> PendingStates { get; set; }
    }
}