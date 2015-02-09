using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ZNS.EliteTracker.Models.Documents;

namespace ZNS.EliteTracker.Models.Views
{
    public class SolarSystemDistanceView
    {
        public SolarSystem SolarSystem { get; set;}
        public List<SolarSystem> Systems { get; set; }
    }
}