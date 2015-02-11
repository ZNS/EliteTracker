using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Raven.Client.Indexes;
using ZNS.EliteTracker.Models.Documents;

namespace ZNS.EliteTracker.Models.Indexes
{
    public class Faction_State : AbstractIndexCreationTask<SolarSystemStatus, ZNS.EliteTracker.Models.Indexes.Faction_State.Result>
    {
        public class Result
        {
            public int SolarSystemId { get; set; }
        }

        public Faction_State()
        {
            Map = statuses => from status in statuses
                              group status by status.SolarSystem into g
                              let system = LoadDocument<SolarSystem>("SolarSystems/" + g.Key)
                              let last = g.OrderBy(x => x.Date).Last()
                              let controllingStation = system.Stations.FirstOrDefault(x => x.Main)
                              let alliedStatus = last.FactionStatus.FirstOrDefault(x => x.Faction.Attitude == FactionAttitude.Ally)
                              let controllingStatus = controllingStation != null ? last.FactionStatus.FirstOrDefault(x => x.Faction.Id == controllingStation.Faction.Id) : null
                              select new
                              {
                                  SolarSystemId = system.Id,
                                  SolarSystemName = system.Name,
                                  SolarSystemPopulation = system.Population,
                                  AlliedId = alliedStatus != null ? alliedStatus.Faction.Id : 0,
                                  AlliedName = alliedStatus != null ? alliedStatus.Faction.Name : "",
                                  AlliedInfluence = alliedStatus != null ? alliedStatus.Influence : 0,
                                  AlliedState = alliedStatus != null ? alliedStatus.State : FactionState.None,
                                  ControllingId = controllingStatus != null ? controllingStatus.Faction.Id : 0,
                                  ControllingName = controllingStatus != null ? controllingStatus.Faction.Name : "",
                                  ControllingInfluence = controllingStatus != null ? controllingStatus.Influence : 0,
                                  ControllingState = controllingStatus != null ? controllingStatus.State : FactionState.None,
                                  Date = last.Date
                              };
        }

        /*statuses.GroupBy(x => x.SolarSystem)
                .Select(g => g.OrderBy(x => x.Date).Last())
                .Select(x => new
                {
                    SolarSystem = x.SolarSystem,
                    ControllingFaction = x.FactionStatus.FirstOrDefault(x => x.Faction.Attitude == FactionAttitude.)
                });*/
    }
}