using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Logisto.BusinessLogic;
using Logisto.Models;
using Logisto.ViewModels;
using Newtonsoft.Json;
using OfficeOpenXml;

namespace Logisto.Controllers
{
	[Authorize]
	public class ContractorsController : BaseController
	{
		#region Pages

		public ActionResult Index()
		{
			var pageSize = int.Parse(ConfigurationManager.AppSettings["App_PageSize"]);
			var dataLogic = new DataLogic();
			var contractorLogic = new ContractorLogic();

			var filter = new ListFilter { PageSize = pageSize };
			if (CurrentUserId > 0)
			{
				filter.Sort = userLogic.GetSetting(CurrentUserId, "Contractors.Sort");
				filter.SortDirection = userLogic.GetSetting(CurrentUserId, "Contractors.SortDirection");
				filter.PageNumber = Convert.ToInt32(userLogic.GetSetting(CurrentUserId, "Contractors.PageNumber"));
			}

			var totalCount = contractorLogic.GetContractorsCount(filter);
			var viewModel = new ContractorsViewModel { Filter = filter, Items = contractorLogic.GetContractors(filter).ToList(), TotalItemsCount = totalCount };

			viewModel.Dictionaries.Add("User", dataLogic.GetUsers());

			if (new Random(Environment.TickCount).Next(8) == 4)
				Task.Run((System.Action)ProcessJob);

			return View(viewModel);
		}

		public ActionResult Details(int id, string activePart)
		{
			// предварительно пересчитать (актуализировать) баланс
			accountingLogic.CalculateContractorBalance(id);

			var viewModel = new ContractorViewModel { Contractor = contractorLogic.GetContractor(id) };

#if !DEBUG
			if ((!string.IsNullOrEmpty(viewModel.Contractor.Name)) && (viewModel.Contractor.Name.StartsWith("#")) && (!participantLogic.GetWorkgroupByContractor(id).Any(w => w.UserId == CurrentUserId)))
				return RedirectToAction("NotAuthorized", "Home");
#endif

			var contractors = contractorLogic.GetContractors(new ListFilter());

			viewModel.Dictionaries.Add("Vat", dataLogic.GetVats());
			viewModel.Dictionaries.Add("Role", dataLogic.GetRoles());
			viewModel.Dictionaries.Add("User", dataLogic.GetUsers());
			viewModel.Dictionaries.Add("Legal", dataLogic.GetLegals());
			viewModel.Dictionaries.Add("Person", dataLogic.GetPersons());
			viewModel.Dictionaries.Add("Legals", legalLogic.GetLegals(new ListFilter()));
			viewModel.Dictionaries.Add("Product", dataLogic.GetProducts());
			viewModel.Dictionaries.Add("TaxType", dataLogic.GetTaxTypes());
			viewModel.Dictionaries.Add("Service", dataLogic.GetServiceTypes());
			viewModel.Dictionaries.Add("Contract", dataLogic.GetContracts());
			viewModel.Dictionaries.Add("OurLegal", dataLogic.GetOurLegals());
			viewModel.Dictionaries.Add("Employee", dataLogic.GetEmployees());
			viewModel.Dictionaries.Add("Currency", dataLogic.GetCurrencies());
			viewModel.Dictionaries.Add("TimeZone", TimeZoneInfo.GetSystemTimeZones().Where(w => !w.Id.StartsWith("UTC")).Select(s => new { ID = s.Id, Display = s.DisplayName }));
			viewModel.Dictionaries.Add("PayMethod", dataLogic.GetPayMethods());
			viewModel.Dictionaries.Add("ActiveUser", dataLogic.GetActiveUsers());
			viewModel.Dictionaries.Add("PaymentTerm", dataLogic.GetPaymentTerms());
			viewModel.Dictionaries.Add("OrderStatus", dataLogic.GetOrderStatuses());
			viewModel.Dictionaries.Add("FinRepCenter", dataLogic.GetFinRepCenters());
			viewModel.Dictionaries.Add("ContractType", dataLogic.GetContractTypes());
			viewModel.Dictionaries.Add("ContractRole", dataLogic.GetContractRoles());
			viewModel.Dictionaries.Add("LegalByContract", dataLogic.GetLegalsByContract());
			viewModel.Dictionaries.Add("ParticipantRole", dataLogic.GetParticipantRoles());
			viewModel.Dictionaries.Add("CurrencyRateUse", dataLogic.GetCurrencyRateUses());
			viewModel.Dictionaries.Add("PositionTemplate", dataLogic.GetPositionTemplates());
			viewModel.Dictionaries.Add("ContractServiceType", dataLogic.GetContractServiceTypes());
			viewModel.Dictionaries.Add("AccountingDocumentType", dataLogic.GetAccountingDocumentTypes());
			viewModel.Dictionaries.Add("ContractorByLegal", legalLogic.GetLegals(new ListFilter()).Select(s => new DynamicDictionary
			{
				ID = s.ID,
				Display = contractors.Where(w => w.ID == s.ContractorId).Select(ss => ss.Name).FirstOrDefault()
			}));

			if (!string.IsNullOrWhiteSpace(activePart))
				ViewBag.ActivePart = activePart;

			return View(viewModel);
		}

		public ActionResult Create()
		{
			var contractor = new Contractor
			{
				CreatedBy = CurrentUserId,
				CreatedDate = DateTime.Now
			};

			var id = contractorLogic.CreateContractor(contractor);

			// инициализация участников
			participantLogic.CreateParticipant(new Participant { ContractorId = id, UserId = 2, ParticipantRoleId = 6 });   // GM ГРИГОРЬЕВ Г.И.
			participantLogic.CreateParticipant(new Participant { ContractorId = id, UserId = 33, ParticipantRoleId = 4 });   // BUH 
			participantLogic.CreateParticipant(new Participant { ContractorId = id, UserId = CurrentUserId, ParticipantRoleId = (int)ParticipantRoles.SM });

			return RedirectToAction("Details", new { id = id });
		}

		public ActionResult CreatePerson()
		{
			var person = new Person { Comment = "" };

			var model = new PersonViewModel
			{
				ID = person.ID,
				Email = person.Email,
				Name = person.Name,
				Address = person.Address,
				Comment = person.Comment,
				DisplayName = person.DisplayName,
				EnName = person.EnName,
				Family = person.Family,
				GenitiveFamily = person.GenitiveFamily,
				GenitiveName = person.GenitiveName,
				GenitivePatronymic = person.GenitivePatronymic,
				Initials = person.Initials,
				IsNotResident = person.IsNotResident,
				IsSubscribed = person.IsSubscribed,
				Patronymic = person.Patronymic
			};

			model.Dictionaries.Add("PhoneType", dataLogic.GetPhoneTypes());

			return View(model);
		}

		public ActionResult EditPerson(int personId)
		{
			var person = personLogic.GetPerson(personId);

			var model = new PersonViewModel
			{
				ID = person.ID,
				Email = person.Email,
				Name = person.Name,
				Address = person.Address,
				Comment = person.Comment,
				DisplayName = person.DisplayName,
				EnName = person.EnName,
				Family = person.Family,
				GenitiveFamily = person.GenitiveFamily,
				GenitiveName = person.GenitiveName,
				GenitivePatronymic = person.GenitivePatronymic,
				Initials = person.Initials,
				IsNotResident = person.IsNotResident,
				IsSubscribed = person.IsSubscribed,
				Patronymic = person.Patronymic
			};

			model.Dictionaries.Add("PhoneType", dataLogic.GetPhoneTypes());

			return View(model);
		}

		public ActionResult ViewLegal(int id)
		{
			var model = legalLogic.GetLegalInfo(id);
			return View(model);
		}

		#endregion

		#region get lists

		public ContentResult GetItems(ListFilter filter)
		{
			userLogic.SetSetting(CurrentUserId, "Contractors.Sort", filter.Sort);
			userLogic.SetSetting(CurrentUserId, "Contractors.SortDirection", filter.SortDirection);
			userLogic.SetSetting(CurrentUserId, "Contractors.PageNumber", filter.PageNumber.ToString());

			var totalCount = contractorLogic.GetContractorsCount(filter);
			var list = contractorLogic.GetContractors(filter);
			return Content(JsonConvert.SerializeObject(new { Items = list, TotalCount = totalCount }));
		}

		public ContentResult GetEmployeesByLegal(int legalId)
		{
			var persons = dataLogic.GetPersons();
			var list = employeeLogic.GetEmployeesByLegal(legalId);
			var resultList = list.Select(item => new EmployeeViewModel
			{
				ID = item.ID,
				LegalId = item.LegalId,
				Department = item.Department,
				Position = item.Position,
				Basis = item.Basis,
				BeginDate = item.BeginDate,
				Comment = item.Comment,
				EnBasis = item.EnBasis,
				EndDate = item.EndDate,
				EnPosition = item.EnPosition,
				GenitivePosition = item.GenitivePosition,
				PersonId = item.PersonId,
				Name = persons.Where(w => w.ID == item.PersonId).Select(s => s.Display).FirstOrDefault(),
			}).ToList();

			foreach (var emp in resultList)
			{
				var person = personLogic.GetPerson(emp.PersonId.Value);
				emp.Email = person.Email;
				emp.PhoneMobile = personLogic.GetPhonesByPerson(emp.PersonId.Value).Where(w => w.TypeId == 2).Select(s => s.Number).FirstOrDefault();
				emp.PhoneWork = personLogic.GetPhonesByPerson(emp.PersonId.Value).Where(w => w.TypeId == 1).Select(s => s.Number).FirstOrDefault();
			}

			return Content(JsonConvert.SerializeObject(new { Items = resultList }));
		}

		public ContentResult GetBankAccountsByLegal(int legalId)
		{
			var banks = bankLogic.GetBanks(new ListFilter());
			var list = bankLogic.GetBankAccountsByLegal(legalId);
			var resultList = list.Select(s => new BankAccoundViewModel
			{
				ID = s.ID,
				BankId = s.BankId,
				CoBankName = s.CoBankName,
				CoBankAccount = s.CoBankAccount,
				CoBankSWIFT = s.CoBankSWIFT,
				CoBankIBAN = s.CoBankIBAN,
				CoBankAddress = s.CoBankAddress,
				CurrencyId = s.CurrencyId,
				LegalId = s.LegalId,
				Number = s.Number,
				BankName = banks.Where(w => w.ID == s.BankId).Select(o => o.Name).FirstOrDefault(),
				BIC = banks.Where(w => w.ID == s.BankId).Select(o => o.BIC).FirstOrDefault()
			});

			return Content(JsonConvert.SerializeObject(new { Items = resultList }));
		}

		public ContentResult GetBankAccountsByOurLegal(int ourLegalId)
		{
			var ourLegal = legalLogic.GetOurLegal(ourLegalId);
			var banks = bankLogic.GetBanks(new ListFilter());
			var list = bankLogic.GetBankAccountsByLegal(ourLegal.LegalId.Value);
			var resultList = list.Select(s => new BankAccoundViewModel
			{
				ID = s.ID,
				BankId = s.BankId,
				CoBankName = s.CoBankName,
				CoBankAccount = s.CoBankAccount,
				CoBankSWIFT = s.CoBankSWIFT,
				CoBankIBAN = s.CoBankIBAN,
				CurrencyId = s.CurrencyId,
				LegalId = s.LegalId,
				Number = s.Number,
				BankName = banks.Where(w => w.ID == s.BankId).Select(o => o.Name).FirstOrDefault(),
				BIC = banks.Where(w => w.ID == s.BankId).Select(o => o.BIC).FirstOrDefault()
			});

			return Content(JsonConvert.SerializeObject(new { Items = resultList }));
		}

		public ContentResult GetLegalsByContractor(int contractorId)
		{
			var contracts = contractLogic.GetContracts(new ListFilter()).Select(s => new { ID = s.ID, LegalId = s.LegalId });
			var list = legalLogic.GetLegalsByContractor(contractorId).ToList();
			var result = list.Select(s => new LegalViewModel
			{
				ID = s.ID,
				AccountantId = s.AccountantId,
				Address = s.Address,
				AddressFact = s.AddressFact,
				PostAddress = s.PostAddress,
				Balance = s.Balance,
				ContractorId = s.ContractorId,
				CreatedBy = s.CreatedBy,
				CreatedDate = s.CreatedDate,
				DirectorId = s.DirectorId,
				DisplayName = s.DisplayName,
				EnAddress = s.EnAddress,
				EnAddressFact = s.EnAddressFact,
				EnPostAddress = s.EnPostAddress,
				EnName = s.EnName,
				EnShortName = s.EnShortName,
				Expense = s.Expense,
				Income = s.Income,
				IsNotResident = s.IsNotResident,
				KPP = s.KPP,
				Name = s.Name,
				OGRN = s.OGRN,
				OKPO = s.OKPO,
				OKVED = s.OKVED,
				PaymentBalance = s.PaymentBalance,
				PaymentExpense = s.PaymentExpense,
				PaymentIncome = s.PaymentIncome,
				TaxTypeId = s.TaxTypeId,
				TIN = s.TIN,
				UpdatedBy = s.UpdatedBy,
				UpdatedDate = s.UpdatedDate,
				WorkTime = s.WorkTime,
				TimeZone = s.TimeZone,
				ContractCount = contracts.Count(c => c.LegalId == s.ID),
			});

			return Content(JsonConvert.SerializeObject(new { Items = result }));
		}

		public ContentResult GetContractsByContractor(int contractorId)
		{
			var list = contractLogic.GetContractsByContractor(contractorId);
			var result = list.Select(s => new ContractViewModel
			{
				ID = s.ID,
				BankAccountId = s.BankAccountId,
				BeginDate = s.BeginDate,
				Comment = s.Comment,
				ContractRoleId = s.ContractRoleId,
				ContractServiceTypeId = s.ContractServiceTypeId,
				ContractTypeId = s.ContractTypeId,
				CurrencyRateUseId = s.CurrencyRateUseId,
				Date = s.Date,
				EndDate = s.EndDate,
				IsFixed = s.IsFixed,
				IsProlongation = s.IsProlongation,
				LegalId = s.LegalId,
				Number = s.Number,
				OurBankAccountId = s.OurBankAccountId,
				OurContractRoleId = s.OurContractRoleId,
				OurLegalId = s.OurLegalId,
				PaymentTermsId = s.PaymentTermsId,
				PayMethodId = s.PayMethodId,
				IsActive = s.EndDate.HasValue ? (s.EndDate < DateTime.Now ? (s.IsProlongation ? true : false) : true) : (true)
			}).ToList();

			foreach (var item in result)
			{
				item.Currencies = contractLogic.GetContractCurrencies(item.ID);
				var marks = contractLogic.GetContractMarkByContract(item.ID);
				if ((marks != null) && (marks.IsContractRejected || marks.IsContractBlocked))
					item.IsActive = false;

				if (marks != null)
					item.Marks = marks.IsContractBlocked ? "Блок " : (marks.IsContractRejected ? "Отклонен " : (marks.IsContractChecked ? "Проверен " : (marks.IsContractOk ? "Ok " : "")));
			}

			return Content(JsonConvert.SerializeObject(new { Items = result }));
		}

		public ContentResult GetContractsByLegal(int legalId)
		{
			var list = contractLogic.GetContractsByLegal(legalId).ToList();
			var result = list.Select(s => new ContractViewModel
			{
				ID = s.ID,
				BankAccountId = s.BankAccountId,
				BeginDate = s.BeginDate,
				Comment = s.Comment,
				ContractRoleId = s.ContractRoleId,
				ContractServiceTypeId = s.ContractServiceTypeId,
				ContractTypeId = s.ContractTypeId,
				CurrencyRateUseId = s.CurrencyRateUseId,
				Date = s.Date,
				EndDate = s.EndDate,
				IsFixed = s.IsFixed,
				IsProlongation = s.IsProlongation,
				LegalId = s.LegalId,
				Number = s.Number,
				OurBankAccountId = s.OurBankAccountId,
				OurContractRoleId = s.OurContractRoleId,
				OurLegalId = s.OurLegalId,
				PaymentTermsId = s.PaymentTermsId,
				PayMethodId = s.PayMethodId,
				IsActive = s.EndDate.HasValue ? (s.EndDate < DateTime.Now ? (s.IsProlongation ? true : false) : true) : (true)
			}).ToList();

			foreach (var item in result)
			{
				var marks = contractLogic.GetContractMarkByContract(item.ID);
				if ((marks != null) && (marks.IsContractRejected || marks.IsContractBlocked))
					item.IsActive = false;

				if (marks != null)
					item.Marks = marks.IsContractBlocked ? "Блок " : (marks.IsContractRejected ? "Отклонен " : (marks.IsContractChecked ? "Проверен " : (marks.IsContractOk ? "Ok " : "")));
			}

			return Content(JsonConvert.SerializeObject(new { Items = result }));
		}

		public ContentResult GetContractCurrencies(int contractId)
		{
			var currencies = contractLogic.GetContractCurrencies(contractId);
			return Content(JsonConvert.SerializeObject(new { Items = currencies }));
		}

		public ContentResult GetAccountingsByContractor(int contractorId)
		{
			var contracts = contractLogic.GetContracts(new ListFilter());
			var orders = orderLogic.GetAllOrders();
			var allMarks = accountingLogic.GetAllAccountingMarks();
			// в баланс Контрагента выводить только те Доходы и Расходы, по которым есть метка по крайней мере Счет ОК/Расход ОК
			var list = accountingLogic.GetAccountingsByContractor(contractorId).Where(w => allMarks.Any(a => a.AccountingId == w.ID) && (allMarks.First(a => a.AccountingId == w.ID).IsInvoiceOk || allMarks.First(a => a.AccountingId == w.ID).IsAccountingOk));
			var resultList = list.Select(s => new AccountingViewModel
			{
				ID = s.ID,
				AccountingDate = s.AccountingDate,
				IsIncome = s.IsIncome,
				AccountingDocumentTypeId = s.AccountingDocumentTypeId,
				AccountingPaymentMethodId = s.AccountingPaymentMethodId,
				AccountingPaymentTypeId = s.AccountingPaymentTypeId,
				PayMethodId = s.PayMethodId,
				ActDate = s.ActDate,
				ActNumber = s.ActNumber,
				Comment = s.Comment,
				ContractId = s.ContractId,
				CreatedDate = s.CreatedDate,
				InvoiceDate = s.InvoiceDate,
				InvoiceNumber = s.InvoiceNumber,
				Number = s.Number,
				OrderId = s.OrderId,
				OriginalSum = s.OriginalSum,
				OriginalVat = s.OriginalVat,
				CurrencyRate = s.CurrencyRate,
				Route = s.Route,
				SecondSignerEmployeeId = s.SecondSignerEmployeeId,
				SecondSignerInitials = s.SecondSignerInitials,
				SecondSignerName = s.SecondSignerName,
				SecondSignerPosition = s.SecondSignerPosition,
				Sum = s.Sum,
				Vat = s.Vat,
				CargoLegalId = s.CargoLegalId,
				PaymentPlanDate = s.PaymentPlanDate,
				OurLegalId = s.OurLegalId,
				LegalId = s.LegalId,
				ContractNumber = "",
				AccountingCurrencyId = accountingLogic.GetAccountingCurrencyId(s.ID),
				OrderNumber = orders.Where(w => w.ID == s.OrderId).Select(os => os.Number).FirstOrDefault()
			}).ToList();

			foreach (var item in resultList)
				if (item.IsIncome)
				{
					var contract = contracts.FirstOrDefault(w => w.ID == orders.First(wo => wo.ID == item.OrderId).ContractId);
					item.ContractNumber = contract?.Number;
					item.LegalId = item.LegalId.HasValue ? item.LegalId : contract.LegalId;
				}
				else
				{
					var contract = contracts.FirstOrDefault(w => w.ID == item.ContractId);
					item.ContractNumber = contract?.Number;
					item.OurLegalId = contract?.OurLegalId;
					item.LegalId = contract?.LegalId;
				}

			return Content(JsonConvert.SerializeObject(new { Items = resultList }));
		}

		public ContentResult GetAccountingsByLegal(int legalId)
		{
			var contracts = contractLogic.GetContracts(new ListFilter());
			var orders = orderLogic.GetAllOrders();
			var allMarks = accountingLogic.GetAllAccountingMarks();
			// в баланс Контрагента выводить только те Доходы и Расходы, по которым есть метка по крайней мере Счет ОК/Расход ОК
			var list = accountingLogic.GetAccountingsByLegal(legalId).Where(w => allMarks.Any(a => a.AccountingId == w.ID) && (allMarks.First(a => a.AccountingId == w.ID).IsInvoiceOk || allMarks.First(a => a.AccountingId == w.ID).IsAccountingOk));
			var resultList = list.Select(s => new AccountingViewModel
			{
				ID = s.ID,
				AccountingDate = s.AccountingDate,
				IsIncome = s.IsIncome,
				AccountingDocumentTypeId = s.AccountingDocumentTypeId,
				AccountingPaymentMethodId = s.AccountingPaymentMethodId,
				AccountingPaymentTypeId = s.AccountingPaymentTypeId,
				PayMethodId = s.PayMethodId,
				ActDate = s.ActDate,
				ActNumber = s.ActNumber,
				Comment = s.Comment,
				ContractId = s.ContractId,
				CreatedDate = s.CreatedDate,
				InvoiceDate = s.InvoiceDate,
				InvoiceNumber = s.InvoiceNumber,
				Number = s.Number,
				OrderId = s.OrderId,
				OriginalSum = s.OriginalSum,
				OriginalVat = s.OriginalVat,
				CurrencyRate = s.CurrencyRate,
				Route = s.Route,
				SecondSignerEmployeeId = s.SecondSignerEmployeeId,
				SecondSignerInitials = s.SecondSignerInitials,
				SecondSignerName = s.SecondSignerName,
				SecondSignerPosition = s.SecondSignerPosition,
				Sum = s.Sum,
				Vat = s.Vat,
				CargoLegalId = s.CargoLegalId,
				PaymentPlanDate = s.PaymentPlanDate,
				OurLegalId = s.IsIncome ? s.OurLegalId : contracts.Where(w => w.ID == orders.First(ow => ow.ID == s.OrderId).ContractId).Select(con => con.OurLegalId).FirstOrDefault(),
				LegalId = s.IsIncome ? s.LegalId : contracts.Where(w => w.ID == s.ContractId).Select(con => con.LegalId).FirstOrDefault(),
				AccountingCurrencyId = accountingLogic.GetServicesByAccounting(s.ID).Select(con => con.CurrencyId).FirstOrDefault(),
				ContractNumber = contracts.Where(w => w.ID == (s.IsIncome ? orders.First(wo => wo.ID == s.OrderId).ContractId : s.ContractId)).Select(con => con.Number).FirstOrDefault(),
				OrderNumber = orders.Where(w => w.ID == s.OrderId).Select(con => con.Number).FirstOrDefault()
			});

			return Content(JsonConvert.SerializeObject(new { Items = resultList }));
		}

		public ContentResult GetPaymentsByContractor(int contractorId)
		{
			var list = accountingLogic.GetPaymentsByContractor(contractorId);
			return Content(JsonConvert.SerializeObject(new { Items = list }));
		}

		public ContentResult GetPhones(int personId)
		{
			var list = personLogic.GetPhonesByPerson(personId);
			return Content(JsonConvert.SerializeObject(new { Items = list }));
		}

		#endregion

		#region new

		public ContentResult GetNewEmployee(int? legalId, int? contractorId)
		{
			Contractor contractor = null;
			if (contractorId.HasValue)
				contractor = contractorLogic.GetContractor(contractorId.Value);
			else if (legalId.HasValue)
			{
				var legal = legalLogic.GetLegal(legalId.Value);
				if (legal.ContractorId.HasValue)
					contractor = contractorLogic.GetContractor(legal.ContractorId.Value);
			}

			if ((contractor != null) && contractor.IsLocked)
				return Content(JsonConvert.SerializeObject(new { ActionId = 5, Message = "Контрагент зафиксирован, менять его данные нельзя." }));

			// проверка прав участника
			var _contractorId = contractorId ?? legalLogic.GetLegal(legalId.Value).ContractorId;
			if (_contractorId.HasValue && !participantLogic.IsAllowedActionByContractor(5, _contractorId.Value, CurrentUserId))
				return Content(JsonConvert.SerializeObject(new { ActionId = 5, Message = "У вас недостаточно прав на выполнение этого действия." }));

			// подстановка значений по-умолчанию
			var entity = new Employee { LegalId = legalId, ContractorId = contractorId };
			var id = employeeLogic.CreateEmployee(entity);
			entity = employeeLogic.GetEmployee(id);

			var viewModel = new EmployeeViewModel
			{
				ID = entity.ID,
				LegalId = entity.LegalId,
				ContractorId = entity.ContractorId,
				Department = entity.Department,
				Position = entity.Position
			};

			return Content(JsonConvert.SerializeObject(viewModel));
		}

		public ContentResult GetNewBankAccount(int legalId)
		{
			var legal = legalLogic.GetLegal(legalId);
			if (legal.ContractorId.HasValue)    // возможно с нашими юрлицами
			{
				if (contractorLogic.GetContractor(legal.ContractorId.Value).IsLocked)
					return Content(JsonConvert.SerializeObject(new { ActionId = 6, Message = "Контрагент зафиксирован, менять его данные нельзя." }));

				// проверка прав участника
				if (!participantLogic.IsAllowedActionByContractor(6, legal.ContractorId.Value, CurrentUserId))
					return Content(JsonConvert.SerializeObject(new { ActionId = 6, Message = "У вас недостаточно прав на выполнение этого действия." }));
			}

			// подстановка значений по-умолчанию
			var entity = new BankAccount { LegalId = legalId, CurrencyId = 1 };
			var id = bankLogic.CreateBankAccount(entity);
			entity = bankLogic.GetBankAccount(id);

			var bankAccount = new BankAccoundViewModel
			{
				ID = entity.ID,
				BankId = entity.BankId,
				CoBankAccount = entity.CoBankAccount,
				CoBankSWIFT = entity.CoBankSWIFT,
				CoBankIBAN = entity.CoBankIBAN,
				CoBankAddress = entity.CoBankAddress,
				CurrencyId = entity.CurrencyId,
				LegalId = entity.LegalId,
				Number = entity.Number
			};

			return Content(JsonConvert.SerializeObject(bankAccount));
		}

		public ContentResult GetNewLegal(int contractorId)
		{
			if (contractorLogic.GetContractor(contractorId).IsLocked)
				return Content(JsonConvert.SerializeObject(new { ActionId = 3, Message = "Контрагент зафиксирован, менять его данные нельзя." }));

			if (!participantLogic.IsAllowedActionByContractor(3, contractorId, CurrentUserId))
				return Content(JsonConvert.SerializeObject(new { ActionId = 3, Message = "У вас недостаточно прав на выполнение этого действия." }));

			// подстановка значений по-умолчанию
			var l = new Legal
			{
				ContractorId = contractorId,
				CreatedBy = CurrentUserId,
				CreatedDate = DateTime.Now,
				WorkTime = "09:00-18:00"
			};

			var id = legalLogic.CreateLegal(l);
			l = legalLogic.GetLegal(id);
			return Content(JsonConvert.SerializeObject(l));
		}

		public ContentResult GetNewContract(int legalId)
		{
			if (contractorLogic.GetContractor(legalLogic.GetLegal(legalId).ContractorId.Value).IsLocked)
				return Content(JsonConvert.SerializeObject(new { Message = "Контрагент зафиксирован, менять его данные нельзя." }));

			// подстановка значений по-умолчанию
			var c = new Contract { LegalId = legalId, CurrencyRateUseId = 2 };

			return Content(JsonConvert.SerializeObject(c));
		}

		public ContentResult GetNewPhone()
		{
			// подстановка значений по-умолчанию
			var phone = new Phone { };
			return Content(JsonConvert.SerializeObject(phone));
		}

		public ContentResult GetNewParticipant(int contractorId)
		{
			// проверка прав участника
			if (!participantLogic.IsAllowedActionByContractor(8, contractorId, CurrentUserId))
				return Content(JsonConvert.SerializeObject(new { ActionId = 8, Message = "У вас недостаточно прав на выполнение этого действия." }));

			// подстановка значений по-умолчанию
			var c = new Participant { ContractorId = contractorId };
			return Content(JsonConvert.SerializeObject(c));
		}

		#endregion

		public FileResult GetAccountingsFile(int legalId)
		{
			var template = Server.MapPath("~/App_Data/AccountingTemplate.xlsx");
			var filename = Server.MapPath("~\\Temp\\Accountings" + Environment.TickCount + ".xlsx");
			var allMarks = accountingLogic.GetAllAccountingMarks();
			// в баланс Контрагента выводить только те Доходы и Расходы, по которым есть метка по крайней мере Счет ОК/Расход ОК
			var list = accountingLogic.GetAccountingsByLegal(legalId).Where(w => allMarks.Any(a => a.AccountingId == w.ID) && (allMarks.First(a => a.AccountingId == w.ID).IsInvoiceOk || allMarks.First(a => a.AccountingId == w.ID).IsAccountingOk));
			var types = dataLogic.GetAccountingDocumentTypes();
			var contracts = contractLogic.GetContracts(new ListFilter());
			var orders = orderLogic.GetAllOrders();
			var currencies = dataLogic.GetCurrencies();

			var fi = new System.IO.FileInfo(template);

			using (var xl = new ExcelPackage(fi))
			{
				xl.DoAdjustDrawings = true;
				xl.Workbook.CalcMode = ExcelCalcMode.Automatic;

				var sheet = xl.Workbook.Worksheets[1];

				var row = 2;
				foreach (var accounting in list)
				{
					var col = 1;
					var ourLegalId = accounting.IsIncome ? accounting.OurLegalId : contracts.Where(w => w.ID == orders.First(ow => ow.ID == accounting.OrderId).ContractId).Select(con => con.OurLegalId).FirstOrDefault();
					var accountingCurrencyId = accountingLogic.GetServicesByAccounting(accounting.ID).Select(con => con.CurrencyId).FirstOrDefault();

					sheet.Cells[row, col++].Value = legalLogic.GetLegal(ourLegalId.Value).DisplayName;
					sheet.Cells[row, col++].Value = orders.First(w => w.ID == accounting.OrderId).Number;
					sheet.Cells[row, col++].Value = types.First(w => w.ID == accounting.AccountingDocumentTypeId).Display;
					sheet.Cells[row, col++].Value = accounting.Number;
					sheet.Cells[row, col++].Value = contracts.First(w => w.ID == (accounting.IsIncome ? orders.First(ow => ow.ID == accounting.OrderId).ContractId : accounting.ContractId)).Number;
					sheet.Cells[row, col++].Value = legalLogic.GetLegal(legalId).DisplayName;
					sheet.Cells[row, col++].Value = accounting.InvoiceNumber;
					sheet.Cells[row, col++].Value = accounting.InvoiceDate;
					sheet.Cells[row, col++].Value = accounting.ActDate;
					sheet.Cells[row, col++].Value = currencies.First(w => w.ID == (accountingCurrencyId ?? 1)).Display;
					sheet.Cells[row, col++].Value = accounting.OriginalSum;
					sheet.Cells[row, col++].Value = accounting.Sum;
					sheet.Cells[row, col++].Value = accounting.Vat;
					row++;
				}

				xl.SaveAs(new System.IO.FileInfo(filename));
			}

			return File(filename, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Баланс.xlsx");
		}

		public ContentResult GetLockingContractorInfo(int contractorId)
		{
			var list = orderLogic.GetOrdersByContractor(contractorId).Where(w => w.OrderStatusId != 6 && w.OrderStatusId != 7 && w.OrderStatusId != 9); // незакрытые заказы
			if (list.Count() > 0)
				return Content(JsonConvert.SerializeObject(new { Message = "Внимание!\nЭтот контрагент имеет несколько незакрытых заказов: " + list.Count() + "\n" + String.Join(",", list.Select(s => s.Number).ToArray()) }));
			else
				return Content(JsonConvert.SerializeObject(""));
		}

		public ContentResult ToggleContractorIsLocked(int contractorId, bool isLocked)
		{
			var contractor = contractorLogic.GetContractor(contractorId);

			if (!participantLogic.IsAllowedActionByContractor(27, contractorId, CurrentUserId))
				return Content(JsonConvert.SerializeObject(new { ActionId = 27, Message = "У вас недостаточно прав на выполнение этого действия." }));

			contractor.IsLocked = isLocked;

			contractorLogic.UpdateContractor(contractor);

			return Content(JsonConvert.SerializeObject(""));
		}

		public ContentResult SaveContractor(ContractorEditModel model)
		{
			var contractor = contractorLogic.GetContractor(model.ID);
			if (contractor.IsLocked)
				return Content(JsonConvert.SerializeObject(new { ActionId = 1, Message = "Контрагент зафиксирован, менять его данные нельзя." }));

			if (!participantLogic.IsAllowedActionByContractor(1, model.ID, CurrentUserId))
				return Content(JsonConvert.SerializeObject(new { ActionId = 1, Message = "У вас недостаточно прав на выполнение этого действия." }));

			contractor.Name = string.IsNullOrEmpty(model.Name) ? "" : model.Name.ToUpperInvariant();

			contractorLogic.UpdateContractor(contractor);

			return Content(JsonConvert.SerializeObject(""));
		}

		public ContentResult SaveLegal(LegalEditModel model)
		{
			//if (contractorLogic.GetContractor(model.ContractorId).IsLocked)
			//	return Content(JsonConvert.SerializeObject(new { ActionId = 4, Message = "Контрагент зафиксирован, менять его данные нельзя." }));

			if ((model.ID == 0) && !model.IsDeleted)
			{
				// новое юрлицо
				var legal = new Legal();

				legal.ContractorId = model.ContractorId;
				legal.Name = model.Name;
				legal.TaxTypeId = (model.TaxTypeId > 0) ? model.TaxTypeId : (int?)null;
				legal.DirectorId = model.DirectorId;
				legal.AccountantId = model.AccountantId;
				legal.Name = model.Name;
				legal.DisplayName = model.DisplayName;
				legal.EnName = model.EnName;
				legal.EnShortName = model.EnShortName;
				legal.TIN = model.TIN;
				legal.OGRN = model.OGRN;
				legal.KPP = model.KPP;
				legal.OKPO = model.OKPO;
				legal.OKVED = model.OKVED;
				legal.Address = model.Address;
				legal.EnAddress = model.EnAddress;
				legal.AddressFact = model.AddressFact;
				legal.EnAddressFact = model.EnAddressFact;
				legal.PostAddress = model.PostAddress;
				legal.EnPostAddress = model.EnPostAddress;
				legal.WorkTime = model.WorkTime;
				legal.TimeZone = model.TimeZone;
				legal.IsNotResident = model.IsNotResident;

				legalLogic.CreateLegal(legal);
			}
			else
			{
				if (!participantLogic.IsAllowedActionByContractor(4, model.ContractorId, CurrentUserId))
					return Content(JsonConvert.SerializeObject(new { ActionId = 4, Message = "У вас недостаточно прав на выполнение этого действия." }));

				if (model.IsDeleted)
				{
					// TODO: Проверить наличие других связанных сущностей
					try
					{
						// удалить юрлицо
						legalLogic.DeleteLegal(model.ID);
					}
					catch
					{
						return Content(JsonConvert.SerializeObject(new { Message = "Невозможно удалить юрлицо пока остались связанные данные" }));
					}

					return Content(JsonConvert.SerializeObject(""));
				}
				else
				{
					// обновить юрлицо
					var legal = legalLogic.GetLegal(model.ID);

					legal.Name = model.Name;
					legal.TaxTypeId = model.TaxTypeId;
					legal.DirectorId = model.DirectorId;
					legal.AccountantId = model.AccountantId;
					legal.Name = model.Name;
					legal.DisplayName = model.DisplayName;
					legal.EnName = model.EnName;
					legal.EnShortName = model.EnShortName;
					legal.TIN = model.TIN;
					legal.OGRN = model.OGRN;
					legal.KPP = model.KPP;
					legal.OKPO = model.OKPO;
					legal.OKVED = model.OKVED;
					legal.Address = model.Address;
					legal.EnAddress = model.EnAddress;
					legal.AddressFact = model.AddressFact;
					legal.EnAddressFact = model.EnAddressFact;
					legal.PostAddress = model.PostAddress;
					legal.EnPostAddress = model.EnPostAddress;
					legal.WorkTime = model.WorkTime;
					legal.TimeZone = model.TimeZone;
					legal.IsNotResident = model.IsNotResident;

					legal.UpdatedBy = CurrentUserId;
					legal.UpdatedDate = DateTime.Now;

					legalLogic.UpdateLegal(legal);
				}
			}

			foreach (var account in model.BankAccounts)
				SaveBankAccount(account);

			foreach (var employee in model.Employees)
				SaveEmployee(employee);

			return Content(JsonConvert.SerializeObject(""));
		}

		public ContentResult SaveBankAccount(BankAccountEditModel model)
		{
			var legal = legalLogic.GetLegal(model.LegalId.Value);
			if (legal.ContractorId.HasValue)
			{
				if (contractorLogic.GetContractor(legal.ContractorId.Value).IsLocked)
					return Content(JsonConvert.SerializeObject(new { ActionId = 7, Message = "Контрагент зафиксирован, менять его данные нельзя." }));

				// проверка прав участника
				if (!participantLogic.IsAllowedActionByContractor(7, legal.ContractorId.Value, CurrentUserId))
					return Content(JsonConvert.SerializeObject(new { ActionId = 7, Message = "У вас недостаточно прав на выполнение этого действия." }));
			}

			// TODO: Create?

			if (model.IsDeleted)
			{
				try
				{
					// удалить банковский счет
					bankLogic.DeleteBankAccount(model.ID);
				}
				catch
				{
					return Content(JsonConvert.SerializeObject(new { Message = "Невозможно удалить банковский счет пока он используется" }));
				}
			}
			else
			{
				// обновить банковский счет
				var account = bankLogic.GetBankAccount(model.ID);

				account.BankId = model.BankId;
				account.LegalId = model.LegalId;
				account.CurrencyId = model.CurrencyId;
				account.Number = model.Number;
				account.CoBankName = model.CoBankName;
				account.CoBankAccount = model.CoBankAccount;
				account.CoBankSWIFT = model.CoBankSWIFT;
				account.CoBankIBAN = model.CoBankIBAN;
				account.CoBankAddress = model.CoBankAddress;

				bankLogic.UpdateBankAccount(account);
			}

			return Content(JsonConvert.SerializeObject(""));
		}

		public ContentResult SaveEmployee(EmployeeEditModel model)
		{
			if (model.ContractorId.HasValue && contractorLogic.GetContractor(model.ContractorId.Value).IsLocked && !IsSuperUser())
				return Content(JsonConvert.SerializeObject(new { Message = "Контрагент зафиксирован, менять его данные нельзя." }));

			if ((model.ID == 0) && !model.IsDeleted)
			{
				// новый сотрудник
				var employee = new Employee();
				employee.PersonId = model.PersonId;
				employee.LegalId = model.LegalId;
				employee.ContractorId = model.ContractorId;
				employee.BeginDate = model.BeginDate;
				employee.EndDate = model.EndDate;
				employee.Department = model.Department;
				employee.Position = model.Position;
				employee.GenitivePosition = model.GenitivePosition;
				employee.Comment = model.Comment;
				employee.Basis = model.Basis;
				employee.EnPosition = model.EnPosition;
				employee.EnBasis = model.EnBasis;
				employee.FinRepCenterId = model.FinRepCenterId;

				employeeLogic.CreateEmployee(employee);
			}
			else
			{
				if (model.IsDeleted)
				{
					// удалить сотрудника
					employeeLogic.DeleteEmployee(model.ID);
				}
				else
				{
					// обновить сотрудника
					var employee = employeeLogic.GetEmployee(model.ID);
					employee.PersonId = model.PersonId;
					employee.LegalId = model.LegalId;
					employee.ContractorId = model.ContractorId;
					employee.BeginDate = model.BeginDate;
					employee.EndDate = model.EndDate;
					employee.Department = model.Department;
					employee.Position = model.Position;
					employee.GenitivePosition = model.GenitivePosition;
					employee.Comment = model.Comment;
					employee.Basis = model.Basis;
					employee.EnPosition = model.EnPosition;
					employee.EnBasis = model.EnBasis;
					employee.FinRepCenterId = model.FinRepCenterId;

					employeeLogic.UpdateEmployee(employee);
				}
			}

			return Content(string.Empty);
		}

		public ContentResult SaveParticipant(ParticipantEditModel model)
		{
			if (model.FromDate.HasValue)
				model.FromDate = model.FromDate.Value.ToUniversalTime();

			if (model.ToDate.HasValue)
				model.ToDate = model.ToDate.Value.ToUniversalTime();

			if (model.IsDeputy && (model.ParticipantRoleId != (int)ParticipantRoles.AM))
				return Content(JsonConvert.SerializeObject(new { Message = "Заместителем может быть только AM" }));

			if ((model.ID == 0) && !model.IsDeleted)
			{
				#region проверить уникальность ответственных

				if (model.IsResponsible && !IsAllowedResponsibleParticipant(model))
					return Content(JsonConvert.SerializeObject(new { Message = "Уже есть ответственный AM в этом периоде." }));

				#endregion

				// новый 
				var participant = new Participant();
				participant.UserId = model.UserId;
				participant.OrderId = model.OrderId;
				participant.ContractorId = model.ContractorId;
				participant.ParticipantRoleId = model.ParticipantRoleId;
				participant.FromDate = model.FromDate;
				participant.ToDate = model.ToDate;
				participant.IsDeputy = model.IsDeputy;
				participant.IsResponsible = model.IsResponsible;

				var id = participantLogic.CreateParticipant(participant);

				// если создан участник-заместитель в контрагенте
				if (participant.IsDeputy && participant.ContractorId.HasValue)
					foreach (var item in orderLogic.GetOrdersByContractor(participant.ContractorId.Value).Where(w => w.OrderStatusId == 2 || w.OrderStatusId == 3))
						participantLogic.CreateParticipant(new Participant
						{
							UserId = participant.UserId,
							IsDeputy = true,
							OrderId = item.ID,
							FromDate = participant.FromDate,
							ToDate = participant.ToDate,
							ParticipantRoleId = (int)ParticipantRoles.AM
						});

				// если создан участник-ответственный в контрагенте, протянуть его в заказы контрагента
				if (participant.IsResponsible && participant.ContractorId.HasValue)
					PullNewParticipant(participant);

				return Content(JsonConvert.SerializeObject(new { ID = id }));
			}
			else
			{
				var participant = participantLogic.GetParticipant(model.ID);

				// TEMP:
				//if (participant.OrderId.HasValue)
				//{
				//	var order = orderLogic.GetOrder(participant.OrderId.Value);
				//	if (order.OrderStatusId >= 4)
				//		return Content(JsonConvert.SerializeObject(new { Message = "Это нельзя делать с заказом в таком статусе." }));

				//	// проверка прав участника
				//	if (!participantLogic.IsAllowedActionByOrder(25, participant.OrderId.Value, CurrentUserId))
				//		return Content(JsonConvert.SerializeObject(new { ActionId = 25, Message = "У вас недостаточно прав на выполнение этого действия." }));
				//}
				//else
				//{
				//	// проверка прав участника
				//	if (!participantLogic.IsAllowedActionByContractor(9, participant.ContractorId.Value, CurrentUserId))
				//		return Content(JsonConvert.SerializeObject(new { ActionId = 9, Message = "У вас недостаточно прав на выполнение этого действия." }));
				//}

				if (model.IsDeleted)
				{
					// удалить 
					participantLogic.DeleteParticipant(model.ID);
				}
				else
				{
					#region проверить уникальность ответственных

					if (model.IsResponsible && !IsAllowedResponsibleParticipant(model))
						return Content(JsonConvert.SerializeObject(new { Message = "Уже есть ответственный AМ в этом периоде." }));

					#endregion

					// обновить 
					var isDeputyChanged = model.IsDeputy != participant.IsDeputy;

					participant.UserId = model.UserId;
					participant.ParticipantRoleId = model.ParticipantRoleId;
					participant.FromDate = model.FromDate;
					participant.ToDate = model.ToDate;
					participant.IsDeputy = model.IsDeputy;
					participant.IsResponsible = model.IsResponsible;

					participantLogic.UpdateParticipant(participant);

					// если установлен флаг заместитель участнику контрагента
					if (isDeputyChanged && participant.IsDeputy && participant.ContractorId.HasValue)
						foreach (var item in orderLogic.GetOrdersByContractor(participant.ContractorId.Value).Where(w => w.OrderStatusId == 2 || w.OrderStatusId == 3))
							participantLogic.CreateParticipant(new Participant
							{
								OrderId = item.ID,
								IsDeputy = true,
								FromDate = participant.FromDate,
								ToDate = participant.ToDate,
								UserId = participant.UserId,
								ParticipantRoleId = (int)ParticipantRoles.AM
							});

					// если измене участник-ответственный в контрагенте, протянуть его изменения в заказах контрагента
					if (participant.IsResponsible && participant.ContractorId.HasValue)
						PullParticipant(participant);
				}
			}

			return Content(JsonConvert.SerializeObject(""));
		}

		public ContentResult SavePerson(PersonEditModel model)
		{
			var personId = model.ID;
			if ((model.ID == 0) && !model.IsDeleted)
			{
				// новое 
				var person = new Person();
				person.Name = model.Name;
				person.DisplayName = model.DisplayName;
				person.EnName = model.EnName;
				person.Address = model.Address;
				person.Email = model.Email;
				person.IsNotResident = model.IsNotResident;
				person.Comment = model.Comment;
				person.Family = (string.IsNullOrEmpty(model.Family)) ? "" : model.Family.ToUpperInvariant();
				person.GenitiveFamily = model.GenitiveFamily;
				person.GenitiveName = model.GenitiveName;
				person.GenitivePatronymic = model.GenitivePatronymic;
				person.Initials = model.Initials;
				person.IsSubscribed = model.IsSubscribed;
				person.Patronymic = model.Patronymic;

				personId = personLogic.CreatePerson(person);
			}
			else
			{
				if (model.IsDeleted)
				{
					// удалить 
					personLogic.DeletePerson(model.ID);
					return Content(string.Empty);
				}
				else
				{
					// обновить 
					var person = personLogic.GetPerson(model.ID);
					person.Name = model.Name;
					person.DisplayName = model.DisplayName;
					person.EnName = model.EnName;
					person.Address = model.Address;
					person.Email = model.Email;
					person.IsNotResident = model.IsNotResident;
					person.Comment = model.Comment;
					person.Family = (string.IsNullOrEmpty(model.Family)) ? "" : model.Family.ToUpperInvariant();
					person.GenitiveFamily = model.GenitiveFamily;
					person.GenitiveName = model.GenitiveName;
					person.GenitivePatronymic = model.GenitivePatronymic;
					person.Initials = model.Initials;
					person.IsSubscribed = model.IsSubscribed;
					person.Patronymic = model.Patronymic;

					personLogic.UpdatePerson(person);
				}
			}

			// phones
			foreach (var phone in model.Phones)
				SavePhone(phone, personId);

			return Content(JsonConvert.SerializeObject(""));
		}

		public void SavePhone(PhoneEditModel model, int personId)
		{
			if ((model.ID == 0) && !model.IsDeleted)
			{
				// новый 
				var phone = new Phone();
				phone.Name = model.Name;
				phone.Number = model.Number;
				phone.TypeId = model.TypeId;

				var phoneId = personLogic.CreatePhone(phone);

				var rel = new PersonPhone();
				rel.PersonId = personId;
				rel.PhoneId = phoneId;
				personLogic.CreatePersonPhone(rel);
			}
			else
			{
				if (model.IsDeleted)
				{
					// удалить 
					var rel = personLogic.GetPersonPhone(personId, model.ID);
					if (rel != null)
						personLogic.DeletePersonPhone(rel.ID);

					// ??
					//personLogic.DeletePhone(model.ID);
				}
				else
				{
					// обновить 
					var phone = personLogic.GetPhone(model.ID);
					phone.Name = model.Name;
					phone.Number = model.Number;
					phone.TypeId = model.TypeId;

					personLogic.UpdatePhone(phone);
				}
			}
		}

		bool IsAllowedResponsibleParticipant(ParticipantEditModel model)
		{
			var fromDate = model.FromDate ?? DateTime.MinValue;
			var toDate = model.ToDate ?? DateTime.MaxValue;
			var workgroup = model.ContractorId.HasValue ? participantLogic.GetWorkgroupByContractor(model.ContractorId.Value) : participantLogic.GetWorkgroupByOrder(model.OrderId.Value);
			foreach (var item in workgroup.Where(w => w.IsResponsible && (w.ID != model.ID) && (w.ParticipantRoleId == (int)ParticipantRoles.AM)))
			{
				var itemFromDate = item.FromDate ?? DateTime.MinValue;
				var itemToDate = item.ToDate ?? DateTime.MaxValue;

				// если один из проверяемых без границ
				if ((fromDate == DateTime.MinValue && toDate == DateTime.MaxValue) || (itemFromDate == DateTime.MinValue && itemToDate == DateTime.MaxValue))
					return false;

				// если model справа
				if ((fromDate <= itemFromDate) && (toDate >= itemFromDate))
					return false;

				// если model слева
				if ((fromDate <= itemToDate) && (toDate >= itemToDate))
					return false;

				// если model внутри
				if ((fromDate >= itemFromDate) && (toDate <= itemToDate))
					return false;
			}

			return true;
		}

		void PullParticipant(Participant model)
		{
			var orders = orderLogic.GetOrdersByContractor(model.ContractorId.Value);
			foreach (var order in orders)
			{
				// с закрытыми ничего не делаем
				if (order.OrderStatusId.In(6, 7, 9))
					continue;

				var participant = participantLogic.GetWorkgroupByOrder(order.ID).FirstOrDefault(w => w.ParticipantRoleId == (int)ParticipantRoles.AM && w.UserId == model.UserId);
				if (participant != null)
				{
					// обновление без проверки
					participant.FromDate = model.FromDate;
					participant.ToDate = model.ToDate;
					participant.IsResponsible = model.IsResponsible;
					participantLogic.UpdateParticipant(participant);
				}
			}
		}

		void PullNewParticipant(Participant model)
		{
			var orders = orderLogic.GetOrdersByContractor(model.ContractorId.Value);
			foreach (var order in orders)
			{
				if (order.OrderStatusId.In(6, 7, 9)) // в закрытые просто добавляем
					participantLogic.CreateParticipant(new Participant
					{
						UserId = model.UserId,
						OrderId = order.ID,
						ParticipantRoleId = model.ParticipantRoleId
					});
				else
					participantLogic.CreateParticipant(new Participant
					{
						UserId = model.UserId,
						OrderId = order.ID,
						ParticipantRoleId = model.ParticipantRoleId,
						FromDate = model.FromDate,
						ToDate = model.ToDate,
						IsResponsible = model.IsResponsible
					});
			}
		}
	}
}