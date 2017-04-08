using System;
using System.Collections.Generic;
using System.Linq;
using LinqToDB;
using Logisto.Data;
using Logisto.Models;

namespace Logisto.BusinessLogic
{
	public class BankLogic : IBankLogic
	{
		#region bank account

		public int CreateBankAccount(BankAccount account)
		{
			using (var db = new LogistoDb())
			{
				var id = Convert.ToInt32(db.InsertWithIdentity(account));
				return id;
			}
		}

		public BankAccount GetBankAccount(int id)
		{
			using (var db = new LogistoDb())
			{
				return db.BankAccounts.FirstOrDefault(w => w.ID == id);
			}
		}

		public void UpdateBankAccount(BankAccount account)
		{
			using (var db = new LogistoDb())
			{
				db.BankAccounts.Where(w => w.ID == account.ID)
				.Set(u => u.Number, account.Number)
				.Set(u => u.CurrencyId, account.CurrencyId)
				.Set(u => u.BankId, account.BankId)
				.Set(u => u.CoBankName, account.CoBankName)
				.Set(u => u.CoBankAccount, account.CoBankAccount)
				.Set(u => u.CoBankSWIFT, account.CoBankSWIFT)
				.Set(u => u.CoBankIBAN, account.CoBankIBAN)
				.Set(u => u.CoBankAddress, account.CoBankAddress)
				.Update();
			}
		}

		public void DeleteBankAccount(int id)
		{
			using (var db = new LogistoDb())
			{
				db.BankAccounts.Delete(w => w.ID == id);
			}
		}

		public IEnumerable<BankAccount> GetBankAccountsByLegal(int legalId)
		{
			using (var db = new LogistoDb())
			{
				return db.BankAccounts.Where(w => w.LegalId == legalId).ToList();
			}
		}

		#endregion

		#region banks

		public IEnumerable<Bank> SearchBanks(string bic)
		{
			using (var db = new LogistoDb())
			{
				return db.Banks.Where(w => w.BIC.Contains(bic)).ToList();
			}
		}

		public int GetBanksCount(ListFilter filter)
		{
			using (var db = new LogistoDb())
			{
				var query = db.Banks.AsQueryable();

				// условия поиска

				if (!string.IsNullOrWhiteSpace(filter.Context))
					query = query.Where(w => w.BIC.Contains(filter.Context) ||
						w.Name.Contains(filter.Context) ||
						w.NNP.Contains(filter.Context) ||
						w.SWIFT.Contains(filter.Context)
						);

				return query.Count();
			}
		}

		public IEnumerable<Bank> GetBanks(ListFilter filter)
		{
			using (var db = new LogistoDb())
			{
				var query = db.Banks.AsQueryable();

				// условия поиска

				if (!string.IsNullOrWhiteSpace(filter.Context))
					query = query.Where(w => w.BIC.Contains(filter.Context) ||
						w.Name.Contains(filter.Context) ||
						w.NNP.Contains(filter.Context) ||
						w.SWIFT.Contains(filter.Context)
						);

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

						case "BIC":
							if (filter.SortDirection == "Asc")
								query = query.OrderBy(o => o.BIC);
							else
								query = query.OrderByDescending(o => o.BIC);

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

		public Bank GetBank(int id)
		{
			using (var db = new LogistoDb())
			{
				return db.Banks.FirstOrDefault(w => w.ID == id);
			}
		}

		public int CreateBank(Bank bank)
		{
			using (var db = new LogistoDb())
			{
				return Convert.ToInt32(db.InsertWithIdentity(bank));
			}
		}

		public void UpdateBank(Bank bank)
		{
			using (var db = new LogistoDb())
			{
				db.Banks.Where(w => w.ID == bank.ID)
				.Set(u => u.BIC, bank.BIC)
				.Set(u => u.KSNP, bank.KSNP)
				.Set(u => u.Name, bank.Name)
				.Set(u => u.NNP, bank.NNP)
				.Set(u => u.PZN, bank.PZN)
				.Set(u => u.SWIFT, bank.SWIFT)
				.Set(u => u.TNP, bank.TNP)
				.Set(u => u.UER, bank.UER)
				.Update();
			}
		}

		#endregion
	}
}