using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ZNS.EliteTracker.Models.Documents;

namespace ZNS.EliteTracker.Models.Views
{
    public class TradeEditView
    {
        public List<SolarSystem> SolarSystems { get; set; }
        public TradeRoute Route { get; set; }

        public TradeEditView()
        {
            Route = new TradeRoute();
        }
    }
}