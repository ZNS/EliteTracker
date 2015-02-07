using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ZNS.EliteTracker.Models.Views
{
    public class LoginView
    {
        public string CommanderName { get; set; }
        public string Password { get; set; }
        public bool Persist { get; set; }
    }
}