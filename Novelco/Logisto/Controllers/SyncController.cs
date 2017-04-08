using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Logisto.Models;
using LinqToDB;
using System.Text.RegularExpressions;

namespace Logisto.Controllers
{
	[Authorize]
	public class SyncController : BaseController
	{
		#region Pages

		public ActionResult Queue()
		{
			var list = dataLogic.GetSyncQueue();
			var viewModel = list.Select(s => new
			{
				ID = s.ID,
				AccountingNumber = accountingLogic.GetAccounting(s.AccountingId)?.Number,
				Error = s.Error
			}).ToList();
			return View("../Data/SyncQueue", viewModel);
		}

		public ActionResult FixPayments()
		{
			FixPaymentsLinks();
			FixPaymentsLinks2();
			return Redirect("~");
		}

		#endregion

		public ContentResult FinSync()
		{
			if (!IsSuperUser())
				return Content("У вас нет прав на это действие.");

			return Content(FinSyncronize());
		}

		public ContentResult DeleteSyncItem(int id)
		{
			if (!IsSuperUser())
				return Content("У вас нет прав на это действие.");

			dataLogic.DeleteSyncQueue(id);
			return Content("");
		}

		internal string FinSyncronize()
		{
			var setting = dataLogic.GetSystemSettings().First(w => w.Name == "LastFinSyncDate");
			var lastSyncDate = DateTime.Parse(setting.Value);
			var client = new Novelco.ExchangeNovelcoPortTypeClient();

			#region upload

			Dictionary<int, string> errors = new Dictionary<int, string>();
			Dictionary<int, string> success = new Dictionary<int, string>();

			var queue = dataLogic.GetSyncQueue();

			foreach (var qItem in queue)
			{
				var id = qItem.AccountingId;
				var accounting = accountingLogic.GetAccounting(id);
				if (accounting == null)
				{
					success.Add(id, "Доход/расход не найден #" + id);
					dataLogic.DeleteSyncQueue(qItem.ID);
					continue;
				}

				var order = orderLogic.GetOrder(accounting.OrderId);
				if (accounting.IsIncome && (order.ProductId == 10))  // доходы по Торговому Агентированию исключаем
				{
					success.Add(id, "Акт Торгового Агентирования исключен #" + id);
					dataLogic.DeleteSyncQueue(qItem.ID);
					continue;
				}

				var contract = accounting.IsIncome ? contractLogic.GetContract(order.ContractId.Value) : contractLogic.GetContract(accounting.ContractId.Value);
				var legal = legalLogic.GetLegal(contract.LegalId);
				var contractor = contractorLogic.GetContractor(legal.ContractorId.Value);
				if (contractor.Name.StartsWith("#"))
				{
					success.Add(id, "Неучитываемый акт исключен #" + id);
					dataLogic.DeleteSyncQueue(qItem.ID);
					continue;
				}

				if (!accounting.IsIncome && (contractor.Name == "ТАМОЖНЯ")) // расходы по ТАМОЖНЯ исключаем
				{
					success.Add(id, "Неучитываемый акт исключен #" + id);
					dataLogic.DeleteSyncQueue(qItem.ID);
					continue;
				}

				var services = accountingLogic.GetServicesByAccounting(accounting.ID);
				var service = services.FirstOrDefault();
				if (service == null)
				{
					qItem.Error = "Нет ни одной услуги для доход/расхода #" + id;
					errors.Add(id, "Нет ни одной услуги для доход/расхода");
					dataLogic.UpdateSyncQueue(qItem);
					continue;
				}

				//if (!accounting.IsIncome && dataLogic.GetServiceType(service.ServiceTypeId.Value).Name == "Организация страхования груза")
				//{
				//	success.Add(id, "Страховой акт исключен #" + id);
				//	dataLogic.DeleteSyncQueue(qItem.ID);
				//	continue;
				//}

				var defaultCurrencyId = service.CurrencyId ?? 1;
				var contractCurrencies = contractLogic.GetContractCurrencies(contract.ID);
				var currency = contractCurrencies.FirstOrDefault(w => w.CurrencyId == defaultCurrencyId);
				if (currency == null)
					currency = contractCurrencies.FirstOrDefault(w => w.CurrencyId == 1);   // рубли присутствуют обязательно в этом случае

				var bankAccount = bankLogic.GetBankAccount(currency.BankAccountId);
				if (bankAccount == null)
				{
					qItem.Error = "Не задан банк для доход/расхода #" + id;
					errors.Add(id, "Не задан банк для доход/расхода");
					dataLogic.UpdateSyncQueue(qItem);
					continue;
				}

				var bank = bankLogic.GetBank(bankAccount.BankId ?? 0);
				var currencies = dataLogic.GetCurrencies();
				var serviceTypes = dataLogic.GetServiceTypes();
				var serviceKinds = dataLogic.GetServiceKinds();
				var vats = dataLogic.GetVats();

				var serviceB = new List<IdTuple>();
				foreach (var svc in services)
				{
					if (svc.ServiceTypeId.HasValue)
					{
						int serviceKindId = serviceTypes.First(w => w.ID == svc.ServiceTypeId).ServiceKindId;
						var serviceKind = serviceKinds.First(w => w.ID == serviceKindId);
						// вычисление НДС
						double tax = 0;
						VAT vat = null;
						if (svc.VatId.HasValue)
						{
							vat = vats.First(w => w.ID == svc.VatId.Value);
							if (currency.CurrencyId == 1)   // рубли
								tax = svc.Sum.Value - svc.Sum.Value / (100 + vat.Percent) * 100;
							else
								tax = svc.OriginalSum.Value - svc.OriginalSum.Value / (100 + vat.Percent) * 100;
						}

						var sbase = serviceB.FirstOrDefault(w => w.Id == serviceKindId);
						if (sbase == null)
						{
							if (currency.CurrencyId == 1)   // рубли
								sbase = new IdTuple { Id = serviceKindId, Name = serviceKind.Name, Value = svc.Sum.Value, Value2 = tax, TaxRate = "" };
							else
								sbase = new IdTuple { Id = serviceKindId, Name = serviceKind.Name, Value = svc.OriginalSum.Value, Value2 = tax, TaxRate = "" };

							serviceB.Add(sbase);
						}
						else
						{
							if (currency.CurrencyId == 1)   // рубли
								sbase.Value += svc.Sum.Value;
							else
								sbase.Value += svc.OriginalSum.Value;

							sbase.Value2 += tax;
						}

						if ((vat != null) && (vat.ID != 2)) // без НДС
							sbase.TaxRate = vat.Percent.ToString();
					}
				}


				// ещё пара проверок
				if (!accounting.ActDate.HasValue)
				{
					qItem.Error = "Не задана дата акта доход/расхода #" + id;
					errors.Add(id, "Не задана дата акта для доход/расхода");
					dataLogic.UpdateSyncQueue(qItem);
					continue;
				}

				try
				{
					var response = client.SetDeliverValue(new Novelco.ConsignmentNote
					{
						TIN = legal.TIN,
						BIK = (bank != null) ? bank.BIC : "",
						RGC = legal.KPP ?? "",
						Swift = bankAccount.CoBankSWIFT,
						Currensy = currencies.First(w => w.ID == currency.CurrencyId).Code,
						BankAccount = bankAccount.Number,
						PartnerName = legal.DisplayName,
						CreateInvoice = accounting.IsIncome ? true : !string.IsNullOrEmpty(serviceB.First().TaxRate),
						DeliverValueDate = accounting.ActDate.Value,
						IncomingDocumentNumber = accounting.ActNumber,
						DeliverValueNumber = accounting.Number,
						ContractDate = accounting.InvoiceDate,
						ContractNumber = accounting.InvoiceNumber,
						ThisSelling = accounting.IsIncome,
						InvoiceNumber = accounting.VatInvoiceNumber ?? accounting.InvoiceNumber,
						ServicesList = serviceB.Select(s => new Novelco.ServicesList
						{
							Price = (float)s.Value,
							ServiceName = s.Name,
							VAT = s.TaxRate
						}).ToArray()
					});

					if (response == null)
					{
						qItem.Error = "Не удалось выгрузить данные доход/расхода #" + id + ", пустой ответ веб-сервиса.";
						errors.Add(id, "Не удалось выгрузить данные доход/расхода, пустой ответ веб-сервиса.");
						dataLogic.UpdateSyncQueue(qItem);
					}
					else
					{
						if (response.Succesfully)
						{
							success.Add(id, "Успешно выгружены данные доход/расхода, 1C-id=" + response.ResultString);
							dataLogic.DeleteSyncQueue(qItem.ID);

							#region LOG
							//string filename = @"\\cm.corp.local.corp.local\cm\Temp\" + DateTime.UtcNow.ToString("yyyy-MM-dd_HHmmss") + ".to_1c.log";
							string filename = System.Web.Hosting.HostingEnvironment.MapPath("~/Temp/" + DateTime.UtcNow.ToString("yyyy-MM-dd_HHmmss") + ".to_1c.log");

							if (System.Web.HttpContext.Current != null)
								filename = System.Web.HttpContext.Current.Server.MapPath("~/Temp/" + DateTime.UtcNow.ToString("yyyy-MM-dd_HHmmss") + ".to_1c.log");

							using (var writer = new System.IO.StreamWriter(filename, true))
							{
								writer.WriteLine("В 1С переданы следующие данные:");
								writer.WriteLine("TIN - " + legal.TIN);
								writer.WriteLine("BIK - " + ((bank != null) ? bank.BIC : ""));
								writer.WriteLine("RGC - " + legal.KPP ?? "");
								writer.WriteLine("Swift - " + bankAccount.CoBankSWIFT);
								writer.WriteLine("Currensy - " + currencies.First(w => w.ID == currency.CurrencyId).Code);
								writer.WriteLine("BankAccount - " + bankAccount.Number);
								writer.WriteLine("PartnerName - " + legal.DisplayName);
								writer.WriteLine("CreateInvoice - " + (accounting.IsIncome ? true : !string.IsNullOrEmpty(serviceB.First().TaxRate)));
								writer.WriteLine("DeliverValueDate - " + accounting.ActDate.Value);
								writer.WriteLine("IncomingDocumentNumber - " + accounting.ActNumber);
								writer.WriteLine("DeliverValueNumber - " + accounting.Number);
								writer.WriteLine("ContractDate - " + accounting.InvoiceDate);
								writer.WriteLine("ContractNumber - " + accounting.InvoiceNumber);
								writer.WriteLine("ThisSelling - " + accounting.IsIncome);
								writer.WriteLine("InvoiceNumber - " + (accounting.VatInvoiceNumber ?? accounting.InvoiceNumber));
								writer.WriteLine("Услуги - ");
								foreach (var svc in serviceB)
									writer.WriteLine(svc.Name + " - " + svc.Value + " - " + svc.TaxRate);

								writer.Close();
							}

							#endregion
						}
						else
						{
							qItem.Error = "Не удалось выгрузить данные доход/расхода #" + id + ", " + response.ResultString;
							errors.Add(id, "Не удалось выгрузить данные доход/расхода, " + response.ResultString);
							dataLogic.UpdateSyncQueue(qItem);
						}
					}
				}
				catch (Exception ex)
				{
					try
					{
						errors.Add(id, "Ошибка при выгрузке данных доход/расхода: " + ex.Message);
					}
					catch
					{ }
				}
			}

			#endregion

			string message = "Синхронизация выполнена с " + errors.Count + " ошибок. Очередь можно посмотреть на странице http://cm.corp.local/Sync/Queue, подробная информация об ошибках 1С в журнале 1С. <br/><br/>";

			message += "Предыдущая синхронизация производилась " + lastSyncDate + "<br/>";
			message += "<h4>Выгрузка актов</h4><br/>";

			foreach (var item in success)
				message += string.Format("<a href='http://cm.corp.local/Orders/Details/{1}?selectedAccountingId={0}'>{2}</a>: {3}<br />", item.Key, orderLogic.GetOrderByAccounting(item.Key).ID, accountingLogic.GetAccounting(item.Key).Number, item.Value);

			foreach (var item in errors)
				message += string.Format("<span style='color:#900'>{0}: {1}</span><br />", accountingLogic.GetAccounting(item.Key).Number, item.Value);

			#region download

			try
			{
				message += "<h4>Загрузка платежей</h4><br/>";
				client.InnerChannel.OperationTimeout = new TimeSpan(0, 15, 0);

				var payments = client.GetPaymentDocumentsList(DateTime.Today.AddYears(-1));
				foreach (var item in payments)
				{
					var number = item.BaseDocumentNumber;
					if ((!string.IsNullOrWhiteSpace(number)) && (number.Length <= 12) && (number.Length > 3) && (!number.Contains("-")))
						number = number.Substring(0, number.Length - 3) + "-" + number.Substring(number.Length - 3, 3);

					var accounting = accountingLogic.GetAccountingByNumber(number);
					var payment = accountingLogic.GetPaymentByFinReference(item.DocumentID);
					if (payment != null)
					{
						payment.Number = item.IncomingDocumentNumber;
						//payment.Sum = item.Amount;
						payment.Sum = double.Parse(item.Amount, System.Globalization.CultureInfo.InvariantCulture);
						payment.Date = item.PaymentDate;
						payment.Description = item.Description;
						payment.AccountingId = (accounting != null) ? accounting.ID : (int?)null;
						payment.CurrencyId = string.IsNullOrEmpty(item.Currency) ? 1 : GetCurrencyIdByCode(item.Currency);
						payment.IsIncome = item.DocumentType == "0";
						payment.BankAccount = item.BankAccount;
						payment.BaseNumber = item.BaseDocumentNumber;
						payment.BIC_Swift = item.BIK_Swift;
						payment.IsMarkingRemoval = item.MarkingRemoval;
						payment.KPP = item.RGC;
						payment.TIN = item.TIN;
						accountingLogic.UpdatePayment(payment);
					}
					else
					{
						payment = new Payment();
						payment.Number = item.IncomingDocumentNumber;
						//payment.Sum = item.Amount;
						payment.Sum = double.Parse(item.Amount, System.Globalization.CultureInfo.InvariantCulture);
						payment.Date = item.PaymentDate;
						payment.Description = item.Description;
						payment.FinReference = item.DocumentID;
						payment.AccountingId = (accounting != null) ? accounting.ID : (int?)null;
						payment.CurrencyId = string.IsNullOrEmpty(item.Currency) ? 1 : GetCurrencyIdByCode(item.Currency);
						payment.IsIncome = item.DocumentType == "0";
						payment.BankAccount = item.BankAccount;
						payment.BaseNumber = item.BaseDocumentNumber;
						payment.BIC_Swift = item.BIK_Swift;
						payment.IsMarkingRemoval = item.MarkingRemoval;
						payment.KPP = item.RGC;
						payment.TIN = item.TIN;
						payment.ID = accountingLogic.CreatePayment(payment);
					}

					message += string.Format("<a href='http://cm.corp.local/Orders/ViewPayment/{0}'>{1}</a>: {2}<br />", payment.ID, item.BaseDocumentNumber, item.Description);
				}

				client.UnregisterChanges();
			}
			catch (Exception ex)
			{
				message += "<p style='color:#F00'>При загрузке платежей возникла ошибка: " + ex + "</p><br/>";
			}

			#endregion

			// set last sync date
			setting.Value = DateTime.Now.AddHours(-1).ToString("yyyy-MM-ddTHH:mm:ssZ");
			dataLogic.UpdateSystemSetting(setting);

			SendMail("grigoriev@novelco.ru", "CarMan-1C синхронизация" + ((errors.Count > 0) ? " (" + errors.Count + " ошибок)" : ""), message, "dabizha@novelco.ru");

			FixPaymentsLinks();
			FixPaymentsLinks2();
			FixReturnPayments();

			// recalculate all contractors
			var contractors = contractorLogic.GetContractors(new ListFilter());
			foreach (var item in contractors)
				accountingLogic.CalculateContractorBalance(item.ID);

			return message;
		}

		int? GetCurrencyIdByCode(string code)
		{
			return dataLogic.GetCurrencies().FirstOrDefault(w => w.Code == code)?.ID;
		}

		internal void FixPaymentsLinks()
		{
			var payments = accountingLogic.GetAllPayments().Where(w => w.AccountingId == null);

			foreach (var payment in payments)
			{
				#region by BaseNumber

				if (!string.IsNullOrWhiteSpace(payment.BaseNumber))
				{
					var number = payment.BaseNumber;
					if ((!string.IsNullOrWhiteSpace(number)) && (number.Length <= 12) && (number.Length > 3) && (!number.Contains("-")))
						number = number.Substring(0, number.Length - 3) + "-" + number.Substring(number.Length - 3);

					var accounting = accountingLogic.GetAccountingByNumber(number);
					if (accounting != null)
					{
						payment.AccountingId = accounting.ID;
						accountingLogic.UpdatePayment(payment);
					}
				}

				#endregion
				/*	#region weak links			

					if (!payment.AccountingId.HasValue && !payment.Description.StartsWith("{VO"))
					{
						if (!payment.AccountingId.HasValue && string.IsNullOrWhiteSpace(number))
						{
							var regex = new Regex(@".*(\d\d-\d\d\d\d-\d\d\d).*");
							var match = regex.Match(payment.Description);
							if (match.Success)
								number = match.Groups[1].Value;
						}

						if (string.IsNullOrWhiteSpace(number))
						{
							var regex = new Regex(@".*(\d\d\d\d\d\d\d\d-\d\d\d).*");
							var match = regex.Match(payment.Description);
							if (match.Success)
								number = match.Groups[1].Value;
						}

						if (!payment.AccountingId.HasValue && !string.IsNullOrWhiteSpace(number))
						{
							number = number.Replace("--", "-");
							if (!number.Contains("-"))
								number = number.Substring(0, number.Length - 3) + "-" + number.Substring(number.Length - 3, 3);

							var accounting = accountingLogic.GetAccountingByNumber(number);
							if (accounting != null)
							{
								payment.AccountingId = accounting.ID;
								accountingLogic.UpdatePayment(payment);
							}
						}
					}
					#endregion */
			}
		}

		internal void FixPaymentsLinks2()
		{
			// наши платежи
			var payments = accountingLogic.GetAllPayments().Where(w => (w.AccountingId == null) && !w.IsIncome);

			foreach (var payment in payments)
			{
				// проверить по BaseNumber
				var accountings = accountingLogic.GetAccountingsByInvoiceNumber(payment.BaseNumber);
				foreach (var accounting in accountings)
				{
					if (!accounting.ContractId.HasValue)
						continue;

					// найти расход на юрлицо, которое осуществляло платеж
					var contract = contractLogic.GetContract(accounting.ContractId.Value);
					var legal = legalLogic.GetLegal(contract.LegalId);
					// TODO:
					//if ((legal.TIN == payment.TIN) || (string.IsNullOrWhiteSpace(legal.TIN) && string.IsNullOrWhiteSpace(payment.TIN)))
					if (!string.IsNullOrWhiteSpace(legal.TIN) && (legal.TIN == payment.TIN))
					{
						payment.AccountingId = accounting.ID;
						accountingLogic.UpdatePayment(payment);
					}
				}

				// если не удалось найти по BaseNumber, попробовать найти в тексте по "... по договору Счет № 3423 от 21.10..."
				if (!payment.AccountingId.HasValue)
				{
					var regex = new Regex(@" Счет № (\S*) от ");
					var match = regex.Match(payment.Description);
					if (match.Success)
					{
						accountings = accountingLogic.GetAccountingsByInvoiceNumber(match.Groups[1].Value);
						foreach (var accounting in accountings)
						{
							if (!accounting.ContractId.HasValue)
								continue;

							// найти расход на юрлицо, которое осуществляло платеж
							var contract = contractLogic.GetContract(accounting.ContractId.Value);
							var legal = legalLogic.GetLegal(contract.LegalId);
							// TODO:
							//if ((legal.TIN == payment.TIN) || (string.IsNullOrWhiteSpace(legal.TIN) && string.IsNullOrWhiteSpace(payment.TIN)))
							if (!string.IsNullOrWhiteSpace(legal.TIN) && (legal.TIN == payment.TIN))
							{
								payment.AccountingId = accounting.ID;
								accountingLogic.UpdatePayment(payment);
							}
						}
					}
				}

				// если не удалось найти, попробовать найти в тексте по "... по договору № 3423 от ..."
				if (!payment.AccountingId.HasValue)
				{
					var regex = new Regex(@" по договору № (\S*) от ");
					var match = regex.Match(payment.Description);
					if (match.Success)
					{
						accountings = accountingLogic.GetAccountingsByInvoiceNumber(match.Groups[1].Value);
						foreach (var accounting in accountings)
						{
							if (!accounting.ContractId.HasValue)
								continue;

							// найти расход на юрлицо, которое осуществляло платеж
							var contract = contractLogic.GetContract(accounting.ContractId.Value);
							var legal = legalLogic.GetLegal(contract.LegalId);
							// TODO:
							//if ((legal.TIN == payment.TIN) || (string.IsNullOrWhiteSpace(legal.TIN) && string.IsNullOrWhiteSpace(payment.TIN)))
							if (!string.IsNullOrWhiteSpace(legal.TIN) && (legal.TIN == payment.TIN))
							{
								payment.AccountingId = accounting.ID;
								accountingLogic.UpdatePayment(payment);
							}
						}
					}
				}

				// если не удалось найти, попробовать найти в тексте по "... по договору 3423-1 от ..."
				if (!payment.AccountingId.HasValue)
				{
					var regex = new Regex(@" по договору (\S*) от ");
					var match = regex.Match(payment.Description);
					if (match.Success)
					{
						accountings = accountingLogic.GetAccountingsByInvoiceNumber(match.Groups[1].Value);
						foreach (var accounting in accountings)
						{
							if (!accounting.ContractId.HasValue)
								continue;

							// найти расход на юрлицо, которое осуществляло платеж
							var contract = contractLogic.GetContract(accounting.ContractId.Value);
							var legal = legalLogic.GetLegal(contract.LegalId);
							// TODO:
							//if ((legal.TIN == payment.TIN) || (string.IsNullOrWhiteSpace(legal.TIN) && string.IsNullOrWhiteSpace(payment.TIN)))
							if (!string.IsNullOrWhiteSpace(legal.TIN) && (legal.TIN == payment.TIN))
							{
								payment.AccountingId = accounting.ID;
								accountingLogic.UpdatePayment(payment);
							}
						}
					}
				}

				// если не удалось найти, попробовать найти в тексте по "... по сч. № 3423-1 от ..."
				if (!payment.AccountingId.HasValue)
				{
					var regex = new Regex(@" по сч\.\s?№?\s?(\S*) от ");
					var match = regex.Match(payment.Description);
					if (match.Success)
					{
						accountings = accountingLogic.GetAccountingsByInvoiceNumber(match.Groups[1].Value);
						foreach (var accounting in accountings)
						{
							// найти расход на юрлицо, которое осуществляло платеж
							if (!accounting.ContractId.HasValue)
								continue;

							var contract = contractLogic.GetContract(accounting.ContractId.Value);
							var legal = legalLogic.GetLegal(contract.LegalId);
							// TODO:
							//if ((legal.TIN == payment.TIN) || (string.IsNullOrWhiteSpace(legal.TIN) && string.IsNullOrWhiteSpace(payment.TIN)))
							if (!string.IsNullOrWhiteSpace(legal.TIN) && (legal.TIN == payment.TIN))
							{
								payment.AccountingId = accounting.ID;
								accountingLogic.UpdatePayment(payment);
							}
						}
					}
				}
			}
		}

		internal void FixReturnPayments()
		{
			// Коррекция знака для платежей возврата ошибочно перечисленных нам средств
			foreach (var accounting in accountingLogic.GetAllAccountings().Where(w => w.IsIncome))
			{
				var payments = accountingLogic.GetPayments(accounting.ID);
				if (payments.Count() > 1)
					if (payments.Any(w => w.IsIncome))
						foreach (var payment in payments.Where(w => !w.IsIncome).ToList())
						{
							payment.Sum = -Math.Abs(payment.Sum);
							accountingLogic.UpdatePayment(payment);
						}
			}
		}
	}
}






