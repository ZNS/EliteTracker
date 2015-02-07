using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Raven.Client;
using ZNS.EliteTracker.Models;
using ZNS.EliteTracker.Models.Documents;
using ZNS.EliteTracker.Models.Views;

namespace ZNS.EliteTracker.Controllers
{
    public class SolarSystemController : BaseController
    {
        public ActionResult Index(int? page)
        {
            page = page ?? 0;
            var view = new SolarSystemIndexView();
            using (var session = DB.Instance.GetSession())
            {
                RavenQueryStatistics stats = null;
                view.SolarSystems = session.Query<SolarSystem>()
                    .Statistics(out stats)
                    .OrderBy(x => x.Name)
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
                    session.SaveChanges();
                }
            }

            if (id.HasValue)
            {
                return RedirectToAction("View", "SolarSystem", new { id = id.Value });
            }
            return RedirectToAction("Edit", "SolarSystem", new { id = input.Id });
        }

        #region Client requests
        [HttpPost]
        public ActionResult SaveStation(int id, Station station)
        {
            using (var session = DB.Instance.GetSession())
            {
                session.Advanced.UseOptimisticConcurrency = true;
                station.Faction = FactionRef.FromFaction(session.Load<Faction>(station.Faction.Id));
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
        public ActionResult SaveStatus(int id, SolarSystemStatus status)
        {
            //Convert date to UTC
            status.Date = status.Date.ToUniversalTime();
            //Update faction refs
            var factionIds = status.FactionStatus.Select(x => x.Faction.Id).Cast<System.ValueType>();
            using (var session = DB.Instance.GetSession())
            {
                var factions = session.Load<Faction>(factionIds);
                foreach (var faction in status.FactionStatus)
                {
                    faction.Faction = FactionRef.FromFaction(factions.First(x => x.Id == faction.Faction.Id));
                }
                session.Store(status);
                session.SaveChanges();
            }
            return new JsonResult { Data = new { status = "OK" } };
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
        #endregion
    }
}