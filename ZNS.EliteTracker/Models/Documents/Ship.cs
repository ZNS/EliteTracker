using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ZNS.EliteTracker.Models.Documents
{
    public class Ship
    {
        public string Guid { get; set; }
        public ShipModel Model { get; set; }
        public string Name { get; set; }
        public ShipFittings Fittings { get; set; }
    }
}