using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Raven.Abstractions.Data;
using Raven.Abstractions.Smuggler;
using Raven.Database.Smuggler;
using ZNS.EliteTracker.Models;
using ZNS.EliteTracker.Models.Documents;

namespace ZNS.EliteTracker.Controllers
{
    public class DataController : Controller
    {
        #region Import
        public ActionResult Import()
        {
            if (!User.IsInRole("administrator"))
            {
                return new HttpUnauthorizedResult();
            }
            return View();
        }

        [HttpPost]
        public ActionResult Import(HttpPostedFileBase file)
        {
            if (!User.IsInRole("administrator"))
            {
                return new HttpUnauthorizedResult();
            }

            if (file != null)
            {
                List<Faction> factions = new List<Faction>();
                List<SolarSystem> systems = new List<SolarSystem>();
                using (StreamReader reader = new StreamReader(file.InputStream))
                {
                    do
                    {
                        var line = reader.ReadLine();
                        var cols = line.Split(',');
                        if (cols.Length < 14)
                            continue;

                        var systemName = cols[0];
                        var factionName = cols[1];
                        var strAllegiance = cols[2];
                        var strEconomy = cols[3];
                        long population = 0;
                        try
                        {
                            population = !String.IsNullOrEmpty(cols[5]) ? long.Parse(cols[5].Replace(".", "").Replace(",", "")) : 0;
                        }
                        catch { }
                        int coordX = !String.IsNullOrEmpty(cols[11]) ? int.Parse(cols[11]) : 0;
                        int coordY = !String.IsNullOrEmpty(cols[12]) ? int.Parse(cols[12]) : 0;
                        int coordZ = !String.IsNullOrEmpty(cols[13]) ? int.Parse(cols[13]) : 0;

                        if (String.IsNullOrEmpty(systemName))
                            continue;
                        systemName = systemName.Trim();

                        using (var session = DB.Instance.GetSession())
                        {
                            SolarSystem system = systems.FirstOrDefault(x => x.Name.Equals(systemName, StringComparison.CurrentCultureIgnoreCase));
                            if (system == null)
                            {
                                //System
                                system = new SolarSystem
                                {
                                    Name = Capitalize(systemName),
                                    Population = population,
                                    Coordinates = new Coordinate
                                    {
                                        X = coordX,
                                        Y = coordY,
                                        Z = coordZ
                                    }
                                };
                                session.Store(system);
                                //Save system to get id
                                session.SaveChanges();
                                systems.Add(system);
                            }

                            //Faction
                            if (!String.IsNullOrEmpty(factionName))
                            {
                                var faction = factions.FirstOrDefault(x => x.Name.Equals(factionName, StringComparison.CurrentCultureIgnoreCase));
                                if (faction == null)
                                {
                                    factionName = factionName.Trim();
                                    faction = new Faction
                                    {
                                        Name = factionName,
                                        Attitude = FactionAttitude.Ally,
                                        Government = FactionGovernment.Communism,
                                        Allegiance = Power.Independent
                                    };
                                    if (!String.IsNullOrEmpty(strAllegiance))
                                    {
                                        Power power = Power.Independent;
                                        if (Enum.TryParse<Power>(strAllegiance, out power))
                                        {
                                            faction.Allegiance = power;
                                        }
                                    }
                                    faction.HomeSolarSystem = new SolarSystemRef
                                    {
                                        Id = system.Id,
                                        Name = system.Name
                                    };
                                    faction.SolarSystems.Add(faction.HomeSolarSystem);
                                    session.Store(faction);
                                    session.SaveChanges();
                                    factions.Add(faction);
                                }
                                else
                                {
                                    faction = session.Load<Faction>(faction.Id);
                                    faction.SolarSystems.Add(SolarSystemRef.FromSolarSystem(system));
                                    session.SaveChanges();
                                }
                            }
                        }
                    }
                    while (reader.Peek() != -1);
                }
            }
            return View();
        }
        #endregion

        #region Export
        /*[HttpPost]
        public ActionResult Export()
        {
            using (var session = DB.Instance.GetSession())
            {
                using (StreamWriter writer = new StreamWriter(new MemoryStream()))
                {
                    using (var stream = session.Advanced.Stream<SolarSystem>(session.Query<SolarSystem>()))
                    {
                        while (stream.MoveNext())
                        {
                            FactionRef alliedFaction = stream.Current.Document.Factions.FirstOrDefault(x => x.Attitude == FactionAttitude.Ally);
                            FactionRef controlingFaction = null;
                            var mainStation = stream.Current.Document.Stations.FirstOrDefault(x => x.Main);
                            if (mainStation != null)
                            {
                                controlingFaction = mainStation.Faction;
                            }
                            writer.WriteLine(
                                stream.Current.Document.Name + "\t" +
                                (alliedFaction != null ? alliedFaction.Name : "") + "\t" +
                                "N/A" + "\t" + //Allegiance
                                String.Join("/", stream.Current.Document.Economies.ToArray()) + "\t" +
                                "N/A" + "\t" + //State
                                stream.Current.Document.Population + "\t" +
                                "N/A" + "\t" + //Influence
                                (controlingFaction != null ? controlingFaction.Name : "") + "\t" +
                                "N/A" + "\t" + //Controlling Allegiance
                                (stream.Current.Document.Coordinates != null ? stream.Current.Document.Coordinates.X.ToString() : "") + "\t" +
                                (stream.Current.Document.Coordinates != null ? stream.Current.Document.Coordinates.Y.ToString() : "") + "\t" +
                                (stream.Current.Document.Coordinates != null ? stream.Current.Document.Coordinates.Z.ToString() : "")
                                );
                        }
                    }
                }
            }
            throw new Exception();
        }*/
        #endregion

        #region Restore
        public async Task<ActionResult> Restore(string key, string path)
        {
            if (!key.Equals(ConfigurationManager.AppSettings["jobkey"], StringComparison.CurrentCulture))
            {
                return new JsonResult { Data = new { status = "unauthorized" }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
            if (ConfigurationManager.AppSettings["installation"] == null || ConfigurationManager.AppSettings["installation"] != "1")
            {
                return new JsonResult { Data = new { status = "need to be in installation mode" }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }

            var backupPath = Server.MapPath(path);
            var dumper = new DatabaseDataDumper(DB.Instance.Store.DocumentDatabase, new SmugglerDatabaseOptions
            {
                OperateOnTypes = ItemType.Documents,
                Incremental = false
            });

            await dumper.ImportData(new SmugglerImportOptions<RavenConnectionStringOptions>
            {
                To = new EmbeddedRavenConnectionStringOptions(),
                FromFile = backupPath
            });

            return new JsonResult { Data = new { status = "restore complete" }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
        #endregion

        private string Capitalize(string str)
        {
            if (str == null || str.Trim().Length == 0)
                return str;
            return char.ToUpper(str[0]) + str.Substring(1).ToLower();
        }
    }
}