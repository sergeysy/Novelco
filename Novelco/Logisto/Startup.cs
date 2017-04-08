using System;
using System.Security.Claims;
using System.Web.Helpers;
using Logisto.Identity;
using Logisto.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.DataProtection;
using Owin;

[assembly: OwinStartupAttribute(typeof(Logisto.Startup))]
namespace Logisto
{
	public partial class Startup
	{
		public static IdentityFactoryOptions<UserManager> IdentityFactoryOptions;


		public void Configuration(IAppBuilder app)
		{
			ConfigureAuth2(app);
		}

		public void ConfigureAuth(IAppBuilder app)
		{
			// Enable the application to use a cookie to store information for the signed in user
			app.UseCookieAuthentication(new CookieAuthenticationOptions
											{
												AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
												LoginPath = new PathString("/Home/Login")
											});


			app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);
			AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.NameIdentifier;
		}

		// For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301864
		public void ConfigureAuth2(IAppBuilder app/*, Container container*/)
		{
			// Регистрируем UserManager в Owin, так как используется внутри SecurityStampValidator.OnValidateIdentity<UserManager, User, long>
			//app.CreatePerOwinContext<UserManager>((options, context) => container.GetInstance<UserManager>());

			// Enable the application to use a cookie to store information for the signed in user
			// and to use a cookie to temporarily store information about a user logging in with a third party login provider
			// Configure the sign in cookie
			app.UseCookieAuthentication(new CookieAuthenticationOptions
			{
				AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
				LoginPath = new PathString("/Home/Login"),
				Provider = new CookieAuthenticationProvider
				{
					// Enables the application to validate the security stamp when the user logs in.
					// This is a security feature which is used when you change a password or add an external login to your account.  
					OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<UserManager, IdentityUser, int>(
						validateInterval: TimeSpan.FromMinutes(3),
						regenerateIdentityCallback: (manager, user) => new SignInHelper(null, null).GenerateUserIdentityAsync(manager, user),
						getUserIdCallback: (id) => id.GetUserId<int>())
				},
				ExpireTimeSpan = TimeSpan.FromDays(365), //Время жизни куки
				CookieName = "auth.logisto",
				SlidingExpiration = true
			});
			app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

			// Enables the application to temporarily store user information when they are verifying the second factor in the two-factor authentication process.
			app.UseTwoFactorSignInCookie(DefaultAuthenticationTypes.TwoFactorCookie, TimeSpan.FromMinutes(5));

			// Enables the application to remember the second login verification factor such as phone or email.
			// Once you check this option, your second step of verification during the login process will be remembered on the device where you logged in from.
			// This is similar to the RememberMe option when you log in.
			app.UseTwoFactorRememberBrowserCookie(DefaultAuthenticationTypes.TwoFactorRememberBrowserCookie);
			AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.NameIdentifier;

			IdentityFactoryOptions = new IdentityFactoryOptions<UserManager> { DataProtectionProvider = app.GetDataProtectionProvider() };
		}
	}
}
