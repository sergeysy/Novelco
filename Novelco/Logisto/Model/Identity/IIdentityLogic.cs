using System.Collections.Generic;
using Logisto.Models;

namespace Logisto.BusinessLogic
{
	public interface IIdentityLogic
	{
		#region roles

		IdentityRole GetRole(int id);
		int CreateRole(IdentityRole role);
		void UpdateRole(IdentityRole role);
		void DeleteRole(int id);
		int GetRolesCount(ListFilter filter);
		IEnumerable<IdentityRole> GetRoles(ListFilter filter);

		#endregion

		#region users

		IdentityUser GetUser(int id);
		int CreateUser(IdentityUser User);
		void UpdateUser(IdentityUser User);
		void DeleteUser(int id);
		int GetUsersCount(ListFilter filter);
		IEnumerable<IdentityUser> GetUsers(ListFilter filter);

		#endregion

		void AddUserToRole(int userId, int roleId);
		void RemoveUserFromRole(int userId, int roleId);
		IEnumerable<IdentityRole> GetUserRoles(int userId);
		IEnumerable<int> GetUsersInRole(int roleId);
		bool IsUserInRole(int userId, int roleId);
	}
}