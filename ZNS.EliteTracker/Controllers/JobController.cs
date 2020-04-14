using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Raven.Client.Linq;
using Raven.Abstractions.Data;
using Raven.Abstractions.Smuggler;
using Raven.Database.Smuggler;
using ZNS.EliteTracker.Models;
using ZNS.EliteTracker.Models.Documents;
using ZNS.EliteTracker.Models.EDDB;
using ZNS.EliteTracker.Models.Indexes;
using Raven.Abstractions.Util;

namespace ZNS.EliteTracker.Controllers
{
    [AllowAnonymous]
    public class JobController : Controller
    {
        #region Backup
        public ActionResult Backup(string key)
        {
            if (!key.Equals(ConfigurationManager.AppSettings["jobkey"], StringComparison.CurrentCulture))
            {
                return new JsonResult { Data = new { status = "unauthorized" }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }

            var backupPath = Server.MapPath(ConfigurationManager.AppSettings["backuppath"]).TrimEnd('\\') + "\\dump.raven";
            
            //Delete old
            if (System.IO.File.Exists(backupPath))
            {
                System.IO.File.Delete(backupPath);
            }

            var dumper = new DatabaseDataDumper(DB.Instance.Store.DocumentDatabase, new SmugglerDatabaseOptions
            {
                OperateOnTypes = ItemType.Documents,
                Incremental = false                
            });
            
            dumper.ExportData(new SmugglerExportOptions<RavenConnectionStringOptions>
	        {
		        From = new EmbeddedRavenConnectionStringOptions(),                
		        ToFile = backupPath
	        });

            return new JsonResult { Data = new { status = "ok" }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public ActionResult BackupJson(string key)
        {
            if (!key.Equals(ConfigurationManager.AppSettings["jobkey"], StringComparison.CurrentCulture))
            {
                return new JsonResult { Data = new { status = "unauthorized" }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }

            using (var session = DB.Instance.GetSession())
            {
                BackupDocs<Commander>(false);
                BackupDocs<Faction>(false);
                BackupDocs<SolarSystem>(false);
                BackupDocs<Task>(false);
                BackupDocs<Resource>(false);
                BackupDocs<SolarSystemStatus>(false);
                BackupDocs<TradeRoute>(false);
                BackupDocs<Comment>(false);

                return new JsonResult { Data = new { status = "ok" }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
        }

        private void BackupDocs<T>(bool append, int page = 0) {
            Raven.Client.RavenQueryStatistics stats = null;
            var path = Server.MapPath(ConfigurationManager.AppSettings["backuppath"]).TrimEnd('\\') + "\\" + typeof(T).Name + ".json";

            FileStream fs = null;
            if (!append)
            {
                fs = System.IO.File.Create(path);
            }
            else
            {
                fs = System.IO.File.Open(path, FileMode.Append);
            }

            using (StreamWriter w = new StreamWriter(fs))
            {
                using (var session = DB.Instance.GetSession())
                {
                    var docs = session.Query<T>().Statistics(out stats).Skip(page * 1024).Take(1024).ToList();
                    foreach (var d in docs)
                    {
                        w.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(d));
                    }
                }
            }

            if (stats != null && (stats.TotalResults - ((page + 1) * 1024)) > 0)
            {
                BackupDocs<T>(true, page + 1);
            }
        }
        #endregion

        #region EDDB
        public ActionResult SyncFactions(string key)
        {
            if (!key.Equals(ConfigurationManager.AppSettings["jobkey"], StringComparison.CurrentCulture))
            {
                return new JsonResult { Data = new { status = "unauthorized" }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }

            //Sync Factions
            var eddb_factions_path = Server.MapPath("/app_data/eddb/factions.json");

            using (WebClient wc = new WebClient())
            {
                try
                {
                    wc.DownloadFile(new Uri("https://eddb.io/archive/v5/factions.json"), eddb_factions_path);
                }
                catch
                {
                    throw new Exception("Unable to download station data from eddb.");
                }
            }

            List<EDDBFaction> factions = new List<EDDBFaction>();
            using (var eddb_factions = new Models.Json.JsonStreamReader<EDDBFaction>(eddb_factions_path))
            {
                while (eddb_factions.Next())
                {
                    factions.Add(eddb_factions.Current);
                    if (factions.Count > 149)
                    {
                        StoreFactions(factions);
                        factions.Clear();
                    }
                }
            }

            return new JsonResult { Data = new { status = "ok" }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public ActionResult SyncEDDB(string key)
        {
            var eddb_systems_path = Server.MapPath("/app_data/eddb/systems.json");

            var prevSyncDate = DateTime.MinValue;
            if (System.IO.File.Exists(eddb_systems_path))
            {
                prevSyncDate = System.IO.File.GetLastWriteTimeUtc(eddb_systems_path).AddHours(-25);
            }

            using (WebClient wc = new WebClient())
            {
                try
                {
                    wc.DownloadFile(new Uri("https://eddb.io/archive/v5/systems_populated.json"), eddb_systems_path);
                }
                catch {
                    throw new Exception("Unable to download station data from eddb.");
                }
            }

            var systems = new List<EDDBSystem>();

            using (var eddb_systems = new Models.Json.JsonStreamReader<EDDBSystem>(eddb_systems_path))
            {
                while (eddb_systems.Next())
                {
                    //Updated? 
                    if (eddb_systems.Current.Updated_At > prevSyncDate)
                    {
                        //Add additional data
                        if (eddb_systems.Current.Population.HasValue && eddb_systems.Current.Population.Value > 0)
                        {
                            eddb_systems.Current.CCIncome = (int)Math.Round(0.4343f * Math.Log(eddb_systems.Current.Population.Value) + 1);
                        }

                        //Store
                        systems.Add(eddb_systems.Current);
                        if (systems.Count > 149)
                        {
                            StoreSystems(systems);
                            systems.Clear();
                        }
                    }
                }
            }

            StoreSystems(systems);

            return new JsonResult { Data = new { status = "ok" }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        private void StoreFactions(List<EDDBFaction> factions)
        {
            using (var session = DB.Instance.GetSession())
            {
                List<Faction> localFactions = new List<Faction>();
                var batches = Math.Ceiling((double)factions.Count / 15);
                for (var i = 0; i < batches; i++)
                {
                    var names = factions.Where(x => !String.IsNullOrEmpty(x.Name))
                        .Select(y => RavenQuery.Escape(y.Name.ToLower())).Skip(i * 15).Take(15);
                    var tmpFactions = session.Advanced.DocumentQuery<Faction>("Faction/Query")
                        .Where("@In<NameExact>:(" + String.Join(",", names) + ")")
                        .ToList();
                    localFactions.AddRange(tmpFactions);
                }

                foreach (var f in factions)
                {
                    var localFaction = localFactions.FirstOrDefault(x => x.Name.Equals(f.Name, StringComparison.CurrentCultureIgnoreCase));
                    if (localFaction != null && localFaction.EDDB_Id == 0)
                    {
                        localFaction.EDDB_Id = f.Id;
                    }
                }

                session.SaveChanges();
            }
        }

        private void StoreSystems(List<EDDBSystem> systems)
        {
            using (var session = DB.Instance.GetSession())
            {
                List<SolarSystem> solarSystems = new List<SolarSystem>();
                var batches = Math.Ceiling((double)systems.Count / 15);
                for (var i = 0; i < batches; i++)
                {
                    var names = systems.Where(x => !String.IsNullOrEmpty(x.Name)).Select(y => RavenQuery.Escape(y.Name)).Skip(i * 15).Take(15);
                    var tmpSystems = session.Advanced.DocumentQuery<SolarSystem>("SolarSystem/Query")
                        .Where("@In<NameExact>:(" + String.Join(",", names) + ")")
                        .ToList();
                    solarSystems.AddRange(tmpSystems);
                }

                foreach (var system in systems)
                {
                    var solarSystem = solarSystems.FirstOrDefault(x => x.Name == system.Name);
                    if (solarSystem != null && system.Updated_At > solarSystem.Updated)
                    {
                        solarSystem.Updated = system.Updated_At;
                        if (system.Population.HasValue && system.Population.Value > 0)
                        {
                            solarSystem.PopulationPrev = solarSystem.Population;
                            solarSystem.Population = system.Population.Value;
                        }
                        if (system.X.HasValue && system.Y.HasValue && system.Z.HasValue)
                        {
                            solarSystem.Coordinates.X = system.X.Value;
                            solarSystem.Coordinates.Y = system.Y.Value;
                            solarSystem.Coordinates.Z = system.Z.Value;
                        }
                        if (!String.IsNullOrEmpty(system.Security))
                        {
                            if (Enum.TryParse<SolarSystemSecurity>(system.Security, out SolarSystemSecurity security))
                            {
                                solarSystem.SecurityPrev = solarSystem.Security;
                                solarSystem.Security = security;
                            }
                        }
                        if (!String.IsNullOrEmpty(system.Power))
                        {
                            PowerPlayLeader leader = PowerPlayLeader.None;
                            if (Enum.TryParse<PowerPlayLeader>(system.Power.Replace(" ", "").Replace("-", ""), out leader))
                            {
                                solarSystem.PowerPlayLeader = leader;
                            }
                        }
                        if (!String.IsNullOrEmpty(system.Power_State))
                        {
                            if (Enum.TryParse<PowerPlayState>(system.Power_State, out PowerPlayState ppstate))
                            {
                                solarSystem.PowerPlayState = ppstate;
                            }
                        }
                    }

                    //Faction status
                    if (solarSystem != null && solarSystem.SyncFactionStatus && system.MinorFactions != null && system.MinorFactions.Count > 0)
                    {
                        var statuses = session.Query<SolarSystemStatus>()
                            .Where(x => x.SolarSystem == solarSystem.Id && x.Date >= system.Updated_At.Date)
                            .ToList();
                        if (statuses == null || statuses.Count == 0)
                        {
                            var status = new SolarSystemStatus();
                            status.SolarSystem = solarSystem.Id;
                            status.Date = system.Updated_At.Date;
                            status.FactionStatus = new List<FactionStatus>();

                            var factionIds = system.MinorFactions.Select(x => x.Id);
                            var localFactions = session.Query<Faction_Query.Result, Faction_Query>()
                                .Where(x => x.EDDBId.In<int>(factionIds))
                                .OfType<Faction>()
                                .ToList();

                            foreach (var f in system.MinorFactions)
                            {
                                if (f.Influence.HasValue)
                                {
                                    var lf = localFactions.FirstOrDefault(x => x.EDDB_Id == f.Id);
                                    if (lf != null)
                                    {
                                        FactionState state = FactionState.None;
                                        Enum.TryParse<FactionState>(f.State, true, out state);
                                        status.FactionStatus.Add(new FactionStatus
                                        {
                                            Faction = FactionRef.FromFaction(lf),
                                            Influence = f.Influence.Value,
                                            State = state
                                        });
                                    }
                                }
                            }
                            session.Store(status);
                        }
                    }
                    session.Store(system);
                }
                session.SaveChanges();
            }
        }
        #endregion
    }
}