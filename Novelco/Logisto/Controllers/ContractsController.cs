using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Logisto.Models;
using Logisto.ViewModels;
using Newtonsoft.Json;

namespace Logisto.Controllers
{
	[Authorize]
	public class ContractsController : BaseController
	{
		#region Pages

		public ActionResult View(int id)
		{
			var model = contractLogic.GetContractInfo(id);
			model.Add("Documents", documentLogic.GetDocumentsByContract(id));
			return View(model);
		}

		public ActionResult Create(int legalId)
		{
			// проверка прав участника
			if (!participantLogic.IsAllowedActionByContractor(28, legalLogic.GetLegal(legalId).ContractorId.Value, CurrentUserId))
				return RedirectToAction("NotAuthorized", "Home", new { actionId = 28 });

			var contract = new Contract
			{
				LegalId = legalId,
				OurLegalId = 1,
				Date = DateTime.Now,
				Number = DateTime.Today.ToOADate().ToString("0"),
				CurrencyRateUseId = 2   // ЦБ РФ на день оплаты + 1,5%
			};

			int id = contractLogic.CreateContract(contract);
			return RedirectToAction("Edit", new { Id = id });
		}

		public ActionResult Edit(int id)
		{
			// TODO: roles
			var contract = contractLogic.GetContract(id);
			var model = new ContractViewModel
			{
				BankAccountId = contract.BankAccountId,
				BeginDate = contract.BeginDate,
				Comment = contract.Comment,
				ContractRoleId = contract.ContractRoleId,
				ContractServiceTypeId = contract.ContractServiceTypeId,
				ContractTypeId = contract.ContractTypeId,
				Date = contract.Date,
				EndDate = contract.EndDate,
				ID = contract.ID,
				IsFixed = contract.IsFixed,
				IsProlongation = contract.IsProlongation,
				LegalId = contract.LegalId,
				Number = contract.Number,
				OurBankAccountId = contract.OurBankAccountId,
				OurContractRoleId = contract.OurContractRoleId,
				OurLegalId = contract.OurLegalId,
				PaymentTermsId = contract.PaymentTermsId,
				CurrencyRateUseId = contract.CurrencyRateUseId,
				PayMethodId = contract.PayMethodId,
				AgentPercentage = contract.AgentPercentage,

				Currencies = contractLogic.GetContractCurrencies(id)
			};

			model.Dictionaries.Add("User", dataLogic.GetUsers());
			model.Dictionaries.Add("Legal", dataLogic.GetLegals());
			model.Dictionaries.Add("OurLegal", dataLogic.GetOurLegals());
			model.Dictionaries.Add("Currency", dataLogic.GetCurrencies());
			model.Dictionaries.Add("PayMethod", dataLogic.GetPayMethods());
			model.Dictionaries.Add("PaymentTerm", dataLogic.GetPaymentTerms());
			model.Dictionaries.Add("DocumentType", dataLogic.GetDocumentTypes());
			model.Dictionaries.Add("ContractType", dataLogic.GetContractTypes());
			model.Dictionaries.Add("ContractRole", dataLogic.GetContractRoles());
			model.Dictionaries.Add("CurrencyRateUse", dataLogic.GetCurrencyRateUses());
			model.Dictionaries.Add("ContractServiceType", dataLogic.GetContractServiceTypes());
			model.Dictionaries.Add("AccountingDocumentType", dataLogic.GetAccountingDocumentTypes());

			return View(model);
		}

		#endregion

		#region marks

		public ContentResult ToggleContractOk(int contractId)
		{
			var legalId = contractLogic.GetContract(contractId).LegalId;
			var contractorId = legalLogic.GetLegal(legalId).ContractorId.Value;

			var mark = contractLogic.GetContractMarkByContract(contractId);
			if (mark == null)
				mark = new ContractMark { ContractId = contractId };

			if (mark.IsContractOk)
			{
				// проверка прав участника
				if (!participantLogic.IsAllowedActionByContractor(30, contractorId, CurrentUserId))
					return Content(JsonConvert.SerializeObject(new { ActionId = 30, Message = "У вас недостаточно прав на выполнение этого действия." }));

				mark.IsContractOk = false;
				mark.ContractOkDate = null;
				mark.ContractOkUserId = null;
			}
			else
			{
				// проверка прав участника
				if (!participantLogic.IsAllowedActionByContractor(29, contractorId, CurrentUserId))
					return Content(JsonConvert.SerializeObject(new { ActionId = 29, Message = "У вас недостаточно прав на выполнение этого действия." }));

				mark.IsContractOk = true;
				mark.ContractOkDate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
				mark.ContractOkUserId = CurrentUserId;
			}

			if (mark.ID == 0)
			{
				var id = contractLogic.CreateContractMark(mark);
				mark = contractLogic.GetContractMark(id);
			}
			else
				contractLogic.UpdateContractMark(mark);

			contractLogic.CreateContractMarksHistory(new ContractMarksHistory { ContractId = contractId, Date = DateTime.Now, Text = mark.IsContractOk ? "Договор Ок" : "Снята метка 'Договор Ок'", Reason = "", UserId = CurrentUserId });

			return Content(JsonConvert.SerializeObject(mark));
		}

		public ContentResult ToggleContractChecked(int contractId)
		{
			var legalId = contractLogic.GetContract(contractId).LegalId;
			var contractorId = legalLogic.GetLegal(legalId).ContractorId.Value;

			var mark = contractLogic.GetContractMarkByContract(contractId);
			if (mark == null)
				mark = new ContractMark { ContractId = contractId };

			if (mark.IsContractChecked)
			{
				// проверка прав участника
				if (!participantLogic.IsAllowedActionByContractor(32, contractorId, CurrentUserId))
					return Content(JsonConvert.SerializeObject(new { ActionId = 32, Message = "У вас недостаточно прав на выполнение этого действия." }));

				mark.IsContractChecked = false;
				mark.ContractCheckedDate = null;
				mark.ContractCheckedUserId = null;
			}
			else
			{
				// проверка прав участника
				if (!participantLogic.IsAllowedActionByContractor(31, contractorId, CurrentUserId))
					return Content(JsonConvert.SerializeObject(new { ActionId = 31, Message = "У вас недостаточно прав на выполнение этого действия." }));

				mark.IsContractChecked = true;
				mark.ContractCheckedDate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
				mark.ContractCheckedUserId = CurrentUserId;
			}

			if (mark.ID == 0)
			{
				var id = contractLogic.CreateContractMark(mark);
				mark = contractLogic.GetContractMark(id);
			}
			else
				contractLogic.UpdateContractMark(mark);

			contractLogic.CreateContractMarksHistory(new ContractMarksHistory { ContractId = contractId, Date = DateTime.Now, Text = mark.IsContractChecked ? "Договор проверен" : "Снята метка 'Договор проверен'", Reason = "", UserId = CurrentUserId });

			return Content(JsonConvert.SerializeObject(mark));
		}

		public ContentResult ToggleContractRejected(int contractId, string comment)
		{
			var legalId = contractLogic.GetContract(contractId).LegalId;
			var contractorId = legalLogic.GetLegal(legalId).ContractorId.Value;

			var mark = contractLogic.GetContractMarkByContract(contractId);
			if (mark == null)
				mark = new ContractMark { ContractId = contractId };

			if (mark.IsContractRejected)
			{
				// проверка прав участника
				if (!participantLogic.IsAllowedActionByContractor(33, contractorId, CurrentUserId))
					return Content(JsonConvert.SerializeObject(new { ActionId = 33, Message = "У вас недостаточно прав на выполнение этого действия." }));

				mark.IsContractRejected = false;
				mark.ContractRejectedDate = null;
				mark.ContractRejectedUserId = null;
				mark.ContractRejectedComment = "";
			}
			else
			{
				// проверка прав участника
				if (!participantLogic.IsAllowedActionByContractor(33, contractorId, CurrentUserId))
					return Content(JsonConvert.SerializeObject(new { ActionId = 33, Message = "У вас недостаточно прав на выполнение этого действия." }));

				mark.IsContractRejected = true;
				mark.ContractRejectedDate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
				mark.ContractRejectedUserId = CurrentUserId;
				mark.ContractRejectedComment = comment;
			}

			if (mark.ID == 0)
			{
				var id = contractLogic.CreateContractMark(mark);
				mark = contractLogic.GetContractMark(id);
			}
			else
				contractLogic.UpdateContractMark(mark);

			contractLogic.CreateContractMarksHistory(new ContractMarksHistory { ContractId = contractId, Date = DateTime.Now, Text = mark.IsContractRejected ? "Договор отклонен" : "Снята метка 'Договор отклонен'", Reason = comment, UserId = CurrentUserId });

			return Content(JsonConvert.SerializeObject(mark));
		}

		public ContentResult ToggleContractBlocked(int contractId, string comment)
		{
			#region проверка прав участника

			var legalId = contractLogic.GetContract(contractId).LegalId;
			var contractorId = legalLogic.GetLegal(legalId).ContractorId.Value;

			var workgroup = participantLogic.GetWorkgroupByContractor(contractorId).Where(w => w.UserId == CurrentUserId);
			if (workgroup.Count() == 0)
				return Content(JsonConvert.SerializeObject(new { Message = "Вы не являетесь участником этого контрагента" }));

			if (!IsAllowed(workgroup, ParticipantRoles.BUH, ParticipantRoles.GM))
				return Content(JsonConvert.SerializeObject(new { Message = "У вас нет прав на этого контрагента" }));

			#endregion

			var mark = contractLogic.GetContractMarkByContract(contractId);
			if (mark == null)
				mark = new ContractMark { ContractId = contractId };

			if (mark.IsContractBlocked)
			{
				// проверка прав участника
				if (!participantLogic.IsAllowedActionByContractor(36, contractorId, CurrentUserId))
					return Content(JsonConvert.SerializeObject(new { ActionId = 36, Message = "У вас недостаточно прав на выполнение этого действия." }));

				mark.IsContractBlocked = false;
				mark.ContractBlockedDate = null;
				mark.ContractBlockedUserId = null;
				mark.ContractBlockedComment = "";
			}
			else
			{
				// проверка прав участника
				if (!participantLogic.IsAllowedActionByContractor(35, contractorId, CurrentUserId))
					return Content(JsonConvert.SerializeObject(new { ActionId = 35, Message = "У вас недостаточно прав на выполнение этого действия." }));

				mark.IsContractBlocked = true;
				mark.ContractBlockedDate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
				mark.ContractBlockedUserId = CurrentUserId;
				mark.ContractBlockedComment = comment;
			}

			if (mark.ID == 0)
				mark = contractLogic.GetContractMark(contractLogic.CreateContractMark(mark));
			else
				contractLogic.UpdateContractMark(mark);

			contractLogic.CreateContractMarksHistory(new ContractMarksHistory { ContractId = contractId, Date = DateTime.Now, Text = mark.IsContractBlocked ? "Договор блокирован" : "Снята метка 'Договор блокирован'", Reason = comment, UserId = CurrentUserId });


			string info = "";
			#region если договор блокируется в тот момент, когда по нему есть текущие доходы или расходы (по заказам в статусе отличном от мотивация)

			var accs = accountingLogic.GetAccountingsByContract(contractId);
			var numbers = new List<string>();
			foreach (var item in accs)
			{
				var order = orderLogic.GetOrder(item.OrderId);
				if (order.OrderStatusId != 7)
					numbers.Add(item.Number);
			}

			var allNumbers = string.Join(", ", numbers.ToArray());
			if (!string.IsNullOrEmpty(allNumbers))
				//return Content(JsonConvert.SerializeObject(new { ActionId = 35, Message = "На этот договор есть немотивированные заказы c доход/расходами: " + allNumbers }));
				info = "На этот договор есть немотивированные заказы c доход/расходами: " + allNumbers;

			#endregion

			return Content(JsonConvert.SerializeObject(new { Mark = mark, InfoMessage = info }));
		}

		#endregion

		#region шаблоны заказов

		public ActionResult OrderTemplates(int contractId)
		{
			var viewmodel = dataLogic.GetOrderTemplatesByContract(contractId);
			ViewBag.ContractId = contractId;
			return View(viewmodel);
		}

		public ContentResult GetNewOrderTemplate(int contractId)
		{
			var legalId = contractLogic.GetContract(contractId).LegalId;
			var contractorId = legalLogic.GetLegal(legalId).ContractorId.Value;

			// проверка прав участника
			if (!participantLogic.IsAllowedActionByContractor(37, legalLogic.GetLegal(contractorId).ContractorId.Value, CurrentUserId))
				return Content(JsonConvert.SerializeObject(new { ActionId = 37, Message = "У вас недостаточно прав на выполнение этого действия." }));

			// подстановка значений по-умолчанию
			var c = new OrderTemplate { ContractId = contractId };
			return Content(JsonConvert.SerializeObject(c));
		}

		public ContentResult SaveOrderTemplate(OrderTemplate model)
		{
			if (model.ID == 0)
			{
				var op = new OrderTemplate { Name = model.Name };
				op.Name = model.Name;
				op.ContractId = model.ContractId;

				var id = dataLogic.CreateOrderTemplate(op);
				return Content(JsonConvert.SerializeObject(new { ID = id }));
			}
			else
			{
				var legalId = contractLogic.GetContract(model.ContractId.Value).LegalId;
				var contractorId = legalLogic.GetLegal(legalId).ContractorId.Value;

				// проверка прав участника
				if (!participantLogic.IsAllowedActionByContractor(38, legalLogic.GetLegal(contractorId).ContractorId.Value, CurrentUserId))
					return Content(JsonConvert.SerializeObject(new { ActionId = 38, Message = "У вас недостаточно прав на выполнение этого действия." }));

				var op = dataLogic.GetOrderTemplate(model.ID);
				op.Name = model.Name;
				op.ContractId = model.ContractId;

				dataLogic.UpdateOrderTemplate(op);
				return Content(JsonConvert.SerializeObject(""));
			}
		}

		#endregion

		public ContentResult GetContractMarks(int contractId)
		{
			var mark = contractLogic.GetContractMarkByContract(contractId);
			if (mark == null)
				mark = new ContractMark { ContractId = contractId };

			return Content(JsonConvert.SerializeObject(mark));
		}

		public ContentResult GetContractMarksHistory(int contractId)
		{
			var list = contractLogic.GetContractMarksHistory(contractId);
			return Content(JsonConvert.SerializeObject(new { Items = list }));
		}

		public ContentResult SaveContract(ContractEditModel model)
		{
			if ((model.ID == 0) && !model.IsDeleted)
			{
				// новое
				var contract = new Contract();
				// исправить даты
				if (model.Date.HasValue && model.Date.Value.Kind != DateTimeKind.Utc)
					model.Date = model.Date.Value.ToUniversalTime();

				if (model.BeginDate.HasValue && model.BeginDate.Value.Kind != DateTimeKind.Utc)
					model.BeginDate = model.BeginDate.Value.ToUniversalTime();

				if (model.EndDate.HasValue && model.EndDate.Value.Kind != DateTimeKind.Utc)
					model.EndDate = model.EndDate.Value.ToUniversalTime();

				contract.LegalId = model.LegalId;
				contract.CurrencyRateUseId = model.CurrencyRateUseId;
				contract.Number = model.Number;
				contract.OurLegalId = model.OurLegalId;
				contract.BankAccountId = model.BankAccountId;
				contract.ContractRoleId = model.ContractRoleId;
				contract.ContractTypeId = model.ContractTypeId;
				contract.PaymentTermsId = model.PaymentTermsId;
				contract.OurBankAccountId = model.OurBankAccountId;
				contract.OurContractRoleId = model.OurContractRoleId;
				contract.ContractServiceTypeId = model.ContractServiceTypeId;
				contract.Comment = model.Comment;
				contract.IsFixed = model.IsFixed;
				contract.IsProlongation = model.IsProlongation;
				contract.Date = model.Date;
				contract.BeginDate = model.BeginDate;
				contract.EndDate = model.EndDate;
				contract.PayMethodId = model.PayMethodId;
				contract.AgentPercentage = model.AgentPercentage;

				var id = contractLogic.CreateContract(contract);
				SaveContractCurrencies(id, model.Currencies);

				return Content(JsonConvert.SerializeObject(new { ID = id }));
			}
			else
			{
				var contract = contractLogic.GetContract(model.ID);

				// проверка прав участника
				if (!participantLogic.IsAllowedActionByContractor(10, legalLogic.GetLegal(contract.LegalId).ContractorId.Value, CurrentUserId))
					return Content(JsonConvert.SerializeObject(new { ActionId = 10, Message = "У вас недостаточно прав на выполнение этого действия." }));

				if (model.IsDeleted)
				{
					// удалить договор
					contractLogic.DeleteContract(model.ID);
				}
				else
				{
					#region marks

					var mark = contractLogic.GetContractMarkByContract(model.ID);
					if ((mark != null) && (mark.IsContractChecked))
						return Content(JsonConvert.SerializeObject(new { ActionId = 10, Message = "Договор проверен, редактирование невозможно." }));

					#endregion

					// исправить даты
					if (model.Date.HasValue && model.Date.Value.Kind != DateTimeKind.Utc)
						model.Date = model.Date.Value.ToUniversalTime();

					if (model.BeginDate.HasValue && model.BeginDate.Value.Kind != DateTimeKind.Utc)
						model.BeginDate = model.BeginDate.Value.ToUniversalTime();

					if (model.EndDate.HasValue && model.EndDate.Value.Kind != DateTimeKind.Utc)
						model.EndDate = model.EndDate.Value.ToUniversalTime();

					contract.LegalId = model.LegalId;
					contract.CurrencyRateUseId = model.CurrencyRateUseId;
					contract.Number = model.Number;
					contract.OurLegalId = model.OurLegalId;
					contract.ContractRoleId = model.ContractRoleId;
					contract.ContractTypeId = model.ContractTypeId;
					contract.PaymentTermsId = model.PaymentTermsId;
					contract.OurContractRoleId = model.OurContractRoleId;
					contract.ContractServiceTypeId = model.ContractServiceTypeId;
					contract.Comment = model.Comment;
					contract.IsFixed = model.IsFixed;
					contract.IsProlongation = model.IsProlongation;
					contract.Date = model.Date;
					contract.BeginDate = model.BeginDate;
					contract.EndDate = model.EndDate;
					contract.PayMethodId = model.PayMethodId;
					contract.AgentPercentage = model.AgentPercentage;

					contractLogic.UpdateContract(contract);

					SaveContractCurrencies(contract.ID, model.Currencies);
				}
			}

			return Content(JsonConvert.SerializeObject(""));
		}

		void SaveContractCurrencies(int id, IEnumerable<ContractCurrency> currencies)
		{
			var list = contractLogic.GetContractCurrencies(id);

			// сначала удалить неактуальные
			foreach (var item in list)
				if (currencies.FirstOrDefault(w => w.CurrencyId == item.CurrencyId) == null)
					contractLogic.DeleteContractCurrency(item.ContractId, item.CurrencyId);

			// добавить новое 
			foreach (var item in currencies)
				if (list.FirstOrDefault(w => w.CurrencyId == item.CurrencyId) == null)
					contractLogic.CreateContractCurrency(new ContractCurrency { ContractId = id, CurrencyId = item.CurrencyId, OurBankAccountId = item.OurBankAccountId, BankAccountId = item.BankAccountId });
				else
				{
					var curr = list.FirstOrDefault(w => w.CurrencyId == item.CurrencyId);
					curr.BankAccountId = item.BankAccountId;
					curr.OurBankAccountId = item.OurBankAccountId;
					contractLogic.UpdateContractCurrency(curr);
				}
		}
	}
}