using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ZNS.EliteTracker.Models.Documents
{
    public enum TaskType
    {
        Influence = 11,
        Assassination = 1,
        Combat = 7,
        Exploring = 4,
        Infiltration = 5,
        Recon = 3,
        Trading  = 2,
        Training = 8,
        Mining = 9,
        [Display(Name = "Power Play")]
        PowerPlay = 10,
        Other = 6
    }
}