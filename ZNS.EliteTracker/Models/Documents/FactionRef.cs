using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ZNS.EliteTracker.Models.Indexes;

namespace ZNS.EliteTracker.Models.Documents
{
    public class FactionRef
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public FactionAttitude Attitude { get; set; }

        public static FactionRef FromFaction(Faction faction)
        {
            if (faction == null)
                return null;
            return new FactionRef
            {
                Id = faction.Id,
                Name = faction.Name,
                Attitude = faction.Attitude
            };
        }
    }
}