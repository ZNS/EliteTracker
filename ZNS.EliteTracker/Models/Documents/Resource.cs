using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ZNS.EliteTracker.Models.Documents
{
    public class Resource : BaseDocument, ICommentable
    {
        public CommanderRef Owner { get; set; }
        public string Heading { get; set; }
        public string Body { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastEdit { get; set; }
        public List<ResourceEdit> Edits { get; set; }

        public string Name
        {
            get { return Heading; }
        }

        public string IdPrefix
        {
            get { return "Resources"; }
        }

        public Resource()
        {
            Edits = new List<ResourceEdit>();
        }

    }
}