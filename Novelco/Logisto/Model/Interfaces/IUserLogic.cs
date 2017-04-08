using System.Collections.Generic;
using Logisto.Models;

namespace Logisto.BusinessLogic
{
	public interface IUserLogic
	{
		/// <summary>
		/// Получить список  с учетом фильтра
		/// </summary>
		IEnumerable<User> GetUsers(ListFilter filter);

		/// <summary>
		/// Получить 
		/// </summary>
		User GetUser(int id);
		User GetUserByEmployee(int employeeId);

		void UpdatePhoto(User user);

		#region user settings
		
		string GetSetting(int userId, string key);
	
		int SetSetting(int userId, string key, string value);

		#endregion
	}
}