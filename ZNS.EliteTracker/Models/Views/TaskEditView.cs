using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ZNS.EliteTracker.Models.Documents;

namespace ZNS.EliteTracker.Models.Views
{
    public class TaskEditView
    {
        public Task Task { get; set; }
        public List<SolarSystem> Systems { get; set; }

        public TaskEditView()
        {
            Task = new Task
            {
                Priority = TaskPriority.Normal,
                Status = TaskStatus.New,
                Type = TaskType.Other
            };
            Systems = new List<SolarSystem>();
        }
    }
}