using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using Raven.Client;
using Raven.Client.Linq;
using ZNS.EliteTracker.Models;
using ZNS.EliteTracker.Models.EDDB;
using ZNS.EliteTracker.Models.Indexes;
using ZNS.EliteTracker.Models.Views;

namespace ZNS.EliteTracker.Controllers
{
    public class DBController : Controller
    {
        // GET: DB
        public ActionResult Index(DBIndexView.Form form)
        {
            var view = new DBIndexView();

            if (form.Ly.HasValue && form.Ly.Value > 50)
            {
                form.Ly = 50;
            }

            if (!String.IsNullOrEmpty(form.System))
            {
                using (var session = DB.Instance.GetSession())
                {
                    //Get systems within distance
                    if (form.Ly.HasValue && form.Ly.Value > 0)
                    {
                        List<EDSMSystem> systems_in_distance = new List<EDSMSystem>();
                        using (WebClient wc = new WebClient())
                        {
                            var json = wc.DownloadString(String.Format("http://www.edsm.net/api-v1/sphere-systems?sysname={0}&radius={1}&showid=1", HttpUtility.UrlEncode(form.System), form.Ly));
                            if (json != null)
                            {
                                systems_in_distance = JsonConvert.DeserializeObject<List<EDSMSystem>>(json);
                            }
                        }
                        if (systems_in_distance.Count > 200)
                        {
                            //Split to multiple queries
                            view.Result = new List<EDDBSystem>();
                            var batches = Math.Ceiling((double)systems_in_distance.Count / 100d);
                            for (var i = 0; i < batches; i++)
                            {
                                var query = session.Query<EDDB_Query.Result, EDDB_Query>()
                                    .Where(x => x.EDSMId.In<int>(systems_in_distance.Select(y => y.Id).Skip(100 * i).Take(100)));
                                view.Result.AddRange(query.OfType<EDDBSystem>().ToList());
                            }
                        }
                        else
                        {
                            var query = session.Query<EDDB_Query.Result, EDDB_Query>()
                                .Where(x => x.EDSMId.In<int>(systems_in_distance.Select(y => y.Id)));
                            view.Result = query.OfType<EDDBSystem>().ToList();
                        }
                    }
                    else
                    {
                        var query = session.Query<EDDB_Query.Result, EDDB_Query>()
                            .Where(x => x.Name == form.System || x.NamePartial == form.System);
                        view.Result = query.OfType<EDDBSystem>().ToList();
                    }

                    //Set up view
                    view.Query = form;

                    //Calculate distances
                    if (form.Ly.HasValue && form.Ly.Value > 0)
                    {
                        var origin = view.Result.FirstOrDefault(x => x.Name.Equals(form.System, StringComparison.InvariantCultureIgnoreCase));
                        foreach (var system in view.Result)
                        {
                            system.Distance = Math.Round(Math.Sqrt(
                            Math.Pow(((double)system.X - (double)origin.X), 2) +
                            Math.Pow(((double)system.Y - (double)origin.Y), 2) +
                            Math.Pow(((double)system.Z - (double)origin.Z), 2)), 2);
                        }

                        //Sort
                        view.Result = view.Result.OrderBy(x => x.Distance).ToList();
                    }
                }
            }
            return View(view);
        }

        public ActionResult GetMostValuable(string system)
        {
            List<dynamic> result = new List<dynamic>();

            List<EDSMSystem> systems_to_test = new List<EDSMSystem>();
            using (WebClient wc = new WebClient())
            {
                var json = wc.DownloadString(String.Format("http://www.edsm.net/api-v1/sphere-systems?sysname={0}&radius={1}&showid=1", HttpUtility.UrlEncode(system), 50));
                if (json != null)
                {
                    systems_to_test = JsonConvert.DeserializeObject<List<EDSMSystem>>(json);
                }
            }

            if (systems_to_test != null && systems_to_test.Count > 0)
            {
                foreach (var current_test in systems_to_test)
                {
                    using (var session = DB.Instance.GetSession())
                    {
                        //Check if populated
                        var current_system = session.Query<EDDB_Query.Result, EDDB_Query>().Where(x => x.EDSMId == current_test.Id).OfType<EDDBSystem>().FirstOrDefault();
                        if (current_system == null || !current_system.Population.HasValue || current_system.Population.Value <= 0)
                            continue;

                        //Get systems within 15ly
                        List<EDSMSystem> systems_cc = new List<EDSMSystem>();
                        using (WebClient wc = new WebClient())
                        {
                            var json = wc.DownloadString(String.Format("http://www.edsm.net/api-v1/sphere-systems?sysname={0}&radius={1}&showid=1", HttpUtility.UrlEncode(current_test.Name), 15));
                            if (json != null)
                            {
                                systems_cc = JsonConvert.DeserializeObject<List<EDSMSystem>>(json);
                            }
                        }

                        if (systems_cc != null)
                        {
                            var cc_systems = session.Query<EDDB_Query.Result, EDDB_Query>()
                                .Where(x => x.EDSMId.In<int>(systems_cc.Select(y => y.Id)))
                                .OfType<EDDBSystem>()
                                .ToList();
                            var total_cc = cc_systems.Where(x => String.IsNullOrEmpty(x.Power)).Sum(x => x.CCIncome);
                            var contested = cc_systems.Where(x => !String.IsNullOrEmpty(x.Power)).Select(x => x.Name).ToArray();
                            result.Add(new { Name = current_test.Name, Income = total_cc, ContestedSystems = contested });
                        }
                    }
                    System.Threading.Thread.Sleep(5000);
                }
            }

            return new JsonResult { Data = result, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public ActionResult GetSystem(string name)
        {
            using (var session = DB.Instance.GetSession())
            {
                var system = session.Query<EDDB_Query.Result, EDDB_Query>().Where(x => x.Name == name).OfType<EDDBSystem>().ToList().FirstOrDefault();
                if (system != null)
                {
                    return new ContentResult { Content = JsonConvert.SerializeObject(system), ContentType = "application/json" };
                }
            }
            return new JsonResult { Data = null, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
    }
}