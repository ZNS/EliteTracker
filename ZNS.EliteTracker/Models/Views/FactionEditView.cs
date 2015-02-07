using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ZNS.EliteTracker.Models.Documents;

namespace ZNS.EliteTracker.Models.Views
{
    public class FactionEditView
    {
        public Faction Faction { get; set; }
        public List<SolarSystem> Systems { get; set; }
        public List<string> PostedSystems { get; set; }

        public FactionEditView()
        {
            Faction = new Faction();
            Systems = new List<SolarSystem>();
        }
    }
}