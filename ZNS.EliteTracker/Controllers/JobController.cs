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
        public ActionResult SyncEDDB(string key)
        {
            if (!key.Equals(ConfigurationManager.AppSettings["jobkey"], StringComparison.CurrentCulture))
            {
                return new JsonResult { Data = new { status = "unauthorized" }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }

            var eddb_systems_path = Server.MapPath("/app_data/eddb/systems.json");
            var edsm_systems_path = Server.MapPath("/app_data/eddb/esdm.json");

            var prevSyncDate = DateTime.MinValue;
            if (System.IO.File.Exists(eddb_systems_path))
            {
                prevSyncDate = System.IO.File.GetLastWriteTimeUtc(eddb_systems_path).AddHours(-12);
            }

            using (WebClient wc = new WebClient())
            {
                try
                {
                    wc.DownloadFile(new Uri("https://eddb.io/archive/v4/systems.json"), eddb_systems_path);
                }
                catch {
                    throw new Exception("Unable to download station data from eddb.");
                }
                try
                {
                    wc.DownloadFile(new Uri("http://www.edsm.net/api-v1/systems/?showid=1"), edsm_systems_path + ".tmp");
                    System.IO.File.Delete(edsm_systems_path);
                    System.IO.File.Move(edsm_systems_path + ".tmp", edsm_systems_path);
                }
                catch { }
            }

            if (!System.IO.File.Exists(edsm_systems_path))
            {
                throw new Exception("No edsm data file found.");
            }

            var systems = new List<EDDBSystem>();

            //Read in edsm system data
            var edsm_systems = new List<EDSMSystem>();
            using (var edsm_systems_stream = new Models.Json.JsonStreamReader<EDSMSystem>(edsm_systems_path))
            {
                while (edsm_systems_stream.Next())
                {
                    edsm_systems.Add(edsm_systems_stream.Current);
                }
            }

            using (var eddb_systems = new Models.Json.JsonStreamReader<EDDBSystem>(eddb_systems_path))
            {
                while (eddb_systems.Next())
                {
                    //Updated?
                    if (eddb_systems.Current.Updated_At > prevSyncDate)
                    {
                        //Get edsm id
                        foreach (var edsm in edsm_systems)
                        {
                            if (edsm.Name.Equals(eddb_systems.Current.Name, StringComparison.InvariantCultureIgnoreCase))
                            {
                                eddb_systems.Current.Id_EDSM = edsm.Id;
                                break;
                            }
                        }

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

        private void StoreSystems(List<EDDBSystem> systems)
        {
            using (var session = DB.Instance.GetSession())
            {
                List<SolarSystem> solarSystems = new List<SolarSystem>();
                var batches = Math.Ceiling((double)systems.Count / 15);
                for (var i = 0; i < batches; i++)
                {
                    var names = systems.Where(x => !String.IsNullOrEmpty(x.Name)).Select(y => RavenQuery.Escape(y.Name)).Skip(i * 15).Take(15);
                    solarSystems.AddRange(session.Query<SolarSystem_Query.Result, SolarSystem_Query>()
                        .Where(x => x.Name != null && x.Name != "" && x.Name.In<string>(names))
                        .OfType<SolarSystem>()
                        .ToList());
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
                            SolarSystemSecurity security = SolarSystemSecurity.Medium;
                            if (Enum.TryParse<SolarSystemSecurity>(system.Security, out security))
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
                            if (system.Power_State.Equals("Control", StringComparison.InvariantCultureIgnoreCase))
                                solarSystem.PowerPlayState = PowerPlayState.Control;
                            else if (system.Power_State.Equals("Exploited", StringComparison.InvariantCultureIgnoreCase))
                                solarSystem.PowerPlayState = PowerPlayState.Exploited;
                            else if (system.Power_State.Equals("Expansion", StringComparison.InvariantCultureIgnoreCase))
                                solarSystem.PowerPlayState = PowerPlayState.Expansion;
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