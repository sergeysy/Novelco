using System.Web.Mvc;
using Logisto.ViewModels;

namespace Logisto.Controllers
{
	[Authorize]
	public class UsersController : BaseController
	{
		#region Pages

		public ActionResult View(int id)
		{
			var user = userLogic.GetUser(id);

			var viewModel = new UserViewModel
			{
				ID = user.ID,
				Login = user.Login,
				PersonId = user.PersonId,
				RoleId = user.RoleId
			};

			viewModel.Person = personLogic.GetPerson(user.PersonId.Value);

			viewModel.Dictionaries.Add("Role", dataLogic.GetRoles());

			return View(viewModel);
		}

		#endregion
	}
}



