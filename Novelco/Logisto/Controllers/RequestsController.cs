using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Logisto.Models;
using Logisto.ViewModels;
using Newtonsoft.Json;
using OfficeOpenXml;

namespace Logisto.Controllers
{
	[Authorize]
	public class RequestsController : BaseController
	{
		const string FILES_ROOT = @"\\corpserv03.corp.local\Common\Carman\RequestFiles\";

		public ActionResult Index()
		{
			var pageSize = int.Parse(ConfigurationManager.AppSettings["App_PageSize"]);
			var viewModel = new IndexViewModel { Filter = new ListFilter { PageSize = pageSize } };
			if (!identityLogic.GetUserRoles(CurrentUserId).Any(w => w.Name == "GM"))
				viewModel.Filter.UserId = CurrentUserId;

			viewModel.Dictionaries.Add("User", dataLogic.GetUsers().OrderBy(o => o.Display));
			viewModel.Dictionaries.Add("ActiveUser", dataLogic.GetActiveUsers().OrderBy(o => o.Display));
			viewModel.Dictionaries.Add("Product", dataLogic.GetProducts());

			return View(viewModel);
		}

		public ContentResult GetItems(ListFilter filter)
		{
			if (!identityLogic.GetUserRoles(CurrentUserId).Any(w => w.Name == "GM"))
				filter.UserId = CurrentUserId;

			if (filter.From.HasValue)
				filter.From = filter.From.Value.Date;

			if (filter.To.HasValue)
				filter.To = filter.To.Value.Date.AddDays(1).AddSeconds(-1);

			if (filter.From2.HasValue)
				filter.From2 = filter.From2.Value.Date;

			if (filter.To2.HasValue)
				filter.To2 = filter.To2.Value.Date.AddDays(1).AddSeconds(-1);

			var totalCount = requestLogic.GetRequestsCount(filter);
			var list = requestLogic.GetRequests(filter);
			return Content(JsonConvert.SerializeObject(new { Items = list, TotalCount = totalCount }));
		}

		public ContentResult GetNewRequest()
		{
			// подстановка значений по-умолчанию
			var entity = new Request { Date = DateTime.Now, SalesUserId = CurrentUserId };
			return Content(JsonConvert.SerializeObject(entity));
		}

		public ContentResult SaveRequest(Request model)
		{
			model.ResponseDate = model.ResponseDate?.ToUniversalTime();

			if (model.ID == 0)
			{
				var request = new Request();
				request.ContractorId = model.ContractorId;
				request.AccountUserId = model.AccountUserId;
				request.ClientName = model.ClientName;
				request.Comment = model.Comment;
				request.Date = model.Date;
				request.ProductId = model.ProductId;
				request.ResponseDate = model.ResponseDate;
				request.SalesUserId = model.SalesUserId;
				request.Text = model.Text;
				request.CargoInfo = model.CargoInfo;
				request.Contacts = model.Contacts;
				request.Route = model.Route;

				var id = requestLogic.CreateRequest(request);
				return Content(JsonConvert.SerializeObject(new { ID = id }));
			}
			else
			{
				var request = requestLogic.GetRequest(model.ID);
				//request.ContractorId = model.ContractorId;
				request.AccountUserId = model.AccountUserId;
				request.ClientName = model.ClientName;
				request.Comment = model.Comment;
				request.Date = model.Date;
				request.ProductId = model.ProductId;
				request.ResponseDate = model.ResponseDate;
				request.SalesUserId = model.SalesUserId;
				request.Text = model.Text;
				request.CargoInfo = model.CargoInfo;
				request.Contacts = model.Contacts;
				request.Route = model.Route;

				requestLogic.UpdateRequest(request);
				return Content(JsonConvert.SerializeObject(""));
			}
		}

		public ContentResult UploadFile(int id)
		{
			if (Request.Files.Count == 0)
				return Content(JsonConvert.SerializeObject(new { Message = "Неверные входные данные" }));

			var file = Request.Files[0];
			file.SaveAs(FILES_ROOT + id);

			var request = requestLogic.GetRequest(id);
			request.Filename = file.FileName;
			requestLogic.UpdateRequest(request);

			return Content(JsonConvert.SerializeObject(""));
		}

		public FileResult DownloadFile(int id)
		{
			var request = requestLogic.GetRequest(id);
			var contentType = "application/octet-stream";
			var filename = FILES_ROOT + id;						
			return File(filename, contentType, request.Filename);
		}

		public FileResult DownloadExcel(ListFilter filter)
		{
			filter.PageNumber = 0;
			filter.PageSize = 9999;

			if (filter.From.HasValue)
				filter.From = filter.From.Value.Date;

			if (filter.To.HasValue)
				filter.To = filter.To.Value.Date.AddDays(1).AddSeconds(-1);

			if (filter.From2.HasValue)
				filter.From2 = filter.From2.Value.Date;

			if (filter.To2.HasValue)
				filter.To2 = filter.To2.Value.Date.AddDays(1).AddSeconds(-1);

			if (!identityLogic.GetUserRoles(CurrentUserId).Any(w => w.Name == "GM"))
				filter.UserId = CurrentUserId;

			var list = requestLogic.GetRequests(filter);
			var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
			var template = Server.MapPath("~/App_Data/RequestsTemplate.xlsx");
			var filename = Server.MapPath("~/Temp/r" + new Random(Environment.TickCount).Next(999999) + ".xlsx");

			GenerateRequestsReport(list, template, filename);

			return File(filename, contentType, "Requests.xlsx");
		}

		void GenerateRequestsReport(IEnumerable<Request> list, string templateFilename, string resultFilename)
		{
			using (var ex = new ExcelPackage(new FileInfo(templateFilename)))
			{
				int currentRow = 4;
				var sheet = ex.Workbook.Worksheets[1];
				var products = dataLogic.GetProducts();
				var users = dataLogic.GetUsers();

				foreach (var item in list)
				{
					int index = 1;
					sheet.Cells[currentRow, index++].Value = item.ID;
					sheet.Cells[currentRow, index++].Value = item.Date;
					sheet.Cells[currentRow, index++].Value = item.ResponseDate;
					sheet.Cells[currentRow, index++].Value = item.ClientName;
					sheet.Cells[currentRow, index++].Value = products.Where(w => w.ID == item.ProductId).Select(s => s.Display).FirstOrDefault();
					sheet.Cells[currentRow, index++].Value = item.Text;
					sheet.Cells[currentRow, index++].Value = item.SalesUserId.HasValue ? users.First(w => w.ID == item.SalesUserId).Display : "";
					sheet.Cells[currentRow, index++].Value = item.AccountUserId.HasValue ? users.First(w => w.ID == item.AccountUserId).Display : "";
					sheet.Cells[currentRow, index++].Value = item.Comment;

					currentRow++;
				}

				ex.SaveAs(new FileInfo(resultFilename));
			}
		}
	}
}






