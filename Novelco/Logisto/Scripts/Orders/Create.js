var CreateOrderViewModel = function (source, options)
{
	var model = this;

	model.Options = $.extend({
		GetOrdersItemsUrl: null,
		GetContractsByLegalItemsUrl: null,
		GetLegalsByContractorItemsUrl: null,
		GetOrderTemplatesByProductUrl: null,
		GetOrderTemplatesByContractUrl: null,
		GetContractsByContractorItemsUrl: null,

		OrderDetailsUrl: null,
		SaveOrderUrl: null
	}, options);

	// поля ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	// Списки связанных сущностей
	model.ContractsItems = ko.observableArray();
	model.FinRepCentersItems = ko.observableArray();
	model.OrderTemplatesItems = ko.observableArray();
	// Справочники
	model.Dictionaries = ko.mapping.fromJS(source.Dictionaries);
	// Выбранное 
	model.SelectedProduct = ko.observable();
	model.SelectedContract = ko.observable();
	model.SelectedContractor = ko.observable();
	model.SelectedFinRepCenter = ko.observable();
	model.SelectedOrderTemplate = ko.observable();
	// Есть несохраненные изменения
	model.IsDirty = ko.observable(false);

	// вычисляемое/подписки ///////////////////////////////////////////////////////////////////////////////////////////////////

	model.SelectedContractor.subscribe(function (newValue)
	{
		model.ContractsItems(model.GetContractsByContractor(newValue));
		if (model.ContractsItems().length == 1)
			model.SelectedContract(model.ContractsItems()[0].ID());
	});

	model.SelectedContract.subscribe(function (newValue)
	{
		if (newValue)
		{
			var contract = ko.utils.arrayFirst(model.ContractsItems(), function (item) { return item.ID() == newValue });
			var list = ko.utils.arrayFilter(model.Dictionaries.FinRepCenter(), function (item) { return item.OurLegalId() == contract.OurLegalId() });
			model.FinRepCentersItems(list);
			if (model.FinRepCentersItems().length == 1)
				model.SelectedFinRepCenter(model.FinRepCentersItems()[0].ID());
		}
		else
			model.FinRepCentersItems([]);
	});

	model.SelectedProduct.subscribe(function (newValue)
	{
		var list = ko.utils.arrayFilter(model.Dictionaries.OrderTemplate(), function (item) { return item.ProductId() == newValue });
		model.OrderTemplatesItems(list);
		if (model.OrderTemplatesItems().length == 1)
			model.SelectedOrderTemplate(model.OrderTemplatesItems()[0].ID());
	});

	// функции ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	model.IsDanger = function (contractId)
	{
		var id = ko.unwrap(contractId);
		var contract = ko.utils.arrayFirst(model.ContractsItems(), function (item) { return item.ID() == id });
		if (contract && !contract.IsActive())
			return true;

		return false;
	};

	model.ContractOptionsAfterRender = function (option, data)
	{
		if (data && !data.IsActive())
			option.className = "text-danger";
		else
			option.className = "text-muted";
	};

	model.GetContractsByContractor = function (contractorId, isOnlyClient)
	{
		var id = ko.unwrap(contractorId);
		if (id)
		{
			var list;
			$.ajax({
				type: "POST",
				async: false,
				url: model.Options.GetContractsByContractorItemsUrl,
				data: { ContractorId: id, IsOnlyClient: isOnlyClient },
				success: function (response) { list = ko.mapping.fromJSON(response).Items(); }
			});

			return list;
		}
	};

	model.Validate = function ()
	{
		if (!model.SelectedContractor())
		{
			alert("Выберите контрагента");
			return false;
		}

		if (!model.SelectedContract())
		{
			alert("Выберите договор");
			return false;
		}

		if (!model.SelectedProduct())
		{
			alert("Выберите продукт");
			return false;
		}

		if (!model.SelectedFinRepCenter())
		{
			alert("Выберите ЦФО");
			return false;
		}

		if (!model.SelectedOrderTemplate())
		{
			alert("Выберите шаблон");
			return false;
		}

		return true;
	};

	model.Save = function ()
	{
		if (!model.Validate())
			return;

		$.ajax({
			type: "POST",
			url: model.Options.SaveOrderUrl,
			data: {
				ProductId: model.SelectedProduct(),
				ContractId: model.SelectedContract(),
				FinRepCenterId: model.SelectedFinRepCenter(),
				OrderTemplateId: model.SelectedOrderTemplate()
			},
			success: function (response)
			{
				var r = JSON.parse(response);
				if (r.Message)
					alert(r.Message);
				else
					window.location = model.Options.OrderDetailsUrl + "/" + r.ID;
			}
		});
	};


	// инициализация //////////////////////////////////////////////////////////////////////////////////////////////////////////

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
	};
}

$(function ()
{
	ko.applyBindings(new CreateOrderViewModel(modelData, {
		GetOrdersItemsUrl: app.urls.GetOrdersItems,
		GetContactsByLegalItemsUrl: app.urls.GetContactsByLegalItems,
		GetLegalsByContractorItemsUrl: app.urls.GetLegalsByContractorItems,
		GetOrderTemplatesByProductUrl: app.urls.GetOrderTemplatesByProduct,
		GetOrderTemplatesByContractUrl: app.urls.GetOrderTemplatesByContract,
		GetContractsByContractorItemsUrl: app.urls.GetContractsByContractorItems,

		OrderDetailsUrl: app.urls.OrderDetails,
		SaveOrderUrl: app.urls.SaveOrder
	}), document.getElementById("ko-root"));
});