using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ZNS.EliteTracker.Models;
using ZNS.EliteTracker.Models.Documents;

namespace ZNS.EliteTracker.Controllers
{
    public class CommentController : BaseController
    {
        [HttpPost]
        public ActionResult Post(Comment comment)
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
            return new JsonResult { Data = new { status = "ok" } };
        }
    }
}