using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ZNS.EliteTracker.Models.Documents
{
    public class Commodity
    {
        public CommodityType Type { get; set; }
        public CommodityAvailability Demand { get; set; }
        public CommodityAvailability Supply { get; set; }
        public double Price { get; set; }

        public Commodity()
        {
            Demand = CommodityAvailability.None;
            Supply = CommodityAvailability.None;
        }
    }
}