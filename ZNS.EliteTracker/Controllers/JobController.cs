using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Raven.Abstractions.Data;
using Raven.Abstractions.Smuggler;
using Raven.Database.Smuggler;
using ZNS.EliteTracker.Models;
using ZNS.EliteTracker.Models.Documents;

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

        public ActionResult BackupJson(string key)
        {
            if (!key.Equals(ConfigurationManager.AppSettings["jobkey"], StringComparison.CurrentCulture))
            {
                return new JsonResult { Data = new { status = "unauthorized" }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }

            using (var session = DB.Instance.GetSession())
            {
                BackupDocs<Commander>(false);
                BackupDocs<Faction>(false);
                BackupDocs<SolarSystem>(false);
                BackupDocs<Task>(false);
                BackupDocs<Resource>(false);
                BackupDocs<SolarSystemStatus>(false);
                BackupDocs<TradeRoute>(false);
                BackupDocs<Comment>(false);

                return new JsonResult { Data = new { status = "ok" }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
        }

        private void BackupDocs<T>(bool append, int page = 0) {
            Raven.Client.RavenQueryStatistics stats = null;
            var path = Server.MapPath(ConfigurationManager.AppSettings["backuppath"]).TrimEnd('\\') + "\\" + typeof(T).Name + ".json";

            FileStream fs = null;
            if (!append)
            {
                fs = System.IO.File.Create(path);
            }
            else
            {
                fs = System.IO.File.Open(path, FileMode.Append);
            }

            using (StreamWriter w = new StreamWriter(fs))
            {
                using (var session = DB.Instance.GetSession())
                {
                    var docs = session.Query<T>().Statistics(out stats).Skip(page * 1024).Take(1024).ToList();
                    foreach (var d in docs)
                    {
                        w.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(d));
                    }
                }
            }

            if (stats != null && (stats.TotalResults - ((page + 1) * 1024)) > 0)
            {
                BackupDocs<T>(true, page + 1);
            }
        }
    }
}