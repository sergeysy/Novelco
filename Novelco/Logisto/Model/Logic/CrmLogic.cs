using System;
using System.Collections.Generic;
using System.Linq;
using Logisto.Data;
using Logisto.Models;
using LinqToDB;

namespace Logisto.BusinessLogic
{
	public class CrmLogic : ICrmLogic
	{
		public int GetCallsCount(ListFilter filter)
		{
			using (var db = new LogistoDb())
			{
				var query = db.CrmCalls.AsQueryable();

				// условия поиска

				if (!string.IsNullOrWhiteSpace(filter.Context))
					query = query.Where(w => w.Direction.Contains(filter.Context)
										);
				// даты

				if (filter.From.HasValue)
					query = query.Where(w => w.Date >= filter.From);

				if (filter.To.HasValue)
					query = query.Where(w => w.Date <= filter.To);

				// user

				if (filter.UserId > 0)
				{
					var crmId = db.Users.Where(w => w.ID == filter.UserId).Select(s => s.CrmId).FirstOrDefault();
					if (crmId.HasValue)
						query = query.Where(w => w.ID_MANAGER == crmId);
				}

				return query.Count();
			}
		}

		public IEnumerable<CrmCall> GetCalls(ListFilter filter)
		{
			using (var db = new LogistoDb())
			{
				var query = db.CrmCalls.AsQueryable();

				// условия поиска

				if (!string.IsNullOrWhiteSpace(filter.Context))
					query = query.Where(w => w.Direction.Contains(filter.Context)
										);

				// даты

				if (filter.From.HasValue)
					query = query.Where(w => w.Date >= filter.From);

				if (filter.To.HasValue)
					query = query.Where(w => w.Date <= filter.To);

				// user

				if (filter.UserId > 0)
				{
					var crmId = db.Users.Where(w => w.ID == filter.UserId).Select(s => s.CrmId).FirstOrDefault();
					if (crmId.HasValue)
						query = query.Where(w => w.ID_MANAGER == crmId);
				}

				// TODO: sort

				// пейджинг

				if (filter.PageNumber > 0)
					query = query.Skip(filter.PageNumber * filter.PageSize);

				if (filter.PageSize > 0)
					query = query.Take(filter.PageSize);

				return query.ToList();
			}
		}

		public bool IsFirstCall(CrmCall call)
		{
			using (var db = new LogistoDb())
			{
				return db.CrmCalls.Any(w => w.ID_COMPANY == call.ID_COMPANY && w.ID < call.ID);
			}
		}

		public IEnumerable<CrmManager> GetAllManagers()
		{
			using (var db = new LogistoDb())
			{
				return db.CrmManagers.ToList();
			}
		}

        public int GetLegalsCount(ListFilter filter)
        {
            using (var db = new LogistoDb())
            {
                var query = db.CrmLegals.AsQueryable();

                // условия поиска

                if (!string.IsNullOrWhiteSpace(filter.Context))
                    query = query.Where(w => w.CompanyName.Contains(filter.Context)
                                        );

                return query.Count();
            }
        }

        public IEnumerable<CrmLegal> GetLegals(ListFilter filter)
        {
            using (var db = new LogistoDb())
            {
                var query = db.CrmLegals.AsQueryable();

                // условия поиска

                if (!string.IsNullOrWhiteSpace(filter.Context))
                    query = query.Where(w => w.CompanyName.Contains(filter.Context)
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
    }
}