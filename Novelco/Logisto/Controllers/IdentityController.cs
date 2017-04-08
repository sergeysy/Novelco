using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Logisto.BusinessLogic;
using Logisto.Data;
using Logisto.Identity;
using Logisto.Models;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;

namespace Logisto.Controllers
{
	[Authorize]
	public class IdentityController : Controller
	{
		IIdentityLogic identityLogic;

		int CurrentUserId
		{
			get { return int.Parse(HttpContext.GetOwinContext().Authentication.User.Claims.First(w => w.Type == ClaimTypes.NameIdentifier).Value); }
		}

		public IdentityController()
		{
			// TODO: DI
			identityLogic = new IdentityLogic();
		}

		#region Roles

		public ActionResult Roles()
		{
			var userManager = new UserManager(new UserStore(new LogistoDb()), Startup.IdentityFactoryOptions, new EmailService());
			if (!userManager.IsInRole(CurrentUserId, "GM"))
				return RedirectToAction("NotAuthorized", "Home");

			var viewModel = new ListFilter { PageSize = 50 };
			return View(viewModel);
		}

		public ContentResult GetRoles(ListFilter filter)
		{
			var totalCount = identityLogic.GetRolesCount(filter);
			var list = identityLogic.GetRoles(filter);
			return Content(JsonConvert.SerializeObject(new { Items = list, TotalCount = totalCount }));
		}

		public ContentResult GetNewRole()
		{
			// подстановка значений по-умолчанию
			var c = new IdentityRole();
			return Content(JsonConvert.SerializeObject(c));
		}

		public ContentResult SaveRole(IdentityRole model)
		{
			if (model.Id == 0)
			{
				var id = identityLogic.CreateRole(model);
				return Content(JsonConvert.SerializeObject(new { Id = id }));
			}
			else
			{
				// обновить позицию
				var term = identityLogic.GetRole(model.Id);
				term.Name = model.Name;
				term.Description = model.Description;

				identityLogic.UpdateRole(term);

				return Content(JsonConvert.SerializeObject(""));
			}
		}

		public ContentResult DeleteRole(int id)
		{
			try
			{
				identityLogic.DeleteRole(id);
				return Content(JsonConvert.SerializeObject(""));
			}
			catch (Exception ex)
			{
				return Content(JsonConvert.SerializeObject(new { Message = "Не удалось удалить роль, возможно она ещё используется. " + ex.Message }));
			}
		}

		public ContentResult ResetUserPassword(int userId)
		{
			var userManager = new UserManager(new UserStore(new LogistoDb()), Startup.IdentityFactoryOptions, new EmailService());
			var stamp = userManager.PasswordHasher;

			var token = userManager.GeneratePasswordResetToken(userId);
			//userManager.ResetPassword(userId, token, stamp.HashPassword("123456789"));
			//userManager.ResetPassword(userId, token, HashPassword("123456789", userId.ToString()));
			userManager.ResetPassword(userId, token, "123456789");

			return Content(JsonConvert.SerializeObject(""));
		}

		#endregion

		#region Users

		public ActionResult Users()
		{
			var userManager = new UserManager(new UserStore(new LogistoDb()), Startup.IdentityFactoryOptions, new EmailService());
			if (!userManager.IsInRole(CurrentUserId, "GM"))
				return RedirectToAction("NotAuthorized", "Home");

			var viewModel = new ListFilter { PageSize = 50 };
			return View(viewModel);
		}

		public ContentResult UsersGetItems(ListFilter filter)
		{
			var totalCount = identityLogic.GetUsersCount(filter);
			var list = identityLogic.GetUsers(filter);
			return Content(JsonConvert.SerializeObject(new { Items = list, TotalCount = totalCount }));
		}

		public ContentResult GetNewUser()
		{
			// подстановка значений по-умолчанию
			var c = new IdentityUser();
			return Content(JsonConvert.SerializeObject(c));
		}

		public ContentResult SaveUser(IdentityUser model)
		{
			if (model.Id == 0)
			{
				var id = identityLogic.CreateUser(model);
				return Content(JsonConvert.SerializeObject(new { Id = id }));
			}
			else
			{
				// обновить позицию
				var user = identityLogic.GetUser(model.Id);
				user.Name = model.Name;
				user.Email = model.Email;
				user.PersonId = model.PersonId;
				user.CrmId = model.CrmId;

				identityLogic.UpdateUser(user);

				return Content(JsonConvert.SerializeObject(""));
			}
		}

		public ContentResult UpdateUserRoles(int userId, IEnumerable<int> roles)
		{
			var allRoles = identityLogic.GetRoles(new ListFilter()).Select(s => s.Id);
			foreach (var roleId in allRoles)
				if ((roles != null) && (roles.Contains(roleId)))
					identityLogic.AddUserToRole(userId, roleId);
				else
					identityLogic.RemoveUserFromRole(userId, roleId);

			return Content(JsonConvert.SerializeObject(""));
		}

		public ContentResult GetUserRoles(int userId)
		{
			var roles = identityLogic.GetUserRoles(userId).Select(s => s.Id);

			return Content(JsonConvert.SerializeObject(roles));
		}

		public ContentResult DeleteUser(int id)
		{
			try
			{
				identityLogic.DeleteUser(id);
				return Content(JsonConvert.SerializeObject(""));
			}
			catch (Exception ex)
			{
				return Content(JsonConvert.SerializeObject(new { Message = "Не удалось удалить пользователя. " + ex.Message }));
			}
		}

		#endregion

		public string HashPassword(string pass, string salt)
		{
			byte[] bytes = Encoding.Unicode.GetBytes(pass);
			byte[] src = Encoding.Unicode.GetBytes(salt);
			byte[] dst = new byte[src.Length + bytes.Length];
			Buffer.BlockCopy(src, 0, dst, 0, src.Length);
			Buffer.BlockCopy(bytes, 0, dst, src.Length, bytes.Length);
			HashAlgorithm algorithm = HashAlgorithm.Create("SHA1");
			byte[] inArray = algorithm.ComputeHash(dst);
			return Convert.ToBase64String(inArray);
		}
	}
}






