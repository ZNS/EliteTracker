using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Raven.Client.Indexes;
using ZNS.EliteTracker.Models.Documents;

namespace ZNS.EliteTracker.Models.Indexes
{
    public class SolarSystem_Query : AbstractIndexCreationTask<SolarSystem, ZNS.EliteTracker.Models.Indexes.SolarSystem_Query.Result>
    {
        public class Result
        {
            public string Name { get; set; }
            public string NamePartial { get; set; }
            public IEnumerable<StationEconomy> Economies { get; set; }
            public FactionAttitude Attitude { get; set; }
            public IEnumerable<int> Factions { get; set; }
            public bool HasAlly { get; set; }
            public bool HasCoordinates { get; set; }
            public List<CommodityType> Supply { get; set; }
            public List<CommodityType> Demand { get; set; }
            public PowerPlayLeader PowerPlayLeader { get; set; }
            public PowerPlayState PowerPlayState { get; set; }
            public IEnumerable<StationOutfitting> Outfitting { get; set; }
        };

        public SolarSystem_Query()
        {

            Map = systems => from system in systems
                             select new
                             {
                                 Name = system.Name,
                                 NamePartial = system.Name,
                                 Economies = system.Stations.SelectMany(x => x.Economy).Distinct(),
                                 Factions = system.Factions.Select(x => x.Id),
                                 Attitude = system.Stations.Where(x => x.Main).Select(x => x.Faction.Attitude).FirstOrDefault(),
                                 HasAlly = system.Factions.Any(x => x.Attitude == FactionAttitude.Ally),
                                 HasCoordinates = (system.Coordinates != null && !(system.Coordinates.X == 0 && system.Coordinates.Y == 0 && system.Coordinates.Z == 0)),
                                 Supply = system.Stations.SelectMany(x => x.Commodities).Where(x => x.Supply != CommodityAvailability.None).Select(x => x.Type).Distinct(),
                                 Demand = system.Stations.SelectMany(x => x.Commodities).Where(x => x.Demand != CommodityAvailability.None).Select(x => x.Type).Distinct(),
                                 PowerPlayLeader = system.PowerPlayLeader,
                                 PowerPlayState = system.PowerPlayState,
                                 Outfitting = system.Stations.Select(x => x.Outfitting).Distinct()
                             };

            Indexes.Add(x => x.Name, Raven.Abstractions.Indexing.FieldIndexing.Analyzed);
            Analyzers.Add(x => x.Name, "KeywordAnalyzer");

            Indexes.Add(x => x.NamePartial, Raven.Abstractions.Indexing.FieldIndexing.Analyzed);
            Analyzers.Add(x => x.NamePartial, "Xemio.RavenDB.NGramAnalyzer.NGramAnalyzer,Xemio.RavenDB.NGramAnalyzer");
        }
    }
}