using System;
using System.Collections.Generic;
using System.Linq;
using LinqToDB;
using Logisto.Data;
using Logisto.Models;

namespace Logisto.BusinessLogic
{
	public class PricelistLogic : IPricelistLogic
	{
		#region Pricelists

		public Pricelist GetPricelist(int id)
		{
			using (var db = new LogistoDb())
			{
				return db.Pricelists.FirstOrDefault(w => w.ID == id);
			}
		}

		public int CreatePricelist(Pricelist entity)
		{
			using (var db = new LogistoDb())
			{
				return Convert.ToInt32(db.InsertWithIdentity(entity));
			}
		}

		public void UpdatePricelist(Pricelist entity)
		{
			using (var db = new LogistoDb())
			{
				db.Pricelists.Where(w => w.ID == entity.ID)
				.Set(u => u.FinRepCenterId, entity.FinRepCenterId)
				.Set(u => u.ContractId, entity.ContractId)
				.Set(u => u.ProductId, entity.ProductId)
				.Set(u => u.Comment, entity.Comment)
				.Set(u => u.Data, entity.Data)
				.Set(u => u.From, entity.From)
				.Set(u => u.Name, entity.Name)
				.Set(u => u.To, entity.To)
				.Update();
			}
		}

		public void DeletePricelist(int id)
		{
			using (var db = new LogistoDb())
			{
				db.Pricelists.Delete(w => w.ID == id);
			}
		}

		public int GetPricelistsCount(ListFilter filter)
		{
			using (var db = new LogistoDb())
			{
				var query = db.Pricelists.AsQueryable();

				// условия поиска
				if (!string.IsNullOrWhiteSpace(filter.Context))
					query = query.Where(w => w.Name.Contains(filter.Context) ||
										w.Comment.Contains(filter.Context));

				if (filter.Type == "Standart")
					query = query.Where(w => w.ContractId == null);

				if (filter.Type == "NonStandart")
					query = query.Where(w => w.ContractId != null);

				return query.Count();
			}
		}

		public IEnumerable<Pricelist> GetPricelists(ListFilter filter)
		{
			using (var db = new LogistoDb())
			{
				var query = db.Pricelists.AsQueryable();

				// условия поиска
				if (!string.IsNullOrWhiteSpace(filter.Context))
					query = query.Where(w => w.Name.Contains(filter.Context) ||
										w.Comment.Contains(filter.Context));

				if (filter.Type == "Standart")
					query = query.Where(w => w.ContractId == null);

				if (filter.Type == "NonStandart")
					query = query.Where(w => w.ContractId != null);


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

						case "Name":
							if (filter.SortDirection == "Asc")
								query = query.OrderBy(o => o.Name);
							else
								query = query.OrderByDescending(o => o.Name);

							break;

						case "Comment":
							if (filter.SortDirection == "Asc")
								query = query.OrderBy(o => o.Comment);
							else
								query = query.OrderByDescending(o => o.Comment);

							break;

						case "ProductId":
							if (filter.SortDirection == "Asc")
								query = query.OrderBy(o => o.ProductId);
							else
								query = query.OrderByDescending(o => o.ProductId);

							break;

						case "FinRepCenterId":
							if (filter.SortDirection == "Asc")
								query = query.OrderBy(o => o.FinRepCenterId);
							else
								query = query.OrderByDescending(o => o.FinRepCenterId);

							break;

						case "ContractId":
							if (filter.SortDirection == "Asc")
								query = query.OrderBy(o => o.ContractId);
							else
								query = query.OrderByDescending(o => o.ContractId);

							break;

						case "From":
							if (filter.SortDirection == "Asc")
								query = query.OrderBy(o => o.From);
							else
								query = query.OrderByDescending(o => o.From);

							break;

						case "To":
							if (filter.SortDirection == "Asc")
								query = query.OrderBy(o => o.To);
							else
								query = query.OrderByDescending(o => o.To);

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

		public IEnumerable<Pricelist> GetPricelists(int legalId)
		{
			using (var db = new LogistoDb())
			{
				var query = from p in db.Pricelists
							join c in db.Contracts on p.ContractId equals c.ID
							where c.LegalId == legalId
							select p;

				return query.ToList();
			}
		}

		public IEnumerable<Pricelist> GetValidPricelists()
		{
			using (var db = new LogistoDb())
			{
				var all = db.Pricelists.ToList();
				List<Pricelist> result = new List<Pricelist>();
				foreach (var item in all)
				{
					if (item.From.HasValue && (item.From > DateTime.Now))
						continue;

					if (item.To.HasValue && (item.To < DateTime.Now))
						continue;

					result.Add(item);
				}

				return result;
			}
		}


		public void ClearPricelist(int id)
		{
			using (var db = new LogistoDb())
			{
				db.Prices.Delete(w => w.PricelistId == id);
			}
		}

		public void InitPricelist(int id, int productId)
		{
			using (var db = new LogistoDb())
			{
				var sks = db.ServiceKinds.Where(w => w.ProductId == productId).ToList();
				foreach (var item in sks)
					db.InsertWithIdentity(new PriceKind
					{
						PricelistId = id,
						ServiceKindId = item.ID,
						Name = item.Name,
						EnName = item.EnName
					});

				var skids = db.ServiceKinds.Where(w => w.ProductId == productId).Select(s => s.ID).ToList();
				var services = db.ServiceTypes.Where(w => skids.Contains(w.ServiceKindId)).ToList();

				var query = from sk in db.ServiceKinds
							join s in db.ServiceTypes on sk.ID equals s.ServiceKindId
							where sk.ProductId == productId
							select new { sk, s };

				foreach (var item in query.ToList())
					db.InsertWithIdentity(new Price
					{
						CurrencyId = 1,
						PricelistId = id,
						ServiceId = item.s.ID,
						VatId = item.sk.VatId.Value,
						Sum = item.s.Price ?? 0,
						Count = item.s.Count ?? 1
					});
			}
		}

		public void FixPricelistKinds(int id, int productId)
		{
			using (var db = new LogistoDb())
			{
				var sks = db.ServiceKinds.Where(w => w.ProductId == productId).ToList();
				foreach (var item in sks)
					if (!db.PriceKinds.Any(w => w.PricelistId == id && w.ServiceKindId == item.ID))
						db.InsertWithIdentity(new PriceKind
						{
							PricelistId = id,
							ServiceKindId = item.ID,
							Name = item.Name,
							EnName = item.EnName
						});
			}
		}

		public void ImportPricelist(int fromPricelistId, int toPricelistId)
		{
			using (var db = new LogistoDb())
			{
				var prices = db.Prices.Where(w => w.PricelistId == fromPricelistId);

				foreach (var item in prices.ToList())
					db.InsertWithIdentity(new Price
					{
						CurrencyId = item.CurrencyId,
						PricelistId = toPricelistId,
						ServiceId = item.ServiceId,
						VatId = item.VatId,
						Sum = item.Sum,
						Count = item.Count
					});
			}
		}

		public void UpdatePricelistData(Pricelist entity)
		{
			using (var db = new LogistoDb())
			{
				db.Pricelists.Where(w => w.ID == entity.ID)
				.Set(u => u.Data, entity.Data)
				.Set(u => u.Filename, entity.Filename)
				.Update();
			}
		}

		#endregion

		public Price GetPrice(int id)
		{
			using (var db = new LogistoDb())
			{
				return db.Prices.FirstOrDefault(w => w.ID == id);
			}
		}

		public void UpdatePrice(Price entity)
		{
			using (var db = new LogistoDb())
			{
				db.Prices.Where(w => w.ID == entity.ID)
				.Set(u => u.Count, entity.Count)
				.Set(u => u.CurrencyId, entity.CurrencyId)
				.Set(u => u.Sum, entity.Sum)
				.Set(u => u.VatId, entity.VatId)
				.Update();
			}
		}

		public IEnumerable<Price> GetPrices(int pricelistId)
		{
			using (var db = new LogistoDb())
			{
				return db.Prices.Where(w => w.PricelistId == pricelistId).ToList();
			}
		}

		public PriceKind GetPriceKind(int id)
		{
			using (var db = new LogistoDb())
			{
				return db.PriceKinds.FirstOrDefault(w => w.ID == id);
			}
		}

		public void UpdatePriceKind(PriceKind entity)
		{
			using (var db = new LogistoDb())
			{
				db.PriceKinds.Where(w => w.ID == entity.ID)
				.Set(u => u.Name, entity.Name)
				.Set(u => u.EnName, entity.EnName)
				.Update();
			}
		}

		public IEnumerable<PriceKind> GetPriceKinds(int pricelistId)
		{
			using (var db = new LogistoDb())
			{
				return db.PriceKinds.Where(w => w.PricelistId == pricelistId).ToList();
			}
		}
	}
}