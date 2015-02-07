using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ZNS.EliteTracker.Models;
using ZNS.EliteTracker.Models.Documents;

namespace ZNS.EliteTracker.Controllers
{
    public class ImportController : Controller
    {
        // GET: Import
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(HttpPostedFileBase file)
        {
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

        private string Capitalize(string str)
        {
            if (str == null || str.Trim().Length == 0)
                return str;
            return char.ToUpper(str[0]) + str.Substring(1).ToLower();
        }
    }
}