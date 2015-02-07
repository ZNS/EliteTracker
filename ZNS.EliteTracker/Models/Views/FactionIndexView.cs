using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ZNS.EliteTracker.Models.Documents;

namespace ZNS.EliteTracker.Models.Views
{
    public class FactionIndexView
    {
        public List<Faction> Factions { get; set; }
        public Pager Pager { get; set; }
    }
}