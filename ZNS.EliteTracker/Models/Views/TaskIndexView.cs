using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ZNS.EliteTracker.Models.Documents;

namespace ZNS.EliteTracker.Models.Views
{
    public class TaskIndexView
    {
        public class Form
        {
            public int Status { get; set; }
            public int Type { get; set; }
        }

        public List<Task> Tasks { get; set; }
        public Pager Pager { get; set; }
        public Form Query { get; set; }
        public List<SelectListItem> Statuses { get; set; }
    }
}