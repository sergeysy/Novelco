var PricelistViewModel = function (source, options)
{
	var model = this;

	model.Options = $.extend({
		UploadPricelistDataUrl: null,
		GetPricelistsItemsUrl: null,
		ImportPricelistUrl: null,
		GetContractorUrl: null,
		GetContractsUrl: null,
		GetPriceKindsUrl: null,
		GetPricesUrl: null,
		SavePriceUrl: null,
		SavePriceKindUrl: null,
		SavePricelistUrl: null
	}, options);

	// поля ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	model.Pricelist = ko.mapping.fromJS(source);
	model.ContractorId = ko.observable();
	model.Contracts = ko.observableArray();

	model.IsDirty = ko.observable(false);
	// Справочники
	model.Dictionaries = ko.mapping.fromJS(source.Dictionaries);

	// вычисляемое/подписки ///////////////////////////////////////////////////////////////////////////////////////////////////

	model.ContractorId.subscribe(function (newValue)
	{
		if (newValue)
			$.ajax({
				type: "POST",
				async: false,	// ВАЖНО при инициализации
				url: model.Options.GetContractsUrl,
				data: { ContractorId: newValue },
				success: function (response)
				{
					var r = JSON.parse(response);
					if (r.Message)
						alert(r.Message);
					else
						model.Contracts(ko.mapping.fromJSON(response).Items());
				}
			});
	});

	// функции ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	model.ExtendPricelist = function (Pricelist)
	{
		Pricelist.IsSubscribed = false;
		Pricelist.FieldsSubscription = function ()
		{
			var old = "";
			//one-time dirty flag that gives up its dependencies on first change
			var result = ko.computed(function ()
			{
				if (!Pricelist.IsSubscribed)
				{
					// for subscriptions
					old += Pricelist.Name();
					old += Pricelist.Comment();
					old += Pricelist.From();
					old += Pricelist.To();
					old += Pricelist.ProductId();
					old += Pricelist.ContractId();

					Pricelist.IsSubscribed = true;
				}
				else
				{
					var newV = ko.toJS(Pricelist);	// TEMP:
					model.IsDirty(true);
				}
			});

			return result;
		};

		Pricelist.FieldsSubscription();
	};

	model.ValidatePricelist = function (entity)
	{

		return true;
	};

	model.Save = function ()
	{
		if (!model.ValidatePricelist(model.Pricelist))
			return;

		// формирование DTO
		var data = {
			ID: model.Pricelist.ID(),
			FinRepCenterId: model.Pricelist.FinRepCenterId(),
			ContractId: model.Pricelist.ContractId(),
			ProductId: model.Pricelist.ProductId(),
			From: app.utility.SerializeDateTime(model.Pricelist.From()),
			To: app.utility.SerializeDateTime(model.Pricelist.To()),
			Name: model.Pricelist.Name(),
			Comment: model.Pricelist.Comment()
		};

		$.ajax({
			type: "POST",
			url: model.Options.SavePricelistUrl,
			data: JSON.stringify(data),
			dataType: "json",
			contentType: "application/json; charset=utf-8",
			success: function (response)
			{
				if (response && response.Message)
					alert(response.Message);
				else
				{
					if (response.IsNeedReload)
						$.ajax({
							type: "POST",
							url: model.Options.GetPricesUrl,
							data: { PricelistId: model.Pricelist.ID() },
							success: function (response)
							{
								var r = JSON.parse(response);
								if (r.Message)
									alert(r.Message);
								else
									model.Pricelist.Prices(ko.mapping.fromJSON(response).Items());
							}
						});

					model.IsDirty(false);
				}
			}
		});
	};

	model.SavePrice = function (data)
	{
		$.ajax({
			type: "POST",
			url: model.Options.SavePriceUrl,
			data: {
				ID: data.ID(),
				PricelistId: model.Pricelist.ID,
				Count: data.Count(),
				VatId: data.VatId(),
				CurrencyId: data.CurrencyId(),
				Sum: data.Sum()
			},
			success: function (response)
			{
				var r = JSON.parse(response);
				if (r.Message)
					alert(r.Message);
			}
		});
	};

	model.SavePriceKind = function (data)
	{
		$.ajax({
			type: "POST",
			url: model.Options.SavePriceKindUrl,
			data: {
				ID: data.ID(),
				PricelistId: model.Pricelist.ID,
				Name: data.Name(),
				EnName: data.EnName()
			},
			success: function (response)
			{
				var r = JSON.parse(response);
				if (r.Message)
					alert(r.Message);
			}
		});
	};

	model.ImportPrices = function (fromPricelist)
	{
		$.ajax({
			type: "POST",
			url: model.Options.ImportPricelistUrl,
			data: { FromPricelistId: fromPricelist.ID(), ToPricelistId: model.Pricelist.ID() },
			success: function (response)
			{
				if (response && response.Message)
					alert(response.Message);
				else
				{
					$.ajax({
						type: "POST",
						url: model.Options.GetPricesUrl,
						data: { PricelistId: model.Pricelist.ID() },
						success: function (response)
						{
							var r = JSON.parse(response);
							if (r.Message)
								alert(r.Message);
							else
								model.Pricelist.Prices(ko.mapping.fromJSON(response).Items());
						}
					});
				}
			}
		});
	};

	// #region price edit modal ///////////////////////////////////////////////////////////////////////////////////////////////

	var priceModalSelector = "#priceEditModal";

	model.OpenPriceEdit = function (document)
	{
		model.PriceEditModal.CurrentItem(document);
		$(priceModalSelector).modal("show");
		$(priceModalSelector).draggable({ handle: ".modal-header" });;
		model.PriceEditModal.Init();
	};

	model.PriceEditModal = {
		CurrentItem: ko.observable(),
		OnClosed: null,
		Close: function (self, e)
		{
			$(priceModalSelector).modal("hide");
			if (self.OnClosed != null)
				self.OnClosed();
		},
		Init: function () { },
		Done: function (self, e)
		{
			$(priceModalSelector).modal("hide");
			// сохранить изменения
			model.SavePrice(self.CurrentItem());
			self.CurrentItem(null);
		}
	};

	// #endregion

	// #region price select modal /////////////////////////////////////////////////////////////////////////////////////////////

	var pricelistModalSelector = "#pricelistSelectModal";

	model.ImportPricelist = function ()
	{
		model.PricelistSelectModal.CurrentItem(model.Pricelist);
		$(pricelistModalSelector).modal("show");
		$(pricelistModalSelector).draggable({ handle: ".modal-header" });;
		model.PricelistSelectModal.Init();
	};

	model.PricelistSelectModal = {
		CurrentItem: ko.observable(),
		SelectedItem: ko.observable(),
		Pricelists: ko.observable(),
		OnClosed: null,
		Close: function (self, e)
		{
			$(pricelistModalSelector).modal("hide");
			if (self.OnClosed != null)
				self.OnClosed();
		},
		Init: function ()
		{
			$.ajax({
				type: "POST",
				url: model.Options.GetPricelistsItemsUrl,
				data: {},
				success: function (response)
				{
					var items = ko.mapping.fromJSON(response).Items();
					model.PricelistSelectModal.Pricelists(ko.utils.arrayFilter(items, function (item) { return item.ProductId() == model.PricelistSelectModal.CurrentItem().ProductId() }));
				}
			});

		},
		Select: function (selected)
		{
			model.PricelistSelectModal.SelectedItem(selected);
		},
		Done: function (self, e)
		{
			$(pricelistModalSelector).modal("hide");
			// TODO: проверить, чтобы не выбрал сам себя
			// сохранить изменения
			model.ImportPrices(self.SelectedItem());
			self.CurrentItem(null);
		}
	};

	// #endregion

	// #region pricekind edit modal ///////////////////////////////////////////////////////////////////////////////////////////

	var priceKindModalSelector = "#priceKindEditModal";

	model.OpenPriceKindEdit = function (entity)
	{
		model.PriceKindEditModal.CurrentItem(entity);
		$(priceKindModalSelector).modal("show");
		$(priceKindModalSelector).draggable({ handle: ".modal-header" });;
		model.PriceKindEditModal.Init();
	};

	model.PriceKindEditModal = {
		CurrentItem: ko.observable(),
		OnClosed: null,
		Close: function (self, e)
		{
			$(priceKindModalSelector).modal("hide");
			if (self.OnClosed != null)
				self.OnClosed();
		},
		Init: function () { },
		Done: function (self, e)
		{
			$(priceKindModalSelector).modal("hide");
			// сохранить изменения
			model.SavePriceKind(self.CurrentItem());
			self.CurrentItem(null);
		}
	};

	// #endregion

	model.GetContractCurrenciesDisplay = function (contract)
	{
		var result = "";
		if (contract.Currencies)
			ko.utils.arrayForEach(contract.Currencies(), function (item) { result += app.utility.GetDisplay(model.Dictionaries.Currency, item.CurrencyId()) + " " });

		return result;
	};

	model.GetMeasureDisplay = function (serviceId)
	{
		var id = ko.unwrap(serviceId);
		var svc = ko.utils.arrayFirst(model.Dictionaries.Service(), function (item) { return item.ID() == id });
		if (svc)
			return app.utility.GetDisplay(model.Dictionaries.Measure, svc.MeasureId);

		return "";
	};

	// #region sorters ////////////////////////////////////////////////////////////////////////////////////////////////////////

	model.PriceSorter = {
		Field: ko.observable(),
		Asc: ko.observable(true),
		Sort: function (field)
		{
			var sorter = this;
			if (sorter.Field() == field)
				sorter.Asc(!sorter.Asc());

			sorter.Field(field);
			var func = function ()
			{
			};
			switch (field)
			{
				case "Name":
					if (sorter.Asc())
						func = function (l, r)
						{
							return app.utility.GetDisplay(model.Dictionaries.Service, l.ServiceId) < app.utility.GetDisplay(model.Dictionaries.Service, r.ServiceId) ? -1 : app.utility.GetDisplay(model.Dictionaries.Service, l.ServiceId) > app.utility.GetDisplay(model.Dictionaries.Service, r.ServiceId) ? 1 : 0;
						};
					else
						func = function (r, l)
						{
							return app.utility.GetDisplay(model.Dictionaries.Service, l.ServiceId) < app.utility.GetDisplay(model.Dictionaries.Service, r.ServiceId) ? -1 : app.utility.GetDisplay(model.Dictionaries.Service, l.ServiceId) > app.utility.GetDisplay(model.Dictionaries.Service, r.ServiceId) ? 1 : 0;
						};

					break;
			}

			model.Pricelist.Prices.sort(func);
		},
		Css: function (field)
		{
			var sorter = this;
			if (sorter.Field() != field)
				return "";

			return sorter.Asc() ? "asc-sorted" : "desc-sorted";
		}
	};

	// #endregion

	// инициализация //////////////////////////////////////////////////////////////////////////////////////////////////////////

	// init contractor/contracts
	if (model.Pricelist.ContractId())
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.GetContractorUrl,
			data: { ContractId: model.Pricelist.ContractId() },
			success: function (response)
			{
				var r = JSON.parse(response);
				if (r.Message)
					alert(r.Message);
				else
				{
					var contractor = ko.mapping.fromJSON(response);
					model.ContractorId(contractor.ID());
				}
			}
		});

	model.ExtendPricelist(model.Pricelist);

	// Стандарный input для файлов
	$('#dataUpload').on("change", function (e)
	{
		var formData = new FormData();
		formData.append("File", e.currentTarget.files[0]);
		var filename = e.currentTarget.files[0].name;
		var filesize = e.currentTarget.files[0].size;

		$.ajax({
			url: model.Options.UploadPricelistDataUrl + "/" + model.Pricelist.ID(),
			type: 'POST',
			data: formData,
			success: function (response) { alert("Файл успешно загружен"); },
			cache: false,
			contentType: false,
			processData: false
		});
	});

	// Контейнер, куда можно помещать файлы методом drag and drop
	var dropBox = $('.fileDropable');
	dropBox.on("dragenter", function ()
	{
		$(this).addClass('highlighted');
		return false;
	});
	dropBox.on("dragover", function () { return false; });
	dropBox.on("dragleave", function ()
	{
		$(this).removeClass('highlighted');
		return false;
	});
	dropBox.on("drop", function (e)
	{
		$(this).removeClass('highlighted');
		var dt = e.originalEvent.dataTransfer;
		var formData = new FormData();
		formData.append("File", dt.files[0]);
		var filename = dt.files[0].name;
		var filesize = dt.files[0].size;

		$.ajax({
			url: model.Options.UploadPricelistDataUrl + "/" + model.Pricelist.ID(),
			type: 'POST',
			data: formData,
			success: function (response) { alert("Файл успешно загружен"); },
			cache: false,
			contentType: false,
			processData: false
		});

		return false;
	});

	setTimeout(function ()
	{
		model.IsDirty(false);
	}, 100);

	window.onbeforeunload = function (evt)
	{
		if (model.IsDirty())
		{
			var message = "Есть несохраненные изменения. Если просто так уйти со страницы, то они не сохранятся.";
			if (typeof evt == "undefined")
				evt = window.event;

			if (evt)
				evt.returnValue = message;

			return message;
		}
	}
}

$(function ()
{
	ko.applyBindings(new PricelistViewModel(modelData, {
		UploadPricelistDataUrl: app.urls.UploadPricelistData,
		GetPricelistsItemsUrl: app.urls.GetPricelists,
		ImportPricelistUrl: app.urls.ImportPricelist,
		GetContractorUrl: app.urls.GetContractorByContract,
		GetContractsUrl: app.urls.GetContracts,
		GetPriceKindsUrl: app.urls.GetPriceKinds,
		GetPricesUrl: app.urls.GetPrices,
		SavePriceUrl: app.urls.SavePrice,
		SavePriceKindUrl: app.urls.SavePriceKind,
		SavePricelistUrl: app.urls.SavePricelist
	}), document.getElementById("ko-root"));
});