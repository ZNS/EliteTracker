using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ZNS.EliteTracker.Models.Documents;

namespace ZNS.EliteTracker.Models.Views
{
    public class SolarSystemIndexView
    {
        public class Form
        {
            public string Query { get; set; }
            public int Economy { get; set; }
            public int Status { get; set; }
            public int Supply { get; set; }
            public int Demand { get; set; }
            public int PowerPlayState { get; set; }
            public int PowerPlayLeader { get; set; }
            public int Outfitting { get; set; }
        }

        public List<SolarSystem> SolarSystems { get; set; }
        public Pager Pager { get; set; }
        public Form Query { get; set; }
        public List<SelectListItem> Statuses { get; set; }

        public SolarSystemIndexView()
        {
            Query = new Form
            {
                Query = ""
            };
        }
    }
}