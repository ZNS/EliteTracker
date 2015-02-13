using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ZNS.EliteTracker.Models.Documents;

namespace ZNS.EliteTracker.Models.Views
{
    public class SolarSystemDistanceView
    {
        public class Form
        {
            public int Supply { get; set; }
            public int Demand { get; set; }
        }

        public SolarSystem SolarSystem { get; set;}
        public List<SolarSystem> Systems { get; set; }
        public Form Query { get; set; }
    }
}