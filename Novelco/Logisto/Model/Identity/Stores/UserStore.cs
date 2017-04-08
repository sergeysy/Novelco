using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LinqToDB;
using Logisto.Data;
using Logisto.Models;
using Microsoft.AspNet.Identity;

namespace Logisto.Identity
{
	public class UserStore :
		IUserStore<IdentityUser, int>,
		//IUserLoginStore<IdentityUser, int>,
		//IUserClaimStore<IdentityUser, int>,
		//IUserLockoutStore<IdentityUser, int>,
		//IUserTwoFactorStore<IdentityUser, int>,
		//IUserPhoneNumberStore<IdentityUser, int>,
		//ISecurityStampManager<IdentityUser, int>,
		//IUserSecurityStampStore<IdentityUser, int>,
		IUserRoleStore<IdentityUser, int>,
		IUserEmailStore<IdentityUser, int>,
		IUserPasswordStore<IdentityUser, int>,
		IQueryableUserStore<IdentityUser, int>
	{

		private bool _disposed;

		protected void ThrowIfDisposed()
		{
			if (_disposed)
				throw new ObjectDisposedException(GetType().Name);
		}

		#region Properties

		public LogistoDb UsersDb { get; private set; }

		public bool DisposeUserDb { get; set; }

		#endregion

		public UserStore(LogistoDb usersDb)
		{
			if (usersDb == null)
				throw new ArgumentNullException("usersDb");

			UsersDb = usersDb;
		}

		#region IDisposable interface

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// If disposing, calls dispose on the usersDb.  Always nulls out the usersDb
		/// </summary>
		/// <param name="disposing"></param>
		protected virtual void Dispose(bool disposing)
		{
			if (DisposeUserDb && disposing && UsersDb != null)
				UsersDb.Dispose();

			_disposed = true;
			UsersDb = null;
		}

		#endregion

		#region IUserStore interface

		public async Task CreateAsync(IdentityUser user)
		{
			ThrowIfDisposed();
			if (user == null)
				throw new ArgumentNullException("user");

			await Task.Run(() =>
			{
				using (var usersDb = new LogistoDb())
				{
					user.Id = Convert.ToInt32(usersDb.InsertWithIdentity(user));
				}
			});
		}

		public async Task UpdateAsync(IdentityUser user)
		{
			ThrowIfDisposed();
			if (user == null)
				throw new ArgumentNullException("user");

			await Task.Run(() => UsersDb.Update(user));
		}

		public async Task DeleteAsync(IdentityUser user)
		{
			ThrowIfDisposed();
			if (user == null)
				throw new ArgumentNullException("user");

			await Task.Run(() =>
			{
				UsersDb.IdentityUserInRoles.Where(ur => ur.UserId == user.Id).Delete();
				UsersDb.IdentityUsers.Where(u => u.Id == user.Id).Delete();
			});
		}

		public async Task<IdentityUser> FindByIdAsync(int userId)
		{
			ThrowIfDisposed();
			if (userId < 1)
				throw new ArgumentNullException("userId");

			return await UsersDb.IdentityUsers.FirstOrDefaultAsync(x => x.Id == userId);
		}

		public async Task<IdentityUser> FindByNameAsync(string userName)
		{
			ThrowIfDisposed();
			if (string.IsNullOrEmpty(userName))
				throw new ArgumentNullException("userName");

			return await UsersDb.IdentityUsers.FirstOrDefaultAsync(x => x.Login == userName);
		}

		#endregion

		#region IUserPasswordStore interface

		public Task SetPasswordHashAsync(IdentityUser user, string passwordHash)
		{
			ThrowIfDisposed();
			if (user == null)
				throw new ArgumentNullException("user");

			var account = UsersDb.IdentityAccounts.First(w => w.UserId == user.Id);
			account.Password = passwordHash;
			UsersDb.Update(account);
			return Task.FromResult(0);
		}

		public Task<string> GetPasswordHashAsync(IdentityUser user)
		{
			ThrowIfDisposed();
			if (user == null)
				throw new ArgumentNullException("user");

			var account = UsersDb.IdentityAccounts.First(w => w.UserId == user.Id);
			return Task.FromResult(account.Password);
		}

		public Task<bool> HasPasswordAsync(IdentityUser user)
		{
			var account = UsersDb.IdentityAccounts.First(w => w.UserId == user.Id);
			return Task.FromResult(!string.IsNullOrEmpty(account.Password));
		}

		#endregion

		#region IUserEmailStore interface

		public Task SetEmailAsync(IdentityUser user, string email)
		{
			ThrowIfDisposed();
			if (user == null)
				throw new ArgumentNullException("user");

			user.Email = email;
			UsersDb.Update(user);
			return Task.FromResult(0);
		}

		public Task<string> GetEmailAsync(IdentityUser user)
		{
			ThrowIfDisposed();
			if (user == null)
				throw new ArgumentNullException("user");

			return Task.FromResult(user.Email);
			//var person = UsersDb.Persons.First(w => w.ID == user.PersonId);
			//return Task.FromResult(person.Email);
		}

		public Task<bool> GetEmailConfirmedAsync(IdentityUser user)
		{
			ThrowIfDisposed();
			if (user == null)
				throw new ArgumentNullException("user");

			var account = UsersDb.IdentityAccounts.First(w => w.UserId == user.Id);
			return Task.FromResult(account.IsApproved);
		}

		public Task SetEmailConfirmedAsync(IdentityUser user, bool confirmed)
		{
			ThrowIfDisposed();
			if (user == null)
				throw new ArgumentNullException("user");

			var account = UsersDb.IdentityAccounts.First(w => w.UserId == user.Id);
			account.IsApproved = confirmed;
			UsersDb.Update(account);
			return Task.FromResult(0);
		}

		public async Task<IdentityUser> FindByEmailAsync(string email)
		{
			ThrowIfDisposed();
			if (string.IsNullOrEmpty(email))
				throw new ArgumentNullException("email");

			return await UsersDb.IdentityUsers.FirstOrDefaultAsync(w => w.Email == email);
			//var person = UsersDb.Persons.FirstOrDefaultAsync(x => x.Email == email);
			//if (person == null)
			//	return null;

			//return await UsersDb.IdentityUsers.FirstOrDefaultAsync(x => x.PersonId == person.Id);
		}

		#endregion
		
		#region IUserRoleStore interface

		public Task AddToRoleAsync(IdentityUser user, string roleName)
		{
			ThrowIfDisposed();
			if (user == null) 				
				throw new ArgumentNullException("user");
			
			if (string.IsNullOrWhiteSpace(roleName)) 
				throw new ArgumentNullException("roleName");

			return Task.Run(() =>
			{
				int roleId = UsersDb.IdentityRoles.Where(r => r.Name == roleName).Select(r => r.Id).Single();
				UsersDb.InsertWithIdentity(new IdentityUserInRole() { UserId = user.Id, RoleId = roleId });
			});
		}

		public Task RemoveFromRoleAsync(IdentityUser user, string roleName)
		{
			ThrowIfDisposed();
			if (user == null) 
				throw new ArgumentNullException("user");

			if (string.IsNullOrWhiteSpace(roleName)) 
				throw new ArgumentNullException("roleName");

			return Task.Run(() =>
			{
				int roleId = UsersDb.IdentityRoles.Where(r => r.Name == roleName).Select(r => r.Id).Single();
				UsersDb.IdentityUserInRoles.Where(ur => ur.UserId == user.Id && ur.RoleId == roleId).Delete();
			});
		}

		public async Task<IList<string>> GetRolesAsync(IdentityUser user)
		{
			ThrowIfDisposed();
			if (user == null)
				throw new ArgumentNullException("user");

			return await Task.Run(() =>
			{
				var query = from ur in UsersDb.IdentityUserInRoles
							join r in UsersDb.IdentityRoles on ur.RoleId equals r.Id
							where ur.UserId == user.Id
							select r.Name;
				return query.ToList();
			});
		}

		public Task<bool> IsInRoleAsync(IdentityUser user, string roleName)
		{
			ThrowIfDisposed();
			if (user == null) 
				throw new ArgumentNullException("user");

			if (string.IsNullOrWhiteSpace(roleName)) 
				throw new ArgumentNullException("roleName");

			return Task.Run(() =>
			{
				var query = from ur in UsersDb.IdentityUserInRoles
							join r in UsersDb.IdentityRoles on ur.RoleId equals r.Id
							where ur.UserId == user.Id && r.Name == roleName
							select ur.RoleId;
				return query.Any();
			});
		}

		#endregion

		#region IQueryableUserStore

		public IQueryable<IdentityUser> Users { get { return UsersDb.IdentityUsers.OrderBy(u => u.Login).AsQueryable(); } }

		#endregion

	}
}
