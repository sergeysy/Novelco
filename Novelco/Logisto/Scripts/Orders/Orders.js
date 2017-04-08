var OrdersViewModel = function (source, options)
{
	var model = this;

	model.Options = $.extend({
		GetItemsUrl: null,
		ContractorDetailsUrl: null,
		ContractDetailsUrl: null,
		LegalDetailsUrl: null,
		OrderDetailsUrl: null,
		UserDetailsUrl: null,
		GetBankAccountsByLegalItemsUrl: null,
		GetContractCurrenciesUrl: null,
		GetDocumentsByContractItemsUrl: null,
		GetContractUrl: null
	}, options);

	// поля ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	// Фильтр
	model.Filter = ko.mapping.fromJS(source.Filter);
	// Список записей
	model.Items = ko.observableArray(source.Items);
	// Общее количество записей для текущего фильтра
	model.TotalItemsCount = ko.observable(source.TotalItemsCount);
	// Справочники
	model.Dictionaries = source.Dictionaries;
	// идет запрос фильтра
	model.DelayHandler = null;
	//
	model.Filter.Statuses = ko.utils.arrayMap(model.Dictionaries.OrderStatus, function (item) { return { ID: item.ID, Name: item.Display, isChecked: ko.observable(false) }; });
	model.Filter.SelectedItems = ko.observableArray();
	model.Filter.TempSelection = ko.pureComputed(function () { return model.Filter.Statuses.filter(function (item) { return item.isChecked(); }); }, this);
	model.Filter.SelectedItemsDisplay = ko.pureComputed(function () { return model.Filter.SelectedItems().map(function (item) { return item.Name; }).join(", ") || "Все"; }, this);

	model.Filter.OptionsShown = ko.observable(false);
	model.Filter.OptionsShown.subscribe(function () { model.Filter.UpdateSelections(); }, this);

	model.Filter.ToggleOptions = function ()
	{
		model.Filter.OptionsShown(!model.Filter.OptionsShown());
	};

	model.Filter.CloseOptions = function ()
	{
		model.Filter.OptionsShown(false);
	}

	model.Filter.ConfirmSelection = function ()
	{
		model.Filter.SelectedItems(model.Filter.TempSelection());
		model.Filter.CloseOptions();
		model.ApplyFilter();
	};

	model.Filter.UpdateSelections = function ()
	{
		var selection = model.Filter.SelectedItems();
		model.Filter.Statuses.forEach(function (item) { item.isChecked(~selection.indexOf(item)); });
	}

	model.Filter.ResetSelection = function ()
	{
		model.Filter.Statuses.forEach(function (item) { item.isChecked(false) });
	};

	// вычисляемое/подписки ///////////////////////////////////////////////////////////////////////////////////////////////////

	model.TotalPageCount = ko.computed(function () { return Math.ceil(model.TotalItemsCount() / model.Filter.PageSize()); });

	model.Filter.Context.subscribe(function () { model.QueryFilter() });
	model.Filter.Type.subscribe(function () { model.ApplyFilter() });


	// функции ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	model.ContractorDetailsUrl = function (id) { return model.Options.ContractorDetailsUrl + "/" + id; };
	model.ContractDetailsUrl = function (id) { return model.Options.ContractDetailsUrl + "/" + id; };
	model.UserDetailsUrl = function (id) { return model.Options.UserDetailsUrl + "/" + id; };
	model.OrderDetailsUrl = function (id) { return model.Options.OrderDetailsUrl + "/" + id; };
	model.LegalDetailsUrl = function (id) { return model.Options.LegalDetailsUrl + "/" + id; };

	model.GotoOrderDetails = function (data) { window.location = model.Options.OrderDetailsUrl + "/" + data.ID; };

	model.QueryFilter = function ()
	{
		if (model.DelayHandler)
			clearTimeout(model.DelayHandler);

		model.DelayHandler = setTimeout(function () { model.ApplyFilter() }, 1000);
	};

	model.SortBy = function (field)
	{
		var f = ko.unwrap(field);
		if (model.Filter.Sort() == f)
			model.Filter.SortDirection(model.Filter.SortDirection() == "Asc" ? "Desc" : "Asc");
		else
			model.Filter.Sort(f);

		model.ApplyFilter();
	};

	model.ApplyFilter = function ()
	{
		model.DelayHandler = null;
		$.ajax({
			type: "POST",
			url: model.Options.GetItemsUrl,
			data: {
				Context: model.Filter.Context() || "",
				Type: model.Filter.Type() || "",
				UserId: model.Filter.UserId() || "",
				PageSize: model.Filter.PageSize(),
				PageNumber: model.Filter.PageNumber(),
				Sort: model.Filter.Sort(),
				SortDirection: model.Filter.SortDirection(),
				Statuses: model.Filter.SelectedItems().map(function (item) { return item.ID })
			},
			success: function (response)
			{
				var r = JSON.parse(response);
				model.Items(r.Items);
				model.TotalItemsCount(r.TotalCount);
				if ((model.Filter.PageNumber() > 0) && (model.Filter.PageNumber() >= model.TotalPageCount()))
				{
					model.Filter.PageNumber(0);
					model.ApplyFilter();
				}
			}
		});
	};

	model.FirstPage = function ()
	{
		if (model.Filter.PageNumber() > 0)
		{
			model.Filter.PageNumber(0);
			model.ApplyFilter();
		}
	};

	model.PrevPage = function ()
	{
		if (model.Filter.PageNumber() > 0)
		{
			model.Filter.PageNumber(model.Filter.PageNumber() - 1);
			model.ApplyFilter();
		}
	};

	model.NextPage = function ()
	{
		if (model.Filter.PageNumber() < (model.TotalPageCount() - 1))
		{
			model.Filter.PageNumber(model.Filter.PageNumber() + 1);
			model.ApplyFilter();
		}
	};

	model.LastPage = function ()
	{
		if (model.Filter.PageNumber() < (model.TotalPageCount() - 1))
		{
			model.Filter.PageNumber(model.TotalPageCount() - 1);
			model.ApplyFilter();
		}
	};

	model.GetContract = function (contractId)
	{
		var id = ko.unwrap(contractId);
		var result;
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.GetContractUrl,
			data: { Id: id },
			success: function (response) { result = ko.mapping.fromJSON(response); }
		});

		return result;
	};

	model.LoadContractCurrencies = function (contractId, observableArray)
	{
		var id = ko.unwrap(contractId);
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.GetContractCurrenciesUrl,
			data: { ContractId: id },
			success: function (response) { observableArray(ko.mapping.fromJSON(response).Items()); }
		});
	};

	// #region contract info modal ////////////////////////////////////////////////////////////////////////////////////////////

	var contractModalSelector = "#contractInfoModal";

	model.OpenContractInfo = function (contractId)
	{
		var contract = model.GetContract(contractId);
		contract.Currencies = contract.Currencies || ko.observableArray();
		model.ContractInfoModal.CurrentItem(contract);
		$(contractModalSelector).modal("show");
		//$(contractModalSelector).draggable({ handle: ".modal-header" });
		model.ContractInfoModal.Init();
	};

	model.ContractInfoModal = {
		CurrentItem: ko.observable(),
		OurBankAccounts: ko.observableArray(),
		OurBankAccountsItems: ko.pureComputed(function () { return ko.utils.arrayFilter(model.ContractInfoModal.OurBankAccounts(), function (item) { return item.CurrencyId() == model.ContractInfoModal.CurrentItem().CurrencyId() }); }),
		BankAccounts: ko.observableArray(),
		Documents: ko.observableArray(),
		Init: function ()
		{
			var self = model.ContractInfoModal;
			model.LoadContractCurrencies(self.CurrentItem().ID(), self.CurrentItem().Currencies);

			// получить счета по нашему юрлицу
			$.ajax({
				type: "POST",
				url: model.Options.GetBankAccountsByLegalItemsUrl,
				data: { LegalId: self.CurrentItem().OurLegalId() },
				success: function (response) { self.OurBankAccounts(ko.mapping.fromJSON(response).Items()); }
			});

			// получить счета по юрлицу
			$.ajax({
				type: "POST",
				url: model.Options.GetBankAccountsByLegalItemsUrl,
				data: { LegalId: self.CurrentItem().LegalId() },
				success: function (response) { self.BankAccounts(ko.mapping.fromJSON(response).Items()); }
			});

			// получить документы
			$.ajax({
				type: "POST",
				url: model.Options.GetDocumentsByContractItemsUrl,
				data: { ContractId: model.ContractInfoModal.CurrentItem().ID() },
				success: function (response) { model.ContractInfoModal.Documents(ko.mapping.fromJSON(response).Items()); }
			});

			self.CurrentItem().OurLegalId.subscribe(function (newValue)
			{
				// получить 
				$.ajax({
					type: "POST",
					url: model.Options.GetBankAccountsByLegalItemsUrl,
					data: { LegalId: newValue },
					success: function (response) { model.ContractEditModal.OurBankAccounts(ko.mapping.fromJSON(response).Items()); }
				});
			});
		},
		GotoContractor: function (data, e)
		{
			window.open(model.LegalDetailsUrl(data.CurrentItem().LegalId()), "_blank");
		},
		GotoContract: function (data, e)
		{
			window.open(model.ContractDetailsUrl(data.CurrentItem().ID()), "_blank");
		},
		Done: function (data, e)
		{
			$(contractModalSelector).modal("hide");
		}
	};

	// #endregion

	model.GetContractCurrenciesDisplay = function (currencies)
	{
		var result = "";
		if (currencies)
			ko.utils.arrayForEach(currencies(), function (item) { result += app.utility.GetDisplay(model.Dictionaries.Currency, item.CurrencyId()) + " " });

		return result;
	};

	// инициализация //////////////////////////////////////////////////////////////////////////////////////////////////////////

	model.Filter.SortDirection(model.Filter.SortDirection() || "Asc");
	model.ApplyFilter();
}

$(function ()
{
	ko.applyBindings(new OrdersViewModel(modelData, {
		GetItemsUrl: app.urls.GetItems,
		ContractorDetailsUrl: app.urls.ContractorDetails,
		ContractDetailsUrl: app.urls.ContractDetails,
		LegalDetailsUrl: app.urls.LegalDetails,
		UserDetailsUrl: app.urls.UserDetails,
		OrderDetailsUrl: app.urls.OrderDetails,
		GetBankAccountsByLegalItemsUrl: app.urls.GetBankAccountsByLegalItems,
		GetContractCurrenciesUrl: app.urls.GetContractCurrencies,
		GetDocumentsByContractItemsUrl: app.urls.GetDocumentsByContractItems,
		GetContractUrl: app.urls.GetContract
	}), document.getElementById("ko-root"));
});