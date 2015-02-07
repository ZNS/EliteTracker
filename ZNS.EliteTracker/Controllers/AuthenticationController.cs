using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using ZNS.EliteTracker.Models;
using ZNS.EliteTracker.Models.Documents;
using ZNS.EliteTracker.Models.Views;

namespace ZNS.EliteTracker.Controllers
{
    public class AuthenticationController : Controller
    {
        IAuthenticationManager Authentication
        {
            get { return HttpContext.GetOwinContext().Authentication; }
        }

        [Route("login")]
        [AllowAnonymous]
        public ActionResult Login()
        {
            return View();
        }

        [Route("login"), HttpPost]
        [AllowAnonymous]
        public ActionResult Login(LoginView input, string returnUrl)
        {
            bool authenticated = false;
            if (!String.IsNullOrEmpty(input.CommanderName) && !String.IsNullOrEmpty(input.Password))
            {
                Commander commander = null;
                using (var session = DB.Instance.GetSession())
                {
                    #region Installation
                    if (ConfigurationManager.AppSettings["installation"] != null && ConfigurationManager.AppSettings["installation"] == "1")
                    {
                        //Check so that there are no existing users
                        if (session.Query<Commander>().ToList().Count == 0)
                        {
                            var cmdr = new Commander
                            {
                                Name = input.CommanderName,
                                Enabled = true,
                                Roles = new List<string>() { "administrator" }
                            };
                            cmdr.Salt = Password.GenerateSalt();
                            cmdr.Password = Password.HashPassword(input.Password, cmdr.Salt);
                            session.Store(cmdr);
                            session.SaveChanges();
                        }
                    }
                    #endregion
                    commander = session.Query<Commander>().Where(x => x.Name == input.CommanderName && x.Enabled).FirstOrDefault();
                    if (commander != null)
                    {
                        authenticated = Password.Authenticate(input.Password, commander.Password, commander.Salt);
                    }
                }

                if (authenticated)
                {
                    var identity = new ClaimsIdentity(
                        new[] {
                        new Claim(ClaimTypes.Name, commander.Name),
                        new Claim(ClaimTypes.NameIdentifier, commander.Id.ToString())
                        },
                        DefaultAuthenticationTypes.ApplicationCookie,
                        ClaimTypes.Name, ClaimTypes.Role);

                    // if you want roles, just add as many as you want here (for loop maybe?)
                    foreach (var role in commander.Roles)
                    {
                        identity.AddClaim(new Claim(ClaimTypes.Role, role));
                    }

                    Authentication.SignIn(new AuthenticationProperties
                    {
                        IsPersistent = input.Persist
                    }, identity);

                    if (!String.IsNullOrEmpty(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    return RedirectToAction("index", "home");
                }
            }
            return RedirectToAction("login", new { status = "failed" });
        }
    }
}