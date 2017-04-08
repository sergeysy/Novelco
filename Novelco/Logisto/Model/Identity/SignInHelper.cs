using System;
using System.Linq;
using System.Globalization;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Logisto.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System.Text;
using System.Security.Cryptography;

namespace Logisto.Identity
{
	public enum SignInStatus
	{
		Success,
		LockedOut,
		RequiresTwoFactorAuthentication,
		Failure
	}

	public class SignInHelper
	{
		public static readonly string EmailValidationRegex = @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z";

		public UserManager<IdentityUser, int> UserManager { get; private set; }

		public IAuthenticationManager AuthenticationManager { get; private set; }

		public SignInHelper(UserManager<IdentityUser, int> userManager, IAuthenticationManager authManager)
		{
			UserManager = userManager;
			AuthenticationManager = authManager;
		}

		public async Task SignInAsync(IdentityUser user, bool isPersistent, bool rememberBrowser)
		{
			// Clear any partial cookies from external or two factor partial sign ins
			AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie, DefaultAuthenticationTypes.TwoFactorCookie);
			var userIdentity = await GenerateUserIdentityAsync(UserManager, user);
			if (rememberBrowser)
			{
				var rememberBrowserIdentity = AuthenticationManager.CreateTwoFactorRememberBrowserIdentity(user.Id.ToString(CultureInfo.InvariantCulture));
				AuthenticationManager.SignIn(new AuthenticationProperties { IsPersistent = isPersistent, AllowRefresh = false, ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8) }, userIdentity, rememberBrowserIdentity);
			}
			else
			{
				AuthenticationManager.SignIn(new AuthenticationProperties { IsPersistent = isPersistent, AllowRefresh = false, ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8) }, userIdentity);
			}
		}

		public async Task<bool> SendTwoFactorCode(string provider)
		{
			var userId = await GetVerifiedUserIdAsync();
			if (userId == -1)
				return false;

			var token = await UserManager.GenerateTwoFactorTokenAsync(userId, provider);
			// See IdentityConfig.cs to plug in Email/SMS services to actually send the code
			await UserManager.NotifyTwoFactorTokenAsync(userId, provider, token);
			return true;
		}

		public async Task<int> GetVerifiedUserIdAsync()
		{
			var result = await AuthenticationManager.AuthenticateAsync(DefaultAuthenticationTypes.TwoFactorCookie);
			if (result != null && result.Identity != null && !String.IsNullOrEmpty(result.Identity.GetUserId()))
				return result.Identity.GetUserId<int>();

			return -1;
		}

		//public async Task<bool> HasBeenVerified()
		//{
		//	return await GetVerifiedUserIdAsync() != -1;
		//}

		public async Task<SignInStatus> TwoFactorSignIn(string provider, string code, bool isPersistent, bool rememberBrowser)
		{
			var userId = await GetVerifiedUserIdAsync();
			if (userId == -1)
				return SignInStatus.Failure;

			var user = await UserManager.FindByIdAsync(userId);
			if (user == null)
				return SignInStatus.Failure;

			if (await UserManager.IsLockedOutAsync(user.Id))
				return SignInStatus.LockedOut;

			if (await UserManager.VerifyTwoFactorTokenAsync(user.Id, provider, code))
			{
				// When token is verified correctly, clear the access failed count used for lockout
				await UserManager.ResetAccessFailedCountAsync(user.Id);
				await SignInAsync(user, isPersistent, rememberBrowser);
				return SignInStatus.Success;
			}

			// If the token is incorrect, record the failure which also may cause the user to be locked out
			await UserManager.AccessFailedAsync(user.Id);
			return SignInStatus.Failure;
		}

		public async Task<SignInStatus> ExternalSignIn(ExternalLoginInfo loginInfo, bool isPersistent)
		{
			var user = await UserManager.FindAsync(loginInfo.Login);
			if (user == null)
				return SignInStatus.Failure;

			if (await UserManager.IsLockedOutAsync(user.Id))
				return SignInStatus.LockedOut;

			return await SignInOrTwoFactor(user, isPersistent);
		}

		private async Task<SignInStatus> SignInOrTwoFactor(IdentityUser user, bool isPersistent)
		{
			//if (await UserManager.GetTwoFactorEnabledAsync(user.Id) &&
			//	!await AuthenticationManager.TwoFactorBrowserRememberedAsync(user.Id.ToString(CultureInfo.InvariantCulture)))
			//{
			//	var identity = new ClaimsIdentity(DefaultAuthenticationTypes.TwoFactorCookie);
			//	identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString(CultureInfo.InvariantCulture)));
			//	AuthenticationManager.SignIn(identity);
			//	return SignInStatus.RequiresTwoFactorAuthentication;
			//}

			await SignInAsync(user, isPersistent, false);
			return SignInStatus.Success;
		}

		public async Task<SignInStatus> PasswordSignIn(string userName, string password, bool isPersistent, bool shouldLockout)
		{
			var user = await UserManager.FindByNameAsync(userName);
			if (user == null)
				return SignInStatus.Failure;

			//if (await UserManager.IsLockedOutAsync(user.Id))
			//	return SignInStatus.LockedOut;

			if (await UserManager.CheckPasswordAsync(user, password))
				return await SignInOrTwoFactor(user, isPersistent);

			var hash = HashPassword(password, user.Id.ToString());
			var stored = await new UserStore(new Data.LogistoDb()).GetPasswordHashAsync(user);


			if (shouldLockout)
			{
				// If lockout is requested, increment access failed count which might lock out the user
				await UserManager.AccessFailedAsync(user.Id);
				if (await UserManager.IsLockedOutAsync(user.Id))
					return SignInStatus.LockedOut;
			}

			return SignInStatus.Failure;
		}

		public IdentityUser PasswordSignIn(string userName, string password)
		{
			IdentityUser user = UserManager.FindByName(userName);

			if (user == null)
				return null;

			if (UserManager.IsLockedOut(user.Id))
				return null;

			if (UserManager.CheckPassword(user, password))
				return user;

			var hash = HashPassword(password, user.Id.ToString());
			var stored = new UserStore(new Data.LogistoDb()).GetPasswordHashAsync(user).Result;

			return null;
		}

		public static IdentityResult ValidateLogin(ref string login)
		{
			if (string.IsNullOrWhiteSpace(login))
				return IdentityResult.Failed(new string[] { "Не указано имя пользователя" });

			// Проверка на соответствие email установленному формату
			//if (login.Contains("@"))
			//	return ValidateEmail(login);

			// Проверка на соответствие номера телефона формату
			//return ValidatePhone(ref login);

			return IdentityResult.Success;
		}

		public static IdentityResult ValidateName(ref string name)
		{
			if (string.IsNullOrWhiteSpace(name))
				return IdentityResult.Failed(new string[] { "Не указано имя пользователя" });

			return IdentityResult.Success;
		}

		public static IdentityResult ValidateEmail(string email)
		{
			if (string.IsNullOrWhiteSpace(email))
				return IdentityResult.Failed(new string[] { "Не указан email пользователя" });

			return Regex.IsMatch(email, EmailValidationRegex, RegexOptions.IgnoreCase) ?
				IdentityResult.Success :
				IdentityResult.Failed(new string[] { "Email неверного формата" });
		}

		/// <summary>
		/// Формирует пароль из случайных символов
		/// </summary>
		/// <param name="passwordLength"></param>
		/// <returns></returns>
		public static string GeneratePassword(int passwordLength = 8)
		{
			const string allowedChars = "abcdefgijkmnopqrstwxyz" + "ABCDEFGHJKLMNPQRSTWXYZ" + "23456789";

			string password = string.Empty;
			while (password.Length < passwordLength)
				password += string.Join("", Convert.ToBase64String(Guid.NewGuid().ToByteArray()).ToCharArray().Where(c => allowedChars.IndexOf(c) != -1).ToArray());

			return password.Substring(1, passwordLength);
		}

		/// <summary>
		/// Генерирует код подтверждения (случайный набор цифр)
		/// </summary>
		/// <param name="codeLength"></param>
		/// <returns></returns>
		public static string GenerateConfirmationCode(int codeLength = 4)
		{
			var rnd = new Random(Environment.TickCount);
			return string.Concat(Enumerable.Range(0, codeLength).Select((index) => rnd.Next(10).ToString()));
		}

		public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<IdentityUser, int> manager, IdentityUser user)
		{
			// Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
			var userIdentity = await manager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);

			// Add custom user claims here
			//userIdentity.AddClaim(new Claim("Name", user.Name));
			using (var db = new Logisto.Data.LogistoDb())
				userIdentity.AddClaim(new Claim("Name", db.Persons.Where(w => w.ID == user.PersonId).Select(s => s.DisplayName).FirstOrDefault() ?? "username"));

			//using (var db = new UsersDB())
			//{
			//	var query = from u in db.Users
			//				join ur in db.UserRoles on u.Id equals ur.UserId
			//				join rp in db.RolePermissions on ur.RoleId equals rp.RoleId
			//				join p in db.Permissions on rp.PermissionId equals p.Id
			//				where u.Id == Id
			//				select p.Name;
			//	userIdentity.AddClaim(new sys.Claim("Permissions", string.Join(",", query)));
			//}

			return userIdentity;
		}

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
