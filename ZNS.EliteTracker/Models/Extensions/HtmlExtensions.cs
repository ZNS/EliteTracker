using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using ZNS.EliteTracker.Models.Documents;

namespace ZNS.EliteTracker.Models.Extensions
{
    public static class HtmlExtensions
    {
        public static IHtmlString EnumDropDown(this System.Web.Mvc.HtmlHelper html, string name, Type tEnum, object htmlAttributes = null)
        {
            var values = Enum.GetValues(tEnum).Cast<Object>();
            var items = values.Select(x => new SelectListItem
            {
                Text = Enum.GetName(tEnum, x),
                Value = ((int)x).ToString()
            });
            return html.DropDownList(name, items, htmlAttributes);
        }

        public static IHtmlString RenderBBCode(this System.Web.Mvc.HtmlHelper html, string bbcode)
        {
            return html.Raw(CodeKicker.BBCode.BBCode.ToHtml(bbcode));
        }

        public static IHtmlString TaskPriorityLabel(this System.Web.Mvc.HtmlHelper html, TaskPriority prio, string cssClass = "")
        {
            string labelClass = "label-success";
            switch (prio)
            {
                case TaskPriority.Low:
                    labelClass = "label-info";
                    break;
                case TaskPriority.High:
                    labelClass = "label-warning";
                    break;
                case TaskPriority.Critical:
                    labelClass = "label-danger";
                    break;
            }
            return html.Raw(@"<span class=""label " + labelClass + " " + cssClass + @""">" + prio.ToString() + "</span>");
        }
    }
}