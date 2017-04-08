using System;
using System.Collections.Generic;
using System.Linq;
using Logisto.Data;
using Logisto.Models;
using LinqToDB;

namespace Logisto.BusinessLogic
{
	public class UserLogic : IUserLogic
	{

		public IEnumerable<User> GetUsers(ListFilter filter)
		{
			using (var db = new LogistoDb())
			{
				var query = db.Users.AsQueryable();

				// условия поиска

				if (!string.IsNullOrWhiteSpace(filter.Context))
					query = query.Where(w => w.Login.Contains(filter.Context)
										);
				// TODO: sort

				// пейджинг

				if (filter.PageNumber > 0)
					query = query.Skip(filter.PageNumber * filter.PageSize);

				if (filter.PageSize > 0)
					query = query.Take(filter.PageSize);

				return query.ToList();
			}
		}

		public User GetUser(int id)
		{
			using (var db = new LogistoDb())
			{
				return db.Users.First(w => w.ID == id);
			}
		}

		public User GetUserByEmployee(int employeeId)
		{
			using (var db = new LogistoDb())
			{
				return (from e in db.Employees
						join p in db.Persons on e.PersonId equals p.ID
						join u in db.Users on p.ID equals u.PersonId
						where e.ID == employeeId
						select u
						).FirstOrDefault();
			}
		}

		public void UpdatePhoto(User user)
		{
			using (var db = new LogistoDb())
			{
				db.Users.Where(w => w.ID == user.ID)
				.Set(u => u.Photo, user.Photo)
				.Update();
			}
		}

		#region user settings

		public string GetSetting(int userId, string key)
		{
			using (var db = new LogistoDb())
			{
				return db.UserSettings.Where(w => w.UserId == userId && w.Key == key).Select(s => s.Value).FirstOrDefault();
			}
		}

		public int SetSetting(int userId, string key, string value)
		{
			using (var db = new LogistoDb())
			{
				int id = db.UserSettings.Where(w => w.UserId == userId && w.Key == key).Select(s => s.ID).FirstOrDefault();
				if (id > 0)
				{
					var setting = db.UserSettings.First(w => w.ID == id);
					setting.Value = value;
					db.Update(setting);
					return id;
				}
				else
				{
					var setting = new UserSetting { UserId = userId, Key = key, Value = value };
					return Convert.ToInt32(db.InsertWithIdentity(setting));
				}
			}
		}

		#endregion
	}
}