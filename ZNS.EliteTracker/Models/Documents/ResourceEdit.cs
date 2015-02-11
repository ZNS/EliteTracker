using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ZNS.EliteTracker.Models.Documents
{
    public class ResourceEdit
    {
        public CommanderRef Commander { get; set; }
        public string Comment { get; set; }
        public DateTime Date { get; set; }
    }
}