using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Logisto.Models;
using Logisto.ViewModels;
using Newtonsoft.Json;

namespace Logisto.Controllers
{
	[Authorize]
	public class OperationsController : BaseController
	{
		#region Pages

		public ActionResult Index()
		{
			var viewModel = new OperationsViewModel { Filter = new OperationsFilter { Responsibles = new List<int> { CurrentUserId }, PageSize = 50 } };
			viewModel.Items = orderLogic.GetOperations(viewModel.Filter).ToList();

			viewModel.Dictionaries.Add("OperationStatus", dataLogic.GetOperationStatuses());
			viewModel.Dictionaries.Add("OrderOperation", dataLogic.GetOrderOperations());
			viewModel.Dictionaries.Add("OperationKind", dataLogic.GetOperationKinds());
			viewModel.Dictionaries.Add("ActiveUser", dataLogic.GetActiveUsers());
			viewModel.Dictionaries.Add("Order", dataLogic.GetOrders());
			viewModel.Dictionaries.Add("User", dataLogic.GetUsers());

			return View(viewModel);
		}

		#endregion

		public ContentResult GetItems(OperationsFilter filter)
		{
			userLogic.SetSetting(CurrentUserId, "Operations.Sort", filter.Sort);
			userLogic.SetSetting(CurrentUserId, "Operations.SortDirection", filter.SortDirection);
			userLogic.SetSetting(CurrentUserId, "Operations.PageNumber", filter.PageNumber.ToString());

			var totalCount = orderLogic.GetOperationsCount(filter);
			var list = orderLogic.GetOperations(filter);

			return Content(JsonConvert.SerializeObject(new { Items = list, TotalCount = totalCount }));
		}
	}
}







