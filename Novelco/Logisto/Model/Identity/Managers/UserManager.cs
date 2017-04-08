using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Logisto.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;

namespace Logisto.Identity
{
	public class UserManager : UserManager<IdentityUser, int>
	{
		private readonly EmailService _emailService;

		public UserManager(IUserStore<IdentityUser, int> store, IdentityFactoryOptions<UserManager> options, EmailService emailService)
			: base(store)
		{
			_emailService = emailService;
			Initialize(options, null);
		}

		void Initialize(IdentityFactoryOptions<UserManager> options, IOwinContext context)
		{
			// Configure validation logic for usernames
			UserValidator = new UserValidator<IdentityUser, int>(this)
			{
				AllowOnlyAlphanumericUserNames = false,
				RequireUniqueEmail = false
			};

			// Configure user lockout defaults
			UserLockoutEnabledByDefault = true;
			DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
			MaxFailedAccessAttemptsBeforeLockout = 5;

			EmailService = _emailService;
			var dataProtectionProvider = options.DataProtectionProvider;
			if (dataProtectionProvider != null)
				UserTokenProvider = new DataProtectorTokenProvider<IdentityUser, int>(dataProtectionProvider.Create("Logisto Identity"));
		}

		public virtual async Task<IdentityResult> AddUserToRolesAsync(int userId, IList<string> roles)
		{
			var userRoleStore = (IUserRoleStore<IdentityUser, int>)Store;

			var user = await FindByIdAsync(userId).ConfigureAwait(false);
			if (user == null)
				throw new InvalidOperationException("Invalid user Id");

			var userRoles = await userRoleStore.GetRolesAsync(user).ConfigureAwait(false);
			// Add user to each role using UserRoleStore
			foreach (var role in roles.Where(role => !userRoles.Contains(role)))
				await userRoleStore.AddToRoleAsync(user, role).ConfigureAwait(false);

			// Call update once when all roles are added
			return await UpdateAsync(user).ConfigureAwait(false);
		}

		public virtual async Task<IdentityResult> RemoveUserFromRolesAsync(int userId, IList<string> roles)
		{
			var userRoleStore = (IUserRoleStore<IdentityUser, int>)Store;

			var user = await FindByIdAsync(userId).ConfigureAwait(false);
			if (user == null)
				throw new InvalidOperationException("Invalid user Id");

			var userRoles = await userRoleStore.GetRolesAsync(user).ConfigureAwait(false);
			// Remove user to each role using UserRoleStore
			foreach (var role in roles.Where(userRoles.Contains))
				await userRoleStore.RemoveFromRoleAsync(user, role).ConfigureAwait(false);

			// Call update once when all roles are removed
			return await UpdateAsync(user).ConfigureAwait(false);
		}
	}
}
