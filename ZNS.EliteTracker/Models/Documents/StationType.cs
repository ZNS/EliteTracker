﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ZNS.EliteTracker.Models.Documents
{
    public enum StationType
    {
        Coriolis = 1,
        Orbis = 2,
        Ocellus = 3,
        Outpost = 4,
        [Display(Name = "Planetary Outpost")]
        PlanetaryOutpost = 5
    }
}