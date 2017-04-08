using System;
using System.Linq;
using System.Web.Mvc;
using Logisto.Models;
using Logisto.ViewModels;
using Newtonsoft.Json;

namespace Logisto.Controllers
{
	[Authorize]
	public class PricelistsController : BaseController
	{
		#region Pages

		public ActionResult Index()
		{
			var model = new PricelistsViewModel { Items = pricelistLogic.GetPricelists(0), Filter = new ListFilter { PageSize = 25 } };
			model.Dictionaries.Add("ContractorByContract", dataLogic.GetContractorsByContract());
			model.Dictionaries.Add("LegalByContract", dataLogic.GetLegalsByContract());
			model.Dictionaries.Add("FinRepCenter", dataLogic.GetFinRepCenters());
			model.Dictionaries.Add("Contract", dataLogic.GetContracts());
			model.Dictionaries.Add("Product", dataLogic.GetProducts());
			return View(model);
		}

		public ActionResult View(int id)
		{
			var item = pricelistLogic.GetPricelist(id);
			var model = new PricelistViewModel
			{
				ID = item.ID,
				ContractId = item.ContractId,
				Data = item.Data,
				FinRepCenterId = item.FinRepCenterId,
				From = item.From,
				Name = item.Name,
				ProductId = item.ProductId,
				To = item.To,
				Comment = item.Comment,
				IsDataExists = item.Data != null,
				Prices = pricelistLogic.GetPrices(id).ToList()
			};

			model.Dictionaries.Add("FinRepCenter", dataLogic.GetFinRepCenters());
			model.Dictionaries.Add("Currency", dataLogic.GetCurrencies());
			model.Dictionaries.Add("Contract", dataLogic.GetContracts());
			model.Dictionaries.Add("Product", dataLogic.GetProducts());
			model.Dictionaries.Add("Service", dataLogic.GetServiceTypes());
			model.Dictionaries.Add("Vat", dataLogic.GetVats());
			return View(model);
		}

		public ActionResult Create(int? contractId)
		{
			var pricelist = new Pricelist { ContractId = contractId, Name = "Новый прайслист" };
			int id = pricelistLogic.CreatePricelist(pricelist);
			return RedirectToAction("Edit", new { Id = id });
		}

		public ActionResult Edit(int id)
		{
			// TODO: roles
			var pricelist = pricelistLogic.GetPricelist(id);
			// TEMP:
			if (pricelist.ProductId.HasValue)
				pricelistLogic.FixPricelistKinds(id, pricelist.ProductId.Value);

			var model = new PricelistViewModel
			{
				ID = pricelist.ID,
				Name = pricelist.Name,
				Comment = pricelist.Comment,
				ProductId = pricelist.ProductId,
				ContractId = pricelist.ContractId,
				FinRepCenterId = pricelist.FinRepCenterId,
				From = pricelist.From,
				To = pricelist.To,
				Prices = pricelistLogic.GetPrices(id).ToList(),
				PriceKinds = pricelistLogic.GetPriceKinds(id).ToList()
			};

			model.Dictionaries.Add("ContractorByContract", dataLogic.GetContractorsByContract());
			model.Dictionaries.Add("FinRepCenter", dataLogic.GetFinRepCenters());
			model.Dictionaries.Add("Contractor", dataLogic.GetContractors());
			model.Dictionaries.Add("Contract", dataLogic.GetContracts());
			model.Dictionaries.Add("Currency", dataLogic.GetCurrencies());
			model.Dictionaries.Add("Product", dataLogic.GetProducts());
			model.Dictionaries.Add("Service", dataLogic.GetServiceTypes());
			model.Dictionaries.Add("Measure", dataLogic.GetMeasures());
			model.Dictionaries.Add("Vat", dataLogic.GetVats());

			return View(model);
		}

		#endregion

		public ContentResult GetItems(ListFilter filter)
		{
			var totalCount = pricelistLogic.GetPricelistsCount(filter);
			var list = pricelistLogic.GetPricelists(filter).Select(s => new { s.Comment, s.ContractId, s.Filename, s.FinRepCenterId, s.From, s.ID, s.Name, s.ProductId, s.To });
			return Content(JsonConvert.SerializeObject(new { Items = list, TotalCount = totalCount }));
		}

		public ContentResult GetPrices(int pricelistId)
		{
			return Content(JsonConvert.SerializeObject(new { Items = pricelistLogic.GetPrices(pricelistId) }));
		}

		public ContentResult GetPriceKinds(int pricelistId)
		{
			return Content(JsonConvert.SerializeObject(new { Items = pricelistLogic.GetPriceKinds(pricelistId) }));
		}

		public ContentResult SavePricelist(Pricelist model)
		{
			bool isNeedReload = false;
			var entity = pricelistLogic.GetPricelist(model.ID);

			if (!IsAllowedPricelist(model))
				return Content(JsonConvert.SerializeObject(new { Message = "Прайслист на этот ЦФО+Продукт(+Договор) в этом периоде уже есть." }));

			var product = dataLogic.GetProduct(model.ProductId.Value);
			if ((product.ManagerUserId != CurrentUserId) && (product.DeputyUserId != CurrentUserId) && !IsSuperUser())
				return Content(JsonConvert.SerializeObject(new { Message = "Вы не менеджер данного продукта." }));

			if (model.ProductId != entity.ProductId)
			{
				pricelistLogic.ClearPricelist(model.ID);
				pricelistLogic.InitPricelist(model.ID, model.ProductId.Value);
				isNeedReload = true;
			}

			entity.Name = model.Name;
			entity.FinRepCenterId = model.FinRepCenterId;
			entity.ContractId = model.ContractId;
			entity.ProductId = model.ProductId;
			entity.Comment = model.Comment;
			entity.From = model.From;
			entity.To = model.To;

			pricelistLogic.UpdatePricelist(entity);
			return Content(JsonConvert.SerializeObject(new { IsNeedReload = isNeedReload }));
		}

		public ContentResult SavePrice(Price model)
		{
			var pricelist = pricelistLogic.GetPricelist(model.PricelistId);
			var product = dataLogic.GetProduct(pricelist.ProductId.Value);
			if ((product.ManagerUserId != CurrentUserId) && (product.DeputyUserId != CurrentUserId) && !IsSuperUser())
				return Content(JsonConvert.SerializeObject(new { Message = "Вы не менеджер данного продукта." }));

			var entity = pricelistLogic.GetPrice(model.ID);
			entity.Count = model.Count;
			entity.CurrencyId = model.CurrencyId;
			entity.Sum = model.Sum;
			entity.VatId = model.VatId;

			pricelistLogic.UpdatePrice(entity);
			return Content(JsonConvert.SerializeObject(""));
		}

		public ContentResult SavePriceKind(PriceKind model)
		{
			var pricelist = pricelistLogic.GetPricelist(model.PricelistId);
			var product = dataLogic.GetProduct(pricelist.ProductId.Value);
			if ((product.ManagerUserId != CurrentUserId) && (product.DeputyUserId != CurrentUserId) && !IsSuperUser())
				return Content(JsonConvert.SerializeObject(new { Message = "Вы не менеджер данного продукта." }));

			var entity = pricelistLogic.GetPriceKind(model.ID);
			entity.Name = model.Name;
			entity.EnName = model.EnName;

			pricelistLogic.UpdatePriceKind(entity);
			return Content(JsonConvert.SerializeObject(""));
		}

		public ContentResult ImportPricelist(int fromPricelistId, int toPricelistId)
		{
			var fromEntity = pricelistLogic.GetPricelist(fromPricelistId);
			var toEntity = pricelistLogic.GetPricelist(toPricelistId);

			var product = dataLogic.GetProduct(toEntity.ProductId.Value);
			if ((product.ManagerUserId != CurrentUserId) && (product.DeputyUserId != CurrentUserId) && !IsSuperUser())
				return Content(JsonConvert.SerializeObject(new { Message = "Вы не менеджер данного продукта." }));

			if (fromEntity.ProductId != toEntity.ProductId)
				return Content(JsonConvert.SerializeObject(new { Message = "Продукт не совпадает." }));

			pricelistLogic.ClearPricelist(toPricelistId);
			pricelistLogic.ImportPricelist(fromPricelistId, toPricelistId);

			return Content(JsonConvert.SerializeObject(""));
		}

		public ContentResult UploadFile(int id)
		{
			if (id <= 0 || Request.Files.Count == 0)
				return Content(JsonConvert.SerializeObject(new { Message = "Неверные входные данные" }));

			var file = Request.Files[0];

			var pricelist = pricelistLogic.GetPricelist(id);
			pricelist.Filename = System.IO.Path.GetFileName(file.FileName);
			pricelist.Data = new byte[file.InputStream.Length];
			file.InputStream.Read(pricelist.Data, 0, (int)file.InputStream.Length);

			pricelistLogic.UpdatePricelistData(pricelist);

			return Content(JsonConvert.SerializeObject(""));
		}

		public FileContentResult GetPricelistData(int id)
		{
			var doc = pricelistLogic.GetPricelist(id);
			var contentType = "application/octet-stream";
			switch (System.IO.Path.GetExtension(doc.Filename).ToUpper())
			{
				case "PDF":
					contentType = "application/pdf";
					break;

				case "PNG":
					contentType = "image/png";
					break;

				default:
					contentType = "image/jpeg";
					break;
			}

			return File(doc.Data, contentType, doc.Filename);
		}

		bool IsAllowedPricelist(Pricelist model)
		{
			var fromDate = model.From ?? DateTime.MinValue;
			var toDate = model.To ?? DateTime.MaxValue;
			var pricelists = pricelistLogic.GetPricelists(new ListFilter()).Where(w => (w.ID != model.ID) && (w.FinRepCenterId == model.FinRepCenterId) && (w.ProductId == model.ProductId) && (w.ContractId == model.ContractId));
			foreach (var item in pricelists)
			{
				var itemFromDate = item.From ?? DateTime.MinValue;
				var itemToDate = item.To ?? DateTime.MaxValue;

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
	}
}
