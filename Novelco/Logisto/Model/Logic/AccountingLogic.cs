using System;
using System.Collections.Generic;
using System.Linq;
using LinqToDB;
using Logisto.Data;
using Logisto.Models;

namespace Logisto.BusinessLogic
{
	public class AccountingLogic : IAccountingLogic
	{
		#region accounting

		public IEnumerable<Accounting> GetAccountings(ListFilter filter)
		{
			using (var db = new LogistoDb())
			{
				var query = db.Accountings.AsQueryable();

				// условия поиска

				if (!string.IsNullOrWhiteSpace(filter.Context))
					query = query.Where(w => w.ActNumber.Contains(filter.Context) ||
						w.Comment.Contains(filter.Context) ||
						w.InvoiceNumber.Contains(filter.Context) ||
						w.Number.Contains(filter.Context)
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

		public int CreateAccounting(Accounting accounting)
		{
			using (var db = new LogistoDb())
			{
				return Convert.ToInt32(db.InsertWithIdentity(accounting));
			}
		}

		public Accounting GetAccounting(int id)
		{
			using (var db = new LogistoDb())
			{
				return db.Accountings.FirstOrDefault(w => w.ID == id);
			}
		}

		public void UpdateAccounting(Accounting accounting)
		{
			using (var db = new LogistoDb())
			{
				db.Accountings.Where(w => w.ID == accounting.ID)
				//.Set(u => u.No, accounting.No)
				.Set(u => u.Vat, accounting.Vat)
				.Set(u => u.Sum, accounting.Sum)
				.Set(u => u.Number, accounting.Number)
				.Set(u => u.Route, accounting.Route)
				.Set(u => u.Comment, accounting.Comment)
				.Set(u => u.ActDate, accounting.ActDate)
				.Set(u => u.RequestDate, accounting.RequestDate)
				.Set(u => u.InvoiceDate, accounting.InvoiceDate)
				.Set(u => u.AccountingDate, accounting.AccountingDate)
				.Set(u => u.PaymentPlanDate, accounting.PaymentPlanDate)
				.Set(u => u.AccountingPaymentMethodId, accounting.AccountingPaymentMethodId)
				.Set(u => u.AccountingDocumentTypeId, accounting.AccountingDocumentTypeId)
				.Set(u => u.AccountingPaymentTypeId, accounting.AccountingPaymentTypeId)
				.Set(u => u.CargoLegalId, accounting.CargoLegalId)
				.Set(u => u.OurLegalId, accounting.OurLegalId)
				.Set(u => u.PayMethodId, accounting.PayMethodId)
				.Set(u => u.ContractId, accounting.ContractId)
				.Set(u => u.LegalId, accounting.LegalId)
				.Set(u => u.VatInvoiceNumber, accounting.VatInvoiceNumber)
				.Set(u => u.InvoiceNumber, accounting.InvoiceNumber)
				.Set(u => u.ActNumber, accounting.ActNumber)
				.Set(u => u.SecondSignerEmployeeId, accounting.SecondSignerEmployeeId)
				.Set(u => u.SecondSignerPosition, accounting.SecondSignerPosition)
				.Set(u => u.SecondSignerInitials, accounting.SecondSignerInitials)
				.Set(u => u.SecondSignerName, accounting.SecondSignerName)
				.Set(u => u.CurrencyRate, accounting.CurrencyRate)
				.Set(u => u.OriginalSum, accounting.OriginalSum)
				.Set(u => u.OriginalVat, accounting.OriginalVat)

				.Set(u => u.ecnExportTO, accounting.ecnExportTO)
				.Set(u => u.ecnExportTOEN, accounting.ecnExportTOEN)
				.Set(u => u.ecnGOcomment, accounting.ecnGOcomment)
				.Set(u => u.ecnGOcommentEN, accounting.ecnGOcommentEN)
				.Set(u => u.ecnGOcontact, accounting.ecnGOcontact)
				.Set(u => u.ecnGOcontactEN, accounting.ecnGOcontactEN)
				.Set(u => u.ecnGOcontTel, accounting.ecnGOcontTel)
				.Set(u => u.ecnGOnameINN, accounting.ecnGOnameINN)
				.Set(u => u.ecnGOnameINNEN, accounting.ecnGOnameINNEN)
				.Set(u => u.ecnGOtime, accounting.ecnGOtime)
				.Set(u => u.ecnGOtimeEN, accounting.ecnGOtimeEN)
				.Set(u => u.ecnGPcomment, accounting.ecnGPcomment)
				.Set(u => u.ecnGPcommentEN, accounting.ecnGPcommentEN)
				.Set(u => u.ecnGPcontact, accounting.ecnGPcontact)
				.Set(u => u.ecnGPcontactEN, accounting.ecnGPcontactEN)
				.Set(u => u.ecnGPconTel, accounting.ecnGPconTel)
				.Set(u => u.ecnGPnameINN, accounting.ecnGPnameINN)
				.Set(u => u.ecnGPnameINNEN, accounting.ecnGPnameINNEN)
				.Set(u => u.ecnGPtime, accounting.ecnGPtime)
				.Set(u => u.ecnGPtimeEN, accounting.ecnGPtimeEN)
				.Set(u => u.ecnImportTO, accounting.ecnImportTO)
				.Set(u => u.ecnImportTOEN, accounting.ecnImportTOEN)
				.Set(u => u.ecnKommentEN, accounting.ecnKommentEN)
				.Set(u => u.ecnLoadingDate, accounting.ecnLoadingDate)
				.Set(u => u.ecnMarshrutEN, accounting.ecnMarshrutEN)
				.Set(u => u.Payment, accounting.Payment)
				.Set(u => u.ecnTransport, accounting.ecnTransport)
				.Set(u => u.ecnTS, accounting.ecnTS)
				.Set(u => u.ecnUnloadingDate, accounting.ecnUnloadingDate)
				.Set(u => u.strGOcontTelEN, accounting.strGOcontTelEN)
				.Set(u => u.strGPcontTelEN, accounting.strGPcontTelEN)
				.Update();
			}
		}

		public void DeleteAccounting(int accountingId)
		{
			using (var db = new LogistoDb())
			{
				db.Accountings.Delete(w => w.ID == accountingId);
			}
		}

		public void DeleteAccounting(int accountingId, bool isCascade)
		{
			using (var db = new LogistoDb())
			{
				if (isCascade)
				{
					db.AccountingMarks.Delete(w => w.AccountingId == accountingId);
					db.Payments.Delete(w => w.AccountingId == accountingId);
					db.Services.Delete(w => w.AccountingId == accountingId);
					db.OrderAccountingRouteSegments.Delete(w => w.AccountingId == accountingId);
					var files = db.TemplatedDocuments.Where(w => w.AccountingId == accountingId).ToList();
					foreach (var file in files)
						db.Delete(file);

					var docs = db.Documents.Where(w => w.AccountingId == accountingId).ToList();
					foreach (var doc in docs)
					{
						db.DocumentData.Delete(w => w.DocumentId == doc.ID);
						db.Delete(doc);
					}
				}

				db.Accountings.Delete(w => w.ID == accountingId);
			}
		}

		public IEnumerable<Accounting> GetAccountingsByOrder(int orderId)
		{
			using (var db = new LogistoDb())
			{
				return db.Accountings.Where(w => w.OrderId == orderId).OrderByDescending(o => o.IsIncome).ThenBy(o => o.SameDirectionNo).ToList();
			}
		}

		public IEnumerable<Accounting> GetAllAccountings()
		{
			using (var db = new LogistoDb())
			{
				return db.Accountings.ToList();
			}
		}

		public int GetAccountingsCountByContractor(int contractorId)
		{
			return GetAccountingsByContractor(contractorId).Count();
		}

		public IEnumerable<Accounting> GetAccountingsByContractor(int contractorId)
		{
			using (var db = new LogistoDb())
			{
				// Для расхода
				var query = from a in db.Accountings
							join ct in db.Contracts on a.ContractId equals ct.ID
							join l in db.Legals on ct.LegalId equals l.ID
							join c in db.Contractors on l.ContractorId equals c.ID
							where c.ID == contractorId && !a.IsIncome
							select a;

				var result = query.ToList();

				// Для дохода
				query = from a in db.Accountings
						from l in db.Legals.Where(w => w.ID == a.LegalId).DefaultIfEmpty()
						where l.ContractorId == contractorId && a.IsIncome
						select a;

				result.AddRange(query.ToList());

				return result.Distinct();
			}
		}

		public IEnumerable<Accounting> GetAccountingsByLegal(int legalId)
		{
			using (var db = new LogistoDb())
			{
				// Для расхода
				var query = from l in db.Legals
							join ct in db.Contracts on l.ID equals ct.LegalId
							join a in db.Accountings on ct.ID equals a.ContractId
							where l.ID == legalId && !a.IsIncome
							select a;

				var result = query.ToList();

				// Для дохода
				query = from a in db.Accountings
						from l in db.Legals.Where(w => w.ID == a.LegalId).DefaultIfEmpty()
						where l.ID == legalId && a.IsIncome
						select a;

				result.AddRange(query.ToList());

				return result.Distinct();
			}
		}

		public IEnumerable<Accounting> GetAccountingsByContract(int contractId)
		{
			using (var db = new LogistoDb())
			{
				// Для расхода
				var query = from ct in db.Contracts
							join a in db.Accountings on ct.ID equals a.ContractId
							where ct.ID == contractId && !a.IsIncome
							select a;

				var result = query.ToList();

				// Для дохода
				query = from a in db.Accountings
						from ct in db.Contracts.Where(w => w.ID == a.ContractId).DefaultIfEmpty()
						where ct.ID == contractId && a.IsIncome
						select a;

				result.AddRange(query.ToList());

				return result.Distinct();
			}
		}

		public Accounting GetAccountingByNumber(string number)
		{
			using (var db = new LogistoDb())
			{
				return db.Accountings.FirstOrDefault(w => w.Number == number);
			}
		}

		public Accounting GetAeAccounting(string number)
		{
			using (var db = new LogistoDb())
			{
				return db.Accountings.FirstOrDefault(w => w.Comment == "ae " + number);
			}
		}

		public Accounting GetApAccounting(string number)
		{
			using (var db = new LogistoDb())
			{
				return db.Accountings.FirstOrDefault(w => w.Comment == "ap " + number);
			}
		}

		public IEnumerable<Accounting> GetAccountingsByInvoiceNumber(string number)
		{
			using (var db = new LogistoDb())
			{
				return db.Accountings.Where(w => w.InvoiceNumber == number).ToList();
			}
		}

		public void AppendAccountingRejectHistory(int accountingId, string message)
		{
			using (var db = new LogistoDb())
			{
				var history = db.Accountings.First(w => w.ID == accountingId).RejectHistory;
				db.Accountings.Where(w => w.ID == accountingId).Set(s => s.RejectHistory, history + "<br/>" + message).Update();
			}
		}

		#endregion

		#region services

		public int CreateService(Service service)
		{
			using (var db = new LogistoDb())
			{
				return Convert.ToInt32(db.InsertWithIdentity(service));
			}
		}

		public Service GetService(int serviceId)
		{
			using (var db = new LogistoDb())
			{
				return db.Services.FirstOrDefault(w => w.ID == serviceId);
			}
		}

		public void UpdateService(Service service)
		{
			using (var db = new LogistoDb())
			{
				db.Services.Where(w => w.ID == service.ID)
								.Set(u => u.ServiceTypeId, service.ServiceTypeId)
								.Set(u => u.Count, service.Count)
								.Set(u => u.Price, service.Price)
								.Set(u => u.Sum, service.Sum)
								.Set(u => u.OriginalSum, service.OriginalSum)
								.Set(u => u.CurrencyId, service.CurrencyId)
								.Set(u => u.VatId, service.VatId)
								.Set(u => u.IsForDetalization, service.IsForDetalization)
								.Update();
			}
		}

		public void DeleteService(int serviceId)
		{
			using (var db = new LogistoDb())
			{
				db.Services.Delete(w => w.ID == serviceId);
			}
		}

		public IEnumerable<Service> GetServicesByAccounting(int accountingId)
		{
			using (var db = new LogistoDb())
			{
				return db.Services.Where(w => w.AccountingId == accountingId).ToList();
			}
		}

		public int? GetAccountingCurrencyId(int accountingId)
		{
			using (var db = new LogistoDb())
			{
				return db.Services.Where(w => w.AccountingId == accountingId).Select(s => s.CurrencyId).FirstOrDefault();
			}
		}

		#endregion

		public IEnumerable<Payment> GetPayments(ListFilter filter)
		{
			using (var db = new LogistoDb())
			{
				var query = db.Payments.AsQueryable();

				// условия поиска

				if (!string.IsNullOrWhiteSpace(filter.Context))
					query = query.Where(w => w.Number.Contains(filter.Context) ||
						w.BaseNumber.Contains(filter.Context) ||
						w.Description.Contains(filter.Context) ||
						(w.TIN == filter.Context)
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

		public IEnumerable<Payment> GetPayments(int accountingId)
		{
			using (var db = new LogistoDb())
			{
				return db.Payments.Where(w => w.AccountingId == accountingId).ToList();
			}
		}

		public int CreatePayment(Payment payment)
		{
			using (var db = new LogistoDb())
			{
				return Convert.ToInt32(db.InsertWithIdentity(payment));
			}
		}

		public Payment GetPayment(int paymentId)
		{
			using (var db = new LogistoDb())
			{
				return db.Payments.FirstOrDefault(w => w.ID == paymentId);
			}
		}

		public void UpdatePayment(Payment payment)
		{
			using (var db = new LogistoDb())
			{
				db.Payments.Where(w => w.ID == payment.ID)
								.Set(u => u.Date, payment.Date)
								.Set(u => u.Sum, payment.Sum)
								.Set(u => u.Number, payment.Number)
								.Set(u => u.Description, payment.Description)
								.Set(u => u.FinReference, payment.FinReference)
								.Set(u => u.AccountingId, payment.AccountingId)
								.Set(u => u.CurrencyId, payment.CurrencyId)
								.Set(u => u.IsMarkingRemoval, payment.IsMarkingRemoval)
								.Set(u => u.BankAccount, payment.BankAccount)
								.Set(u => u.BaseNumber, payment.BaseNumber)
								.Set(u => u.BIC_Swift, payment.BIC_Swift)
								.Set(u => u.KPP, payment.KPP)
								.Set(u => u.TIN, payment.TIN)
								.Update();
			}
		}

		public void DeletePayment(int paymentId)
		{
			using (var db = new LogistoDb())
			{
				db.Payments.Delete(w => w.ID == paymentId);
			}
		}

		public Payment GetPaymentByFinReference(string reference)
		{
			using (var db = new LogistoDb())
			{
				return db.Payments.FirstOrDefault(w => w.FinReference == reference);
			}
		}

		public Dictionary<string, object> GetPaymentInfo(int id)
		{
			using (var db = new LogistoDb())
			{
				var entity = new Dictionary<string, object>();
				var payment = db.Payments.First(w => w.ID == id);

				// основные поля
				entity.Add("ID", payment.ID);
				entity.Add("Number", payment.Number);
				entity.Add("AccountingId", payment.AccountingId);
				entity.Add("BankAccount", payment.BankAccount);
				entity.Add("BaseNumber", payment.BaseNumber);
				entity.Add("BIC_Swift", payment.BIC_Swift);
				entity.Add("Date", payment.Date);
				entity.Add("Description", payment.Description);
				entity.Add("FinReference", payment.FinReference);
				entity.Add("IsIncome", payment.IsIncome);
				entity.Add("KPP", payment.KPP);
				entity.Add("Sum", payment.Sum);
				entity.Add("TIN", payment.TIN);

				// поля из справочников
				entity.Add("Accounting", db.Accountings.Where(w => w.ID == payment.AccountingId).Select(s => s.Number).FirstOrDefault());
				entity.Add("Currency", db.Currencies.Where(w => w.ID == payment.CurrencyId).Select(s => s.Display).FirstOrDefault());

				return entity;
			}
		}

		public IEnumerable<Payment> GetAllPayments()
		{
			using (var db = new LogistoDb())
			{
				return db.Payments.ToList();
			}
		}

		public IEnumerable<Payment> GetPaymentsByContractor(int contractorId)
		{
			using (var db = new LogistoDb())
			{
				// Для расхода
				var query = from a in db.Accountings
							join ct in db.Contracts on a.ContractId equals ct.ID
							join l in db.Legals on ct.LegalId equals l.ID
							join c in db.Contractors on l.ContractorId equals c.ID
							where c.ID == contractorId && !a.IsIncome
							select a.ID;

				var result = query.ToList();

				// Для дохода
				query = from a in db.Accountings
						from l in db.Legals.Where(w => w.ID == a.LegalId).DefaultIfEmpty()
						where l.ContractorId == contractorId && a.IsIncome
						select a.ID;

				result.AddRange(query.ToList());
				var acc = result.Distinct().ToList();
				return db.Payments.Where(w => acc.Contains(w.AccountingId ?? 0)).ToList();
			}
		}

		public void CalculateContractorBalance(int contractorId)
		{
			CalculateContractorAccountingsPayments(contractorId);

			using (var db = new LogistoDb())
			{
				var legals = db.Legals.Where(w => w.ContractorId == contractorId).Select(s => s.ID).ToList();
				double totalIncome = 0;
				double totalExpense = 0;
				double totalIncomePayments = 0;
				double totalExpensePayments = 0;

				foreach (var legalId in legals)
				{
					var query = from a in db.Accountings
								from l in db.Legals.Where(w => w.ID == a.LegalId)
								from ct in db.Contracts.Where(w => w.LegalId == l.ID)
								where l.ID == legalId && a.IsIncome
								select a;

					var income = query.Distinct().Sum(s => s.Sum ?? 0d);
					var incomePayments = query.Distinct().Sum(s => s.Payment ?? 0d);

					query = from a in db.Accountings
							from ct in db.Contracts.Where(w => w.ID == a.ContractId)
							from l in db.Legals.Where(w => w.ID == ct.LegalId)
							where l.ID == legalId && !a.IsIncome
							select a;

					var expense = query.Distinct().Sum(s => s.Sum ?? 0d);
					var expensePayments = query.Distinct().Sum(s => s.Payment ?? 0d);

					db.Legals.Where(w => w.ID == legalId)
						.Set(u => u.Income, income)
						.Set(u => u.PaymentIncome, incomePayments)
						.Set(u => u.Expense, expense)
						.Set(u => u.PaymentExpense, expensePayments)
						.Set(u => u.Balance, income - expense)
						.Set(u => u.PaymentBalance, incomePayments - expensePayments)
						.Update();

					totalIncome += income;
					totalExpense += expense;
					totalIncomePayments += incomePayments;
					totalExpensePayments += expensePayments;
				}

				db.Contractors.Where(w => w.ID == contractorId)
					.Set(u => u.Income, totalIncome)
					.Set(u => u.Expense, totalExpense)
					.Set(u => u.Balance, totalIncome - totalExpense)
					.Set(u => u.PaymentIncome, totalIncomePayments)
					.Set(u => u.PaymentExpense, totalExpensePayments)
					.Set(u => u.PaymentBalance, totalIncomePayments - totalExpensePayments)
					.Update();
			}
		}

		public void CalculateContractorAccountingsPayments(int contractorId)
		{
			using (var db = new LogistoDb())
			{
				var legals = db.Legals.Where(w => w.ContractorId == contractorId).Select(s => s.ID).ToList();
				foreach (var legalId in legals)
				{
					#region расходы

					var accountings = (from a in db.Accountings
									   from ct in db.Contracts.Where(w => w.ID == a.ContractId)
									   from l in db.Legals.Where(w => w.ID == ct.LegalId)
									   where l.ID == legalId && !a.IsIncome
									   select new { a.ID, a.OrderId, a.CurrencyRate, a.InvoiceDate, a.ContractId }).ToList();

					foreach (var accounting in accountings)
					{
						var pSum = (from p in db.Payments where p.AccountingId == accounting.ID select p.Sum).Sum();
						var p1 = (from p in db.Payments where p.AccountingId == accounting.ID select p).FirstOrDefault();
						var s1 = (from p in db.Services where p.AccountingId == accounting.ID select p).FirstOrDefault();
						if ((s1 != null) && (p1 != null) && (p1.CurrencyId != s1.CurrencyId))   // валюта не совпадает
						{
							double rate = 1;
							if (s1.CurrencyId == 1) // счет в рублях
							{
								//if (accounting.CurrencyRate.HasValue)
								//	rate = accounting.CurrencyRate.Value;   // курс жестко задан в доходе
								//else
								{
									var ruse = db.CurrencyRateUses.First(w => w.ID == db.Contracts.First(wc => wc.ID == accounting.ContractId).CurrencyRateUseId);
									float rateUse = ruse?.Value ?? 1;
									var date = p1.Date;
									if ((ruse != null) && (ruse.IsDocumentDate) && (accounting.InvoiceDate.HasValue))
										date = accounting.InvoiceDate.Value;

									rate = (from r in db.CurrencyRates where r.CurrencyId == p1.CurrencyId && r.Date.Value.Date == date.Date select r.Rate).FirstOrDefault() ?? 1;
									rate = rate * rateUse;
								}
							}
							else    // счет в валюте
							{
								//if (accounting.CurrencyRate.HasValue)
								//	rate = 1 / accounting.CurrencyRate.Value;   // курс жестко задан в доходе
								//else
								{
									var ruse = db.CurrencyRateUses.First(w => w.ID == db.Contracts.First(wc => wc.ID == accounting.ContractId).CurrencyRateUseId);
									float rateUse = ruse?.Value ?? 1;
									var date = p1.Date;
									if ((ruse != null) && (ruse.IsDocumentDate) && (accounting.InvoiceDate.HasValue))
										date = accounting.InvoiceDate.Value;

									var rt = (from r in db.CurrencyRates where r.CurrencyId == s1.CurrencyId && r.Date.Value.Date == p1.Date.Date select r.Rate).FirstOrDefault();
									rate = 1 / (rateUse * (from r in db.CurrencyRates where r.CurrencyId == s1.CurrencyId && r.Date.Value.Date == p1.Date.Date select r.Rate).FirstOrDefault() ?? 1);
								}
							}

							if (double.IsInfinity(rate))
								rate = 1;

							pSum = pSum * rate;
						}

						db.Accountings.Where(w => w.ID == accounting.ID).Set(s => s.Payment, pSum).Update();
					}

					#endregion
					#region доходы

					accountings = (from a in db.Accountings
								   where a.LegalId == legalId && a.IsIncome
								   select new { a.ID, a.OrderId, a.CurrencyRate, a.InvoiceDate, a.ContractId }).ToList();

					foreach (var accounting in accountings)
					{
						double pSum = 0;

						var serviceCurrency = (from s in db.Services where s.AccountingId == accounting.ID select s.CurrencyId).FirstOrDefault();
						foreach (var payment in db.Payments.Where(w => w.AccountingId == accounting.ID).Select(s => new { s.CurrencyId, s.Date, s.Sum }).ToList())
						{
							if (payment.CurrencyId != serviceCurrency)
							{
								double rate = 1;
								if (serviceCurrency == 1)
								{
									//if (accounting.CurrencyRate.HasValue)
									//	rate = accounting.CurrencyRate.Value;
									//else
									{
										var contractId = db.Orders.Where(w => w.ID == accounting.OrderId).Select(s => s.ContractId).First();
										var ruse = db.CurrencyRateUses.First(w => w.ID == db.Contracts.First(wc => wc.ID == contractId).CurrencyRateUseId);
										float rateUse = 1;
										if (ruse != null)
											rateUse = ruse.Value;

										rate = (from r in db.CurrencyRates where r.CurrencyId == payment.CurrencyId && r.Date.Value.Date == payment.Date.Date select r.Rate).FirstOrDefault() ?? 1;
										rate = rate * rateUse;
									}
								}
								else
								{
									// тут вообще непонятно получается, запутал Г
									//if (accounting.CurrencyRate.HasValue)
									//	rate = 1 / accounting.CurrencyRate.Value;
									//else
									{
										var contractId = db.Orders.Where(w => w.ID == accounting.OrderId).Select(s => s.ContractId).First();
										var ruse = db.CurrencyRateUses.First(w => w.ID == db.Contracts.Where(wc => wc.ID == contractId).Select(s => s.CurrencyRateUseId).First());
										float rateUse = 1;
										if (ruse != null)
											rateUse = ruse.Value;

										rate = 1 / (rateUse * (from r in db.CurrencyRates where r.CurrencyId == serviceCurrency && r.Date.Value.Date == payment.Date.Date select r.Rate).FirstOrDefault() ?? 1);
									}
								}

								pSum += payment.Sum * rate;
							}
							else
								pSum += payment.Sum;
						}

						db.Accountings.Where(w => w.ID == accounting.ID).Set(s => s.Payment, pSum).Update();
					}

					#endregion
				}
			}
		}

		public void CalculateAccountingBalance(int accountingId)
		{
			double totalOriginalSum = 0;
			double totalOriginalVat = 0;
			double totalSum = 0;
			double totalVat = 0;

			using (var db = new LogistoDb())
			{
				var accounting = db.Accountings.First(w => w.ID == accountingId);
				var services = db.Services.Where(w => w.AccountingId == accountingId).Select(s => new { OriginalSum = s.OriginalSum, VatId = s.VatId, Sum = s.Sum }).ToList();
				var vats = db.Vats.ToList();

				foreach (var service in services)
				{
					if (service.OriginalSum.HasValue && service.VatId.HasValue)
					{
						totalOriginalSum = totalOriginalSum + service.OriginalSum.Value;
						Double tax = vats.First(w => w.ID == service.VatId).Percent;
						totalOriginalVat = totalOriginalVat + (service.OriginalSum.Value - service.OriginalSum.Value / (1 + tax / 100));
					}

					if (service.Sum.HasValue && service.VatId.HasValue)
					{
						totalSum = totalSum + service.Sum.Value;
						Double tax = vats.First(w => w.ID == service.VatId).Percent;
						totalVat = totalVat + (service.Sum.Value - service.Sum.Value / (1 + tax / 100));
					}
				}

				accounting.OriginalSum = Math.Round(totalOriginalSum, 4);
				accounting.OriginalVat = Math.Round(totalOriginalVat, 4);
				accounting.Sum = Math.Round(totalSum, 4);
				accounting.Vat = Math.Round(totalVat, 4);

				db.Accountings.Where(w => w.ID == accounting.ID)
					.Set(u => u.OriginalSum, accounting.OriginalSum)
					.Set(u => u.OriginalVat, accounting.OriginalVat)
					.Set(u => u.Sum, accounting.Sum)
					.Set(u => u.Vat, accounting.Vat)
					.Update();
			}
		}

		public string CalculateServiceBalance(int serviceId)
		{
			string result = string.Empty;
			using (var db = new LogistoDb())
			{
				var service = db.Services.First(w => w.ID == serviceId);
				var accounting = db.Accountings.First(w => w.ID == service.AccountingId.Value);
				var contractId = db.Orders.Where(w => w.ID == accounting.OrderId).Select(s => s.ContractId).First();
				var contract = db.Contracts.First(w => w.ID == (accounting.IsIncome ? contractId : accounting.ContractId.Value));

				service.OriginalSum = service.Count.Value * service.Price.Value;

				if (contract == null)
				{
					service.Sum = -4;
					result = "Договор не выбран";
				}
				else
				{
					if (service.CurrencyId == 1)
					{
						service.Sum = service.OriginalSum;
					}
					else
					{
						//пересчет по курсу в рубли для дохода
						if (accounting.CurrencyRate > 0)
						{
							service.Sum = service.OriginalSum.Value * accounting.CurrencyRate.Value;
						}
						else if (accounting.AccountingDate.HasValue)
						{
							DateTime curDate = accounting.AccountingDate.Value;
							var rate = db.CurrencyRates.Where(w => w.CurrencyId == service.CurrencyId && w.Date <= curDate).OrderByDescending(o => o.Date).FirstOrDefault();
							if (rate != null)
							{
								float rateUse = 1;
								if (contract.CurrencyRateUseId.HasValue)
									rateUse = db.CurrencyRateUses.FirstOrDefault(w => w.ID == contract.CurrencyRateUseId.Value).Value;

								service.Sum = service.OriginalSum.Value * rate.Rate.Value * rateUse;
							}
							else
							{
								service.Sum = -1;
								result = "Нет курса валюты на указанную дату";
							}
						}
						else
						{
							service.Sum = -2;
							result = "В доходе не указана дата пересчета";
						}
					}
				}

				db.Services.Where(w => w.ID == serviceId)
								.Set(u => u.OriginalSum, Math.Round(service.OriginalSum.Value, 4))
								.Set(u => u.Sum, Math.Round(service.Sum.Value, 4))
								.Update();
			}

			return result;
		}

		public string CalculatePaymentPlanDate(int accountingId)
		{
			using (var db = new LogistoDb())
			{
				var accounting = db.Accountings.First(w => w.ID == accountingId);
				var order = db.Orders.First(w => w.ID == accounting.OrderId);

				if (!(accounting.IsIncome ? order.ContractId.HasValue : accounting.ContractId.HasValue))
					return "Не выбран договор.";

				var contract = db.Contracts.First(w => w.ID == (accounting.IsIncome ? order.ContractId.Value : accounting.ContractId.Value));
				if (!contract.PaymentTermsId.HasValue)
					return "Не выбраны условия оплаты в договоре.";

				var term = db.PaymentTerms.First(w => w.ID == contract.PaymentTermsId.Value);

				#region расчет даты по первому условию

				var date = contract.Date.Value;
				if ((term.Condition1_From == 1) && (accounting.InvoiceDate.HasValue))   // от даты счета
					date = accounting.InvoiceDate.Value;

				if ((term.Condition1_From == 2) && (accounting.ActDate.HasValue))   // от даты акта
					date = accounting.ActDate.Value;

				// посчитать дату
				if (term.Condition1_BankDays)   // банковские дни
				{
					byte days = 0;
					while (days < term.Condition1_Days)
					{
						date = date.AddDays(1);
						if ((date.DayOfWeek != DayOfWeek.Sunday) && (date.DayOfWeek != DayOfWeek.Saturday))
							days++;
					}
				}
				else
					date = date.AddDays((double)term.Condition1_Days);  // календарные дни

				// Проверить количество заказов этого контрагента?
				if (term.Condition1_OrdersFrom.HasValue && accounting.ActDate.HasValue)
				{
					var orders = db.Orders.Where(w => w.ContractId == contract.ID).ToList();
					int count = 0;

					foreach (var item in orders)
					{
						var itemAccountings = db.Accountings.Where(w => w.OrderId == item.ID).ToList();
						foreach (var itemAcc in itemAccountings)
							if (itemAcc.ActDate.HasValue && (itemAcc.ActDate < accounting.ActDate))
							{
								count++;
								break;
							}
					}

					if ((count > term.Condition1_OrdersTo) || (count < term.Condition1_OrdersFrom))
						date = DateTime.MinValue;  // сбросить дату, посколько условие не выполнено
				}

				#endregion

				if (date == DateTime.MinValue)
				{
					#region расчет даты по второму условию, раз первая не подходит

					if ((term.Condition2_From == 1) && (accounting.InvoiceDate.HasValue))   // от даты счета
						date = accounting.InvoiceDate.Value;

					if ((term.Condition2_From == 2) && (accounting.ActDate.HasValue))   // от даты акта
						date = accounting.ActDate.Value;

					// посчитать дату
					if (term.Condition2_BankDays)   // банковские дни
					{
						byte days = 0;
						while (days < term.Condition2_Days)
						{
							date = date.AddDays(1);
							if ((date.DayOfWeek != DayOfWeek.Sunday) && (date.DayOfWeek != DayOfWeek.Saturday))
								days++;
						}
					}
					else
						date = date.AddDays((double)term.Condition2_Days);  // календарные дни

					// Проверить количество заказов этого контрагента?
					if (term.Condition2_OrdersFrom.HasValue && accounting.ActDate.HasValue)
					{
						var orders = db.Orders.Where(w => w.ContractId == contract.ID).ToList();
						int count = 0;

						foreach (var item in orders)
						{
							var itemAccountings = db.Accountings.Where(w => w.OrderId == item.ID).ToList();
							foreach (var itemAcc in itemAccountings)
								if (itemAcc.ActDate.HasValue && (itemAcc.ActDate < accounting.ActDate))
								{
									count++;
									break;
								}
						}

						if ((count > term.Condition2_OrdersTo) || (count < term.Condition2_OrdersFrom))
							date = DateTime.MinValue;  // сбросить дату, посколько условие не выполнено
					}

					#endregion
				}

				if (date != DateTime.MinValue)
					db.Accountings.Where(w => w.ID == accounting.ID).Set(u => u.PaymentPlanDate, date).Update();

				return "";
			}
		}

		public IEnumerable<OrderAccountingRouteSegment> GetAccountingRouteSegments(int accountingId)
		{
			using (var db = new LogistoDb())
			{
				return db.OrderAccountingRouteSegments.Where(w => w.AccountingId == accountingId).ToList();
			}
		}

		public int CreateAccountingRouteSegment(OrderAccountingRouteSegment segment)
		{
			using (var db = new LogistoDb())
			{
				return Convert.ToInt32(db.InsertWithIdentity(segment));
			}
		}

		public OrderAccountingRouteSegment GetAccountingRouteSegment(int segmentId)
		{
			using (var db = new LogistoDb())
			{
				return db.OrderAccountingRouteSegments.FirstOrDefault(w => w.ID == segmentId);
			}
		}

		public void UpdateAccountingRouteSegment(OrderAccountingRouteSegment segment)
		{
			using (var db = new LogistoDb())
			{
				db.OrderAccountingRouteSegments.Where(w => w.ID == segment.ID)
				.Set(u => u.AccountingId, segment.AccountingId)
				.Set(u => u.RouteSegmentId, segment.RouteSegmentId)
				.Update();
			}
		}

		public void DeleteAccountingRouteSegment(int segmentId)
		{
			using (var db = new LogistoDb())
			{
				db.OrderAccountingRouteSegments.Delete(w => w.ID == segmentId);
			}
		}

		#region marks

		public int CreateAccountingMark(AccountingMark mark)
		{
			using (var db = new LogistoDb())
			{
				return Convert.ToInt32(db.InsertWithIdentity(mark));
			}
		}

		public IEnumerable<AccountingMark> GetAllAccountingMarks()
		{
			using (var db = new LogistoDb())
			{
				return db.AccountingMarks.ToList();
			}
		}

		public AccountingMark GetAccountingMarkByAccounting(int accountingId)
		{
			using (var db = new LogistoDb())
			{
				return db.AccountingMarks.FirstOrDefault(w => w.AccountingId == accountingId);
			}
		}

		public IEnumerable<AccountingMark> GetAccountingMarksByOrder(int orderId)
		{
			using (var db = new LogistoDb())
			{
				return (from m in db.AccountingMarks
						join a in db.Accountings on m.AccountingId equals a.ID
						where a.OrderId == orderId
						select m).ToList();
			}
		}

		public void UpdateAccountingMark(AccountingMark mark)
		{
			using (var db = new LogistoDb())
			{
				db.AccountingMarks.Where(w => w.ID == mark.ID)
								.Set(u => u.IsAccountingOk, mark.IsAccountingOk)
								.Set(u => u.AccountingOkDate, mark.AccountingOkDate)
								.Set(u => u.AccountingOkUserId, mark.AccountingOkUserId)
								.Set(u => u.IsAccountingChecked, mark.IsAccountingChecked)
								.Set(u => u.AccountingCheckedDate, mark.AccountingCheckedDate)
								.Set(u => u.AccountingCheckedUserId, mark.AccountingCheckedUserId)
								.Set(u => u.IsAccountingRejected, mark.IsAccountingRejected)
								.Set(u => u.AccountingRejectedDate, mark.AccountingRejectedDate)
								.Set(u => u.AccountingRejectedUserId, mark.AccountingRejectedUserId)
								.Set(u => u.AccountingRejectedComment, mark.AccountingRejectedComment)

								.Set(u => u.IsInvoiceOk, mark.IsInvoiceOk)
								.Set(u => u.InvoiceOkDate, mark.InvoiceOkDate)
								.Set(u => u.InvoiceOkUserId, mark.InvoiceOkUserId)
								.Set(u => u.IsInvoiceChecked, mark.IsInvoiceChecked)
								.Set(u => u.InvoiceCheckedDate, mark.InvoiceCheckedDate)
								.Set(u => u.InvoiceCheckedUserId, mark.InvoiceCheckedUserId)
								.Set(u => u.IsInvoiceRejected, mark.IsInvoiceRejected)
								.Set(u => u.InvoiceRejectedDate, mark.InvoiceRejectedDate)
								.Set(u => u.InvoiceRejectedUserId, mark.InvoiceRejectedUserId)
								.Set(u => u.InvoiceRejectedComment, mark.InvoiceRejectedComment)

								.Set(u => u.IsActOk, mark.IsActOk)
								.Set(u => u.ActOkDate, mark.ActOkDate)
								.Set(u => u.ActOkUserId, mark.ActOkUserId)
								.Set(u => u.IsActChecked, mark.IsActChecked)
								.Set(u => u.ActCheckedDate, mark.ActCheckedDate)
								.Set(u => u.ActCheckedUserId, mark.ActCheckedUserId)
								.Set(u => u.IsActRejected, mark.IsActRejected)
								.Set(u => u.ActRejectedDate, mark.ActRejectedDate)
								.Set(u => u.ActRejectedUserId, mark.ActRejectedUserId)
								.Set(u => u.ActRejectedComment, mark.ActRejectedComment)

								//.Set(u => u.IsVatInvoiceOk, mark.IsVatInvoiceOk)
								//.Set(u => u.VatInvoiceOkDate, mark.VatInvoiceOkDate)
								//.Set(u => u.VatInvoiceOkUserId, mark.VatInvoiceOkUserId)
								//.Set(u => u.IsVatInvoiceChecked, mark.IsVatInvoiceChecked)
								//.Set(u => u.VatInvoiceCheckedDate, mark.VatInvoiceCheckedDate)
								//.Set(u => u.VatInvoiceCheckedUserId, mark.VatInvoiceCheckedUserId)
								//.Set(u => u.IsVatInvoiceRejected, mark.IsVatInvoiceRejected)
								//.Set(u => u.VatInvoiceRejectedDate, mark.VatInvoiceRejectedDate)
								//.Set(u => u.VatInvoiceRejectedUserId, mark.VatInvoiceRejectedUserId)
								//.Set(u => u.VatInvoiceRejectedComment, mark.VatInvoiceRejectedComment)
								.Update();
			}
		}

		#endregion

		#region Matchings

		public int CreateAccountingMatching(AccountingMatching entity)
		{
			using (var db = new LogistoDb())
			{
				return Convert.ToInt32(db.InsertWithIdentity(entity));
			}
		}

		public IEnumerable<AccountingMatching> GetAccountingMatchingsByAccounting(int accountingId)
		{
			using (var db = new LogistoDb())
			{
				return db.AccountingMatchings.Where(w => w.ExpenseAccountingId == accountingId).ToList();
			}
		}

		public void UpdateAccountingMatching(AccountingMatching entity)
		{
			using (var db = new LogistoDb())
			{
				db.AccountingMatchings.Where(w => w.ID == entity.ID)
								.Set(u => u.IncomeAccountingId, entity.IncomeAccountingId)
								.Set(u => u.Sum, entity.Sum)
								.Update();
			}
		}

		#endregion
	}
}