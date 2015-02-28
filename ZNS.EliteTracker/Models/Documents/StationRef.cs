using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ZNS.EliteTracker.Models.Documents
{
    public class StationRef
    {
        public SolarSystemRef SolarSystem { get; set; }
        public string StationGuid { get; set; }

        [Raven.Imports.Newtonsoft.Json.JsonIgnore]
        public SolarSystem System { get; set; }

        public Station GetStation()
        {
            return System.Stations.FirstOrDefault(x => x.Guid == StationGuid);
        }
    }
}