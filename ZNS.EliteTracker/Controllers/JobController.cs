using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ZNS.EliteTracker.Models;

namespace ZNS.EliteTracker.Controllers
{
    [AllowAnonymous]
    public class JobController : Controller
    {
        public ActionResult Backup(string key)
        {
            var backupPath = Server.MapPath(ConfigurationManager.AppSettings["backuppath"]);
            if (key.Equals(ConfigurationManager.AppSettings["jobkey"]))
            {
                DB.Instance.GetDatabaseCommands()
                    .GlobalAdmin
                    .StartBackup(
                    backupPath,
                    null,
                    false,
                    Raven.Abstractions.Data.Constants.SystemDatabase);
            }
            return new JsonResult { Data = new { status = "ok" }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
    }
}