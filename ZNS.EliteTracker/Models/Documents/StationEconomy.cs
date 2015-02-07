using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ZNS.EliteTracker.Models.Documents
{
    [Flags]
    public enum StationEconomy
    {
        Agriculture = 1,
        Refinery = 2,
        Extraction = 3,
        HighTech = 4,
        Terraforming = 5,
        Tourism = 6,
        Service = 7,
        Military = 8,
        Industrial = 9
    }
}