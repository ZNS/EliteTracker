using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ZNS.EliteTracker.Models.Documents
{
    public class CommanderRef
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public static CommanderRef FromCommander(Commander cmdr)
        {
            return new CommanderRef
            {
                Id = cmdr.Id,
                Name = cmdr.Name
            };
        }
    }
}