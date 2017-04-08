using Logisto.Model;
using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Logisto
{
	public class MvcApplication : HttpApplication
	{
		SyncScheduler sheduler = null;

		protected void Application_Start()
		{
			AreaRegistration.RegisterAllAreas();
			FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
			RouteConfig.RegisterRoutes(RouteTable.Routes);
			BundleConfig.RegisterBundles(BundleTable.Bundles);

			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
			sheduler = new SyncScheduler();
			BackgroundWorker worker = new BackgroundWorker();
			worker.DoWork += Worker_DoWork;
			worker.RunWorkerAsync(sheduler);
		}

		private void Worker_DoWork(object sender, DoWorkEventArgs e)
		{
#if !DEBUG
			((SyncScheduler)e.Argument).Start();
#endif
		}

		protected void Application_Error(object sender, EventArgs e)
		{
			//Последнее исключение
			var exception = Server.GetLastError();

			//Игнорируем HttpAntiForgeryException
			if (exception is HttpAntiForgeryException)
			{
				Response.Clear();
				Response.StatusCode = (int)HttpStatusCode.BadRequest;
				Response.End();
				return;
			}

			try
			{
				string filename = HttpContext.Current.Server.MapPath("~/Temp/" + DateTime.UtcNow.ToString("yyyy-MM-dd_HHmmss") + ".log");
				using (var writer = new StreamWriter(filename, true))
				{
					writer.WriteLine(exception != null ? exception.ToString() : "[exception: null]");
					writer.Close();
				}
			}
			catch
			{
				Server.ClearError();

				//Делать редирект на страницу ошибок нужно с большой осторожностью, поскольку может произойти зацикливание (например, если страница ошибок обращается к БД, а БД недоступна). Для обработки таких ошибок надо делать редирект на простую статическую страницу.
				Response.Clear();
				Response.StatusCode = (int)HttpStatusCode.InternalServerError;
				Response.Write("Aaarghh! Сервис временно недоступен.");
				Response.End();
			}
		}
	}
}
