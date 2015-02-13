using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Raven.Client;
using ZNS.EliteTracker.Models;
using ZNS.EliteTracker.Models.Documents;
using ZNS.EliteTracker.Models.Indexes;
using ZNS.EliteTracker.Models.Views;

namespace ZNS.EliteTracker.Controllers
{
    public class SolarSystemController : BaseController
    {
        public ActionResult Index(int? page, SolarSystemIndexView.Form form)
        {
            page = page ?? 0;
            var view = new SolarSystemIndexView();
            using (var session = DB.Instance.GetSession())
            {
                //Query
                RavenQueryStatistics stats = null;
                var query = session.Query<SolarSystem_Query.Result, SolarSystem_Query>()
                    .Statistics(out stats)
                    .OrderBy(x => x.Name)
                    .Skip(page.Value * 35)
                    .Take(35);
                if (!String.IsNullOrEmpty(form.Query))
                {
                    query = query.Where(x => x.NamePartial == form.Query);
                }
                if (form.Economy != 0)
                {
                    var enumEconomy = (StationEconomy)Enum.Parse(typeof(StationEconomy), form.Economy.ToString());
                    query = query.Where(x => x.Economies.Any(e => e == enumEconomy));
                }
                if (form.Supply != 0)
                {
                    var enumSupply = (CommodityType)Enum.Parse(typeof(CommodityType), form.Supply.ToString());
                    query = query.Where(x => x.Supply.Any(s => s == enumSupply));
                }
                if (form.Demand != 0)
                {
                    var enumDemand = (CommodityType)Enum.Parse(typeof(CommodityType), form.Demand.ToString());
                    query = query.Where(x => x.Demand.Any(s => s == enumDemand));
                }
                switch (form.Status)
                {
                    case 1:
                        query = query.Where(x => x.Attitude == FactionAttitude.Ally);
                        break;
                    case 2:
                        query = query.Where(x => x.Attitude == FactionAttitude.Hostile);
                        break;
                    case 3:
                        query = query.Where(x => x.Attitude != FactionAttitude.Hostile);
                        break;
                    case 4:
                        query = query.Where(x => x.Attitude == FactionAttitude.Hostile && x.HasAlly);
                        break;
                }

                //Set up view
                view.SolarSystems = query.OfType<SolarSystem>().ToList();
                view.Query = form;
                view.Pager = new Pager
                {
                    Count = stats.TotalResults,
                    Page = page.Value,
                    PageSize = 35
                };
 
                //Select list item
                view.Statuses = new List<SelectListItem>
                {
                    new SelectListItem { Text = "All statuses", Value = "0", Selected = form.Status == 0 },
                    new SelectListItem { Text = "Allied", Value = "1", Selected = form.Status == 1 },
                    new SelectListItem { Text = "Hostile", Value = "2", Selected = form.Status == 2 },
                    new SelectListItem { Text = "Not hostile", Value = "3", Selected = form.Status == 3 },
                    new SelectListItem { Text = "Allied faction not in control", Value = "4", Selected = form.Status == 4 }
                };
                return View(view); 
            }
        }

        public ActionResult View(int id)
        {
            SolarSystem system = null;
            ViewBag.Factions = new List<Faction>();
            using (var session = DB.Instance.GetSession())
            {
                system = session.Load<SolarSystem>(id);
                ViewBag.Factions = session.Query<Faction>().Where(x => x.SolarSystems.Any(s => s.Id == id)).ToList();
            }
            return View(system);
        }

        public ActionResult Comments(int id, int? page)
        {
            page = page ?? 0;
            var view = new SolarSystemCommentsView();
            view.Comments = new CommentView
            {
                 DocumentId = "SolarSystems/" + id
            };
            using (var session = DB.Instance.GetSession())
            {
                RavenQueryStatistics stats = null;
                view.SolarSystem = session.Load<SolarSystem>(id);
                view.Comments.Comments = session.Advanced.DocumentQuery<Comment>()
                    .Statistics(out stats)
                    .WhereEquals(x => x.DocumentId, view.Comments.DocumentId)
                    .OrderByDescending(x => x.Date)
                    .Skip(page.Value * 15)
                    .Take(15)
                    .ToList();
                view.Comments.Pager = new Pager
                {
                    Count = stats.TotalResults,
                    Page = page.Value,
                    PageSize = 15
                };
            }
            return View(view);
        }

        public ActionResult Tasks(int id, int? page)
        {
            page = page ?? 0;
            var view = new SolarSystemTasksView();
            using (var session = DB.Instance.GetSession())
            {
                RavenQueryStatistics stats = null;
                view.SolarSystem = session.Load<SolarSystem>(id);
                view.Tasks = session.Query<Task>()
                    .Statistics(out stats)
                    .Where(x => x.SolarSystem.Id == id && x.Status != TaskStatus.Completed)
                    .OrderByDescending(x => x.Priority)
                    .ThenByDescending(x => x.Date)
                    .Skip(page.Value * 20)
                    .Take(20)
                    .ToList();
                view.Pager = new Pager
                {
                    Count = stats.TotalResults,
                    Page = page.Value,
                    PageSize = 20
                };
                return View(view);
            }
        }

        public ActionResult Distance(int id, SolarSystemDistanceView.Form form)
        {
            var view = new SolarSystemDistanceView();
            using (var session = DB.Instance.GetSession())
            {
                view.SolarSystem = session.Load<SolarSystem>(id);
                if (view.SolarSystem.HasCoordinates)
                {
                    var query = session.Query<SolarSystem_Query.Result, SolarSystem_Query>()
                        .Where(x => x.HasCoordinates)
                        .Take(512);
                    if (form.Supply != 0)
                    {
                        var enumSupply = (CommodityType)Enum.Parse(typeof(CommodityType), form.Supply.ToString());
                        query = query.Where(x => x.Supply.Any(s => s == enumSupply));
                    }
                    if (form.Demand != 0)
                    {
                        var enumDemand = (CommodityType)Enum.Parse(typeof(CommodityType), form.Demand.ToString());
                        query = query.Where(x => x.Demand.Any(s => s == enumDemand));
                    }

                    view.Systems = query
                        .OfType<SolarSystem>()
                        .ToList();

                    foreach (var system in view.Systems)
                    {
                        system.Distance = Math.Sqrt(
                            Math.Pow(((double)system.Coordinates.X - (double)view.SolarSystem.Coordinates.X), 2) +
                            Math.Pow(((double)system.Coordinates.Y - (double)view.SolarSystem.Coordinates.Y), 2) +
                            Math.Pow(((double)system.Coordinates.Z - (double)view.SolarSystem.Coordinates.Z), 2)
                            );
                    }
                    view.Systems.RemoveAll(x => x.Id == view.SolarSystem.Id);
                    view.Systems = view.Systems.OrderBy(x => x.Distance).ToList();
                    view.Query = form;
                }
                else
                {
                    view.Systems = new List<SolarSystem>();
                }
            }
            return View(view);
        }

        #region Market
        public ActionResult Market(int id, string guid)
        {
            var view = new SolarSystemMarketView();
            using (var session = DB.Instance.GetSession())
            {
                view.SolarSystem = session.Load<SolarSystem>(id);
                view.StationGuid = guid;
                if (String.IsNullOrEmpty(view.StationGuid) && view.SolarSystem.Stations != null && view.SolarSystem.Stations.Count > 0)
                {
                    view.StationGuid = view.SolarSystem.Stations.First().Guid;
                }
            }
            return View(view);
        }

        [HttpPost]
        public ActionResult Market(int id, FormCollection frm)
        {
            using (var session = DB.Instance.GetSession())
            {
                var guid = frm["guid"];
                var system = session.Load<SolarSystem>(id);
                var station = system.Stations.First(x => x.Guid == guid);
                station.Commodities.Clear();
                foreach (var post in frm.AllKeys)
                {
                    if (post.StartsWith("c__"))
                    {
                        var parts = post.Substring(3).Split('_');
                        
                        CommodityType type = (CommodityType)Enum.Parse(typeof(CommodityType), parts[0]);
                        CommodityAvailability supply = CommodityAvailability.None;
                        CommodityAvailability demand = CommodityAvailability.None;
                        double price = 0;

                        if (parts[1] == "supply")
                        {
                            supply = (CommodityAvailability)Enum.Parse(typeof(CommodityAvailability), frm[post]);
                        }
                        if (parts[1] == "demand")
                        {
                            demand = (CommodityAvailability)Enum.Parse(typeof(CommodityAvailability), frm[post]);
                        }
                        if (parts[1] == "price")
                        {
                            price = !String.IsNullOrEmpty(frm[post]) ? int.Parse(frm[post]) : 0;
                        }

                        if (supply != CommodityAvailability.None || demand != CommodityAvailability.None || price != 0)
                        {
                            var commodity = station.Commodities.FirstOrDefault(x => x.Type == type);
                            if (commodity == null)
                            {
                                commodity = new Commodity
                                {
                                    Type = type
                                };
                                station.Commodities.Add(commodity);
                            }
                            commodity.Supply = supply != CommodityAvailability.None ? supply : commodity.Supply;
                            commodity.Demand = demand != CommodityAvailability.None ? demand : commodity.Demand;
                            commodity.Price = price != 0 ? price : commodity.Price;
                            commodity.Updated = DateTime.UtcNow;
                        }
                    }
                }
                session.SaveChanges();
                return RedirectToAction("Market", new { id = system.Id, guid = guid });
            }
        }
        #endregion

        #region Edit
        public ActionResult Edit(int? id)
        {
            SolarSystem system = new SolarSystem();
            ViewBag.Factions = new List<Faction>();
            using (var session = DB.Instance.GetSession())
            {                
                if (id.HasValue)
                {
                    system = session.Load<SolarSystem>(id.Value);
                    ViewBag.Factions = session.Query<Faction>().Where(x => x.SolarSystems.Any(s => s.Id == id.Value)).ToList();
                }                
            }
            return View(system);
        }

        [HttpPost]
        public ActionResult Edit(int? id, SolarSystem input)
        {
            using (var session = DB.Instance.GetSession())
            {
                //Check if exists
                if (!id.HasValue)
                {
                    var existing = session.Query<SolarSystem>()
                        .Where(x => x.Name == input.Name).ToList()
                        .FirstOrDefault(x => x.Name.Equals(input.Name, StringComparison.CurrentCultureIgnoreCase));
                    if (existing != null)
                    {
                        return RedirectToAction("Edit", new { status = "Solar system already exists" });
                    }
                }

                if (!id.HasValue)
                {
                    session.Store(input);
                    session.SaveChanges();
                }
                else
                {
                    var system = session.Load<SolarSystem>(id);
                    system.Name = input.Name;
                    system.PopulationPrev = system.Population;
                    system.Population = input.Population;
                    system.SecurityPrev = system.Security;
                    system.Security = input.Security;
                    if (system.Coordinates == null)
                    {
                        system.Coordinates = new Coordinate();
                    }
                    system.Coordinates.X = input.Coordinates.X;
                    system.Coordinates.Y = input.Coordinates.Y;
                    system.Coordinates.Z = input.Coordinates.Z;
                    session.SaveChanges();
                }
            }

            if (id.HasValue)
            {
                return RedirectToAction("View", "SolarSystem", new { id = id.Value });
            }
            return RedirectToAction("Edit", "SolarSystem", new { id = input.Id });
        }
        #endregion

        #region Client requests
        [HttpPost]
        public ActionResult SaveStation(int id, Station station)
        {
            using (var session = DB.Instance.GetSession())
            {
                session.Advanced.UseOptimisticConcurrency = true;
                
                station.Faction = FactionRef.FromFaction(session.Load<Faction>(station.Faction.Id));
                //Check for undefined faction
                if (station.Faction == null)
                {
                    station.Faction = new FactionRef
                    {
                        Name = "[Undefined]",
                        Id  = 0,
                        Attitude = FactionAttitude.Neutral
                    };
                }

                var system = session.Load<SolarSystem>(id);
                if (system != null)
                {
                    if (!String.IsNullOrEmpty(station.Guid))
                    {
                        system.Stations.Remove(system.Stations.First(x => x.Guid == station.Guid));
                    }
                    else
                    {
                        station.Guid = Guid.NewGuid().ToString();
                    }
                    system.Stations.Add(station);
                }
                session.SaveChanges();
            }
            return new JsonResult { Data = new { status = "OK" } };
        }

        [HttpPost]
        public ActionResult RemoveStation(int id, string guid)
        {
            using (var session = DB.Instance.GetSession())
            {
                var system = session.Load<SolarSystem>(id);
                if (system != null)
                {
                    var station = system.Stations.FirstOrDefault(x => x.Guid == guid);
                    if (station != null)
                    {
                        system.Stations.Remove(station);
                        session.SaveChanges();
                    }
                }
            }
            return new JsonResult { Data = new { status = "OK" } };
        }

        [HttpPost]
        public ActionResult SaveStatus(int id, SolarSystemStatus status)
        {
            //Convert date to UTC
            status.Date = status.Date.ToUniversalTime();
            //Update faction refs
            var factionIds = status.FactionStatus.Select(x => x.Faction.Id).Cast<System.ValueType>();
            using (var session = DB.Instance.GetSession())
            {
                var factions = session.Load<Faction>(factionIds);
                foreach (var factionStatus in status.FactionStatus)
                {
                    var faction = factions.First(x => x.Id == factionStatus.Faction.Id);
                    //Set faction ref for status
                    factionStatus.Faction = FactionRef.FromFaction(faction);
                    //Update faction with current state
                    if (status.Date.Date == DateTime.UtcNow.Date)
                    {
                        faction.State = factionStatus.State;
                        faction.PendingStates = factionStatus.PendingStates;
                    }
                }
                session.Store(status);
                session.SaveChanges();
            }
            return new JsonResult { Data = new { status = "OK", id = status.Id, date = status.Date.ToString("o") } };
        }

        public ActionResult GetStations(int id)
        {
            using (var session = DB.Instance.GetSession())
            {
                var system = session.Load<SolarSystem>(id);
                return new JsonResult() { Data = system.Stations, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }                
        }

        public ActionResult GetStatus(int id, DateTime from, DateTime to)
        {
            using (var session = DB.Instance.GetSession())
            {
                var status = session.Query<SolarSystemStatus>().Where(x => x.SolarSystem == id && x.Date >= from && x.Date <= to).OrderBy(x => x.Date).ToList();
                //Use Newtonsoft for serializing dates
                return new ContentResult
                {
                    ContentType = "application/json",
                    Content = Newtonsoft.Json.JsonConvert.SerializeObject(status)
                };
            }
        }

        public ActionResult GetStationEconomies()
        {
            var economies = Enum.GetValues(typeof(StationEconomy))
                .Cast<StationEconomy>()
                .Select(x => new { text = x.ToString(), value = (int)x })
                .ToList();
            return new ContentResult
            {
                ContentType = "application/json",
                Content = Newtonsoft.Json.JsonConvert.SerializeObject(economies)
            };
        }

        [HttpPost]
        public ActionResult AddActiveCommander(int id)
        {
            using (var session = DB.Instance.GetSession())
            {
                var system = session.Load<SolarSystem>(id);
                if (system != null && !system.ActiveCommanders.Any(x => x.Id == CommanderId))
                {
                    system.ActiveCommanders.Add(new CommanderRef
                    {
                        Id = CommanderId,
                        Name = User.Identity.Name
                    });
                    session.SaveChanges();
                }
            }
            return new JsonResult { Data = new { status = "ok" } };
        }

        [HttpPost]
        public ActionResult RemoveActiveCommander(int id)
        {
            using (var session = DB.Instance.GetSession())
            {
                var system = session.Load<SolarSystem>(id);
                if (system != null)
                {
                    var cmdr = system.ActiveCommanders.FirstOrDefault(x => x.Id == CommanderId);
                    if (cmdr != null)
                    {
                        system.ActiveCommanders.Remove(cmdr);
                    }
                    session.SaveChanges();
                }
            }
            return new JsonResult { Data = new { status = "ok" } };
        }

        #endregion
    }
}