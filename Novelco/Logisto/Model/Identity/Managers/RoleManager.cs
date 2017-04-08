using Logisto.Data;
using Logisto.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;

namespace Logisto.Identity
{
	public class RoleManager : RoleManager<IdentityRole, int>
	{
		public RoleManager(IRoleStore<IdentityRole, int> store)
			: base(store)
		{ }

		public static RoleManager Create(IdentityFactoryOptions<RoleManager> options, IOwinContext context)
		{
			var manager = new RoleManager(new RoleStore(context.Get<LogistoDb>()));
			return manager;
		}
	}
}
