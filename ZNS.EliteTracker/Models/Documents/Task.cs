using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ZNS.EliteTracker.Models.Documents
{
    public class Task : BaseDocument, ICommentable
    {
        public SolarSystemRef SolarSystem { get; set; }
        public CommanderRef Owner { get; set; }
        public List<CommanderRef> AssignedCommanders { get; set; }
        public TaskType Type { get; set; }
        public TaskStatus Status { get; set; }
        public TaskPriority Priority { get; set; }
        public string Heading { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }

        [Raven.Imports.Newtonsoft.Json.JsonIgnore]
        public string IdPrefix
        {
            get
            {
                return "Tasks";
            }
        }

        [Raven.Imports.Newtonsoft.Json.JsonIgnore]
        public string Name
        {
            get
            {
                return Heading;
            }
        }

        public Task() {
            SolarSystem = new SolarSystemRef();
            AssignedCommanders = new List<CommanderRef>();
        }
    }
}