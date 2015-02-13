using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json.Linq;
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

        #region Commander's log
        [HttpPost]
        public ActionResult CommandersLog(HttpPostedFileBase file)
        {
            string json = null;

            if (file == null)
            {
                return View("CommandersLogError", null, "No file uploaded");
            }

            var kb = (double)file.ContentLength / 1024d;
            if (kb > 1024 || kb == 0)
            {
                return View("CommandersLogError", null, "File cannot be larger than 1 MB");
            }

            try
            {
                using (var output = new MemoryStream())
                {
                    using (var writer = new StreamWriter(output))
                    {
                        using (var reader = new StreamReader(file.InputStream))
                        {
                            do
                            {
                                var inp = reader.ReadLine().Trim();
                                if (!String.IsNullOrEmpty(inp))
                                {
                                    if (inp == "{")
                                    {
                                        writer.Write(inp);
                                    }
                                    else if (inp == "}")
                                    {
                                        writer.Write("},");
                                    }
                                    else if (Char.IsUpper(inp, 0))
                                    {
                                        writer.Write("\"" + inp.ToLower().Replace(" ", "_") + "\":");
                                    }
                                    else if (inp.Contains("="))
                                    {
                                        var parts = inp.Split('=');
                                        writer.Write("\"" + parts[0].Trim().ToLower() + "\":" + parts[1].Trim() + ",");
                                    }
                                }
                            }
                            while (reader.Peek() != -1);
                        }
                    }

                    json = Encoding.UTF8.GetString(output.ToArray());
                    json = json.Replace(",}", "}");
                    json = "{" + json.Trim(',') + "}";
                }
            }
            catch
            {
                return View("CommandersLogError", null, "Unable to read file.");
            }

            Dictionary<string, List<string>> saved = new Dictionary<string, List<string>>();
            List<string> skipped = new List<string>();
            List<string> failed = new List<string>();
            
            if (json != null)
            {
                JToken data = null;
                try
                {
                    data = JObject.Parse(json).SelectToken("save_data");
                }
                catch
                {
                    return View("CommandersLogError", null, "Unable to parse data");
                }

                var version = Convert.ToInt32(((JValue)data.SelectToken("saveversion")).Value);                
                if (version != 2)
                {
                    return View("CommandersLogError", null, "Incorrect save version. Must be 2.");
                }

                foreach (var child in data.Children())
                {
                    try
                    {
                        if (child is JProperty && ((JProperty)child).Value is JObject)
                        {
                            var systemName = ((JProperty)child).Name.Replace("_", " ");
                            using (var session = DB.Instance.GetSession())
                            {
                                var solarSystem = session.Query<SolarSystem>()
                                    .Where(x => x.Name == systemName)
                                    .FirstOrDefault();

                                if (solarSystem == null)
                                {
                                    skipped.Add(Capitalize(systemName));
                                    continue;
                                }

                                foreach (var objStation in child)
                                {
                                    var stationName = ((JObject)objStation).Properties().First().Name.Replace("_", " ");
                                    try
                                    {
                                        var station = solarSystem.Stations.FirstOrDefault(x => x.Name.Equals(stationName, StringComparison.CurrentCultureIgnoreCase));
                                        if (station != null)
                                        {
                                            var commodities = ((JObject)objStation)
                                                .Properties()
                                                .First()
                                                .Value
                                                .SelectToken("commodities");
                                            if (commodities != null)
                                            {
                                                //Gather commodities for station
                                                List<Commodity> stationCommodities = new List<Commodity>();
                                                foreach (var commodity in commodities)
                                                {
                                                    var objCommodity = (JProperty)commodity;
                                                    var commodityName = objCommodity.Name;
                                                    CommodityType type = CommodityType.Advanced_Catalysers;
                                                    if (Enum.TryParse<CommodityType>(commodityName, true, out type))
                                                    {
                                                        var status = Convert.ToInt32(((JValue)objCommodity.Value.SelectToken("status")).Value);
                                                        var price = 0;
                                                        var tokenPrice = objCommodity.Value.SelectToken("status");
                                                        if (tokenPrice != null)
                                                        {
                                                            int.TryParse(((JValue)tokenPrice).Value.ToString(), out price);
                                                        }
                                                        int[] timeArray = null;
                                                        var tokenTime = objCommodity.Value.SelectToken("modtime");
                                                        if (tokenTime != null)
                                                        {
                                                            timeArray = ((JValue)tokenTime).Value.ToString()
                                                                .Split(',')
                                                                .Select(x => int.Parse(x.TrimStart('0').PadLeft(1, '0')))
                                                                .ToArray();
                                                        }

                                                        var stationCommodity = new Commodity
                                                        {
                                                            Type = type,
                                                            Price = price
                                                        };
                                                        if (timeArray != null)
                                                        {
                                                            stationCommodity.Updated = new DateTime(timeArray[0], timeArray[1], timeArray[2], timeArray[3], timeArray[4], 0).ToUniversalTime();
                                                        }

                                                        switch (status)
                                                        {
                                                            case 0:
                                                                stationCommodity.Supply = CommodityAvailability.High;
                                                                break;
                                                            case 1:
                                                                stationCommodity.Supply = CommodityAvailability.Medium;
                                                                break;
                                                            case 2:
                                                                stationCommodity.Supply = CommodityAvailability.Low;
                                                                break;
                                                            case 4:
                                                                stationCommodity.Demand = CommodityAvailability.Low;
                                                                break;
                                                            case 5:
                                                                stationCommodity.Demand = CommodityAvailability.Medium;
                                                                break;
                                                            case 6:
                                                                stationCommodity.Demand = CommodityAvailability.High;
                                                                break;
                                                        }

                                                        stationCommodities.Add(stationCommodity);
                                                    }
                                                }

                                                //Update db with commodities
                                                List<Commodity> removedCommodities = new List<Commodity>();
                                                foreach (var c in stationCommodities)
                                                {
                                                    var existingCommodity = station.Commodities.FirstOrDefault(x => x.Type == c.Type);
                                                    if (existingCommodity != null && c.Updated >= existingCommodity.Updated)
                                                    {
                                                        if (c.Demand == CommodityAvailability.None && c.Supply == CommodityAvailability.None)
                                                        {
                                                            removedCommodities.Add(existingCommodity);
                                                        }
                                                        else
                                                        {
                                                            existingCommodity.Demand = c.Demand;
                                                            existingCommodity.Supply = c.Supply;
                                                            existingCommodity.Price = c.Price;
                                                        }
                                                    }
                                                    else if (c.Demand != CommodityAvailability.None || c.Supply != CommodityAvailability.None)
                                                    {
                                                        station.Commodities.Add(c);
                                                    }
                                                }
                                                if (removedCommodities.Count > 0)
                                                {
                                                    station.Commodities = station.Commodities.Except(removedCommodities).ToList();
                                                }

                                                if (!saved.ContainsKey(systemName))
                                                {
                                                    saved.Add(systemName, new List<string>());
                                                }
                                                saved[systemName].Add(stationName);
                                            }
                                        }
                                    }
                                    catch
                                    {
                                        failed.Add(stationName);
                                    }
                                }
                                session.SaveChanges();
                            }
                        }
                    }
                    catch {
                        //Failed system
                    }
                }
            }

            ViewBag.Saved = saved;
            ViewBag.Skipped = skipped;
            ViewBag.Failed = failed;
            return View("CommandersLogSuccess");
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