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
    public class TradeController : Controller
    {
        public ActionResult Index(int? page)
        {
            page = page ?? 0;
            var view = new TradeIndexView();
            using (var session = DB.Instance.GetSession())
            {
                RavenQueryStatistics stats = null;
                view.Routes = session.Query<TradeRoute>()
                    .Statistics(out stats)
                    .OrderByDescending(x => x.Name)
                    .Skip(page.Value * 15)
                    .Take(15)
                    .ToList();
                view.Pager = new Pager
                {
                    Count = stats.TotalResults,
                    Page = page.Value,
                    PageSize = 15
                };
            }
            return View(view);
        }

        public ActionResult View(int id)
        {
            using (var session = DB.Instance.GetSession())
            {
                var route = session.Load<TradeRoute>(id);

                //Load systems
                var systemIds = route.Milestones
                    .Select(x => x.From.SolarSystem.Id)
                    .ToList();
                systemIds.AddRange(route.Milestones.Select(x => x.To.SolarSystem.Id));
                var systems = session.Load<SolarSystem>(systemIds.Distinct().Cast<ValueType>());
                foreach (var ms in route.Milestones)
                {
                    ms.From.System = systems.First(x => x.Id == ms.From.SolarSystem.Id);
                    ms.To.System = systems.First(x => x.Id == ms.To.SolarSystem.Id);
                }

                return View(route);
            }
        }

        public ActionResult Edit(int? id)
        {
            TradeEditView view = new TradeEditView();
            using (var session = DB.Instance.GetSession())
            {
                view.SolarSystems = session.Query<SolarSystem>().OrderBy(x => x.Name).Take(512).ToList();
                if (id.HasValue)
                {
                    view.Route = session.Load<TradeRoute>(id);
                }
            }
            return View(view);
        }

        [HttpPost]
        public ActionResult Edit(int? id, TradeEditView input)
        {
            TradeRoute route = null;
            using (var session = DB.Instance.GetSession())
            {
                if (id.HasValue)
                {
                    route = session.Load<TradeRoute>(id);
                    route.Name = input.Route.Name;
                    route.Notes = input.Route.Notes;
                }
                else
                {
                    route = new TradeRoute
                    {
                        Name = input.Route.Name
                    };
                    session.Store(route);
                }
                session.SaveChanges();
            }

            if (id.HasValue)
            {
                return RedirectToAction("View", new { id = route.Id });
            }
            return RedirectToAction("Edit", new { id = route.Id });
        }

        #region Client Requests
        [HttpPost]
        public ActionResult Delete(int id)
        {
            using (var session = DB.Instance.GetSession())
            {
                session.Delete("traderoutes/" + id);
                session.SaveChanges();
            }
            return new JsonResult { Data = new { status = "ok" } };
        }

        public ActionResult GetMilestones(int id)
        {
            using (var session = DB.Instance.GetSession())
            {
                var route = session.Load<TradeRoute>(id);
                return new JsonResult { Data = route.Milestones, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
        }

        [HttpPost]
        public ActionResult SaveMilestone(int id, TradeMilestone milestone)
        {
            using (var session = DB.Instance.GetSession())
            {
                session.Advanced.UseOptimisticConcurrency = true;
                var route = session.Load<TradeRoute>(id);
                if (route != null)
                {
                    if (!String.IsNullOrEmpty(milestone.Guid))
                    {
                        route.Milestones.Remove(route.Milestones.First(x => x.Guid == milestone.Guid));
                    }
                    else
                    {
                        milestone.Guid = Guid.NewGuid().ToString();
                    }

                    //Refs
                    var systems = session.Load<SolarSystem>(new int[] {
                        milestone.From.SolarSystem.Id,
                        milestone.To.SolarSystem.Id                    
                    }.Cast<ValueType>());
                    milestone.From.SolarSystem = SolarSystemRef.FromSolarSystem(systems.First(x => x.Id == milestone.From.SolarSystem.Id));
                    milestone.To.SolarSystem = SolarSystemRef.FromSolarSystem(systems.First(x => x.Id == milestone.To.SolarSystem.Id));

                    route.Milestones.Add(milestone);
                }
                session.SaveChanges();
            }
            return new JsonResult { Data = new { status = "OK" } };
        }
        #endregion

    }
}