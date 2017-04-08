using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using LinqToDB;
using Logisto.BusinessLogic;
using Logisto.Data;
using Logisto.Identity;
using Logisto.Models;
using Logisto.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;

namespace Logisto.Controllers
{
	[Authorize]
	public class HomeController : Controller
	{
		UserManager userManager;

		int CurrentUserId
		{
			get { return int.Parse(HttpContext.GetOwinContext().Authentication.User.Claims.First(w => w.Type == ClaimTypes.NameIdentifier).Value); }
		}

		string CurrentUserName
		{
			get { return HttpContext.GetOwinContext().Authentication.User.Claims.Where(w => w.Type == ClaimTypes.Name).Select(s => s.Value).FirstOrDefault(); }
		}

		IAuthenticationManager AuthenticationManager { get { return HttpContext.GetOwinContext().Authentication; } }

		public ActionResult Index()
		{
			//Проставь всем ДЕЙСТВУЮЩИМ договорам метки ОК, Проверен
			//using (var db = new LogistoDb())
			//{
			//	foreach (var item in db.Contracts.ToList())
			//	{
			//		var IsActive = item.EndDate.HasValue ? (item.EndDate < DateTime.Now ? (item.IsProlongation ? true : false) : true) : (true);
			//		if (!IsActive)
			//			continue;

			//		var marks = db.ContractMarks.FirstOrDefault(w => w.ContractId == item.ID);
			//		if (marks != null)
			//			db.ContractMarks.Where(w => w.ID == marks.ID).Set(u => u.IsContractOk, true).Set(u => u.IsContractChecked, true).Update();
			//		else
			//			db.InsertWithIdentity(new ContractMark { ContractId = item.ID, IsContractOk = true, IsContractChecked = true });
			//	}
			//}



			var dataLogic = new DataLogic();
			var model = new HomeViewModel();

			var rates = dataLogic.GetCurrencyRates(DateTime.Today);
			model.UsdRate = rates.Where(w => w.CurrencyId == 2).Select(s => s.Rate ?? 0).FirstOrDefault();
			model.EurRate = rates.Where(w => w.CurrencyId == 3).Select(s => s.Rate ?? 0).FirstOrDefault();
			model.CnyRate = rates.Where(w => w.CurrencyId == 5).Select(s => s.Rate ?? 0).FirstOrDefault();
			model.GbpRate = rates.Where(w => w.CurrencyId == 6).Select(s => s.Rate ?? 0).FirstOrDefault();

			model.LastRateUpdated = DateTime.Parse(dataLogic.GetSystemSettings().First(w => w.Name == "CurrencyUpdate").Value, CultureInfo.GetCultureInfo("ru-RU"));

			if (User.Identity.IsAuthenticated)
				ViewBag.CurrentUserId = CurrentUserId;

			return View(model);
		}

		[AllowAnonymous]
		public ActionResult Login(string returnUrl)
		{
			var model = new LoginViewModel();
			if (!string.IsNullOrWhiteSpace(returnUrl))
				ViewBag.ReturnUrl = returnUrl;

			return View(model);
		}

		[HttpPost]
		[AllowAnonymous]
		public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
		{
			if (!ModelState.IsValid)
				return View(model);

			userManager = new UserManager(new UserStore(new LogistoDb()), Startup.IdentityFactoryOptions, new EmailService());

			// Валидация входных данных: проверка на корректность
			var login = model.Login;
			var loginValidity = SignInHelper.ValidateLogin(ref login);
			var passwordValidity = userManager.PasswordValidator.ValidateAsync(model.Password).Result;
			if (!loginValidity.Succeeded || !passwordValidity.Succeeded)
			{
				ModelState.AddModelError("", "LoginFormWrongInputMessage");
				return View(model);
			}

			// This doen't count login failures towards lockout only two factor authentication
			// To enable password failures to trigger lockout, change to shouldLockout: true

			var result = await new SignInHelper(userManager, AuthenticationManager).PasswordSignIn(login, model.Password, /*model.RememberMe*/true, shouldLockout: false);
			switch (result)
			{
				case Logisto.Identity.SignInStatus.Success:
					if (model.Password == "123456789")
						return RedirectToAction("ChangePassword");

					if (!string.IsNullOrWhiteSpace(returnUrl))
						return Redirect(returnUrl);
					else
						return RedirectToAction("Index");

				case Logisto.Identity.SignInStatus.LockedOut:
					return View("Lockout");

				case Logisto.Identity.SignInStatus.RequiresTwoFactorAuthentication:
					return RedirectToAction("SendCode", new { ReturnUrl = returnUrl });

				case Logisto.Identity.SignInStatus.Failure:
				default:
					ModelState.AddModelError("", "Неверный логин или пароль");
					return View(model);
			}
		}

		public ActionResult Logoff()
		{
			IdentitySignout();
			return RedirectToAction("Index");
		}

		public ActionResult Profile()
		{
			var model = new ProfileViewModel { Login = HttpContext.GetOwinContext().Authentication.User.Claims.First(w => w.Type == ClaimTypes.Name).Value };

			var userId = int.Parse(HttpContext.GetOwinContext().Authentication.User.Claims.First(w => w.Type == ClaimTypes.NameIdentifier).Value);
			var user = new UserLogic().GetUser(userId);
			var person = new PersonLogic().GetPerson(user.PersonId.Value);

			model.UserName = user.Login;
			model.Email = person.Email;
			model.Address = person.Address;
			model.Comment = person.Comment;
			model.DisplayName = person.DisplayName;
			model.EnName = person.EnName;
			model.Family = person.Family;
			model.GenitiveFamily = person.GenitiveFamily;
			model.GenitiveName = person.GenitiveName;
			model.GenitivePatronymic = person.GenitivePatronymic;
			model.Initials = person.Initials;
			model.IsNotResident = person.IsNotResident;
			model.IsSubscribed = person.IsSubscribed;
			model.Name = person.Name;
			model.Patronymic = person.Patronymic;
			using (var db = new LogistoDb())
			{
				var role = db.Roles.FirstOrDefault(w => w.ID == user.RoleId);
				if (role != null)
					model.Roles = role.Display;
			}

			return View(model);
		}

		public ActionResult ChangePassword(string token)
		{
			// TODO: process token / autorize

			var tip = GeneratePassword(6);

			var model = new ChangePasswordViewModel { Login = HttpContext.GetOwinContext().Authentication.User.Claims.First(w => w.Type == ClaimTypes.Name).Value, Tip = tip };

			return View(model);
		}

		[HttpPost]
		public ActionResult ChangePassword(ChangePasswordViewModel model)
		{
			// TODO: check valid

			var userId = int.Parse(HttpContext.GetOwinContext().Authentication.User.Claims.First(w => w.Type == ClaimTypes.NameIdentifier).Value);

			//
			userManager = new UserManager(new UserStore(new LogistoDb()), Startup.IdentityFactoryOptions, new EmailService());
			var stamp = userManager.PasswordHasher;

			var token = userManager.GeneratePasswordResetToken(userId);
			userManager.ResetPassword(userId, token, model.Password);

			return RedirectToAction("Index");
		}

		#region Contractor security 

		public ActionResult ChangeEmployeePassword(int id)
		{

			using (var db = new LogistoDb())
			{
				var employee = db.Employees.First(w => w.ID == id);
				var legal = db.Legals.First(w => w.ID == employee.LegalId);
				var contractor = db.Contractors.First(w => w.ID == legal.ContractorId);
				if (contractor.IsLocked && !IsSuperUser())
					return RedirectToAction("NotAuthorized", "Home");

				var person = db.Persons.First(w => w.ID == employee.PersonId);
				var model = new ChangeEmployeePasswordViewModel { EmployeeId = id, ContractorId = contractor.ID, Contractor = contractor.Name, Employee = person.DisplayName };
				return View(model);
			}
		}

		[HttpPost]
		public ActionResult ChangeEmployeePassword(ChangeEmployeePasswordViewModel model)
		{
			// TODO: check valid

			var hash = EncodePassword2(model.Password, model.EmployeeId.ToString());

			using (var db = new LogistoDb())
			{
				db.Employees.Where(w => w.ID == model.EmployeeId).Set(u => u.Password, hash).Update();

				if (!db.ContractorEmployeeSettings.Any(w => w.EmployeeId == model.EmployeeId))
					db.InsertWithIdentity(new ContractorEmployeeSettings
					{
						ContractorId = model.ContractorId,
						EmployeeId = model.EmployeeId,
						Password = hash,
						IsEnUI = false,
						NotifyDocumentChanged = true,
						NotifyDocumentCreated = true,
						NotifyDocumentDeleted = true,
						NotifyEventCreated = true,
						NotifyStatusChanged = true,
						NotifyTemplatedDocumentChanged = true,
						NotifyTemplatedDocumentCreated = true
					});

				return RedirectToAction("Index");
			}
		}

		#endregion

		public ActionResult NotAuthorized(int? actionId)
		{
			if (actionId.HasValue)
				ViewBag.ActionId = actionId.Value;

			return View();
		}

		public void IdentitySignin(User user, string providerKey = null, bool isPersistent = false)
		{
			var claims = new List<Claim>();

			// create required claims
			claims.Add(new Claim(ClaimTypes.NameIdentifier, user.ID.ToString()));
			claims.Add(new Claim(ClaimTypes.Name, user.Login));

			var identity = new ClaimsIdentity(claims, DefaultAuthenticationTypes.ApplicationCookie);

			AuthenticationManager.SignIn(new AuthenticationProperties()
			{
				AllowRefresh = true,
				IsPersistent = isPersistent,
				ExpiresUtc = DateTime.UtcNow.AddDays(7)
			}, identity);
		}

		public void IdentitySignout()
		{
			AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie, DefaultAuthenticationTypes.ExternalCookie);
		}

		public string GeneratePassword(int length)
		{
			const string valid = "!@-abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
			var result = new StringBuilder();
			var rnd = new Random();
			while (0 < length--)
				result.Append(valid[rnd.Next(valid.Length)]);

			return result.ToString();
		}

		public string EncodePassword(string pass, string salt)
		{
			byte[] bytes = Encoding.Unicode.GetBytes(pass);
			byte[] src = Convert.FromBase64String(salt);
			byte[] dst = new byte[src.Length + bytes.Length];
			Buffer.BlockCopy(src, 0, dst, 0, src.Length);
			Buffer.BlockCopy(bytes, 0, dst, src.Length, bytes.Length);
			HashAlgorithm algorithm = HashAlgorithm.Create("SHA1");
			byte[] inArray = algorithm.ComputeHash(dst);
			return Convert.ToBase64String(inArray);
		}

		public string EncodePassword2(string pass, string salt)
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

		bool IsSuperUser()
		{
			return new IdentityLogic().IsUserInRole(CurrentUserId, 1);    // GM
		}
	}
}