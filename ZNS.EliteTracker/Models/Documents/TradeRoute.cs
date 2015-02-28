using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ZNS.EliteTracker.Models.Documents
{
    public class TradeRoute : BaseDocument, ICommentable
    {
        public string Name { get; set; }
        public List<TradeMilestone> Milestones { get; set; }
        public string Notes { get; set; }

        public string IdPrefix
        {
            get { return "TradeRoutes"; }
        }

        public TradeRoute()
        {
            Milestones = new List<TradeMilestone>();
        }
    }
}