using System;
using System.Collections.Generic;
using System.Linq;
using Logisto.Data;
using Logisto.Models;
using LinqToDB;

namespace Logisto.BusinessLogic
{
	public class ContractLogic : IContractLogic
	{
		public int GetContractsCount(ListFilter filter)
		{
			using (var db = new LogistoDb())
			{
				var query = db.Contracts.AsQueryable();

				// условия поиска

				if (!string.IsNullOrWhiteSpace(filter.Context))
					query = query.Where(w => w.Number.Contains(filter.Context) ||
										w.Comment.Contains(filter.Context)
										);

				if (filter.Type == "Client")
					query = query.Where(w => w.ContractServiceTypeId != 2);

				// даты

				if (filter.From.HasValue)
					query = query.Where(w => w.Date > filter.From);

				if (filter.To.HasValue)
					query = query.Where(w => w.Date < filter.To);

				return query.Count();
			}

		}

		public IEnumerable<Contract> GetContracts(ListFilter filter)
		{
			using (var db = new LogistoDb())
			{
				var query = db.Contracts.AsQueryable();

				// условия поиска

				if (!string.IsNullOrWhiteSpace(filter.Context))
					query = query.Where(w => w.Number.Contains(filter.Context) ||
										w.Comment.Contains(filter.Context)
										);

				if (filter.Type == "Client")
					query = query.Where(w => w.ContractServiceTypeId != 2);

				// даты

				if (filter.From.HasValue)
					query = query.Where(w => w.Date > filter.From);

				if (filter.To.HasValue)
					query = query.Where(w => w.Date < filter.To);

				// TODO: sort

				// пейджинг

				if (filter.PageNumber > 0)
					query = query.Skip(filter.PageNumber * filter.PageSize);

				if (filter.PageSize > 0)
					query = query.Take(filter.PageSize);

				return query.ToList();
			}
		}

		public int CreateContract(Contract contract)
		{
			using (var db = new LogistoDb())
			{
				return Convert.ToInt32(db.InsertWithIdentity(contract));
			}
		}

		public Contract GetContract(int id)
		{
			using (var db = new LogistoDb())
			{
				return db.Contracts.FirstOrDefault(w => w.ID == id);
			}
		}

		public void UpdateContract(Contract contract)
		{
			using (var db = new LogistoDb())
			{
				db.Contracts.Where(w => w.ID == contract.ID)
				.Set(u => u.Number, contract.Number)
				.Set(u => u.CurrencyRateUseId, contract.CurrencyRateUseId)
				.Set(u => u.LegalId, contract.LegalId)
				.Set(u => u.OurLegalId, contract.OurLegalId)
				.Set(u => u.BankAccountId, contract.BankAccountId)
				.Set(u => u.ContractRoleId, contract.ContractRoleId)
				.Set(u => u.ContractTypeId, contract.ContractTypeId)
				.Set(u => u.PaymentTermsId, contract.PaymentTermsId)
				.Set(u => u.OurBankAccountId, contract.OurBankAccountId)
				.Set(u => u.OurContractRoleId, contract.OurContractRoleId)
				.Set(u => u.ContractServiceTypeId, contract.ContractServiceTypeId)
				.Set(u => u.Comment, contract.Comment)
				.Set(u => u.IsFixed, contract.IsFixed)
				.Set(u => u.IsProlongation, contract.IsProlongation)
				.Set(u => u.Date, contract.Date)
				.Set(u => u.BeginDate, contract.BeginDate)
				.Set(u => u.EndDate, contract.EndDate)
				.Set(u => u.PayMethodId, contract.PayMethodId)
				.Set(u => u.AgentPercentage, contract.AgentPercentage)
				.Update();
			}
		}

		public void DeleteContract(int id)
		{
			using (var db = new LogistoDb())
			{
				db.Contracts.Delete(w => w.ID == id);
			}
		}

		public IEnumerable<Contract> GetContractsByLegal(int legalId)
		{
			using (var db = new LogistoDb())
			{
				return db.Contracts.Where(w => w.LegalId == legalId).ToList();
			}
		}

		public IEnumerable<Contract> GetContractsByContractor(int contractorId)
		{
			using (var db = new LogistoDb())
			{
				var query = from c in db.Contracts
							join l in db.Legals on c.LegalId equals l.ID
							where l.ContractorId == contractorId
							select c;

				return query.ToList();
			}
		}

		public Dictionary<string, object> GetContractInfo(int id)
		{
			using (var db = new LogistoDb())
			{
				var entity = new Dictionary<string, object>();
				var contract = db.Contracts.First(w => w.ID == id);

				// основные поля
				entity.Add("ID", contract.ID);
				entity.Add("Number", contract.Number);
				entity.Add("Date", contract.Date);
				entity.Add("BeginDate", contract.BeginDate);
				entity.Add("EndDate", contract.EndDate);
				entity.Add("Comment", contract.Comment);
				entity.Add("IsFixed", contract.IsFixed);
				entity.Add("IsProlongation", contract.IsProlongation);
				entity.Add("LegalId", contract.LegalId);

				// поля из справочников
				var ourLegal = db.OurLegals.First(w => w.ID == contract.OurLegalId);
				entity.Add("OurLegal", db.Legals.Where(w => w.ID == ourLegal.LegalId).Select(s => s.DisplayName).FirstOrDefault());
				entity.Add("Legal", db.Legals.Where(w => w.ID == contract.LegalId).Select(s => s.DisplayName).FirstOrDefault());
				entity.Add("OurBankAccount", db.BankAccounts.Where(w => w.ID == contract.OurBankAccountId).Select(s => s.Number).FirstOrDefault());
				entity.Add("BankAccount", db.BankAccounts.Where(w => w.ID == contract.BankAccountId).Select(s => s.Number).FirstOrDefault());
				entity.Add("OurContractRole", db.ContractRoles.Where(w => w.ID == contract.OurContractRoleId).Select(s => s.Display).FirstOrDefault());
				entity.Add("ContractRole", db.ContractRoles.Where(w => w.ID == contract.ContractRoleId).Select(s => s.Display).FirstOrDefault());
				entity.Add("ContractServiceType", db.ContractServiceTypes.Where(w => w.ID == contract.ContractServiceTypeId).Select(s => s.Display).FirstOrDefault());
				entity.Add("ContractType", db.ContractTypes.Where(w => w.ID == contract.ContractTypeId).Select(s => s.Display).FirstOrDefault());
				entity.Add("PaymentTerm", db.PaymentTerms.Where(w => w.ID == contract.PaymentTermsId).Select(s => s.Display).FirstOrDefault());
				entity.Add("CurrencyRateUse", db.CurrencyRateUses.Where(w => w.ID == contract.CurrencyRateUseId).Select(s => s.Display).FirstOrDefault());

				var q = from cc in db.ContractCurrencies
						from c in db.Currencies.Where(w => w.ID == cc.CurrencyId).DefaultIfEmpty()
						from oa in db.BankAccounts.Where(woa => woa.ID == cc.OurBankAccountId).DefaultIfEmpty()
						from oab in db.Banks.Where(woab => woab.ID == oa.BankId).DefaultIfEmpty()
						from a in db.BankAccounts.Where(wa => wa.ID == cc.BankAccountId).DefaultIfEmpty()
						from ab in db.Banks.Where(wab => wab.ID == a.BankId).DefaultIfEmpty()
						where cc.ContractId == id
						select new { Display = c.Display, OurBankAccount = oa.Number, OurBank = oab.Name, BankAccount = a.Number, Bank = ab.Name ?? a.CoBankName };

				entity.Add("Currencies", q.ToList());

				var personByUsers = (from u in db.Users
									 from p in db.Persons.Where(w => w.ID == u.PersonId).DefaultIfEmpty()
									 select new { u.ID, p.DisplayName }).ToList();

				var marks = db.ContractMarks.Where(w => w.ContractId == id).Select(s => new
																							{
																								s.IsContractOk,
																								s.ContractOkDate,
																								s.ContractOkUserId,
																								ContractOkUser = personByUsers.Where(pbuw => pbuw.ID == s.ContractOkUserId).Select(pbu => pbu.DisplayName).FirstOrDefault(),

																								s.IsContractChecked,
																								s.ContractCheckedDate,
																								s.ContractCheckedUserId,
																								ContractCheckedUser = personByUsers.Where(pbuw => pbuw.ID == s.ContractCheckedUserId).Select(pbu => pbu.DisplayName).FirstOrDefault(),

																								s.IsContractRejected,
																								s.ContractRejectedDate,
																								s.ContractRejectedUserId,
																								ContractRejectedUser = personByUsers.Where(pbuw => pbuw.ID == s.ContractRejectedUserId).Select(pbu => pbu.DisplayName).FirstOrDefault(),
																								s.ContractRejectedComment,

																								s.IsContractBlocked,
																								s.ContractBlockedDate,
																								s.ContractBlockedUserId,
																								ContractBlockedUser = personByUsers.Where(pbuw => pbuw.ID == s.ContractBlockedUserId).Select(pbu => pbu.DisplayName).FirstOrDefault(),
																								s.ContractBlockedComment
																							}).FirstOrDefault();

				entity.Add("Marks", marks);
				
				return entity;
			}
		}


		//
		public ContractCurrency GetContractCurrency(int contractId, int currencyId)
		{
			using (var db = new LogistoDb()) { return db.ContractCurrencies.FirstOrDefault(w => w.ContractId == contractId && w.CurrencyId == currencyId); }
		}

		public IEnumerable<Currency> GetCurrenciesByContract(int contractId)
		{
			using (var db = new LogistoDb())
			{
				return (from c in db.Currencies
						join cc in db.ContractCurrencies on c.ID equals cc.CurrencyId
						where cc.ContractId == contractId
						select c).ToList();
			}
		}

		public IEnumerable<ContractCurrency> GetContractCurrencies(int contractId)
		{
			using (var db = new LogistoDb())
			{
				return db.ContractCurrencies.Where(w => w.ContractId == contractId).ToList();
			}
		}

		public void CreateContractCurrency(ContractCurrency entity)
		{
			using (var db = new LogistoDb())
			{
				db.InsertWithIdentity(entity);
			}
		}

		public void UpdateContractCurrency(ContractCurrency entity)
		{
			using (var db = new LogistoDb())
			{
				db.ContractCurrencies.Where(w => w.ContractId == entity.ContractId && w.CurrencyId == entity.CurrencyId)
					.Set(s => s.BankAccountId, entity.BankAccountId)
					.Set(s => s.OurBankAccountId, entity.OurBankAccountId)
					.Update();
			}
		}

		public void DeleteContractCurrency(int contractId, int currencyId)
		{
			using (var db = new LogistoDb()) { db.ContractCurrencies.Delete(w => w.ContractId == contractId && w.CurrencyId == currencyId); }
		}

		#region marks

		public ContractMark GetContractMark(int id)
		{
			using (var db = new LogistoDb())
			{
				return db.ContractMarks.First(w => w.ID == id);
			}
		}

		public ContractMark GetContractMarkByContract(int contractId)
		{
			using (var db = new LogistoDb())
			{
				return db.ContractMarks.FirstOrDefault(w => w.ContractId == contractId);
			}
		}

		public int CreateContractMark(ContractMark entity)
		{
			using (var db = new LogistoDb())
			{
				return Convert.ToInt32(db.InsertWithIdentity(entity));
			}
		}

		public void UpdateContractMark(ContractMark mark)
		{
			using (var db = new LogistoDb())
			{
				db.ContractMarks.Where(w => w.ID == mark.ID)
				.Set(s => s.ContractBlockedDate, mark.ContractBlockedDate)
				.Set(s => s.ContractCheckedDate, mark.ContractCheckedDate)
				.Set(s => s.ContractOkDate, mark.ContractOkDate)
				.Set(s => s.ContractRejectedComment, mark.ContractRejectedComment)
				.Set(s => s.ContractRejectedDate, mark.ContractRejectedDate)
				.Set(s => s.IsContractBlocked, mark.IsContractBlocked)
				.Set(s => s.IsContractChecked, mark.IsContractChecked)
				.Set(s => s.IsContractOk, mark.IsContractOk)
				.Set(s => s.IsContractRejected, mark.IsContractRejected)
				.Set(s => s.ContractBlockedUserId, mark.ContractBlockedUserId)
				.Set(s => s.ContractCheckedUserId, mark.ContractCheckedUserId)
				.Set(s => s.ContractOkUserId, mark.ContractOkUserId)
				.Set(s => s.ContractRejectedUserId, mark.ContractRejectedUserId)
				.Update();
			}
		}

		#endregion

		#region contract marks history

		public int CreateContractMarksHistory(ContractMarksHistory entity)
		{
			using (var db = new LogistoDb())
			{
				return Convert.ToInt32(db.InsertWithIdentity(entity));
			}
		}

		public IEnumerable<ContractMarksHistory> GetContractMarksHistory(int contractId)
		{
			using (var db = new LogistoDb())
			{
				return db.ContractMarksHistory.Where(w => w.ContractId == contractId).OrderBy(o => o.ID).ToList();
			}
		}

		#endregion
	}
}