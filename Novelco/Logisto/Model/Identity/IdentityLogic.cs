using System;
using System.Collections.Generic;
using System.Linq;
using LinqToDB;
using Logisto.Data;
using Logisto.Identity;
using Logisto.Models;
using Microsoft.AspNet.Identity;

namespace Logisto.BusinessLogic
{
	public class IdentityLogic : IIdentityLogic
	{
		#region roles

		public int GetRolesCount(ListFilter filter)
		{
			using (var db = new LogistoDb())
			{
				var query = db.IdentityRoles.AsQueryable();

				// условия поиска

				if (!string.IsNullOrWhiteSpace(filter.Context))
					query = query.Where(w => w.Name.Contains(filter.Context) ||
						w.Description.Contains(filter.Context)
						);

				return query.Count();
			}
		}

		public IEnumerable<IdentityRole> GetRoles(ListFilter filter)
		{
			using (var db = new LogistoDb())
			{
				var query = db.IdentityRoles.AsQueryable();

				// условия поиска

				if (!string.IsNullOrWhiteSpace(filter.Context))
					query = query.Where(w => w.Name.Contains(filter.Context) ||
						w.Description.Contains(filter.Context)
						);

				// sort
				if (!string.IsNullOrWhiteSpace(filter.Sort))
				{
					if (string.IsNullOrWhiteSpace(filter.SortDirection))
						filter.SortDirection = "Asc";

					switch (filter.Sort)
					{
						case "Id":
							if (filter.SortDirection == "Asc")
								query = query.OrderBy(o => o.Id);
							else
								query = query.OrderByDescending(o => o.Id);

							break;

						case "Name":
							if (filter.SortDirection == "Asc")
								query = query.OrderBy(o => o.Name);
							else
								query = query.OrderByDescending(o => o.Name);

							break;
					}
				}

				// пейджинг

				if (filter.PageNumber > 0)
					query = query.Skip(filter.PageNumber * filter.PageSize);

				if (filter.PageSize > 0)
					query = query.Take(filter.PageSize);

				return query.ToList();
			}
		}

		public int CreateRole(IdentityRole role)
		{
			using (var db = new LogistoDb())
			{
				return Convert.ToInt32(db.InsertWithIdentity(role));
			}
		}

		public IdentityRole GetRole(int id)
		{
			using (var db = new LogistoDb())
			{
				return db.IdentityRoles.First(w => w.Id == id);
			}
		}

		public void UpdateRole(IdentityRole role)
		{
			using (var db = new LogistoDb())
			{
				db.IdentityRoles.Where(w => w.Id == role.Id)
				.Set(u => u.Name, role.Name)
				.Set(u => u.Description, role.Description)
				.Update();
			}
		}

		public void DeleteRole(int id)
		{
			using (var db = new LogistoDb())
			{
				db.IdentityRoles.Delete(w => w.Id == id);
			}
		}

		#endregion

		#region users

		public int GetUsersCount(ListFilter filter)
		{
			using (var db = new LogistoDb())
			{
				var query = db.IdentityUsers.AsQueryable();

				// условия поиска

				if (!string.IsNullOrWhiteSpace(filter.Context))
					query = query.Where(w => w.Name.Contains(filter.Context) ||
						w.Login.Contains(filter.Context) ||
						w.Email.Contains(filter.Context)
						);

				return query.Count();
			}
		}

		public IEnumerable<IdentityUser> GetUsers(ListFilter filter)
		{
			using (var db = new LogistoDb())
			{
				var query = db.IdentityUsers.AsQueryable();

				// условия поиска

				if (!string.IsNullOrWhiteSpace(filter.Context))
					query = query.Where(w => w.Name.Contains(filter.Context) ||
						w.Login.Contains(filter.Context) ||
						w.Email.Contains(filter.Context)
						);

				// sort
				if (!string.IsNullOrWhiteSpace(filter.Sort))
				{
					if (string.IsNullOrWhiteSpace(filter.SortDirection))
						filter.SortDirection = "Asc";

					switch (filter.Sort)
					{
						case "Id":
							if (filter.SortDirection == "Asc")
								query = query.OrderBy(o => o.Id);
							else
								query = query.OrderByDescending(o => o.Id);

							break;

						case "Login":
							if (filter.SortDirection == "Asc")
								query = query.OrderBy(o => o.Login);
							else
								query = query.OrderByDescending(o => o.Login);

							break;

						case "Name":
							if (filter.SortDirection == "Asc")
								query = query.OrderBy(o => o.Name);
							else
								query = query.OrderByDescending(o => o.Name);

							break;
					}
				}

				// пейджинг

				if (filter.PageNumber > 0)
					query = query.Skip(filter.PageNumber * filter.PageSize);

				if (filter.PageSize > 0)
					query = query.Take(filter.PageSize);

				return query.ToList();
			}
		}

		public int CreateUser(IdentityUser user)
		{
			using (var db = new LogistoDb())
			{
				var userId = Convert.ToInt32(db.InsertWithIdentity(user));

				var userManager = new UserManager(new UserStore(new LogistoDb()), Startup.IdentityFactoryOptions, new EmailService());
				var stamp = userManager.PasswordHasher;

				// добавить аккаунт
				var account = new IdentityAccount
									{
										CreatedDate = DateTime.Now,
										IsApproved = true,
										Password = stamp.HashPassword("123456789"),	// default password
										UserId = userId
									};

				db.InsertWithIdentity(account);
				return userId;
			}
		}

		public IdentityUser GetUser(int id)
		{
			using (var db = new LogistoDb())
			{
				return db.IdentityUsers.First(w => w.Id == id);
			}
		}

		public void UpdateUser(IdentityUser user)
		{
			using (var db = new LogistoDb())
			{
				db.IdentityUsers.Where(w => w.Id == user.Id)
				.Set(u => u.Name, user.Name)
				.Set(u => u.Email, user.Email)
				.Set(u => u.PersonId, user.PersonId)
				.Set(u => u.CrmId, user.CrmId)
				.Update();

				if (db.IdentityAccounts.FirstOrDefault(w => w.UserId == user.Id) == null)
					db.InsertWithIdentity(new IdentityAccount
												{
													CreatedDate = DateTime.Now,
													IsApproved = true,
													Password = "",	// default password
													UserId = user.Id
												});
			}
		}

		public void DeleteUser(int id)
		{
			using (var db = new LogistoDb())
			{
				db.IdentityAccounts.Delete(w => w.UserId == id);
				db.IdentityUserInRoles.Delete(w => w.UserId == id);
				db.IdentityUsers.Delete(w => w.Id == id);
			}
		}

		#endregion


		public void AddUserToRole(int userId, int roleId)
		{
			using (var db = new LogistoDb())
			{
				var rel = db.IdentityUserInRoles.FirstOrDefault(w => w.UserId == userId && w.RoleId == roleId);
				if (rel == null)
					db.InsertWithIdentity(new IdentityUserInRole { UserId = userId, RoleId = roleId });
			}
		}

		public void RemoveUserFromRole(int userId, int roleId)
		{
			using (var db = new LogistoDb())
			{
				var rel = db.IdentityUserInRoles.FirstOrDefault(w => w.UserId == userId && w.RoleId == roleId);
				if (rel != null)
					db.Delete(rel);
			}
		}

		public IEnumerable<IdentityRole> GetUserRoles(int userId)
		{
			using (var db = new LogistoDb())
			{
				var query = from ur in db.IdentityUserInRoles
							join r in db.IdentityRoles on ur.RoleId equals r.Id
							where ur.UserId == userId
							select r;
				return query;
			}
		}

		public IEnumerable<int> GetUsersInRole(int roleId)
		{
			using (var db = new LogistoDb())
			{
				return db.IdentityUserInRoles.Where(w => w.RoleId == roleId).Select(s => s.UserId).Distinct();
			}
		}

		public bool IsUserInRole(int userId, int roleId)
		{
			using (var db = new LogistoDb())
			{
				return db.IdentityUserInRoles.Any(w => w.UserId == userId && w.RoleId == roleId);
			}
		}
	}
}