var OrderTemplatesViewModel = function (source, dictionaries, options) {
	var model = this;

	model.Options = $.extend({
		DeleteOrderTemplateOperationUrl: null,
		SaveOrderTemplateOperationUrl: null,
		GetOrderTemplateOperationsUrl: null,
		GetNewOrderTemplateUrl: null,
		GetOperationsItemsUrl: null,
		DeleteOrderTemplateUrl: null,
		SaveOrderTemplateUrl: null,
		GetProductsItemsUrl: null
	}, options);

	// поля ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	// Список записей
	model.Items = ko.mapping.fromJS(source);
	model.ProductsItems = ko.observableArray();
	model.OperationsItems = ko.observableArray();
	model.TemplateOperationsItems = ko.observableArray();
	//
	model.SelectedTemplate = ko.observable();
	// Справочники
	model.Dictionaries = ko.mapping.fromJS(dictionaries);

	// вычисляемое/подписки ///////////////////////////////////////////////////////////////////////////////////////////////////


	// функции ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	model.SelectTemplate = function (template) {
		model.SelectedTemplate(template);
		model.GetTemplateOperations(template.ID);
	};

	model.SaveOrderTemplate = function (data) {
		$.ajax({
			type: "POST",
			url: model.Options.SaveOrderTemplateUrl,
			data: {
				ID: data.ID(),
				Name: data.Name(),
				ProductId: data.ProductId()
			}
		});
	};

	model.DeleteOrderTemplate = function (data) {
		if (!confirm("Операции уже добавленные в существующие заказы НЕ будут удалены."))
			return;

		$.ajax({
			type: "POST",
			url: model.Options.DeleteOrderTemplateUrl,
			data: { ID: data.ID() },
			success: function (response) { model.Items.remove(data); }
		});
	};

	model.DeleteOrderTemplateOperation = function (data) {
		if (!confirm("Операции уже добавленные в существующие заказы НЕ будут удалены."))
			return;

		$.ajax({
			type: "POST",
			url: model.Options.DeleteOrderTemplateOperationUrl,
			data: {
				OrderTemplateId: data.OrderTemplateId(),
				OrderOperationId: data.OrderOperationId(),
			},
			success: function (response) { model.TemplateOperationsItems.remove(data); }
		});
	};

	model.SaveOrderTemplateOperation = function (data) {
		$.ajax({
			type: "POST",
			url: model.Options.SaveOrderTemplateOperationUrl,
			data: {
				OrderTemplateId: data.OrderTemplateId(),
				OrderOperationId: data.OrderOperationId(),
				No: data.No()
			}
		});
	};

	model.GetTemplateOperations = function (templateId) {
		var id = ko.unwrap(templateId);
		$.ajax({
			type: "POST",
			data: { OrderTemplateId: id },
			url: model.Options.GetOrderTemplateOperationsUrl,
			success: function (response) { model.TemplateOperationsItems(ko.mapping.fromJSON(response).Items()); }
		});
	};

	model.GetProducts = function () {
		$.ajax({
			type: "POST",
			url: model.Options.GetProductsItemsUrl,
			success: function (response) { model.ProductsItems(ko.mapping.fromJSON(response).Items()); }
		});
	};

	model.GetOperations = function () {
		$.ajax({
			type: "POST",
			url: model.Options.GetOperationsItemsUrl,
			success: function (response) { model.OperationsItems(ko.mapping.fromJSON(response).Items()); }
		});
	};

	model.GetOperationKind = function (operationId) {
		var id = ko.unwrap(operationId);
		var kind = ko.utils.arrayFirst(model.OperationsItems(), function (item) { return item.ID() == id });
		if (kind)
			return app.utility.GetDisplay(model.Dictionaries.OperationKind, kind.OperationKindId());

		return "";
	};

	// #region order Template create/edit modal ///////////////////////////////////////////////////////////////////////////////

	var orderTemplateModalSelector = "#orderTemplateEditModal";

	model.OpenOrderTemplateCreate = function () {
		var data = null;
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.GetNewOrderTemplateUrl,
			success: function (response) {
				data = ko.mapping.fromJSON(response);
				model.Items.push(data);
			}
		});

		model.OrderTemplateEditModal.CurrentItem(data);
		$(orderTemplateModalSelector).modal("show");
		//$(orderTemplateModalSelector).draggable({ handle: ".modal-header" });
		model.OrderTemplateEditModal.OnClosed = function () { model.Items.remove(data); };
		model.OrderTemplateEditModal.Init();
	};

	model.OpenOrderTemplateEdit = function (template) {
		model.OrderTemplateEditModal.CurrentItem(template);
		$(orderTemplateModalSelector).modal("show");
		//$(orderTemplateModalSelector).draggable({ handle: ".modal-header" });;
		model.OrderTemplateEditModal.Init();
	};

	model.OrderTemplateEditModal = {
		CurrentItem: ko.observable(),
		Init: function () { },
		OnClosed: null,
		Close: function (self, e) {
			$(orderTemplateModalSelector).modal("hide");
			if (self.OnClosed != null)
				self.OnClosed();
		},
		Done: function (self, e) {
			// сохранить изменения
			model.SaveOrderTemplate(self.CurrentItem());

			$(orderTemplateModalSelector).modal("hide");
		}
	};

	// #endregion

	// #region order Template Operation create/edit modal /////////////////////////////////////////////////////////////////////

	var orderTemplateOperationModalSelector = "#orderTemplateOperationEditModal";

	model.OpenOrderTemplateOperationCreate = function () {
		var data = {
			OrderTemplateId: ko.observable(model.SelectedTemplate().ID()),
			OrderOperationId: ko.observable(),
			No: ko.observable(model.TemplateOperationsItems().length)
		};

		model.TemplateOperationsItems.push(data);
		model.OrderTemplateOperationEditModal.CurrentItem(data);
		$(orderTemplateOperationModalSelector).modal("show");
		//$(orderTemplateOperationModalSelector).draggable({ handle: ".modal-header" });
		model.OrderTemplateOperationEditModal.OnClosed = function () { model.TemplateOperationsItems.remove(data); };
		model.OrderTemplateOperationEditModal.Init();
	};

	model.OpenOrderTemplateOperationEdit = function (TemplateOperation) {
		model.OrderTemplateOperationEditModal.CurrentItem(TemplateOperation);
		$(orderTemplateOperationModalSelector).modal("show");
		//$(orderTemplateOperationModalSelector).draggable({ handle: ".modal-header" });;
		model.OrderTemplateOperationEditModal.Init();
	};

	model.OrderTemplateOperationEditModal = {
		CurrentItem: ko.observable(),
		Init: function () { },
		OnClosed: null,
		Close: function (self, e) {
			$(orderTemplateOperationModalSelector).modal("hide");
			if (self.OnClosed != null)
				self.OnClosed();
		},
		Done: function (self, e) {
			// сохранить изменения
			model.SaveOrderTemplateOperation(self.CurrentItem());

			$(orderTemplateOperationModalSelector).modal("hide");
		}
	};

	// #endregion

	// инициализация //////////////////////////////////////////////////////////////////////////////////////////////////////////

	model.GetProducts();
	model.GetOperations();
}

$(function () {
	ko.applyBindings(new OrderTemplatesViewModel(modelData, modelDictionaries, {
		DeleteOrderTemplateOperationUrl: app.urls.DeleteOrderTemplateOperation,
		SaveOrderTemplateOperationUrl: app.urls.SaveOrderTemplateOperation,
		GetOrderTemplateOperationsUrl: app.urls.GetOrderTemplateOperations,
		GetNewOrderTemplateUrl: app.urls.GetNewOrderTemplate,
		GetOperationsItemsUrl: app.urls.GetOperationsItems,
		DeleteOrderTemplateUrl: app.urls.DeleteOrderTemplate,
		SaveOrderTemplateUrl: app.urls.SaveOrderTemplate,
		GetProductsItemsUrl: app.urls.GetProductsItems
	}), document.getElementById("ko-root"));
});