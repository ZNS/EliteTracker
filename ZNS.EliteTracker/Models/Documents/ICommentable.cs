using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ZNS.EliteTracker.Models.Documents
{
    public interface ICommentable
    {
        int Id { get; }
        string Name { get; }
        string IdPrefix { get; }
    }
}