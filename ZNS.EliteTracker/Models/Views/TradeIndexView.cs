using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ZNS.EliteTracker.Models.Documents;

namespace ZNS.EliteTracker.Models.Views
{
    public class TradeIndexView
    {
        public class Form
        {
            public int Supply { get; set; }
            public int Demand { get; set; }
        }

        public List<TradeRoute> Routes { get; set; }
        public Pager Pager { get; set; }
        public Form Query { get; set; }

        public TradeIndexView()
        {
        }
    }
}