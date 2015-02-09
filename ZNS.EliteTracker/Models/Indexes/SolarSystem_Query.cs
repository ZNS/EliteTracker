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
            public bool HasAlly { get; set; }
            public bool HasCoordinates { get; set; }
        };

        public SolarSystem_Query()
        {

            Map = systems => from system in systems
                             select new
                             {
                                 Name = system.Name,
                                 NamePartial = system.Name,
                                 Economies = system.Stations.SelectMany(x => x.Economy).Distinct(),
                                 Attitude = system.Stations.Where(x => x.Main).Select(x => x.Faction.Attitude).FirstOrDefault(),
                                 HasAlly = system.Factions.Any(x => x.Attitude == FactionAttitude.Ally),
                                 HasCoordinates = (system.Coordinates != null && system.Coordinates.X != 0 && system.Coordinates.Y != 0 && system.Coordinates.Z != 0)
                             };

            Indexes.Add(x => x.NamePartial, Raven.Abstractions.Indexing.FieldIndexing.Analyzed);

            Analyzers.Add(x => x.NamePartial, "Xemio.RavenDB.NGramAnalyzer.NGramAnalyzer,Xemio.RavenDB.NGramAnalyzer");
        }
    }
}