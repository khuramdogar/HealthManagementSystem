using System.Web.Optimization;

namespace HMS.HealthTrack.Web
{
   public class BundleConfig
   {
      // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
      public static void RegisterBundles(BundleCollection bundles)
      {
         bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
            "~/Scripts/jquery-{version}.js",
            "~/Scripts/AjaxSetup.js"));

         // Use the development version of Modernizr to develop with and learn from. Then, when you're
         // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
         bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
            "~/Scripts/modernizr-*"));

         bundles.Add(new ScriptBundle("~/bundles/kendo").Include(
            "~/Scripts/kendo/2018.2.516/kendo.all.min.js",
            "~/Scripts/kendo/kendo.culture.en-AU.min.js"));

         bundles.Add(new ScriptBundle("~/bundles/angularjs").Include(
            "~/Scripts/kendo/2018.2.516/angular.min.js"));

         bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
            "~/Scripts/bootstrap.js",
            "~/Scripts/respond.js",
            "~/Scripts/jquery-ui-1.10.4.js",
            "~/Scripts/DataTables-1.10.0/jquery.dataTables.js",
            "~/Scripts/DataTables-1.10.0/dataTables.bootstrap.js"));

         bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
            "~/Scripts/jquery.unobtrusive*",
            "~/Scripts/jquery.validate*",
            "~/Scripts/datepicker.js"));

         bundles.Add(new ScriptBundle("~/bundles/datetimepicker").Include(
            "~/Scripts/jquery-ui-timepicker-addon.min.js",
            "~/Scripts/moment.min.js"
            ));

         bundles.Add(new StyleBundle("~/Content/css").Include(
            "~/Content/bootstrap.css",
            "~/Content/site.css",
            "~/Content/themes/base/jquery-ui.css",
            "~/Content/jquery-ui-timepicker-addon.min.css",
            "~/Content/InventoryToolbar.css"));

         bundles.Add(new StyleBundle("~/Content/kendo").Include(
            "~/Content/kendo/2018.2.516/kendo.bootstrap.min.css",
            "~/Content/kendo/2018.2.516/kendo.common.min.css",
            "~/Content/kendo/2018.2.516/kendo.default.min.css",
            "~/Content/BootstrapFix.css"));


         // Angular bundles
         bundles.Add(new ScriptBundle("~/bundles/Angular")
            .Include(
               "~/bundles/AngularOutput/inline.*",
               "~/bundles/AngularOutput/polyfills.*",
               "~/bundles/AngularOutput/scripts.*",
               "~/bundles/AngularOutput/vendor.*",
               "~/bundles/AngularOutput/runtime.*",
               "~/bundles/AngularOutput/main.*"));

         bundles.Add(new StyleBundle("~/Content/Angular")
            .Include("~/bundles/AngularOutput/styles.*"));
      }
   }
}