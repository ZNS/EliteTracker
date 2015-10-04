using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ZNS.EliteTracker.Models.Documents;

namespace ZNS.EliteTracker.Models.Views
{
    public class FactionIndexView
    {
        public class Form
        {
            public int State { get; set; }
            public int Attitude { get; set; }
            public string Query { get; set; }
        }

        public List<Faction> Factions { get; set; }
        public Pager Pager { get; set; }
        public Form Query { get; set; }

        public FactionIndexView()
        {
            Query = new Form();
        }
    }
}