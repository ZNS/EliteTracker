﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ZNS.EliteTracker.Models.Documents
{
    public enum PowerPlayState
    {
        None = 1,
        Control = 2,
        Exploited = 3,
        Expansion = 4
    }
}