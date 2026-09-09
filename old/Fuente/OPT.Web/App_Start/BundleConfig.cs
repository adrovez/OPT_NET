using System.Web;
using System.Web.Optimization;

namespace OPT.Web
{
    public class BundleConfig
    {
        // Para obtener más información sobre Bundles, visite http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                       "~/Scripts/DataTable/datatables.min.js",
                       "~/Scripts/jquery-{version}.js",
                       "~/Scripts/jquery.blockUI.js",
                       "~/Scripts/jquery.number.min.js",
                       "~/Scripts/jquery.Rut.js"));

            //bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
            //            "~/Scripts/jquery.validate*"));

            // Utilice la versión de desarrollo de Modernizr para desarrollar y obtener información. De este modo, estará
            // preparado para la producción y podrá utilizar la herramienta de compilación disponible en http://modernizr.com para seleccionar solo las pruebas que necesite.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/popper.min.js",
                      "~/Scripts/bootstrap.min.js",
                      "~/Scripts/bootstrap-datepicker.min.js",
                      "~/Scripts/locales/bootstrap-datepicker.es.min.js",
                      "~/Scripts/sweetalert.min.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/bootstrap-theme.css",
                      "~/Content/DataTable/datatables.min.css",
                      "~/Content/bootstrap-datepicker.min.css",
                      "~/Content/fontawesome-all.min.css",
                       "~/Content/sweetalert.css",
                      "~/Content/Site.css"));
        }
    }
}
