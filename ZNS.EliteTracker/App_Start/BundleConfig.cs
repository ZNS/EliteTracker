using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Optimization;
using BundleTransformer.Core.Bundles;
using BundleTransformer.Core.Orderers;

namespace ZNS.EliteTracker
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            var css = new CustomStyleBundle("~/Css/lib");
            css.Include("~/Content/Css/Bootstrap/bootstrap.less");
            css.Include("~/Content/Css/Fontawesome/css/font-awesome.css");
            css.IncludeDirectory("~/Content/Css/Plugins", "*.css");
            css.Include("~/Content/ed3d/css/styles.css");
            css.IncludeDirectory("~/Content/Css", "*.less");
            css.Orderer = new NullOrderer();
            bundles.Add(css);

            var js = new CustomScriptBundle("~/Js/lib");
            js.Include("~/Content/Js/lib/jquery.min.js");
            js.Include("~/Content/Js/lib/angular.min.js");
            js.Include("~/Content/Js/lib/moment.min.js");
            js.Include("~/Content/Js/lib/jquery.wysibb.min.js");
            js.Include("~/Content/Js/lib/ng-tags-input.js");
            js.Include("~/Content/Js/lib/ng-google-chart.js");
            js.Include("~/Content/Js/lib/checklist-model.js");
            js.Include("~/Content/Js/app.js");
            js.IncludeDirectory("~/Content/Js/Controllers", "*.js");
            js.IncludeDirectory("~/Content/Js/Directives", "*.js");
            js.Orderer = new NullOrderer();
            bundles.Add(js);

#if !DEBUG
            BundleTable.EnableOptimizations = true;
#endif
        }
    }
}