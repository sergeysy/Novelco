using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing.Imaging;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using GemBox.Spreadsheet;
using Logisto.Models;
using Logisto.ViewModels;
using Newtonsoft.Json;
using Spire.Pdf;
using Spire.Pdf.Actions;
using IO = System.IO;
using System.Diagnostics;
using Syncfusion.Pdf.Parsing;
using Logisto.Model;
using OfficeOpenXml;

namespace Logisto.Controllers
{
	[Authorize]
	public class OrdersController : BaseController
	{
		const int INTERNAL_CONTRACTOR_ID = 168;
		const int RATE_DIFF_CONTRACTOR_ID = 348;
		const int FINSERVICES_CONTRACTOR_ID = 372;
		const int FINSERVICES_LEGAL_ID = 798;
		const int AUTOEXPENSE_LEGAL_ID = 799;
		const int DIFF_LEGAL_ID = 738;
		const int NEWTYPELIMIT = 2198;

		#region Pages

		public ActionResult Index()
		{
			var pageSize = int.Parse(ConfigurationManager.AppSettings["App_PageSize"]);
			var filter = new ListFilter { PageSize = pageSize, Type = "Current" };
			if (CurrentUserId > 0)
			{
				filter.Sort = userLogic.GetSetting(CurrentUserId, "Orders.Sort");
				filter.SortDirection = userLogic.GetSetting(CurrentUserId, "Orders.SortDirection");
				filter.PageNumber = Convert.ToInt32(userLogic.GetSetting(CurrentUserId, "Orders.PageNumber"));
				filter.UserId = CurrentUserId;
			}

			var totalCount = orderLogic.GetOrdersCount(filter);
			var viewModel = new OrdersViewModel { Filter = filter, Items = new List<Order>() };

			viewModel.Dictionaries.Add("Legal", dataLogic.GetLegals());
			viewModel.Dictionaries.Add("Product", dataLogic.GetProducts());
			viewModel.Dictionaries.Add("Contract", dataLogic.GetContracts());
			viewModel.Dictionaries.Add("OurLegal", dataLogic.GetOurLegals());
			viewModel.Dictionaries.Add("Currency", dataLogic.GetCurrencies());
			viewModel.Dictionaries.Add("PayMethod", dataLogic.GetPayMethods());
			viewModel.Dictionaries.Add("OrderStatus", dataLogic.GetOrderStatuses());
			viewModel.Dictionaries.Add("ServiceType", dataLogic.GetServiceTypes());
			viewModel.Dictionaries.Add("PaymentTerm", dataLogic.GetPaymentTerms());
			viewModel.Dictionaries.Add("ContractRole", dataLogic.GetContractRoles());
			viewModel.Dictionaries.Add("ContractType", dataLogic.GetContractTypes());
			viewModel.Dictionaries.Add("LegalByContract", dataLogic.GetLegalsByContract());
			viewModel.Dictionaries.Add("ContractorByOrder", dataLogic.GetContractorsByOrder());
			viewModel.Dictionaries.Add("ContractServiceType", dataLogic.GetContractServiceTypes());

			// время от времени проверять необходимость выполнения задач по расписанию
			if (new Random(Environment.TickCount).Next(8) == 4)
				System.Threading.Tasks.Task.Run((System.Action)ProcessJob);

			return View(viewModel);
		}

		public ActionResult Details(int id, int? selectedAccountingId)
		{
#if !DEBUG
			if (!participantLogic.GetWorkgroupByOrder(id).Any(w => w.UserId == CurrentUserId))
				return RedirectToAction("NotAuthorized", "Home");
#endif

			orderLogic.CalculateOrderBalance(id);   // TEMP:

			var viewModel = new OrderViewModel { Order = orderLogic.GetOrder(id) };
			viewModel.PricelistId = GetPricelist(viewModel.Order)?.ID ?? 0;
			viewModel.FinRepCenter = dataLogic.GetFinRepCenter(viewModel.Order.FinRepCenterId.Value);
			viewModel.Rentability = ((viewModel.Order.Balance ?? 0) / (viewModel.Order.Income ?? 1)) * 100;
			if (viewModel.Order.ContractId.HasValue)
			{
				var contract = contractLogic.GetContract(viewModel.Order.ContractId.Value);
				viewModel.ContractorId = legalLogic.GetLegal(contract.LegalId).ContractorId.Value;
			}

			viewModel.MinRentability = dataLogic.GetOrdersRentability().Where(w => w.FinRepCenterId == viewModel.Order.FinRepCenterId)
														.Where(w => w.OrderTemplateId == viewModel.Order.OrderTemplateId)
														.Where(w => w.ProductId == viewModel.Order.ProductId)
														.Where(w => w.Year == viewModel.Order.CreatedDate.Value.Year)
														.Select(s => s.Rentability).FirstOrDefault();


			viewModel.Dictionaries.Add("Vat", dataLogic.GetVats());
			viewModel.Dictionaries.Add("Role", dataLogic.GetRoles());
			viewModel.Dictionaries.Add("User", dataLogic.GetUsers());
			viewModel.Dictionaries.Add("Event", dataLogic.GetEvents());
			viewModel.Dictionaries.Add("Legal", dataLogic.GetLegals());
			viewModel.Dictionaries.Add("TaxType", dataLogic.GetTaxTypes());
			viewModel.Dictionaries.Add("Measure", dataLogic.GetMeasures());
			viewModel.Dictionaries.Add("Product", dataLogic.GetProducts());
			viewModel.Dictionaries.Add("OurLegal", dataLogic.GetOurLegals());
			viewModel.Dictionaries.Add("Currency", dataLogic.GetCurrencies());
			viewModel.Dictionaries.Add("Employee", dataLogic.GetEmployees());
			viewModel.Dictionaries.Add("Template", dataLogic.GetTemplates().Select(s => new { ID = s.ID, Name = s.Name }));
			viewModel.Dictionaries.Add("ActiveUser", dataLogic.GetActiveUsers());
			viewModel.Dictionaries.Add("OrderType", dataLogic.GetOrderTypes());
			viewModel.Dictionaries.Add("Pricelist", pricelistLogic.GetPricelists(new Models.ListFilter()).Select(s => new Pricelist { ID = s.ID, Comment = s.Comment, ContractId = s.ContractId, FinRepCenterId = s.FinRepCenterId, From = s.From, Name = s.Name, ProductId = s.ProductId, To = s.To }));
			viewModel.Dictionaries.Add("PayMethod", dataLogic.GetPayMethods());
			viewModel.Dictionaries.Add("Contractor", dataLogic.GetContractors());
			viewModel.Dictionaries.Add("OrderStatus", dataLogic.GetOrderStatuses());
			viewModel.Dictionaries.Add("PackageType", dataLogic.GetPackageTypes());
			viewModel.Dictionaries.Add("PaymentTerm", dataLogic.GetPaymentTerms());
			viewModel.Dictionaries.Add("FinRepCenter", dataLogic.GetFinRepCenters());
			viewModel.Dictionaries.Add("ContractType", dataLogic.GetContractTypes());
			viewModel.Dictionaries.Add("ContractRole", dataLogic.GetContractRoles());
			viewModel.Dictionaries.Add("CurrencyRate", dataLogic.GetCurrencyRates(DateTime.Today));
			viewModel.Dictionaries.Add("OrderTemplate", dataLogic.GetOrderTemplates());
			viewModel.Dictionaries.Add("InsuranceType", dataLogic.GetInsuranceTypes());
			viewModel.Dictionaries.Add("TransportType", dataLogic.GetTransportTypes());
			viewModel.Dictionaries.Add("LegalProvider", dataLogic.GetLegalProviders());
			viewModel.Dictionaries.Add("OperationKind", dataLogic.GetOperationKinds());
			viewModel.Dictionaries.Add("OrderOperation", dataLogic.GetOrderOperations());
			viewModel.Dictionaries.Add("RoutePointType", dataLogic.GetRoutePointTypes());
			viewModel.Dictionaries.Add("ParticipantRole", dataLogic.GetParticipantRoles());
			viewModel.Dictionaries.Add("OperationStatus", dataLogic.GetOperationStatuses());
			viewModel.Dictionaries.Add("VolumetricRatio", dataLogic.GetVolumetricRatios());
			viewModel.Dictionaries.Add("LegalByContract", dataLogic.GetLegalsByContract());
			viewModel.Dictionaries.Add("UninsuranceType", dataLogic.GetUninsuranceTypes());
			viewModel.Dictionaries.Add("PositionTemplate", dataLogic.GetPositionTemplates());
			viewModel.Dictionaries.Add("CargoDescription", dataLogic.GetCargoDescriptions());
			viewModel.Dictionaries.Add("ContractServiceType", dataLogic.GetContractServiceTypes());
			viewModel.Dictionaries.Add("AccountingPaymentType", dataLogic.GetAccountingPaymentTypes());
			viewModel.Dictionaries.Add("AccountingDocumentType", dataLogic.GetAccountingDocumentTypes());
			viewModel.Dictionaries.Add("AccountingPaymentMethod", dataLogic.GetAccountingPaymentMethods());

			var types = dataLogic.GetDocumentTypes();
			var prints = dataLogic.GetDocumentTypePrints(viewModel.Order.ProductId);
			viewModel.Dictionaries.Add("DocumentType", types.Select(s => new { s.ID, s.Display, s.IsNipVisible, IsPrint = prints.Any(w => w.DocumentTypeId == s.ID) }));

			// выбрать подходящие типы сервиса из дерева
			var serviceTypesVM = new List<ServiceTypeViewModel> { new ServiceTypeViewModel { ID = 0, Name = "" } };
			var serviceKinds = dataLogic.GetServiceKinds().Where(w => w.ProductId == viewModel.Order.ProductId).ToList();
			foreach (var item in dataLogic.GetServiceTypes())
			{
				var kind = serviceKinds.FirstOrDefault(w => w.ID == item.ServiceKindId);
				if (kind != null)
					serviceTypesVM.Add(new ServiceTypeViewModel
					{
						ID = item.ID,
						Count = item.Count,
						MeasureId = item.MeasureId,
						Name = item.Name,
						Price = item.Price,
						ServiceKindId = item.ServiceKindId,
						VatId = kind.VatId,
						Kind = kind.Name
					});
			}

			viewModel.Dictionaries.Add("ServiceType", serviceTypesVM);

			if (selectedAccountingId.HasValue)
				ViewBag.SelectedAccountingId = selectedAccountingId;

			return View(viewModel);
		}

		public ActionResult Create()
		{
			var model = new CreateOrderViewModel();
			model.Dictionaries.Add("Product", dataLogic.GetProducts());
			model.Dictionaries.Add("Contractor", dataLogic.GetContractors());
			model.Dictionaries.Add("FinRepCenter", dataLogic.GetFinRepCenters());
			model.Dictionaries.Add("OrderTemplate", dataLogic.GetOrderTemplates());
			return View(model);
		}

		public ContentResult CreateOrder(int productId, int orderTemplateId, int contractId, int finRepCenterId)
		{
			#region проверить проверенность договора
			var contractMarks = contractLogic.GetContractMarkByContract(contractId);
			if ((contractMarks == null) || (!contractMarks.IsContractChecked))
				return Content(JsonConvert.SerializeObject(new { Message = "Договор не проверен. Обратитесь к администратору" }));
			#endregion

			var order = new Order
			{
				ProductId = productId,
				ContractId = contractId,
				FinRepCenterId = finRepCenterId,
				OrderTemplateId = orderTemplateId,
				OrderStatusId = 2,  // created
				OrderTypeId = 1,    // стандарт
				CreatedBy = CurrentUserId,
				CreatedDate = DateTime.Now,
				RequestDate = DateTime.Now
			};

			if (productId.In(1, 2, 3, 8))
				order.VolumetricRatioId = 1;

			if (productId.In(4, 5, 6, 7, 9))
				order.VolumetricRatioId = 2;

			var id = orderLogic.CreateOrder(order);
			order = orderLogic.GetOrder(id);
			var frc = dataLogic.GetFinRepCenter(order.FinRepCenterId.Value);
			order.Number = frc.Code + id.ToString("D7");
			order.RequestNumber = order.Number;

			orderLogic.UpdateOrder(order);

			var contract = contractLogic.GetContract(contractId);
			var legal = legalLogic.GetLegal(contract.LegalId);
			// инициализация участников
			var wg = FilterByDates(participantLogic.GetWorkgroupByContractor(legal.ContractorId.Value));
			var responsibleId = GetParticipantUsers(wg.Where(w => w.IsResponsible), ParticipantRoles.AM).FirstOrDefault();
			var deputy = wg.FirstOrDefault(w => w.IsDeputy);
			if ((deputy != null) && (deputy.FromDate < DateTime.Now) && (deputy.ToDate > DateTime.Now))
				responsibleId = deputy.UserId.Value;

			foreach (var item in wg)
				if (!item.ToDate.HasValue || item.ToDate.Value > DateTime.Now)
					participantLogic.CreateParticipant(new Participant
					{
						UserId = item.UserId,
						OrderId = id,
						ParticipantRoleId = item.ParticipantRoleId,
						FromDate = item.FromDate,
						ToDate = item.ToDate,
						IsDeputy = item.IsDeputy,
						IsResponsible = (item.UserId == responsibleId) && (item.ParticipantRoleId == (int)ParticipantRoles.AM)
					});

			// применение шаблона заказа
			var orderOperations = dataLogic.GetOrderOperationsByTemplate(orderTemplateId);
			int index = 0;
			foreach (var item in orderOperations)
				orderLogic.CreateOperation(new Operation { OrderOperationId = item.ID, Name = item.Name, OrderId = id, No = index++, ResponsibleUserId = CurrentUserId });

			// история статусов
			orderLogic.CreateOrderStatusHistory(new OrderStatusHistory { OrderId = id, Date = DateTime.Now, OrderStatusId = order.OrderStatusId, Reason = "", UserId = CurrentUserId });

			try
			{
				//создание директорий при сохранении заявки
				if (IO.Directory.Exists("\\\\corpserv03.corp.local/Common/5 Перевозки"))
				{
					if (!IO.Directory.Exists("\\\\corpserv03.corp.local/Common/5 Перевозки/" + order.Number))
					{
						IO.Directory.CreateDirectory("\\\\corpserv03.corp.local/Common/5 Перевозки/" + order.Number);
						IO.Directory.CreateDirectory("\\\\corpserv03.corp.local/Common/5 Перевозки/" + order.Number + "/Docs");
						IO.Directory.CreateDirectory("\\\\corpserv03.corp.local/Common/5 Перевозки/" + order.Number + "/Customer");
						IO.Directory.CreateDirectory("\\\\corpserv03.corp.local/Common/5 Перевозки/" + order.Number + "/Supplier");
					}
				}
			}
			catch (Exception ex)
			{ }

			return Content(JsonConvert.SerializeObject(new { ID = id }));
		}

		public ActionResult RouteContact(int? id, int? legalId, int? placeId)
		{
			RouteContact contact;
			if (id.HasValue)
				contact = legalLogic.GetRouteContact(id.Value);
			else
				contact = new RouteContact { LegalId = legalId, PlaceId = placeId };

			var legal = legalLogic.GetLegal(contact.LegalId.Value);
			var result = new RouteContactViewModel
			{
				Contact = contact.Contact,
				Email = contact.Email,
				EnContact = contact.EnContact,
				ID = contact.ID,
				LegalId = contact.LegalId,
				Name = contact.Name,
				Phones = contact.Phones,
				PlaceId = contact.PlaceId,
				Address = contact.Address,
				EnAddress = contact.EnAddress,

				LegalName = (legal != null) ? legal.DisplayName : ""
			};

			return View(result);
		}

		public ActionResult CreateShortLegal(int contractorId)
		{
			var legal = new Legal { ContractorId = contractorId, WorkTime = "09:00-18:00" };
			ViewBag.Dictionaries = new Dictionary<string, object>();
			ViewBag.Dictionaries.Add("Employee", dataLogic.GetEmployees());
			ViewBag.Dictionaries.Add("TaxType", dataLogic.GetTaxTypes());
			ViewBag.Dictionaries.Add("TimeZone", TimeZoneInfo.GetSystemTimeZones().Where(w => !w.Id.StartsWith("UTC")).Select(s => new { ID = s.Id, Display = s.DisplayName }));

			return View(legal);
		}

		public ActionResult LegalDetails(int id)
		{
			var contractorId = legalLogic.GetLegal(id).ContractorId;
			return RedirectToAction("Details", "Contractors", new { id = contractorId, activePart = "Legals" });
		}

		public ActionResult AccountingDetails(int id)
		{
			var orderId = accountingLogic.GetAccounting(id).OrderId;
			return RedirectToAction("Details", "Orders", new { id = orderId, selectedAccountingId = id });
		}

		public ActionResult ChangeProduct(int orderId)
		{
			var viewModel = new OrderViewModel { Order = orderLogic.GetOrder(orderId) };

			var serviceKinds = dataLogic.GetServiceKinds();
			var serviceTypes = dataLogic.GetServiceTypes();
			var list = new List<ServiceTypeViewModel> { new ServiceTypeViewModel { ID = 0, Name = "" } };
			foreach (var item in serviceTypes)
			{
				var kind = serviceKinds.FirstOrDefault(w => w.ID == item.ServiceKindId) ?? new ServiceKind();
				list.Add(new ServiceTypeViewModel
				{
					ID = item.ID,
					Count = item.Count,
					MeasureId = item.MeasureId,
					Name = item.Name,
					Price = item.Price,
					ServiceKindId = item.ServiceKindId,
					VatId = kind.VatId,
					Kind = kind.Name,
					ProductId = kind.ProductId
				});
			}

			viewModel.Dictionaries.Add("ServiceType", list);
			viewModel.Dictionaries.Add("Product", dataLogic.GetProducts());
			viewModel.Dictionaries.Add("Measure", dataLogic.GetMeasures());

			return View(viewModel);
		}

		public ActionResult ViewTemplatedDocument(int id)
		{
			var doc = documentLogic.GetTemplatedDocument(id);
#if !DEBUG
			var orderId = doc.OrderId ?? 0;
			if (doc.AccountingId.HasValue)
				orderId = accountingLogic.GetAccounting(doc.AccountingId.Value).OrderId;

			if (orderId > 0)
				if (!participantLogic.GetWorkgroupByOrder(orderId).Any(w => w.UserId == CurrentUserId))
					return RedirectToAction("NotAuthorized", "Home");
#endif
			var pdfData = documentLogic.GetTemplatedDocumentData(id, "pdf");
			var cutPdfData = documentLogic.GetTemplatedDocumentData(id, "cutpdf");
			var cleanPdfData = documentLogic.GetTemplatedDocumentData(id, "cleanpdf");
			Accounting accounting = null;
			if (doc.AccountingId.HasValue)
				accounting = accountingLogic.GetAccounting(doc.AccountingId.Value);

			var viewModel = new TemplatedDocumentViewModel
			{
				ID = doc.ID,
				TemplateId = doc.TemplateId,
				CreatedBy = doc.CreatedBy,
				CreatedDate = doc.CreatedDate,
				ChangedBy = doc.ChangedBy,
				ChangedDate = doc.ChangedDate,
				Date = GetTemplatedDocumentDate(id),
				Filename = doc.Filename,
				AccountingId = doc.AccountingId,
				HasPdf = pdfData != null,
				HasCleanPdf = cleanPdfData != null,
				HasCutPdf = cutPdfData != null,
				AccountingNumber = (accounting != null) ? accounting.Number : ""
			};

			viewModel.Dictionaries.Add("Template", dataLogic.GetTemplates());
			viewModel.Dictionaries.Add("User", dataLogic.GetUsers());

			return View(viewModel);
		}

		public ActionResult ViewDocument(int id)
		{
			var doc = documentLogic.GetDocument(id);
#if !DEBUG
			var orderId = doc.OrderId ?? 0;
			if (doc.AccountingId.HasValue)
				orderId = accountingLogic.GetAccounting(doc.AccountingId.Value).OrderId;

			if (orderId > 0)
				if (!participantLogic.GetWorkgroupByOrder(orderId).Any(w => w.UserId == CurrentUserId))
					return RedirectToAction("NotAuthorized", "Home");
#endif
			var viewModel = new DocumentViewModel
			{
				ID = doc.ID,
				UploadedBy = doc.UploadedBy,
				UploadedDate = doc.UploadedDate,
				Date = doc.Date,
				DocumentTypeId = doc.DocumentTypeId,
				Filename = doc.Filename,
				FileSize = doc.FileSize,
				IsPrint = doc.IsPrint,
				IsNipVisible = doc.IsNipVisible,
				OriginalSentDate = doc.OriginalSentDate,
				OriginalReceivedDate = doc.OriginalReceivedDate,

				Number = doc.Number,
				AccountingId = doc.AccountingId
			};

			viewModel.Dictionaries.Add("DocumentType", dataLogic.GetDocumentTypes());
			viewModel.Dictionaries.Add("User", dataLogic.GetUsers());

			return View(viewModel);
		}

		public ActionResult ViewPayment(int id)
		{
			var model = accountingLogic.GetPaymentInfo(id);
			return View(model);
		}

		#endregion

		#region get lists

		public ContentResult GetItems(ListFilter filter)
		{
			userLogic.SetSetting(CurrentUserId, "Orders.Sort", filter.Sort);
			userLogic.SetSetting(CurrentUserId, "Orders.SortDirection", filter.SortDirection);
			userLogic.SetSetting(CurrentUserId, "Orders.PageNumber", filter.PageNumber.ToString());

			var totalCount = orderLogic.GetOrdersCount(filter);
			var list = orderLogic.GetOrders(filter).Select(s => new
			{
				s.Balance,
				s.CreatedDate,
				s.ClosedDate,
				s.ContractId,
				s.From,
				s.ID,
				s.GrossWeight,
				s.Number,
				s.OrderStatusId,
				s.ProductId,
				s.To
			}).ToList();

			return Content(JsonConvert.SerializeObject(new { Items = list, TotalCount = totalCount }));
		}

		public ContentResult GetCargoSeats(int orderId)
		{
			var list = orderLogic.GetCargoSeats(orderId);
			return Content(JsonConvert.SerializeObject(new { Items = list }));
		}

		public ContentResult GetOperations(int orderId)
		{
			var list = orderLogic.GetOperationsByOrder(orderId).OrderBy(o => o.No);
			return Content(JsonConvert.SerializeObject(new { Items = list }));
		}

		public ContentResult GetOrderEvents(int orderId)
		{
			var list = orderLogic.GetOrderEvents(orderId);
			return Content(JsonConvert.SerializeObject(new { Items = list }));
		}

		public ContentResult GetRoutePoints(int orderId)
		{
			var places = dataLogic.GetPlaces(new PlaceListFilter());
			var list = orderLogic.GetRoutePoints(orderId).Select(s => new RoutePointViewModel
			{
				ID = s.ID,
				OrderId = s.OrderId,
				No = s.No,
				RoutePointTypeId = s.RoutePointTypeId,
				PlaceId = s.PlaceId,
				PlanDate = s.PlanDate,
				FactDate = s.FactDate,
				Address = s.Address,
				EnAddress = s.EnAddress,
				Contact = s.Contact,
				EnContact = s.EnContact,
				ParticipantLegalId = s.ParticipantLegalId,
				ParticipantComment = s.ParticipantComment,
				RouteContactID = s.RouteContactID,
				EnParticipantComment = s.EnParticipantComment,

				Place = places.Where(w => w.ID == s.PlaceId).Select(ps => ps.Name).FirstOrDefault() ?? ""
			}).ToList();

			return Content(JsonConvert.SerializeObject(new { Items = list }));
		}

		public ContentResult GetRouteSegments(int orderId)
		{
			var list = orderLogic.GetRouteSegments(orderId);
			return Content(JsonConvert.SerializeObject(new { Items = list }));
		}

		public ContentResult GetAccountings(int orderId)
		{
			var contracts = contractLogic.GetContracts(new ListFilter());
			var legals = legalLogic.GetLegals(new ListFilter());
			var orders = orderLogic.GetOrders(new ListFilter());
			var list = accountingLogic.GetAccountingsByOrder(orderId);
			var resultList = list.Select(s => new AccountingViewModel
			{
				ID = s.ID,
				AccountingDate = s.AccountingDate,
				RequestDate = s.RequestDate,
				IsIncome = s.IsIncome,
				AccountingDocumentTypeId = s.AccountingDocumentTypeId,
				AccountingPaymentMethodId = s.AccountingPaymentMethodId,
				PayMethodId = s.PayMethodId,
				AccountingPaymentTypeId = s.AccountingPaymentTypeId,
				ActDate = s.ActDate,
				ActNumber = s.ActNumber,
				Comment = s.Comment,
				ContractId = s.ContractId,
				CreatedDate = s.CreatedDate,
				InvoiceDate = s.InvoiceDate,
				InvoiceNumber = s.InvoiceNumber,
				VatInvoiceNumber = s.VatInvoiceNumber,
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
				RejectHistory = s.RejectHistory,
				OurLegalId = s.OurLegalId ?? contracts.Where(w => w.ID == s.ContractId).Select(con => con.OurLegalId).FirstOrDefault(),
				LegalId = s.LegalId ?? contracts.Where(w => w.ID == s.ContractId).Select(con => con.LegalId).FirstOrDefault(),
				ContractorId = legals.Where(w => w.ID == contracts.Where(w2 => w2.ID == s.ContractId).Select(con => con.LegalId).FirstOrDefault()).Select(con => con.ContractorId ?? 0).FirstOrDefault(),
				ContractNumber = contracts.Where(w => w.ID == s.ContractId).Select(con => con.Number).FirstOrDefault(),
				AccountingCurrencyId = accountingLogic.GetServicesByAccounting(s.ID).Select(con => con.CurrencyId).FirstOrDefault(),
				OrderNumber = orders.Where(w => w.ID == s.OrderId).Select(con => con.Number).FirstOrDefault()
			});

			return Content(JsonConvert.SerializeObject(new { Items = resultList }));
		}

		public ContentResult GetAccountingsByContractor(int contractorId)
		{
			var legals = legalLogic.GetLegals(new ListFilter());
			var contracts = contractLogic.GetContracts(new ListFilter());
			var orders = orderLogic.GetOrders(new ListFilter());
			var list = accountingLogic.GetAccountingsByContractor(contractorId).ToList();
			var resultList = list.Select(s => new AccountingViewModel
			{
				ID = s.ID,
				AccountingDate = s.AccountingDate,
				RequestDate = s.RequestDate,
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
				VatInvoiceNumber = s.VatInvoiceNumber,
				//No = s.No,
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
				RejectHistory = s.RejectHistory,
				OurLegalId = s.OurLegalId ?? contracts.Where(w => w.ID == s.ContractId).Select(con => con.OurLegalId).FirstOrDefault(),
				LegalId = s.LegalId ?? contracts.Where(w => w.ID == s.ContractId).Select(con => con.LegalId).FirstOrDefault(),
				ContractorId = legals.Where(w => w.ID == contracts.Where(w2 => w2.ID == s.ContractId).Select(con => con.LegalId).FirstOrDefault()).Select(con => con.ContractorId ?? 0).FirstOrDefault(),
				ContractNumber = contracts.Where(w => w.ID == s.ContractId).Select(con => con.Number).FirstOrDefault(),
				OrderNumber = orders.Where(w => w.ID == s.OrderId).Select(con => con.Number).FirstOrDefault()
			});

			return Content(JsonConvert.SerializeObject(new { Items = resultList }));
		}

		public ContentResult GetServices(int orderId)
		{
			var accountings = accountingLogic.GetAccountingsByOrder(orderId);
			var list = new List<Service>();
			foreach (var item in accountings)
				list.AddRange(accountingLogic.GetServicesByAccounting(item.ID).ToList());

			return Content(JsonConvert.SerializeObject(new { Items = list }));
		}

		public ContentResult GetServicesByAccounting(int accountingId)
		{
			var list = accountingLogic.GetServicesByAccounting(accountingId);
			return Content(JsonConvert.SerializeObject(new { Items = list }));
		}

		public ContentResult GetDocuments(int orderId)
		{
			var accountings = accountingLogic.GetAccountingsByOrder(orderId);
			var list = documentLogic.GetDocumentsByOrder(orderId);
			var result = list.Select(s => new DocumentViewModel
			{
				ID = s.ID,
				Number = s.Number,
				Date = s.Date,
				UploadedBy = s.UploadedBy,
				UploadedDate = s.UploadedDate,
				DocumentTypeId = s.DocumentTypeId,
				Filename = s.Filename,
				FileSize = s.FileSize,
				IsPrint = s.IsPrint,
				IsNipVisible = s.IsNipVisible,
				AccountingId = s.AccountingId,
				OriginalSentDate = s.OriginalSentDate,
				OriginalReceivedDate = s.OriginalReceivedDate,
				ContractId = accountings.Where(w => w.ID == s.AccountingId).Select(sl => sl.ContractId ?? 0).FirstOrDefault(),
				OrderAccountingName = accountings.Where(w => w.ID == s.AccountingId).Select(sl => sl.Number).FirstOrDefault()
			});

			return Content(JsonConvert.SerializeObject(new { Items = result }));
		}

		public ContentResult GetDocumentsByContract(int contractId)
		{
			var list = documentLogic.GetDocumentsByContract(contractId);
			var result = list.Select(s => new DocumentViewModel
			{
				ID = s.ID,
				Number = s.Number,
				Date = s.Date,
				UploadedBy = s.UploadedBy,
				UploadedDate = s.UploadedDate,
				DocumentTypeId = s.DocumentTypeId,
				Filename = s.Filename,
				FileSize = s.FileSize,
				IsPrint = s.IsPrint,
				IsNipVisible = s.IsNipVisible,
				AccountingId = s.AccountingId,
				OriginalSentDate = s.OriginalSentDate,
				OriginalReceivedDate = s.OriginalReceivedDate
			});

			return Content(JsonConvert.SerializeObject(new { Items = result }));
		}

		public ContentResult GetDocumentsByAccounting(int accountingId)
		{
			var list = documentLogic.GetDocumentsByAccounting(accountingId);
			return Content(JsonConvert.SerializeObject(new { Items = list }));
		}

		public ContentResult GetTemplatedDocuments(int orderId)
		{
			var list = documentLogic.GetTemplatedDocumentsByOrder(orderId);
			return Content(JsonConvert.SerializeObject(new { Items = list }));
		}

		public ContentResult GetJointDocuments(int orderId)
		{
			var order = orderLogic.GetOrder(orderId);
			var accountings = accountingLogic.GetAccountingsByOrder(orderId);
			var contracts = contractLogic.GetContracts(new ListFilter());

			var result = new List<JointDocumentModel>();
			// добавить прикрепленные документы
			foreach (var s in documentLogic.GetDocumentsByOrder(orderId))
			{
				var jointDocument = new JointDocumentModel
				{
					IsDocument = true,
					ID = s.ID,
					Date = s.Date,
					Number = s.Number,
					UploadedDate = s.UploadedDate,
					UploadedBy = s.UploadedBy,
					DocumentTypeId = s.DocumentTypeId,
					IsPrint = s.IsPrint,
					IsNipVisible = s.IsNipVisible,
					OriginalSentDate = s.OriginalSentDate,
					OriginalSentBy = s.OriginalSentUserId,
					OriginalReceivedDate = s.OriginalReceivedDate,
					OriginalReceivedBy = s.OriginalReceivedUserId,
					AccountingId = s.AccountingId,
					ReceivedBy = s.ReceivedBy,
					ReceivedNumber = s.ReceivedNumber,
					Filename = s.Filename,
					FileSize = s.FileSize
				};

				if (s.AccountingId.HasValue)
				{
					var accounting = accountings.First(w => w.ID == s.AccountingId);
					jointDocument.OrderAccountingName = accounting.Number;
					if (accounting.IsIncome)
						jointDocument.LegalId = accounting.LegalId.Value;
					else
						jointDocument.LegalId = accounting.ContractId.HasValue ? contracts.First(wc => wc.ID == accounting.ContractId.Value).LegalId : 0;
				}
				else if (s.OrderId.HasValue)
				{
					jointDocument.LegalId = contracts.First(wc => wc.ID == order.ContractId.Value).LegalId;
				}

				result.Add(jointDocument);
			}

			// добавить сформированные документы
			foreach (var s in documentLogic.GetAllTemplatedDocumentsByOrder(orderId))
			{
				var jointDocument = new JointDocumentModel
				{
					IsDocument = false,
					ID = s.ID,
					TemplateId = s.TemplateId,
					UploadedDate = s.ChangedDate ?? s.CreatedDate,
					UploadedBy = s.ChangedBy ?? s.CreatedBy,
					OriginalSentDate = s.OriginalSentDate,
					OriginalSentBy = s.OriginalSentUserId,
					OriginalReceivedDate = s.OriginalReceivedDate,
					OriginalReceivedBy = s.OriginalReceivedUserId,
					AccountingId = s.AccountingId,
					ReceivedBy = s.ReceivedBy,
					ReceivedNumber = s.ReceivedNumber
				};

				if (s.AccountingId.HasValue)
				{
					var accounting = accountings.First(w => w.ID == s.AccountingId);
					jointDocument.OrderAccountingName = accounting.Number;
					jointDocument.Number = accounting.Number;
					if (accounting.IsIncome)
					{
						jointDocument.IsNipVisible = true;
						jointDocument.LegalId = accounting.LegalId ?? -1;   // TODO:
						jointDocument.Date = accounting.ActDate ?? accounting.RequestDate;
					}
					else
					{
						jointDocument.Date = accounting.InvoiceDate ?? accounting.RequestDate;
						if (accounting.ContractId.HasValue)
							jointDocument.LegalId = contracts.First(wc => wc.ID == accounting.ContractId.Value).LegalId;
					}
				}
				else if (s.OrderId.HasValue)
				{
					jointDocument.LegalId = contracts.First(wc => wc.ID == order.ContractId.Value).LegalId;
					jointDocument.Number = order.Number;
					jointDocument.Date = order.RequestDate;
				}

				result.Add(jointDocument);
			}

			return Content(JsonConvert.SerializeObject(new { Items = result }));
		}

		public ContentResult GetJointDocumentsByAccounting(int accountingId)
		{
			var accounting = accountingLogic.GetAccounting(accountingId);
			var order = orderLogic.GetOrder(accounting.OrderId);
			var accountings = accountingLogic.GetAccountingsByOrder(accounting.OrderId);
			var contracts = contractLogic.GetContracts(new ListFilter());

			var result = new List<JointDocumentModel>();
			// добавить прикрепленные документы
			foreach (var s in documentLogic.GetDocumentsByAccounting(accountingId))
			{
				var jointDocument = new JointDocumentModel
				{
					IsDocument = true,
					ID = s.ID,
					Date = s.Date,
					Number = s.Number,
					UploadedDate = s.UploadedDate,
					UploadedBy = s.UploadedBy,
					DocumentTypeId = s.DocumentTypeId,
					IsPrint = s.IsPrint,
					IsNipVisible = s.IsNipVisible,
					OriginalSentDate = s.OriginalSentDate,
					OriginalSentBy = s.OriginalSentUserId,
					OriginalReceivedDate = s.OriginalReceivedDate,
					OriginalReceivedBy = s.OriginalReceivedUserId,
					AccountingId = s.AccountingId,
					ReceivedBy = s.ReceivedBy,
					ReceivedNumber = s.ReceivedNumber,
					Filename = s.Filename,
					FileSize = s.FileSize
				};

				if (s.AccountingId.HasValue)
				{
					accounting = accountings.First(w => w.ID == s.AccountingId);
					jointDocument.OrderAccountingName = accounting.Number;
					if (accounting.IsIncome)
						jointDocument.LegalId = accounting.LegalId.Value;
					else
						jointDocument.LegalId = contracts.Where(wc => wc.ID == accounting.ContractId).Select(sj => sj.LegalId).FirstOrDefault();
				}
				else if (s.OrderId.HasValue)
				{
					jointDocument.LegalId = contracts.Where(wc => wc.ID == order.ContractId.Value).Select(sj => sj.LegalId).FirstOrDefault();
				}

				result.Add(jointDocument);
			}

			// добавить сформированные документы
			foreach (var s in documentLogic.GetTemplatedDocumentsByAccounting(accountingId))
			{
				var jointDocument = new JointDocumentModel
				{
					IsDocument = false,
					ID = s.ID,
					TemplateId = s.TemplateId,
					UploadedDate = s.ChangedDate ?? s.CreatedDate,
					UploadedBy = s.ChangedBy ?? s.CreatedBy,
					OriginalSentDate = s.OriginalSentDate,
					OriginalSentBy = s.OriginalSentUserId,
					OriginalReceivedDate = s.OriginalReceivedDate,
					OriginalReceivedBy = s.OriginalReceivedUserId,
					AccountingId = s.AccountingId,
					ReceivedBy = s.ReceivedBy,
					ReceivedNumber = s.ReceivedNumber
				};

				if (s.AccountingId.HasValue)
				{
					accounting = accountings.First(w => w.ID == s.AccountingId);
					jointDocument.OrderAccountingName = accounting.Number;
					jointDocument.Number = accounting.Number;
					if (accounting.IsIncome)
					{
						jointDocument.IsNipVisible = true;
						jointDocument.LegalId = accounting.LegalId ?? -1;   // TODO:
						jointDocument.Date = accounting.ActDate ?? accounting.RequestDate;
					}
					else
					{
						jointDocument.Date = accounting.InvoiceDate ?? accounting.RequestDate;
						if (accounting.ContractId.HasValue)
							jointDocument.LegalId = contracts.First(wc => wc.ID == accounting.ContractId.Value).LegalId;
					}
				}
				else if (s.OrderId.HasValue)
				{
					jointDocument.LegalId = contracts.First(wc => wc.ID == order.ContractId.Value).LegalId;
					jointDocument.Number = order.Number;
					jointDocument.Date = order.RequestDate;
				}

				result.Add(jointDocument);
			}

			return Content(JsonConvert.SerializeObject(new { Items = result }));
		}

		public ContentResult GetOrderStatusHistory(int orderId)
		{
			var list = orderLogic.GetOrderStatusHistory(orderId);
			return Content(JsonConvert.SerializeObject(new { Items = list }));
		}

		public ContentResult GetEmployeesByLegal(int legalId)
		{
			var persons = dataLogic.GetPersons();
			var list = employeeLogic.GetEmployeesByLegal(legalId);
			var resultList = list.Select(item => new EmployeeViewModel
			{
				ID = item.ID,
				Position = item.Position,
				Department = item.Department,
				Name = persons.First(w => w.ID == item.PersonId).Display
			});

			return Content(JsonConvert.SerializeObject(new { Items = resultList }));
		}

		public ContentResult GetBankAccountsByLegal(int legalId)
		{
			var banks = bankLogic.GetBanks(new ListFilter());
			var list = bankLogic.GetBankAccountsByLegal(legalId);
			var resultList = list.Select(s => new BankAccoundViewModel
			{
				ID = s.ID,
				Number = s.Number,
				BankId = s.BankId,
				LegalId = s.LegalId,
				CurrencyId = s.CurrencyId,
				CoBankIBAN = s.CoBankIBAN,
				CoBankAddress = s.CoBankAddress,
				CoBankSWIFT = s.CoBankSWIFT,
				CoBankAccount = s.CoBankAccount,
				BankName = banks.Where(w => w.ID == s.BankId).Select(o => o.Name).FirstOrDefault(),
				BIC = banks.Where(w => w.ID == s.BankId).Select(o => o.BIC).FirstOrDefault()
			});

			return Content(JsonConvert.SerializeObject(new { Items = resultList }));
		}

		public ContentResult GetPaymentsByAccounting(int accountingId)
		{
			var list = accountingLogic.GetPayments(accountingId).Select(s => new PaymentViewModel
			{
				AccountingId = s.AccountingId,
				BankAccount = s.BankAccount,
				CurrencyId = s.CurrencyId,
				Date = s.Date,
				Description = s.Description,
				ID = s.ID,
				IsIncome = s.IsIncome,
				Number = s.Number,
				OriginalSum = s.Sum,
				Sum = s.Sum,
				CurrencyRateCB = (float)0,
				CurrencyRate = (float)0,
			}).ToList();

			var accounting = accountingLogic.GetAccounting(accountingId);
			var order = orderLogic.GetOrder(accounting.OrderId);
			var contract = contractLogic.GetContract(accounting.IsIncome ? order.ContractId.Value : (accounting.ContractId ?? 0));
			var service = accountingLogic.GetServicesByAccounting(accountingId).FirstOrDefault();
			foreach (var payment in list)
			{
				if (contract == null)
					continue;   // нет договора

				if (service == null)
					continue;   // нет услуги

				if (service.CurrencyId == payment.CurrencyId)
					continue;   // не нужна конверсия

				// конверсия в рубли
				if (service.CurrencyId == 1)
				{
					var contractCurrencies = contractLogic.GetContractCurrencies(contract.ID);
					if (contractCurrencies.Any(w => w.CurrencyId == payment.CurrencyId))
						continue;

					// перевести в рубли
					var ruse = dataLogic.GetCurrencyRateUses().FirstOrDefault(w => w.ID == contract.CurrencyRateUseId);
					float rateUse = ruse?.Value ?? 1;
					var date = payment.Date;
					var rate = dataLogic.GetCurrencyRates(date).First(w => w.CurrencyId == payment.CurrencyId).Rate;
					payment.CurrencyRateCB = rate ?? 1;

					if ((ruse != null) && (ruse.IsDocumentDate) && (accounting.InvoiceDate.HasValue))
						rate = dataLogic.GetCurrencyRates(accounting.InvoiceDate.Value).First(w => w.CurrencyId == payment.CurrencyId).Rate;

					payment.CurrencyRate = (rate ?? 1) * rateUse;
					payment.Sum = payment.Sum * payment.CurrencyRate;
				}
				else if (payment.CurrencyId == 1)    // конверсия из рублей
				{
					// перевести в валюту
					var ruse = dataLogic.GetCurrencyRateUses().FirstOrDefault(w => w.ID == contract.CurrencyRateUseId);
					float rateUse = ruse?.Value ?? 1;
					var date = payment.Date;
					var rate = dataLogic.GetCurrencyRates(date).First(w => w.CurrencyId == service.CurrencyId).Rate;
					payment.CurrencyRateCB = rate ?? 1;

					if ((ruse != null) && (ruse.IsDocumentDate) && (accounting.InvoiceDate.HasValue))
						rate = dataLogic.GetCurrencyRates(accounting.InvoiceDate.Value).First(w => w.CurrencyId == service.CurrencyId).Rate;

					payment.CurrencyRate = (rate ?? 1) * rateUse;
					payment.Sum = payment.Sum * (1 / payment.CurrencyRate);
				}
				else
					Debug.WriteLine("Перевод валют не поддерживается");
			}

			return Content(JsonConvert.SerializeObject(new { Items = list }));
		}

		public ContentResult GetOrderAccountingRouteSegments(int accountingId)
		{
			var list = accountingLogic.GetAccountingRouteSegments(accountingId);
			return Content(JsonConvert.SerializeObject(new { Items = list }));
		}

		public ContentResult GetLegalsByContractor(int contractorId)
		{
			var list = legalLogic.GetLegals(new ListFilter()).Where(w => w.ContractorId == contractorId);
			return Content(JsonConvert.SerializeObject(new { Items = list }));
		}

		public ContentResult GetContractsByContractor(int contractorId)
		{
			var legals = dataLogic.GetLegals();
			var olegals = dataLogic.GetOurLegalLegals();
			var types = dataLogic.GetContractTypes();
			var list = contractLogic.GetContractsByContractor(contractorId);
			var result = list.Select(s => new ContractInfoModel
			{
				ID = s.ID,
				Number = s.Number,
				Date = s.Date,
				OurLegalId = s.OurLegalId,
				ContractTypeId = s.ContractTypeId ?? 1, // TODO:
				ContractServiceTypeId = s.ContractServiceTypeId ?? 1,   // TODO:
				CurrencyRateUseId = s.CurrencyRateUseId ?? 2,
				LegalId = s.LegalId,
				PayMethodId = s.PayMethodId,
				Legal = legals.Where(w => w.ID == s.LegalId).Select(con => con.Display).FirstOrDefault(),
				OurLegal = olegals.Where(w => w.ID == s.OurLegalId).Select(con => con.Display).FirstOrDefault(),
				Type = types.Where(w => w.ID == s.ContractTypeId).Select(con => con.Display).FirstOrDefault(),
				IsActive = s.EndDate.HasValue ? (s.EndDate < DateTime.Now ? (s.IsProlongation ? true : false) : true) : (true),
				Currencies = contractLogic.GetContractCurrencies(s.ID)
			}).ToList();

			foreach (var item in result)
			{
				var marks = contractLogic.GetContractMarkByContract(item.ID);
				if ((marks != null) && (marks.IsContractRejected || marks.IsContractBlocked))
					item.IsActive = false;
			}

			return Content(JsonConvert.SerializeObject(new { Items = result }));
		}

		public ContentResult GetOrderTemplatesByContract(int contractId)
		{
			var list = dataLogic.GetOrderTemplatesByContract(contractId);
			return Content(JsonConvert.SerializeObject(new { Items = list }));
		}

		public ContentResult GetMatchings(int orderId)
		{
			var matchings = new List<MatchingViewModel>();
			// сопоставление
			var accountings = accountingLogic.GetAccountingsByOrder(orderId);
			foreach (var acc in accountings.Where(w => !w.IsIncome).ToList())
			{
				var matches = accountingLogic.GetAccountingMatchingsByAccounting(acc.ID);
				if (matches.Count() == 0)
					matchings.Add(new MatchingViewModel { ExpenseAccountingId = acc.ID, Sum = acc.Sum ?? 0, Percent = 0 });
				else
				{
				}
			}

			return Content(JsonConvert.SerializeObject(new { Items = matchings }));
		}

		#endregion

		#region new entities

		public ContentResult GetNewCargoSeat(int orderId)
		{
			// проверка прав участника
			if (!participantLogic.IsAllowedActionByOrder(13, orderId, CurrentUserId))
				return Content(JsonConvert.SerializeObject(new { ActionId = 13, Message = "У вас недостаточно прав на выполнение этого действия." }));

			var order = orderLogic.GetOrder(orderId);
			if (order.OrderStatusId == 2)
				return Content(JsonConvert.SerializeObject(new { ActionId = 13, Message = "Это нельзя делать с заказом в таком статусе." }));

			// подстановка значений по-умолчанию
			var s = new CargoSeat { OrderId = orderId };
			return Content(JsonConvert.SerializeObject(s));
		}

		public ContentResult GetNewRoutePoint(int orderId)
		{
			// проверка прав участника
			if (!participantLogic.IsAllowedActionByOrder(15, orderId, CurrentUserId))
				return Content(JsonConvert.SerializeObject(new { ActionId = 15, Message = "У вас недостаточно прав на выполнение этого действия." }));

			var order = orderLogic.GetOrder(orderId);
			if (order.OrderStatusId == 2)
				return Content(JsonConvert.SerializeObject(new { ActionId = 15, Message = "Это нельзя делать с заказом в таком статусе." }));

			// подстановка значений по-умолчанию
			var p = new RoutePointViewModel { OrderId = orderId, PlanDate = DateTime.Now };
			return Content(JsonConvert.SerializeObject(p));
		}

		public ContentResult GetNewService(int accountingId)
		{
			var accounting = accountingLogic.GetAccounting(accountingId);

			// проверка прав участника
			if (!participantLogic.IsAllowedActionByOrder(17, accounting.OrderId, CurrentUserId))
				return Content(JsonConvert.SerializeObject(new { ActionId = 17, Message = "У вас недостаточно прав на выполнение этого действия." }));

			#region проверка статуса контрагента

			if (!accounting.IsIncome && !accounting.ContractId.HasValue)
				return Content(JsonConvert.SerializeObject(new { Message = "Договор не выбран." }));

			var contractor = contractorLogic.GetContractorByAccounting(accountingId);
			if (!contractor.IsLocked)
				return Content(JsonConvert.SerializeObject(new { ActionId = 17, Message = "Контрагент не зафиксирован." }));

			#endregion

			#region проверка меток и прочего

			var workgroup = participantLogic.GetWorkgroupByOrder(accounting.OrderId).Where(w => w.UserId == CurrentUserId);
			var mark = accountingLogic.GetAccountingMarkByAccounting(accountingId);
			if (mark != null && (mark.IsInvoiceChecked || mark.IsAccountingChecked) && !IsAllowed(workgroup, ParticipantRoles.BUH, ParticipantRoles.GM))
				return Content(JsonConvert.SerializeObject(new { ActionId = 17, Message = "Нельзя менять состав услуг после проставления метки 'Счет проверен' или 'Расход проверен'" }));

			#endregion

			// подстановка значений по-умолчанию
			var s = new Service { AccountingId = accountingId };
			return Content(JsonConvert.SerializeObject(s));
		}

		public ContentResult GetNewDocument(int? accountingId, int? orderId, int? contractId)
		{
			if (accountingId.HasValue)
			{
				var accounting = accountingLogic.GetAccounting(accountingId.Value);

				#region проверка прав участника

				var workgroup = participantLogic.GetWorkgroupByOrder(accounting.OrderId).Where(w => w.UserId == CurrentUserId);
				if (workgroup.Count() == 0)
					return Content(JsonConvert.SerializeObject(new { Message = "Вы не являетесь участником этого заказа" }));

				if (!IsAllowed(workgroup, ParticipantRoles.AM, ParticipantRoles.OPER, ParticipantRoles.GM))
					return Content(JsonConvert.SerializeObject(new { Message = "У вас нет прав на выполнение этого действия" }));

				#endregion

				#region проверка меток и прочего

				var mark = accountingLogic.GetAccountingMarkByAccounting(accountingId.Value);
				if (mark != null && (mark.IsAccountingChecked) && !IsAllowed(workgroup, ParticipantRoles.BUH, ParticipantRoles.GM))
					return Content(JsonConvert.SerializeObject(new { Message = "Нельзя менять документы после проставления метки 'Расход проверен'" }));

				#endregion

				// подстановка значений по-умолчанию
				var d = new Document { AccountingId = accountingId, IsPrint = false, IsNipVisible = false, Date = DateTime.Now };

				var id = documentLogic.CreateDocument(d);

				d = documentLogic.GetDocument(id);
				return Content(JsonConvert.SerializeObject(d));
			}
			else if (orderId.HasValue)
			{
				#region проверка прав участника
				var workgroup = participantLogic.GetWorkgroupByOrder(orderId.Value).Where(w => w.UserId == CurrentUserId);
				if (workgroup.Count() == 0)
					return Content(JsonConvert.SerializeObject(new { Message = "Вы не являетесь участником этого заказа" }));

				if (!IsAllowed(workgroup, ParticipantRoles.AM, ParticipantRoles.OPER, ParticipantRoles.GM))
					return Content(JsonConvert.SerializeObject(new { Message = "У вас нет прав на выполнение этого действия" }));

				#endregion

				// подстановка значений по-умолчанию
				var d = new Document { OrderId = orderId, IsPrint = false, IsNipVisible = false, Date = DateTime.Now };

				var id = documentLogic.CreateDocument(d);
				d = documentLogic.GetDocument(id);
				return Content(JsonConvert.SerializeObject(d));
			}
			else
			{
				#region проверка прав участника

				var contract = contractLogic.GetContract(contractId.Value);
				var legal = legalLogic.GetLegal(contract.LegalId);

				var workgroup = participantLogic.GetWorkgroupByContractor(legal.ContractorId.Value).Where(w => w.UserId == CurrentUserId);
				if (workgroup.Count() == 0)
					return Content(JsonConvert.SerializeObject(new { Message = "Вы не являетесь участником этого контрагента" }));

				if (!IsAllowed(workgroup, ParticipantRoles.AM, ParticipantRoles.OPER, ParticipantRoles.GM))
					return Content(JsonConvert.SerializeObject(new { Message = "У вас нет прав на выполнение этого действия" }));

				#endregion

				// подстановка значений по-умолчанию
				var d = new Document { ContractId = contractId, IsPrint = false, IsNipVisible = false, Date = DateTime.Now };

				var id = documentLogic.CreateDocument(d);
				d = documentLogic.GetDocument(id);
				return Content(JsonConvert.SerializeObject(d));
			}
		}

		public ContentResult GetNewAccounting(int orderId, bool isIncome)
		{
			var order = orderLogic.GetOrder(orderId);

			// проверка прав участника
			if (!participantLogic.IsAllowedActionByOrder(19, orderId, CurrentUserId))
				return Content(JsonConvert.SerializeObject(new { ActionId = 19, Message = "У вас недостаточно прав на выполнение этого действия." }));

			#region проверка статуса заказа

			var workgroup = participantLogic.GetWorkgroupByOrder(orderId).Where(w => w.UserId == CurrentUserId);
			if (!IsAllowed(workgroup, ParticipantRoles.GM))
			{
				if (isIncome)
				{
					if (!order.OrderStatusId.In(2, 3, 4))
						return Content(JsonConvert.SerializeObject(new { ActionId = 19, Message = "Доход нельзя добавить в заказ с таким статусом." }));
				}
				else
				{
					if (!order.OrderStatusId.In(3, 4))
						return Content(JsonConvert.SerializeObject(new { ActionId = 19, Message = "Расход нельзя добавить в заказ с таким статусом." }));
				}
			}

			#endregion

			#region проверить проверенность договора

			var contractMarks = contractLogic.GetContractMarkByContract(order.ContractId.Value);
			if ((contractMarks == null) || (!contractMarks.IsContractChecked))
				return Content(JsonConvert.SerializeObject(new { Message = "Договор не проверен. Обратитесь к администратору" }));

			#endregion

			#region проверить договор

			var contract = contractLogic.GetContract(order.ContractId.Value);
			if (!(contract.EndDate.HasValue ? (contract.EndDate < DateTime.Now ? (contract.IsProlongation ? true : false) : true) : (true)))
				return Content(JsonConvert.SerializeObject(new { Message = "Договор просрочен. Обратитесь к администратору" }));

			#endregion

			#region проверка статуса контрагента

			var legal = legalLogic.GetLegal(contract.LegalId);
			var contractor = contractorLogic.GetContractor(legal.ContractorId.Value);
			if (!contractor.IsLocked)
				return Content(JsonConvert.SerializeObject(new { Message = "Контрагент не зафиксирован." }));

			#endregion

			var list = accountingLogic.GetAccountingsByOrder(orderId);
			var accountings = accountingLogic.GetAccountingsByOrder(orderId);

			// подстановка значений по-умолчанию
			var e = new Accounting
			{
				OrderId = orderId,
				IsIncome = isIncome,
				AccountingPaymentTypeId = isIncome ? 2 : 2,
				AccountingDocumentTypeId = isIncome ? 1 : 2,
				AccountingPaymentMethodId = 2,
				AccountingDate = DateTime.Now,
				RequestDate = DateTime.Now,
				CreatedDate = DateTime.Now,
				InvoiceDate = DateTime.Now,
				OurLegalId = isIncome ? (int?)contract.OurLegalId : null,
				LegalId = isIncome ? (int?)contract.LegalId : null
			};

			int dirNo = 0;
			foreach (var accounting in accountings)
				if ((accounting.IsIncome == isIncome) && (accounting.SameDirectionNo > dirNo))
					dirNo = accounting.SameDirectionNo;

			#region new number
			var number = order.FinRepCenterId + orderId.ToString("D7") + "-";
			if (isIncome)
			{
				e.Number = number + (dirNo + 101);
				e.InvoiceNumber = e.Number;
				e.ActNumber = e.Number;
			}
			else
				e.Number = number + (dirNo + 201);
			#endregion

			e.SameDirectionNo = ++dirNo;
			var id = accountingLogic.CreateAccounting(e);
			e = accountingLogic.GetAccounting(id);

			var result = new AccountingViewModel
			{
				ID = e.ID,
				AccountingDate = e.AccountingDate,
				RequestDate = e.RequestDate,
				IsIncome = e.IsIncome,
				AccountingDocumentTypeId = e.AccountingDocumentTypeId,
				AccountingPaymentMethodId = e.AccountingPaymentMethodId,
				AccountingPaymentTypeId = e.AccountingPaymentTypeId,
				PayMethodId = e.PayMethodId,
				ActDate = e.ActDate,
				ActNumber = e.ActNumber,
				Comment = e.Comment,
				ContractId = e.ContractId,
				CreatedDate = e.CreatedDate,
				InvoiceDate = e.InvoiceDate,
				InvoiceNumber = e.InvoiceNumber,
				//No = e.No,
				Number = e.Number,
				OrderId = e.OrderId,
				OriginalSum = e.OriginalSum,
				OriginalVat = e.OriginalVat,
				CurrencyRate = e.CurrencyRate,
				Route = e.Route,
				SecondSignerEmployeeId = e.SecondSignerEmployeeId,
				SecondSignerInitials = e.SecondSignerInitials,
				SecondSignerName = e.SecondSignerName,
				SecondSignerPosition = e.SecondSignerPosition,
				Sum = e.Sum,
				Vat = e.Vat,
				LegalId = e.LegalId,
				OurLegalId = e.OurLegalId,

				ContractNumber = "",
				ContractorId = isIncome ? legal.ContractorId.Value : 0,
				OrderNumber = ""
			};

			return Content(JsonConvert.SerializeObject(result));
		}

		public ContentResult GetNewOperation(int orderId)
		{
			// проверка прав участника
			if (!participantLogic.IsAllowedActionByOrder(22, orderId, CurrentUserId))
				return Content(JsonConvert.SerializeObject(new { ActionId = 22, Message = "У вас недостаточно прав на выполнение этого действия." }));

			var order = orderLogic.GetOrder(orderId);
			if (order.OrderStatusId == 2)
				return Content(JsonConvert.SerializeObject(new { ActionId = 22, Message = "Это нельзя делать с заказом в таком статусе." }));

			// подстановка значений по-умолчанию
			var o = new Operation
			{
				OrderId = orderId,
				No = orderLogic.GetOperationsByOrder(orderId).Count(),
				ResponsibleUserId = GetParticipantUsers(participantLogic.GetWorkgroupByOrder(orderId), 3).FirstOrDefault()
			};

			return Content(JsonConvert.SerializeObject(o));
		}

		public ContentResult GetNewParticipant(int orderId)
		{
			// проверка прав участника
			if (!participantLogic.IsAllowedActionByOrder(24, orderId, CurrentUserId))
				return Content(JsonConvert.SerializeObject(new { ActionId = 24, Message = "У вас недостаточно прав на выполнение этого действия." }));

			#region проверка статуса заказа
			var order = orderLogic.GetOrder(orderId);
			if (order.OrderStatusId >= 4)
				return Content(JsonConvert.SerializeObject(new { ActionId = 24, Message = "Это нельзя делать с заказом в таком статусе." }));
			#endregion

			// подстановка значений по-умолчанию
			var e = new Participant { OrderId = orderId };
			return Content(JsonConvert.SerializeObject(e));
		}

		#endregion

		#region marks

		public ContentResult GetAccountingMarks(int accountingId)
		{
			var mark = accountingLogic.GetAccountingMarkByAccounting(accountingId);
			if (mark == null)
				mark = new AccountingMark { AccountingId = accountingId };

			return Content(JsonConvert.SerializeObject(mark));
		}

		public ContentResult GetAccountingMarksByOrder(int orderId)
		{
			var list = accountingLogic.GetAccountingMarksByOrder(orderId);
			return Content(JsonConvert.SerializeObject(new { Items = list }));
		}

		public ContentResult ToggleAccountingInvoiceOk(int accountingId)
		{
			var order = orderLogic.GetOrderByAccounting(accountingId);

			var mark = accountingLogic.GetAccountingMarkByAccounting(accountingId);
			if (mark == null)
				mark = new AccountingMark { AccountingId = accountingId };

			var workgroup = participantLogic.GetWorkgroupByOrder(order.ID);
			// если проставлена метка Счет/акт проверен, ОМ не может уже снять метку Счет/акт ОК
			if (mark.IsInvoiceChecked && !IsAllowed(workgroup.Where(w => w.UserId == CurrentUserId), ParticipantRoles.GM))
				return Content(JsonConvert.SerializeObject(new { Message = "Нельзя снять метку ОК после установки метки Проверен" }));

			if (mark.IsInvoiceOk)
			{
				// проверка прав участника
				if (!participantLogic.IsAllowedActionByOrder(47, order.ID, CurrentUserId))
					return Content(JsonConvert.SerializeObject(new { ActionId = 47, Message = "У вас недостаточно прав на выполнение этого действия." }));

				mark.IsInvoiceOk = false;
				mark.InvoiceOkDate = null;
				mark.InvoiceOkUserId = null;

				// также скинуть "Счет отказан"
				mark.IsInvoiceRejected = false;
				mark.InvoiceRejectedDate = null;
				mark.InvoiceRejectedUserId = null;
				mark.InvoiceRejectedComment = "";
			}
			else
			{
				// проверка прав участника
				if (!participantLogic.IsAllowedActionByOrder(46, order.ID, CurrentUserId))
					return Content(JsonConvert.SerializeObject(new { ActionId = 46, Message = "У вас недостаточно прав на выполнение этого действия." }));

				mark.IsInvoiceOk = true;
				mark.InvoiceOkDate = DateTime.Now;
				mark.InvoiceOkUserId = CurrentUserId;
			}

			if (mark.ID == 0)
			{
				var id = accountingLogic.CreateAccountingMark(mark);
				mark = accountingLogic.GetAccountingMarkByAccounting(accountingId);
			}
			else
				accountingLogic.UpdateAccountingMark(mark);

			#region send notifications

			var accounting = accountingLogic.GetAccounting(accountingId);
			var subject = accounting.Number;
			var message = "{0}, Создан и сохранен Счет {3} в заказе <a href='http://cm.corp.local/Orders/Details/{1}?selectedAccountingId={4}'>{2}</a> прошу проставить метку 'Счет проверен'. <br /> Спасибо.";
			foreach (var item in GetParticipantUsers(workgroup, 4)) // Buh-4
			{
				var user = identityLogic.GetUser(item);
				var fm = string.Format(message, user.Name, order.ID, order.Number, accounting.Number, accounting.ID);
				SendMail(user.Email, subject, fm);
			}

			#endregion

			return Content(JsonConvert.SerializeObject(mark));
		}

		public ContentResult ToggleAccountingInvoiceChecked(int accountingId)
		{
			var order = orderLogic.GetOrderByAccounting(accountingId);

			var mark = accountingLogic.GetAccountingMarkByAccounting(accountingId);
			if (mark == null)
				mark = new AccountingMark { AccountingId = accountingId };

			if (mark.IsInvoiceChecked)
			{
				// проверка прав участника
				if (!participantLogic.IsAllowedActionByOrder(49, order.ID, CurrentUserId))
					return Content(JsonConvert.SerializeObject(new { ActionId = 49, Message = "У вас недостаточно прав на выполнение этого действия." }));

				mark.IsInvoiceChecked = false;
				mark.InvoiceCheckedDate = null;
				mark.InvoiceCheckedUserId = null;
			}
			else
			{
				// проверка прав участника
				if (!participantLogic.IsAllowedActionByOrder(48, order.ID, CurrentUserId))
					return Content(JsonConvert.SerializeObject(new { ActionId = 48, Message = "У вас недостаточно прав на выполнение этого действия." }));

				// проверить наличие метки "Ок"
				if (!mark.IsInvoiceOk)
					return Content(JsonConvert.SerializeObject(new { ActionId = 48, Message = "Сначала установите метку 'Счет Ок'." }));

				mark.IsInvoiceChecked = true;
				mark.InvoiceCheckedDate = DateTime.Now;
				mark.InvoiceCheckedUserId = CurrentUserId;
			}

			if (mark.ID == 0)
			{
				var id = accountingLogic.CreateAccountingMark(mark);
				mark = accountingLogic.GetAccountingMarkByAccounting(accountingId);
			}
			else
				accountingLogic.UpdateAccountingMark(mark);

			#region send notifications

			var workgroup = participantLogic.GetWorkgroupByOrder(order.ID);
			var accounting = accountingLogic.GetAccounting(accountingId);
			var subject = accounting.Number;
			var message = "{0}, Счет {3} в заказе <a href='http://cm.corp.local/Orders/Details/{1}?selectedAccountingId={4}'>{2}</a> проверен. <br /> Спасибо.";
			foreach (var item in GetParticipantUsers(workgroup, ParticipantRoles.AM))
			{
				var user = identityLogic.GetUser(item);
				var fm = string.Format(message, user.Name, order.ID, order.Number, accounting.Number, accounting.ID);
				SendMail(user.Email, subject, fm);
			}

			#endregion

			return Content(JsonConvert.SerializeObject(mark));
		}

		public ContentResult ToggleAccountingInvoiceRejected(int accountingId, string comment)
		{
			var mark = accountingLogic.GetAccountingMarkByAccounting(accountingId);
			if (mark == null)
				mark = new AccountingMark { AccountingId = accountingId };

			#region check roles

			var order = orderLogic.GetOrderByAccounting(accountingId);
			var workgroup = participantLogic.GetWorkgroupByOrder(order.ID);
			if (!workgroup.Any(w => w.UserId == CurrentUserId))
				return Content(JsonConvert.SerializeObject(new { Message = "Вы не являетесь участником этого заказа" }));

			if (!(mark.IsInvoiceRejected && IsAllowed(workgroup.Where(w => w.UserId == CurrentUserId), ParticipantRoles.AM)))
				if (!IsAllowed(workgroup.Where(w => w.UserId == CurrentUserId), ParticipantRoles.BUH, ParticipantRoles.GM))
					return Content(JsonConvert.SerializeObject(new { Message = "У вас нет прав на выполнение этого действия" }));

			#endregion

			if (mark.IsInvoiceRejected)
			{
				// проверка прав участника
				if (!participantLogic.IsAllowedActionByOrder(51, order.ID, CurrentUserId))
					return Content(JsonConvert.SerializeObject(new { ActionId = 51, Message = "У вас недостаточно прав на выполнение этого действия." }));

				mark.IsInvoiceRejected = false;
				mark.InvoiceRejectedDate = null;
				mark.InvoiceRejectedUserId = null;
				mark.InvoiceRejectedComment = "";
			}
			else
			{
				// проверка прав участника
				if (!participantLogic.IsAllowedActionByOrder(50, order.ID, CurrentUserId))
					return Content(JsonConvert.SerializeObject(new { ActionId = 50, Message = "У вас недостаточно прав на выполнение этого действия." }));

				mark.IsInvoiceRejected = true;
				mark.InvoiceRejectedDate = DateTime.Now;
				mark.InvoiceRejectedUserId = CurrentUserId;
				mark.InvoiceRejectedComment = comment;

				// также скинуть "Счет проверен"
				mark.IsInvoiceChecked = false;
				mark.InvoiceCheckedDate = null;
				mark.InvoiceCheckedUserId = null;

				// добавить запись в историю
				var personId = userLogic.GetUser(CurrentUserId).PersonId;
				var person = personLogic.GetPerson(personId.Value);
				accountingLogic.AppendAccountingRejectHistory(accountingId, DateTime.Now.ToString("dd-MM-yyyy HH:mm") + " " + person.DisplayName + " отклонил(а) счет, " + comment);
			}

			if (mark.ID == 0)
			{
				var id = accountingLogic.CreateAccountingMark(mark);
				mark = accountingLogic.GetAccountingMarkByAccounting(accountingId);
			}
			else
				accountingLogic.UpdateAccountingMark(mark);

			#region send notifications

			var accounting = accountingLogic.GetAccounting(accountingId);
			var subject = accounting.Number;
			var message = "{0}, Счет {3} в заказе <a href='http://cm.corp.local/Orders/Details/{1}?selectedAccountingId={5}'>{2}</a> ОТКЛОНЕН. Причина отказа:{4}. <br /> Спасибо.";
			foreach (var item in GetParticipantUsers(workgroup, ParticipantRoles.AM))
			{
				var user = identityLogic.GetUser(item);
				var fm = string.Format(message, user.Name, order.ID, order.Number, accounting.Number, comment, accounting.ID);
				SendMail(user.Email, subject, fm);
			}

			#endregion

			return Content(JsonConvert.SerializeObject(mark));
		}


		public ContentResult ToggleAccountingActOk(int accountingId)
		{
			var order = orderLogic.GetOrderByAccounting(accountingId);

			var mark = accountingLogic.GetAccountingMarkByAccounting(accountingId);
			if (mark == null)
				mark = new AccountingMark { AccountingId = accountingId };

			var workgroup = participantLogic.GetWorkgroupByOrder(order.ID);
			// если проставлена метка Счет/акт проверен, ОМ не может уже снять метку Счет/акт ОК
			if (mark.IsActChecked && !IsAllowed(workgroup.Where(w => w.UserId == CurrentUserId), ParticipantRoles.GM))
				return Content(JsonConvert.SerializeObject(new { Message = "Нельзя снять метку ОК после установки метки Проверен" }));

			if (mark.IsActOk)
			{
				// проверка прав участника
				if (!participantLogic.IsAllowedActionByOrder(41, order.ID, CurrentUserId))
					return Content(JsonConvert.SerializeObject(new { ActionId = 41, Message = "У вас недостаточно прав на выполнение этого действия." }));

				mark.IsActOk = false;
				mark.ActOkDate = null;
				mark.ActOkUserId = null;

				// также скинуть "Акт отказан"
				mark.IsActRejected = false;
				mark.ActRejectedDate = null;
				mark.ActRejectedUserId = null;
				mark.ActRejectedComment = "";
			}
			else
			{
				// проверка прав участника
				if (!participantLogic.IsAllowedActionByOrder(40, order.ID, CurrentUserId))
					return Content(JsonConvert.SerializeObject(new { ActionId = 40, Message = "У вас недостаточно прав на выполнение этого действия." }));

				mark.IsActOk = true;
				mark.ActOkDate = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Russian Standard Time");
				mark.ActOkUserId = CurrentUserId;
			}

			if (mark.ID == 0)
			{
				var id = accountingLogic.CreateAccountingMark(mark);
				mark = accountingLogic.GetAccountingMarkByAccounting(accountingId);
			}
			else
				accountingLogic.UpdateAccountingMark(mark);

			#region создать документы

			if (mark.IsActOk)
			{
				Legal legal;
				var acc = accountingLogic.GetAccounting(accountingId);
				if (acc.IsIncome)
				{
					var contract = contractLogic.GetContract(order.ContractId.Value);
					legal = legalLogic.GetLegalByOurLegal(contract.OurLegalId);
				}
				else
				{
					var contract = contractLogic.GetContract(acc.ContractId.Value);
					legal = legalLogic.GetLegalByOurLegal(contract.OurLegalId);
				}

				if (!legal.IsNotResident)
				{
					var dmessage = CreateAct_internal(accountingId, null, null);
					if (!string.IsNullOrWhiteSpace(dmessage))
					{
						// reset
						mark.IsActOk = false;
						mark.ActOkDate = null;
						mark.ActOkUserId = null;
						accountingLogic.UpdateAccountingMark(mark);

						return Content(JsonConvert.SerializeObject(new { Message = dmessage }));
					}
				}
			}

			#endregion

			#region send notifications

			var accounting = accountingLogic.GetAccounting(accountingId);
			var subject = accounting.Number;
			var message = "{0}, Создан и сохранен Акт и счет-фактура {3} в заказе <a href='http://cm.corp.local/Orders/Details/{1}?selectedAccountingId={4}'>{2}</a> прошу проставить метку 'Акт проверен'. <br /> Спасибо.";
			foreach (var item in GetParticipantUsers(workgroup, (int)ParticipantRoles.BUH))
			{
				var user = identityLogic.GetUser(item);
				var fm = string.Format(message, user.Name, order.ID, order.Number, accounting.Number, accounting.ID);
				SendMail(user.Email, subject, fm);
			}

			#endregion

			return Content(JsonConvert.SerializeObject(mark));
		}

		public ContentResult ToggleAccountingActChecked(int accountingId)
		{
			var order = orderLogic.GetOrderByAccounting(accountingId);

			var mark = accountingLogic.GetAccountingMarkByAccounting(accountingId);
			if (mark == null)
				mark = new AccountingMark { AccountingId = accountingId };

			if (mark.IsActChecked)
			{
				// проверка прав участника
				if (!participantLogic.IsAllowedActionByOrder(43, order.ID, CurrentUserId))
					return Content(JsonConvert.SerializeObject(new { ActionId = 43, Message = "У вас недостаточно прав на выполнение этого действия." }));

				mark.IsActChecked = false;
				mark.ActCheckedDate = null;
				mark.ActCheckedUserId = null;
			}
			else
			{
				// проверка прав участника
				if (!participantLogic.IsAllowedActionByOrder(42, order.ID, CurrentUserId))
					return Content(JsonConvert.SerializeObject(new { ActionId = 42, Message = "У вас недостаточно прав на выполнение этого действия." }));

				// проверить наличие метки "Ок"
				if (!mark.IsActOk)
					return Content(JsonConvert.SerializeObject(new { ActionId = 42, Message = "Сначала установите метку 'Акт Ок'." }));

				mark.IsActChecked = true;
				mark.ActCheckedDate = DateTime.Now;
				mark.ActCheckedUserId = CurrentUserId;
			}

			if (mark.ID == 0)
			{
				accountingLogic.CreateAccountingMark(mark);
				mark = accountingLogic.GetAccountingMarkByAccounting(accountingId);
			}
			else
				accountingLogic.UpdateAccountingMark(mark);

			#region send notifications

			var workgroup = participantLogic.GetWorkgroupByOrder(order.ID);
			var accounting = accountingLogic.GetAccounting(accountingId);
			var subject = accounting.Number;
			var message = "{0}, Акт в заказе <a href='http://cm.corp.local/Orders/Details/{1}?selectedAccountingId={3}'>{2}</a> проверен. <br /> Спасибо.";
			foreach (var item in GetParticipantUsers(workgroup, ParticipantRoles.AM))
			{
				var user = identityLogic.GetUser(item);
				var fm = string.Format(message, user.Name, order.ID, order.Number, accounting.ID);
				SendMail(user.Email, subject, fm);
			}

			#endregion

			#region add to sync queue

			dataLogic.CreateSyncQueue(new SyncQueue { AccountingId = accountingId });

			#endregion

			return Content(JsonConvert.SerializeObject(mark));
		}

		public ContentResult ToggleAccountingActRejected(int accountingId, string comment)
		{
			var order = orderLogic.GetOrderByAccounting(accountingId);

			var mark = accountingLogic.GetAccountingMarkByAccounting(accountingId);
			if (mark == null)
				mark = new AccountingMark { AccountingId = accountingId };

			if (mark.IsActRejected)
			{
				// проверка прав участника
				if (!participantLogic.IsAllowedActionByOrder(45, order.ID, CurrentUserId))
					return Content(JsonConvert.SerializeObject(new { ActionId = 45, Message = "У вас недостаточно прав на выполнение этого действия." }));

				mark.IsActRejected = false;
				mark.ActRejectedDate = null;
				mark.ActRejectedUserId = null;
				mark.ActRejectedComment = "";
			}
			else
			{
				// проверка прав участника
				if (!participantLogic.IsAllowedActionByOrder(44, order.ID, CurrentUserId))
					return Content(JsonConvert.SerializeObject(new { ActionId = 44, Message = "У вас недостаточно прав на выполнение этого действия." }));

				mark.IsActRejected = true;
				mark.ActRejectedDate = DateTime.Now;
				mark.ActRejectedUserId = CurrentUserId;
				mark.ActRejectedComment = comment;

				// также скинуть "Акт проверен"
				mark.IsActChecked = false;
				mark.ActCheckedDate = null;
				mark.ActCheckedUserId = null;

				// добавить запись в историю
				var personId = userLogic.GetUser(CurrentUserId).PersonId;
				var person = personLogic.GetPerson(personId.Value);
				accountingLogic.AppendAccountingRejectHistory(accountingId, DateTime.Now.ToString("dd-MM-yyyy HH:mm") + " " + person.DisplayName + " отклонил(а) счет, " + comment);
			}

			if (mark.ID == 0)
			{
				accountingLogic.CreateAccountingMark(mark);
				mark = accountingLogic.GetAccountingMarkByAccounting(accountingId);
			}
			else
				accountingLogic.UpdateAccountingMark(mark);

			#region send notifications

			var workgroup = participantLogic.GetWorkgroupByOrder(order.ID);
			var accounting = accountingLogic.GetAccounting(accountingId);
			var subject = accounting.Number;
			var message = "{0}, Акт в заказе <a href='http://cm.corp.local/Orders/Details/{1}?selectedAccountingId={4}'>{2}</a> ОТКЛОНЕН. Причина отказа:{3}. <br /> Спасибо.";
			foreach (var item in GetParticipantUsers(workgroup, ParticipantRoles.AM))
			{
				var user = identityLogic.GetUser(item);
				var fm = string.Format(message, user.Name, order.ID, order.Number, comment, accounting.ID);
				SendMail(user.Email, subject, fm);
			}

			#endregion

			return Content(JsonConvert.SerializeObject(mark));
		}


		public ContentResult ToggleAccountingOk(int accountingId)
		{
			var order = orderLogic.GetOrderByAccounting(accountingId);

			#region проверка на наличие услуг

			var services = accountingLogic.GetServicesByAccounting(accountingId);
			if (services.Count() == 0)
				return Content(JsonConvert.SerializeObject(new { Message = "Не добавлена ни одна услуга." }));

			#endregion

			#region проверка на наличие документов

			var accounting = accountingLogic.GetAccounting(accountingId);
			var contract = contractLogic.GetContract(accounting.ContractId.Value);
			var legal = legalLogic.GetLegal(contract.LegalId);
			var docs = documentLogic.GetDocumentsByAccounting(accountingId);
			var tdocs = documentLogic.GetTemplatedDocumentsByAccounting(accountingId);

			if (legal.ContractorId == 48)   // ТАМОЖНЯ
			{
				// для контрагента ТАМОЖНЯ добавить проверку на наличие привязанных документов с типом ДТ
				if (docs.Where(w => w.DocumentTypeId == 20).Count() == 0)
					return Content(JsonConvert.SerializeObject(new { Message = "Сначала прикрепите 'ДТ'" }));
			}
			else if (legal.ID == FINSERVICES_LEGAL_ID)
			{
				//  по юрлицу "# Финансовые услуги" при проставлении метки Расход Ок оставь проверку на наличие только привязанного документа с типом "Счет от поставщика"
				if (docs.Where(w => w.DocumentTypeId == 59).Select(s => s.DocumentTypeId).Distinct().Count() < 1)
					return Content(JsonConvert.SerializeObject(new { Message = "Сначала прикрепите 'Счет от поставщика'" }));
			}
			else if ((legal.ContractorId != INTERNAL_CONTRACTOR_ID) && (legal.ID != AUTOEXPENSE_LEGAL_ID)) // !контрагент "Касса" // по юрлицо "# Авторасходы" ничего не проверять.
			{
				if (legal.IsNotResident && (contract.ContractTypeId != 2))
				{
					// если поставщик нерезидент наличия привязанного к счету документа с типом "счет от поставщика", заполненной по нему дате и номеру и созданного акта
					if (docs.Where(w => w.DocumentTypeId == 59 || w.DocumentTypeId == 10).Select(s => s.DocumentTypeId).Distinct().Count() < 2)
						return Content(JsonConvert.SerializeObject(new { Message = "Сначала прикрепите 'Счет от поставщика' и 'Акт оказанных услуг постав.'" }));

					//// Act En
					//if (tdocs.Where(w => w.TemplateId == 18).Count() == 0)
					//	return Content(JsonConvert.SerializeObject(new { Message = "Сначала сформируйте 'Акт'" }));
				}
				else if (legal.IsNotResident && (contract.ContractTypeId == 2))
				{
					// Исключение для Расходов, проведенных на договора купли-продажи
					if (docs.Where(w => w.DocumentTypeId == 59).Select(s => s.DocumentTypeId).Distinct().Count() < 1)
						return Content(JsonConvert.SerializeObject(new { Message = "Сначала прикрепите 'Счет от поставщика'" }));
				}
				else
				{
					if (legal.TaxTypeId == 2)   // ЮЛ упрощенная система налогообложения (УСН)
					{
						// Сними проверку при проставлении метки «Расход ОК» на наличие привязанной сф для юрлиц на упрощенной системе налогообложения
						// если поставщик резидент, наличия привязанный к счету документов с типом "счет от поставщика", "акт оказанных услуг постав"
						if (docs.Where(w => w.DocumentTypeId == 59 || w.DocumentTypeId == 10).Select(s => s.DocumentTypeId).Distinct().Count() < 2)
							return Content(JsonConvert.SerializeObject(new { Message = "Сначала прикрепите 'Счет от поставщика', 'Акт оказанных услуг постав.'" }));
					}
					else
					{
						if (contract.ContractTypeId != 2)
						{
							if (docs.Where(w => w.DocumentTypeId == 59 || w.DocumentTypeId == 61 || w.DocumentTypeId == 10).Select(s => s.DocumentTypeId).Distinct().Count() < 3)
								return Content(JsonConvert.SerializeObject(new { Message = "Сначала прикрепите 'Счет от поставщика', 'Счет-фактура от поставщика', 'Акт оказанных услуг постав.'" }));
						}
						else
						{
							// для Расходов, проведенных на договора купли-продажи
							// проверять только это «наличия привязанного к Расходу документов с типом «счет от поставщика», «ТОРГ12», «счет-фактура от поставщика»
							if (docs.Where(w => w.DocumentTypeId == 59 || w.DocumentTypeId == 61 || w.DocumentTypeId == 62).Select(s => s.DocumentTypeId).Distinct().Count() < 3)
								return Content(JsonConvert.SerializeObject(new { Message = "Сначала прикрепите 'Счет от поставщика', 'Счет-фактура от поставщика', 'ТОРГ12'" }));
						}
					}
				}
			}


			// При проставлении метки Расход ОК по договору страхования проверять наличие созданной заявки и привязанного документа с типом "страховой полис"
			if (contract.ContractTypeId == 4)   // Генеральный договор страхования грузов
			{
				if (docs.Where(w => w.DocumentTypeId == 55).Count() == 0)
					return Content(JsonConvert.SerializeObject(new { Message = "Сначала прикрепите 'Страховой полис'" }));

				if (tdocs.Where(w => (w.TemplateId == 1) || (w.TemplateId == 3) || (w.TemplateId == 7)).Count() == 0)
					return Content(JsonConvert.SerializeObject(new { Message = "Сначала сформируйте 'Заявку'" }));
			}

			#endregion

			var mark = accountingLogic.GetAccountingMarkByAccounting(accountingId);
			if (mark == null)
				mark = new AccountingMark { AccountingId = accountingId };

			if (mark.IsAccountingOk)
			{
				// проверка прав участника
				if (!participantLogic.IsAllowedActionByOrder(53, order.ID, CurrentUserId))
					return Content(JsonConvert.SerializeObject(new { ActionId = 53, Message = "У вас недостаточно прав на выполнение этого действия." }));

				mark.IsAccountingOk = false;
				mark.AccountingOkDate = null;
				mark.AccountingOkUserId = null;

				// также сбросить "Расход отказан"
				mark.IsAccountingRejected = false;
				mark.AccountingRejectedDate = null;
				mark.AccountingRejectedUserId = null;
				mark.AccountingRejectedComment = "";
			}
			else
			{
				// проверка прав участника
				if (!participantLogic.IsAllowedActionByOrder(52, order.ID, CurrentUserId))
					return Content(JsonConvert.SerializeObject(new { ActionId = 52, Message = "У вас недостаточно прав на выполнение этого действия." }));

				mark.IsAccountingOk = true;
				mark.AccountingOkDate = DateTime.Now;
				mark.AccountingOkUserId = CurrentUserId;
			}

			if (mark.ID == 0)
			{
				accountingLogic.CreateAccountingMark(mark);
				mark = accountingLogic.GetAccountingMarkByAccounting(accountingId);
			}
			else
				accountingLogic.UpdateAccountingMark(mark);

			#region send notifications

			var workgroup = participantLogic.GetWorkgroupByOrderAtDate(order.ID, DateTime.Now);
			var subject = accounting.Number;
			var message = "{0}, Создан и сохранен Расход {3} в заказе <a href='http://cm.corp.local/Orders/Details/{1}?selectedAccountingId={4}'>{2}</a> прошу проставить метку 'Расход проверен'. <br /> Спасибо.";
			foreach (var item in workgroup.Where(w => w.ParticipantRoleId == (int)ParticipantRoles.BUH))
			{
				var user = identityLogic.GetUser(item.UserId.Value);
				var fm = string.Format(message, user.Name, order.ID, order.Number, accounting.Number, accounting.ID);
				SendMail(user.Email, subject, fm);
			}

			#endregion

			return Content(JsonConvert.SerializeObject(mark));
		}

		public ContentResult ToggleAccountingChecked(int accountingId)
		{
			var order = orderLogic.GetOrderByAccounting(accountingId);

			var mark = accountingLogic.GetAccountingMarkByAccounting(accountingId);
			if (mark == null)
				mark = new AccountingMark { AccountingId = accountingId };

			if (mark.IsAccountingChecked)
			{
				// проверка прав участника
				if (!participantLogic.IsAllowedActionByOrder(55, order.ID, CurrentUserId))
					return Content(JsonConvert.SerializeObject(new { ActionId = 55, Message = "У вас недостаточно прав на выполнение этого действия." }));

				mark.IsAccountingChecked = false;
				mark.AccountingCheckedDate = null;
				mark.AccountingCheckedUserId = null;
			}
			else
			{
				// проверка прав участника
				if (!participantLogic.IsAllowedActionByOrder(54, order.ID, CurrentUserId))
					return Content(JsonConvert.SerializeObject(new { ActionId = 54, Message = "У вас недостаточно прав на выполнение этого действия." }));

				// проверить наличие метки "Ок"
				if (!mark.IsAccountingOk)
					return Content(JsonConvert.SerializeObject(new { ActionId = 54, Message = "Сначала установите метку 'Расход Ок'." }));

				mark.IsAccountingChecked = true;
				mark.AccountingCheckedDate = DateTime.Now;
				mark.AccountingCheckedUserId = CurrentUserId;
			}

			if (mark.ID == 0)
			{
				var id = accountingLogic.CreateAccountingMark(mark);
				mark = accountingLogic.GetAccountingMarkByAccounting(accountingId);
			}
			else
				accountingLogic.UpdateAccountingMark(mark);

			#region add to sync queue

			dataLogic.CreateSyncQueue(new SyncQueue { AccountingId = accountingId });

			#endregion

			#region send notifications

			var workgroup = participantLogic.GetWorkgroupByOrderAtDate(order.ID, DateTime.Now);
			var accounting = accountingLogic.GetAccounting(accountingId);
			var subject = accounting.Number;
			var message = "{0}, Расход в заказе <a href='http://cm.corp.local/Orders/Details/{1}?selectedAccountingId={3}'>{2}</a> проверен. <br /> Спасибо.";
			foreach (var item in GetParticipantUsers(workgroup, ParticipantRoles.OPER, ParticipantRoles.AM))
			{
				var user = identityLogic.GetUser(item);
				var fm = string.Format(message, user.Name, order.ID, order.Number, accounting.ID);
				SendMail(user.Email, subject, fm);
			}

			#endregion

			if (mark.IsAccountingChecked)
			{
				CreateUpdateDiffAccounting(accountingId);  // создать расход Курсовая разница
				CreateUpdateAutoExpenseAccounting(accountingId);   // создать/обновить авторасход
				CreateUpdateAgentPercentageAccounting(accountingId);   // создать/обновить авторасход
			}

			#region отзеркалировать в авторасход
			var aeAccounting = accountingLogic.GetAeAccounting(accounting.Number);
			if (aeAccounting != null)
			{
				var mark2 = accountingLogic.GetAccountingMarkByAccounting(aeAccounting.ID);
				if (mark2 == null)
					mark2 = new AccountingMark { AccountingId = aeAccounting.ID };

				mark2.IsAccountingChecked = mark.IsAccountingChecked;
				mark2.AccountingCheckedDate = mark.AccountingCheckedDate;
				mark2.AccountingCheckedUserId = mark.AccountingCheckedUserId;

				if (mark2.ID == 0)
					accountingLogic.CreateAccountingMark(mark2);
				else
					accountingLogic.UpdateAccountingMark(mark2);
			}

			#endregion

			#region отзеркалировать в авторасход
			var apAccounting = accountingLogic.GetApAccounting(accounting.Number);
			if (apAccounting != null)
			{
				var mark2 = accountingLogic.GetAccountingMarkByAccounting(apAccounting.ID);
				if (mark2 == null)
					mark2 = new AccountingMark { AccountingId = apAccounting.ID };

				mark2.IsAccountingChecked = mark.IsAccountingChecked;
				mark2.AccountingCheckedDate = mark.AccountingCheckedDate;
				mark2.AccountingCheckedUserId = mark.AccountingCheckedUserId;

				if (mark2.ID == 0)
					accountingLogic.CreateAccountingMark(mark2);
				else
					accountingLogic.UpdateAccountingMark(mark2);
			}

			#endregion

			return Content(JsonConvert.SerializeObject(mark));
		}

		public ContentResult ToggleAccountingRejected(int accountingId, string comment)
		{
			var order = orderLogic.GetOrderByAccounting(accountingId);

			var mark = accountingLogic.GetAccountingMarkByAccounting(accountingId);
			if (mark == null)
				mark = new AccountingMark { AccountingId = accountingId };

			if (mark.IsAccountingRejected)
			{
				// проверка прав участника
				if (!participantLogic.IsAllowedActionByOrder(57, order.ID, CurrentUserId))
					return Content(JsonConvert.SerializeObject(new { ActionId = 57, Message = "У вас недостаточно прав на выполнение этого действия." }));

				mark.IsAccountingRejected = false;
				mark.AccountingRejectedDate = null;
				mark.AccountingRejectedUserId = null;
				mark.AccountingRejectedComment = "";
			}
			else
			{
				// проверка прав участника
				if (!participantLogic.IsAllowedActionByOrder(56, order.ID, CurrentUserId))
					return Content(JsonConvert.SerializeObject(new { ActionId = 56, Message = "У вас недостаточно прав на выполнение этого действия." }));

				mark.IsAccountingRejected = true;
				mark.AccountingRejectedDate = DateTime.Now;
				mark.AccountingRejectedUserId = CurrentUserId;
				mark.AccountingRejectedComment = comment;

				// также сбросить "Расход проверен"
				mark.IsAccountingChecked = false;
				mark.AccountingCheckedDate = null;
				mark.AccountingCheckedUserId = null;

				// добавить запись в историю
				var personId = userLogic.GetUser(CurrentUserId).PersonId;
				var person = personLogic.GetPerson(personId.Value);
				accountingLogic.AppendAccountingRejectHistory(accountingId, DateTime.Now.ToString("dd-MM-yyyy HH:mm") + " " + person.DisplayName + " отклонил(а) счет, " + comment);
			}

			if (mark.ID == 0)
			{
				var id = accountingLogic.CreateAccountingMark(mark);
				mark = accountingLogic.GetAccountingMarkByAccounting(accountingId);
			}
			else
				accountingLogic.UpdateAccountingMark(mark);

			#region send notifications

			var accounting = accountingLogic.GetAccounting(accountingId);
			var subject = accounting.Number;
			var message = "{0}, Расход в заказе <a href='http://cm.corp.local/Orders/Details/{1}?selectedAccountingId={4}'>{2}</a> ОТКЛОНЕН. Причина отказа:{3}. <br /> Спасибо.";
			var workgroup = participantLogic.GetWorkgroupByOrder(order.ID);
			//foreach (var item in identityLogic.GetUsersInRole(10))  // CS-10
			foreach (var item in GetParticipantUsers(workgroup, ParticipantRoles.AM))
			{
				var user = identityLogic.GetUser(item);
				var fm = string.Format(message, user.Name, order.ID, order.Number, comment, accounting.ID);
				SendMail(user.Email, subject, fm);
			}

			#endregion

			return Content(JsonConvert.SerializeObject(mark));
		}

		#endregion

		#region Change

		public ContentResult ChangePayMethod(int accountingId, int payMethodId)
		{
			// NOTE: эта функция только для расхода
			var accounting = accountingLogic.GetAccounting(accountingId);
			var contract = contractLogic.GetContract(accounting.ContractId.Value);
			var legal = legalLogic.GetLegal(contract.LegalId);
			// проверка прав участника
			var wg = FilterByDates(participantLogic.GetWorkgroupByOrder(accounting.OrderId));
			if (GetParticipantUsers(wg.Where(w => w.UserId == CurrentUserId), ParticipantRoles.GM, ParticipantRoles.BUH).Count == 0)
				return Content(JsonConvert.SerializeObject(new { Message = "У вас нет прав на это." }));

			var marks = accountingLogic.GetAccountingMarkByAccounting(accountingId);
			if ((marks != null) && marks.IsAccountingChecked && !wg.Any(w => (w.UserId == CurrentUserId) && (w.ParticipantRoleId == (int)ParticipantRoles.GM)))
				return Content(JsonConvert.SerializeObject(new { Message = "Расход уже подтвержден, нельзя менять это поле." }));

			accounting.PayMethodId = payMethodId;
			accountingLogic.UpdateAccounting(accounting);

			// если в рамках какого либо расхода на # Финансовые услуги будет сменен метод оплаты КОНСАЛТ на какой-либо другой, в соответствующем Авторасходе проставляется тот же метод оплаты
			var aeAccounting = accountingLogic.GetAeAccounting(accounting.Number);
			if (aeAccounting != null)
			{
				aeAccounting.PayMethodId = payMethodId;
				accountingLogic.UpdateAccounting(aeAccounting);
			}

			return Content(JsonConvert.SerializeObject(""));
		}

		public ContentResult ChangeOrderTemplate(int orderId, int orderTemplateId)
		{
			// проверка прав участника
			if (!participantLogic.IsAllowedActionByOrder(11, orderId, CurrentUserId))
				return Content(JsonConvert.SerializeObject(new { ActionId = 11, Message = "У вас недостаточно прав на выполнение этого действия." }));

			_ChangeOrderTemplate(orderId, orderTemplateId);

			return Content(JsonConvert.SerializeObject(""));
		}

		public ContentResult ChangeOrderStatus(int orderId, int orderStatusId, DateTime? closeDate, string reason)
		{
			var order = orderLogic.GetOrder(orderId);

			if (order.OrderStatusId == orderStatusId)
				return Content(JsonConvert.SerializeObject(""));

			var workgroup = participantLogic.GetWorkgroupByOrder(orderId).Where(w => w.UserId == CurrentUserId);
			if (workgroup.Count() == 0)
				return Content(JsonConvert.SerializeObject(new { Message = "Вы не являетесь участником этого заказа" }));

			// мегасвитч
			switch (order.OrderStatusId)
			{
				case 1: // Отказ 
					switch (orderStatusId)
					{
						//case 1:	// Отказ -> Отказ break;
						case 2: // Отказ -> Создан 
								// разрешено, проверить права
							if (!IsAllowed(workgroup, ParticipantRoles.OPER, ParticipantRoles.AM, ParticipantRoles.GM))
								return Content(JsonConvert.SerializeObject(new { Message = "У вас нет прав на выполнение этого действия" }));

							break;

						case 3: // Отказ -> В работе break;
						case 4: // Отказ -> Выполнен break;
						case 5: // Отказ -> Расходы внесены break;
						case 6: // Отказ -> Проверен break;
						case 7: // Отказ -> Мотивация break;
						case 8: // Отказ -> Корректировка break;
						case 9: // Отказ -> Закрыт break;
							return Content(JsonConvert.SerializeObject(new { Message = "Этот переход недопустим." }));
					}
					break;

				case 2: // Создан
					switch (orderStatusId)
					{
						case 1: // Создан -> Отказ 
								// разрешено, проверить права
							break;

						//case 2:	// Создан -> Создан break;
						case 3: // Создан -> В работе
								// разрешено, проверить права
							if (!IsAllowed(workgroup, ParticipantRoles.OPER, ParticipantRoles.AM, ParticipantRoles.GM))
								return Content(JsonConvert.SerializeObject(new { Message = "У вас нет прав на выполнение этого действия" }));

							break;

						case 4: // Создан -> Выполнен break;
						case 5: // Создан -> Расходы внесены break;
						case 6: // Создан -> Проверен break;
						case 7: // Создан -> Мотивация break;
						case 8: // Создан -> Корректировка break;
						case 9: // Создан -> Закрыт break;

							return Content(JsonConvert.SerializeObject(new { Message = "Этот переход недопустим." }));
					}
					break;

				case 3: // В работе 
					switch (orderStatusId)
					{
						case 1: // В работе -> Отказ 
								// разрешено, проверить права
							break;

						case 2: // В работе -> Создан break;
							return Content(JsonConvert.SerializeObject(new { Message = "Этот переход недопустим." }));
						//case 3:	// В работе -> В работе break;
						case 4: // В работе -> Выполнен 
								// разрешено, проверить права
							if (!IsAllowed(workgroup, ParticipantRoles.OPER, ParticipantRoles.AM, ParticipantRoles.GM))
								return Content(JsonConvert.SerializeObject(new { Message = "У вас нет прав на выполнение этого действия" }));

							break;

						case 5: // В работе -> Расходы внесены break;
						case 6: // В работе -> Проверен break;
						case 7: // В работе -> Мотивация break;
						case 8: // В работе -> Корректировка break;
						case 9: // В работе -> Закрыт break;
							return Content(JsonConvert.SerializeObject(new { Message = "Этот переход недопустим." }));
					}
					break;

				case 4: // Выполнен 
					switch (orderStatusId)
					{
						case 1: // Выполнен -> Отказ 
								// разрешено, проверить права
							break;

						case 2: // Выполнен -> Создан break;
							return Content(JsonConvert.SerializeObject(new { Message = "Этот переход недопустим." }));

						case 3: // Выполнен -> В работе break;
								// разрешено, проверить права
							if (!IsAllowed(workgroup, ParticipantRoles.GM))
								return Content(JsonConvert.SerializeObject(new { Message = "У вас нет прав на выполнение этого действия" }));

							break;

						//case 4:	// Выполнен -> Выполнен break;
						case 5: // Выполнен -> Расходы внесены 
								// разрешено, проверить права
							if (!IsAllowed(workgroup, ParticipantRoles.OPER, ParticipantRoles.AM, ParticipantRoles.GM))
								return Content(JsonConvert.SerializeObject(new { Message = "У вас нет прав на выполнение этого действия" }));

							break;

						case 6: // Выполнен -> Проверен break;
						case 7: // Выполнен -> Мотивация break;
						case 8: // Выполнен -> Корректировка break;
						case 9: // Выполнен -> Закрыт break;
							return Content(JsonConvert.SerializeObject(new { Message = "Этот переход недопустим." }));
					}

					break;

				case 5: // Расходы внесены 
					switch (orderStatusId)
					{
						case 1: // Расходы внесены -> Отказ break;
						case 2: // Расходы внесены -> Создан break;
						case 3: // Расходы внесены -> В работе break;
						case 4: // Расходы внесены -> Выполнен 
							if (!IsAllowed(workgroup, ParticipantRoles.OPER, ParticipantRoles.AM, ParticipantRoles.GM))
								return Content(JsonConvert.SerializeObject(new { Message = "У вас нет прав на выполнение этого действия" }));

							break;

						//case 5:	// Расходы внесены -> Расходы внесены break;
						case 6: // Расходы внесены -> Проверен 	break;
						case 7: // Расходы внесены -> Мотивация break;
						case 8: // Расходы внесены -> Корректировка break;
							return Content(JsonConvert.SerializeObject(new { Message = "Этот переход недопустим." }));

						case 9: // Расходы внесены -> Закрыт 
								// разрешено, проверить права
							if (!IsAllowed(workgroup, ParticipantRoles.OPER, ParticipantRoles.AM, ParticipantRoles.GM))
								return Content(JsonConvert.SerializeObject(new { Message = "У вас нет прав на выполнение этого действия" }));

							break;
					}

					break;

				case 6: // Проверен 
					switch (orderStatusId)
					{
						case 1: // Проверен -> Отказ break;
						case 2: // Проверен -> Создан break;
						case 3: // Проверен -> В работе break;
						case 4: // Проверен -> Выполнен break;
						case 5: // Проверен -> Расходы внесены break;
							return Content(JsonConvert.SerializeObject(new { Message = "Этот переход недопустим." }));

						//case 6:	// Проверен -> Проверен break;
						case 7: // Проверен -> Мотивация 
								// разрешено, проверить права
							if (!IsAllowed(workgroup, ParticipantRoles.BUH, ParticipantRoles.GM))
								return Content(JsonConvert.SerializeObject(new { Message = "У вас нет прав на выполнение этого действия" }));

							break;

						case 8: // Проверен -> Корректировка 
								// разрешено, проверить права
							if (!IsAllowed(workgroup, ParticipantRoles.BUH, ParticipantRoles.GM))
								return Content(JsonConvert.SerializeObject(new { Message = "У вас нет прав на выполнение этого действия" }));

							break;

						case 9: // Проверен -> Закрыт break;
							return Content(JsonConvert.SerializeObject(new { Message = "Этот переход недопустим." }));

							// TODO Проверен -> Ошибка
					}

					break;

				case 7: // Мотивация 
					switch (orderStatusId)
					{
						case 1: // Мотивация -> Отказ break;
						case 2: // Мотивация -> Создан break;
						case 3: // Мотивация -> В работе break;
						case 4: // Мотивация -> Выполнен break;
						case 5: // Мотивация -> Расходы внесены break;
						case 6: // Мотивация -> Проверен break;
							return Content(JsonConvert.SerializeObject(new { Message = "Этот переход недопустим." }));

						//case 7:	// Мотивация -> Мотивация break;
						case 8: // Мотивация -> Корректировка 
								// разрешено, проверить права
							if (!IsAllowed(workgroup, /*ParticipantRoles.BUH, */ParticipantRoles.GM))
								return Content(JsonConvert.SerializeObject(new { Message = "У вас нет прав на выполнение этого действия" }));

							break;

						case 9: // Мотивация -> Закрыт break;
							return Content(JsonConvert.SerializeObject(new { Message = "Этот переход недопустим." }));
					}

					break;

				case 8: // Корректировка 
					switch (orderStatusId)
					{
						case 1: // Корректировка -> Отказ break;
						case 2: // Корректировка -> Создан break;
						case 3: // Корректировка -> В работе break;
						case 4: // Корректировка -> Выполнен break;
							return Content(JsonConvert.SerializeObject(new { Message = "Этот переход недопустим." }));
						case 5: // Корректировка -> Расходы внесены 
								// разрешено, проверить права
							if (!IsAllowed(workgroup, ParticipantRoles.OPER, ParticipantRoles.AM, ParticipantRoles.GM))
								return Content(JsonConvert.SerializeObject(new { Message = "У вас нет прав на выполнение этого действия" }));

							break;

						case 6: // Корректировка -> Проверен break;
						case 7: // Корректировка -> Мотивация break;
								//case 8:	// Корректировка -> Корректировка break;
						case 9: // Корректировка -> Закрыт break;
							return Content(JsonConvert.SerializeObject(new { Message = "Этот переход недопустим." }));
					}

					break;

				case 9: // Закрыт break;
					switch (orderStatusId)
					{
						case 1: // Закрыт -> Отказ break;
						case 2: // Закрыт -> Создан break;
						case 3: // Закрыт -> В работе break;
						case 4: // Закрыт -> Выполнен break;
							return Content(JsonConvert.SerializeObject(new { Message = "Этот переход недопустим." }));

						case 5: // Закрыт -> Расходы внесены 
								// разрешено, проверить права
							if (!IsAllowed(workgroup, ParticipantRoles.OPER, ParticipantRoles.AM, ParticipantRoles.GM))
								return Content(JsonConvert.SerializeObject(new { Message = "У вас нет прав на выполнение этого действия" }));

							break;

						case 6: // Закрыт -> Проверен 
								// разрешено, проверить права
							if (!IsAllowed(workgroup, ParticipantRoles.BUH, ParticipantRoles.GM))
								return Content(JsonConvert.SerializeObject(new { Message = "У вас нет прав на выполнение этого действия" }));

							break;

						case 7: // Закрыт -> Мотивация break;
						case 8: // Закрыт -> Корректировка break;
							return Content(JsonConvert.SerializeObject(new { Message = "Этот переход недопустим." }));
					}

					break;

				default:
					break;
			}

			order.OrderStatusId = orderStatusId;

			if (order.OrderStatusId == 7)   // мотивация
				order.ClosedDate = closeDate ?? DateTime.Now;

			orderLogic.UpdateOrder(order);
			orderLogic.CreateOrderStatusHistory(new OrderStatusHistory { OrderId = orderId, Date = DateTime.Now, OrderStatusId = orderStatusId, Reason = reason, UserId = CurrentUserId });

			if (order.OrderStatusId == 7)   // мотивация
				MoveOrderFilesToArchive(order.Number, order.ClosedDate.Value);

			// send emails
			SendStatusChangeAlerts(orderId, orderStatusId, reason);
			var contractor = contractorLogic.GetContractorByContract(order.ContractId.Value);
			new NotificationMailer().SendNotificationOf_StatusChanged(contractor.ID, order, CurrentUserId);

			return Content(JsonConvert.SerializeObject(""));
		}

		public ContentResult ChangeOrderContractor(int orderId, int contractorId)
		{
			// проверка прав участника
			if (!participantLogic.IsAllowedActionByContractor(12, contractorId, CurrentUserId))
				return Content(JsonConvert.SerializeObject(new { ActionId = 12, Message = "У вас недостаточно прав на выполнение этого действия." }));

			participantLogic.ClearWorkgroupByOrder(orderId);

			var wg = participantLogic.GetWorkgroupByContractor(contractorId);
			var responsibleId = GetParticipantUsers(wg.Where(w => w.IsResponsible), ParticipantRoles.AM).FirstOrDefault();
			var deputy = wg.FirstOrDefault(w => w.IsDeputy);
			if ((deputy != null) && (deputy.FromDate < DateTime.Now) && (deputy.ToDate > DateTime.Now))
				responsibleId = deputy.ID;

			foreach (var item in wg)
				if (!item.ToDate.HasValue || item.ToDate.Value > DateTime.Now)
					participantLogic.CreateParticipant(new Participant
					{
						UserId = item.UserId,
						OrderId = orderId,
						ParticipantRoleId = item.ParticipantRoleId,
						FromDate = item.FromDate,
						ToDate = item.ToDate,
						IsDeputy = item.IsDeputy,
						IsResponsible = (item.UserId == responsibleId) && (item.ParticipantRoleId == (int)ParticipantRoles.AM)
					});

			// TODO:

			#region сменить юрлицо в доходах и перегенерировать документы
			var ffactory = new FileFactory();

			var incomes = accountingLogic.GetAccountingsByOrder(orderId).Where(w => w.IsIncome);
			foreach (var item in incomes)
			{
				var marks = accountingLogic.GetAccountingMarkByAccounting(item.ID);
				if ((marks != null) && (marks.IsInvoiceChecked || marks.IsActChecked))
					continue;

				var documents = documentLogic.GetTemplatedDocumentsByAccounting(item.ID);
				foreach (var document in documents)
				{
					document.ChangedDate = DateTime.Now;
					document.ChangedBy = CurrentUserId;
					documentLogic.UpdateTemplatedDocument(document);

					switch (document.TemplateId)
					{
						case 1:
							if (string.IsNullOrEmpty(ffactory.GenerateInsurance(document, TemplatedDocumentFormat.Pdf)))
							{
								ffactory.GenerateInsurance(document, TemplatedDocumentFormat.CleanPdf);
								ffactory.GenerateInsurance(document, TemplatedDocumentFormat.CutPdf);
							}
							break;

						case 3:
							if (string.IsNullOrEmpty(ffactory.GenerateRequest(document, TemplatedDocumentFormat.Pdf)))
							{
								ffactory.GenerateRequest(document, TemplatedDocumentFormat.CleanPdf);
								ffactory.GenerateRequest(document, TemplatedDocumentFormat.CutPdf);
							}
							break;

						case 5:
							if (string.IsNullOrEmpty(ffactory.GenerateDetails(document, TemplatedDocumentFormat.Pdf)))
							{
								ffactory.GenerateDetails(document, TemplatedDocumentFormat.CleanPdf);
								ffactory.GenerateDetails(document, TemplatedDocumentFormat.CutPdf);
							}
							break;

						case 7:
							if (string.IsNullOrEmpty(ffactory.GenerateEnRequest(document, TemplatedDocumentFormat.Pdf)))
							{
								ffactory.GenerateEnRequest(document, TemplatedDocumentFormat.CleanPdf);
								ffactory.GenerateEnRequest(document, TemplatedDocumentFormat.CutPdf);
							}
							break;

						case 15:
							if (string.IsNullOrEmpty(ffactory.GenerateVatInvoice(document, TemplatedDocumentFormat.Pdf)))
							{
								ffactory.GenerateVatInvoice(document, TemplatedDocumentFormat.CleanPdf);
								ffactory.GenerateVatInvoice(document, TemplatedDocumentFormat.CutPdf);
							}
							break;

						case 16:
							if (string.IsNullOrEmpty(ffactory.GenerateInvoice(document, TemplatedDocumentFormat.Pdf)))
							{
								ffactory.GenerateInvoice(document, TemplatedDocumentFormat.CleanPdf);
								ffactory.GenerateInvoice(document, TemplatedDocumentFormat.CutPdf);
							}
							break;

						case 17:
							if (string.IsNullOrEmpty(ffactory.GenerateAct(document, TemplatedDocumentFormat.Pdf)))
							{
								ffactory.GenerateAct(document, TemplatedDocumentFormat.CleanPdf);
								ffactory.GenerateAct(document, TemplatedDocumentFormat.CutPdf);
							}
							break;

						case 18:
							if (string.IsNullOrEmpty(ffactory.GenerateEnAct(document, TemplatedDocumentFormat.Pdf)))
							{
								ffactory.GenerateEnAct(document, TemplatedDocumentFormat.CleanPdf);
								ffactory.GenerateEnAct(document, TemplatedDocumentFormat.CutPdf);
							}
							break;

						case 19:
							if (string.IsNullOrEmpty(ffactory.GenerateEnInvoice(document, TemplatedDocumentFormat.Pdf)))
							{
								ffactory.GenerateEnInvoice(document, TemplatedDocumentFormat.CleanPdf);
								ffactory.GenerateEnInvoice(document, TemplatedDocumentFormat.CutPdf);
							}
							break;

						// TODO: 20 and rest

						case 21:
							if (string.IsNullOrEmpty(ffactory.GenerateNonresidentEnInvoice(document, TemplatedDocumentFormat.Pdf)))
							{
								ffactory.GenerateNonresidentEnInvoice(document, TemplatedDocumentFormat.CleanPdf);
								ffactory.GenerateNonresidentEnInvoice(document, TemplatedDocumentFormat.CutPdf);
							}
							break;

						case 23:
							if (string.IsNullOrEmpty(ffactory.GenerateNonresidentInvoice(document, TemplatedDocumentFormat.Pdf)))
							{
								ffactory.GenerateNonresidentInvoice(document, TemplatedDocumentFormat.CleanPdf);
								ffactory.GenerateNonresidentInvoice(document, TemplatedDocumentFormat.CutPdf);
							}
							break;

						default:
							break;
					}
				}
			}
			#endregion

			return Content(JsonConvert.SerializeObject(""));
		}

		public ContentResult ChangeOrderProduct(int orderId, int productId, int templateId, IEnumerable<IdPair> services)
		{
			// проверка прав участника
			if (!participantLogic.IsAllowedActionByOrder(39, orderId, CurrentUserId))
				return Content(JsonConvert.SerializeObject(new { ActionId = 39, Message = "У вас недостаточно прав на выполнение этого действия." }));

			var order = orderLogic.GetOrder(orderId);
			order.ProductId = productId;
			order.OrderTemplateId = templateId;

			var product = dataLogic.GetProduct(productId);
			if (product.VolumetricRatioId.HasValue)
				order.VolumetricRatioId = product.VolumetricRatioId;

			orderLogic.UpdateOrder(order);
			orderLogic.CalculateCargoSeats(order.ID);

			_ChangeOrderTemplate(orderId, templateId);

			if (services != null)
				foreach (var idpair in services)
				{
					var service = accountingLogic.GetService(idpair.Id);
					var serviceType = dataLogic.GetServiceType(idpair.Id2);
					var serviceKind = dataLogic.GetServiceKind(serviceType.ServiceKindId);
					service.ServiceTypeId = idpair.Id2;
					service.VatId = serviceKind.VatId;
					accountingLogic.UpdateService(service);
				}

			return Content(JsonConvert.SerializeObject(new { Url = Url.Action("Details", new { id = orderId }) }));
		}

		#endregion

		#region Create

		public ContentResult CreateClaim(int orderId)
		{
			Template template;
			var order = orderLogic.GetOrder(orderId);
			var contract = contractLogic.GetContract(order.ContractId.Value);
			var legal = legalLogic.GetLegal(contract.LegalId);
			if (legal.IsNotResident)
				template = dataLogic.GetTemplate(7);    // En
			else
				template = dataLogic.GetTemplate(2);

			var document = documentLogic.GetTemplatedDocumentsByOrder(orderId).FirstOrDefault(w => w.TemplateId == 2);
			int documentId = 0;
			if (document != null)
			{
				documentId = document.ID;
				document.ChangedDate = DateTime.Now;
				document.ChangedBy = CurrentUserId;
				documentLogic.UpdateTemplatedDocument(document);
			}
			else
			{
				document = new TemplatedDocument
				{
					TemplateId = template.ID,
					Filename = order.Number + "_" + template.Suffix,
					CreatedDate = DateTime.Now,
					CreatedBy = CurrentUserId,
					OrderId = orderId
				};

				documentId = documentLogic.CreateTemplatedDocument(document);
				document = documentLogic.GetTemplatedDocument(documentId);
			}

			var ffactory = new FileFactory();

			switch (document.TemplateId)
			{
				case 2: // заявление поставщика
					var result = ffactory.GenerateClaim(document, TemplatedDocumentFormat.Pdf);
					if (!string.IsNullOrEmpty(result))
						return Content(JsonConvert.SerializeObject(new { Message = result }));

					ffactory.GenerateClaim(document, TemplatedDocumentFormat.CleanPdf);
					ffactory.GenerateClaim(document, TemplatedDocumentFormat.CutPdf);
					break;

				case 7: // заявление поставщика RU-EN
					var result7 = ffactory.GenerateEnRequest(document, TemplatedDocumentFormat.Pdf);
					if (!string.IsNullOrEmpty(result7))
						return Content(JsonConvert.SerializeObject(new { Message = result7 }));

					ffactory.GenerateEnRequest(document, TemplatedDocumentFormat.CleanPdf);
					ffactory.GenerateEnRequest(document, TemplatedDocumentFormat.CutPdf);
					break;
			}

			return Content(JsonConvert.SerializeObject(new { FileId = documentId }));
		}

		public ContentResult CreateVatInvoice(int accountingId)
		{
			var accounting = accountingLogic.GetAccounting(accountingId);

			// проверка прав участника
			if (!participantLogic.IsAllowedActionByOrder(60, accounting.OrderId, CurrentUserId))
				return Content(JsonConvert.SerializeObject(new { ActionId = 60, Message = "У вас недостаточно прав на выполнение этого действия." }));

			#region проверка меток и прочего

			var mark = accountingLogic.GetAccountingMarkByAccounting(accountingId);
			var workgroup = participantLogic.GetWorkgroupByOrder(accounting.OrderId).Where(w => w.UserId == CurrentUserId);
			if (mark != null && (mark.IsActChecked) && !IsAllowed(workgroup, ParticipantRoles.BUH, ParticipantRoles.GM))
				return Content(JsonConvert.SerializeObject(new { Message = "Нельзя менять документ после проставления метки 'Акт проверен'" }));

			#endregion

			var message = CreateVatInvoice_internal(accountingId);
			if (!string.IsNullOrWhiteSpace(message))
				return Content(JsonConvert.SerializeObject(new { Message = message }));

			var document = documentLogic.GetTemplatedDocumentsByAccounting(accountingId).FirstOrDefault(w => w.TemplateId == 15);

			return Content(JsonConvert.SerializeObject(new { FileId = document.ID }));
		}

		string CreateVatInvoice_internal(int accountingId)
		{
			const int TEMPLATE_ID = 15;
			var accounting = accountingLogic.GetAccounting(accountingId);
			if (!accounting.IsIncome)   // расход
				return "Для РАСХОДА счета-фактуры не формируются.";

			if (!accounting.ActDate.HasValue)
				return "Поле 'Дата акта' не заполнено.";

			var order = orderLogic.GetOrder(accounting.OrderId);
			var contract = contractLogic.GetContract(order.ContractId.Value);
			var ourLegalE = legalLogic.GetOurLegal(contract.OurLegalId);
			var ourLegal = legalLogic.GetLegal(ourLegalE.LegalId.Value);
			if (ourLegal.IsNotResident)
				return "Для нашего контрагента-нерезидента этот документ не создается.";

			var documents = documentLogic.GetTemplatedDocumentsByAccounting(accountingId);
			// формирование счета фактуры возможно только после создания акта выполненных работ и проставления метки Акт ОК.
			if (documents.FirstOrDefault(w => w.TemplateId == 6 || w.TemplateId == 8 || w.TemplateId == 17 || w.TemplateId == 18) == null)
				return "Перед формированием счет-фактуры необходимо сформировать акт!";

			var mark = accountingLogic.GetAccountingMarkByAccounting(accountingId);
			if ((mark == null) || !mark.IsActOk)
				return "Формирование счет-фактуры возможно только после проставления метки Акт ОК!";

			int documentId = 0;
			var document = documents.FirstOrDefault(w => w.TemplateId == TEMPLATE_ID);
			if (document != null)
			{
				documentId = document.ID;
				document.ChangedDate = DateTime.Now;
				document.ChangedBy = CurrentUserId;
				documentLogic.UpdateTemplatedDocument(document);
			}
			else
			{
				var template = dataLogic.GetTemplate(TEMPLATE_ID);

				document = new TemplatedDocument
				{
					TemplateId = template.ID,
					Filename = accounting.Number + "_" + template.Suffix,
					CreatedDate = DateTime.Now,
					CreatedBy = CurrentUserId,
					AccountingId = accountingId
				};

				documentId = documentLogic.CreateTemplatedDocument(document);
				document = documentLogic.GetTemplatedDocument(documentId);
			}

			var ffactory = new FileFactory();
			var result = ffactory.GenerateVatInvoice(document, TemplatedDocumentFormat.Pdf);
			if (!string.IsNullOrEmpty(result))
				return result;

			ffactory.GenerateVatInvoice(document, TemplatedDocumentFormat.CleanPdf);
			ffactory.GenerateVatInvoice(document, TemplatedDocumentFormat.CutPdf);

			return string.Empty;
		}

		public ContentResult CreateInvoice(int accountingId)
		{
			var accounting = accountingLogic.GetAccounting(accountingId);
			if (!accounting.IsIncome)
				return Content(JsonConvert.SerializeObject(new { Message = "Для РАСХОДА счета не формируются." }));

			// проверка прав участника
			if (!participantLogic.IsAllowedActionByOrder(58, accounting.OrderId, CurrentUserId))
				return Content(JsonConvert.SerializeObject(new { ActionId = 58, Message = "У вас недостаточно прав на выполнение этого действия." }));

			//#region проверка меток и прочего

			//var workgroup = participantLogic.GetWorkgroupByOrder(accounting.OrderId).Where(w => w.UserId == CurrentUserId);
			//var mark = accountingLogic.GetAccountingMarkByAccounting(accountingId);
			//if (mark != null && (mark.IsInvoiceChecked) && !IsAllowed(workgroup, ParticipantRoles.BUH, ParticipantRoles.GM))
			//	return Content(JsonConvert.SerializeObject(new { Message = "Нельзя менять документ после проставления метки 'Счет проверен'" }));

			//#endregion

			int documentId = 0;
			var document = documentLogic.GetTemplatedDocumentsByAccounting(accountingId).FirstOrDefault(w => w.TemplateId == 16 || w.TemplateId == 19 || w.TemplateId == 21 || w.TemplateId == 23);
			if (document != null)
			{
				documentId = document.ID;
				document.ChangedDate = DateTime.Now;
				document.ChangedBy = CurrentUserId;
				documentLogic.UpdateTemplatedDocument(document);
			}
			else
			{
				var template = dataLogic.GetTemplate(16);
				var order = orderLogic.GetOrder(accounting.OrderId);
				var contract = contractLogic.GetContract(accounting.IsIncome ? order.ContractId.Value : accounting.ContractId.Value);
				var legal = legalLogic.GetLegal(accounting.IsIncome ? accounting.LegalId.Value : contract.LegalId);
				var ourLegal = legalLogic.GetLegalByOurLegal(contract.OurLegalId);
				if (legal.IsNotResident)
					template = dataLogic.GetTemplate(19);   // счет на англ

				if (ourLegal.IsNotResident && legal.IsNotResident)
					template = dataLogic.GetTemplate(21);   // счет нашего нерезидента к нерезиденту

				if (ourLegal.IsNotResident && !legal.IsNotResident)
					template = dataLogic.GetTemplate(23);   // счет нашего нерезидента к резиденту

				document = new TemplatedDocument
				{
					TemplateId = template.ID,
					Filename = accounting.Number + "_" + template.Suffix,
					CreatedDate = DateTime.Now,
					CreatedBy = CurrentUserId,
					AccountingId = accountingId
				};

				documentId = documentLogic.CreateTemplatedDocument(document);
				document = documentLogic.GetTemplatedDocument(documentId);
			}

			documentLogic.ClearTemplatedDocument(documentId);

			var ffactory = new FileFactory();
			switch (document.TemplateId)
			{
				case 16:
					var result = ffactory.GenerateInvoice(document, TemplatedDocumentFormat.Pdf);
					if (!string.IsNullOrEmpty(result))
						return Content(JsonConvert.SerializeObject(new { Message = result }));

					ffactory.GenerateInvoice(document, TemplatedDocumentFormat.CleanPdf);
					ffactory.GenerateInvoice(document, TemplatedDocumentFormat.CutPdf);
					break;

				case 19:
					var result2 = ffactory.GenerateEnInvoice(document, TemplatedDocumentFormat.Pdf);
					if (!string.IsNullOrEmpty(result2))
						return Content(JsonConvert.SerializeObject(new { Message = result2 }));

					ffactory.GenerateEnInvoice(document, TemplatedDocumentFormat.CleanPdf);
					ffactory.GenerateEnInvoice(document, TemplatedDocumentFormat.CutPdf);
					break;

				case 21:
					var result3 = ffactory.GenerateNonresidentEnInvoice(document, TemplatedDocumentFormat.Pdf);
					if (!string.IsNullOrEmpty(result3))
						return Content(JsonConvert.SerializeObject(new { Message = result3 }));

					ffactory.GenerateNonresidentEnInvoice(document, TemplatedDocumentFormat.CleanPdf);
					ffactory.GenerateNonresidentEnInvoice(document, TemplatedDocumentFormat.CutPdf);
					break;

				case 23:
					var result4 = ffactory.GenerateNonresidentInvoice(document, TemplatedDocumentFormat.Pdf);
					if (!string.IsNullOrEmpty(result4))
						return Content(JsonConvert.SerializeObject(new { Message = result4 }));

					ffactory.GenerateNonresidentInvoice(document, TemplatedDocumentFormat.CleanPdf);
					ffactory.GenerateNonresidentInvoice(document, TemplatedDocumentFormat.CutPdf);
					break;
			}

			return Content(JsonConvert.SerializeObject(new { FileId = documentId }));
		}

		public ContentResult CreateDetailing(int accountingId)
		{
			const int TEMPLATE_ID = 5;
			var accounting = accountingLogic.GetAccounting(accountingId);

			// проверка прав участника
			if (!participantLogic.IsAllowedActionByOrder(61, accounting.OrderId, CurrentUserId))
				return Content(JsonConvert.SerializeObject(new { ActionId = 61, Message = "У вас недостаточно прав на выполнение этого действия." }));

			int documentId = 0;
			var document = documentLogic.GetTemplatedDocumentsByAccounting(accountingId).FirstOrDefault(w => w.TemplateId == TEMPLATE_ID);
			if (document != null)
			{
				documentId = document.ID;
				document.ChangedBy = CurrentUserId;
				document.ChangedDate = DateTime.Now;
				documentLogic.UpdateTemplatedDocument(document);
			}
			else
			{
				var template = dataLogic.GetTemplate(TEMPLATE_ID);
				document = new TemplatedDocument
				{
					TemplateId = template.ID,
					Filename = accounting.Number + "_" + template.Suffix,
					CreatedDate = DateTime.Now,
					CreatedBy = CurrentUserId,
					AccountingId = accountingId
				};

				documentId = documentLogic.CreateTemplatedDocument(document);
				document = documentLogic.GetTemplatedDocument(documentId);
			}

			var ffactory = new FileFactory();
			var result = ffactory.GenerateDetails(document, TemplatedDocumentFormat.Pdf);
			if (!string.IsNullOrEmpty(result))
				return Content(JsonConvert.SerializeObject(new { Message = result }));

			ffactory.GenerateDetails(document, TemplatedDocumentFormat.CleanPdf);
			ffactory.GenerateDetails(document, TemplatedDocumentFormat.CutPdf);

			return Content(JsonConvert.SerializeObject(new { FileId = documentId }));
		}

		public ContentResult CreateAmpleDetailing(int accountingId)
		{
			const int TEMPLATE_ID = 25;
			var accounting = accountingLogic.GetAccounting(accountingId);
			if (!accounting.IsIncome)
				return Content(JsonConvert.SerializeObject(new { Message = "Для РАСХОДА детализация не формируются." }));

			// проверка прав участника
			if (!participantLogic.IsAllowedActionByOrder(61, accounting.OrderId, CurrentUserId))
				return Content(JsonConvert.SerializeObject(new { ActionId = 61, Message = "У вас недостаточно прав на выполнение этого действия." }));

			int documentId = 0;
			var document = documentLogic.GetTemplatedDocumentsByAccounting(accountingId).FirstOrDefault(w => w.TemplateId == TEMPLATE_ID);
			if (document != null)
			{
				documentId = document.ID;
				document.ChangedBy = CurrentUserId;
				document.ChangedDate = DateTime.Now;
				documentLogic.UpdateTemplatedDocument(document);
			}
			else
			{
				var template = dataLogic.GetTemplate(TEMPLATE_ID);
				document = new TemplatedDocument
				{
					TemplateId = template.ID,
					Filename = accounting.Number + "_" + template.Suffix,
					CreatedDate = DateTime.Now,
					CreatedBy = CurrentUserId,
					AccountingId = accountingId
				};

				documentId = documentLogic.CreateTemplatedDocument(document);
				document = documentLogic.GetTemplatedDocument(documentId);
			}

			var ffactory = new FileFactory();
			var result = ffactory.GenerateAmpleDetails(document, TemplatedDocumentFormat.Pdf);
			if (!string.IsNullOrEmpty(result))
				return Content(JsonConvert.SerializeObject(new { Message = result }));

			ffactory.GenerateAmpleDetails(document, TemplatedDocumentFormat.CleanPdf);
			ffactory.GenerateAmpleDetails(document, TemplatedDocumentFormat.CutPdf);

			return Content(JsonConvert.SerializeObject(new { FileId = documentId }));
		}

		public ContentResult CreateAct(int accountingId, string number, DateTime? date)
		{
			var accounting = accountingLogic.GetAccounting(accountingId);

			// проверка прав участника
			if (!participantLogic.IsAllowedActionByOrder(59, accounting.OrderId, CurrentUserId))
				return Content(JsonConvert.SerializeObject(new { ActionId = 59, Message = "У вас недостаточно прав на выполнение этого действия." }));

			#region проверка меток и прочего

			var workgroup = participantLogic.GetWorkgroupByOrder(accounting.OrderId).Where(w => w.UserId == CurrentUserId);
			var mark = accountingLogic.GetAccountingMarkByAccounting(accountingId);
			if (mark != null && (mark.IsActChecked) && !IsAllowed(workgroup, ParticipantRoles.BUH, ParticipantRoles.GM))
				return Content(JsonConvert.SerializeObject(new { Message = "Нельзя менять документ после проставления метки 'Акт проверен'" }));

			if (!accounting.IsIncome && !date.HasValue)
				return Content(JsonConvert.SerializeObject(new { Message = "Не указана дата акта расхода! Перед формированием документа необходимо указать дату акта." }));

			#endregion

			var message = CreateAct_internal(accountingId, number, date);
			if (!string.IsNullOrWhiteSpace(message))
				return Content(JsonConvert.SerializeObject(new { Message = message }));

			var document = documentLogic.GetTemplatedDocumentsByAccounting(accountingId).FirstOrDefault(w => w.TemplateId == 17 || w.TemplateId == 18 || w.TemplateId == 20);

			return Content(JsonConvert.SerializeObject(new { FileId = document.ID }));
		}

		string CreateAct_internal(int accountingId, string number, DateTime? date)
		{
			var accounting = accountingLogic.GetAccounting(accountingId);
			var order = orderLogic.GetOrder(accounting.OrderId);
			var contract = contractLogic.GetContract(accounting.IsIncome ? order.ContractId.Value : accounting.ContractId.Value);
			var legal = legalLogic.GetLegal(accounting.IsIncome ? accounting.LegalId.Value : contract.LegalId);
			var ourLegal = legalLogic.GetLegalByOurLegal(contract.OurLegalId);

			if (!accounting.IsIncome)
			{
				accounting.ActNumber = number;
				accounting.ActDate = date;
				accountingLogic.UpdateAccounting(accounting);
			}

			if (!accounting.IsIncome && !legal.IsNotResident)
				return "Акт по договору с поставщиком резидентом формирует контрагент!";

			if (ourLegal.IsNotResident && legal.IsNotResident)
				return "Для нашего контрагента-нерезидента этот документ не создается.";

			var templatedDocs = documentLogic.GetTemplatedDocumentsByAccounting(accountingId);
			var document = templatedDocs.FirstOrDefault(w => w.TemplateId == 17 || w.TemplateId == 18 || w.TemplateId == 20);
			var invoiceFile = templatedDocs.FirstOrDefault(w => w.TemplateId == 4 || w.TemplateId == 16 || w.TemplateId == 19); // счет

			if (accounting.IsIncome && (invoiceFile == null))
				return "Перед формированием акта необходимо сформировать счет!";

			int documentId = 0;
			if (document != null)
			{
				documentId = document.ID;
				document.ChangedDate = DateTime.Now;
				document.ChangedBy = CurrentUserId;
				documentLogic.UpdateTemplatedDocument(document);
			}
			else
			{
				var template = dataLogic.GetTemplate(17);   // обычный акт
				if (!accounting.IsIncome)
					template = dataLogic.GetTemplate(20);   // для расходы
				else if (legal.IsNotResident)
					template = dataLogic.GetTemplate(18);   // для нерезидентов

				document = new TemplatedDocument
				{
					TemplateId = template.ID,
					Filename = accounting.Number + "_" + template.Suffix,
					CreatedDate = DateTime.Now,
					CreatedBy = CurrentUserId,
					AccountingId = accountingId
				};

				documentId = documentLogic.CreateTemplatedDocument(document);
				document = documentLogic.GetTemplatedDocument(documentId);
			}

			var ffactory = new FileFactory();

			if (document.TemplateId == 17)
			{
				var result = ffactory.GenerateAct(document, TemplatedDocumentFormat.Pdf);
				if (!string.IsNullOrEmpty(result))
					return result;

				ffactory.GenerateAct(document, TemplatedDocumentFormat.CutPdf);
			}
			else
			{
				var result = ffactory.GenerateEnAct(document, TemplatedDocumentFormat.Pdf);
				if (!string.IsNullOrEmpty(result))
					return result;

				ffactory.GenerateEnAct(document, TemplatedDocumentFormat.CutPdf);
			}

			// при перегенерации акт сразу пересоздавать и сф
			if (accounting.IsIncome)
				return CreateVatInvoice_internal(accountingId);
			else
				return string.Empty;
		}

		public ContentResult CreateRequest(int accountingId)
		{
			var accounting = accountingLogic.GetAccounting(accountingId);
			if (accounting.IsIncome)
				return Content(JsonConvert.SerializeObject(new { Message = "Для ДОХОДА заявки не формируются." }));
			else
			{
				if (!accounting.ContractId.HasValue)
					return Content(JsonConvert.SerializeObject(new { Message = "Не выбран договор с поставщиком." }));
			}

			// проверка прав участника
			if (!participantLogic.IsAllowedActionByOrder(62, accounting.OrderId, CurrentUserId))
				return Content(JsonConvert.SerializeObject(new { ActionId = 62, Message = "У вас недостаточно прав на выполнение этого действия." }));

			var order = orderLogic.GetOrder(accounting.OrderId);
			var contract = contractLogic.GetContract(accounting.IsIncome ? order.ContractId.Value : accounting.ContractId.Value);
			var legal = legalLogic.GetLegal(accounting.IsIncome ? accounting.LegalId.Value : contract.LegalId);
			var ourLegal = legalLogic.GetLegalByOurLegal(contract.OurLegalId);
			var document = documentLogic.GetTemplatedDocumentsByAccounting(accountingId).FirstOrDefault(w => w.TemplateId == 1 || w.TemplateId == 3 || w.TemplateId == 7);

			int documentId = 0;
			if (document != null)
				documentId = document.ID;
			else
			{
				Template template;
				if (contract.ContractTypeId == 4)   // 4-договор страхования
					template = dataLogic.GetTemplate(1);
				else
				{
					if (ourLegal.IsNotResident)
					{
						template = dataLogic.GetTemplate(24);
					}
					else
					{
						if (legal.IsNotResident)
							template = dataLogic.GetTemplate(7);    // En
						else
							template = dataLogic.GetTemplate(3);
					}
				}

				document = new TemplatedDocument
				{
					TemplateId = template.ID,
					Filename = accounting.Number + "_" + template.Suffix,
					CreatedDate = DateTime.Now,
					CreatedBy = CurrentUserId,
					AccountingId = accountingId
				};

				documentId = documentLogic.CreateTemplatedDocument(document);
				document = documentLogic.GetTemplatedDocument(documentId);
			}

			//по заявке поставщику
			if ((document.TemplateId == 3) || (document.TemplateId == 7))
				foreach (var segment in orderLogic.GetRouteSegments(accounting.OrderId))
					if (!segment.TransportTypeId.HasValue)
						return Content(JsonConvert.SerializeObject(new { Message = "Не для всех этапов маршрута заполнен вид транспорта!" }));

			RecalculateTrash(accounting, orderLogic.GetRouteSegments(accounting.OrderId));
			accountingLogic.UpdateAccounting(accounting);

			var ffactory = new FileFactory();
			switch (document.TemplateId)
			{
				case 1: // заявление на страхование
					var result = ffactory.GenerateInsurance(document, TemplatedDocumentFormat.Pdf);
					if (!string.IsNullOrEmpty(result))
						return Content(JsonConvert.SerializeObject(new { Message = result }));

					ffactory.GenerateInsurance(document, TemplatedDocumentFormat.CleanPdf);
					ffactory.GenerateInsurance(document, TemplatedDocumentFormat.CutPdf);
					break;

				case 3: // заявление поставщика
					var result3 = ffactory.GenerateRequest(document, TemplatedDocumentFormat.Pdf);
					if (!string.IsNullOrEmpty(result3))
						return Content(JsonConvert.SerializeObject(new { Message = result3 }));

					ffactory.GenerateRequest(document, TemplatedDocumentFormat.CleanPdf);
					ffactory.GenerateRequest(document, TemplatedDocumentFormat.CutPdf);
					break;

				case 7: // заявление поставщика RU-EN
					var result7 = ffactory.GenerateEnRequest(document, TemplatedDocumentFormat.Pdf);
					if (!string.IsNullOrEmpty(result7))
						return Content(JsonConvert.SerializeObject(new { Message = result7 }));

					ffactory.GenerateEnRequest(document, TemplatedDocumentFormat.CleanPdf);
					ffactory.GenerateEnRequest(document, TemplatedDocumentFormat.CutPdf);
					break;
			}

			return Content(JsonConvert.SerializeObject(new { FileId = documentId }));
		}

		public ContentResult CloneOrder(int orderId, bool isCopyCargo, bool isCopyRoute, bool isCopyWorkgroup)
		{
			var order = orderLogic.GetOrder(orderId);

			#region проверить проверенность договора
			var contractMarks = contractLogic.GetContractMarkByContract(order.ContractId.Value);
			if ((contractMarks == null) || (!contractMarks.IsContractChecked))
				return Content(JsonConvert.SerializeObject(new { Message = "Договор не проверен. Обратитесь к администратору" }));
			#endregion

			var newOrder = new Order
			{
				ProductId = order.ProductId,
				OrderTemplateId = order.OrderTemplateId,
				FinRepCenterId = order.FinRepCenterId,
				OrderTypeId = order.OrderTypeId,
				OrderStatusId = 2,  // created
				CreatedBy = CurrentUserId,
				CreatedDate = DateTime.Now,
				RequestDate = DateTime.Now,
				CargoInfo = order.CargoInfo,
				EnCargoInfo = order.EnCargoInfo,
				CargoPrice = order.CargoPrice,
				ContractId = order.ContractId,
				Cost = order.Cost,
				Danger = order.Danger,
				EnDanger = order.EnDanger,
				EnSpecialCustody = order.EnSpecialCustody,
				EnTemperatureRegime = order.EnTemperatureRegime,
				InsuranceTypeId = order.InsuranceTypeId,
				IsPrintDetails = order.IsPrintDetails,
				SpecialCustody = order.SpecialCustody,
				TemperatureRegime = order.TemperatureRegime,
				UninsuranceTypeId = order.UninsuranceTypeId
			};

			var id = orderLogic.CreateOrder(newOrder);
			newOrder = orderLogic.GetOrder(id);

			newOrder.Number = "1" + id.ToString("D7");
			newOrder.RequestNumber = newOrder.Number;

			if (newOrder.ProductId.In(1, 2, 3, 8))
				newOrder.VolumetricRatioId = 1;

			if (newOrder.ProductId.In(4, 5, 6, 7, 9))
				newOrder.VolumetricRatioId = 2;

			var user = userLogic.GetUser(CurrentUserId);
			var employee = employeeLogic.GetEmployeesByPerson(user.PersonId.Value).FirstOrDefault(w => w.EmployeeStatusId == 1 && (w.LegalId < 3));
			if (employee != null)
				newOrder.FinRepCenterId = employee.FinRepCenterId;

			orderLogic.UpdateOrder(newOrder);

			// применение шаблона заказа
			if (newOrder.OrderTemplateId.HasValue)
			{
				var orderOperations = dataLogic.GetOrderOperationsByTemplate(newOrder.OrderTemplateId.Value);
				int index = 0;
				foreach (var item in orderOperations)
					orderLogic.CreateOperation(new Operation { OrderOperationId = item.ID, Name = item.Name, OrderId = id, No = index++, ResponsibleUserId = CurrentUserId });
			}

			// инициализация участников
			if (isCopyWorkgroup)
			{
				var wg = participantLogic.GetWorkgroupByOrder(orderId);
				foreach (var item in wg)
					participantLogic.CreateParticipant(new Participant { OrderId = id, UserId = item.UserId, ParticipantRoleId = item.ParticipantRoleId, FromDate = item.FromDate, ToDate = item.ToDate, IsResponsible = item.IsResponsible });
			}
			else
			{
				var contract = contractLogic.GetContract(newOrder.ContractId.Value);
				var legal = legalLogic.GetLegal(contract.LegalId);
				var wg = participantLogic.GetWorkgroupByContractor(legal.ContractorId.Value);
				foreach (var item in wg)
					participantLogic.CreateParticipant(new Participant { OrderId = id, UserId = item.UserId, ParticipantRoleId = item.ParticipantRoleId, FromDate = item.FromDate, ToDate = item.ToDate, IsResponsible = item.IsResponsible });
			}

			// инициализация груза
			if (isCopyCargo)
			{
				var cargo = orderLogic.GetCargoSeats(orderId);
				foreach (var item in cargo)
					orderLogic.CreateCargoSeat(new CargoSeat
					{
						OrderId = id,
						CargoDescriptionId = item.CargoDescriptionId,
						GrossWeight = item.GrossWeight,
						Height = item.Height,
						IsStacking = item.IsStacking,
						Length = item.Length,
						PackageTypeId = item.PackageTypeId,
						SeatCount = item.SeatCount,
						Volume = item.Volume,
						Width = item.Width
					});
			}

			// инициализация маршрута
			if (isCopyRoute)
			{
				var route = orderLogic.GetRoutePoints(orderId);
				foreach (var item in route)
					orderLogic.CreateRoutePoint(new RoutePoint
					{
						OrderId = id,
						Address = item.Address,
						Contact = item.Contact,
						EnAddress = item.EnAddress,
						EnContact = item.EnContact,
						EnParticipantComment = item.EnParticipantComment,
						No = item.No,
						ParticipantComment = item.ParticipantComment,
						ParticipantLegalId = item.ParticipantLegalId,
						PlaceId = item.PlaceId,
						PlanDate = item.PlanDate,
						RouteContactID = item.RouteContactID,
						RoutePointTypeId = item.RoutePointTypeId
					});

				RecalculateRoute(id);

				var segments = orderLogic.GetRouteSegments(orderId);
				var newSegments = orderLogic.GetRouteSegments(id);
				foreach (var segment in segments)
				{
					var seg = newSegments.FirstOrDefault(w => w.No == segment.No);
					if ((seg != null) && segment.IsAfterBorder)
					{
						seg.IsAfterBorder = segment.IsAfterBorder;
						orderLogic.UpdateRouteSegment(seg);
					}
				}
			}

			try
			{
				//создание директорий при сохранении заявки
				if (IO.Directory.Exists("\\\\corpserv03.corp.local/Common/5 Перевозки") == true)
				{
					if (IO.Directory.Exists("\\\\corpserv03.corp.local/Common/5 Перевозки/" + newOrder.Number) == false)
					{
						IO.Directory.CreateDirectory("\\\\corpserv03.corp.local/Common/5 Перевозки/" + newOrder.Number);
						IO.Directory.CreateDirectory("\\\\corpserv03.corp.local/Common/5 Перевозки/" + newOrder.Number + "/Docs");
						IO.Directory.CreateDirectory("\\\\corpserv03.corp.local/Common/5 Перевозки/" + newOrder.Number + "/Customer");
						IO.Directory.CreateDirectory("\\\\corpserv03.corp.local/Common/5 Перевозки/" + newOrder.Number + "/Supplier");
					}
				}
			}
			catch (Exception ex)
			{ }

			return Content(JsonConvert.SerializeObject(new { ID = id }));
		}

		#endregion

		public FileContentResult OpenTemplatedDocument(int id, string type)
		{
			var doc = documentLogic.GetTemplatedDocument(id);
			var data = documentLogic.GetTemplatedDocumentData(id, type);

			if (type == "pdf" || type == "cleanpdf" || type == "cutpdf")
				return File(data.Data, "application/pdf", doc.Filename + ".pdf");
			else
				return File(data.Data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", doc.Filename + ".xlsx");
		}

		public FileContentResult OpenPrintTemplatedDocument(int id, string type)
		{
			var contentType = "application/pdf";
			var doc = documentLogic.GetTemplatedDocument(id);
			var data = documentLogic.GetTemplatedDocumentData(id, type ?? "pdf");
			var result = Server.MapPath("~\\Temp\\print_" + Environment.TickCount + ".pdf");
			var filename = Server.MapPath("~\\Temp\\" + Environment.TickCount + ".pdf");
			using (var fs = new IO.FileStream(filename, IO.FileMode.OpenOrCreate, IO.FileAccess.Write))
			{
				using (var writer = new IO.BinaryWriter(fs))
				{
					writer.Write(data.Data);
					writer.Flush();
					writer.Close();
					fs.Close();
				}
			}

			var pdf = new PdfDocument();
			pdf.LoadFromFile(filename);
			pdf.AfterOpenAction = new PdfJavaScriptAction("this.print();");
			pdf.SaveToFile(result);

			Response.AppendHeader("Content-Disposition", "inline; filename=" + doc.AccountingId + "_print.pdf");
			return File(IO.File.ReadAllBytes(result), contentType);
		}

		//public FileContentResult OpenPrintTemplatedDocument(int id, string type)
		//{
		//	var contentType = "application/pdf";
		//	var doc = documentLogic.GetTemplatedDocument(id);
		//	var data = documentLogic.GetTemplatedDocumentData(id, type ?? "pdf");
		//	var result = Server.MapPath("~\\Temp\\print_" + id + ".pdf");

		//	var culture = Thread.CurrentThread.CurrentCulture;
		//	Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("ru-RU");

		//	var reader = new iTextSharp.text.pdf.PdfReader(TEMPLATED_DOCUMENTS_ROOT + data.ID);
		//	int pageCount = reader.NumberOfPages;
		//	var pageSize = reader.GetPageSize(1);
		//	var document = new iTextSharp.text.Document(pageSize);
		//	using (var fs = new IO.FileStream(result, IO.FileMode.OpenOrCreate, IO.FileAccess.Write))
		//	{
		//		var writer = iTextSharp.text.pdf.PdfWriter.GetInstance(document, fs);
		//		document.Open();

		//		// Copy each page
		//		var content = writer.DirectContent;
		//		for (int i = 0; i < pageCount; i++)
		//		{
		//			document.NewPage();
		//			var page = writer.GetImportedPage(reader, i + 1); // page numbers are one-based
		//			content.AddTemplate(page, 0, 0); // x and y correspond to position on the page?
		//		}

		//		writer.AddJavaScript(iTextSharp.text.pdf.PdfAction.JavaScript("this.print();", writer));
		//		document.Close();
		//		fs.Close();
		//	}

		//	Thread.CurrentThread.CurrentCulture = culture;
		//	Response.AppendHeader("Content-Disposition", "inline; filename=" + doc.Filename + ".pdf");
		//	return File(IO.File.ReadAllBytes(result), contentType);
		//}

		public FileContentResult OpenDocument(int id)
		{
			var doc = documentLogic.GetDocument(id);
			var docData = documentLogic.GetDocumentDataByDocument(id);
			var contentType = "application/octet-stream";
			if (doc.Filename.EndsWith(".pdf"))
				contentType = "application/pdf";

			return File(docData.Data, contentType, doc.Filename);
		}

		public FileContentResult OpenPrintDocument(int id)
		{
			var doc = documentLogic.GetDocument(id);
			var docData = documentLogic.GetDocumentDataByDocument(id);
			var contentType = "application/pdf";
			var result = Server.MapPath("~\\Temp\\print_" + id + ".pdf");
			//var pdf = new PdfDocument();
			//pdf.LoadFromFile(DOCUMENTS_ROOT + docData.ID);
			//pdf.AfterOpenAction = new PdfJavaScriptAction("this.print();");
			//pdf.SaveToFile(result);
			//return File(result, contentType, doc.Filename);

			var culture = Thread.CurrentThread.CurrentCulture;
			Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("ru-RU");

			var reader = new iTextSharp.text.pdf.PdfReader(DOCUMENTS_ROOT + docData.ID);
			int pageCount = reader.NumberOfPages;
			var pageSize = reader.GetPageSize(1);
			var document = new iTextSharp.text.Document(pageSize);
			using (var fs = new IO.FileStream(result, IO.FileMode.OpenOrCreate, IO.FileAccess.Write))
			{
				var writer = iTextSharp.text.pdf.PdfWriter.GetInstance(document, fs);
				document.Open();

				// Copy each page
				var content = writer.DirectContent;
				for (int i = 0; i < pageCount; i++)
				{
					document.NewPage();
					var page = writer.GetImportedPage(reader, i + 1); // page numbers are one-based
					content.AddTemplate(page, 0, 0); // x and y correspond to position on the page?
				}

				writer.AddJavaScript(iTextSharp.text.pdf.PdfAction.JavaScript("this.print();", writer));
				document.Close();
				fs.Close();
			}

			Thread.CurrentThread.CurrentCulture = culture;

			Response.AppendHeader("Content-Disposition", "inline; filename=" + doc.Filename);
			return File(IO.File.ReadAllBytes(result), contentType);
		}

		[OutputCache(NoStore = true, Duration = 0)]
		public FileResult OpenTemplatedDocumentPreview(int id)
		{
			var doc = documentLogic.GetTemplatedDocument(id);
			var data = documentLogic.GetTemplatedDocumentData(id, "xlsx");
			var contentType = "application/octet-stream";
			if (data == null || data.Data == null)
				return null;

			SpreadsheetInfo.SetLicense("FREE-LIMITED-KEY");
			try
			{
				var culture = Thread.CurrentThread.CurrentCulture;
				Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("ru-RU");
				var filename = Server.MapPath("~/Temp/" + doc.Filename + ".jpg");
				using (var stream = new IO.MemoryStream(data.Data))
				{
					ExcelFile ef = ExcelFile.Load(stream, LoadOptions.XlsxDefault);
					ef.Save(filename);
				}
				Thread.CurrentThread.CurrentCulture = culture;
				return File(filename, contentType);
			}
			catch (FreeLimitReachedException flex)
			{
				return File(Server.MapPath("~/img/freelimit.jpg"), contentType);
			}
		}

		[OutputCache(NoStore = true, Duration = 0)]
		public FileResult OpenDocumentPreview(int id, int? pageNo)
		{
			var doc = documentLogic.GetDocument(id);
			var docData = documentLogic.GetDocumentDataByDocument(id);
			var contentType = "application/octet-stream";

			if (doc.Filename.EndsWith(".pdf"))
			{
				// syncfusion
				PdfLoadedDocument p = new PdfLoadedDocument(docData.Data);

				var page = 0;
				if (pageNo > 0)
					page = pageNo.Value;

				if (p.Pages.Count <= page)
					return null;

				var mf = p.ExportAsImage(page);
				using (var stream = new IO.MemoryStream())
				{
					mf.Save(stream, ImageFormat.Jpeg);
					stream.Position = 0;
					return File(stream.GetBuffer(), contentType);
				}
			}

			//if (doc.Filename.EndsWith(".pdf"))
			//{
			//	// gnostice
			//	Gnostice.Documents.Framework.ActivateLicense("0DB3-2B34-B7BD-F22C-FB63-DD2F-CF46-A686-B812-CC13-37D8-C54E");
			//	var p = new Gnostice.Documents.PDF.PDF();
			//	p.Load(docData.Data);

			//	var page = 1;
			//	if (pageNo > 0)
			//		page = pageNo.Value + 1;

			//	if (p.GetPageCount() < page)
			//		return null;

			//	var mf = p.GetPageMetafile(page);
			//	using (var stream = new IO.MemoryStream())
			//	{
			//		mf.Save(stream, ImageFormat.Jpeg);
			//		stream.Position = 0;
			//		return File(stream.GetBuffer(), contentType);
			//	}
			//}

			if (doc.Filename.EndsWith(".xlsx"))
			{
				// gembox
				if (pageNo > 0)
					return null;

				var culture = Thread.CurrentThread.CurrentCulture;
				Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("ru-RU");
				SpreadsheetInfo.SetLicense("FREE-LIMITED-KEY");
				try
				{
					var filename = Server.MapPath("~/Temp/" + doc.Filename + pageNo + ".jpg");
					using (var stream = new IO.MemoryStream(docData.Data))
					{
						ExcelFile ef = ExcelFile.Load(stream, LoadOptions.XlsxDefault);
						ef.Save(filename);
					}

					Thread.CurrentThread.CurrentCulture = culture;
					return File(filename, contentType);
				}
				catch (FreeLimitReachedException flex)
				{
					return File(Server.MapPath("~/img/freelimit.jpg"), contentType);
				}
			}

			if (doc.Filename.EndsWith(".xls"))
			{
				if (pageNo > 0)
					return null;

				var culture = Thread.CurrentThread.CurrentCulture;
				Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("ru-RU");
				SpreadsheetInfo.SetLicense("FREE-LIMITED-KEY");
				try
				{
					using (var stream = new IO.MemoryStream(docData.Data))
					{
						ExcelFile ef = ExcelFile.Load(stream, LoadOptions.XlsDefault);
						var filename = Server.MapPath("~/Temp/" + doc.Filename + ".jpg");
						ef.Save(filename);
						Thread.CurrentThread.CurrentCulture = culture;
						return File(filename, contentType);
					}
				}
				catch (FreeLimitReachedException flex)
				{
					return File(Server.MapPath("~/img/freelimit.jpg"), contentType);
				}
			}

			//if (doc.Filename.EndsWith(".doc"))
			//{
			//	// TODO:
			//	GemBox.Document.ComponentInfo.SetLicense("FREE-LIMITED-KEY");
			//	GemBox.Document.ComponentInfo.FreeLimitReached += ComponentInfo_FreeLimitReached;

			//	try
			//	{
			//		using (var stream = new IO.MemoryStream(docData.Data))
			//		{
			//			var dm = GemBox.Document.DocumentModel.Load(stream, GemBox.Document.LoadOptions.DocDefault);
			//			GemBox.Document.ImageSaveOptions options = new GemBox.Document.ImageSaveOptions(GemBox.Document.ImageSaveFormat.Jpeg);

			//			var page = 0;
			//			if (pageNo > 0)
			//				page = pageNo.Value;

			//			if (dm.GetPaginator().Pages.Count - 1 < page)
			//				return null;

			//			options.PageNumber = page;
			//			using (var streamOut = new IO.MemoryStream())
			//			{
			//				//dm.Save(streamOut, options);
			//				//stream.Position = 0;
			//				//return File(stream.GetBuffer(), contentType);
			//				var filename = Server.MapPath("~/Temp/" + doc.Filename + pageNo + ".jpg");
			//				dm.Save(filename, options);
			//				return File(filename, contentType);
			//			}
			//		}
			//	}
			//	catch (FreeLimitReachedException flex)
			//	{
			//		return File(Server.MapPath("~/img/freelimit.jpg"), contentType);
			//	}
			//}

			//if (doc.Filename.EndsWith(".doc") || doc.Filename.EndsWith(".docx"))
			//{
			//	using (var stream = new IO.MemoryStream(docData.Data))
			//	{
			//		var doc2 = new Spire.Doc.Document(stream);

			//		var page = 0;
			//		if (pageNo > 0)
			//			page = pageNo.Value;

			//		if (doc2.PageCount - 1 < page)
			//			return null;

			//		var img = doc2.SaveToImages(page, ImageFormat.Jpeg);
			//		img.Position = 0;
			//		return File(((IO.MemoryStream)img).ToArray(), contentType);
			//	}
			//}

			if ((doc.Filename.EndsWith(".doc")) || (doc.Filename.EndsWith(".docx")))
			{
				// syncfusion
				using (var stream = new IO.MemoryStream(docData.Data))
				{
					// syncfusion key: @31342e342e30ORVrmhyyrkIb9NcZIThFIbmnOX2kDwPlz/qnAK//P5g=
					using (var docx = new Syncfusion.DocIO.DLS.WordDocument(stream))
					{
						try
						{
							var imgs = docx.RenderAsImages(Syncfusion.DocIO.DLS.ImageType.Bitmap);
							if (imgs.Length > (pageNo ?? 0))
								using (var bstream = new IO.MemoryStream())
								{
									imgs[pageNo ?? 0].Save(bstream, ImageFormat.Jpeg);
									bstream.Position = 0;
									return File(bstream.GetBuffer(), contentType);
								}
						}
						catch
						{
						}
					}
				}
			}

			return null;
		}

		[OutputCache(NoStore = true, Duration = 0)]
		public FileResult OpenMergedAccountingDocuments(int accountingId)
		{
			var accounting = accountingLogic.GetAccounting(accountingId);

			var order = orderLogic.GetOrder(accounting.OrderId);
			var contract = contractLogic.GetContract(order.ContractId.Value);
			var ourLegal = legalLogic.GetLegalByOurLegal(contract.OurLegalId);

			#region syncfusion

			// https://help.syncfusion.com/file-formats/pdf/merge-documents

			var merged = new Syncfusion.Pdf.PdfDocument();
			var docs = documentLogic.GetTemplatedDocumentsByAccounting(accountingId);
			foreach (var item in docs)
			{
				if (ourLegal.IsNotResident && !(item.TemplateId == 21))
					continue;

				var cutpdfData = documentLogic.GetTemplatedDocumentData(item.ID, "cutpdf");
				if (cutpdfData == null)
					continue;

				var pdf = new PdfLoadedDocument(cutpdfData.Data);

				merged.ImportPageRange(pdf, 0, pdf.Pages.Count - 1);

				if (item.TemplateId.In(17, 18, 20)) // акт 2 раза
					merged.ImportPageRange(pdf, 0, pdf.Pages.Count - 1);
			}

			merged.Actions.AfterOpen = new Syncfusion.Pdf.Interactive.PdfJavaScriptAction("this.print();");
			var result = Server.MapPath("~\\Temp\\merged" + accountingId + ".pdf");
			merged.Save(result);
			return File(result, "application/pdf", accounting.Number + "_printer.pdf");

			#endregion

			#region Spire

			//var merged = new PdfDocument();
			//var docs = documentLogic.GetTemplatedDocumentsByAccounting(accountingId);
			//foreach (var item in docs)
			//{
			//	if (ourLegal.IsNotResident && !(item.TemplateId == 21))
			//		continue;

			//	var cutpdf = documentLogic.GetTemplatedDocumentData(item.ID, "cutpdf");
			//	if (cutpdf == null)
			//		continue;

			//	var filename = Server.MapPath("~\\Temp\\" + item.Filename + ".pdf");
			//	using (var fs = new IO.FileStream(filename, IO.FileMode.OpenOrCreate, IO.FileAccess.Write))
			//	{
			//		using (var writer = new IO.BinaryWriter(fs))
			//		{
			//			writer.Write(cutpdf.Data);
			//			writer.Flush();
			//			writer.Close();
			//			fs.Close();
			//		}
			//	}

			//	var pdf = new PdfDocument();
			//	pdf.LoadFromFile(filename);

			//	merged.AppendPage(pdf);

			//	if (item.TemplateId.In(17, 18, 20)) // акт 2 раза
			//		merged.AppendPage(pdf);
			//}

			//merged.AfterOpenAction = new PdfJavaScriptAction("this.print();");
			//var result = Server.MapPath("~\\Temp\\merged" + accountingId + ".pdf");
			//merged.SaveToFile(result);
			//return File(result, "application/pdf", accounting.Number + "_printer.pdf");

			#endregion
		}

		[OutputCache(NoStore = true, Duration = 0)]
		public FileResult OpenClientAccountingDocuments(int accountingId)
		{
			var accounting = accountingLogic.GetAccounting(accountingId);
			var order = orderLogic.GetOrder(accounting.OrderId);
			var contract = contractLogic.GetContract(order.ContractId.Value);
			var ourLegal = legalLogic.GetLegalByOurLegal(contract.OurLegalId);

			var merged = new PdfDocument();
			var docs = documentLogic.GetTemplatedDocumentsByAccounting(accountingId);
			foreach (var item in docs)
			{
				if (ourLegal.IsNotResident && !(item.TemplateId == 21))
					continue;

				var srcPdf = documentLogic.GetTemplatedDocumentData(item.ID, "pdf");

				if (item.TemplateId.In(17, 18, 20)) // акт
					srcPdf = documentLogic.GetTemplatedDocumentData(item.ID, "cleanpdf") ?? srcPdf;

				if (srcPdf == null)
					continue;

				var filename = Server.MapPath("~\\Temp\\" + item.Filename + ".pdf");
				using (var fs = new IO.FileStream(filename, IO.FileMode.OpenOrCreate, IO.FileAccess.Write))
				{
					using (var writer = new IO.BinaryWriter(fs))
					{
						writer.Write(srcPdf.Data);
						writer.Flush();
						writer.Close();
						fs.Close();
					}
				}

				var pdf = new PdfDocument();
				pdf.LoadFromFile(filename);
				merged.AppendPage(pdf);
			}

			var result = Server.MapPath("~\\Temp\\merged" + accountingId + ".pdf");
			merged.SaveToFile(result);
			return File(result, "application/pdf", accounting.Number + "_documents.pdf");
		}

		public ContentResult RegenerateAccountingDocuments(int accountingId)
		{
			var ffactory = new FileFactory();
			var list = documentLogic.GetTemplatedDocumentsByAccounting(accountingId);
			foreach (var document in list)
			{
				document.ChangedDate = DateTime.Now;
				document.ChangedBy = CurrentUserId;
				documentLogic.UpdateTemplatedDocument(document);

				switch (document.TemplateId)
				{
					case 1:
						if (string.IsNullOrEmpty(ffactory.GenerateInsurance(document, TemplatedDocumentFormat.Pdf)))
						{
							ffactory.GenerateInsurance(document, TemplatedDocumentFormat.CleanPdf);
							ffactory.GenerateInsurance(document, TemplatedDocumentFormat.CutPdf);
						}
						break;

					case 3:
						if (string.IsNullOrEmpty(ffactory.GenerateRequest(document, TemplatedDocumentFormat.Pdf)))
						{
							ffactory.GenerateRequest(document, TemplatedDocumentFormat.CleanPdf);
							ffactory.GenerateRequest(document, TemplatedDocumentFormat.CutPdf);
						}
						break;

					case 5:
						if (string.IsNullOrEmpty(ffactory.GenerateDetails(document, TemplatedDocumentFormat.Pdf)))
						{
							ffactory.GenerateDetails(document, TemplatedDocumentFormat.CleanPdf);
							ffactory.GenerateDetails(document, TemplatedDocumentFormat.CutPdf);
						}
						break;

					case 7:
						if (string.IsNullOrEmpty(ffactory.GenerateEnRequest(document, TemplatedDocumentFormat.Pdf)))
						{
							ffactory.GenerateEnRequest(document, TemplatedDocumentFormat.CleanPdf);
							ffactory.GenerateEnRequest(document, TemplatedDocumentFormat.CutPdf);
						}
						break;

					case 15:
						if (string.IsNullOrEmpty(ffactory.GenerateVatInvoice(document, TemplatedDocumentFormat.Pdf)))
						{
							ffactory.GenerateVatInvoice(document, TemplatedDocumentFormat.CleanPdf);
							ffactory.GenerateVatInvoice(document, TemplatedDocumentFormat.CutPdf);
						}
						break;

					case 16:
						if (string.IsNullOrEmpty(ffactory.GenerateInvoice(document, TemplatedDocumentFormat.Pdf)))
						{
							ffactory.GenerateInvoice(document, TemplatedDocumentFormat.CleanPdf);
							ffactory.GenerateInvoice(document, TemplatedDocumentFormat.CutPdf);
						}
						break;

					case 17:
						if (string.IsNullOrEmpty(ffactory.GenerateAct(document, TemplatedDocumentFormat.Pdf)))
						{
							ffactory.GenerateAct(document, TemplatedDocumentFormat.CleanPdf);
							ffactory.GenerateAct(document, TemplatedDocumentFormat.CutPdf);
						}
						break;

					case 18:
						if (string.IsNullOrEmpty(ffactory.GenerateEnAct(document, TemplatedDocumentFormat.Pdf)))
						{
							ffactory.GenerateEnAct(document, TemplatedDocumentFormat.CleanPdf);
							ffactory.GenerateEnAct(document, TemplatedDocumentFormat.CutPdf);
						}
						break;

					case 19:
						if (string.IsNullOrEmpty(ffactory.GenerateEnInvoice(document, TemplatedDocumentFormat.Pdf)))
						{
							ffactory.GenerateEnInvoice(document, TemplatedDocumentFormat.CleanPdf);
							ffactory.GenerateEnInvoice(document, TemplatedDocumentFormat.CutPdf);
						}
						break;

					default:
						break;
				}
			}

			if (list.Count() > 0)
			{
				var accounting = accountingLogic.GetAccounting(accountingId);
				var workgroup = participantLogic.GetWorkgroupByOrder(accounting.OrderId);
				var subject = "Документы обновлены";
				var message = "{0}, документы в <a href='http://cm.corp.local/Orders/Details/{1}?selectedAccountingId={2}'>{3}</a> были обновлены в связи с изменением курса пересчета. Спасибо.";

				foreach (var item in GetParticipantUsers(workgroup, ParticipantRoles.AM, ParticipantRoles.BUH))
				{
					var user = identityLogic.GetUser(item);
					var fm = string.Format(message, user.Name, accounting.OrderId, accounting.ID, accounting.Number);
					SendMail(user.Email, subject, fm);
				}
			}

			return Content(JsonConvert.SerializeObject(""));
		}

		public ContentResult CalculateRouteLength(int orderId)
		{
			var list = orderLogic.GetRouteSegments(orderId).ToList();

			bool isPrintDetails = false;
			double lenghtBeforeBoard = 0;
			double lenghtAfterBoard = 0;
			string routeBeforeBoard = "";
			string routeAfterBoard = "";

			foreach (var segment in list)
			{
				if (!segment.Length.HasValue)
					return Content(JsonConvert.SerializeObject(new { Message = "Не для всех этапов маршрута заполнено расстояние! Сначала необходимо указать расстояние для каждого этапа маршрута." }));

				if (segment.IsAfterBorder)
				{
					isPrintDetails = true;
					lenghtAfterBoard = lenghtAfterBoard + segment.Length.Value;

					if (string.IsNullOrEmpty(routeAfterBoard))
					{
						var point = orderLogic.GetRoutePoint(segment.FromRoutePointId);
						var place = dataLogic.GetPlace(point.PlaceId.Value);
						var country = dataLogic.GetCountry(place.CountryId.Value);
						routeAfterBoard = country.Name + ", " + place.Name;
					}

					var pointTo = orderLogic.GetRoutePoint(segment.ToRoutePointId);
					var placeTo = dataLogic.GetPlace(pointTo.PlaceId.Value);
					var countryTo = dataLogic.GetCountry(placeTo.CountryId.Value);
					routeAfterBoard += " - " + countryTo.Name + ", " + placeTo.Name;
				}
				else
				{
					lenghtBeforeBoard = lenghtBeforeBoard + segment.Length.Value;
					if (string.IsNullOrEmpty(routeBeforeBoard))
					{
						var point = orderLogic.GetRoutePoint(segment.FromRoutePointId);
						var place = dataLogic.GetPlace(point.PlaceId.Value);
						var country = dataLogic.GetCountry(place.CountryId.Value);
						routeBeforeBoard = country.Name + ", " + place.Name;
					}

					var pointTo = orderLogic.GetRoutePoint(segment.ToRoutePointId);
					var placeTo = dataLogic.GetPlace(pointTo.PlaceId.Value);
					var countryTo = dataLogic.GetCountry(placeTo.CountryId.Value);
					routeBeforeBoard += " - " + countryTo.Name + ", " + placeTo.Name;
				}
			}

			var order = orderLogic.GetOrder(orderId);

			order.IsPrintDetails = isPrintDetails;
			order.RouteBeforeBoard = routeBeforeBoard;
			order.RouteAfterBoard = routeAfterBoard;
			order.RouteLengthBeforeBoard = lenghtBeforeBoard;
			order.RouteLengthAfterBoard = lenghtAfterBoard;

			orderLogic.UpdateOrder(order);

			return Content(JsonConvert.SerializeObject(new
			{
				Message = isPrintDetails ? "" : "Так как участки маршрута после границы отсутствуют, детализация к счету не формируется!",
				RouteBeforeBoard = routeBeforeBoard,
				RouteAfterBoard = routeAfterBoard,
				RouteLengthBeforeBoard = lenghtBeforeBoard,
				RouteLengthAfterBoard = lenghtAfterBoard
			}));
		}

		public ContentResult RecalculateRoute(int orderId)
		{
			var order = orderLogic.GetOrder(orderId);
			var routePoints = orderLogic.GetRoutePoints(orderId).OrderBy(o => o.No).ToList();
			var routeSegments = orderLogic.GetRouteSegments(orderId).ToList();

			RoutePoint pointFrom = null;
			RoutePoint pointTo = null;
			RouteSegment segment;

			int currentSection = 1;
			bool isFirstPoint = true;

			string strGOnameINN = "";
			string strGOcontact = "";
			string strGOcontTel = "";
			string strGOtime = "";
			string strGOcomment = "";
			string strGPnameINN = "";
			string strGPcontact = "";
			string strGPcontTel = "";
			string strGPtime = "";
			string strGPcomment = "";
			string strGOname = "";
			string strGPname = "";
			string route = "";
			string strExpTO = "";
			string strImpTO = "";
			string strFirspPointGO = "";
			string strFirstPointGP = "";

			int GOCounter = 0;
			int GPCounter = 0;

			if (routePoints.Count == 0)
				order.From = "";

			foreach (var currentPoint in routePoints)
			{
				if (!currentPoint.PlaceId.HasValue)
					return Content(JsonConvert.SerializeObject(new { Message = "В маршрутной точке " + currentPoint.No + " не выбран пункт/место!" }));

				var place = dataLogic.GetPlace(currentPoint.PlaceId.Value);
				var country = dataLogic.GetCountry(place.CountryId.Value);

				if (isFirstPoint)
				{
					pointTo = currentPoint;
					isFirstPoint = false;
					order.From = country.Name + ", " + place.Name;
					order.LoadingDate = currentPoint.PlanDate;
				}
				else
				{
					pointFrom = pointTo;
					pointTo = currentPoint;
					segment = routeSegments.Where(w => w.FromRoutePointId == pointFrom.ID && w.ToRoutePointId == pointTo.ID).FirstOrDefault();
					if (segment == null)
						segment = routeSegments.Where(w => w.FromRoutePointId == pointFrom.ID).FirstOrDefault();

					if (segment == null)
						segment = routeSegments.Where(w => w.ToRoutePointId == pointTo.ID).FirstOrDefault();

					if (segment == null)
						segment = orderLogic.GetRouteSegment(orderLogic.CreateRouteSegment(new RouteSegment { OrderId = order.ID }));

					segment.No = currentSection;
					segment.FromRoutePointId = pointFrom.ID;
					segment.ToRoutePointId = pointTo.ID;

					#region Подставить длину из последнего сегмента в БД

					if (!segment.Length.HasValue && pointFrom.PlaceId.HasValue && pointTo.PlaceId.HasValue)
					{
						var rs = dataLogic.GetLastRouteSegment(pointFrom.PlaceId.Value, pointTo.PlaceId.Value);
						if (rs != null)
							segment.Length = rs.Length;
					}

					#endregion

					orderLogic.UpdateRouteSegment(segment);
					routeSegments.RemoveAll(w => w.ID == segment.ID);

					currentSection++;
				}

				if (currentPoint.RoutePointTypeId == 1 || currentPoint.RoutePointTypeId == 3)   // Пункт загрузки || Пункт разгрузки
				{
					if (route != "")
						route = route + ";\n";

					var type = dataLogic.GetRoutePointTypes().First(w => w.ID == currentPoint.RoutePointTypeId);
					route = route + type.Display + ": " + country.Name + ", " + place.Name + ", " + currentPoint.Address;
				}

				if (currentPoint.RoutePointTypeId == 5) // Экспортное ТО
				{
					if (strExpTO != "")
						strExpTO = strExpTO + "; ";

					strExpTO = strExpTO + currentPoint.Address;
				}

				if (currentPoint.RoutePointTypeId == 6) // Импортное ТО
				{
					if (strImpTO != "")
						strImpTO = strImpTO + "; ";

					strImpTO = strImpTO + currentPoint.Address;
				}

				if (currentPoint.ParticipantLegalId.HasValue)
				{
					var legal = legalLogic.GetLegal(currentPoint.ParticipantLegalId.Value);

					//заполняем грузоотправителей
					if (currentPoint.RoutePointTypeId == 1) // Пункт загрузки
					{
						if (GOCounter == 0)
						{
							strFirspPointGO = place.Name;

							strGOnameINN = legal.DisplayName + ", ИНН " + legal.TIN;
							if (!currentPoint.RouteContactID.HasValue)
								return Content(JsonConvert.SerializeObject(new { Message = "Не выбраны контакты грузоотправителя! " + currentPoint.No }));

							var routeContact = legalLogic.GetRouteContact(currentPoint.RouteContactID.Value);
							strGOcontact = routeContact.Contact;
							strGOcontTel = routeContact.Phones;
							strGOtime = legal.WorkTime;
							strGOcomment = currentPoint.ParticipantComment;
							strGOname = legal.DisplayName;
						}
						else
						{
							if (strGOnameINN != "")
							{ strGOnameINN = strGOnameINN + "; "; }
							if (strGOcontact != "")
							{ strGOcontact = strGOcontact + "; "; }
							if (strGOcontTel != "")
							{ strGOcontTel = strGOcontTel + "; "; }
							if (strGOtime != "")
							{ strGOtime = strGOtime + "; "; }
							if (strGOcomment != "")
							{ strGOcomment = strGOcomment + "; "; }
							if (strGOname != "")
							{ strGOname = strGOname + "; "; }

							strGOnameINN = strGOnameINN + place.Name + ":" + legal.DisplayName + ", ИНН " + legal.TIN;

							if (!currentPoint.RouteContactID.HasValue)
								return Content(JsonConvert.SerializeObject(new { Message = "Не выбраны контакты грузоотправителя! " + currentPoint.No }));

							var routeContact = legalLogic.GetRouteContact(currentPoint.RouteContactID.Value);
							strGOcontact = strGOcontact + place.Name + ":" + routeContact.Contact;
							strGOcontTel = strGOcontTel + place.Name + ":" + routeContact.Phones;
							strGOtime = strGOtime + place.Name + ":" + legal.WorkTime;
							strGOcomment = strGOcomment + place.Name + ":" + currentPoint.ParticipantComment;
							strGOname = strGOname + "; " + legal.DisplayName;
						}

						GOCounter++;
					}

					//заполняем грузополучаетелей
					if (currentPoint.RoutePointTypeId == 3) // Пункт разгрузки
					{
						if (GPCounter == 0)
						{
							strFirstPointGP = place.Name;
							strGPnameINN = legal.DisplayName + ", ИНН " + legal.TIN;
							if (!currentPoint.RouteContactID.HasValue)
								return Content(JsonConvert.SerializeObject(new { Message = "Не выбраны контакты грузополучателя! " + currentPoint.No }));

							var routeContact = legalLogic.GetRouteContact(currentPoint.RouteContactID.Value);
							strGPcontact = routeContact.Contact;
							strGPcontTel = routeContact.Phones;
							strGPtime = legal.WorkTime;
							strGPcomment = currentPoint.ParticipantComment;
							strGPname = legal.DisplayName;
						}
						else
						{
							if (strGPnameINN != "")
							{ strGPnameINN = strGPnameINN + "; "; }
							if (strGPcontact != "")
							{ strGPcontact = strGPcontact + "; "; }
							if (strGPcontTel != "")
							{ strGPcontTel = strGPcontTel + "; "; }
							if (strGPtime != "")
							{ strGPtime = strGPtime + "; "; }
							if (strGPcomment != "")
							{ strGPcomment = strGPcomment + "; "; }
							if (strGPname != "")
							{ strGPname = strGPname + "; "; }

							strGPnameINN = strGPnameINN + place.Name + ":" + legal.DisplayName + ", ИНН " + legal.TIN;
							if (!currentPoint.RouteContactID.HasValue)
								return Content(JsonConvert.SerializeObject(new { Message = "Не выбраны контакты грузополучателя! " + currentPoint.No }));

							var routeContact = legalLogic.GetRouteContact(currentPoint.RouteContactID.Value);
							strGPcontact = strGPcontact + place.Name + ":" + routeContact.Contact;
							strGPcontTel = strGPcontTel + place.Name + ":" + routeContact.Phones;
							strGPtime = strGPtime + place.Name + ":" + legal.WorkTime;
							strGPcomment = strGPcomment + place.Name + ":" + currentPoint.ParticipantComment;
							strGPname = strGPname + "; " + legal.DisplayName;
						}

						GPCounter++;
					}
				}
			}

			// удалить неиспользованные сегменты (радикально поменялся маршрут или сократился и это хвост)
			foreach (var item in routeSegments)
				orderLogic.DeleteRouteSegment(item.ID);

			if (pointTo != null)
			{
				var toPlace = dataLogic.GetPlace(pointTo.PlaceId.Value);
				var toCountry = dataLogic.GetCountry(toPlace.CountryId.Value);

				order.To = toCountry.Name + ", " + toPlace.Name;
				order.UnloadingDate = pointTo.PlanDate;
			}
			else
				order.To = "";


			if (GOCounter > 1)
			{
				order.zkzGOnameINN = strFirspPointGO + ": " + strGOnameINN;
				order.zkzGOcontact = strFirspPointGO + ": " + strGOcontact;
				order.zkzGOcontTel = strFirspPointGO + ": " + strGOcontTel;
				order.zkzGOtime = strFirspPointGO + ": " + strGOtime;
				order.zkzGOcomment = strFirspPointGO + ": " + strGOcomment;
			}
			else
			{
				order.zkzGOnameINN = strGOnameINN;
				order.zkzGOcontact = strGOcontact;
				order.zkzGOcontTel = strGOcontTel;
				order.zkzGOtime = strGOtime;
				order.zkzGOcomment = strGOcomment;
			}

			if (GPCounter > 1)
			{
				order.zkzGPnameINN = strFirstPointGP + ": " + strGPnameINN;
				order.zkzGPcontact = strFirstPointGP + ": " + strGPcontact;
				order.zkzGPconTel = strFirstPointGP + ": " + strGPcontTel;
				order.zkzGPtime = strFirstPointGP + ": " + strGPtime;
				order.zkzGPcomment = strFirstPointGP + ": " + strGPcomment;
			}
			else
			{
				order.zkzGPnameINN = strGPnameINN;
				order.zkzGPcontact = strGPcontact;
				order.zkzGPconTel = strGPcontTel;
				order.zkzGPtime = strGPtime;
				order.zkzGPcomment = strGPcomment;
			}

			order.zkzGOname = strGOname;
			order.zkzGPname = strGPname;
			order.zkzMarshrut = route;
			order.zkzExpTO = strExpTO;
			order.zkzImpTO = strImpTO;

			orderLogic.UpdateOrder(order);

			return Content(JsonConvert.SerializeObject(new { Order = order }));
		}

		public ContentResult RecalculateAccountingSum(int accountingId)
		{
			accountingLogic.CalculateAccountingBalance(accountingId);
			var accounting = accountingLogic.GetAccounting(accountingId);
			return Content(JsonConvert.SerializeObject(new { OriginalSum = accounting.OriginalSum, OriginalVat = accounting.OriginalVat, Sum = accounting.Sum, Vat = Math.Round(accounting.Vat.Value, 4) }));
		}

		public ContentResult RecalculateOrderCargoSeats(int orderId)
		{
			orderLogic.CalculateCargoSeats(orderId);
			var order = orderLogic.GetOrder(orderId);
			return Content(JsonConvert.SerializeObject(new { SeatsCount = order.SeatsCount, GrossWeight = order.GrossWeight, PaidWeight = order.PaidWeight, Volume = order.Volume }));
		}

		public ContentResult RecalculatePaymentPlanDate(int accountingId)
		{
			var result = accountingLogic.CalculatePaymentPlanDate(accountingId);
			if (!string.IsNullOrEmpty(result))
				return Content(JsonConvert.SerializeObject(new { Message = result }));

			var accounting = accountingLogic.GetAccounting(accountingId);

			return Content(JsonConvert.SerializeObject(new { PaymentPlanDate = accounting.PaymentPlanDate }));
		}

		public ContentResult RecalculatePricelist(int orderId, int contractId)
		{
			var order = orderLogic.GetOrder(orderId);
			order.ContractId = contractId;
			var pricelistId = GetPricelist(order)?.ID ?? 0;
			return Content(JsonConvert.SerializeObject(new { PricelistId = pricelistId }));
		}

		public ContentResult ToggleDocumentIsPrint(int documentId, bool isPrint)
		{
			// обновить позицию
			var document = documentLogic.GetDocument(documentId);
			document.IsPrint = isPrint;
			documentLogic.UpdateDocument(document, CurrentUserId);
			return Content(JsonConvert.SerializeObject(""));
		}

		public ContentResult CheckDocumentsIsPrintLimit(int orderId)
		{
			var count = documentLogic.GetDocumentsByOrder(orderId).Count(w => w.IsPrint == true);
			return Content(JsonConvert.SerializeObject(new { Count = count }));
		}

		public ContentResult CheckStatusRules(int orderId, int orderStatusId)
		{
			var errors = new List<string>();
			var order = orderLogic.GetOrder(orderId);

			if (orderStatusId == 5) // Расходы внесены
			{
				CheckOrderExpenseMarksRejected(order, errors);
			}

			if (orderStatusId == 6) // проверен
			{
				CheckOrderIncomeMarksChecked(order, errors);
				CheckOrderExpenseMarksChecked(order, errors);
			}

			if (orderStatusId == 9) // закрыт
			{
				switch (order.ProductId)
				{
					case 1:
					case 2:
					case 3:
					case 4:
					case 6:
					case 7:
						CheckOrderCargoInfo(order, errors);
						CheckOrderSeats(order, errors);
						CheckOrderRoute(order, errors);
						CheckOrderRouteSegments(order, errors);
						CheckOrderIncomeAccountings(order, errors);
						CheckOrderExpenseAccountings(order, errors);
						CheckOrderIncomeMarksOk(order, errors);
						CheckOrderExpenseMarksOk(order, errors);
						CheckOrderClaim(order, errors);
						CheckOrderDocuments(order, errors);
						CheckOrderIncomeMarksRejected(order, errors);
						break;

					case 5: // АВТО МЕЖДУНАРОДНЫЕ
						CheckOrderCargoInfo(order, errors);
						CheckOrderSeats(order, errors);
						CheckOrderRoute(order, errors);
						CheckOrderRouteBorder(order, errors);
						CheckOrderRouteSegments(order, errors);
						CheckOrderIncomeAccountings(order, errors);
						CheckOrderExpenseAccountings(order, errors);
						CheckOrderIncomeMarksOk(order, errors);
						CheckOrderExpenseMarksOk(order, errors);
						CheckOrderClaim(order, errors);
						CheckOrderDocuments(order, errors);
						CheckOrderIncomeMarksRejected(order, errors);
						break;

					case 8: // ТАМОЖНЯ АВИА
					case 9: // ТАМОЖНЯ НАЗЕМНАЯ
						CheckOrderCargoInfo(order, errors);
						CheckOrderSeats(order, errors);
						CheckOrderIncomeAccountings(order, errors);
						CheckOrderExpenseAccountings(order, errors);
						CheckOrderIncomeMarksOk(order, errors);
						CheckOrderExpenseMarksOk(order, errors);
						CheckOrderClaim(order, errors);
						CheckOrderDocuments(order, errors);
						CheckOrderIncomeMarksRejected(order, errors);
						break;

					case 10:    // ТОРГОВОЕ АГЕНТИРОВАНИЕ
						CheckOrderCargoInfo(order, errors);
						CheckOrderSeats(order, errors);
						CheckOrderRoute(order, errors);
						CheckOrderRouteSegments(order, errors);
						CheckOrderIncomeAccountings(order, errors);
						CheckOrderExpenseAccountings(order, errors);
						CheckOrderIncomeMarksOk(order, errors);
						CheckOrderExpenseMarksOk(order, errors);
						CheckOrderSpecification(order, errors);
						CheckOrderDocuments(order, errors);
						CheckOrderIncomeMarksRejected(order, errors);
						break;

					case 11:    // СКЛАД
						CheckOrderIncomeAccountings(order, errors);
						CheckOrderExpenseAccountings(order, errors);
						CheckOrderIncomeMarksOk(order, errors);
						CheckOrderExpenseMarksOk(order, errors);
						CheckOrderDocuments(order, errors);
						CheckOrderIncomeMarksRejected(order, errors);
						break;
				}
			}

			return Content(JsonConvert.SerializeObject(new { Errors = errors }));
		}

		public ContentResult IsHasVatInvoice(int legalId)
		{
			var legal = legalLogic.GetLegal(legalId);
			var result = (!legal.IsNotResident) && (legal.TaxTypeId == 1);
			return Content(JsonConvert.SerializeObject(new { IsHasVatInvoice = result }));
		}

		public ContentResult GetRoutePoint(int routePointId)
		{
			var s = orderLogic.GetRoutePoint(routePointId);
			var result = new RoutePointViewModel
			{
				ID = s.ID,
				OrderId = s.OrderId,
				No = s.No,
				RoutePointTypeId = s.RoutePointTypeId,
				PlaceId = s.PlaceId,
				PlanDate = s.PlanDate,
				FactDate = s.FactDate,
				Address = s.Address,
				EnAddress = s.EnAddress,
				Contact = s.Contact,
				EnContact = s.EnContact,
				ParticipantLegalId = s.ParticipantLegalId,
				ParticipantComment = s.ParticipantComment,
				RouteContactID = s.RouteContactID,
				EnParticipantComment = s.EnParticipantComment,

				Place = s.PlaceId.HasValue ? dataLogic.GetPlace(s.PlaceId.Value).Name : ""
			};

			return Content(JsonConvert.SerializeObject(result));
		}

		public ContentResult GetOrderBalance(int orderId, bool? recalculate)
		{
			if (recalculate.HasValue && recalculate.Value)
				orderLogic.CalculateOrderBalance(orderId);

			var order = orderLogic.GetOrder(orderId);
			var rentability = ((order.Balance ?? 0) / (order.Income ?? 1)) * 100;
			var minRentability = dataLogic.GetOrdersRentability().Where(w => w.FinRepCenterId == order.FinRepCenterId)
													.Where(w => w.OrderTemplateId == order.OrderTemplateId)
													.Where(w => w.ProductId == order.ProductId)
													.Where(w => w.Year == order.CreatedDate.Value.Year)
													.Select(s => s.Rentability).FirstOrDefault();
			return Content(JsonConvert.SerializeObject(new { Income = order.Income, Expense = order.Expense, Balance = order.Balance, Rentability = rentability, MinRentability = minRentability }));
		}

		public ContentResult GetPrice(int serviceTypeId, int accountingId)
		{
			var accounting = accountingLogic.GetAccounting(accountingId);
			var order = orderLogic.GetOrder(accounting.OrderId);
			var contractId = accounting.IsIncome ? order.ContractId.Value : accounting.ContractId.Value;
			var contract = contractLogic.GetContract(contractId);
			var legal = accounting.IsIncome ? legalLogic.GetLegal(accounting.LegalId.Value) : legalLogic.GetLegal(contract.LegalId);
			var finRepCenterId = order.FinRepCenterId.Value;
			var productId = order.ProductId;

			var pricelists = pricelistLogic.GetValidPricelists().Where(w => (w.FinRepCenterId == finRepCenterId) && (w.ProductId == productId));

			var pricelist = pricelists.FirstOrDefault(w => w.ContractId == contractId);
			if (pricelist == null)
				pricelist = pricelists.FirstOrDefault(w => !w.ContractId.HasValue);

			if (pricelist != null)
			{
				var price = pricelistLogic.GetPrices(pricelist.ID).FirstOrDefault(w => w.ServiceId == serviceTypeId);

				// TEMP: не должно быть NULL
				if (price == null)
					return Content(JsonConvert.SerializeObject(new { Price = 0, VatId = 2/*Без НДС*/, Count = 1, CurrencyId = 1 }));

				return Content(JsonConvert.SerializeObject(new { Price = accounting.IsIncome ? price.Sum : 0, VatId = legal.IsNotResident ? 2/*Без НДС*/ : price.VatId, price.Count, price.CurrencyId }));
			}

			var serviceType = dataLogic.GetServiceType(serviceTypeId);
			var serviceKind = dataLogic.GetServiceKind(serviceType.ServiceKindId);
			return Content(JsonConvert.SerializeObject(new { Price = accounting.IsIncome ? serviceType.Price : 0, VatId = legal.IsNotResident ? 2/*Без НДС*/ : serviceKind.VatId, Count = serviceType.Count ?? 1 }));
		}

		public ContentResult SetCurrencyRate(int accountingId, double? newRate)
		{
			var accounting = accountingLogic.GetAccounting(accountingId);

			if (!accounting.IsIncome)
				return Content(JsonConvert.SerializeObject(new { Message = "Курс пересчета устанавливается только для доходов." }));

			if (accounting.CurrencyRate == newRate)
				return Content(JsonConvert.SerializeObject(new { Message = "Курс не изменился." }));

			// TODO: roles !!! https://pyrus.com/t#id10856308
			// менять курс пересчета может только ответственный по заказу АМ, BUH, GM до статуса Проверен. Далее курс пересчета можно изменить только через статус Корректировка.
			var order = orderLogic.GetOrder(accounting.OrderId);
			if (order.OrderStatusId.In(6, 7, 9))
				return Content(JsonConvert.SerializeObject(new { Message = "Нельзя менять курс для заказа в таком статусе." }));

			accounting.CurrencyRate = newRate;
			accountingLogic.UpdateAccounting(accounting);

			if (accounting.CurrencyRate.HasValue)
				// поменялся курс пересчета, пересчитать услуги (без привлечения CurrencyRateUse)
				foreach (var service in accountingLogic.GetServicesByAccounting(accounting.ID))
				{
					service.Sum = Math.Round(service.OriginalSum.Value * accounting.CurrencyRate.Value, 4);
					accountingLogic.UpdateService(service);
				}
			else
				;   // TODO:

			accountingLogic.CalculateAccountingBalance(accounting.ID);

			return Content(JsonConvert.SerializeObject(""));
		}

		#region Save

		public ContentResult SaveOrder(OrderEditModel model)
		{
			var order = orderLogic.GetOrder(model.ID);

			#region проверка прав участника

			if (!participantLogic.IsAllowedActionByOrder(2, model.ID, CurrentUserId))
				return Content(JsonConvert.SerializeObject(new { ActionId = 2, Message = "У вас нет прав на выполнение этого действия" }));

			#endregion
			#region проверить проверенность договора

			var contractMarks = contractLogic.GetContractMarkByContract(model.ContractId.Value);
			if ((contractMarks == null) || (!contractMarks.IsContractChecked))
				return Content(JsonConvert.SerializeObject(new { Message = "Договор не проверен. Обратитесь к администратору" }));

			#endregion

			model.InvoiceDate = model.InvoiceDate?.ToUniversalTime();
			model.RequestDate = model.RequestDate.ToUniversalTime();

			bool isOrderChanged = order.ProductId != model.ProductId ||
									order.ContractId != model.ContractId ||
									order.RequestNumber != model.RequestNumber ||
									order.OrderTypeId != model.OrderTypeId ||
									order.SpecialCustody != model.SpecialCustody ||
									order.EnSpecialCustody != model.EnSpecialCustody ||
									order.VolumetricRatioId != model.VolumetricRatioId ||
									order.Danger != model.Danger ||
									order.EnDanger != model.EnDanger ||
									order.PaidWeight != model.PaidWeight ||
									order.TemperatureRegime != model.TemperatureRegime ||
									order.EnTemperatureRegime != model.EnTemperatureRegime ||
									order.InsurancePolicy != model.InsurancePolicy ||
									order.InsuranceTypeId != model.InsuranceTypeId ||
									order.UninsuranceTypeId != model.UninsuranceTypeId ||
									order.RequestDate.Date != model.RequestDate.Date ||
									order.InvoiceDate?.Date != (model.InvoiceDate.HasValue ? model.InvoiceDate.Value.Date : (DateTime?)null) ||
									order.InvoiceCurrencyId != model.InvoiceCurrencyId ||
									order.InvoiceNumber != model.InvoiceNumber ||
									order.InvoiceSum != model.InvoiceSum ||
									order.FinRepCenterId != model.FinRepCenterId ||
									order.CargoInfo != model.CargoInfo ||
									order.EnCargoInfo != model.EnCargoInfo ||
									order.Comment != model.Comment ||
									order.Cost != model.Cost;

			if (isOrderChanged)
			{
				#region проверка статуса заказа
				if ((order.OrderStatusId != 2) && (order.OrderStatusId != 3) && (order.OrderStatusId != 4))
					if (!participantLogic.GetWorkgroupByOrder(model.ID).Where(w => w.UserId == CurrentUserId).Any(a => a.ParticipantRoleId == (int)ParticipantRoles.BUH))
						return Content(JsonConvert.SerializeObject(new { ActionId = 2, Message = "Заказ в этом статусе изменять нельзя." }));
				#endregion

				order.ProductId = model.ProductId;
				order.ContractId = model.ContractId;
				order.RequestNumber = model.RequestNumber;
				order.RequestDate = model.RequestDate;
				order.OrderTypeId = model.OrderTypeId;
				order.SpecialCustody = model.SpecialCustody;
				order.EnSpecialCustody = model.EnSpecialCustody;
				order.VolumetricRatioId = model.VolumetricRatioId;
				order.Danger = model.Danger;
				order.EnDanger = model.EnDanger;
				order.PaidWeight = model.PaidWeight;
				order.TemperatureRegime = model.TemperatureRegime;
				order.EnTemperatureRegime = model.EnTemperatureRegime;
				order.InsurancePolicy = model.InsurancePolicy;
				order.InsuranceTypeId = model.InsuranceTypeId;
				order.UninsuranceTypeId = model.UninsuranceTypeId;
				order.InvoiceNumber = model.InvoiceNumber;
				order.InvoiceDate = model.InvoiceDate;
				order.InvoiceSum = model.InvoiceSum;
				order.InvoiceCurrencyId = model.InvoiceCurrencyId;
				order.CargoInfo = model.CargoInfo;
				order.EnCargoInfo = model.EnCargoInfo;
				order.Comment = model.Comment;
				order.FinRepCenterId = model.FinRepCenterId;
				order.Cost = model.Cost;

				if (order.ProductId != model.ProductId) // product changed
				{
					if (model.ProductId.In(1, 2, 3, 8))
						order.VolumetricRatioId = 1;

					if (model.ProductId.In(4, 5, 6, 7, 9))
						order.VolumetricRatioId = 2;
				}

				order.VehicleNumbers = string.Join(",", orderLogic.GetRouteSegments(order.ID).Select(s => s.VehicleNumber).Distinct().ToArray());
				orderLogic.UpdateOrder(order);
			}

			orderLogic.CalculateOrderBalance(model.ID);
			order = orderLogic.GetOrder(model.ID); // обновить данные
			orderLogic.UpdateOrder(order);

			if (order.ID > NEWTYPELIMIT)
				foreach (var item in orderLogic.GetAllOrders())
					orderLogic.CalculateOrderBalance(item.ID);

			return Content(JsonConvert.SerializeObject(""));
		}

		public ContentResult SaveService(ServiceEditModel model)
		{
			var accounting = accountingLogic.GetAccounting(model.AccountingId.Value);

			#region проверка статуса заказа

			var order = orderLogic.GetOrder(accounting.OrderId);
			if (!order.OrderStatusId.In(2, 3, 4, 8))
				return Content(JsonConvert.SerializeObject(new { Message = "Заказ в этом статусе изменять нельзя." }));

			#endregion
			#region проверка статуса контрагента

			var contract = contractLogic.GetContract(accounting.IsIncome ? order.ContractId.Value : accounting.ContractId.Value);
			var legal = legalLogic.GetLegal(contract.LegalId);
			var contractor = contractorLogic.GetContractor(legal.ContractorId.Value);
			if (!contractor.IsLocked)
				return Content(JsonConvert.SerializeObject(new { Message = "Контрагент не зафиксирован." }));

			#endregion

			// проверка прав участника
			if (!participantLogic.IsAllowedActionByOrder(18, accounting.OrderId, CurrentUserId))
				return Content(JsonConvert.SerializeObject(new { ActionId = 18, Message = "У вас недостаточно прав на выполнение этого действия." }));

			#region проверка меток и прочего

			var workgroup = participantLogic.GetWorkgroupByOrder(accounting.OrderId).Where(w => w.UserId == CurrentUserId);
			var mark = accountingLogic.GetAccountingMarkByAccounting(model.AccountingId.Value);
			if (mark != null && (mark.IsInvoiceChecked || mark.IsAccountingChecked) && !IsAllowed(workgroup, ParticipantRoles.BUH, ParticipantRoles.GM))
				return Content(JsonConvert.SerializeObject(new { ActionId = 18, Message = "Нельзя менять состав услуг после проставления метки 'Счет проверен' или 'Расход проверен'" }));

			// Для контрагентов на упрощенной системе налогообложения, а также контрагентам, расход по которым проводится на договор страхования, проверять что ставка НДС стоит "без НДС"
			if (!accounting.IsIncome)
				if (model.VatId != 2)
					if ((legal.TaxTypeId == 2) || (contract.ContractTypeId == 4))
						return Content(JsonConvert.SerializeObject(new { ActionId = 18, Message = "Для контрагентов на упрощенной системе налогообложения, а также контрагентам, расход по которым проводится на договор страхования, может быть только 'без НДС'" }));

			#endregion

			if ((model.ID == 0) && !model.IsDeleted)
			{
				// новое 
				var service = new Service();
				service.AccountingId = model.AccountingId;
				service.ServiceTypeId = model.ServiceTypeId;
				service.Count = model.Count;
				service.Price = model.Price;
				service.CurrencyId = model.CurrencyId;
				service.VatId = model.VatId;
				service.IsForDetalization = model.IsForDetalization;

				int id = accountingLogic.CreateService(service);
				var message = accountingLogic.CalculateServiceBalance(id);
				if (!string.IsNullOrEmpty(message))
					return Content(JsonConvert.SerializeObject(new { Message = message }));

				service = accountingLogic.GetService(id);

				orderLogic.CalculateOrderBalance(accounting.OrderId);   // ??

				return Content(JsonConvert.SerializeObject(new { Service = service }));
			}
			else
			{
				if (model.IsDeleted)
				{
					// удалить 
					accountingLogic.DeleteService(model.ID);
					accountingLogic.CalculateContractorBalance(accounting.ID);
					return Content(JsonConvert.SerializeObject(""));
				}
				else
				{
					// обновить позицию
					var service = accountingLogic.GetService(model.ID);
					service.ServiceTypeId = model.ServiceTypeId;
					service.Count = model.Count;
					service.Price = model.Price;
					service.CurrencyId = model.CurrencyId;
					service.VatId = model.VatId;
					service.IsForDetalization = model.IsForDetalization;

					accountingLogic.UpdateService(service);

					var message = accountingLogic.CalculateServiceBalance(service.ID);
					if (!string.IsNullOrEmpty(message))
						return Content(JsonConvert.SerializeObject(new { Message = message }));

					service = accountingLogic.GetService(service.ID);

					orderLogic.CalculateOrderBalance(accounting.OrderId);   // ??

					return Content(JsonConvert.SerializeObject(new { Service = service }));
				}
			}
		}

		public ContentResult SaveRouteContact(RouteContact model)
		{
			if (model.ID == 0)
			{
				// новое 
				var contact = new RouteContact();
				contact.LegalId = model.LegalId;
				contact.Contact = model.Contact;
				contact.Email = model.Email;
				contact.EnContact = model.EnContact;
				contact.Name = model.Name;
				contact.Phones = model.Phones;
				contact.PlaceId = model.PlaceId;
				contact.Address = model.Address;
				contact.EnAddress = model.EnAddress;

				var id = legalLogic.CreateRouteContact(contact);
				return Content(JsonConvert.SerializeObject(new { ID = id }));
			}
			else
			{
				// обновить позицию
				var contact = legalLogic.GetRouteContact(model.ID);
				contact.Contact = model.Contact;
				contact.Email = model.Email;
				contact.EnContact = model.EnContact;
				contact.Name = model.Name;
				contact.Phones = model.Phones;
				contact.PlaceId = model.PlaceId;
				contact.Address = model.Address;
				contact.EnAddress = model.EnAddress;

				legalLogic.UpdateRouteContact(contact);
				return Content(JsonConvert.SerializeObject(""));
			}
		}

		public ContentResult SaveCargoSeat(CargoSeatEditModel model)
		{
			// проверка прав участника
			if (!participantLogic.IsAllowedActionByOrder(14, model.OrderId.Value, CurrentUserId))
				return Content(JsonConvert.SerializeObject(new { ActionId = 14, Message = "У вас недостаточно прав на выполнение этого действия." }));

			#region проверка статуса заказа
			var order = orderLogic.GetOrder(model.OrderId.Value);
			if (!order.OrderStatusId.In(2, 3, 4))
				return Content(JsonConvert.SerializeObject(new { ActionId = 14, Message = "Заказ в этом статусе изменять нельзя." }));
			#endregion

			if ((model.ID == 0) && !model.IsDeleted)
			{
				// новое 
				var seat = new CargoSeat();
				seat.OrderId = model.OrderId;
				seat.CargoDescriptionId = model.CargoDescriptionId;
				seat.SeatCount = model.SeatCount;
				seat.Length = model.Length;
				seat.Height = model.Height;
				seat.Width = model.Width;
				seat.Volume = model.Volume;
				seat.GrossWeight = model.GrossWeight;
				seat.PackageTypeId = model.PackageTypeId;
				seat.IsStacking = model.IsStacking;

				var id = orderLogic.CreateCargoSeat(seat);
				return Content(JsonConvert.SerializeObject(new { ID = id }));
			}
			else
			{
				if (model.IsDeleted)
				{
					// удалить договор
					orderLogic.DeleteCargoSeat(model.ID);
					return Content(JsonConvert.SerializeObject(""));
				}
				else
				{
					// обновить позицию
					var seat = orderLogic.GetCargoSeat(model.ID);
					seat.CargoDescriptionId = model.CargoDescriptionId;
					seat.SeatCount = model.SeatCount;
					seat.Length = model.Length;
					seat.Height = model.Height;
					seat.Width = model.Width;
					seat.Volume = model.Volume;
					seat.GrossWeight = model.GrossWeight;
					seat.PackageTypeId = model.PackageTypeId;
					seat.IsStacking = model.IsStacking;

					orderLogic.UpdateCargoSeat(seat);
					return Content(JsonConvert.SerializeObject(""));
				}
			}
		}

		public ContentResult SaveOperation(OperationEditModel model)
		{
			#region проверка статуса заказа
			var order = orderLogic.GetOrder(model.OrderId);
			if (!order.OrderStatusId.In(2, 3, 4, 8))
				return Content(JsonConvert.SerializeObject(new { Message = "Заказ в этом статусе изменять нельзя." }));
			#endregion

			model.StartFactDate = model.StartFactDate?.ToUniversalTime();
			model.FinishFactDate = model.FinishFactDate?.ToUniversalTime();

			if (model.ID == 0)
			{
				var operation = new Operation();
				operation.No = model.No;
				operation.OrderId = model.OrderId;
				operation.FinishFactDate = (model.FinishFactDate.HasValue) ? model.FinishFactDate.Value.ToUniversalTime() : model.FinishFactDate;
				operation.FinishPlanDate = (model.FinishPlanDate.HasValue) ? model.FinishPlanDate.Value.ToUniversalTime() : model.FinishPlanDate;
				operation.StartFactDate = (model.StartFactDate.HasValue) ? model.StartFactDate.Value.ToUniversalTime() : model.StartFactDate;
				operation.StartPlanDate = (model.StartPlanDate.HasValue) ? model.StartPlanDate.Value.ToUniversalTime() : model.StartPlanDate;
				operation.OperationStatusId = model.OperationStatusId;
				operation.OrderOperationId = model.OrderOperationId;
				operation.ResponsibleUserId = model.ResponsibleUserId;
				operation.Name = model.Name;

				var id = orderLogic.CreateOperation(operation);
				return Content(JsonConvert.SerializeObject(new { ID = id }));
			}
			else
			{
				// проверка прав участника
				if (!participantLogic.IsAllowedActionByOrder(23, model.OrderId, CurrentUserId))
					return Content(JsonConvert.SerializeObject(new { ActionId = 23, Message = "У вас недостаточно прав на выполнение этого действия." }));

				if (model.IsDeleted)
				{
					// удалить операцию
					orderLogic.DeleteOperation(model.ID);
					return Content(JsonConvert.SerializeObject(""));
				}
				else
				{
					// обновить операцию
					var operation = orderLogic.GetOperation(model.ID);

					#region проверить на изменение дат и наличие прав на это

					var workgroup = participantLogic.GetWorkgroupByOrder(model.OrderId).Where(w => w.UserId == CurrentUserId);
					if (operation.FinishFactDate != model.FinishFactDate ||
						operation.FinishPlanDate != model.FinishPlanDate ||
						operation.StartFactDate != model.StartFactDate ||
						operation.StartPlanDate != model.StartPlanDate)
					{
						if (!IsAllowed(workgroup, ParticipantRoles.AM, ParticipantRoles.GM))
							if (model.ResponsibleUserId != CurrentUserId)
								return Content(JsonConvert.SerializeObject(new { Message = "Вы не можете изменять даты в этой операции" }));
					}

					#endregion

					bool isStarted = model.StartFactDate.HasValue && (operation.StartFactDate != model.StartFactDate);
					bool isFinished = model.FinishFactDate.HasValue && (operation.FinishFactDate != model.FinishFactDate);
					operation.No = model.No;
					operation.FinishFactDate = (model.FinishFactDate.HasValue) ? model.FinishFactDate.Value.ToUniversalTime() : model.FinishFactDate;
					operation.FinishPlanDate = (model.FinishPlanDate.HasValue) ? model.FinishPlanDate.Value.ToUniversalTime() : model.FinishPlanDate;
					operation.StartFactDate = (model.StartFactDate.HasValue) ? model.StartFactDate.Value.ToUniversalTime() : model.StartFactDate;
					operation.StartPlanDate = (model.StartPlanDate.HasValue) ? model.StartPlanDate.Value.ToUniversalTime() : model.StartPlanDate;
					operation.OperationStatusId = model.FinishFactDate.HasValue ? 2 : (model.StartFactDate.HasValue ? 1 : (model.StartPlanDate.HasValue ? 3 : (int?)null));
					operation.OrderOperationId = model.OrderOperationId;
					operation.ResponsibleUserId = model.ResponsibleUserId;
					operation.Name = model.Name;

					orderLogic.UpdateOperation(operation);

					#region raise events

					if (isStarted || isFinished)
					{
						var orderOperation = dataLogic.GetOrderOperation(operation.OrderOperationId);
						var _event = dataLogic.GetEvent(isStarted ? orderOperation.StartFactEventId ?? 0 : orderOperation.FinishFactEventId ?? 0);
						if (_event != null)
						{
							var oev = orderLogic.GetOrderEvent(model.OrderId, _event.ID);
							if (oev == null)
								orderLogic.CreateOrderEvent(new OrderEvent { OrderId = model.OrderId, EventId = _event.ID, IsExternal = _event.IsExternal, Date = isStarted ? operation.StartFactDate.Value : operation.FinishFactDate.Value, CreatedDate = DateTime.Now, City = model.City, Comment = model.Comment });
							else
							{
								oev.CreatedDate = DateTime.Now;
								oev.Date = isStarted ? operation.StartFactDate.Value : operation.FinishFactDate.Value;
								oev.City = model.City;
								oev.Comment = model.Comment;
								orderLogic.UpdateOrderEvent(oev);
							}

							var contractor = contractorLogic.GetContractorByContract(order.ContractId.Value);
							new NotificationMailer().SendNotificationOf_Event(contractor.ID, order, _event);
						}
					}

					#endregion

					return Content(JsonConvert.SerializeObject(operation));
				}
			}
		}

		public ContentResult SaveRoutePoint(RoutePointEditModel model)
		{
			#region проверка статуса заказа
			var order = orderLogic.GetOrder(model.OrderId);
			if (!order.OrderStatusId.In(2, 3, 4))
				return Content(JsonConvert.SerializeObject(new { Message = "Заказ в этом статусе изменять нельзя." }));
			#endregion

			if ((model.ID == 0) && !model.IsDeleted)
			{
				// новое 
				var point = new RoutePoint();
				point.OrderId = model.OrderId;
				point.No = model.No;
				point.RoutePointTypeId = model.RoutePointTypeId;
				point.PlaceId = (model.PlaceId > 0) ? model.PlaceId : null;
				point.PlanDate = (model.PlanDate == DateTime.MinValue) ? DateTime.Now : model.PlanDate;
				point.FactDate = model.FactDate;
				point.Address = model.Address;
				point.EnAddress = model.EnAddress;
				point.Contact = model.Contact;
				point.EnContact = model.EnContact;
				point.ParticipantLegalId = model.ParticipantLegalId;
				point.ParticipantComment = model.ParticipantComment;
				point.RouteContactID = model.RouteContactID;
				point.EnParticipantComment = model.EnParticipantComment;

				var id = orderLogic.CreateRoutePoint(point);
				return Content(JsonConvert.SerializeObject(new { ID = id }));
			}
			else
			{
				// проверка прав участника
				if (!participantLogic.IsAllowedActionByOrder(16, model.OrderId, CurrentUserId))
					return Content(JsonConvert.SerializeObject(new { ActionId = 16, Message = "У вас недостаточно прав на выполнение этого действия." }));

				if (model.IsDeleted)
				{
					// удалить 
					orderLogic.DeleteRoutePoint(model.ID);
					return Content(JsonConvert.SerializeObject(""));
				}
				else
				{
					// обновить позицию
					var point = orderLogic.GetRoutePoint(model.ID);
					point.No = model.No;
					point.RoutePointTypeId = model.RoutePointTypeId;
					point.PlaceId = (model.PlaceId > 0) ? model.PlaceId : null;
					point.PlanDate = (model.PlanDate == DateTime.MinValue) ? DateTime.Now : model.PlanDate;
					point.FactDate = model.FactDate;
					point.Address = model.Address;
					point.EnAddress = model.EnAddress;
					point.Contact = model.Contact;
					point.EnContact = model.EnContact;
					point.ParticipantLegalId = model.ParticipantLegalId;
					point.ParticipantComment = model.ParticipantComment;
					point.RouteContactID = model.RouteContactID;
					point.EnParticipantComment = model.EnParticipantComment;

					orderLogic.UpdateRoutePoint(point);
					return Content(JsonConvert.SerializeObject(""));
				}
			}
		}

		public ContentResult SaveDocument(DocumentEditModel model)
		{
			//// в заказе документ ДТ с одинаковыми реквизитами (номер и дата) не имел дважды метку «выводить в NIP».
			//if (model.OrderId.HasValue)
			//	if (model.IsNipVisible && model.DocumentTypeId == 20)
			//	{
			//		var existsDocument = documentLogic.GetDocumentsByOrder(model.OrderId.Value).FirstOrDefault(w => w.ID != model.ID && w.IsNipVisible && w.DocumentTypeId == 20);
			//		if ((existsDocument != null) && (model.Number.ToUpper() == existsDocument.Number.ToUpper()) && (model.Date.Value.Date == existsDocument.Date.Value.Date))
			//			return Content(JsonConvert.SerializeObject(new { ActionId = 63, Message = "Уже есть такой ДТ, с таким же номером и датой" }));
			//	}

			// в заказе документ с одинаковыми реквизитами (номер и дата) не имел дважды метку «выводить в NIP».
			if (model.OrderId.HasValue && model.IsNipVisible)
			{
				var existsDocument = documentLogic.GetDocumentsByOrder(model.OrderId.Value).FirstOrDefault(w => w.ID != model.ID && w.IsNipVisible && w.DocumentTypeId == model.DocumentTypeId);
				if ((existsDocument != null) && (model.Number.ToUpper() == existsDocument.Number.ToUpper()) && (model.Date.Value.Date == existsDocument.Date.Value.Date))
					return Content(JsonConvert.SerializeObject(new { ActionId = 63, Message = "Уже есть такой документ, с таким же номером и датой" }));
			}

			if (model.ID == 0)
				throw new ArgumentException();

			// only update
			{
				var document = documentLogic.GetDocument(model.ID);

				int orderId = 0;
				if (document.OrderId.HasValue)
					orderId = document.OrderId.Value;
				else if (document.AccountingId.HasValue)
					orderId = accountingLogic.GetAccounting(document.AccountingId.Value).OrderId;

				if (orderId > 0)
					// проверка прав участника
					if (!participantLogic.IsAllowedActionByOrder(63, orderId, CurrentUserId))
						return Content(JsonConvert.SerializeObject(new { ActionId = 63, Message = "У вас недостаточно прав на выполнение этого действия." }));

				// TODO: document.ContractId - IsAllowedActionByContractor

				#region проверка меток и прочего

				if (document.AccountingId.HasValue)
				{
					var workgroup = participantLogic.GetWorkgroupByOrder(orderId).Where(w => w.UserId == CurrentUserId);
					var mark = accountingLogic.GetAccountingMarkByAccounting(document.AccountingId.Value);
					if (mark != null && (mark.IsAccountingChecked) && !IsAllowed(workgroup, ParticipantRoles.BUH, ParticipantRoles.GM))
						return Content(JsonConvert.SerializeObject(new { Message = "Нельзя менять документы после проставления метки 'Расход проверен'" }));
				}

				#endregion

				if (model.IsDeleted)
				{
					if (model.AccountingId.HasValue)
					{
						var marks = accountingLogic.GetAccountingMarkByAccounting(model.AccountingId.Value);
						if ((marks != null) && (marks.IsActOk || marks.IsAccountingOk))
							return Content(JsonConvert.SerializeObject(new { Message = "Нельзя удалить документ при наличии метки Ок" }));
					}

					// удалить 
					documentLogic.DeleteDocument(model.ID, CurrentUserId);
				}
				else
				{
					// обновить позицию
					document.DocumentTypeId = model.DocumentTypeId;
					document.Number = model.Number;
					document.Filename = model.Filename;
					document.Date = model.Date;
					document.UploadedDate = model.UploadedDate?.ToUniversalTime();
					document.UploadedBy = model.UploadedBy;
					document.IsPrint = model.IsPrint;
					document.IsNipVisible = model.IsNipVisible;
					document.FileSize = model.FileSize;
					document.OriginalSentDate = model.OriginalSentDate;
					document.OriginalReceivedDate = model.OriginalReceivedDate;

					documentLogic.UpdateDocument(document, CurrentUserId);

					#region запись в лог

					if (!model.ContractId.HasValue)
					{
						if (!model.AccountingId.HasValue || accountingLogic.GetAccounting(model.AccountingId.Value).IsIncome)   // только для доходов и документов заказа
						{
							//int orderId = model.OrderId ?? accountingLogic.GetAccounting(model.AccountingId.Value).OrderId;
							int contractorId = contractorLogic.GetContractorByContract(orderLogic.GetOrder(orderId).ContractId.Value).ID;

							documentLogic.CreateOrUpdateDocumentLog(new DocumentLog { DocumentId = document.ID, DocumentDate = document.Date ?? DateTime.Now, DocumentNumber = document.Number, DocumentTypeId = document.DocumentTypeId ?? 0, Date = DateTime.Now, UserId = CurrentUserId, OrderId = orderId, ContractorId = contractorId, Action = "updated" });
						}
					}

					#endregion

					UpdateAccountingNumbers(model);
				}
			}

			return Content(JsonConvert.SerializeObject(""));
		}

		public ContentResult SaveDocumentDeliveryInfo(int id, bool isDocument, string receivedBy, string receivedNumber)
		{
			if (isDocument)
			{
				var document = documentLogic.GetDocument(id);
				document.ReceivedBy = receivedBy;
				document.ReceivedNumber = receivedNumber;
				documentLogic.UpdateDocument(document, CurrentUserId);
			}
			else
			{
				var tdocument = documentLogic.GetTemplatedDocument(id);
				tdocument.ReceivedBy = receivedBy;
				tdocument.ReceivedNumber = receivedNumber;
				documentLogic.UpdateTemplatedDocument(tdocument);
			}

			return Content(JsonConvert.SerializeObject(""));
		}

		public ContentResult SaveRouteSegment(RouteSegmentEditModel model)
		{
			if ((model.ID == 0) && !model.IsDeleted)
			{
				// новое 
				var segment = new RouteSegment();
				segment.OrderId = model.OrderId;
				segment.No = model.No;
				segment.TransportTypeId = model.TransportTypeId;
				segment.IsAfterBorder = model.IsAfterBorder;
				segment.Length = model.Length;
				segment.Vehicle = model.Vehicle;
				segment.VehicleNumber = model.VehicleNumber;
				segment.DriverName = model.DriverName;
				segment.DriverPhone = model.DriverPhone;

				var id = orderLogic.CreateRouteSegment(segment);
				return Content(JsonConvert.SerializeObject(new { ID = id }));
			}
			else
			{
				if (model.IsDeleted)
				{
					// удалить 
					orderLogic.DeleteRouteSegment(model.ID);
				}
				else
				{
					// обновить позицию
					var segment = orderLogic.GetRouteSegment(model.ID);
					segment.No = model.No;
					segment.TransportTypeId = model.TransportTypeId;
					segment.IsAfterBorder = model.IsAfterBorder;
					segment.Length = model.Length;
					segment.Vehicle = model.Vehicle;
					segment.VehicleNumber = model.VehicleNumber;
					segment.DriverName = model.DriverName;
					segment.DriverPhone = model.DriverPhone;

					orderLogic.UpdateRouteSegment(segment);
				}
			}

			return Content(JsonConvert.SerializeObject(""));
		}

		public ContentResult SaveAccounting(AccountingEditModel model)
		{
			model.InvoiceDate = model.InvoiceDate?.ToUniversalTime();
			model.ActDate = model.ActDate?.ToUniversalTime();

			// проверка прав участника
			if (!participantLogic.IsAllowedActionByOrder(20, model.OrderId, CurrentUserId))
				return Content(JsonConvert.SerializeObject(new { ActionId = 20, Message = "У вас недостаточно прав на выполнение этого действия." }));

			var workgroup = participantLogic.GetWorkgroupByOrder(model.OrderId);

			#region проверка статуса заказа

			var order = orderLogic.GetOrder(model.OrderId);
			if (!order.OrderStatusId.In(2, 3, 4, 8))
				if (!workgroup.Where(w => w.UserId == CurrentUserId).Any(a => a.ParticipantRoleId == (int)ParticipantRoles.BUH))
					return Content(JsonConvert.SerializeObject(new { ActionId = 20, Message = "Расход нельзя редактировать в заказе с таким статусом." }));

			#endregion
			#region проверить проверенность договора

			if (!model.IsIncome)
			{
				var contractMarks = contractLogic.GetContractMarkByContract(model.ContractId.Value);
				if ((contractMarks == null) || (!contractMarks.IsContractChecked))
					return Content(JsonConvert.SerializeObject(new { Message = "Договор не проверен. Обратитесь к администратору" }));
			}

			#endregion
			#region проверить договор

			var contract = contractLogic.GetContract(model.IsIncome ? order.ContractId.Value : model.ContractId.Value);
			if (!(contract.EndDate.HasValue ? (contract.EndDate < DateTime.Now ? (contract.IsProlongation ? true : false) : true) : (true)))
				return Content(JsonConvert.SerializeObject(new { Message = "Договор просрочен. Обратитесь к администратору" }));

			#endregion

			// обновить сущность
			var accounting = accountingLogic.GetAccounting(model.ID);

			bool isLegalIdChanged = (accounting.LegalId != model.LegalId) || (accounting.ContractId != model.ContractId);
			bool isAccountingPaymentMethodIdChanged = accounting.AccountingPaymentMethodId != model.AccountingPaymentMethodId;
			bool isAccountingPaymentTypeIdChanged = accounting.AccountingPaymentTypeId != model.AccountingPaymentTypeId;
			bool isInvoiceNumberChanged = accounting.InvoiceNumber != model.InvoiceNumber;
			bool isInvoiceDateChanged = (accounting.InvoiceDate ?? DateTime.MinValue).Date != (model.InvoiceDate ?? DateTime.MinValue).Date;
			bool isActNumberChanged = accounting.ActNumber != model.ActNumber;
			bool isActDateChanged = (accounting.ActDate ?? DateTime.MinValue).Date != (model.ActDate ?? DateTime.MinValue).Date;
			bool isCurrencyRateChanged = accounting.CurrencyRate != model.CurrencyRate;

			var mark = accountingLogic.GetAccountingMarkByAccounting(model.ID);

			#region проверка меток и прочего

			if (mark != null
				&& (mark.IsInvoiceChecked || mark.IsAccountingChecked)
				&& (isLegalIdChanged || isAccountingPaymentMethodIdChanged || isAccountingPaymentTypeIdChanged || isInvoiceNumberChanged || isInvoiceDateChanged)
				&& !IsAllowed(workgroup.Where(w => w.UserId == CurrentUserId), ParticipantRoles.BUH, ParticipantRoles.GM))
				return Content(JsonConvert.SerializeObject(new { Message = "Нельзя менять платежные поля после проставления метки 'Счет проверен' или 'Расход проверен'" }));


			if (mark != null
				&& (mark.IsAccountingChecked)
				&& (isActNumberChanged || isActDateChanged)
				&& !IsAllowed(workgroup.Where(w => w.UserId == CurrentUserId), ParticipantRoles.BUH, ParticipantRoles.GM))
				return Content(JsonConvert.SerializeObject(new { Message = "Нельзя менять платежные поля после проставления метки 'Расход проверен'" }));

			// TODO: проверка даты акта
			/* ри попытке проставить дату акта/счет фактуры 
до 31 декабря предыдущего года после 20 января включительно
до 31 марта после 20 апреля включительно
до 30 июня после 20 июля включительно
до 30 октября после 20 ноября включительно
выдавать предупреждение: "После 20го числа первого месяца текущего квартала выставление актов и счетов-фактур, датированных предыдущим кварталом, невозможно. Обратитесь в бухгалтерию."
			 */
			#endregion

			accounting.AccountingDocumentTypeId = model.AccountingDocumentTypeId;
			accounting.AccountingPaymentTypeId = model.AccountingPaymentTypeId;
			accounting.AccountingPaymentMethodId = model.AccountingPaymentMethodId;
			accounting.SecondSignerEmployeeId = model.SecondSignerEmployeeId;
			accounting.ContractId = model.ContractId;
			accounting.Number = model.Number;
			accounting.InvoiceNumber = model.InvoiceNumber;
			accounting.VatInvoiceNumber = model.VatInvoiceNumber;
			accounting.ActNumber = model.ActNumber;
			accounting.Route = model.Route;
			accounting.Comment = model.Comment;
			accounting.SecondSignerName = model.SecondSignerName;
			accounting.SecondSignerPosition = model.SecondSignerPosition;
			accounting.SecondSignerInitials = model.SecondSignerInitials;
			accounting.CurrencyRate = model.CurrencyRate;
			accounting.InvoiceDate = model.InvoiceDate;
			accounting.ActDate = model.ActDate;
			accounting.AccountingDate = model.AccountingDate;
			accounting.RequestDate = model.RequestDate;
			accounting.CargoLegalId = model.CargoLegalId;
			accounting.PaymentPlanDate = model.PaymentPlanDate;
			accounting.OurLegalId = model.OurLegalId;
			accounting.LegalId = model.LegalId;
			accounting.PayMethodId = model.PayMethodId;

			// дополнительно 
			//accounting.CurrencyRate = GetAccountingRate(accounting);
			// при выборе контрагента "Касса" устанавливать способ оплаты "Нал"
			if (accounting.ContractId.HasValue && accounting.ContractId > 0)
			{
				// Для расхода
				var contractor = contractorLogic.GetContractorByContract(accounting.ContractId.Value);
				if (contractor.ID == INTERNAL_CONTRACTOR_ID)
				{
					accounting.AccountingPaymentMethodId = 1;
					accounting.ActNumber = accounting.Number;
					accounting.InvoiceNumber = accounting.Number;
					accounting.ActDate = accounting.InvoiceDate;
				}
			}
			else if (accounting.LegalId.HasValue && accounting.LegalId > 0)
			{
				//для дохода
				var contractor = contractorLogic.GetContractorByLegal(accounting.LegalId.Value);
				if (contractor.ID == INTERNAL_CONTRACTOR_ID)
					accounting.AccountingPaymentMethodId = 1;
			}

			accountingLogic.UpdateAccounting(accounting);

			SaveAccountingRouteSegments(model.RouteSegments, model.ID);

			if (accounting.CurrencyRate.HasValue && isCurrencyRateChanged)
			{
				// поменялся курс пересчета, пересчитать услуги (без привлечения CurrencyRateUse)
				foreach (var service in accountingLogic.GetServicesByAccounting(accounting.ID))
				{
					service.Sum = Math.Round(service.OriginalSum.Value * accounting.CurrencyRate.Value, 4);
					accountingLogic.UpdateService(service);
				}
			}

			accountingLogic.CalculateAccountingBalance(accounting.ID);
			accountingLogic.CalculatePaymentPlanDate(model.ID);

			return Content(JsonConvert.SerializeObject(""));
		}

		void SaveAccountingRouteSegments(List<OrderAccountingRouteSegmentEditModel> model, int accountingId)
		{
			var list = accountingLogic.GetAccountingRouteSegments(accountingId);

			// сначала удалить неактуальные
			foreach (var item in list)
				if (!model.Any(w => w.RouteSegmentId == item.RouteSegmentId))
					accountingLogic.DeleteAccountingRouteSegment(item.ID);

			// добавить новое 
			foreach (var item in model)
				if (!list.Any(w => w.RouteSegmentId == item.RouteSegmentId))
					accountingLogic.CreateAccountingRouteSegment(new OrderAccountingRouteSegment { AccountingId = item.AccountingId, RouteSegmentId = item.RouteSegmentId });
		}

		#endregion

		public ContentResult DeleteAccounting(int accountingId, bool isCascade = false)
		{
			var accounting = accountingLogic.GetAccounting(accountingId);

			// проверка прав участника
			if (!participantLogic.IsAllowedActionByOrder(21, accounting.OrderId, CurrentUserId))
				return Content(JsonConvert.SerializeObject(new { ActionId = 21, Message = "У вас недостаточно прав на выполнение этого действия." }));

			try
			{
				accountingLogic.DeleteAccounting(accountingId, isCascade);
			}
			catch (Exception ex)
			{
				return Content(JsonConvert.SerializeObject(new { Message = "Не удалось удалить, " + ex.Message }));
			}

			return Content(JsonConvert.SerializeObject(""));
		}

		public ContentResult DeleteTemplatedDocument(int id)
		{
			var document = documentLogic.GetTemplatedDocument(id);
			var accounting = accountingLogic.GetAccounting(document.AccountingId.Value);

			// проверка прав участника
			if (!participantLogic.IsAllowedActionByOrder(26, accounting.OrderId, CurrentUserId))
				return Content(JsonConvert.SerializeObject(new { ActionId = 26, Message = "У вас недостаточно прав на выполнение этого действия." }));

			#region проверка меток и прочего

			var workgroup = participantLogic.GetWorkgroupByOrder(accounting.OrderId).Where(w => w.UserId == CurrentUserId);
			var mark = accountingLogic.GetAccountingMarkByAccounting(accounting.ID);
			switch (document.TemplateId)
			{
				//Заявление на страхование
				case 1:
					break;
				//Заявление на оказание услуг от клиента
				case 2:
					break;
				//Заявление на оказание услуг поставщику
				case 3:
					break;
				//Счет
				case 4:
				//	Счет (новый)
				case 16:
				//	Счет EN
				case 19:
					if (mark != null && (mark.IsInvoiceChecked) && !IsAllowed(workgroup, ParticipantRoles.BUH, ParticipantRoles.GM))
						return Content(JsonConvert.SerializeObject(new { Message = "Нельзя удалять документ после проставления метки 'Счет проверен'" }));
					break;

				//Детализация
				case 5:
					break;
				//Заявление на оказание услуг поставщику RU-EN
				case 7:
					break;
				//Акт
				case 6:
				//Акт EN
				case 8:
				//	Акт (новый)
				case 17:
				//	Акт англ (новый)
				case 18:
					if (mark != null && (mark.IsActChecked) && !IsAllowed(workgroup, ParticipantRoles.BUH, ParticipantRoles.GM))
						return Content(JsonConvert.SerializeObject(new { Message = "Нельзя удалять документ после проставления метки 'Акт проверен'" }));
					break;

				//	Счет-фактура
				case 15:
					break;

					// TODO: rest
			}

			var order = orderLogic.GetOrder(accounting.OrderId);
			if (order.OrderStatusId == 9)       // закрыт
				return Content(JsonConvert.SerializeObject(new { Message = "Нельзя удалять документы, поскольку заказ закрыт." }));

			#endregion

			documentLogic.DeleteTemplatedDocument(id);
			return Content(JsonConvert.SerializeObject(""));
		}

		public ContentResult UpdateTemplatedDocument(TemplatedDocument model)
		{
			if (model.ID == 0)
				return Content(JsonConvert.SerializeObject(new { Message = "Документ ещё не сохранен" }));

			var document = documentLogic.GetTemplatedDocument(model.ID);

			#region проверка меток и прочего

			if (((document.OriginalSentDate != model.OriginalSentDate) && !model.OriginalSentDate.HasValue)
				|| ((document.OriginalReceivedDate != model.OriginalReceivedDate) && !model.OriginalReceivedDate.HasValue))
				if (document.AccountingId.HasValue)
				{
					var orderId = accountingLogic.GetAccounting(document.AccountingId.Value).OrderId;
					var workgroup = participantLogic.GetWorkgroupByOrder(orderId).Where(w => w.UserId == CurrentUserId);
					if (!IsAllowed(workgroup, ParticipantRoles.BUH, ParticipantRoles.GM))
						return Content(JsonConvert.SerializeObject(new { Message = "Нельзя!" }));
				}

			#endregion

			document.OriginalSentDate = model.OriginalSentDate.HasValue ? model.OriginalSentDate.Value.ToUniversalTime() : (DateTime?)null;
			document.OriginalSentUserId = model.OriginalSentDate.HasValue ? CurrentUserId : (int?)null;
			document.OriginalReceivedDate = model.OriginalReceivedDate.HasValue ? model.OriginalReceivedDate.Value.ToUniversalTime() : (DateTime?)null;
			document.OriginalReceivedUserId = model.OriginalReceivedDate.HasValue ? CurrentUserId : (int?)null;

			documentLogic.UpdateTemplatedDocument(document);
			return Content(JsonConvert.SerializeObject(new { OriginalSentUserId = document.OriginalSentUserId, OriginalReceivedUserId = document.OriginalReceivedUserId }));
		}

		public ContentResult UpdateDocument(Document model)
		{
			if (model.ID == 0)
				return Content(JsonConvert.SerializeObject(new { Message = "Документ ещё не сохранен" }));

			var document = documentLogic.GetDocument(model.ID);

			#region проверка меток и прочего

			if (((document.OriginalSentDate != model.OriginalSentDate) && !model.OriginalSentDate.HasValue)
				|| ((document.OriginalReceivedDate != model.OriginalReceivedDate) && !model.OriginalReceivedDate.HasValue))
				if (document.AccountingId.HasValue)
				{
					var orderId = accountingLogic.GetAccounting(document.AccountingId.Value).OrderId;
					var workgroup = participantLogic.GetWorkgroupByOrder(orderId).Where(w => w.UserId == CurrentUserId);

					var order = orderLogic.GetOrder(orderId);
					if (order.OrderStatusId.In(6, 7, 9))
					{
						if (!IsAllowed(workgroup, ParticipantRoles.BUH, ParticipantRoles.GM))
							return Content(JsonConvert.SerializeObject(new { Message = "Нельзя!" }));
					}
					else
					{
						if (!IsAllowed(workgroup, ParticipantRoles.AM, ParticipantRoles.OPER, ParticipantRoles.BUH, ParticipantRoles.GM))
							return Content(JsonConvert.SerializeObject(new { Message = "Нельзя!" }));
					}
				}

			#endregion

			document.OriginalSentDate = model.OriginalSentDate.HasValue ? model.OriginalSentDate.Value.ToUniversalTime() : (DateTime?)null;
			document.OriginalSentUserId = model.OriginalSentDate.HasValue ? CurrentUserId : (int?)null;
			document.OriginalReceivedDate = model.OriginalReceivedDate.HasValue ? model.OriginalReceivedDate.Value.ToUniversalTime() : (DateTime?)null;
			document.OriginalReceivedUserId = model.OriginalReceivedDate.HasValue ? CurrentUserId : (int?)null;

			documentLogic.UpdateDocument(document, CurrentUserId);
			return Content(JsonConvert.SerializeObject(new { OriginalSentUserId = document.OriginalSentUserId, OriginalReceivedUserId = document.OriginalReceivedUserId }));
		}

		[HttpPost]
		public ContentResult UploadDocument(int id)
		{
			if (id <= 0 || Request.Files.Count == 0)
				return Content(JsonConvert.SerializeObject(new { Message = "Неверные входные данные" }));

			var file = Request.Files[0];

			var ddata = documentLogic.GetDocumentDataByDocument(id);
			if (ddata == null)
			{
				ddata = new DocumentData { DocumentId = id };
				var ddataId = documentLogic.CreateDocumentData(ddata);
				ddata = documentLogic.GetDocumentDataByDocument(id);
			}

			ddata.Data = new byte[file.InputStream.Length];
			file.InputStream.Read(ddata.Data, 0, (int)file.InputStream.Length);

			documentLogic.UpdateDocumentData(ddata);

			return Content(JsonConvert.SerializeObject(new { CurrentUserId = CurrentUserId }));
		}

		public FileResult GetPaymentsFile(int accountingId)
		{
			var template = Server.MapPath("~/App_Data/PaymentsTemplate.xlsx");
			var filename = Server.MapPath("~\\Temp\\Payments" + Environment.TickCount + ".xlsx");
			var currencies = dataLogic.GetCurrencies();

			#region prepare data

			var list = accountingLogic.GetPayments(accountingId).Select(s => new PaymentViewModel
			{
				AccountingId = s.AccountingId,
				BankAccount = s.BankAccount,
				CurrencyId = s.CurrencyId,
				Date = s.Date,
				Description = s.Description,
				ID = s.ID,
				IsIncome = s.IsIncome,
				Number = s.Number,
				OriginalSum = s.Sum,
				Sum = s.Sum,
				CurrencyRateCB = (float)0,
				CurrencyRate = (float)0,
			}).ToList();

			var accounting = accountingLogic.GetAccounting(accountingId);
			var order = orderLogic.GetOrder(accounting.OrderId);
			var contract = contractLogic.GetContract(accounting.IsIncome ? order.ContractId.Value : (accounting.ContractId ?? 0));
			var service = accountingLogic.GetServicesByAccounting(accountingId).FirstOrDefault();
			foreach (var payment in list)
			{
				if (contract == null)
					continue;   // нет договора

				if (service == null)
					continue;   // нет услуги

				payment.InvoiceCurrencyId = service.CurrencyId ?? 1;

				if (service.CurrencyId == payment.CurrencyId)
					continue;   // не нужна конверсия

				// конверсия в рубли
				if (service.CurrencyId == 1)
				{
					var contractCurrencies = contractLogic.GetContractCurrencies(contract.ID);
					if (contractCurrencies.Any(w => w.CurrencyId == payment.CurrencyId))
						continue;

					// перевести в рубли
					var ruse = dataLogic.GetCurrencyRateUses().FirstOrDefault(w => w.ID == contract.CurrencyRateUseId);
					float rateUse = ruse?.Value ?? 1;
					var date = payment.Date;
					var rate = dataLogic.GetCurrencyRates(date).First(w => w.CurrencyId == payment.CurrencyId).Rate;
					payment.CurrencyRateCB = rate ?? 1;

					if ((ruse != null) && (ruse.IsDocumentDate) && (accounting.InvoiceDate.HasValue))
						rate = dataLogic.GetCurrencyRates(accounting.InvoiceDate.Value).First(w => w.CurrencyId == payment.CurrencyId).Rate;

					payment.CurrencyRate = (rate ?? 1) * rateUse;
					payment.Sum = payment.Sum * payment.CurrencyRate;
				}
				else if (payment.CurrencyId == 1)    // конверсия из рублей
				{
					// перевести в валюту
					var ruse = dataLogic.GetCurrencyRateUses().FirstOrDefault(w => w.ID == contract.CurrencyRateUseId);
					float rateUse = ruse?.Value ?? 1;
					var date = payment.Date;
					var rate = dataLogic.GetCurrencyRates(date).First(w => w.CurrencyId == service.CurrencyId).Rate;
					payment.CurrencyRateCB = rate ?? 1;

					if ((ruse != null) && (ruse.IsDocumentDate) && (accounting.InvoiceDate.HasValue))
						rate = dataLogic.GetCurrencyRates(accounting.InvoiceDate.Value).First(w => w.CurrencyId == service.CurrencyId).Rate;

					payment.CurrencyRate = (rate ?? 1) * rateUse;
					payment.Sum = payment.Sum * (1 / payment.CurrencyRate);
				}
				else
					Debug.WriteLine("Перевод валют не поддерживается");
			}

			#endregion

			var fi = new System.IO.FileInfo(template);

			using (var xl = new ExcelPackage(fi))
			{
				xl.DoAdjustDrawings = true;
				xl.Workbook.CalcMode = ExcelCalcMode.Automatic;

				var sheet = xl.Workbook.Worksheets[1];
				var row = 3;
				foreach (var payment in list)
				{
					var col = 1;
					sheet.Cells[row, col++].Value = payment.Number;
					sheet.Cells[row, col++].Value = payment.Date;
					sheet.Cells[row, col++].Value = currencies.First(w => w.ID == (payment.CurrencyId ?? 1)).Display;
					sheet.Cells[row, col++].Value = payment.OriginalSum;
					sheet.Cells[row, col++].Value = currencies.First(w => w.ID == (payment.InvoiceCurrencyId)).Display;
					sheet.Cells[row, col++].Value = payment.Sum;
					sheet.Cells[row, col++].Value = payment.CurrencyRateCB;
					sheet.Cells[row, col++].Value = payment.CurrencyRate;
					sheet.Cells[row, col++].Value = payment.Description;
					row++;
				}

				xl.SaveAs(new System.IO.FileInfo(filename));
			}

			var contractor = contractorLogic.GetContractorByContract(contract.ID);
			return File(filename, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", contractor.Name + "_" + accounting.Number + "_Payments.xlsx");
		}

		#region Проверки при смене статуса

		void CheckOrderCargoInfo(Order order, List<string> errors)
		{
			if (string.IsNullOrWhiteSpace(order.CargoInfo))
				errors.Add("Нет описания груза");
		}

		void CheckOrderSeats(Order order, List<string> errors)
		{
			if (order.SeatsCount == 0)
				errors.Add("Нет мест");

			//if (!order.Volume.HasValue || order.Volume.Value == 0)
			//	errors.Add("Не указан объем мест");

			if (!order.GrossWeight.HasValue || order.GrossWeight.Value == 0)
				errors.Add("Не указан вес брутто мест");

			var seats = orderLogic.GetCargoSeats(order.ID);
			foreach (var item in seats)
			{
				if (item.SeatCount == 0)
					errors.Add("Не указано количество мест #" + item.ID);

				//if (!item.Volume.HasValue || item.Volume.Value == 0)
				//	errors.Add("Не указан объем мест #" + item.ID);

				if (!item.GrossWeight.HasValue || item.GrossWeight.Value == 0)
					errors.Add("Не указан вес брутто мест #" + item.ID);

				if (!item.PackageTypeId.HasValue)
					errors.Add("Не указан тип упаковки мест #" + item.ID);
			}
		}

		void CheckOrderRoute(Order order, List<string> errors)
		{
			var route = orderLogic.GetRoutePoints(order.ID);

			if (!route.Any(w => w.RoutePointTypeId == 1))   // Пункт загрузки
				errors.Add("Нет пункта загрузки в маршруте");

			if (!route.Any(w => w.RoutePointTypeId == 3))   // Пункт разгрузки
				errors.Add("Нет пункта разгрузки в маршруте");
		}

		void CheckOrderRouteBorder(Order order, List<string> errors)
		{
			var route = orderLogic.GetRoutePoints(order.ID);
			if (!route.Any(w => w.RoutePointTypeId == 2))   // Пограничный переход
				errors.Add("Нет пограничного перехода в маршруте");
		}

		void CheckOrderRouteSegments(Order order, List<string> errors)
		{
			var segments = orderLogic.GetRouteSegments(order.ID);
			if (segments.Count() == 0)
				errors.Add("Не сформированы сегменты маршрута");
		}

		void CheckOrderIncomeAccountings(Order order, List<string> errors)
		{
			var accountings = accountingLogic.GetAccountingsByOrder(order.ID).Where(w => w.IsIncome);
			foreach (var item in accountings)
			{
				if (string.IsNullOrWhiteSpace(item.InvoiceNumber))
					errors.Add("Нет номера счета в доходе " + item.Number);

				if (string.IsNullOrWhiteSpace(item.ActNumber))
					errors.Add("Нет номера акта в доходе " + item.Number);

				if (!item.InvoiceDate.HasValue)
					errors.Add("Нет даты счета в доходе " + item.Number);

				if (!item.ActDate.HasValue)
					errors.Add("Нет даты акта в доходе " + item.Number);
			}
		}

		void CheckOrderExpenseAccountings(Order order, List<string> errors)
		{
			var accountings = accountingLogic.GetAccountingsByOrder(order.ID).Where(w => !w.IsIncome);
			foreach (var item in accountings)
			{
				var contract = contractLogic.GetContract(item.ContractId.Value);
				var legal = legalLogic.GetLegal(contract.LegalId);
				var documents = documentLogic.GetDocumentsByAccounting(item.ID);

				if (legal.ContractorId.In(INTERNAL_CONTRACTOR_ID, FINSERVICES_CONTRACTOR_ID, RATE_DIFF_CONTRACTOR_ID))  // контрагента "Касса" или ...
					continue;

				if (string.IsNullOrWhiteSpace(item.InvoiceNumber))
					errors.Add("Нет номера счета в расходе " + item.Number);

				if (string.IsNullOrWhiteSpace(item.ActNumber))
					errors.Add("Нет номера акта в расходе " + item.Number);

				if (!item.InvoiceDate.HasValue)
					errors.Add("Нет даты счета в расходе " + item.Number);

				if (!item.ActDate.HasValue)
					errors.Add("Нет даты акта в расходе " + item.Number);

				if (!item.ContractId.HasValue)
				{
					errors.Add("Не указан договор в расходе " + item.Number);
					return;
				}

				// для контрагента ТАМОЖНЯ проверку на наличие привязанных документов с типом ДТ
				if (legal.ContractorId == 48)   // ТАМОЖНЯ
				{
					if (documents.Where(w => w.DocumentTypeId == 20).Count() == 0)
						errors.Add("Не прикреплен 'ДТ' в расходе " + item.Number);

					continue;
				}

				if (legal.IsNotResident)
				{
					if (!documents.Any(w => w.DocumentTypeId == 59))    // Счет от поставщика
						errors.Add("Не прикреплен 'Счет от поставщика' в расходе " + item.Number);
				}
				else
				{
					if (legal.TaxTypeId == 1)   // ЮЛ общая система налогообложения (ОСН)
					{
						if (!documents.Any(w => w.DocumentTypeId == 59))    // Счет от поставщика
							errors.Add("Не прикреплен 'Счет от поставщика' в расходе " + item.Number);

						if (!documents.Any(w => w.DocumentTypeId == 10))    // Акт оказанных услуг постав.
							errors.Add("Не прикреплен 'Акт оказанных услуг постав.' в расходе " + item.Number);

						if (!documents.Any(w => w.DocumentTypeId == 61))    // Счет-фактура от поставщика
							errors.Add("Не прикреплен 'Счет-фактура от поставщика' в расходе " + item.Number);
					}
					if (legal.TaxTypeId == 2)   // ЮЛ упрощенная система налогообложения (УСН)
					{
						if (!documents.Any(w => w.DocumentTypeId == 59))    // Счет от поставщика
							errors.Add("Не прикреплен 'Счет от поставщика' в расходе " + item.Number);

						if (!documents.Any(w => w.DocumentTypeId == 10))    // Акт оказанных услуг постав.
							errors.Add("Не прикреплен 'Акт оказанных услуг постав.' в расходе " + item.Number);
					}
				}

				var templatedDocuments = documentLogic.GetTemplatedDocumentsByAccounting(item.ID);
				if (legal.IsNotResident && !templatedDocuments.Any(w => w.TemplateId == 6 || w.TemplateId == 8 || w.TemplateId == 17 || w.TemplateId == 18 || w.TemplateId == 20 || w.TemplateId == 22))    // Акт 
					errors.Add("Не сформирован Акт в расходе " + item.Number);
			}
		}

		void CheckOrderIncomeMarksOk(Order order, List<string> errors)
		{
			var accountings = accountingLogic.GetAccountingsByOrder(order.ID).Where(w => w.IsIncome);
			foreach (var item in accountings)
			{
				var marks = accountingLogic.GetAccountingMarkByAccounting(item.ID);
				if (marks == null)
					errors.Add("Не проставлены метки в доходе " + item.Number);
				else
				{
					if (!marks.IsInvoiceOk)
						errors.Add("Не проставлена метка 'Счет Ок' в доходе " + item.Number);

					if (!marks.IsActOk)
						errors.Add("Не проставлена метка 'Акт Ок' в доходе " + item.Number);
				}
			}
		}

		void CheckOrderExpenseMarksOk(Order order, List<string> errors)
		{
			var accountings = accountingLogic.GetAccountingsByOrder(order.ID).Where(w => !w.IsIncome);
			foreach (var item in accountings)
			{
				if ((item.ContractId == 587) || (item.ContractId == 588))   // контрагент "курсовые разницы"
					continue;

				var marks = accountingLogic.GetAccountingMarkByAccounting(item.ID);
				if (marks == null)
					errors.Add("Не проставлены метки в расходе " + item.Number);
				else
				{
					if (!marks.IsAccountingOk)
						errors.Add("Не проставлена метка 'Расход Ок' в расходе " + item.Number);
				}
			}
		}

		void CheckOrderIncomeMarksChecked(Order order, List<string> errors)
		{
			var accountings = accountingLogic.GetAccountingsByOrder(order.ID).Where(w => w.IsIncome);
			foreach (var item in accountings)
			{
				var marks = accountingLogic.GetAccountingMarkByAccounting(item.ID);
				if (marks == null)
					errors.Add("Не проставлены метки в доходе " + item.Number);
				else
				{
					if (!marks.IsInvoiceChecked)
						errors.Add("Не проставлена метка 'Счет проверен' в доходе " + item.Number);

					if (!marks.IsActChecked)
						errors.Add("Не проставлена метка 'Акт проверен' в доходе " + item.Number);
				}
			}
		}

		void CheckOrderExpenseMarksChecked(Order order, List<string> errors)
		{
			var accountings = accountingLogic.GetAccountingsByOrder(order.ID).Where(w => !w.IsIncome);
			foreach (var item in accountings)
			{
				if ((item.ContractId == 587) || (item.ContractId == 588))   // контрагент "курсовые разницы"
					continue;

				var marks = accountingLogic.GetAccountingMarkByAccounting(item.ID);
				if (marks == null)
					errors.Add("Не проставлены метки в расходе " + item.Number);
				else
				{
					if (!marks.IsAccountingChecked)
						errors.Add("Не проставлена метка 'Расход проверен' в расходе " + item.Number);
				}
			}
		}

		void CheckOrderIncomeMarksRejected(Order order, List<string> errors)
		{
			var accountings = accountingLogic.GetAccountingsByOrder(order.ID).Where(w => w.IsIncome);
			foreach (var item in accountings)
			{
				var marks = accountingLogic.GetAccountingMarkByAccounting(item.ID);
				if (marks == null)
					errors.Add("Не проставлены метки в доходе " + item.Number);
				else
				{
					if (marks.IsAccountingRejected || marks.IsActRejected || marks.IsInvoiceRejected)
						errors.Add("Не должна присутствовать метка 'отклонен' " + item.Number);
				}
			}
		}

		void CheckOrderExpenseMarksRejected(Order order, List<string> errors)
		{
			var accountings = accountingLogic.GetAccountingsByOrder(order.ID).Where(w => w.IsIncome);
			foreach (var item in accountings)
			{
				var marks = accountingLogic.GetAccountingMarkByAccounting(item.ID);
				if (marks == null)
					errors.Add("Не проставлены метки в расходе " + item.Number);
				else
				{
					if (marks.IsAccountingRejected || marks.IsActRejected || marks.IsInvoiceRejected)
						errors.Add("Не должна присутствовать метка 'отклонен' " + item.Number);
				}
			}
		}

		void CheckOrderClaim(Order order, List<string> errors)
		{
			//var templatedDocuments = documentLogic.GetTemplatedDocumentsByOrder(order.ID);
			//if (!templatedDocuments.Any(w => w.TemplateId == 2))
			//	errors.Add("Не сформирована Заявка клиента");
			var documents = documentLogic.GetDocumentsByOrder(order.ID);
			if (!documents.Any(w => w.DocumentTypeId == 23))
				errors.Add("Не прикреплена Заявка клиента");
		}

		void CheckOrderSpecification(Order order, List<string> errors)
		{
			var documents = documentLogic.GetDocumentsByOrder(order.ID);
			if (!documents.Any(w => w.DocumentTypeId == 53))
				errors.Add("Не прикреплена Спецификация");
		}

		void CheckOrderDocuments(Order order, List<string> errors)
		{
			var documents = documentLogic.GetDocumentsByOrder(order.ID);
			if (documents.Any(w => string.IsNullOrEmpty(w.Filename)))
				errors.Add("В заказе присутствуют документы, к которым не прикреплены реальные файлы");
		}

		#endregion

		void RecalculateTrash(Accounting accounting, IEnumerable<RouteSegment> route)
		{
			int GOCounter = 0;
			int GPCounter = 0;

			DateTime loadingDate = new DateTime(1901, 1, 1, 0, 0, 0, DateTimeKind.Local);
			DateTime unloadingDate = new DateTime(1901, 1, 1, 0, 0, 0, DateTimeKind.Local);

			string strFirspPointGO = "";
			string strFirstPointGP = "";
			string strFirstPointGPLast = "";
			string strFirspPointGOEN = "";
			string strFirstPointGPEN = "";
			string strFirstPointGPENLast = "";
			string strCurMarshrut = "";
			string strExportTO = "";
			string strImportTO = "";
			string strGOnameINN = "";
			string strGOcontact = "";
			string strGOcontTel = "";
			string strGOtime = "";
			string strGOcomment = "";
			string strGPnameINN = "";
			string strGPcontact = "";
			string strGPcontTel = "";
			string strGPtime = "";
			string strGPcomment = "";
			string strGPnameINNLast = "";
			string strGPcontactLast = "";
			string strGPcontTelLast = "";
			string strGPtimeLast = "";
			string strGPcommentLast = "";
			string strCurMarshrutEN = "";
			string strExportTOEN = "";
			string strImportTOEN = "";
			string strGOnameINNEN = "";
			string strGOcontactEN = "";
			string strGOcontTelEN = "";
			string strGOtimeEN = "";
			string strGOcommentEN = "";
			string strGPnameINNEN = "";
			string strGPcontactEN = "";
			string strGPcontTelEN = "";
			string strGPtimeEN = "";
			string strGPcommentEN = "";
			string strGPnameINNENLast = "";
			string strGPcontactENLast = "";
			string strGPcontTelENLast = "";
			string strGPtimeENLast = "";
			string strGPcommentENLast = "";
			string strCurMarshrutFE = "";
			string strCurMarshrutLE = "";
			bool isFirstSegment = false;
			bool isLastSegment = false;
			bool blnFEforDates = false;

			foreach (var segment in route)
			{
				isLastSegment = false;
				var pointFrom = orderLogic.GetRoutePoint(segment.FromRoutePointId);
				var placeFrom = dataLogic.GetPlace(pointFrom.PlaceId.Value);
				var countryFrom = dataLogic.GetCountry(placeFrom.CountryId.Value);

				var pointTo = orderLogic.GetRoutePoint(segment.ToRoutePointId);
				var placeTo = dataLogic.GetPlace(pointTo.PlaceId.Value);
				var countryTo = dataLogic.GetCountry(placeTo.CountryId.Value);

				if (blnFEforDates == false)
				{
					loadingDate = pointFrom.PlanDate;
					strCurMarshrutFE = "Пункт загрузки:" + countryFrom.Name + ", " + placeFrom.Name + " " + pointFrom.Address;
					blnFEforDates = true;
				}

				strCurMarshrutLE = "Пункт разгрузки:" + countryTo.Name + ", " + placeTo.Name + " " + pointTo.Address;
				unloadingDate = pointTo.PlanDate;

				if (pointFrom.RoutePointTypeId == 1)    // Пункт загрузки
				{
					var ptype = dataLogic.GetRoutePointTypes().First(w => w.ID == pointFrom.RoutePointTypeId);
					strCurMarshrut = strCurMarshrut + (char)10 + ptype.Display + ":" + countryFrom.Name + ", " + placeFrom.Name + " " + pointFrom.Address;
					strCurMarshrutEN = strCurMarshrutEN + (char)10 + ptype.EnDisplay + ": " + countryFrom.EnName + ", " + placeFrom.EnName + " " + pointFrom.EnAddress;
				}

				if (pointTo.RoutePointTypeId == 3)  // Пункт разгрузки
				{
					var ptype = dataLogic.GetRoutePointTypes().First(w => w.ID == pointTo.RoutePointTypeId);
					strCurMarshrut = strCurMarshrut + (char)10 + ptype.Display + ":" + countryTo.Name + ", " + placeTo.Name + " " + pointFrom.Address;
					strCurMarshrutEN = strCurMarshrutEN + (char)10 + ptype.EnDisplay + ": " + countryTo.EnName + ", " + placeTo.EnName + " " + pointFrom.EnAddress;
					isLastSegment = true;
				}

				if (pointFrom.RoutePointTypeId == 5)    // Экспортное ТО
				{
					strExportTO = pointFrom.Address;
					strExportTOEN = pointFrom.EnAddress;
				}

				if (pointFrom.RoutePointTypeId == 6)    // Импортное ТО
				{
					strImportTO = pointFrom.Address;
					strImportTOEN = pointFrom.EnAddress;
				}

				if (pointTo.RoutePointTypeId == 5)  // Экспортное ТО
				{
					strExportTO = pointTo.Address;
					strExportTOEN = pointTo.EnAddress;
				}

				if (pointTo.RoutePointTypeId == 6)  // Импортное ТО
				{
					strImportTO = pointTo.Address;
					strImportTOEN = pointTo.EnAddress;
				}

				//сохраняем информацию о грузоотправителях/грузополучателях
				if (pointFrom.RoutePointTypeId == 1) //1-пункт загрузки
				{
					if (isFirstSegment)
					{
						if (pointFrom.ParticipantLegalId != null)
						{
							var legal = legalLogic.GetLegal(pointFrom.ParticipantLegalId.Value);
							placeFrom = dataLogic.GetPlace(pointFrom.PlaceId.Value);
							if (GOCounter == 0)
							{
								strFirspPointGO = placeFrom.Name;
								strFirspPointGOEN = placeFrom.EnName;
							}

							if (strGOnameINN != "")
							{ strGOnameINN = strGOnameINN + "; "; }
							if (strGOcontact != "")
							{ strGOcontact = strGOcontact + "; "; }
							if (strGOcontTel != "")
							{ strGOcontTel = strGOcontTel + "; "; }
							if (strGOtime != "")
							{ strGOtime = strGOtime + "; "; }
							if (strGOcomment != "")
							{ strGOcomment = strGOcomment + "; "; }
							if (strGOnameINNEN != "")
							{ strGOnameINNEN = strGOnameINNEN + "; "; }
							if (strGOcontactEN != "")
							{ strGOcontactEN = strGOcontactEN + "; "; }
							if (strGOcontTelEN != "")
							{ strGOcontTelEN = strGOcontTelEN + "; "; }
							if (strGOtimeEN != "")
							{ strGOtimeEN = strGOtimeEN + "; "; }
							if (strGOcommentEN != "")
							{ strGOcommentEN = strGOcommentEN + "; "; }

							if (GOCounter == 0)
							{
								strGOnameINN = legal.DisplayName + ", ИНН " + legal.TIN;
								if (!pointFrom.RouteContactID.HasValue)
								{
									strGOcontactEN = "Не выбраны контакты для пункта маршрута";
									strGOcontTelEN = "Не выбраны контакты для пункта маршрута";
								}
								else
								{
									var contact = legalLogic.GetRouteContact(pointFrom.RouteContactID.Value);
									strGOcontact = contact.Contact;
									strGOcontTel = contact.Phones;
									strGOcontactEN = contact.EnContact;
									strGOcontTelEN = contact.Phones;
								}

								strGOtime = legal.WorkTime;
								strGOcomment = pointFrom.ParticipantComment;
								strGOnameINNEN = legal.EnShortName ?? legal.DisplayName + ", TIN " + legal.TIN;
								strGOtimeEN = legal.WorkTime;
								strGOcommentEN = pointFrom.ParticipantComment;
							}
							else
							{
								strGOnameINN = strGOnameINN + placeFrom.Name + ": " + legal.DisplayName + ", ИНН " + legal.TIN;
								if (!pointFrom.RouteContactID.HasValue)
								{
									strGOcontactEN = "Не выбраны контакты для пункта маршрута";
									strGOcontTelEN = "Не выбраны контакты для пункта маршрута";
								}
								else
								{
									var contact = legalLogic.GetRouteContact(pointFrom.RouteContactID.Value);
									strGOcontact = strGOcontact + placeFrom.Name + ": " + contact.Contact;
									strGOcontTel = strGOcontTel + placeFrom.Name + ": " + contact.Phones;
									strGOcontactEN = strGOcontactEN + placeFrom.Name + ": " + contact.EnContact;
									strGOcontTelEN = strGOcontTelEN + placeFrom.EnName + ": " + contact.Phones;
								}

								strGOtime = strGOtime + placeFrom.Name + ": " + legal.WorkTime;
								strGOcomment = strGOcomment + placeFrom.Name + ": " + pointFrom.ParticipantComment;
								strGOnameINNEN = strGOnameINNEN + placeFrom.Name + ": " + legal.EnShortName ?? legal.DisplayName + ", TIN " + legal.TIN;
								strGOtimeEN = strGOtimeEN + placeFrom.EnName + ": " + legal.WorkTime;
								strGOcommentEN = strGOcommentEN + placeFrom.EnName + ": " + pointFrom.ParticipantComment;
							}

							GOCounter++;
						}
					}
				}

				if (!isFirstSegment)
				{
					if (pointFrom.ParticipantLegalId.HasValue)
					{
						var legal = legalLogic.GetLegal(pointFrom.ParticipantLegalId.Value);
						placeFrom = dataLogic.GetPlace(pointFrom.PlaceId.Value);
						strFirspPointGO = placeFrom.Name;
						strFirspPointGOEN = placeFrom.EnName;
						strGOnameINN = legal.DisplayName + ", ИНН " + legal.TIN;
						if (!pointFrom.RouteContactID.HasValue)
						{
							strGOcontact = "Не выбраны контакты для пункта маршрута";
							strGOcontTel = "Не выбраны контакты для пункта маршрута";
						}
						else
						{
							var contact = legalLogic.GetRouteContact(pointFrom.RouteContactID.Value);
							strGOcontact = contact.Contact;
							strGOcontTel = contact.Phones;
							strGOcontactEN = contact.EnContact;
							strGOcontTelEN = contact.Phones;
						}

						strGOtime = legal.WorkTime;
						strGOcomment = pointFrom.ParticipantComment;
						strGOnameINNEN = legal.EnShortName ?? legal.DisplayName + ", TIN " + legal.TIN;
						strGOtimeEN = legal.WorkTime;
						strGOcommentEN = pointFrom.ParticipantComment;
						isFirstSegment = true;

						GOCounter++;
					}
				}

				if (pointTo.ParticipantLegalId.HasValue)
				{
					placeTo = dataLogic.GetPlace(pointTo.PlaceId.Value);
					var legalTo = legalLogic.GetLegal(pointTo.ParticipantLegalId.Value);
					strFirstPointGPLast = placeTo.Name;
					strFirstPointGPENLast = placeTo.EnName;
					strGPnameINNLast = legalTo.DisplayName + ", ИНН " + legalTo.TIN;
					if (!pointTo.RouteContactID.HasValue)
					{
						strGPcontactLast = "Не выбраны контакты для пункта маршрута";
						strGPcontTelLast = "Не выбраны контакты для пункта маршрута";
					}
					else
					{
						var contact = legalLogic.GetRouteContact(pointTo.RouteContactID.Value);
						strGPcontactLast = contact.Contact;
						strGPcontTelLast = contact.Phones;
						strGPcontactENLast = contact.EnContact;
						strGPcontTelENLast = contact.Phones;
					}

					strGPtimeLast = legalTo.WorkTime;
					strGPcommentLast = pointTo.ParticipantComment;
					strGPnameINNENLast = legalTo.EnShortName ?? legalTo.DisplayName + ", TIN " + legalTo.TIN;
					strGPtimeENLast = legalTo.WorkTime;
					strGPcommentENLast = pointTo.ParticipantComment;
				}

				if (pointTo.RoutePointTypeId == 3) //3-пункт разгрузки
				{
					if (pointTo.ParticipantLegalId.HasValue)
					{
						placeTo = dataLogic.GetPlace(pointTo.PlaceId.Value);
						var legalTo = legalLogic.GetLegal(pointTo.ParticipantLegalId.Value);
						if (GPCounter == 0)
						{
							strFirstPointGP = placeTo.Name;
							strFirstPointGPEN = placeTo.EnName;
						}

						if (strGPnameINN != "")
						{ strGPnameINN = strGPnameINN + "; "; }
						if (strGPcontact != "")
						{ strGPcontact = strGPcontact + "; "; }
						if (strGPcontTel != "")
						{ strGPcontTel = strGPcontTel + "; "; }
						if (strGPtime != "")
						{ strGPtime = strGPtime + "; "; }
						if (strGPcomment != "")
						{ strGPcomment = strGPcomment + "; "; }
						if (strGPnameINNEN != "")
						{ strGPnameINNEN = strGPnameINNEN + "; "; }
						if (strGPcontactEN != "")
						{ strGPcontactEN = strGPcontactEN + "; "; }
						if (strGPcontTelEN != "")
						{ strGPcontTelEN = strGPcontTelEN + "; "; }
						if (strGPtimeEN != "")
						{ strGPtimeEN = strGPtimeEN + "; "; }
						if (strGPcommentEN != "")
						{ strGPcommentEN = strGPcommentEN + "; "; }

						if (GPCounter == 0)
						{
							strGPnameINN = legalTo.DisplayName + ", ИНН " + legalTo.TIN;

							if (!pointTo.RouteContactID.HasValue)
							{
								strGPcontact = "Не выбраны контакты для пункта маршрута";
								strGPcontTel = "Не выбраны контакты для пункта маршрута";
							}
							else
							{
								var contact = legalLogic.GetRouteContact(pointTo.RouteContactID.Value);
								strGPcontact = contact.Contact;
								strGPcontTel = contact.Phones;
								strGPcontactEN = contact.EnContact;
								strGPcontTelEN = contact.Phones;
							}

							strGPtime = legalTo.WorkTime;
							strGPcomment = pointTo.ParticipantComment;
							strGPnameINNEN = legalTo.EnShortName ?? legalTo.DisplayName + ", TIN " + legalTo.TIN;
							strGPtimeEN = legalTo.WorkTime;
							strGPcommentEN = pointTo.ParticipantComment;
						}
						else
						{
							strGPnameINN = strGPnameINN + placeTo.Name + ": " + legalTo.DisplayName + ", ИНН " + legalTo.TIN;
							if (!pointTo.RouteContactID.HasValue)
							{
								strGPcontact = "Не выбраны контакты для пункта маршрута";
								strGPcontTel = "Не выбраны контакты для пункта маршрута";
							}
							else
							{
								var contact = legalLogic.GetRouteContact(pointTo.RouteContactID.Value);
								strGPcontact = strGPcontact + placeTo.Name + ": " + contact.Contact;
								strGPcontTel = strGPcontTel + placeTo.Name + ": " + contact.Phones;
								strGPcontactEN = strGPcontactEN + placeTo.EnName + ": " + contact.EnContact;
								strGPcontTelEN = strGPcontTelEN + placeTo.EnName + ": " + contact.Phones;
							}

							strGPtime = strGPtime + placeTo.Name + ": " + legalTo.WorkTime;
							strGPcomment = strGPcomment + placeTo.Name + ": " + pointTo.ParticipantComment;
							strGPnameINNEN = strGPnameINNEN + placeTo.EnName + ": " + legalTo.EnShortName ?? legalTo.DisplayName + ", TIN " + legalTo.TIN;
							strGPtimeEN = strGPtimeEN + placeTo.EnName + ": " + legalTo.WorkTime;
							strGPcommentEN = strGPcommentEN + placeTo.EnName + ": " + pointTo.ParticipantComment;
						}

						GPCounter++;
					}
				}
			}

			if (isLastSegment)
				accounting.Route = /*strCurMarshrutFE + */strCurMarshrut;
			else
				accounting.Route = /*strCurMarshrutFE +*/ strCurMarshrut + "; " + (char)10 + strCurMarshrutLE;

			accounting.ecnExportTO = strExportTO;
			accounting.ecnImportTO = strImportTO;
			accounting.ecnMarshrutEN = strCurMarshrutEN;
			accounting.ecnExportTOEN = strExportTOEN;
			accounting.ecnImportTOEN = strImportTOEN;

			if (GOCounter > 1)
			{
				accounting.ecnGOnameINN = strFirspPointGO + ": " + strGOnameINN;
				accounting.ecnGOcontact = strFirspPointGO + ": " + strGOcontact;
				accounting.ecnGOcontTel = strFirspPointGO + ": " + strGOcontTel;
				accounting.ecnGOtime = strFirspPointGO + ": " + strGOtime;
				accounting.ecnGOcomment = strFirspPointGO + ": " + strGOcomment;
				accounting.ecnGOnameINNEN = strFirspPointGOEN + ": " + strGOnameINNEN;
				accounting.ecnGOcontactEN = strFirspPointGOEN + ": " + strGOcontactEN;
				accounting.strGOcontTelEN = strFirspPointGOEN + ": " + strGOcontTelEN;
				accounting.ecnGOtimeEN = strFirspPointGOEN + ": " + strGOtimeEN;
				accounting.ecnGOcommentEN = strFirspPointGOEN + ": " + strGOcommentEN;
			}
			else
			{
				accounting.ecnGOnameINN = strGOnameINN;
				accounting.ecnGOcontact = strGOcontact;
				accounting.ecnGOcontTel = strGOcontTel;
				accounting.ecnGOtime = strGOtime;
				accounting.ecnGOcomment = strGOcomment;
				accounting.ecnGOnameINNEN = strGOnameINNEN;
				accounting.ecnGOcontactEN = strGOcontactEN;
				accounting.strGOcontTelEN = strGOcontTelEN;
				accounting.ecnGOtimeEN = strGOtimeEN;
				accounting.ecnGOcommentEN = strGOcommentEN;
			}

			if (GPCounter > 1)
			{
				if (isLastSegment == true)
				{
					accounting.ecnGPnameINN = strFirstPointGP + ": " + strGPnameINN;
					accounting.ecnGPcontact = strFirstPointGP + ": " + strGPcontact;
					accounting.ecnGPconTel = strFirstPointGP + ": " + strGPcontTel;
					accounting.ecnGPtime = strFirstPointGP + ": " + strGPtime;
					accounting.ecnGPcomment = strFirstPointGP + ": " + strGPcomment;
					accounting.ecnGPnameINNEN = strFirstPointGPEN + ": " + strGPnameINNEN;
					accounting.ecnGPcontactEN = strFirstPointGPEN + ": " + strGPcontactEN;
					accounting.strGPcontTelEN = strFirstPointGPEN + ": " + strGPcontTelEN;
					accounting.ecnGPtimeEN = strFirstPointGPEN + ": " + strGPtimeEN;
					accounting.ecnGPcommentEN = strFirstPointGPEN + ": " + strGPcommentEN;
				}
				else
				{
					accounting.ecnGPnameINN = strFirstPointGP + ": " + strGPnameINN + ";" + strFirstPointGPLast + ": " + strGPnameINNLast;
					accounting.ecnGPcontact = strFirstPointGP + ": " + strGPcontact + ";" + strFirstPointGPLast + ": " + strGPcontactLast;
					accounting.ecnGPconTel = strFirstPointGP + ": " + strGPcontTel + ";" + strFirstPointGPLast + ": " + strGPcontTelLast;
					accounting.ecnGPtime = strFirstPointGP + ": " + strGPtime + ";" + strFirstPointGPLast + ": " + strGPtimeLast;
					accounting.ecnGPcomment = strFirstPointGP + ": " + strGPcomment + ";" + strFirstPointGPLast + ": " + strGPcommentLast;
					accounting.ecnGPnameINNEN = strFirstPointGPEN + ": " + strGPnameINNEN + ";" + strFirstPointGPENLast + ": " + strGPnameINNENLast;
					accounting.ecnGPcontactEN = strFirstPointGPEN + ": " + strGPcontactEN + ";" + strFirstPointGPENLast + ": " + strGPcontactENLast;
					accounting.strGPcontTelEN = strFirstPointGPEN + ": " + strGPcontTelEN + ";" + strFirstPointGPENLast + ": " + strGPcontTelENLast;
					accounting.ecnGPtimeEN = strFirstPointGPEN + ": " + strGPtimeEN + ";" + strFirstPointGPENLast + ": " + strGPtimeENLast;
					accounting.ecnGPcommentEN = strFirstPointGPEN + ": " + strGPcommentEN + ";" + strFirstPointGPENLast + ": " + strGPcommentENLast;
				}
			}
			else
			{
				if (GPCounter == 1)
				{
					if (isLastSegment)
					{
						accounting.ecnGPnameINN = strGPnameINN;
						accounting.ecnGPcontact = strGPcontact;
						accounting.ecnGPconTel = strGPcontTel;
						accounting.ecnGPtime = strGPtime;
						accounting.ecnGPcomment = strGPcomment;
						accounting.ecnGPnameINNEN = strGPnameINNEN;
						accounting.ecnGPcontactEN = strGPcontactEN;
						accounting.strGPcontTelEN = strGPcontTelEN;
						accounting.ecnGPtimeEN = strGPtimeEN;
						accounting.ecnGPcommentEN = strGPcommentEN;
					}
					else
					{
						accounting.ecnGPnameINN = strFirstPointGP + ": " + strGPnameINN + ";" + strFirstPointGPLast + ": " + strGPnameINNLast;
						accounting.ecnGPcontact = strFirstPointGP + ": " + strGPcontact + ";" + strFirstPointGPLast + ": " + strGPcontactLast;
						accounting.ecnGPconTel = strFirstPointGP + ": " + strGPcontTel + ";" + strFirstPointGPLast + ": " + strGPcontTelLast;
						accounting.ecnGPtime = strFirstPointGP + ": " + strGPtime + ";" + strFirstPointGPLast + ": " + strGPtimeLast;
						accounting.ecnGPcomment = strFirstPointGP + ": " + strGPcomment + ";" + strFirstPointGPLast + ": " + strGPcommentLast;
						accounting.ecnGPnameINNEN = strFirstPointGPEN + ": " + strGPnameINNEN + ";" + strFirstPointGPENLast + ": " + strGPnameINNENLast;
						accounting.ecnGPcontactEN = strFirstPointGPEN + ": " + strGPcontactEN + ";" + strFirstPointGPENLast + ": " + strGPcontactENLast;
						accounting.strGPcontTelEN = strFirstPointGPEN + ": " + strGPcontTelEN + ";" + strFirstPointGPENLast + ": " + strGPcontTelENLast;
						accounting.ecnGPtimeEN = strFirstPointGPEN + ": " + strGPtimeEN + ";" + strFirstPointGPENLast + ": " + strGPtimeENLast;
						accounting.ecnGPcommentEN = strFirstPointGPEN + ": " + strGPcommentEN + ";" + strFirstPointGPENLast + ": " + strGPcommentENLast;
					}
				}
				else
				{
					accounting.ecnGPnameINN = strGPnameINNLast;
					accounting.ecnGPcontact = strGPcontactLast;
					accounting.ecnGPconTel = strGPcontTelLast;
					accounting.ecnGPtime = strGPtimeLast;
					accounting.ecnGPcomment = strGPcommentLast;
					accounting.ecnGPnameINNEN = strGPnameINNENLast;
					accounting.ecnGPcontactEN = strGPcontactENLast;
					accounting.strGPcontTelEN = strGPcontTelENLast;
					accounting.ecnGPtimeEN = strGPtimeENLast;
					accounting.ecnGPcommentEN = strGPcommentENLast;
				}
			}

			accounting.ecnLoadingDate = loadingDate;
			accounting.ecnUnloadingDate = unloadingDate;
		}

		void SendStatusChangeAlerts(int orderId, int orderStatusId, string reason)
		{
			var order = orderLogic.GetOrder(orderId);
			var workgroup = participantLogic.GetWorkgroupByOrder(orderId);

			var subject = "";
			var message = "";
			switch (orderStatusId)
			{
				case 1: // -> Отказ 
					subject = order.Number + " перешел в статус 'Отказ'";
					message = "{0}, заказ <a href='http://cm.corp.local/Orders/Details/{1}'>{2}</a> переведен в статус <b>'{3}'</b>. <br /> Спасибо.";
					foreach (var item in GetParticipantUsers(workgroup, 2, 3))
					{
						var user = identityLogic.GetUser(item);
						var fm = string.Format(message, user.Name, order.ID, order.Number, "Отказ");
						SendMail(user.Email, subject, fm);
					}
					break;

				case 2: // -> Создан 
					subject = order.Number + " перешел в статус 'Создан'";
					message = "{0}, заказ <a href='http://cm.corp.local/Orders/Details/{1}'>{2}</a> переведен в статус <b>'{3}'</b>. <br /> Спасибо.";
					foreach (var item in GetParticipantUsers(workgroup, 2, 3))
					{
						var user = identityLogic.GetUser(item);
						var fm = string.Format(message, user.Name, order.ID, order.Number, "Создан");
						SendMail(user.Email, subject, fm);
					}
					break;

				case 3: // -> В работе 
					subject = order.Number + " перешел в статус 'В работе'";
					message = "{0}, заказ <a href='http://cm.corp.local/Orders/Details/{1}'>{2}</a> переведен в статус <b>'{3}'</b>. <br /> Спасибо.";
					foreach (var item in GetParticipantUsers(workgroup, 2, 3))
					{
						var user = identityLogic.GetUser(item);
						var fm = string.Format(message, user.Name, order.ID, order.Number, "В работе");
						SendMail(user.Email, subject, fm);
					}
					break;

				case 4: // -> Выполнен 
					subject = order.Number + " перешел в статус 'Выполнен'";
					message = "{0}, заказ <a href='http://cm.corp.local/Orders/Details/{1}'>{2}</a> переведен в статус <b>'{3}'</b>. <br /> Спасибо.";
					foreach (var item in GetParticipantUsers(workgroup, 2, 3))
					{
						var user = identityLogic.GetUser(item);
						var fm = string.Format(message, user.Name, order.ID, order.Number, "Выполнен");
						SendMail(user.Email, subject, fm);
					}
					break;

				case 5: // -> Расходы внесены 
					subject = order.Number + " перешел в статус 'Расходы внесены'";
					message = "{0}, заказ <a href='http://cm.corp.local/Orders/Details/{1}'>{2}</a> переведен в статус <b>'{3}'</b>. <br /> Спасибо.";
					foreach (var item in GetParticipantUsers(workgroup, 2, 3))
					{
						var user = identityLogic.GetUser(item);
						var fm = string.Format(message, user.Name, order.ID, order.Number, "Расходы внесены");
						SendMail(user.Email, subject, fm);
					}
					break;

				case 6: // -> Проверен 
					subject = order.Number + " перешел в статус 'Проверен'";
					message = "{0}, заказ <a href='http://cm.corp.local/Orders/Details/{1}'>{2}</a> переведен в статус <b>'{3}'</b>. <br /> Спасибо.";
					foreach (var item in GetParticipantUsers(workgroup, 4))
					{
						var user = identityLogic.GetUser(item);
						var fm = string.Format(message, user.Name, order.ID, order.Number, "Проверен");
						SendMail(user.Email, subject, fm);
					}
					break;

				case 7: // -> Мотивация 
					subject = order.Number + " перешел в статус 'Мотивация'";
					message = "{0}, заказ <a href='http://cm.corp.local/Orders/Details/{1}'>{2}</a> переведен в статус <b>'{3}'</b>. <br /> Спасибо.";
					foreach (var item in GetParticipantUsers(workgroup, 4))
					{
						var user = identityLogic.GetUser(item);
						var fm = string.Format(message, user.Name, order.ID, order.Number, "Мотивация");
						SendMail(user.Email, subject, fm);
					}
					break;

				case 8: // -> Корректировка
					subject = order.Number + " перешел в статус 'Корректировка'";
					message = "{0}, заказ <a href='http://cm.corp.local/Orders/Details/{1}'>{2}</a> переведен в статус <b>'{3}'</b>. Причина:{4} <br /> Спасибо.";
					foreach (var item in GetParticipantUsers(workgroup, 4))
					{
						var user = identityLogic.GetUser(item);
						var fm = string.Format(message, user.Name, order.ID, order.Number, "Корректировка", reason);
						SendMail(user.Email, subject, fm);
					}
					break;

				case 9: // -> Закрыт 
					subject = order.Number + " перешел в статус 'Закрыт'";
					message = "{0}, заказ <a href='http://cm.corp.local/Orders/Details/{1}'>{2}</a> переведен в статус <b>'{3}'</b>. Прошу проставить статус 'Проверен'. <br /> Спасибо.";
					foreach (var item in GetParticipantUsers(workgroup, 2, 3))
					{
						var user = identityLogic.GetUser(item);
						var fm = string.Format(message, user.Name, order.ID, order.Number, "Закрыт");
						SendMail(user.Email, subject, fm);
					}
					break;
			}
		}

		void MoveOrderFilesToArchive(string orderNumber, DateTime date)
		{
			IO.Directory.CreateDirectory(@"\\corpserv03.corp.local\Common\5 Перевозки\Закрытые\" + date.Year.ToString() + "\\" + date.Month.ToString("00"));
			IO.Directory.Move(@"\\corpserv03.corp.local\Common\5 Перевозки\" + orderNumber, @"\\corpserv03.corp.local\Common\5 Перевозки\Закрытые\" + date.Year.ToString() + "\\" + date.Month.ToString("00") + "\\" + orderNumber);
		}

		double GetAccountingRate(Accounting accounting)
		{
			if (accounting.CurrencyRate.Value > 0)
				return accounting.CurrencyRate.Value;

			var services = accountingLogic.GetServicesByAccounting(accounting.ID);
			if (services.Count() > 0)
			{
				var currencyId = services.First().CurrencyId ?? 1;
				var rate = dataLogic.GetCurrencyRates(accounting.InvoiceDate ?? DateTime.Now);
				return rate.Where(w => w.CurrencyId == currencyId).Select(s => s.Rate).FirstOrDefault() ?? 1;
			}

			return 0;
		}

		void CreateUpdateDiffAccounting(int accountingId)
		{
			var accounting = accountingLogic.GetAccounting(accountingId);
			// только для расхода
			if (accounting.IsIncome)
				return;

			var contract = contractLogic.GetContract(accounting.ContractId.Value);
			// кроме "# Финансовые услуги" и "# Авторасходы"
			if ((contract.LegalId == FINSERVICES_LEGAL_ID) || (contract.LegalId == AUTOEXPENSE_LEGAL_ID))
				return;

			var marks = accountingLogic.GetAccountingMarkByAccounting(accountingId);
			var diff = dataLogic.GetCurrencyRateDiffs().Where(w => w.From < marks.AccountingCheckedDate.Value && w.To > marks.AccountingCheckedDate.Value).FirstOrDefault();
			// если нет курсовой разницы на эту дату
			if (diff == null)
				return;

			var accountings = accountingLogic.GetAccountingsByOrder(accounting.OrderId);
			var order = orderLogic.GetOrder(accounting.OrderId);
			var diffAccounting = accountings.Where(w => w.Comment == accounting.Number).FirstOrDefault();

			if (diffAccounting == null)
			{
				diffAccounting = new Accounting
				{
					OrderId = accounting.OrderId,
					IsIncome = accounting.IsIncome,
					AccountingDocumentTypeId = accounting.IsIncome ? 1 : 2,
					AccountingPaymentMethodId = 2,
					AccountingPaymentTypeId = accounting.IsIncome ? 2 : 2,
					AccountingDate = marks.AccountingCheckedDate,
					RequestDate = marks.AccountingCheckedDate,
					CreatedDate = DateTime.Now,
					InvoiceDate = marks.AccountingCheckedDate,
					ActDate = marks.AccountingCheckedDate,
					InvoiceNumber = accounting.Number,
					ActNumber = accounting.Number,
					Comment = accounting.Number,
					PayMethodId = accounting.PayMethodId,
					OurLegalId = accounting.OurLegalId,
					ContractId = contractLogic.GetContractsByLegal(DIFF_LEGAL_ID).Where(w => w.OurLegalId == contract.OurLegalId).Select(s => s.ID).FirstOrDefault()
				};

				#region new number

				int dirNo = accountings.Where(w => !w.IsIncome).Max(s => s.SameDirectionNo);

				diffAccounting.Number = order.FinRepCenterId + order.ID.ToString("D7") + "-" + (dirNo + 201);
				diffAccounting.SameDirectionNo = ++dirNo;

				#endregion
			}

			var service = accountingLogic.GetServicesByAccounting(accountingId).FirstOrDefault();
			if (service == null)
				return;

			var currencyId = service.CurrencyId ?? 1;
			var contractCurrencies = contractLogic.GetContractCurrencies(accounting.ContractId.Value);
			var currency = contractCurrencies.FirstOrDefault(w => w.CurrencyId == currencyId);
			if (currency == null)
				currency = contractCurrencies.FirstOrDefault(w => w.CurrencyId == 1);   // рубли присутствуют обязательно в этом случае

			if ((currency == null) || (currency.CurrencyId == 1))
				return;

			switch (currency.CurrencyId)
			{
				//case 1:	// RUB
				//	diffAccounting.OriginalSum = accounting.OriginalSum;
				//	break;

				case 2: // USD
					diffAccounting.OriginalSum = accounting.OriginalSum * diff.USD;
					break;

				case 3: // EUR
					diffAccounting.OriginalSum = accounting.OriginalSum * diff.EUR;
					break;

				case 5: // CNY 
					diffAccounting.OriginalSum = accounting.OriginalSum * diff.CNY;
					break;

				case 6: // GBP
					diffAccounting.OriginalSum = accounting.OriginalSum * diff.GBP;
					break;
			}

			if (diffAccounting.ID == 0)
			{
				var id = accountingLogic.CreateAccounting(diffAccounting);
				var serviceKind = dataLogic.GetServiceKinds().First(w => (w.ProductId == order.ProductId) && w.Name.Contains("Финанс"));
				var serviceType = dataLogic.GetServiceTypes().First(w => (w.ServiceKindId == serviceKind.ID) && w.Name.Contains("Курсов"));
				var serviceId = accountingLogic.CreateService(new Service
				{
					Count = 1,
					CurrencyId = currency.CurrencyId,
					AccountingId = id,
					Price = diffAccounting.OriginalSum,
					ServiceTypeId = serviceType.ID,
					VatId = 2   // без НДС
				});

				accountingLogic.CalculateServiceBalance(serviceId);
				accountingLogic.CalculateAccountingBalance(id);
				orderLogic.CalculateOrderBalance(diffAccounting.OrderId);

				accountingLogic.CreateAccountingMark(new AccountingMark
				{
					AccountingCheckedDate = marks.AccountingCheckedDate,
					AccountingCheckedUserId = marks.AccountingCheckedUserId,
					AccountingId = id,
					AccountingOkDate = marks.AccountingOkDate,
					AccountingOkUserId = marks.AccountingOkUserId,
					AccountingRejectedComment = marks.AccountingRejectedComment,
					AccountingRejectedDate = marks.AccountingRejectedDate,
					AccountingRejectedUserId = marks.AccountingRejectedUserId,
					ActCheckedDate = marks.ActCheckedDate,
					ActCheckedUserId = marks.ActCheckedUserId,
					ActOkDate = marks.ActOkDate,
					ActOkUserId = marks.ActOkUserId,
					ActRejectedComment = marks.ActRejectedComment,
					ActRejectedDate = marks.ActRejectedDate,
					ActRejectedUserId = marks.ActRejectedUserId,
					InvoiceCheckedDate = marks.InvoiceCheckedDate,
					InvoiceCheckedUserId = marks.InvoiceCheckedUserId,
					InvoiceOkDate = marks.InvoiceOkDate,
					InvoiceOkUserId = marks.InvoiceOkUserId,
					InvoiceRejectedComment = marks.InvoiceRejectedComment,
					InvoiceRejectedDate = marks.InvoiceRejectedDate,
					InvoiceRejectedUserId = marks.InvoiceRejectedUserId,
					IsAccountingChecked = marks.IsAccountingChecked,
					IsAccountingOk = marks.IsAccountingOk,
					IsAccountingRejected = marks.IsAccountingRejected,
					IsActChecked = marks.IsActChecked,
					IsActOk = marks.IsActOk,
					IsActRejected = marks.IsActRejected,
					IsInvoiceChecked = marks.IsInvoiceChecked,
					IsInvoiceOk = marks.IsInvoiceOk,
					IsInvoiceRejected = marks.IsInvoiceRejected
				});
			}
			else
			{
				accountingLogic.UpdateAccounting(diffAccounting);

				var diffService = accountingLogic.GetServicesByAccounting(diffAccounting.ID).First();
				diffService.Price = diffAccounting.OriginalSum;

				accountingLogic.CalculateServiceBalance(diffService.ID);
				accountingLogic.CalculateAccountingBalance(diffAccounting.ID);
				orderLogic.CalculateOrderBalance(diffAccounting.OrderId);

				var diffMarks = accountingLogic.GetAccountingMarkByAccounting(diffAccounting.ID);

				diffMarks.AccountingCheckedDate = marks.AccountingCheckedDate;
				diffMarks.AccountingCheckedUserId = marks.AccountingCheckedUserId;
				diffMarks.AccountingId = accounting.ID;
				diffMarks.AccountingOkDate = marks.AccountingOkDate;
				diffMarks.AccountingOkUserId = marks.AccountingOkUserId;
				diffMarks.AccountingRejectedComment = marks.AccountingRejectedComment;
				diffMarks.AccountingRejectedDate = marks.AccountingRejectedDate;
				diffMarks.AccountingRejectedUserId = marks.AccountingRejectedUserId;
				diffMarks.ActCheckedDate = marks.ActCheckedDate;
				diffMarks.ActCheckedUserId = marks.ActCheckedUserId;
				diffMarks.ActOkDate = marks.ActOkDate;
				diffMarks.ActOkUserId = marks.ActOkUserId;
				diffMarks.ActRejectedComment = marks.ActRejectedComment;
				diffMarks.ActRejectedDate = marks.ActRejectedDate;
				diffMarks.ActRejectedUserId = marks.ActRejectedUserId;
				diffMarks.InvoiceCheckedDate = marks.InvoiceCheckedDate;
				diffMarks.InvoiceCheckedUserId = marks.InvoiceCheckedUserId;
				diffMarks.InvoiceOkDate = marks.InvoiceOkDate;
				diffMarks.InvoiceOkUserId = marks.InvoiceOkUserId;
				diffMarks.InvoiceRejectedComment = marks.InvoiceRejectedComment;
				diffMarks.InvoiceRejectedDate = marks.InvoiceRejectedDate;
				diffMarks.InvoiceRejectedUserId = marks.InvoiceRejectedUserId;
				diffMarks.IsAccountingChecked = marks.IsAccountingChecked;
				diffMarks.IsAccountingOk = marks.IsAccountingOk;
				diffMarks.IsAccountingRejected = marks.IsAccountingRejected;
				diffMarks.IsActChecked = marks.IsActChecked;
				diffMarks.IsActOk = marks.IsActOk;
				diffMarks.IsActRejected = marks.IsActRejected;
				diffMarks.IsInvoiceChecked = marks.IsInvoiceChecked;
				diffMarks.IsInvoiceOk = marks.IsInvoiceOk;
				diffMarks.IsInvoiceRejected = marks.IsInvoiceRejected;

				accountingLogic.UpdateAccountingMark(diffMarks);
			}
		}

		void CreateUpdateAutoExpenseAccounting(int accountingId)
		{
			var accounting = accountingLogic.GetAccounting(accountingId);
			// только для расхода
			if (accounting.IsIncome)
				return;

			var contract = contractLogic.GetContract(accounting.ContractId.Value);
			// только для "# Финансовые услуги"
			if (contract.LegalId != FINSERVICES_LEGAL_ID)
				return;

			var service = accountingLogic.GetServicesByAccounting(accountingId).FirstOrDefault();
			if (service == null)
				return;

			var marks = accountingLogic.GetAccountingMarkByAccounting(accountingId);
			var ae = dataLogic.GetAutoExpenses().Where(w => w.From < marks.AccountingCheckedDate.Value && w.To > marks.AccountingCheckedDate.Value).FirstOrDefault();
			// если нет на эту дату
			if (ae == null)
				return;

			var accountings = accountingLogic.GetAccountingsByOrder(accounting.OrderId);
			var order = orderLogic.GetOrder(accounting.OrderId);
			var aeAccounting = accountings.Where(w => w.Comment == "ae " + accounting.Number).FirstOrDefault();

			if (aeAccounting == null)
			{
				aeAccounting = new Accounting
				{
					OrderId = accounting.OrderId,
					//No = accountings.Count() + 1,
					CreatedDate = DateTime.Now,
					IsIncome = accounting.IsIncome,
					AccountingDocumentTypeId = accounting.IsIncome ? 1 : 2,
					AccountingPaymentMethodId = 2,
					AccountingPaymentTypeId = accounting.IsIncome ? 2 : 2,
					AccountingDate = marks.AccountingCheckedDate,
					RequestDate = marks.AccountingCheckedDate,
					InvoiceDate = marks.AccountingCheckedDate,
					ActDate = marks.AccountingCheckedDate,
					InvoiceNumber = accounting.Number,
					ActNumber = accounting.Number,
					Comment = "ae " + accounting.Number,
					PayMethodId = accounting.PayMethodId,
					OurLegalId = accounting.OurLegalId,
					ContractId = contractLogic.GetContractsByLegal(AUTOEXPENSE_LEGAL_ID).Where(w => w.OurLegalId == contract.OurLegalId).Select(s => s.ID).FirstOrDefault()
				};

				#region new number

				int dirNo = accountings.Where(w => !w.IsIncome).Max(s => s.SameDirectionNo);

				aeAccounting.Number = order.FinRepCenterId + order.ID.ToString("D7") + "-" + (dirNo + 201);
				aeAccounting.SameDirectionNo = ++dirNo;

				#endregion
			}

			var currencyId = service.CurrencyId ?? 1;
			var contractCurrencies = contractLogic.GetContractCurrencies(accounting.ContractId.Value);
			var currency = contractCurrencies.FirstOrDefault(w => w.CurrencyId == currencyId);
			if (currency == null)
				currency = contractCurrencies.FirstOrDefault(w => w.CurrencyId == 1);   // рубли присутствуют обязательно в этом случае

			if ((currency == null) || (currency.CurrencyId == 1))
				return;

			switch (currency.CurrencyId)
			{
				//case 1:	// RUB
				//	diffAccounting.OriginalSum = accounting.OriginalSum;
				//	break;

				case 2: // USD
					aeAccounting.OriginalSum = accounting.OriginalSum * ae.USD;
					break;

				case 3: // EUR
					aeAccounting.OriginalSum = accounting.OriginalSum * ae.EUR;
					break;

				case 5: // CNY 
					aeAccounting.OriginalSum = accounting.OriginalSum * ae.CNY;
					break;

				case 6: // GBP
					aeAccounting.OriginalSum = accounting.OriginalSum * ae.GBP;
					break;
			}

			if (aeAccounting.ID == 0)
			{
				var id = accountingLogic.CreateAccounting(aeAccounting);
				var serviceKinds = dataLogic.GetServiceKinds().Where(w => w.ProductId == order.ProductId).Select(s => s.ID).ToList();
				var serviceType = dataLogic.GetServiceTypes().FirstOrDefault(w => serviceKinds.Contains(w.ServiceKindId) && (w.Name == "Финансовые услуги"));
				if (serviceType == null)
					return;

				var serviceId = accountingLogic.CreateService(new Service
				{
					Count = 1,
					CurrencyId = currency.CurrencyId,
					AccountingId = id,
					Price = aeAccounting.OriginalSum,
					ServiceTypeId = serviceType.ID,
					VatId = 2   // без НДС
				});

				accountingLogic.CalculateServiceBalance(serviceId);
				accountingLogic.CalculateAccountingBalance(id);
				orderLogic.CalculateOrderBalance(aeAccounting.OrderId);

				accountingLogic.CreateAccountingMark(new AccountingMark
				{
					AccountingCheckedDate = marks.AccountingCheckedDate,
					AccountingCheckedUserId = marks.AccountingCheckedUserId,
					AccountingId = id,
					AccountingOkDate = marks.AccountingOkDate,
					AccountingOkUserId = marks.AccountingOkUserId,
					AccountingRejectedComment = marks.AccountingRejectedComment,
					AccountingRejectedDate = marks.AccountingRejectedDate,
					AccountingRejectedUserId = marks.AccountingRejectedUserId,
					ActCheckedDate = marks.ActCheckedDate,
					ActCheckedUserId = marks.ActCheckedUserId,
					ActOkDate = marks.ActOkDate,
					ActOkUserId = marks.ActOkUserId,
					ActRejectedComment = marks.ActRejectedComment,
					ActRejectedDate = marks.ActRejectedDate,
					ActRejectedUserId = marks.ActRejectedUserId,
					InvoiceCheckedDate = marks.InvoiceCheckedDate,
					InvoiceCheckedUserId = marks.InvoiceCheckedUserId,
					InvoiceOkDate = marks.InvoiceOkDate,
					InvoiceOkUserId = marks.InvoiceOkUserId,
					InvoiceRejectedComment = marks.InvoiceRejectedComment,
					InvoiceRejectedDate = marks.InvoiceRejectedDate,
					InvoiceRejectedUserId = marks.InvoiceRejectedUserId,
					IsAccountingChecked = marks.IsAccountingChecked,
					IsAccountingOk = marks.IsAccountingOk,
					IsAccountingRejected = marks.IsAccountingRejected,
					IsActChecked = marks.IsActChecked,
					IsActOk = marks.IsActOk,
					IsActRejected = marks.IsActRejected,
					IsInvoiceChecked = marks.IsInvoiceChecked,
					IsInvoiceOk = marks.IsInvoiceOk,
					IsInvoiceRejected = marks.IsInvoiceRejected
				});
			}
			else
			{
				accountingLogic.UpdateAccounting(aeAccounting);

				var aeService = accountingLogic.GetServicesByAccounting(aeAccounting.ID).First();
				aeService.Price = aeAccounting.OriginalSum;

				accountingLogic.CalculateServiceBalance(aeService.ID);
				accountingLogic.CalculateAccountingBalance(aeAccounting.ID);
				orderLogic.CalculateOrderBalance(aeAccounting.OrderId);

				var aeMarks = accountingLogic.GetAccountingMarkByAccounting(aeAccounting.ID);

				aeMarks.AccountingCheckedDate = marks.AccountingCheckedDate;
				aeMarks.AccountingCheckedUserId = marks.AccountingCheckedUserId;
				aeMarks.AccountingId = accounting.ID;
				aeMarks.AccountingOkDate = marks.AccountingOkDate;
				aeMarks.AccountingOkUserId = marks.AccountingOkUserId;
				aeMarks.AccountingRejectedComment = marks.AccountingRejectedComment;
				aeMarks.AccountingRejectedDate = marks.AccountingRejectedDate;
				aeMarks.AccountingRejectedUserId = marks.AccountingRejectedUserId;
				aeMarks.ActCheckedDate = marks.ActCheckedDate;
				aeMarks.ActCheckedUserId = marks.ActCheckedUserId;
				aeMarks.ActOkDate = marks.ActOkDate;
				aeMarks.ActOkUserId = marks.ActOkUserId;
				aeMarks.ActRejectedComment = marks.ActRejectedComment;
				aeMarks.ActRejectedDate = marks.ActRejectedDate;
				aeMarks.ActRejectedUserId = marks.ActRejectedUserId;
				aeMarks.InvoiceCheckedDate = marks.InvoiceCheckedDate;
				aeMarks.InvoiceCheckedUserId = marks.InvoiceCheckedUserId;
				aeMarks.InvoiceOkDate = marks.InvoiceOkDate;
				aeMarks.InvoiceOkUserId = marks.InvoiceOkUserId;
				aeMarks.InvoiceRejectedComment = marks.InvoiceRejectedComment;
				aeMarks.InvoiceRejectedDate = marks.InvoiceRejectedDate;
				aeMarks.InvoiceRejectedUserId = marks.InvoiceRejectedUserId;
				aeMarks.IsAccountingChecked = marks.IsAccountingChecked;
				aeMarks.IsAccountingOk = marks.IsAccountingOk;
				aeMarks.IsAccountingRejected = marks.IsAccountingRejected;
				aeMarks.IsActChecked = marks.IsActChecked;
				aeMarks.IsActOk = marks.IsActOk;
				aeMarks.IsActRejected = marks.IsActRejected;
				aeMarks.IsInvoiceChecked = marks.IsInvoiceChecked;
				aeMarks.IsInvoiceOk = marks.IsInvoiceOk;
				aeMarks.IsInvoiceRejected = marks.IsInvoiceRejected;

				accountingLogic.UpdateAccountingMark(aeMarks);
			}
		}

		void CreateUpdateAgentPercentageAccounting(int accountingId)
		{
			var accounting = accountingLogic.GetAccounting(accountingId);
			// только для расхода
			if (accounting.IsIncome)
				return;

			var contract = contractLogic.GetContract(accounting.ContractId.Value);
			// только для 
			if (contract.AgentPercentage > 0)
			{
				var service = accountingLogic.GetServicesByAccounting(accountingId).FirstOrDefault();
				if (service == null)
					return;

				var marks = accountingLogic.GetAccountingMarkByAccounting(accountingId);
				var order = orderLogic.GetOrder(accounting.OrderId);
				var apAccounting = accountingLogic.GetApAccounting(accounting.Number);

				if (apAccounting == null)
				{
					apAccounting = new Accounting
					{
						OrderId = accounting.OrderId,
						CreatedDate = DateTime.Now,
						IsIncome = accounting.IsIncome,
						AccountingDocumentTypeId = accounting.IsIncome ? 1 : 2,
						AccountingPaymentMethodId = 2,
						AccountingPaymentTypeId = accounting.IsIncome ? 2 : 2,
						AccountingDate = accounting.AccountingDate,
						RequestDate = marks.AccountingCheckedDate,
						InvoiceDate = marks.AccountingCheckedDate,
						ActDate = marks.AccountingCheckedDate,
						InvoiceNumber = accounting.Number,
						ActNumber = accounting.Number,
						Comment = "ap " + accounting.Number,
						OurLegalId = accounting.OurLegalId,
						ContractId = contractLogic.GetContractsByLegal(AUTOEXPENSE_LEGAL_ID).Where(w => w.OurLegalId == contract.OurLegalId).Select(s => s.ID).FirstOrDefault()
					};

					#region new number

					var accountings = accountingLogic.GetAccountingsByOrder(accounting.OrderId);
					int dirNo = accountings.Where(w => !w.IsIncome).Max(s => s.SameDirectionNo);

					apAccounting.Number = order.FinRepCenterId + order.ID.ToString("D7") + "-" + (dirNo + 201);
					apAccounting.SameDirectionNo = ++dirNo;

					#endregion
				}

				apAccounting.OriginalSum = accounting.OriginalSum * contract.AgentPercentage;

				if (apAccounting.ID == 0)
				{
					var id = accountingLogic.CreateAccounting(apAccounting);
					var currencyId = service.CurrencyId ?? 1;
					var serviceKinds = dataLogic.GetServiceKinds().Where(w => w.ProductId == order.ProductId).Select(s => s.ID).ToList();
					var serviceType = dataLogic.GetServiceTypes().FirstOrDefault(w => serviceKinds.Contains(w.ServiceKindId) && (w.Name == "Финансовые услуги"));
					if (serviceType == null)
						return;

					var serviceId = accountingLogic.CreateService(new Service
					{
						Count = 1,
						CurrencyId = currencyId,
						AccountingId = id,
						Price = apAccounting.OriginalSum,
						ServiceTypeId = serviceType.ID,
						VatId = 2   // без НДС
					});

					accountingLogic.CalculateServiceBalance(serviceId);
					accountingLogic.CalculateAccountingBalance(id);
					orderLogic.CalculateOrderBalance(apAccounting.OrderId);

					accountingLogic.CreateAccountingMark(new AccountingMark
					{
						AccountingCheckedDate = marks.AccountingCheckedDate,
						AccountingCheckedUserId = marks.AccountingCheckedUserId,
						AccountingId = id,
						AccountingOkDate = marks.AccountingOkDate,
						AccountingOkUserId = marks.AccountingOkUserId,
						AccountingRejectedComment = marks.AccountingRejectedComment,
						AccountingRejectedDate = marks.AccountingRejectedDate,
						AccountingRejectedUserId = marks.AccountingRejectedUserId,
						ActCheckedDate = marks.ActCheckedDate,
						ActCheckedUserId = marks.ActCheckedUserId,
						ActOkDate = marks.ActOkDate,
						ActOkUserId = marks.ActOkUserId,
						ActRejectedComment = marks.ActRejectedComment,
						ActRejectedDate = marks.ActRejectedDate,
						ActRejectedUserId = marks.ActRejectedUserId,
						InvoiceCheckedDate = marks.InvoiceCheckedDate,
						InvoiceCheckedUserId = marks.InvoiceCheckedUserId,
						InvoiceOkDate = marks.InvoiceOkDate,
						InvoiceOkUserId = marks.InvoiceOkUserId,
						InvoiceRejectedComment = marks.InvoiceRejectedComment,
						InvoiceRejectedDate = marks.InvoiceRejectedDate,
						InvoiceRejectedUserId = marks.InvoiceRejectedUserId,
						IsAccountingChecked = marks.IsAccountingChecked,
						IsAccountingOk = marks.IsAccountingOk,
						IsAccountingRejected = marks.IsAccountingRejected,
						IsActChecked = marks.IsActChecked,
						IsActOk = marks.IsActOk,
						IsActRejected = marks.IsActRejected,
						IsInvoiceChecked = marks.IsInvoiceChecked,
						IsInvoiceOk = marks.IsInvoiceOk,
						IsInvoiceRejected = marks.IsInvoiceRejected
					});
				}
				else
				{
					accountingLogic.UpdateAccounting(apAccounting);

					var apService = accountingLogic.GetServicesByAccounting(apAccounting.ID).First();
					apService.Price = apAccounting.OriginalSum;

					accountingLogic.CalculateServiceBalance(apService.ID);
					accountingLogic.CalculateAccountingBalance(apAccounting.ID);
					orderLogic.CalculateOrderBalance(apAccounting.OrderId);

					var aeMarks = accountingLogic.GetAccountingMarkByAccounting(apAccounting.ID);

					aeMarks.AccountingCheckedDate = marks.AccountingCheckedDate;
					aeMarks.AccountingCheckedUserId = marks.AccountingCheckedUserId;
					aeMarks.AccountingId = accounting.ID;
					aeMarks.AccountingOkDate = marks.AccountingOkDate;
					aeMarks.AccountingOkUserId = marks.AccountingOkUserId;
					aeMarks.AccountingRejectedComment = marks.AccountingRejectedComment;
					aeMarks.AccountingRejectedDate = marks.AccountingRejectedDate;
					aeMarks.AccountingRejectedUserId = marks.AccountingRejectedUserId;
					aeMarks.ActCheckedDate = marks.ActCheckedDate;
					aeMarks.ActCheckedUserId = marks.ActCheckedUserId;
					aeMarks.ActOkDate = marks.ActOkDate;
					aeMarks.ActOkUserId = marks.ActOkUserId;
					aeMarks.ActRejectedComment = marks.ActRejectedComment;
					aeMarks.ActRejectedDate = marks.ActRejectedDate;
					aeMarks.ActRejectedUserId = marks.ActRejectedUserId;
					aeMarks.InvoiceCheckedDate = marks.InvoiceCheckedDate;
					aeMarks.InvoiceCheckedUserId = marks.InvoiceCheckedUserId;
					aeMarks.InvoiceOkDate = marks.InvoiceOkDate;
					aeMarks.InvoiceOkUserId = marks.InvoiceOkUserId;
					aeMarks.InvoiceRejectedComment = marks.InvoiceRejectedComment;
					aeMarks.InvoiceRejectedDate = marks.InvoiceRejectedDate;
					aeMarks.InvoiceRejectedUserId = marks.InvoiceRejectedUserId;
					aeMarks.IsAccountingChecked = marks.IsAccountingChecked;
					aeMarks.IsAccountingOk = marks.IsAccountingOk;
					aeMarks.IsAccountingRejected = marks.IsAccountingRejected;
					aeMarks.IsActChecked = marks.IsActChecked;
					aeMarks.IsActOk = marks.IsActOk;
					aeMarks.IsActRejected = marks.IsActRejected;
					aeMarks.IsInvoiceChecked = marks.IsInvoiceChecked;
					aeMarks.IsInvoiceOk = marks.IsInvoiceOk;
					aeMarks.IsInvoiceRejected = marks.IsInvoiceRejected;

					accountingLogic.UpdateAccountingMark(aeMarks);
				}
			}
		}

		void UpdateAccountingNumbers(DocumentEditModel document)
		{
			if (!document.AccountingId.HasValue)
				return;

			var accounting = accountingLogic.GetAccounting(document.AccountingId.Value);

			if (!accounting.IsIncome && (document.DocumentTypeId == 59))// счет от поставщика
			{
				accounting.InvoiceNumber = document.Number;
				accounting.InvoiceDate = document.Date;
				accountingLogic.UpdateAccounting(accounting);
				accountingLogic.CalculatePaymentPlanDate(accounting.ID);
			}
			else if (document.DocumentTypeId == 10)// акт от поставщика
			{
				accounting.ActNumber = document.Number;
				accounting.ActDate = document.Date;
				accountingLogic.UpdateAccounting(accounting);
				accountingLogic.CalculatePaymentPlanDate(accounting.ID);
			}
			else if (!accounting.IsIncome && (document.DocumentTypeId == 61))// Счет-фактура от поставщика
			{
				accounting.VatInvoiceNumber = document.Number;
				accountingLogic.UpdateAccounting(accounting);
			}

			if (!accounting.IsIncome)
			{
				var contract = contractLogic.GetContract(accounting.ContractId.Value);
				var legal = legalLogic.GetLegal(contract.LegalId);
				if ((legal.ContractorId == 48) && (document.DocumentTypeId == 20))  // Таможня, ДТ
				{
					accounting.InvoiceNumber = document.Number;
					accounting.InvoiceDate = document.Date;
					accounting.ActNumber = document.Number;
					accounting.ActDate = document.Date;
					accountingLogic.UpdateAccounting(accounting);
					accountingLogic.CalculatePaymentPlanDate(accounting.ID);
				}
			}
		}

		DateTime GetTemplatedDocumentDate(int id)
		{
			var document = documentLogic.GetTemplatedDocument(id);
			switch (document.TemplateId)
			{
				case 4:
				case 5:
				case 15:
				case 16:
				case 19:
				case 23:
					var accounting = accountingLogic.GetAccounting(document.AccountingId.Value);
					return accounting.InvoiceDate ?? DateTime.MinValue;

				case 6:
				case 8:
				case 17:
				case 18:
				case 20:
				case 22:
					var accounting2 = accountingLogic.GetAccounting(document.AccountingId.Value);
					return accounting2.ActDate ?? DateTime.MinValue;

				// TODO:

				default:
					return DateTime.MinValue;
			}
		}

		Pricelist GetPricelist(Order order)
		{
			var contractId = order.ContractId.Value;
			var finRepCenterId = order.FinRepCenterId.Value;
			var productId = order.ProductId;

			var pricelists = pricelistLogic.GetValidPricelists().Where(w => (w.FinRepCenterId == finRepCenterId) && (w.ProductId == productId));

			var pricelist = pricelists.FirstOrDefault(w => w.ContractId == contractId);
			if (pricelist == null)
				pricelist = pricelists.FirstOrDefault(w => !w.ContractId.HasValue);

			return pricelist;
		}

		void _ChangeOrderTemplate(int orderId, int orderTemplateId)
		{
			var order = orderLogic.GetOrder(orderId);
			order.OrderTemplateId = orderTemplateId;
			orderLogic.UpdateOrder(order);

			var newOps = dataLogic.GetOrderOperationsByTemplate(orderTemplateId).ToList();
			var ops = orderLogic.GetOperationsByOrder(orderId).ToList();
			foreach (var operation in ops)
				if (newOps.FirstOrDefault(w => w.ID == operation.OrderOperationId) == null)
					orderLogic.DeleteOperation(operation.ID);

			int index = 0;
			foreach (var orderOperation in newOps)
				if (ops.FirstOrDefault(w => w.OrderOperationId == orderOperation.ID) == null)
				{
					var amId = GetParticipantUsers(participantLogic.GetWorkgroupByOrder(orderId).Where(w => w.IsResponsible), 3).FirstOrDefault();
					if (amId == 0)
						amId = GetParticipantUsers(participantLogic.GetWorkgroupByOrder(orderId), 3).FirstOrDefault();

					orderLogic.CreateOperation(new Operation { OrderOperationId = orderOperation.ID, Name = orderOperation.Name, OrderId = orderId, No = index++, ResponsibleUserId = amId });
				}
				else
				{
					var op = ops.First(w => w.OrderOperationId == orderOperation.ID);
					op.No = index++;
					orderLogic.UpdateOperation(op);
				}
		}
	}
}