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
    public class FactionController : BaseController
    {
        public ActionResult Index(int? page)
        {
            page = page ?? 0;
            var view = new FactionIndexView();
            using (var session = DB.Instance.GetSession())
            {
                RavenQueryStatistics stats = null;
                view.Factions = session.Query<Faction>()
                    .Statistics(out stats)
                    .OrderBy(x => x.HomeSolarSystem.Name)
                    .ThenBy(x => x.Name)
                    .Skip(page.Value * 25)
                    .Take(25)
                    .ToList();
                view.Pager = new Pager
                {
                    Count = stats.TotalResults,
                    Page = page.Value,
                    PageSize = 25
                };
            }
            return View(view);
        }       

        public ActionResult Edit(int? id)
        {            
            var view = new FactionEditView();
            using (var session = DB.Instance.GetSession())
            {
                if (id.HasValue)
                {
                    view.Faction = session.Load<Faction>(id);
                }
                view.Systems = session.Query<SolarSystem>().OrderBy(x => x.Name).ToList();
            }
            return View(view);
        }

        [HttpPost]
        public ActionResult Edit(int? id, FactionEditView input)
        {
            var faction = input.Faction;
            using (var session = DB.Instance.GetSession())
            {
                if (id.HasValue)
                {
                    faction = session.Load<Faction>(id.Value);
                    faction.Name = input.Faction.Name;
                    faction.Government = input.Faction.Government;
                    faction.Attitude = input.Faction.Attitude;
                    faction.Allegiance = input.Faction.Allegiance;
                }
                
                //Solar systems
                SolarSystem homeSystem = null;
                if (input.PostedSystems.Count > 0)
                {
                    var systemIds = input.PostedSystems.Select(x => int.Parse(x)).Cast<System.ValueType>();
                    var systems = session.Load<SolarSystem>(systemIds);
                    faction.SolarSystems = systems.Select(x => SolarSystemRef.FromSolarSystem(x)).ToList();
                    homeSystem = systems.FirstOrDefault(x => x.Id == input.Faction.HomeSolarSystem.Id);
                }
                else
                {
                    faction.SolarSystems.Clear();
                }

                if (homeSystem == null)
                {
                    homeSystem = session.Load<SolarSystem>(input.Faction.HomeSolarSystem.Id);
                }
                faction.HomeSolarSystem = SolarSystemRef.FromSolarSystem(homeSystem);

                if (!id.HasValue)
                {
                    session.Store(faction);
                }

                session.SaveChanges();
            }
            return RedirectToAction("Edit", new { id = faction.Id, status = "saved" });
        }
    }
}