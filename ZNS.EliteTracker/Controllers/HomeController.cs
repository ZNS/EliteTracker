using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ZNS.EliteTracker.Models;
using ZNS.EliteTracker.Models.Documents;
using ZNS.EliteTracker.Models.Views;

namespace ZNS.EliteTracker.Controllers
{
    public class HomeController : BaseController
    {
        // GET: Home
        public ActionResult Index()
        {
            var view = new HomeView();
            using (var session = DB.Instance.GetSession())
            {
                //Latest comments
                view.Comments = session.Advanced.DocumentQuery<Comment>()
                    .Include("DocumentId")
                    .OrderByDescending(x => x.Date)
                    .Take(10)
                    .ToList();
                foreach (var comment in view.Comments)
                {
                    comment.Entity = session.Load<ICommentable>(comment.DocumentId);
                }
                //Systems
                view.SolarSystems = session.Query<SolarSystem>()
                    .Where(x => x.ActiveCommanders.Any(c => c.Id == CommanderId))
                    .OrderBy(x => x.Name)
                    .ToList();
                //Tasks
                view.MyTasks = session.Query<Task>()
                    .Where(x => x.AssignedCommanders.Any(c => c.Id == CommanderId))
                    .OrderByDescending(x => x.Priority)
                    .ThenByDescending(x => x.Date)
                    .ToList();
                view.NewTasks = session.Query<Task>()
                    .Where(x => x.Date >= DateTime.UtcNow.AddDays(-7))
                    .OrderByDescending(x => x.Priority)
                    .ThenByDescending(x => x.Date)
                    .ToList();
            }
            return View(view);
        }
    }
}