using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ZNS.EliteTracker.Models.Documents;

namespace ZNS.EliteTracker.Models.Views
{
    public class CommentView
    {
        public string DocumentId { get; set; }
        public List<Comment> Comments { get; set; }
    }
}