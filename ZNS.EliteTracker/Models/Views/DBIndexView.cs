using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ZNS.EliteTracker.Models.EDDB;

namespace ZNS.EliteTracker.Models.Views
{
    public class DBIndexView
    {
        public class Form
        {
            public string System { get; set; }
            public int? Ly { get; set; }
        }

        public Form Query { get; set; }
        public List<EDDBSystem> Result { get; set; }

        public DBIndexView()
        {
            Query = new Form
            {
                System = ""
            };
        }
    }
}