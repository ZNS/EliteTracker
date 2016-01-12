using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ZNS.EliteTracker.Models.Documents
{
    public enum TaskStatus
    {
        New = 1,
        [Display(Name = "In Progress")]
        InProgress = 2,
        Completed = 3,
        [Display(Name = "On Hold")]
        OnHold = 4
    }
}