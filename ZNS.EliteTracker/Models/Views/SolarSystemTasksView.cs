﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ZNS.EliteTracker.Models.Documents;

namespace ZNS.EliteTracker.Models.Views
{
    public class SolarSystemTasksView
    {
        public SolarSystem SolarSystem { get; set; }
        public List<Task> Tasks { get; set; }
        public Pager Pager { get; set; }
    }
}