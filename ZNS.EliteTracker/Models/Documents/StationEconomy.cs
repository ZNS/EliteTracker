using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ZNS.EliteTracker.Models.Documents
{
    public enum StationEconomy
    {
        Agriculture = 1,
        Refinery = 2,
        Extraction = 3,
        [Display(Name = "High tech")]
        High_Tech = 4,
        Terraforming = 5,
        Tourism = 6,
        Service = 7,
        Military = 8,
        Industrial = 9
    }
}