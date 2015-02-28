using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ZNS.EliteTracker.Models.Documents
{
    public class TradeMilestone
    {
        public string Guid { get; set; }
        public StationRef From { get; set; }
        public StationRef To { get; set; }
        public CommodityType Commodity { get; set; }

        public double GetDistance()
        {
            if (From.System.HasCoordinates && To.System.HasCoordinates)
            {
                return Math.Sqrt(
                    Math.Pow(((double)From.System.Coordinates.X - (double)To.System.Coordinates.X), 2) +
                    Math.Pow(((double)From.System.Coordinates.Y - (double)To.System.Coordinates.Y), 2) +
                    Math.Pow(((double)From.System.Coordinates.Z - (double)To.System.Coordinates.Z), 2)
                    );
            }
            return 0;
        }

        public int GetProfit(Station from, Station to)
        {
            var comFrom = from.Commodities.FirstOrDefault(x => x.Type == Commodity);
            var comTo = to.Commodities.FirstOrDefault(x => x.Type == Commodity);
            if (comFrom != null && comTo != null && comFrom.Supply != CommodityAvailability.None && comTo.Demand != CommodityAvailability.None && comFrom.Price > 0 && comTo.Price > 0)
            {
                return Convert.ToInt32(comTo.Price - comFrom.Price);
            }
            return 0;
        }
    }
}