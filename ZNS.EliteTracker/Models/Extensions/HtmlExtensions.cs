using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using CodeKicker.BBCode;
using ZNS.EliteTracker.Models.Documents;

namespace ZNS.EliteTracker.Models.Extensions
{
    public static class HtmlExtensions
    {
        public static readonly BBCodeParser DefaultBBCodeParser = new BBCodeParser(new[]
        {
            new BBTag("b", "<b>", "</b>"), 
            new BBTag("i", "<span style=\"font-style:italic;\">", "</span>"), 
            new BBTag("u", "<span style=\"text-decoration:underline;\">", "</span>"),
            new BBTag("s", "<span style=\"text-decoration:line-through;\">", "</span>"),
            new BBTag("img", "<img src=\"${content}\" alt=\"\" />", "", false, true),
            new BBTag("quote", "<blockquote>", "</blockquote>"),
            new BBTag("url", "<a href=\"${href}\">", "</a>", new BBAttribute("href", ""), new BBAttribute("href", "href")), 
            new BBTag("br", "<br/>", "", true, false),
            new BBTag("size", "<span style=\"font-size:${size}%\">", "</span>", new BBAttribute("size", ""), new BBAttribute("size", "size")),
            new BBTag("video", "<div class=\"embed-responsive embed-responsive-16by9\"><iframe class=\"embed-responsive-item\" width=\"560\" height=\"315\" src=\"https://www.youtube.com/embed/${content}\" frameborder=\"0\" allowfullscreen></iframe></div>", "", false, true),
        });

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
            bbcode = bbcode.Replace("\n", "[br]");
            return html.Raw(DefaultBBCodeParser.ToHtml(bbcode));
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