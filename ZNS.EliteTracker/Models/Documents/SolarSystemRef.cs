using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ZNS.EliteTracker.Models.Documents
{
    public class SolarSystemRef
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public static SolarSystemRef FromSolarSystem(SolarSystem system)
        {
            return new SolarSystemRef
            {
                Id = system.Id,
                Name = system.Name
            };
        }
    }
}