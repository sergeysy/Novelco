var SearchViewModel = function (source, dictionaries, options)
{
	var model = this;

	model.Options = $.extend({
		GetContractorsUrl: null,
		GetRequestsUrl: null,
		GetLegalsUrl: null,
		GetOrdersUrl: null,
		GetDocumentsUrl: null,
		GetContractsUrl: null,
		OrderDetailsUrl: null,
		RequestDetailsUrl: null,
		ContractDetailsUrl: null,
		AccountingDetailsUrl: null,
		ContractorDetailsUrl: null,
		ViewPaymentUrl: null,
		ViewDocumentUrl: null,
		ViewTemplatedDocumentUrl: null,
	}, options);

	// поля ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	model.Number = source;
	// Список записей
	model.OrdersItems = ko.observableArray();
	model.LegalsItems = ko.observableArray();
	model.RequestsItems = ko.observableArray();
	model.PaymentsItems = ko.observableArray();
	model.DocumentsItems = ko.observableArray();
	model.ContractsItems = ko.observableArray();
	model.ContractorsItems = ko.observableArray();
	// Справочники
	model.Dictionaries = ko.mapping.fromJS(dictionaries);

	// вычисляемое/подписки ///////////////////////////////////////////////////////////////////////////////////////////////////

	// функции ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	model.GotoRequest = function (data) { window.open(model.Options.RequestDetailsUrl + "/" + data.ID(), "_blank") };

	model.GotoOrder = function (data)
	{
		if (data.OrderId && data.OrderId())
			window.open(model.Options.OrderDetailsUrl + "/" + data.OrderId(), "_blank")
		else
			window.open(model.Options.OrderDetailsUrl + "/" + data.ID(), "_blank")
	};

	model.GotoAccounting = function (data)
	{
		if (data.AccountingId && data.AccountingId())
			window.open(model.Options.AccountingDetailsUrl + "/" + data.AccountingId(), "_blank")
		else
			window.open(model.Options.AccountingDetailsUrl + "/" + data.ID(), "_blank")
	};

	model.GotoContract = function (data) { window.open(model.Options.ContractDetailsUrl + "/" + data.ID(), "_blank") };

	model.GotoContractor = function (data) { window.open(model.Options.ContractorDetailsUrl + "/" + data.ID(), "_blank") };

	model.GotoLegal = function (data) { window.open(model.Options.ContractorDetailsUrl + "/" + data.ContractorId(), "_blank") };

	model.ViewJointDocument = function (data)
	{
		if (data.IsDocument())
			model.ViewDocument(data);
		else
			model.ViewTemplatedDocument(data);
	};

	model.ViewTemplatedDocument = function (data)
	{
		var id = ko.unwrap(data.ID());
		window.open(model.Options.ViewTemplatedDocumentUrl + "/" + id, "_blank");
	};

	model.ViewDocument = function (data)
	{
		var id = ko.unwrap(data.ID());
		window.open(model.Options.ViewDocumentUrl + "/" + id, "_blank");
	};

	model.ViewPayment = function (data)
	{
		var id = ko.unwrap(data.ID());
		window.open(model.Options.ViewPaymentUrl + "/" + id, "_blank");
	};

	//#region gets

	model.GetContractors = function (number)
	{
		$.ajax({
			type: "POST",
			url: model.Options.GetContractorsUrl,
			data: { Number: number },
			success: function (response)
			{
				model.ContractorsItems(ko.mapping.fromJSON(response).Items());
				$("#contractors").toggle(model.ContractorsItems().length ? true : false);
			}
		});
	};

	model.GetLegals = function (number)
	{
		$.ajax({
			type: "POST",
			url: model.Options.GetLegalsUrl,
			data: { Number: number },
			success: function (response)
			{
				model.LegalsItems(ko.mapping.fromJSON(response).Items());
				$("#legals").toggle(model.LegalsItems().length ? true : false);
			}
		});
	};

	model.GetOrders = function (number)
	{
		$.ajax({
			type: "POST",
			url: model.Options.GetOrdersUrl,
			data: { Number: number },
			success: function (response)
			{
				model.OrdersItems(ko.mapping.fromJSON(response).Items());
				$("#orders").toggle(model.OrdersItems().length ? true : false);
			}
		});
	};

	model.GetRequests = function (number)
	{
		$.ajax({
			type: "POST",
			url: model.Options.GetRequestsUrl,
			data: { Number: number },
			success: function (response)
			{
				model.RequestsItems(ko.mapping.fromJSON(response).Items());
				$("#requests").toggle(model.RequestsItems().length ? true : false);
			}
		});
	};

	model.GetDocuments = function (number)
	{
		$.ajax({
			type: "POST",
			url: model.Options.GetDocumentsUrl,
			data: { Number: number },
			success: function (response)
			{
				model.DocumentsItems(ko.mapping.fromJSON(response).Items());
				$("#documents").toggle(model.DocumentsItems().length ? true : false);
			}
		});
	};

	model.GetContracts = function (number)
	{
		$.ajax({
			type: "POST",
			url: model.Options.GetContractsUrl,
			data: { Number: number },
			success: function (response)
			{
				model.ContractsItems(ko.mapping.fromJSON(response).Items());
				$("#contracts").toggle(model.ContractsItems().length ? true : false);
			}
		});
	};

	model.GetPayments = function (number)
	{
		$.ajax({
			type: "POST",
			url: model.Options.GetPaymentsUrl,
			data: { Number: number },
			success: function (response)
			{
				model.PaymentsItems(ko.mapping.fromJSON(response).Items());
				$("#payments").toggle(model.PaymentsItems().length ? true : false);
			}
		});
	};

	//#endregion

	// инициализация //////////////////////////////////////////////////////////////////////////////////////////////////////////

	setTimeout(function ()
	{
		model.GetDocuments(model.Number);
		model.GetContractors(model.Number);
		model.GetLegals(model.Number);
		model.GetOrders(model.Number);
		model.GetContracts(model.Number);
		model.GetRequests(model.Number);
		model.GetPayments(model.Number);
	}, 100);
}

$(function ()
{
	ko.applyBindings(new SearchViewModel(modelData, modelDictionaries, {
		GetContractorsUrl: app.urls.GetContractors,
		GetRequestsUrl: app.urls.GetRequests,
		GetLegalsUrl: app.urls.GetLegals,
		GetOrdersUrl: app.urls.GetOrders,
		GetPaymentsUrl: app.urls.GetPayments,
		GetDocumentsUrl: app.urls.GetDocuments,
		GetContractsUrl: app.urls.GetContracts,
		OrderDetailsUrl: app.urls.OrderDetails,
		RequestDetailsUrl: app.urls.RequestDetails,
		ContractDetailsUrl: app.urls.ContractDetails,
		AccountingDetailsUrl: app.urls.AccountingDetails,
		ContractorDetailsUrl: app.urls.ContractorDetails,
		ViewPaymentUrl: app.urls.ViewPayment,
		ViewDocumentUrl: app.urls.ViewDocument,
		ViewTemplatedDocumentUrl: app.urls.ViewTemplatedDocument
	}), document.getElementById("ko-root"));
});