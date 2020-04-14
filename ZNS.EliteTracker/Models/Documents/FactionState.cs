using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ZNS.EliteTracker.Models.Documents
{
    public enum FactionState
    {
        None = 1,
        Boom = 2,
        Expansion = 3,
        Lockdown = 4,
        [Display(Name = "Civil Unrest")]
        Civil_Unrest = 5,
        [Display(Name = "Civil War")]
        Civil_War = 6,
        War = 7,
        Outbreak = 8,
        Election = 9,
        Bust = 10,
        Retreat = 11,
        Investment = 12,
        Famine = 13
    }
}