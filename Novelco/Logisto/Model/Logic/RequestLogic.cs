using System;
using System.Collections.Generic;
using System.Linq;
using LinqToDB;
using Logisto.Data;
using Logisto.Models;

namespace Logisto.BusinessLogic
{
	public class RequestLogic : IRequestLogic
	{
		#region Requests

		public int GetRequestsCount(ListFilter filter)
		{
			using (var db = new LogistoDb())
			{
				var query = db.Requests.AsQueryable();

				// условия поиска

				if (!string.IsNullOrWhiteSpace(filter.Context))
					query = query.Where(w => w.ClientName.Contains(filter.Context) ||
											w.Comment.Contains(filter.Context) ||
											w.Text.Contains(filter.Context));

				if (filter.ParentId > 0)
					query = query.Where(w => w.ContractorId == filter.ParentId);

				if (filter.UserId > 0)
					query = query.Where(w => w.SalesUserId == filter.UserId || w.AccountUserId == filter.UserId);

				// даты

				if (filter.From.HasValue)
					query = query.Where(w => w.Date > filter.From);

				if (filter.To.HasValue)
					query = query.Where(w => w.Date < filter.To);

				if (filter.From2.HasValue)
					query = query.Where(w => w.ResponseDate > filter.From2);

				if (filter.To2.HasValue)
					query = query.Where(w => w.ResponseDate < filter.To2);

				// продукт 
				if (filter.Statuses.Count > 0)
					query = query.Where(w => filter.Statuses.Contains(w.ProductId.Value));

				return query.Count();
			}
		}

		public IEnumerable<Request> GetRequests(ListFilter filter)
		{
			using (var db = new LogistoDb())
			{
				var query = db.Requests.AsQueryable();

				// условия поиска

				if (!string.IsNullOrWhiteSpace(filter.Context))
					query = query.Where(w => w.ClientName.Contains(filter.Context) ||
											w.Comment.Contains(filter.Context) ||
											w.Text.Contains(filter.Context));

				if (filter.ParentId > 0)
					query = query.Where(w => w.ContractorId == filter.ParentId);

				if (filter.UserId > 0)
					query = query.Where(w => w.SalesUserId == filter.UserId || w.AccountUserId == filter.UserId);

				// даты

				if (filter.From.HasValue)
					query = query.Where(w => w.Date > filter.From);

				if (filter.To.HasValue)
					query = query.Where(w => w.Date < filter.To);

				if (filter.From2.HasValue)
					query = query.Where(w => w.ResponseDate > filter.From2);

				if (filter.To2.HasValue)
					query = query.Where(w => w.ResponseDate < filter.To2);

				// продукт 
				if (filter.Statuses.Count > 0)
					query = query.Where(w => filter.Statuses.Contains(w.ProductId.Value));

				// сортировка

				if (!string.IsNullOrWhiteSpace(filter.Sort))
				{
					if (string.IsNullOrWhiteSpace(filter.SortDirection))
						filter.SortDirection = "Asc";

					switch (filter.Sort)
					{
						case "ID":
							if (filter.SortDirection == "Asc")
								query = query.OrderBy(o => o.ID);
							else
								query = query.OrderByDescending(o => o.ID);

							break;

						case "ClientName":
							if (filter.SortDirection == "Asc")
								query = query.OrderBy(o => o.ClientName);
							else
								query = query.OrderByDescending(o => o.ClientName);

							break;

						case "AccountUserId":
							if (filter.SortDirection == "Asc")
								query = query.OrderBy(o => o.AccountUserId);
							else
								query = query.OrderByDescending(o => o.AccountUserId);

							break;

						case "Date":
							if (filter.SortDirection == "Asc")
								query = query.OrderBy(o => o.Date);
							else
								query = query.OrderByDescending(o => o.Date);

							break;

						case "ResponseDate":
							if (filter.SortDirection == "Asc")
								query = query.OrderBy(o => o.ResponseDate);
							else
								query = query.OrderByDescending(o => o.ResponseDate);

							break;

						case "SalesUserId":
							if (filter.SortDirection == "Asc")
								query = query.OrderBy(o => o.SalesUserId);
							else
								query = query.OrderByDescending(o => o.SalesUserId);

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

		public Request GetRequest(int id)
		{
			using (var db = new LogistoDb())
			{
				return db.Requests.FirstOrDefault(w => w.ID == id);
			}
		}

		public int CreateRequest(Request request)
		{
			using (var db = new LogistoDb())
			{
				return Convert.ToInt32(db.InsertWithIdentity(request));
			}
		}

		public void UpdateRequest(Request request)
		{
			using (var db = new LogistoDb())
			{
				db.Requests.Where(w => w.ID == request.ID)
				.Set(u => u.ContractorId, request.ContractorId)
				.Set(u => u.AccountUserId, request.AccountUserId)
				.Set(u => u.ClientName, request.ClientName)
				.Set(u => u.Comment, request.Comment)
				.Set(u => u.Date, request.Date)
				.Set(u => u.ProductId, request.ProductId)
				.Set(u => u.ResponseDate, request.ResponseDate)
				.Set(u => u.SalesUserId, request.SalesUserId)
				.Set(u => u.Text, request.Text)
				.Set(u => u.CargoInfo, request.CargoInfo)
				.Set(u => u.Contacts, request.Contacts)
				.Set(u => u.Route, request.Route)
				.Set(u => u.Filename, request.Filename)
				.Update();
			}
		}

		public void DeleteRequest(int id)
		{
			using (var db = new LogistoDb())
			{
				db.Requests.Delete(w => w.ID == id);
			}
		}

		#endregion
	}
}