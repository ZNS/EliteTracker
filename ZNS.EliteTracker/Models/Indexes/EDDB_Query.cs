using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Raven.Client.Indexes;
using ZNS.EliteTracker.Models.EDDB;

namespace ZNS.EliteTracker.Models.Indexes
{
    public class EDDB_Query : AbstractIndexCreationTask<EDDBSystem, ZNS.EliteTracker.Models.Indexes.EDDB_Query.Result>
    {
        public class Result
        {
            public int EDSMId { get; set; }
            public string Name { get; set; }
            public string NamePartial { get; set; }
            public int? Population { get; set; }
            public string Power { get; set; }
            public string Power_State { get; set; }
            public int CCIncome { get; set; }
        };

        public EDDB_Query()
        {

            Map = systems => from system in systems
                             select new
                             {
                                 EDSMId = system.EDSM_Id,
                                 Name = system.Name,
                                 NamePartial = system.Name,
                                 Population = system.Population,
                                 Power = system.Power,
                                 Power_State = system.Power_State,
                                 CCIncome = system.CCIncome
                             };

            Indexes.Add(x => x.NamePartial, Raven.Abstractions.Indexing.FieldIndexing.Analyzed);
            Analyzers.Add(x => x.NamePartial, "Xemio.RavenDB.NGramAnalyzer.NGramAnalyzer,Xemio.RavenDB.NGramAnalyzer");
        }
    }
}