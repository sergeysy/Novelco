var OrderTemplatesViewModel = function (source, contractId, options) {
	var model = this;

	model.Options = $.extend({
		SaveOrderTemplateOperationUrl: null,
		GetOrderTemplateOperationsUrl: null,
		GetNewOrderTemplateUrl: null,
		GetOperationsItemsUrl: null,
		SaveOrderTemplateUrl: null
	}, options);

	// поля ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	// Список записей
	model.Items = ko.mapping.fromJS(source);
	model.OperationsItems = ko.observableArray();
	model.TemplateOperationsItems = ko.observableArray();
	//
	model.SelectedTemplate = ko.observable();
	model.ContractId = ko.observable(contractId);

	// вычисляемое/подписки ///////////////////////////////////////////////////////////////////////////////////////////////////


	// функции ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	model.SelectTemplate = function (template) {
		model.SelectedTemplate(template);
		model.GetTemplateOperations(template.ID);
	};

	model.SaveOrderTemplate = function (data) {
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.SaveOrderTemplateUrl,
			data: {
				ID: data.ID(),
				Name: data.Name(),
				ContractId: data.ContractId()
			},
			success: function (response) {
				var r = ko.mapping.fromJSON(response);
				if (r.ID)
					data.ID(r.ID());
			}
		});
	};

	model.SaveOrderTemplateOperation = function (data) {
		$.ajax({
			type: "POST",
			async: false,
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

	model.GetOperations = function () {
		$.ajax({
			type: "POST",
			url: model.Options.GetOperationsItemsUrl,
			success: function (response) { model.OperationsItems(ko.mapping.fromJSON(response).Items()); }
		});
	};

	// #region order Template create/edit modal ///////////////////////////////////////////////////////////////////////////////

	var orderTemplateModalSelector = "#orderTemplateEditModal";

	model.OpenOrderTemplateCreate = function () {
		var data = null;
		$.ajax({
			type: "POST",
			async: false,
			data: { ContractId: model.ContractId() },
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

	model.OpenOrderTemplateEdit = function (Template) {
		model.OrderTemplateEditModal.CurrentItem(Template);
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
			No: ko.observable(0)
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

	model.GetOperations();
}

$(function () {
	ko.applyBindings(new OrderTemplatesViewModel(modelData, contractId, {
		GetOrderTemplateOperationsUrl: app.urls.GetOrderTemplateOperations,
		SaveOrderTemplateOperationUrl: app.urls.SaveOrderTemplateOperation,
		GetNewOrderTemplateUrl: app.urls.GetNewOrderTemplate,
		GetOperationsItemsUrl: app.urls.GetOperationsItems,
		SaveOrderTemplateUrl: app.urls.SaveOrderTemplate
	}), document.getElementById("ko-root"));
});