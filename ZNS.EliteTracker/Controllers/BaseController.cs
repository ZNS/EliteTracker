using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ZNS.EliteTracker.Controllers
{
    public class BaseController : Controller
    {
        public int CommanderId { get; set; }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (User.Identity.IsAuthenticated)
            {
                var identity = ((System.Security.Claims.ClaimsIdentity)User.Identity);
                ViewBag.CommanderName = identity.Name;
                CommanderId = int.Parse(identity.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").Value);
                ViewBag.CommanderId = CommanderId;
            }
            base.OnActionExecuting(filterContext);
        }
    }
}