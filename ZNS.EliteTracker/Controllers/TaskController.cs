﻿using System;
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
    public class TaskController : BaseController
    {
        public ActionResult Index(int? page)
        {
            page = page ?? 0;
            var view = new TaskIndexView();
            using (var session = DB.Instance.GetSession())
            {
                RavenQueryStatistics stats = null;
                view.Tasks = session.Query<Task>()
                    .Statistics(out stats)
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

        public ActionResult View(int id)
        {
            var view = new TaskViewView();
            using (var session = DB.Instance.GetSession())
            {
                view.Task = session.Load<Task>(id);
                view.Comments = new CommentView()
                {
                    DocumentId = "Tasks/" + id
                };
                view.Comments.Comments = session.Query<Comment>()
                    .Where(x => x.DocumentId == view.Comments.DocumentId)
                    .OrderByDescending(x => x.Date)
                    .Take(15)
                    .ToList();

                return View(view);
            }
        }

        public ActionResult Edit(int? id)
        {
            var view = new TaskEditView();
            using (var session = DB.Instance.GetSession())
            {
                view.Systems = session.Query<SolarSystem>().OrderBy(x => x.Name).ToList();
                if (id.HasValue)
                {
                    view.Task = session.Load<Task>(id);
                    //Check if owner
                    if (view.Task.Owner.Id != CommanderId)
                    {
                        return new HttpUnauthorizedResult("Unauthorized access detected!");
                    }
                }
            }
            return View(view);
        }

        [HttpPost]
        public ActionResult Edit(int? id, TaskEditView input)
        {
            var task = input.Task;
            using (var session = DB.Instance.GetSession())
            {
                //Create solarsystem ref
                if (task.SolarSystem != null && task.SolarSystem.Id != 0)
                {
                    task.SolarSystem = SolarSystemRef.FromSolarSystem(session.Load<SolarSystem>(task.SolarSystem.Id));
                }
                else
                {
                    task.SolarSystem = null;
                }

                if (id.HasValue)
                {
                    var org = session.Load<Task>(id);
                    org.Description = task.Description;
                    org.Heading = task.Heading;
                    org.Priority = task.Priority;
                    org.Status = task.Status;
                    org.Type = task.Type;
                    org.SolarSystem = task.SolarSystem;

                    org.Owner = new CommanderRef
                    {
                        Id = CommanderId,
                        Name = User.Identity.Name
                    };
                    org.Date = DateTime.UtcNow;
                }
                else
                {
                    task.Owner = new CommanderRef {
                        Id = CommanderId,
                        Name = User.Identity.Name
                    };
                    task.Date = DateTime.UtcNow;
                    session.Store(task);
                }
                session.SaveChanges();
            }

            return RedirectToAction("View", new { id = id ?? task.Id });
        }

        [HttpPost]
        public ActionResult Signup(int id)
        {
            using (var session = DB.Instance.GetSession())
            {
                var task = session.Load<Task>(id);
                if (task != null && !task.AssignedCommanders.Any(x => x.Id == CommanderId))
                {
                    task.AssignedCommanders.Add(new CommanderRef
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
        public ActionResult Withdraw(int id)
        {
            using (var session = DB.Instance.GetSession())
            {
                var task = session.Load<Task>(id);
                if (task != null)
                {
                    var cmdr = task.AssignedCommanders.FirstOrDefault(x => x.Id == CommanderId);
                    if (cmdr != null)
                    {
                        task.AssignedCommanders.Remove(cmdr);
                    }
                    session.SaveChanges();
                }
            }
            return new JsonResult { Data = new { status = "ok" } };
        }
    }
}