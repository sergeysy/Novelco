using System;
using System.Collections.Generic;
using System.Linq;
using LinqToDB;
using Logisto.Data;
using Logisto.Models;

namespace Logisto.BusinessLogic
{
	public class ContractorLogic : IContractorLogic
	{
		public int GetContractorsCount(ListFilter filter)
		{
			using (var db = new LogistoDb())
			{
				var query = db.Contractors.AsQueryable();

				// условия поиска

				if (!string.IsNullOrWhiteSpace(filter.Context))
					query = query.Where(w => w.Name.Contains(filter.Context)
						);

				if (filter.Type == "Client")
				{
					var legalIds = db.Contracts.Where(w => w.ContractServiceTypeId != 2).Select(s => s.LegalId).Distinct().ToList();
					var contractorIds = db.Legals.Where(w => legalIds.Contains(w.ID)).Select(s => s.ContractorId).Distinct().ToList();
					query = query.Where(w => contractorIds.Contains(w.ID));
				}

				return query.Count();
			}
		}

		public IEnumerable<Contractor> GetContractors(ListFilter filter)
		{
			using (var db = new LogistoDb())
			{
				var query = db.Contractors.AsQueryable();

				// условия поиска

				if (!string.IsNullOrWhiteSpace(filter.Context))
					query = query.Where(w => w.Name.Contains(filter.Context)
						);

				if (filter.Type == "Client")
				{
					var legalIds = db.Contracts.Where(w => w.ContractServiceTypeId != 2).Select(s => s.LegalId).Distinct().ToList();
					var contractorIds = db.Legals.Where(w => legalIds.Contains(w.ID)).Select(s => s.ContractorId).Distinct().ToList();
					query = query.Where(w => contractorIds.Contains(w.ID));
				}

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

						case "CreatedDate":
							if (filter.SortDirection == "Asc")
								query = query.OrderBy(o => o.CreatedDate);
							else
								query = query.OrderByDescending(o => o.CreatedDate);

							break;

						case "Balance":
							if (filter.SortDirection == "Asc")
								query = query.OrderBy(o => o.Balance);
							else
								query = query.OrderByDescending(o => o.Balance);

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

		public int CreateContractor(Contractor contractor)
		{
			using (var db = new LogistoDb())
			{
				return Convert.ToInt32(db.InsertWithIdentity(contractor));
			}
		}

		public Contractor GetContractor(int id)
		{
			using (var db = new LogistoDb())
			{
				return db.Contractors.FirstOrDefault(w => w.ID == id);
			}
		}

		public Contractor GetContractorByAccounting(int accountingId)
		{
			using (var db = new LogistoDb())
			{
				var accounting = db.Accountings.First(w => w.ID == accountingId);
				if (accounting.IsIncome)
					return (from cn in db.Contractors
							join l in db.Legals on cn.ID equals l.ContractorId
							join c in db.Contracts on l.ID equals c.LegalId
							join o in db.Orders on c.ID equals o.ContractId
							where o.ID == accounting.OrderId
							select cn).FirstOrDefault();
				else
					return (from cn in db.Contractors
							join l in db.Legals on cn.ID equals l.ContractorId
							join c in db.Contracts on l.ID equals c.LegalId
							where c.ID == accounting.ContractId
							select cn).FirstOrDefault();
			}
		}

		public Contractor GetContractorByContract(int contractId)
		{
			using (var db = new LogistoDb())
			{
				var contract = db.Contracts.First(w => w.ID == contractId);
				var legal = db.Legals.First(w => w.ID == contract.LegalId);
				return db.Contractors.First(w => w.ID == legal.ContractorId);
			}
		}

		public Contractor GetContractorByLegal(int legalId)
		{
			using (var db = new LogistoDb())
			{
				var cId = db.Legals.Where(w => w.ID == legalId).Select(s => s.ContractorId).First();
				return db.Contractors.First(w => w.ID == cId);
			}
		}

		public void UpdateContractor(Contractor contractor)
		{
			using (var db = new LogistoDb())
			{
				db.Contractors.Where(w => w.ID == contractor.ID)
				.Set(u => u.Name, contractor.Name)
				.Set(u => u.IsLocked, contractor.IsLocked)
				.Update();
			}
		}

		public ContractorEmployeeSettings GetContractorEmployeeSettings(int employeeId)
		{
			using (var db = new LogistoDb())
			{
				return db.ContractorEmployeeSettings.FirstOrDefault(w => w.EmployeeId == employeeId);
			}
		}

		public void UpdateContractorEmployeeSettings(ContractorEmployeeSettings entity)
		{
			using (var db = new LogistoDb())
			{
				db.ContractorEmployeeSettings.Where(w => w.EmployeeId == entity.EmployeeId)
				.Set(u => u.IsEnUI, entity.IsEnUI)
				.Set(u => u.NotifyDocumentChanged, entity.NotifyDocumentChanged)
				.Set(u => u.NotifyDocumentCreated, entity.NotifyDocumentCreated)
				.Set(u => u.NotifyDocumentDeleted, entity.NotifyDocumentDeleted)
				.Set(u => u.NotifyEventCreated, entity.NotifyEventCreated)
				.Set(u => u.NotifyStatusChanged, entity.NotifyStatusChanged)
				.Set(u => u.NotifyTemplatedDocumentChanged, entity.NotifyTemplatedDocumentChanged)
				.Set(u => u.NotifyTemplatedDocumentCreated, entity.NotifyTemplatedDocumentCreated)
				.Set(u => u.Password, entity.Password)
				.Update();
			}
		}
	}
}