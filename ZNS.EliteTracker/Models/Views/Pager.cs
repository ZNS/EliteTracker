using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ZNS.EliteTracker.Models.Views
{
    public class Pager
    {
        public int Count { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}