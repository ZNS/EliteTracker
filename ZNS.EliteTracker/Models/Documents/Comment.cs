using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ZNS.EliteTracker.Models.Documents
{
    public class Comment : BaseDocument
    {
        public string DocumentId { get; set; }
        public CommanderRef Commander { get; set; }
        public string Body { get; set; }
        public DateTime Date { get; set; }
        [Raven.Imports.Newtonsoft.Json.JsonIgnore]
        public ICommentable Entity { get; set; }
    }
}