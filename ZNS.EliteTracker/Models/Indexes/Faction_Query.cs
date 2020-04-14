using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Raven.Client.Indexes;
using ZNS.EliteTracker.Models.Documents;

namespace ZNS.EliteTracker.Models.Indexes
{
    public class Faction_Query : AbstractIndexCreationTask<Faction, ZNS.EliteTracker.Models.Indexes.Faction_Query.Result>
    {
        public class Result
        {
            public string Name { get; set; }
            public string NamePartial { get; set; }
            public string NameExact { get; set; }
            public string HomeSystem { get; set; }
            public FactionAttitude Attitude { get; set; }
            public FactionState State { get; set; }
            public int EDDBId { get; set; }
            public List<int> SystemGroups { get; set; }
        }

        public Faction_Query()
        {
            Map = factions => from faction in factions
                              select new
                              {
                                  Name = faction.Name,
                                  NamePartial = faction.Name,
                                  NameExact = faction.Name,
                                  HomeSystem = faction.HomeSolarSystem.Name,
                                  Attitude = faction.Attitude,
                                  State = faction.State,
                                  EDDBId = faction.EDDB_Id,
                                  SystemGroups = faction.SolarSystems.SelectMany(x => LoadDocument<SolarSystem>("SolarSystems/" + x.Id).Groups).Distinct()
                              };

            Indexes.Add(x => x.Name, Raven.Abstractions.Indexing.FieldIndexing.Analyzed);
            Indexes.Add(x => x.NamePartial, Raven.Abstractions.Indexing.FieldIndexing.Analyzed);
            Indexes.Add(x => x.NameExact, Raven.Abstractions.Indexing.FieldIndexing.Default);
            Indexes.Add(x => x.HomeSystem, Raven.Abstractions.Indexing.FieldIndexing.Default);

            Analyzers.Add(x => x.NamePartial, "Xemio.RavenDB.NGramAnalyzer.NGramAnalyzer,Xemio.RavenDB.NGramAnalyzer");
        }
    }
}