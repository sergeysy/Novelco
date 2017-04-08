using System;
using System.Linq;
using System.Threading.Tasks;
using LinqToDB;
using Logisto.Data;
using Logisto.Models;
using Microsoft.AspNet.Identity;

namespace Logisto.Identity
{
	public class RoleStore : IQueryableRoleStore<IdentityRole, int>
	{
		public LogistoDb Context { get; private set; }

		public IQueryable<IdentityRole> Roles { get { return Context.IdentityRoles.OrderBy(r => r.Name).AsQueryable(); } }

		public RoleStore(LogistoDb context)
		{
			if (context == null)
				throw new ArgumentNullException("context");

			Context = context;
		}

		public void Dispose()
		{
			//throw new NotImplementedException();
		}

		public Task CreateAsync(IdentityRole role)
		{
			if (role == null)
				throw new ArgumentNullException("role");

			return Task.Run(() => Context.InsertWithIdentity(role));
		}

		public Task UpdateAsync(IdentityRole role)
		{
			if (role == null)
				throw new ArgumentNullException("role");

			return Task.Run(() => Context.Update(role));
		}

		public Task DeleteAsync(IdentityRole role)
		{
			if (role == null)
				throw new ArgumentNullException("role");

			return Task.Run(() =>
			{
				Context.IdentityUserInRoles.Where(ur => ur.RoleId == role.Id).Delete();
				Context.IdentityRoles.Where(r => r.Id == role.Id).Delete();
			});
		}

		public async Task<IdentityRole> FindByIdAsync(int roleId)
		{
			if (roleId < 1)
				throw new ArgumentNullException("roleId");

			return await Context.IdentityRoles.FirstOrDefaultAsync(r => r.Id == roleId);
		}

		public async Task<IdentityRole> FindByNameAsync(string roleName)
		{
			if (string.IsNullOrWhiteSpace(roleName))
				throw new ArgumentNullException("roleName");

			return await Context.IdentityRoles.FirstOrDefaultAsync(r => r.Name == roleName);
		}
	}
}
