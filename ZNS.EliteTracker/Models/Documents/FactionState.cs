using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ZNS.EliteTracker.Models.Documents
{
    public enum FactionState
    {
        None = 1,
        EconomicBoom = 2,
        Expansion = 3,
        Lockdown = 4,
        CivilUnrest = 5,
        CivilWar = 6,
        War = 7,
        Outbreak = 8
    }
}