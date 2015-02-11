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
    public class ResourceController : BaseController
    {
        // GET: Resource
        public ActionResult Index(int? page)
        {
            page = page ?? 0;
            var view = new ResourceIndexView();
            using (var session = DB.Instance.GetSession())
            {
                RavenQueryStatistics stats = null;
                view.Resources = session.Query<Resource>()
                    .Statistics(out stats)
                    .OrderByDescending(x => x.LastEdit)
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
        
        public ActionResult View(int id, int? page)
        {
            page = page ?? 0;
            var view = new ResourceViewView();
            view.Comments = new CommentView
            {
                DocumentId = "Resources/" + id,
            };

            using (var session = DB.Instance.GetSession())
            {
                RavenQueryStatistics stats = null;
                var comments = session.Query<Comment>()
                        .Statistics(out stats)
                        .Where(x => x.DocumentId == view.Comments.DocumentId)
                        .OrderByDescending(x => x.Date)
                        .Skip(page.Value * 15)
                        .Take(15)
                        .ToList();
                view.Resource = session.Load<Resource>(id);
                view.Comments.Comments = comments;
                view.Comments.Pager = new Pager
                {
                    Count = stats.TotalResults,
                    Page = page.Value,
                    PageSize = 15
                };
            }
            return View(view);
        }

        public ActionResult Edit(int? id)
        {
            var resource = new Resource();
            using (var session = DB.Instance.GetSession())
            {
                if (id.HasValue)
                {
                    resource = session.Load<Resource>(id);
                }
            }
            return View(resource);
        }

        [HttpPost]
        public ActionResult Edit(int? id, Resource resource, string reason)
        {
            using (var session = DB.Instance.GetSession())
            {
                if (id.HasValue)
                {
                    var oldResource = session.Load<Resource>(id);
                    oldResource.Heading = resource.Heading;
                    oldResource.Body = resource.Body;
                    oldResource.LastEdit = DateTime.UtcNow;
                    oldResource.Edits.Add(new ResourceEdit
                    {
                        Commander = new CommanderRef
                        {
                            Id = CommanderId,
                            Name = User.Identity.Name
                        },
                        Comment = reason,
                        Date = oldResource.LastEdit
                    });
                }
                else
                {
                    resource.Owner = new CommanderRef {
                            Id = CommanderId,
                            Name = User.Identity.Name
                        };
                    resource.Created = DateTime.UtcNow;
                    resource.LastEdit = resource.Created;
                    session.Store(resource);
                }
                session.SaveChanges();
            }
            return RedirectToAction("View", new { id = id.HasValue ? id : resource.Id });
        }
    }
}