using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ZNS.EliteTracker.Models.Documents
{
    public enum Power
    {
        Independent = 0,
        Federation = 1,
        Empire = 2,
        Alliance = 3,
        [Display(Name = "Pilots Federation")]
        Pilots_Federation = 4,
        Pirate = 5
    }
}