using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ZNS.EliteTracker.Models.Documents
{
    public class Faction : BaseDocument
    {
        public SolarSystemRef HomeSolarSystem { get; set; }
        public List<SolarSystemRef> SolarSystems { get; set; }
        public string Name { get; set; }
        public FactionGovernment Government { get; set; }
        public Power Allegiance { get; set; }
        public FactionState State { get; set; }
        public List<FactionState> PendingStates { get; set; }
        public FactionAttitude Attitude { get; set; }
        public int EDDB_Id { get; set; }

        public Faction()
        {
            SolarSystems = new List<SolarSystemRef>();
            State = FactionState.None;
        }
    }
}