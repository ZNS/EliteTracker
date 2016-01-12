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
    public class CommentController : BaseController
    {
        public ActionResult Index(int? page)
        {
            if (!User.IsInRole("administrator"))
            {
                return new HttpUnauthorizedResult("Unauthorized access detected...");
            }

            page = page ?? 0;
            var view = new CommentIndexView();
            using (var session = DB.Instance.GetSession())
            {
                RavenQueryStatistics stats = null;
                view.Comments = session.Query<Comment>()
                    .Statistics(out stats)
                    .OrderByDescending(x => x.Date)
                    .Skip(page.Value * 25)
                    .Take(25)
                    .ToList();
                view.Pager = new Pager
                {
                    Count = stats.TotalResults,
                    Page = page.Value,
                    PageSize = 25
                };
                return View(view);
            }
        }

        public ActionResult Get(int id)
        {
            Comment comment = null;
            using (var session = DB.Instance.GetSession())
            {
                comment = session.Load<Comment>(id);
            }
            return new JsonResult { Data = comment, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [HttpPost]
        public ActionResult Post(int? id, Comment comment)
        {
            if (!id.HasValue)
            {
                comment.Date = DateTime.UtcNow;
                comment.Commander = new CommanderRef
                {
                    Id = CommanderId,
                    Name = User.Identity.Name
                };
                using (var session = DB.Instance.GetSession())
                {
                    session.Store(comment);
                    session.SaveChanges();
                }
            }
            else
            {
                using (var session = DB.Instance.GetSession())
                {
                    var c = session.Load<Comment>(id.Value);
                    if (c.Commander.Id == CommanderId)
                    {
                        c.Body = comment.Body;
                    }
                    session.SaveChanges();
                }
            }
            return new JsonResult { Data = new { status = "ok" } };
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            if (!User.IsInRole("administrator"))
            {
                return new HttpUnauthorizedResult("Unauthorized access detected...");
            }

            using (var session = DB.Instance.GetSession())
            {
                var comment = session.Load<Comment>(id);
                if (comment != null)
                {
                    session.Delete<Comment>(comment);
                    session.SaveChanges();
                }
            }
            return new JsonResult { Data = new { status = "ok" } };
        }
    }
}