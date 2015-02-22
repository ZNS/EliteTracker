using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Raven.Client.Indexes;
using ZNS.EliteTracker.Models.Documents;

namespace ZNS.EliteTracker.Models.Indexes
{
    public class Influence_Report : AbstractIndexCreationTask<SolarSystemStatus, ZNS.EliteTracker.Models.Indexes.Influence_Report.Result>
    {
        public class Result
        {
            public int SolarSystemId { get; set; }
        }

        public Influence_Report()
        {
            Map = statuses => from status in statuses
                              from faction in status.FactionStatus
                              where status.Date >= status.Date.AddDays(-7) && faction.Faction.Attitude == FactionAttitude.Ally
                              group faction by new { SolarSystemId = status.SolarSystem, FactionId = faction.Faction.Id } into g
                              select new
                              {
                                  SolarSystemId = g.Key.SolarSystemId,
                                  FactionId = g.Key.FactionId,
                                  InfluenceChange = g.Max(x => x.Influence) - g.Min(x => x.Influence)
                              };
        }
    }
}