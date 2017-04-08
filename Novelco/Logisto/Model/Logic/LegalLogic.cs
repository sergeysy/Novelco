using System;
using System.Collections.Generic;
using System.Linq;
using LinqToDB;
using Logisto.Data;
using Logisto.Models;

namespace Logisto.BusinessLogic
{
	public class LegalLogic : ILegalLogic
	{
		public IEnumerable<Legal> GetLegals(ListFilter filter)
		{
			using (var db = new LogistoDb())
			{
				var query = db.Legals.AsQueryable();

				// условия поиска

				if (!string.IsNullOrWhiteSpace(filter.Context))
					query = query.Where(w =>
						w.Name.Contains(filter.Context) ||
						w.EnName.Contains(filter.Context) ||
						w.DisplayName.Contains(filter.Context) ||
						w.Address.Contains(filter.Context) ||
						w.AddressFact.Contains(filter.Context) ||
						w.EnAddress.Contains(filter.Context) ||
						w.EnAddressFact.Contains(filter.Context) ||
						w.KPP.Contains(filter.Context) ||
						w.OGRN.Contains(filter.Context) ||
						w.OKPO.Contains(filter.Context) ||
						w.OKVED.Contains(filter.Context) ||
						w.TIN.Contains(filter.Context)
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

		public int CreateLegal(Legal legal)
		{
			using (var db = new LogistoDb())
			{
				return Convert.ToInt32(db.InsertWithIdentity(legal));
			}
		}

		public Legal GetLegal(int id)
		{
			using (var db = new LogistoDb())
			{
				return db.Legals.FirstOrDefault(w => w.ID == id);
			}
		}

		public void UpdateLegal(Legal legal)
		{
			using (var db = new LogistoDb())
			{
				db.Legals.Where(w => w.ID == legal.ID)
				.Set(u => u.TaxTypeId, legal.TaxTypeId)
				.Set(u => u.DirectorId, legal.DirectorId)
				.Set(u => u.AccountantId, legal.AccountantId)
				.Set(u => u.Name, legal.Name)
				.Set(u => u.DisplayName, legal.DisplayName)
				.Set(u => u.EnName, legal.EnName)
				.Set(u => u.EnShortName, legal.EnShortName)
				.Set(u => u.TIN, legal.TIN)
				.Set(u => u.OGRN, legal.OGRN)
				.Set(u => u.KPP, legal.KPP)
				.Set(u => u.OKPO, legal.OKPO)
				.Set(u => u.OKVED, legal.OKVED)
				.Set(u => u.Address, legal.Address)
				.Set(u => u.EnAddress, legal.EnAddress)
				.Set(u => u.AddressFact, legal.AddressFact)
				.Set(u => u.EnAddressFact, legal.EnAddressFact)
				.Set(u => u.PostAddress, legal.PostAddress)
				.Set(u => u.EnPostAddress, legal.EnPostAddress)
				.Set(u => u.WorkTime, legal.WorkTime)
				.Set(u => u.TimeZone, legal.TimeZone)
				.Set(u => u.IsNotResident, legal.IsNotResident)
				.Set(u => u.UpdatedBy, legal.UpdatedBy)
				.Set(u => u.UpdatedDate, legal.UpdatedDate)
				.Update();
			}
		}

		public void DeleteLegal(int legalId)
		{
			using (var db = new LogistoDb())
			{
				// TODO: каскадное удаление ?
				//db.Orders.Delete(w => w.ReceiverLegalId == legalId);
				//db.Accountings.Delete(w => w.CargoLegalId == legalId);
				//db.RouteContacts.Delete(w => w.LegalId == legalId);
				//db.RoutePoints.Delete(w => w.ParticipantLegalId == legalId);

				db.Legals.Delete(w => w.ID == legalId);
			}
		}

		public Legal GetLegalByOurLegal(int id)
		{
			using (var db = new LogistoDb())
			{
				var legalId = db.OurLegals.Where(w => w.ID == id).Select(s => s.LegalId).First();
				return db.Legals.FirstOrDefault(w => w.ID == legalId);
			}
		}

		public Dictionary<string, object> GetLegalInfo(int id)
		{
			using (var db = new LogistoDb())
			{
				var entity = new Dictionary<string, object>();
				var legal = db.Legals.First(w => w.ID == id);

				// основные поля
				entity.Add("ID", legal.ID);
				entity.Add("Address", legal.Address);
				entity.Add("AddressFact", legal.AddressFact);
				entity.Add("CreatedDate", legal.CreatedDate);
				entity.Add("DisplayName", legal.DisplayName);
				entity.Add("IsNotResident", legal.IsNotResident);
				entity.Add("KPP", legal.KPP);
				entity.Add("Name", legal.Name);
				entity.Add("OGRN", legal.OGRN);
				entity.Add("OKPO", legal.OKPO);
				entity.Add("OKVED", legal.OKVED);
				entity.Add("PostAddress", legal.PostAddress);
				entity.Add("TIN", legal.TIN);
				entity.Add("WorkTime", legal.WorkTime);

				// поля из справочников
				entity.Add("TaxType", db.TaxTypes.Where(w => w.ID == legal.TaxTypeId).Select(s => s.Display).FirstOrDefault());

				// договоры
				var contracts = db.Contracts.Where(w => w.LegalId == id).Select(s => new
				{
					s.ID,
					s.Date,
					s.Comment,
					s.Number,
					s.IsProlongation
				}).ToList();

				entity.Add("Contracts", contracts);

				return entity;
			}
		}

		public IEnumerable<Phone> GetPhonesByLegal(int legalId)
		{
			using (var db = new LogistoDb())
			{
				var phoneIds = db.LegalPhones.Where(w => w.LegalId == legalId).Select(s => s.PhoneId);
				return db.Phones.Where(w => phoneIds.Contains(w.ID));
			}
		}

		public int CreateRouteContact(RouteContact contact)
		{
			using (var db = new LogistoDb())
			{
				return Convert.ToInt32(db.InsertWithIdentity(contact));
			}
		}

		public RouteContact GetRouteContact(int contactId)
		{
			using (var db = new LogistoDb())
			{
				return db.RouteContacts.FirstOrDefault(w => w.ID == contactId);
			}
		}

		public void UpdateRouteContact(RouteContact contact)
		{
			using (var db = new LogistoDb())
			{
				db.RouteContacts.Where(w => w.ID == contact.ID)
				.Set(u => u.Contact, contact.Contact)
				.Set(u => u.Email, contact.Email)
				.Set(u => u.EnContact, contact.EnContact)
				.Set(u => u.Name, contact.Name)
				.Set(u => u.Phones, contact.Phones)
				.Set(u => u.PlaceId, contact.PlaceId)
				.Set(u => u.Address, contact.Address)
				.Set(u => u.EnAddress, contact.EnAddress)
				.Update();
			}
		}

		public IEnumerable<RouteContact> GetRouteContactsByLegal(int legalId)
		{
			using (var db = new LogistoDb())
			{
				return db.RouteContacts.Where(w => w.LegalId == legalId).OrderBy(o => o.Name);
			}
		}

		// OurLegal

		public int CreateOurLegal(OurLegal legal)
		{
			using (var db = new LogistoDb())
			{
				return Convert.ToInt32(db.InsertWithIdentity(legal));
			}
		}

		public OurLegal GetOurLegal(int id)
		{
			using (var db = new LogistoDb())
			{
				return db.OurLegals.FirstOrDefault(w => w.ID == id);
			}
		}

		public void UpdateOurLegal(OurLegal legal)
		{
			using (var db = new LogistoDb())
			{
				db.OurLegals.Where(w => w.ID == legal.ID)
					.Set(u => u.Name, legal.Name)
					.Set(u => u.LegalId, legal.LegalId)
					.Set(s => s.Sign, legal.Sign)
					.Update();
			}
		}

		public IEnumerable<OurLegal> GetOurLegals()
		{
			using (var db = new LogistoDb())
			{
				return db.OurLegals.ToList();
			}
		}

		public int GetLegalsCountByContractor(int contractorId)
		{
			using (var db = new LogistoDb())
			{
				return db.Legals.Where(w => w.ContractorId == contractorId).Count();
			}
		}

		public IEnumerable<Legal> GetLegalsByContractor(int contractorId)
		{
			using (var db = new LogistoDb())
			{
				return db.Legals.Where(w => w.ContractorId == contractorId).OrderBy(o => o.DisplayName).ToList();
			}
		}

		public Legal GetLegalByAccounting(int accountingId)
		{
			using (var db = new LogistoDb())
			{
				var accounting = db.Accountings.First(w => w.ID == accountingId);
				if (accounting.IsIncome)
					return db.Legals.Where(w => w.ID == accounting.LegalId).First();
				else
				{
					var contract = db.Contracts.First(w => w.ID == accounting.ContractId);
					return db.Legals.Where(w => w.ID == contract.LegalId).First();
				}
			}
		}
	}
}