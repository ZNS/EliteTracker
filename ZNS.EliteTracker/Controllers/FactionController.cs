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
    public class FactionController : BaseController
    {
        public ActionResult Index(int? page, FactionIndexView.Form form)
        {
            page = page ?? 0;
            var view = new FactionIndexView();
            using (var session = DB.Instance.GetSession())
            {
                RavenQueryStatistics stats = null;
                var query = session.Query<Faction_Query.Result, Faction_Query>()
                    .Statistics(out stats)
                    .OrderBy(x => x.HomeSystem)
                    .ThenBy(x => x.Name)
                    .Skip(page.Value * 25)
                    .Take(25);
                if (!String.IsNullOrEmpty(form.Query))
                {
                    query = query.Where(x => x.Name == form.Query || x.NamePartial == form.Query);
                }
                if (form.Attitude != 0)
                {
                    var enumAttitude = (FactionAttitude)Enum.Parse(typeof(FactionAttitude), form.Attitude.ToString());
                    query = query.Where(x => x.Attitude == enumAttitude);
                }
                if (form.State != 0)
                {
                    var enumState = (FactionState)Enum.Parse(typeof(FactionState), form.State.ToString());
                    query = query.Where(x => x.State == enumState);
                }

                view.Factions = query.OfType<Faction>().ToList();
                view.Query = form;
                view.Pager = new Pager
                {
                    Count = stats.TotalResults,
                    Page = page.Value,
                    PageSize = 25
                };
            }
            return View(view);
        }       

        public ActionResult Edit(int? id, string status)
        {            
            var view = new FactionEditView();
            using (var session = DB.Instance.GetSession())
            {
                if (id.HasValue)
                {
                    view.Faction = session.Load<Faction>(id);
                }
                view.Systems = session.Query<SolarSystem>().OrderBy(x => x.Name).Take(512).ToList();
            }
            if (!String.IsNullOrEmpty(status))
            {
                view.ErrorStatus = status;
            }
            return View(view);
        }

        [HttpPost]
        public ActionResult Edit(int? id, FactionEditView input)
        {
            var faction = input.Faction;
            List<SolarSystem> postedSystems = new List<SolarSystem>();
            List<SolarSystem> oldSystems = null;
            using (var session = DB.Instance.GetSession())
            {
                //Check if faction exists?
                if (!id.HasValue)
                {
                    var existing = session.Query<Faction>().Where(x => x.Name == input.Faction.Name).ToList().FirstOrDefault(x => x.Name.Equals(input.Faction.Name, StringComparison.CurrentCultureIgnoreCase));
                    if (existing != null)
                    {
                        return RedirectToAction("Edit", new { status = "Faction already exists" });
                    }
                }

                if (id.HasValue)
                {
                    faction = session.Load<Faction>(id.Value);
                    oldSystems = session.Load<SolarSystem>(faction.SolarSystems.Select(x => x.Id).Cast<ValueType>()).ToList();
                    faction.Name = input.Faction.Name;
                    faction.Government = input.Faction.Government;
                    faction.Attitude = input.Faction.Attitude;
                    faction.Allegiance = input.Faction.Allegiance;
                }
                
                //Solar systems
                SolarSystem homeSystem = null;
                if (input.PostedSystems != null && input.PostedSystems.Count > 0)
                {
                    var systemIds = input.PostedSystems.Select(x => int.Parse(x)).Cast<System.ValueType>();
                    postedSystems = session.Load<SolarSystem>(systemIds).ToList();
                    faction.SolarSystems = postedSystems.Select(x => SolarSystemRef.FromSolarSystem(x)).ToList();
                    //Try to set homesystem by checking posted systems
                    homeSystem = postedSystems.FirstOrDefault(x => x.Id == input.Faction.HomeSolarSystem.Id);
                }
                else
                {
                    faction.SolarSystems.Clear();
                }

                //If homesystem was not among posted systems
                if (homeSystem == null)
                {
                    homeSystem = session.Load<SolarSystem>(input.Faction.HomeSolarSystem.Id);
                    postedSystems.Add(homeSystem);
                    //Make sure it's added to posted systems
                    if (!faction.SolarSystems.Any(x => x.Id == homeSystem.Id))
                    {
                        faction.SolarSystems.Add(SolarSystemRef.FromSolarSystem(homeSystem));
                    }
                }
                //Create ref
                faction.HomeSolarSystem = SolarSystemRef.FromSolarSystem(homeSystem);

                //Store faction, attaches id
                if (!id.HasValue)
                {
                    session.Store(faction);
                }

                //Remove faction from removed systems
                if (oldSystems != null)
                {
                    foreach (var system in oldSystems.Except(postedSystems))
                    {
                        system.Factions.RemoveAll(x => x.Id == faction.Id);
                    }
                }
                //Add allied factions to current systems
                foreach (var system in postedSystems)
                {
                    if (!system.Factions.Any(x => x.Id == faction.Id))
                    {
                        system.Factions.Add(FactionRef.FromFaction(faction));
                    }
                }

                session.SaveChanges();
            }
            return RedirectToAction("Edit", new { id = faction.Id, status = "saved" });
        }
    }
}