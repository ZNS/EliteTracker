using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Raven.Abstractions.Data;
using Raven.Abstractions.Smuggler;
using Raven.Database.Smuggler;
using ZNS.EliteTracker.Models;

namespace ZNS.EliteTracker.Controllers
{
    [AllowAnonymous]
    public class JobController : Controller
    {
        public ActionResult Backup(string key)
        {
            if (!key.Equals(ConfigurationManager.AppSettings["jobkey"], StringComparison.CurrentCulture))
            {
                return new JsonResult { Data = new { status = "unauthorized" }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }

            var backupPath = Server.MapPath(ConfigurationManager.AppSettings["backuppath"]).TrimEnd('\\') + "\\dump.raven";
            
            //Delete old
            if (System.IO.File.Exists(backupPath))
            {
                System.IO.File.Delete(backupPath);
            }

            var dumper = new DatabaseDataDumper(DB.Instance.Store.DocumentDatabase, new SmugglerDatabaseOptions
            {
                OperateOnTypes = ItemType.Documents,
                Incremental = false                
            });
            
            dumper.ExportData(new SmugglerExportOptions<RavenConnectionStringOptions>
	        {
		        From = new EmbeddedRavenConnectionStringOptions(),                
		        ToFile = backupPath
	        });

            return new JsonResult { Data = new { status = "ok" }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
    }
}