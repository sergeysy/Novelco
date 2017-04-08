using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using Logisto.BusinessLogic;
using Logisto.Data;
using Logisto.Models;
using Logisto.ViewModels;
using Newtonsoft.Json;
using OfficeOpenXml;
using LinqToDB;
using System.Globalization;

namespace Logisto.Controllers
{
	[Authorize]
	public class DataController : BaseController
	{
		const int SEARCH_LIMIT = 10;

		#region Pages

		public ActionResult Index()
		{
			var viewModel = dataLogic.GetDataCounters();
			return View(viewModel);
		}

		public ActionResult Templates()
		{
			if (!IsSuperUser())
				return RedirectToAction("NotAuthorized", "Home");

			var viewModel = new TemplatesViewModel();
			viewModel.Items = dataLogic.GetTemplates().Select(s => new Template
			{
				ID = s.ID,
				Filename = s.Filename,
				FileSize = s.FileSize,
				Name = s.Name,
				//SqlDataSource = s.SqlDataSource,
				xlfColumns = s.xlfColumns,
				Suffix = s.Suffix,
				ListFirstColumn = s.ListFirstColumn,
				ListLastColumn = s.ListLastColumn,
				ListRow = s.ListRow
			}).ToList();
			return View(viewModel);
		}

		public ActionResult CargoDescriptions()
		{
			if (!IsSuperUser())
				return RedirectToAction("NotAuthorized", "Home");

			var viewModel = dataLogic.GetCargoDescriptions();
			return View(viewModel);
		}

		public ActionResult PackageTypes()
		{
			if (!IsSuperUser())
				return RedirectToAction("NotAuthorized", "Home");

			var viewModel = dataLogic.GetPackageTypes();
			return View(viewModel);
		}

		public ActionResult OurLegals()
		{
			if (!IsSuperUser())
				return RedirectToAction("NotAuthorized", "Home");

			var viewModel = legalLogic.GetOurLegals();
			ViewBag.Dictionaries = new Dictionary<string, object>();
			ViewBag.Dictionaries.Add("Legal", dataLogic.GetLegals());
			ViewBag.Dictionaries.Add("Employee", dataLogic.GetEmployees());
			ViewBag.Dictionaries.Add("TaxType", dataLogic.GetTaxTypes());
			ViewBag.Dictionaries.Add("User", dataLogic.GetUsers());
			ViewBag.Dictionaries.Add("Currency", dataLogic.GetCurrencies());

			return View(viewModel);
		}

		public ActionResult ParticipantPermissions()
		{
			var viewModel = new ParticipantPermissionsViewModel();
			viewModel.Actions = participantLogic.GetAllActions().OrderBy(o => o.Name);
			viewModel.Roles = dataLogic.GetParticipantRoles();
			return View(viewModel);
		}

		public ActionResult CreateParticipantRole()
		{
			var viewModel = new ParticipantRole();
			return View(viewModel);
		}

		public ActionResult CreateOurLegal()
		{
			var legal = new Legal
			{
				CreatedBy = CurrentUserId,
				CreatedDate = DateTime.Now,
				DisplayName = "Новое юрлицо",
				Name = "Новое юрлицо"
			};

			var id = legalLogic.CreateLegal(legal);

			var ourLegal = new OurLegal
			{
				LegalId = id,
				Name = "Новое"
			};

			legalLogic.CreateOurLegal(ourLegal);

			return RedirectToAction("OurLegals");
		}

		#endregion

		#region Search

		public ActionResult Search(string number)
		{
			ViewBag.Dictionaries = new Dictionary<string, object>();
			ViewBag.Dictionaries.Add("OrderStatus", dataLogic.GetOrderStatuses());
			return View((object)number);
		}

		public ContentResult SearchContractorsGetItems(string number)
		{
			int id = 0;
			var list = contractorLogic.GetContractors(new ListFilter { Context = number }).Take(SEARCH_LIMIT).ToList();
			if (int.TryParse(number, out id))
				list.AddIfNotNull(contractorLogic.GetContractor(id));

			return Content(JsonConvert.SerializeObject(new { Items = list }));
		}

		public ContentResult SearchLegalsGetItems(string number)
		{
			int id = 0;
			var list = legalLogic.GetLegals(new ListFilter { Context = number }).Take(SEARCH_LIMIT).ToList();
			if (int.TryParse(number, out id))
				list.AddIfNotNull(legalLogic.GetLegal(id));

			return Content(JsonConvert.SerializeObject(new { Items = list }));
		}

		public ContentResult SearchOrdersGetItems(string number)
		{
			int id = 0;
			var list = orderLogic.GetOrders(new ListFilter { Context = number, UserId = CurrentUserId }).Take(SEARCH_LIMIT).ToList();
			if (int.TryParse(number, out id))
				list.AddIfNotNull(orderLogic.GetOrder(id));

			return Content(JsonConvert.SerializeObject(new { Items = list }));
		}

		public ContentResult SearchRequestsGetItems(string number)
		{
			int id = 0;
			var list = requestLogic.GetRequests(new ListFilter { Context = number }).Take(SEARCH_LIMIT).ToList();
			if (int.TryParse(number, out id))
				list.AddIfNotNull(requestLogic.GetRequest(id));

			return Content(JsonConvert.SerializeObject(new { Items = list }));
		}

		public ContentResult SearchDocumentsGetItems(string number)
		{
			int id = 0;
			var documents = documentLogic.GetDocuments(new ListFilter { Context = number }).Take(SEARCH_LIMIT).ToList();
			if (int.TryParse(number, out id))
				documents.AddIfNotNull(documentLogic.GetDocument(id));

			var templatedDocuments = documentLogic.GetTemplatedDocuments(new ListFilter { Context = number }).Take(SEARCH_LIMIT).ToList();
			if (int.TryParse(number, out id))
				templatedDocuments.AddIfNotNull(documentLogic.GetTemplatedDocument(id));

			var list = documents.Select(s => new JointDocumentModel
			{
				ID = s.ID,
				IsDocument = true,
				Number = s.Number,
				Filename = s.Filename,
				Date = s.Date,
				UploadedDate = s.UploadedDate,
				IsPrint = s.IsPrint,
				IsNipVisible = s.IsNipVisible,
				OrderId = s.OrderId,
				AccountingId = s.AccountingId
			}).Union(
				templatedDocuments.Select(ts => new JointDocumentModel
				{
					ID = ts.ID,
					IsDocument = false,
					Filename = ts.Filename,
					Date = ts.CreatedDate,
					UploadedDate = ts.ChangedDate,
					IsPrint = true,
					IsNipVisible = true,
					OrderId = ts.OrderId,
					AccountingId = ts.AccountingId
				})
				).ToList();

			var orders = orderLogic.GetAllOrders();
			foreach (var item in list)
			{
				if (item.OrderId.HasValue)
					item.OrderNumber = orders.Where(w => w.ID == item.OrderId).Select(s => s.Number).FirstOrDefault();

				if (item.AccountingId.HasValue)
					item.OrderNumber = orders.Where(w => w.ID == (accountingLogic.GetAccounting(item.AccountingId.Value).OrderId)).Select(s => s.Number).FirstOrDefault();
			}

			return Content(JsonConvert.SerializeObject(new { Items = list }));
		}

		public ContentResult SearchContractsGetItems(string number)
		{
			int id = 0;
			var list = contractLogic.GetContracts(new ListFilter { Context = number }).Take(SEARCH_LIMIT).ToList();
			if (int.TryParse(number, out id))
				list.AddIfNotNull(contractLogic.GetContract(id));

			return Content(JsonConvert.SerializeObject(new { Items = list }));
		}

		public ContentResult SearchPaymentsGetItems(string number)
		{
			int id = 0;
			var list = accountingLogic.GetPayments(new ListFilter { Context = number }).Take(SEARCH_LIMIT).ToList();
			if (int.TryParse(number, out id))
				list.AddIfNotNull(accountingLogic.GetPayment(id));

			return Content(JsonConvert.SerializeObject(new { Items = list }));
		}

		#endregion

		public ContentResult IsAllowed(int actionId, int participantRoleId)
		{
			var permissions = participantLogic.GetAllowedRoles(actionId);

			return Content(permissions.Any(w => w.ID == participantRoleId).ToString());
		}

		public ContentResult SetParticipantPermission(int actionId, int participantRoleId, bool allow)
		{
			participantLogic.AllowRole(actionId, participantRoleId, allow);
			return Content(JsonConvert.SerializeObject(""));
		}

		public FileContentResult OpenTemplate(int id)
		{
			var template = dataLogic.GetTemplate(id);
			var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
			return File(template.Data, contentType, template.Filename + ".xlsx");
		}

		[HttpPost]
		public ContentResult UploadTemplate(int id)
		{
			if (id <= 0 || Request.Files.Count == 0)
				return Content(JsonConvert.SerializeObject(new { Message = "Неверные входные данные" }));

			var file = Request.Files[0];

			var template = dataLogic.GetTemplate(id);
			template.Data = new byte[file.InputStream.Length];
			file.InputStream.Read(template.Data, 0, (int)file.InputStream.Length);

			dataLogic.UpdateTemplate(template);

			return Content(JsonConvert.SerializeObject(new { Message = "" }));
		}

		public ContentResult SaveTemplate(Template model)
		{
			// обновить позицию
			var template = dataLogic.GetTemplate(model.ID);
			template.Filename = model.Filename;
			template.FileSize = model.FileSize;
			template.Name = model.Name;
			//template.SqlDataSource = model.SqlDataSource;
			template.xlfColumns = model.xlfColumns;
			template.Suffix = model.Suffix;
			template.ListFirstColumn = model.ListFirstColumn;
			template.ListLastColumn = model.ListLastColumn;
			template.ListRow = model.ListRow;

			dataLogic.UpdateTemplate(template);

			return Content(JsonConvert.SerializeObject(""));
		}

		public ContentResult SaveParticipantRole(ParticipantRole model)
		{
			var id = dataLogic.CreateParticipantRole(model);
			return Content(JsonConvert.SerializeObject(new { ID = id }));
		}

		public ContentResult SaveTemplateField(TemplateField model)
		{
			// обновить позицию
			var field = dataLogic.GetTemplateField(model.ID);
			field.Name = model.Name;
			field.FieldName = model.FieldName;
			field.IsAtable = model.IsAtable;
			field.Range = model.Range;

			dataLogic.UpdateTemplateField(field);

			return Content(JsonConvert.SerializeObject(""));
		}

		public ContentResult SaveCargoDescription(CargoDescription model)
		{
			if (model.ID == 0)
			{
				var id = dataLogic.CreateCargoDescription(model);
				return Content(JsonConvert.SerializeObject(new { ID = id }));
			}
			else
			{
				// обновить позицию
				var cargoDescription = dataLogic.GetCargoDescription(model.ID);
				cargoDescription.Display = model.Display;
				cargoDescription.EnDisplay = model.EnDisplay;

				dataLogic.UpdateCargoDescription(cargoDescription);
				return Content(JsonConvert.SerializeObject(""));
			}
		}

		public ContentResult SavePackageType(PackageType model)
		{
			// обновить позицию
			var type = dataLogic.GetPackageType(model.ID);
			type.Display = model.Display;
			type.EnDisplay = model.EnDisplay;

			dataLogic.UpdatePackageType(type);

			return Content(JsonConvert.SerializeObject(""));
		}

		public ContentResult GetTemplateFields(int templateId)
		{
			var list = dataLogic.GetTemplateFields().Where(w => w.TemplateId == templateId);
			return Content(JsonConvert.SerializeObject(new { Items = list }));
		}

		[HttpPost]
		public ContentResult UploadOurLegalSign(int id)
		{
			if (id <= 0 || Request.Files.Count == 0)
				return Content(JsonConvert.SerializeObject(new { Message = "Неверные входные данные" }));

			var file = Request.Files[0];

			// проверить - только jpg
			//if ((!file.FileName.ToUpperInvariant().EndsWith(".JPG")) && (!file.FileName.ToUpperInvariant().EndsWith(".JPEG")))
			//	return Content(JsonConvert.SerializeObject(new { Message = "Неверные входные данные (разрешены только файлы формата JPEG)" }));

			var ol = legalLogic.GetOurLegal(id);
			ol.Sign = new byte[file.InputStream.Length];
			file.InputStream.Read(ol.Sign, 0, (int)file.InputStream.Length);

			legalLogic.UpdateOurLegal(ol);

			return Content(JsonConvert.SerializeObject(new { Message = "" }));
		}

		[OutputCache(Duration = 0, NoStore = true)]
		public FileResult DownloadOurLegalSign(int id)
		{
			var ol = legalLogic.GetOurLegal(id);
			var contentType = "application/octet-stream";
			return File(ol.Sign ?? new byte[] { }, contentType, ol.ID + ".jpg");
		}

		public ContentResult SaveOurLegal(OurLegal model)
		{
			var legal = legalLogic.GetOurLegal(model.ID);

			legal.Name = model.Name;
			legal.LegalId = model.LegalId ?? legal.LegalId;

			legalLogic.UpdateOurLegal(legal);

			return Content(JsonConvert.SerializeObject(""));
		}

		public ActionResult LegalDetails(int id)
		{
			var contractorId = legalLogic.GetLegal(id).ContractorId;
			return RedirectToAction("Details", "Contractors", new { id = contractorId, activePart = "Legals" });
		}

		public ContentResult SaveLegal(LegalEditModel model)
		{
			var legalLogic = new LegalLogic();

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
			legal.WorkTime = model.WorkTime;
			legal.IsNotResident = model.IsNotResident;

			legal.UpdatedBy = CurrentUserId;
			legal.UpdatedDate = DateTime.Now;

			legalLogic.UpdateLegal(legal);

			return Content(JsonConvert.SerializeObject(""));
		}

		public ContentResult GetCurrentCurrencyRate()
		{
			var rates = dataLogic.GetCurrencyRates(DateTime.Today);
			return Content(JsonConvert.SerializeObject(new { Rates = rates }));
		}

		#region Volumetric ratio

		public ActionResult VolumetricRatios()
		{
			if (!IsSuperUser())
				return RedirectToAction("NotAuthorized", "Home");

			var viewModel = dataLogic.GetVolumetricRatios();
			return View(viewModel);
		}

		public ContentResult GetNewVolumetricRatio()
		{
			// подстановка значений по-умолчанию
			var c = new VolumetricRatio();
			return Content(JsonConvert.SerializeObject(c));
		}

		public ContentResult SaveVolumetricRatio(VolumetricRatio model)
		{
			if (model.ID == 0)
			{
				var id = dataLogic.CreateVolumetricRatio(model);
				return Content(JsonConvert.SerializeObject(new { ID = id }));
			}
			else
			{
				// обновить позицию
				var term = dataLogic.GetVolumetricRatio(model.ID);
				term.Display = model.Display;
				term.Value = model.Value;

				dataLogic.UpdateVolumetricRatio(term);

				return Content(JsonConvert.SerializeObject(""));
			}
		}

		public ContentResult DeleteVolumetricRatio(int id)
		{
			dataLogic.DeleteVolumetricRatio(id);
			return Content(JsonConvert.SerializeObject(""));
		}

		#endregion

		#region цфо

		public ActionResult FinRepCenters()
		{
			if (!IsSuperUser())
				return RedirectToAction("NotAuthorized", "Home");

			var viewModel = dataLogic.GetFinRepCenters();
			ViewBag.Dictionaries = new Dictionary<string, object>();
			ViewBag.Dictionaries.Add("OurLegal", dataLogic.GetOurLegals());
			return View(viewModel);
		}

		public ContentResult GetNewFinRepCenter()
		{
			// подстановка значений по-умолчанию
			var c = new FinRepCenter();
			return Content(JsonConvert.SerializeObject(c));
		}

		public ContentResult SaveFinRepCenter(FinRepCenter model)
		{
			if (model.ID == 0)
			{
				var id = dataLogic.CreateFinRepCenter(model);
				return Content(JsonConvert.SerializeObject(new { ID = id }));
			}
			else
			{
				// обновить позицию
				var term = dataLogic.GetFinRepCenter(model.ID);
				term.Name = model.Name;
				term.Code = model.Code;
				term.Description = model.Description;
				term.OurLegalId = model.OurLegalId;

				dataLogic.UpdateFinRepCenter(term);

				return Content(JsonConvert.SerializeObject(""));
			}
		}

		#endregion

		#region document types

		public ActionResult DocumentTypes()
		{
			var viewModel = dataLogic.GetDocumentTypes();
			return View(viewModel);
		}

		public ContentResult GetNewDocumentType()
		{
			// подстановка значений по-умолчанию
			var c = new DocumentType();
			return Content(JsonConvert.SerializeObject(c));
		}

		public ContentResult SaveDocumentType(DocumentTypeEditModel model)
		{
			if (model.ID == 0)
			{
				var id = dataLogic.CreateDocumentType(model);

				// TODO: prints
				return Content(JsonConvert.SerializeObject(new { ID = id }));
			}
			else
			{
				// обновить позицию
				var t = dataLogic.GetDocumentType(model.ID);
				t.Display = model.Display;
				t.Description = model.Description;
				t.EnDescription = model.EnDescription;
				t.IsNipVisible = model.IsNipVisible;

				dataLogic.UpdateDocumentType(t);

				var list = dataLogic.GetDocumentTypeProductPrints(model.ID);

				// сначала удалить неактуальные
				foreach (var item in list)
					if (!model.Prints.Any(w => w == item.ProductId))
						dataLogic.DeleteDocumentTypeProductPrint(item.ID);

				// добавить новое 
				foreach (var item in model.Prints)
					if (!list.Any(w => w.ProductId == item))
					{
						var prnt = new DocumentTypeProductPrint();
						prnt.DocumentTypeId = model.ID;
						prnt.ProductId = item;
						dataLogic.CreateDocumentTypeProductPrint(prnt);
					}

				return Content(JsonConvert.SerializeObject(""));
			}
		}

		public ContentResult GetDocumentTypeProductPrints(int documentTypeId)
		{
			var products = dataLogic.GetProducts();
			var prints = dataLogic.GetDocumentTypeProductPrints(documentTypeId).Select(s => s.ProductId).ToList();
			var list = products.Select(s => new
			{
				s.ID,
				s.Display,
				Checked = prints.Contains(s.ID)
			});
			return Content(JsonConvert.SerializeObject(new { Items = list }));
		}

		#endregion

		#region payment terms

		public ActionResult PaymentTerms()
		{
			if (!IsSuperUser())
				return RedirectToAction("NotAuthorized", "Home");

			var viewModel = dataLogic.GetPaymentTerms();
			return View(viewModel);
		}

		public ContentResult GetNewPaymentTerm()
		{
			// подстановка значений по-умолчанию
			var c = new PaymentTerm();
			return Content(JsonConvert.SerializeObject(c));
		}

		public ContentResult SavePaymentTerm(PaymentTerm model)
		{
			if (model.ID == 0)
			{
				var id = dataLogic.CreatePaymentTerm(model);
				return Content(JsonConvert.SerializeObject(new { ID = id }));
			}
			else
			{
				// обновить позицию
				var term = dataLogic.GetPaymentTerm(model.ID);
				term.Display = model.Display;

				dataLogic.UpdatePaymentTerm(term);

				return Content(JsonConvert.SerializeObject(""));
			}
		}

		#endregion

		#region Orders Rentability

		public ActionResult OrdersRentability()
		{
			if (!IsSuperUser())
				return RedirectToAction("NotAuthorized", "Home");

			var viewModel = new OrdersRentabilityViewModel { Items = dataLogic.GetOrdersRentability().ToList() };

			viewModel.Dictionaries.Add("OrderTemplate", dataLogic.GetOrderTemplates());
			viewModel.Dictionaries.Add("FinRepCenter", dataLogic.GetFinRepCenters());
			viewModel.Dictionaries.Add("Product", dataLogic.GetProducts());

			return View(viewModel);
		}

		public ContentResult GetNewOrderRentability()
		{
			// подстановка значений по-умолчанию
			var c = new OrderRentability { Year = DateTime.Today.Year };
			return Content(JsonConvert.SerializeObject(c));
		}

		public ContentResult SaveOrderRentability(OrderRentability model)
		{
			if (model.ID == 0)
			{
				var id = dataLogic.CreateOrderRentability(model);
				return Content(JsonConvert.SerializeObject(new { ID = id }));
			}
			else
			{
				// обновить позицию
				var rentability = dataLogic.GetOrderRentability(model.ID);
				rentability.OrderTemplateId = model.OrderTemplateId;
				rentability.FinRepCenterId = model.FinRepCenterId;
				rentability.Rentability = model.Rentability;
				rentability.ProductId = model.ProductId;
				rentability.Year = model.Year;

				dataLogic.UpdateOrderRentability(rentability);

				return Content(JsonConvert.SerializeObject(""));
			}
		}

		#endregion

		#region CurrencyRateUses

		public ActionResult CurrencyRateUses()
		{
			if (!IsSuperUser())
				return RedirectToAction("NotAuthorized", "Home");

			var viewModel = dataLogic.GetCurrencyRateUses();
			return View(viewModel);
		}

		public ContentResult GetNewCurrencyRateUse()
		{
			// подстановка значений по-умолчанию
			var c = new CurrencyRateUse();
			return Content(JsonConvert.SerializeObject(c));
		}

		public ContentResult SaveCurrencyRateUse(CurrencyRateUse model)
		{
			if (model.ID == 0)
			{
				var id = dataLogic.CreateCurrencyRateUse(model);
				return Content(JsonConvert.SerializeObject(new { ID = id }));
			}
			else
			{
				// обновить позицию
				var cru = dataLogic.GetCurrencyRateUse(model.ID);
				cru.Display = model.Display;
				cru.Value = model.Value;
				cru.IsDocumentDate = model.IsDocumentDate;

				dataLogic.UpdateCurrencyRateUse(cru);

				return Content(JsonConvert.SerializeObject(""));
			}
		}

		#endregion

		#region CurrencyRateDiff

		public ActionResult CurrencyRateDiff()
		{
			if (!IsSuperUser())
				return RedirectToAction("NotAuthorized", "Home");

			var viewModel = dataLogic.GetCurrencyRateDiffs();
			return View(viewModel);
		}

		public ContentResult GetNewCurrencyRateDiff()
		{
			// подстановка значений по-умолчанию
			var c = new CurrencyRateDiff { From = DateTime.Today, To = DateTime.Today.AddMonths(1) };
			return Content(JsonConvert.SerializeObject(c));
		}

		public ContentResult SaveCurrencyRateDiff(CurrencyRateDiff model)
		{
			if (model.ID == 0)
			{
				var id = dataLogic.CreateCurrencyRateDiff(model);
				return Content(JsonConvert.SerializeObject(new { ID = id }));
			}
			else
			{
				if (!IsSuperUser())
					return Content(JsonConvert.SerializeObject(new { Message = "У вас недостаточно прав" }));

				// обновить позицию
				var cru = dataLogic.GetCurrencyRateDiff(model.ID);
				cru.From = model.From;
				cru.To = model.To;
				cru.CNY = model.CNY;
				cru.EUR = model.EUR;
				cru.GBP = model.GBP;
				cru.USD = model.USD;

				dataLogic.UpdateCurrencyRateDiff(cru);

				return Content(JsonConvert.SerializeObject(""));
			}
		}

		#endregion

		#region contract roles

		public ActionResult ContractRoles()
		{
			if (!IsSuperUser())
				return RedirectToAction("NotAuthorized", "Home");

			var viewModel = dataLogic.GetContractRoles();
			return View(viewModel);
		}

		public ContentResult GetNewContractRole()
		{
			// подстановка значений по-умолчанию
			var c = new ContractRole();
			return Content(JsonConvert.SerializeObject(c));
		}

		public ContentResult SaveContractRole(ContractRole model)
		{
			if (model.ID == 0)
			{
				// создать
				var role = new ContractRole();
				role.Display = model.Display;
				role.AblativeName = model.AblativeName;
				role.DativeName = model.DativeName;
				role.EnName = model.EnName;

				var id = dataLogic.CreateContractRole(role);
				return Content(JsonConvert.SerializeObject(new { ID = id }));
			}
			else
			{
				// обновить позицию
				var role = dataLogic.GetContractRole(model.ID);
				role.Display = model.Display;
				role.AblativeName = model.AblativeName;
				role.DativeName = model.DativeName;
				role.EnName = model.EnName;

				dataLogic.UpdateContractRole(role);
				return Content(JsonConvert.SerializeObject(""));
			}
		}

		public ContentResult DeleteContractRole(int id)
		{
			dataLogic.DeleteContractRole(id);
			return Content(JsonConvert.SerializeObject(""));
		}

		#endregion

		#region contract types

		public ActionResult ContractTypes()
		{
			if (!IsSuperUser())
				return RedirectToAction("NotAuthorized", "Home");

			var viewModel = dataLogic.GetContractTypes();
			ViewBag.Dictionaries = new Dictionary<string, object>();
			ViewBag.Dictionaries.Add("ContractRole", dataLogic.GetContractRoles());
			return View(viewModel);
		}

		public ContentResult GetNewContractType()
		{
			// подстановка значений по-умолчанию
			var c = new ContractType { };
			return Content(JsonConvert.SerializeObject(c));
		}

		public ContentResult SaveContractType(ContractType model)
		{
			if (model.ID == 0)
			{
				// создать
				var type = new ContractType();
				type.Display = model.Display;
				type.ContractRoleId = model.ContractRoleId;
				type.OurContractRoleId = model.OurContractRoleId;

				var id = dataLogic.CreateContractType(type);
				return Content(JsonConvert.SerializeObject(new { ID = id }));
			}
			else
			{
				// обновить позицию
				var type = dataLogic.GetContractType(model.ID);
				type.Display = model.Display;
				type.ContractRoleId = model.ContractRoleId;
				type.OurContractRoleId = model.OurContractRoleId;

				dataLogic.UpdateContractType(type);
			}

			return Content(string.Empty);
		}

		#endregion

		#region Банки

		public ActionResult Banks()
		{
			if (!IsSuperUser())
				return RedirectToAction("NotAuthorized", "Home");

			var viewModel = new ListFilter { PageSize = 50 };
			return View(viewModel);
		}

		public ContentResult BankGetItems(ListFilter filter)
		{
			var totalCount = bankLogic.GetBanksCount(filter);
			var list = bankLogic.GetBanks(filter);
			return Content(JsonConvert.SerializeObject(new { Items = list, TotalCount = totalCount }));
		}

		public ContentResult SaveBank(Bank model)
		{
			var bank = bankLogic.GetBank(model.ID);
			bank.BIC = model.BIC;
			bank.KSNP = model.KSNP;
			bank.Name = model.Name;
			bank.NNP = model.NNP;
			bank.PZN = model.PZN;
			bank.SWIFT = model.SWIFT;
			bank.TNP = model.TNP;
			bank.UER = model.UER;

			bankLogic.UpdateBank(bank);
			return Content(JsonConvert.SerializeObject(""));
		}

		#endregion

		#region Курсы

		public ActionResult CurrencyRates()
		{
			if (!IsSuperUser())
				return RedirectToAction("NotAuthorized", "Home");

			var viewModel = new ListFilter { PageSize = 50, From = DateTime.Today.AddMonths(-1), To = DateTime.Today };
			return View(viewModel);
		}

		public ContentResult CurrencyRatesGetItems(ListFilter filter)
		{
			var list = dataLogic.GetCurrenciesRates(filter);
			return Content(JsonConvert.SerializeObject(new { Items = list.OrderByDescending(o => o.Key).Select(s => new { Date = s.Key.ToString("yyyy-MM-dd"), Rates = s.Value }), TotalCount = list.Count }));
		}

		#endregion

		#region продукты и услуги

		public ActionResult Services()
		{
			if (!IsSuperUser())
				return RedirectToAction("NotAuthorized", "Home");

			var viewModel = dataLogic.GetProducts();
			ViewBag.Dictionaries = new Dictionary<string, object>();
			ViewBag.Dictionaries.Add("VolumetricRatio", dataLogic.GetVolumetricRatios());
			ViewBag.Dictionaries.Add("Measure", dataLogic.GetMeasures());
			ViewBag.Dictionaries.Add("ActiveUser", dataLogic.GetActiveUsers());
			ViewBag.Dictionaries.Add("User", dataLogic.GetUsers());
			ViewBag.Dictionaries.Add("Vat", dataLogic.GetVats());
			return View(viewModel);
		}

		public ContentResult GetProducts()
		{
			var list = dataLogic.GetProducts();
			return Content(JsonConvert.SerializeObject(new { Items = list }));
		}

		public ContentResult GetServiceKinds(int productId)
		{
			var list = dataLogic.GetServiceKinds().Where(w => w.ProductId == productId);
			return Content(JsonConvert.SerializeObject(new { Items = list }));
		}

		public ContentResult GetServiceTypes(int serviceKindId)
		{
			var list = dataLogic.GetServiceTypes().Where(w => w.ServiceKindId == serviceKindId);
			return Content(JsonConvert.SerializeObject(new { Items = list }));
		}



		public ContentResult GetOrderOperationsByTemplate(int templateId)
		{
			var list = dataLogic.GetOrderOperationsByTemplate(templateId);
			return Content(JsonConvert.SerializeObject(new { Items = list }));
		}

		public ContentResult GetOrderTemplatesByProduct(int productId)
		{
			var list = dataLogic.GetOrderTemplates().Where(w => w.ProductId == productId);
			return Content(JsonConvert.SerializeObject(new { Items = list }));
		}

		public ContentResult GetNewServiceType(int serviceKindId)
		{
			if (!IsSuperUser())
				return Content(JsonConvert.SerializeObject(new { Message = "Это может делать только GM." }));

			// подстановка значений по-умолчанию
			var c = new ServiceType { ServiceKindId = serviceKindId };
			return Content(JsonConvert.SerializeObject(c));
		}

		public ContentResult GetNewServiceKind(int productId)
		{
			if (!IsSuperUser())
				return Content(JsonConvert.SerializeObject(new { Message = "Это может делать только GM." }));

			// подстановка значений по-умолчанию
			var c = new ServiceKind { ProductId = productId };
			return Content(JsonConvert.SerializeObject(c));
		}

		public ContentResult GetNewProduct()
		{
			if (!IsSuperUser())
				return Content(JsonConvert.SerializeObject(new { Message = "Это может делать только GM." }));

			// подстановка значений по-умолчанию
			var c = new Product();
			return Content(JsonConvert.SerializeObject(c));
		}

		public ContentResult SaveProduct(Product model)
		{
			if (model.ID == 0)
			{
				var product = new Product();
				product.Display = model.Display;
				product.IsWorking = model.IsWorking;
				product.VolumetricRatioId = model.VolumetricRatioId;
				product.ManagerUserId = model.ManagerUserId;
				product.DeputyUserId = model.DeputyUserId;

				if ((product.ManagerUserId != model.ManagerUserId) || (product.DeputyUserId != model.DeputyUserId))
					if (!IsSuperUser())
						return Content(JsonConvert.SerializeObject(new { Message = "Поля Продакт-менеджер и Заместитель Продакт-Менеджера может редактировать только GM." }));

				var id = dataLogic.CreateProduct(product);
				return Content(JsonConvert.SerializeObject(new { ID = id }));
			}
			else
			{
				var product = dataLogic.GetProduct(model.ID);
				product.Display = model.Display;
				product.IsWorking = model.IsWorking;
				product.VolumetricRatioId = model.VolumetricRatioId;
				product.ManagerUserId = model.ManagerUserId;
				product.DeputyUserId = model.DeputyUserId;

				if ((product.ManagerUserId != model.ManagerUserId) || (product.DeputyUserId != model.DeputyUserId))
					if (!IsSuperUser())
						return Content(JsonConvert.SerializeObject(new { Message = "Поля Продакт-менеджер и Заместитель Продакт-Менеджера может редактировать только GM." }));

				dataLogic.UpdateProduct(product);
				return Content(JsonConvert.SerializeObject(""));
			}
		}

		public ContentResult SaveServiceKind(ServiceKind model)
		{
			if (model.ID == 0)
			{
				var entity = new ServiceKind();
				entity.ProductId = model.ProductId;
				entity.EnName = model.EnName;
				entity.Name = model.Name;
				entity.VatId = model.VatId;

				var id = dataLogic.CreateServiceKind(entity);
				return Content(JsonConvert.SerializeObject(new { ID = id }));
			}
			else
			{
				var kind = dataLogic.GetServiceKind(model.ID);
				kind.Name = model.Name;
				kind.EnName = model.EnName;
				kind.VatId = model.VatId;

				dataLogic.UpdateServiceKind(kind);
				return Content(JsonConvert.SerializeObject(""));
			}
		}

		public ContentResult SaveServiceType(ServiceType model)
		{
			if (model.ID == 0)
			{
				var entity = new ServiceType();
				entity.Name = model.Name;
				entity.Count = model.Count;
				entity.MeasureId = model.MeasureId;
				entity.Price = model.Price;
				entity.ServiceKindId = model.ServiceKindId;

				var id = dataLogic.CreateServiceType(entity);
				return Content(JsonConvert.SerializeObject(new { ID = id }));
			}
			else
			{
				var kind = dataLogic.GetServiceType(model.ID);
				kind.Name = model.Name;
				kind.Count = model.Count;
				kind.MeasureId = model.MeasureId;
				kind.Price = model.Price;

				dataLogic.UpdateServiceType(kind);
				return Content(JsonConvert.SerializeObject(""));
			}
		}

		public ContentResult SaveOrderTemplateOperation(OrderTemplateOperation model)
		{
			var entity = dataLogic.GetOrderTemplateOperation(model.OrderTemplateId, model.OrderOperationId);
			if (entity == null)
			{
				var op = new OrderTemplateOperation
				{
					OrderTemplateId = model.OrderTemplateId,
					OrderOperationId = model.OrderOperationId,
					No = model.No
				};

				dataLogic.CreateOrderTemplateOperation(op);
				return Content(JsonConvert.SerializeObject(""));
			}
			else
			{
				entity.No = model.No;

				dataLogic.UpdateOrderTemplateOperation(entity);
				return Content(JsonConvert.SerializeObject(""));
			}
		}

		public ContentResult DeleteOrderTemplateOperation(int orderTemplateId, int orderOperationId)
		{
			dataLogic.DeleteOrderTemplateOperation(orderTemplateId, orderOperationId);
			return Content(JsonConvert.SerializeObject(""));
		}

		#endregion

		#region операции

		public ActionResult OrderOperations()
		{
			if (!IsSuperUser())
				return RedirectToAction("NotAuthorized", "Home");

			var viewmodel = dataLogic.GetOrderOperations();
			ViewBag.Dictionaries = new Dictionary<string, object>();
			ViewBag.Dictionaries.Add("OperationKind", dataLogic.GetOperationKinds());
			ViewBag.Dictionaries.Add("Event", dataLogic.GetEvents());
			return View(viewmodel);
		}

		public ContentResult GetNewOrderOperation()
		{
			// подстановка значений по-умолчанию
			var c = new OrderOperation { };
			return Content(JsonConvert.SerializeObject(c));
		}

		public ContentResult SaveOrderOperation(OrderOperation model)
		{
			if (model.ID == 0)
			{
				var op = new OrderOperation();
				op.Name = model.Name;
				op.EnName = model.EnName;
				op.OperationKindId = model.OperationKindId;
				op.StartFactEventId = model.StartFactEventId;
				op.FinishFactEventId = model.FinishFactEventId;

				var id = dataLogic.CreateOrderOperation(op);
				return Content(JsonConvert.SerializeObject(new { ID = id }));
			}
			else
			{
				var op = dataLogic.GetOrderOperation(model.ID);
				op.Name = model.Name;
				op.EnName = model.EnName;
				op.OperationKindId = model.OperationKindId;
				op.StartFactEventId = model.StartFactEventId;
				op.FinishFactEventId = model.FinishFactEventId;

				dataLogic.UpdateOrderOperation(op);
				return Content(JsonConvert.SerializeObject(""));
			}
		}

		#endregion

		#region шаблоны заказов

		public ActionResult OrderTemplates()
		{
			if (!IsSuperUser())
				return RedirectToAction("NotAuthorized", "Home");

			var viewmodel = dataLogic.GetOrderTemplates();
			ViewBag.Dictionaries = new Dictionary<string, object>();
			ViewBag.Dictionaries.Add("OperationKind", dataLogic.GetOperationKinds());
			ViewBag.Dictionaries.Add("Event", dataLogic.GetEvents());
			return View(viewmodel);
		}

		public ContentResult GetNewOrderTemplate()
		{
			// подстановка значений по-умолчанию
			var c = new OrderTemplate { };
			return Content(JsonConvert.SerializeObject(c));
		}

		public ContentResult SaveOrderTemplate(OrderTemplate model)
		{
			if (model.ID == 0)
			{
				var op = new OrderTemplate { Name = model.Name };
				op.Name = model.Name;
				op.ProductId = model.ProductId;

				var id = dataLogic.CreateOrderTemplate(op);
				return Content(JsonConvert.SerializeObject(new { ID = id }));
			}
			else
			{
				var op = dataLogic.GetOrderTemplate(model.ID);
				op.Name = model.Name;
				op.ProductId = model.ProductId;

				dataLogic.UpdateOrderTemplate(op);
				return Content(JsonConvert.SerializeObject(""));
			}
		}

		public ContentResult DeleteOrderTemplate(int id)
		{
			dataLogic.DeleteOrderTemplate(id);
			return Content(JsonConvert.SerializeObject(""));
		}

		#endregion

		#region Страны и континенты

		public ActionResult Places()
		{
			if (!IsSuperUser())
				return RedirectToAction("NotAuthorized", "Home");

			var filter = new ListFilter { PageSize = 8 };
			var list = dataLogic.GetCountries(filter).ToList();
			var totalCount = dataLogic.GetCountriesCount(filter);
			var viewModel = new CountriesViewModel { Items = list, Filter = filter, TotalItemsCount = totalCount };
			return View(viewModel);
		}

		public ContentResult GetCountries(ListFilter filter)
		{
			var totalCount = dataLogic.GetCountriesCount(filter);
			var list = dataLogic.GetCountries(filter);
			return Content(JsonConvert.SerializeObject(new { Items = list, TotalCount = totalCount }));
		}

		public ContentResult GetRegions(ListFilter filter)
		{
			var totalCount = dataLogic.GetRegionsCount(filter);
			var list = dataLogic.GetRegions(filter);
			return Content(JsonConvert.SerializeObject(new { Items = list, TotalCount = totalCount }));
		}

		public ContentResult GetSubRegions(ListFilter filter)
		{
			var totalCount = dataLogic.GetSubRegionsCount(filter);
			var list = dataLogic.GetSubRegions(filter);
			return Content(JsonConvert.SerializeObject(new { Items = list, TotalCount = totalCount }));
		}

		public ContentResult GetPlaces(PlaceListFilter filter)
		{
			var totalCount = dataLogic.GetPlacesCount(filter);
			var list = dataLogic.GetPlaces(filter);
			return Content(JsonConvert.SerializeObject(new { Items = list, TotalCount = totalCount }));
		}

		public ContentResult GetNewPlace(int countryId, int? regionId, int? subregionId)
		{
			// подстановка значений по-умолчанию
			var c = new Place { CountryId = countryId, RegionId = regionId, SubRegionId = subregionId };
			return Content(JsonConvert.SerializeObject(c));
		}

		public ContentResult SaveCountry(Country model)
		{
			var country = dataLogic.GetCountry(model.ID);
			country.Name = model.Name;
			country.EnName = model.EnName;
			country.IsoCode = model.IsoCode;

			dataLogic.UpdateCountry(country);
			return Content(JsonConvert.SerializeObject(""));
		}

		public ContentResult SaveRegion(Region model)
		{
			var region = dataLogic.GetRegion(model.ID);
			region.Name = model.Name;
			region.EnName = model.EnName;
			region.IsoCode = model.IsoCode;

			dataLogic.UpdateRegion(region);
			return Content(JsonConvert.SerializeObject(""));
		}

		public ContentResult SaveSubRegion(SubRegion model)
		{
			var region = dataLogic.GetSubRegion(model.ID);
			region.Name = model.Name;
			region.EnName = model.EnName;

			dataLogic.UpdateSubRegion(region);
			return Content(JsonConvert.SerializeObject(""));
		}

		public ContentResult SavePlace(Place model)
		{
			if (model.ID == 0)
			{
				var place = new Place();
				place.Name = model.Name;
				place.EnName = model.EnName;
				place.Airport = model.Airport;
				place.IataCode = model.IataCode;
				place.IcaoCode = model.IcaoCode;
				place.CountryId = model.CountryId;
				place.RegionId = model.RegionId;
				place.SubRegionId = model.SubRegionId;

				int id = dataLogic.CreatePlace(place);
				return Content(JsonConvert.SerializeObject(new { ID = id }));
			}
			else
			{
				var place = dataLogic.GetPlace(model.ID);
				place.Name = model.Name;
				place.EnName = model.EnName;
				place.Airport = model.Airport;
				place.IataCode = model.IataCode;
				place.IcaoCode = model.IcaoCode;

				dataLogic.UpdatePlace(place);
				return Content(JsonConvert.SerializeObject(""));
			}
		}

		#endregion

		#region шаблоны должностей

		public ActionResult PositionTemplates()
		{
			if (!IsSuperUser())
				return RedirectToAction("NotAuthorized", "Home");

			var viewmodel = dataLogic.GetPositionTemplates();
			return View(viewmodel);
		}

		public ContentResult GetNewPositionTemplate()
		{
			// подстановка значений по-умолчанию
			var c = new PositionTemplate { };
			return Content(JsonConvert.SerializeObject(c));
		}

		public ContentResult SavePositionTemplate(PositionTemplate model)
		{
			if (model.ID == 0)
			{
				var op = new PositionTemplate();
				op.Basis = model.Basis;
				op.EnBasis = model.EnBasis;
				op.EnPosition = model.EnPosition;
				op.GenitivePosition = model.GenitivePosition;
				op.Position = model.Position;
				op.Department = model.Department;

				var id = dataLogic.CreatePositionTemplate(op);
				return Content(JsonConvert.SerializeObject(new { ID = id }));
			}
			else
			{
				var op = dataLogic.GetPositionTemplate(model.ID);
				op.Basis = model.Basis;
				op.EnBasis = model.EnBasis;
				op.EnPosition = model.EnPosition;
				op.GenitivePosition = model.GenitivePosition;
				op.Position = model.Position;
				op.Department = model.Department;

				dataLogic.UpdatePositionTemplate(op);
				return Content(JsonConvert.SerializeObject(""));
			}
		}

		public ContentResult DeletePositionTemplate(int id)
		{
			dataLogic.DeletePositionTemplate(id);
			return Content(JsonConvert.SerializeObject(""));
		}

		#endregion

		public ContentResult GetNewCargoDescription()
		{
			// подстановка значений по-умолчанию
			var c = new CargoDescription();
			return Content(JsonConvert.SerializeObject(c));
		}

		[HttpPost]
		public ContentResult UploadBicData()
		{
			//http://www.cbr.ru/mcirabis/BIK/bik_db_02082016.zip (Обратите внимание, ссылка меняется ежедневно, в выходные ссылка формируется следующим рабочим днем )
			// Распаковать и извлечь из него файл bnkseek.dbf.
			// Это файл формата DBF в кодировке CP866. для его просмотра Вы можете использовать MS Excel.

			if (Request.Files.Count == 0)
				return Content(JsonConvert.SerializeObject(new { Message = "Неверные входные данные" }));

			var file = Request.Files[0];

			var ef = new ExcelPackage(file.InputStream);

			var sheet = ef.Workbook.Worksheets[1];
			int bicIndex = 0;
			int pznIndex = 0;
			int uerIndex = 0;
			int tnpIndex = 0;
			int nnpIndex = 0;
			int nameIndex = 0;
			int ksnpIndex = 0;
			for (int ind = 1; ind < 32; ind++)
			{
				var cell = sheet.Cells[1, ind];
				switch (cell.Text)
				{
					case "NEWNUM":
						bicIndex = ind;
						break;

					case "PZN":
						pznIndex = ind;
						break;

					case "UER":
						uerIndex = ind;
						break;

					case "TNP":
						tnpIndex = ind;
						break;

					case "NNP":
						nnpIndex = ind;
						break;

					case "NAMEP":
						nameIndex = ind;
						break;

					case "KSNP":
						ksnpIndex = ind;
						break;
				}
			}

			var banks = bankLogic.GetBanks(new ListFilter());
			int parsed = 0;
			if (bicIndex > 0)
				for (int ind = 2; ind < 4096; ind++)
				{
					var bic = sheet.Cells[ind, bicIndex].Text;
					if (string.IsNullOrWhiteSpace(bic))
						break;

					var bank = banks.FirstOrDefault(w => w.BIC == bic);
					if (bank == null)
						bank = new Bank { BIC = bic };

					if (pznIndex > 0)
					{
						var val = sheet.Cells[ind, pznIndex].Text;
						int.TryParse(val, out parsed);
						bank.PZN = (parsed > 0) ? parsed : (int?)null;
					}

					if (uerIndex > 0)
					{
						var val = sheet.Cells[ind, uerIndex].Text;
						int.TryParse(val, out parsed);
						bank.UER = parsed;
					}

					if (tnpIndex > 0)
					{
						var val = sheet.Cells[ind, tnpIndex].Text;
						int.TryParse(val, out parsed);
						bank.TNP = parsed;
					}

					if (nnpIndex > 0)
						bank.NNP = sheet.Cells[ind, nnpIndex].Text;

					if (nameIndex > 0)
						bank.Name = sheet.Cells[ind, nameIndex].Text;

					if (ksnpIndex > 0)
						bank.KSNP = sheet.Cells[ind, ksnpIndex].Text;

					if (bank.ID > 0)
						bankLogic.UpdateBank(bank);
					else
						bankLogic.CreateBank(bank);
				}

			return Content(JsonConvert.SerializeObject(new { Message = "" }));
		}

		[HttpPost]
		public ContentResult UploadRusSwiftData()
		{
			// http://www.cbr.ru/analytics/?PrtId=digest

			if (Request.Files.Count == 0)
				return Content(JsonConvert.SerializeObject(new { Message = "Неверные входные данные" }));

			var file = Request.Files[0];

			var ef = new ExcelPackage(file.InputStream);

			var sheet = ef.Workbook.Worksheets[1];
			int bicIndex = 0;
			int swiftIndex = 0;
			int nameIndex = 0;
			for (int ind = 1; ind < 8; ind++)
			{
				var cell = sheet.Cells[1, ind];
				switch (cell.Text)
				{
					case "KOD_RUS":
						bicIndex = ind;
						break;

					case "NAME_SRUS":
						nameIndex = ind;
						break;

					case "KOD_SWIFT":
						swiftIndex = ind;
						break;
				}
			}

			var banks = bankLogic.GetBanks(new ListFilter());

			if (bicIndex > 0)
				for (int ind = 2; ind < 512; ind++) // сраная рашка катится в сраное г. Число банков, работающих международно будет сокращаться.
				{
					var bic = sheet.Cells[ind, bicIndex].Text;
					if (string.IsNullOrWhiteSpace(bic))
						break;

					var bank = banks.FirstOrDefault(w => w.BIC == bic);
					if (bank == null)
						continue;

					if (swiftIndex > 0)
						bank.SWIFT = sheet.Cells[ind, swiftIndex].Text;

					// проверить соответствие названия?
					//if (nameIndex > 0)
					//	bank.Name = sheet.Cells[ind, nameIndex].Text;

					bankLogic.UpdateBank(bank);
				}

			return Content(JsonConvert.SerializeObject(new { Message = "" }));
		}

		[HttpPost]
		public ContentResult RefreshRates()
		{
			using (var db = new LogistoDb())
			{
				//var maxDate = db.CurrencyRates.Max(w => w.Date).Value;
				var maxDate = DateTime.Today.AddMonths(-1);

				while (maxDate.Date <= DateTime.Today)
				{
					string url = "http://www.cbr.ru/currency_base/daily.aspx?date_req=" + maxDate.ToString("dd-MM-yyyy");
					var webRequest = (HttpWebRequest)HttpWebRequest.Create(url);
					var webResponse = (HttpWebResponse)webRequest.GetResponse();
					string html = new StreamReader(webResponse.GetResponseStream()).ReadToEnd();

					// получить доллары
					var rate = GetRate("USD", html);
					rate = rate.Replace(',', '.');
					double parsed = 0;
					double.TryParse(rate, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out parsed);
					CreateOrUpdateRate(db, new CurrencyRate { CurrencyId = 2, Date = maxDate.Date, Rate = parsed });
					// получить евро
					rate = GetRate("EUR", html);
					rate = rate.Replace(',', '.');
					double.TryParse(rate, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out parsed);
					CreateOrUpdateRate(db, new CurrencyRate { CurrencyId = 3, Date = maxDate.Date, Rate = parsed });
					// получить 
					rate = GetRate("GBP", html);
					rate = rate.Replace(',', '.');
					double.TryParse(rate, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out parsed);
					CreateOrUpdateRate(db, new CurrencyRate { CurrencyId = 6, Date = maxDate.Date, Rate = parsed });
					// получить 
					//rate = GetRate(maxDate, "CNY", html);
					rate = GetCNYRate(html);
					rate = rate.Replace(',', '.');
					double.TryParse(rate, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out parsed);
					if (parsed > 0)
						CreateOrUpdateRate(db, new CurrencyRate { CurrencyId = 5, Date = maxDate.Date, Rate = parsed });
					else
					{
						rate = Get10CNYRate(html);
						rate = rate.Replace(',', '.');
						double.TryParse(rate, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out parsed);
						if (parsed > 0)
							CreateOrUpdateRate(db, new CurrencyRate { CurrencyId = 5, Date = maxDate.Date, Rate = parsed / 10 });   // курс за 10 единиц
					}

					maxDate = maxDate.AddDays(1);
				}

				db.SystemSettings.Where(w => w.Name == "CurrencyUpdate").Set(u => u.Value, DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")).Update();
			}

			return Content(JsonConvert.SerializeObject(new { Message = "Готово." }));
		}

		public ContentResult RecalculateContractors()
		{
			System.Diagnostics.Debug.WriteLine(DateTime.Now.ToString("mm.ss.ffff"));
			var contractors = contractorLogic.GetContractors(new ListFilter());
			foreach (var item in contractors)
				accountingLogic.CalculateContractorBalance(item.ID);

			System.Diagnostics.Debug.WriteLine(DateTime.Now.ToString("mm.ss.ffff"));
			return Content(JsonConvert.SerializeObject("Готово"));
		}

		public ContentResult RecalculateContractor(int id)
		{
			accountingLogic.CalculateContractorBalance(id);
			return Content(JsonConvert.SerializeObject("Готово"));
		}

		public ContentResult RecalculateOrders()
		{
			var orders = orderLogic.GetAllOrders();
			foreach (var item in orders)
				orderLogic.CalculateOrderBalance(item.ID);

			return Content(JsonConvert.SerializeObject("Готово"));
		}

		#region AutoExpense

		public ActionResult AutoExpenses()
		{
			if (!IsSuperUser())
				return RedirectToAction("NotAuthorized", "Home");

			var viewModel = dataLogic.GetAutoExpenses();
			return View(viewModel);
		}

		public ContentResult GetNewAutoExpense()
		{
			// подстановка значений по-умолчанию
			var c = new AutoExpense { From = DateTime.Today, To = DateTime.Today.AddMonths(1) };
			return Content(JsonConvert.SerializeObject(c));
		}

		public ContentResult SaveAutoExpense(AutoExpense model)
		{
			if (model.ID == 0)
			{
				var id = dataLogic.CreateAutoExpense(model);
				return Content(JsonConvert.SerializeObject(new { ID = id }));
			}
			else
			{
				if (!IsSuperUser())
					return Content(JsonConvert.SerializeObject(new { Message = "У вас недостаточно прав" }));

				// обновить позицию
				var cru = dataLogic.GetAutoExpense(model.ID);
				cru.From = model.From;
				cru.To = model.To;
				cru.CNY = model.CNY;
				cru.EUR = model.EUR;
				cru.GBP = model.GBP;
				cru.USD = model.USD;

				dataLogic.UpdateAutoExpense(cru);

				return Content(JsonConvert.SerializeObject(""));
			}
		}

		#endregion

		#region PayMethod

		public ActionResult PayMethods()
		{
			if (!IsSuperUser())
				return RedirectToAction("NotAuthorized", "Home");

			var viewModel = dataLogic.GetPayMethods();
			return View(viewModel);
		}

		public ContentResult GetNewPayMethod()
		{
			// подстановка значений по-умолчанию
			var c = new PayMethod { From = DateTime.Today, To = DateTime.Today.AddMonths(1) };
			return Content(JsonConvert.SerializeObject(c));
		}

		public ContentResult SavePayMethod(PayMethod model)
		{
			if (model.ID == 0)
			{
				var id = dataLogic.CreatePayMethod(model);
				return Content(JsonConvert.SerializeObject(new { ID = id }));
			}
			else
			{
				if (!IsSuperUser())
					return Content(JsonConvert.SerializeObject(new { Message = "У вас недостаточно прав" }));

				// обновить позицию
				var cru = dataLogic.GetPayMethod(model.ID);
				cru.From = model.From;
				cru.To = model.To;
				cru.Display = model.Display;

				dataLogic.UpdatePayMethod(cru);

				return Content(JsonConvert.SerializeObject(""));
			}
		}

		#endregion

		#region Schedule

		public ActionResult Schedules()
		{
			if (!IsSuperUser())
				return RedirectToAction("NotAuthorized", "Home");

			var viewModel = dataLogic.GetSchedules();
			return View(viewModel);
		}

		public ContentResult GetNewSchedule()
		{
			// подстановка значений по-умолчанию
			var c = new Schedule { };
			return Content(JsonConvert.SerializeObject(c));
		}

		public ContentResult SaveSchedule(Schedule model)
		{
			if (model.ID == 0)
			{
				var id = dataLogic.CreateSchedule(model);
				return Content(JsonConvert.SerializeObject(new { ID = id }));
			}
			else
			{
				if (!IsSuperUser())
					return Content(JsonConvert.SerializeObject(new { Message = "У вас недостаточно прав" }));

				// обновить позицию
				var s = dataLogic.GetSchedule(model.ID);
				s.Weekday = model.Weekday;
				s.Hour = model.Hour;
				s.Minute = model.Minute;
				s.ReportName = model.ReportName;

				dataLogic.UpdateSchedule(s);

				return Content(JsonConvert.SerializeObject(""));
			}
		}

		public ContentResult DeleteSchedule(int id)
		{
			if (!IsSuperUser())
				return Content(JsonConvert.SerializeObject(new { Message = "У вас недостаточно прав" }));

			dataLogic.DeleteSchedule(id);

			return Content(JsonConvert.SerializeObject(""));
		}

		#endregion

		void CreateOrUpdateRate(LogistoDb db, CurrencyRate rate)
		{
			var exRate = db.CurrencyRates.Delete(w => w.CurrencyId == rate.CurrencyId && w.Date == rate.Date);
			db.InsertWithIdentity(rate);
		}

		string GetRate(string currency, string html)
		{
			string pattern = "";

			if (currency == "USD")
				pattern = "Доллар США</td>\r\n<td>(.*)</td>";
			else if (currency == "EUR")
				pattern = "Евро</td>\r\n<td>(.*)</td>";
			//else if (currency == "CNY")
			//	pattern = "Китайских юаней</td>\r\n<td>(.*)</td>";
			else if (currency == "GBP")
				pattern = "Фунт стерлингов Соединенного королевства</td>\r\n<td>(.*)</td>";

			Match match = Regex.Match(html, pattern);
			if (match.Success)
				return match.Groups[1].ToString();
			else
				return "";
		}

		string Get10CNYRate(string html)
		{
			Match match = Regex.Match(html, "Китайских юаней</td>\r\n<td>(.*)</td>");
			if (match.Success)
				return match.Groups[1].ToString();
			else
				return "";
		}

		string GetCNYRate(string html)
		{
			Match match = Regex.Match(html, "Китайский юань</td>\r\n<td>(.*)</td>");
			if (match.Success)
				return match.Groups[1].ToString();
			else
				return "";
		}
	}
}






