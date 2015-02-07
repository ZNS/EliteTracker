using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ZNS.EliteTracker.Models.Documents
{
    public enum TaskStatus
    {
        New = 1,
        InProgress = 2,
        Completed = 3,
        OnHold = 4
    }
}