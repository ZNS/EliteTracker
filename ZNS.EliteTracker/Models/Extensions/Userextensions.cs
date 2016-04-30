using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;

namespace ZNS.EliteTracker.Models.Extensions
{
    public static class Userextensions
    {
        public static bool IsAnyRole(this IPrincipal user, string roles)
        {
            foreach (var role in roles.Split(','))
            {
                if (user.IsInRole(role))
                    return true;
            }
            return false;
        }
    }
}