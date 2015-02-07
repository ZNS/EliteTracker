using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ZNS.EliteTracker.Models.Documents;

namespace ZNS.EliteTracker.Models.Views
{
    public class CommanderViewView
    {
        public Commander Commander { get; set; }
        public List<Task> Tasks { get; set; }
        public List<SolarSystem> SolarSystems { get; set; }
    }
}