using System.Web;
using System.Web.Optimization;

namespace PhotosManager
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js",
                        "~/Scripts/autoRefreshPanel.js",
                        "~/Scripts/session.js",
                        "~/Scripts/bootbox-custom.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                       "~/Scripts/jquery-ui.js",
                       "~/Scripts/imageControl-2.0.js",
                       "~/Scripts/selections.js",
                       "~/Scripts/jquery.maskedinput.js",
                       "~/Scripts/SiteScripts.js",
                       "~/Scripts/jquery.validate*"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                     "~/Content/jquery-ui.css",
                     "~/Content/jquery-ui.strucure.css",
                     "~/Content/jquery-ui.theme.css",
                     "~/Content/site.css",
                     "~/Content/popup.css",
                     "~/Content/Accounts.css",
                     "~/Content/Icons.css",
                     "~/Content/photo.css",
                     "~/Content/Selections.css"));
        }
    }
}
