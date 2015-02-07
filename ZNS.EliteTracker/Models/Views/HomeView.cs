using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ZNS.EliteTracker.Models.Documents;

namespace ZNS.EliteTracker.Models.Views
{
    public class HomeView
    {
        public List<SolarSystem> SolarSystems { get; set; }
        public List<Comment> Comments { get; set; }
        public List<Task> NewTasks { get; set; }
        public List<Task> MyTasks { get; set; }
    }
}