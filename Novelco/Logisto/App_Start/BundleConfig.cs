using System.Web;
using System.Web.Optimization;

namespace Logisto
{
	public class BundleConfig
	{
		// For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
		public static void RegisterBundles(BundleCollection bundles)
		{
			#region scripts

			bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
						"~/Scripts/jquery-{version}.js"));

			bundles.Add(new ScriptBundle("~/bundles/jquery-ui").Include(
			"~/Scripts/jquery-ui-{version}.js",
			"~/Scripts/datepicker-ru.js"
			));

			bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
						"~/Scripts/jquery.validate*"));

			// Use the development version of Modernizr to develop with and learn from. Then, when you're
			// ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
			bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
						"~/Scripts/modernizr-*"));

			bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
					  "~/Scripts/bootstrap.js",
					  "~/Scripts/respond.js"));


			#region Admin

			bundles.Add(new ScriptBundle("~/bundles/admin").Include(
					"~/Scripts/knockout-{version}.js",
					"~/Scripts/knockout.mapping-latest.js",
					"~/Scripts/Utility.js"
					));

			#endregion

			#region Client

			bundles.Add(new ScriptBundle("~/bundles/client").Include(
					"~/Scripts/knockout-{version}.js",
					"~/Scripts/knockout.mapping-latest.js",
					"~/Scripts/Utility.js"
					));

			#endregion

			#endregion

			#region styles

			bundles.Add(new StyleBundle("~/Content/css/client").Include(
						"~/Content/bootstrap.css",
						"~/Content/style.css"));

			bundles.Add(new StyleBundle("~/Content/css/admin").Include(
						"~/Content/bootstrap.css",
						"~/Areas/Admin/admin.css"));

			#endregion
		}
	}
}
