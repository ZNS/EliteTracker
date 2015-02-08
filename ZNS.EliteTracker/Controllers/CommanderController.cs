using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ImageProcessor;
using Raven.Client;
using ZNS.EliteTracker.Models;
using ZNS.EliteTracker.Models.Documents;
using ZNS.EliteTracker.Models.Views;

namespace ZNS.EliteTracker.Controllers
{
    public class CommanderController : BaseController
    {
        // GET: Commander
        public ActionResult Index(int? page)
        {
            page = page ?? 0;
            var view = new CommanderIndexView();
            using (var session = DB.Instance.GetSession())
            {
                RavenQueryStatistics stats = null;
                view.Commanders = session.Query<Commander>()
                    .Statistics(out stats)
                    .OrderBy(x => x.Name)
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
            var view = new CommanderViewView();
            using (var session = DB.Instance.GetSession())
            {
                view.Commander = session.Load<Commander>(id);
                view.Tasks = session.Query<Task>()
                    .Where(x => x.AssignedCommanders.Any(c => c.Id == id) && x.Status != TaskStatus.Completed)
                    .OrderByDescending(x => x.Date)
                    .ToList();
                view.SolarSystems = session.Query<SolarSystem>()
                    .Where(x => x.ActiveCommanders.Any(c => c.Id == id))
                    .OrderBy(x => x.Name)
                    .ToList();
            }
            return View(view);
        }

        public ActionResult Manage(int? id)
        {
            if (!@User.IsInRole("administrator"))
            {
                return new HttpUnauthorizedResult("I'm sorry, " + User.Identity.Name + ". I'm afraid I can't do that. ");
            }
            
            using (var session = DB.Instance.GetSession())
            {
                if (id.HasValue)
                {
                    return View(session.Load<Commander>(id));
                }
                return View(new Commander());
            }
        }

        [HttpPost]
        public ActionResult Manage(int? id, FormCollection form)
        {
            if (!@User.IsInRole("administrator"))
            {
                return new HttpUnauthorizedResult("I'm sorry, " + User.Identity.Name + ". I'm afraid I can't do that. ");
            }

            var commander = new Commander();
            using (var session = DB.Instance.GetSession())
            {
                if (id.HasValue)
                {
                    commander = session.Load<Commander>(id.Value);
                }

                commander.Name = form["Name"].Trim();
                commander.Roles.Clear();
                commander.Roles.Add(form["Role"]);
                commander.Enabled = form["Enabled"].Split(',').Contains("true");
                
                var password = form["pwd"].Trim();
                if (!String.IsNullOrEmpty(password))
                {
                    commander.Salt = Password.GenerateSalt();
                    commander.Password = Password.HashPassword(password, commander.Salt);
                }
                else if (!id.HasValue)
                {
                    throw new Exception("No password entered");
                }

                if (!id.HasValue)
                {
                    commander.Country = new Country
                    {
                        Code = "",
                        Name = "Unknown"
                    };
                    session.Store(commander);
                }
                session.SaveChanges();
            }
            return RedirectToAction("View", new { id = id.HasValue ? id.Value : commander.Id });
        }

        public ActionResult Edit()
        {
            //Countries
            var countries = new List<SelectListItem>();
            foreach (var ci in CultureInfo.GetCultures(CultureTypes.SpecificCultures))
            {
                var info = new RegionInfo(ci.LCID);
                if (!countries.Any(x => x.Value == info.TwoLetterISORegionName))
                {
                    countries.Add(new SelectListItem
                    {
                        Text = info.DisplayName,
                        Value = info.TwoLetterISORegionName
                    });
                }
            }
            ViewBag.Countries = countries.OrderBy(x => x.Text).ToList();

            using (var session = DB.Instance.GetSession())
            {
                var commander = session.Load<Commander>(CommanderId);
                if (commander.Country == null)
                {
                    commander.Country = new Country
                    {
                        Code = "",
                        Name = "Unknown"
                    };
                }
                return View(commander);
            }
        }

        [HttpPost]
        public ActionResult Edit(Commander input, string pwd, HttpPostedFileBase file)
        {
            using (var session = DB.Instance.GetSession())
            {
                var commander = session.Load<Commander>(CommanderId);
                commander.PlayerName = input.PlayerName;
                commander.Story = input.Story;
                //Password
                if (!String.IsNullOrEmpty(pwd))
                {
                    if (pwd.Length < 6)
                    {
                        throw new Exception("Password must be at least 6 characters long");
                    }
                    commander.Salt = Password.GenerateSalt();
                    commander.Password = Password.HashPassword(pwd, commander.Salt);
                }
                //Country
                foreach (var ci in CultureInfo.GetCultures(CultureTypes.SpecificCultures))
                {
                    var info = new RegionInfo(ci.LCID);
                    if (info.TwoLetterISORegionName == input.Country.Code)
                    {
                        commander.Country = new Country
                        {
                            Code = info.TwoLetterISORegionName,
                            Name = info.DisplayName
                        };
                        break;
                    }
                }
                //Image
                if (IsImage(file))
                {
                    var ext = Path.GetExtension(file.FileName);
                    var imagePath = "/App_Data/upload/commander_" + CommanderId + ".jpg";
                    using (file.InputStream)
                    {
                        using (FileStream fs = new FileStream(Server.MapPath(imagePath), FileMode.Create))
                        {
                            using (ImageFactory imageFactory = new ImageFactory())
                            {
                                imageFactory.Load(file.InputStream)
                                    .Quality(60)
                                    .Resize(new ImageProcessor.Imaging.ResizeLayer(
                                        anchorPosition: ImageProcessor.Imaging.AnchorPosition.Center,
                                        resizeMode: ImageProcessor.Imaging.ResizeMode.Crop,
                                        upscale: false,
                                        size: new System.Drawing.Size
                                        {
                                            Width = 360,
                                            Height = 480
                                        }
                                    ))
                                    .Format(new ImageProcessor.Imaging.Formats.JpegFormat())
                                    .Save(fs);
                            }
                        }
                    }
                }
                session.SaveChanges();
            }
            return RedirectToAction("View", new { id = CommanderId });
        }

        public ActionResult Image(int id)
        {
            var imagePath = Server.MapPath("/App_Data/upload/commander_" + id + ".jpg");
            if (!System.IO.File.Exists(imagePath))
            {
                return File(Server.MapPath("/content/images/commander_placeholder.png"), "image/png");
            }
            return File(imagePath, System.Net.Mime.MediaTypeNames.Image.Jpeg);
        }

        private bool IsImage(HttpPostedFileBase postedFile)
        {
            if (postedFile == null)
                return false;
            //-------------------------------------------
            //  Check the image mime types
            //-------------------------------------------
            if (postedFile.ContentType.ToLower() != "image/jpg" &&
                        postedFile.ContentType.ToLower() != "image/jpeg" &&
                        postedFile.ContentType.ToLower() != "image/pjpeg" &&
                        postedFile.ContentType.ToLower() != "image/x-png" &&
                        postedFile.ContentType.ToLower() != "image/png")
            {
                return false;
            }

            //-------------------------------------------
            //  Check the image extension
            //-------------------------------------------
            if (Path.GetExtension(postedFile.FileName).ToLower() != ".jpg"
                && Path.GetExtension(postedFile.FileName).ToLower() != ".png"
                && Path.GetExtension(postedFile.FileName).ToLower() != ".jpeg")
            {
                return false;
            }

            return true;
        }

        #region JS Requests
        public ActionResult GetShips(int id)
        {
            using (var session = DB.Instance.GetSession())
            {
                var commander = session.Load<Commander>(id);
                return new JsonResult
                {
                    Data = commander.Ships ?? new List<Ship>(),
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
        }

        [HttpPost]
        public ActionResult SaveShip(int id, Ship ship)
        {
            using (var session = DB.Instance.GetSession())
            {
                session.Advanced.UseOptimisticConcurrency = true;
                var commander = session.Load<Commander>(id);
                if (commander != null)
                {
                    if (!String.IsNullOrEmpty(ship.Guid))
                    {
                        commander.Ships.Remove(commander.Ships.First(x => x.Guid == ship.Guid));
                    }
                    else
                    {
                        ship.Guid = Guid.NewGuid().ToString();
                    }
                    commander.Ships.Add(ship);
                }
                session.SaveChanges();
            }
            return new JsonResult { Data = new { status = "OK" } };
        }

        [HttpPost]
        public ActionResult RemoveShip(int id, string guid)
        {
            using (var session = DB.Instance.GetSession())
            {
                var commander = session.Load<Commander>(id);
                if (commander != null)
                {
                    var ship = commander.Ships.FirstOrDefault(x => x.Guid == guid);
                    if (ship != null)
                    {
                        commander.Ships.Remove(ship);
                        session.SaveChanges();
                    }
                }
            }
            return new JsonResult { Data = new { status = "OK" } };
        }
        #endregion
    }
}