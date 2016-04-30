using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;

namespace ZNS.EliteTracker.Controllers
{
    public class BaseController : Controller
    {
        public int CommanderId { get; set; }
        public List<int> CommanderSystemGroups { get; set; }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (User.Identity.IsAuthenticated)
            {
                var identity = ((System.Security.Claims.ClaimsIdentity)User.Identity);
                ViewBag.CommanderName = identity.Name;
                CommanderId = int.Parse(identity.FindFirst(ClaimTypes.NameIdentifier).Value);
                ViewBag.CommanderId = CommanderId;
                var systemGroups = identity.FindFirst(ClaimTypes.GroupSid).Value;
                if (!String.IsNullOrEmpty(systemGroups))
                {
                    CommanderSystemGroups = systemGroups.Split(',').Select(x => int.Parse(x)).ToList();
                }
                else
                {
                    CommanderSystemGroups = new List<int>();
                }
            }
            base.OnActionExecuting(filterContext);
        }
    }
}