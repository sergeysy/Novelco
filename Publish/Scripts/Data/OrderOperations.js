var OrderOperationsViewModel = function (source, dictionaries, options) {
	var model = this;

	model.Options = $.extend({
		GetNewOrderOperationUrl: null,
		SaveOrderOperationUrl: null
	}, options);

	// поля ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	// Список записей
	model.Items = ko.mapping.fromJS(source);
	// Справочники
	model.Dictionaries = ko.mapping.fromJS(dictionaries);

	// вычисляемое/подписки ///////////////////////////////////////////////////////////////////////////////////////////////////


	// инициализация //////////////////////////////////////////////////////////////////////////////////////////////////////////


	// функции ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	model.SaveOrderOperation = function (data) {
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.SaveOrderOperationUrl,
			data: {
				ID: data.ID(),
				Name: data.Name(),
				EnName: data.EnName(),
				OperationKindId: data.OperationKindId(),
				StartFactEventId: data.StartFactEventId(),
				FinishFactEventId: data.FinishFactEventId()
			}
		});
	};

	// #region operation create/edit modal ////////////////////////////////////////////////////////////////////////////////////

	var orderOperationModalSelector = "#orderOperationEditModal";

	model.OpenOrderOperationCreate = function () {
		var data = null;
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.GetNewOrderOperationUrl,
			success: function (response) {
				data = ko.mapping.fromJSON(response);
				model.Items.push(data);
			}
		});

		model.OrderOperationEditModal.CurrentItem(data);
		$(orderOperationModalSelector).modal("show");
		//$(orderOperationModalSelector).draggable({ handle: ".modal-header" });
		model.OrderOperationEditModal.OnClosed = function () { model.Items.remove(data); };
		model.OrderOperationEditModal.Init();
	};

	model.OpenOrderOperationEdit = function (operation) {
		model.OrderOperationEditModal.CurrentItem(operation);
		$(orderOperationModalSelector).modal("show");
		//$(orderOperationModalSelector).draggable({ handle: ".modal-header" });;
		model.OrderOperationEditModal.Init();
	};

	model.OrderOperationEditModal = {
		CurrentItem: ko.observable(),
		Init: function () { },
		OnClosed: null,
		Close: function (self, e) {
			$(orderOperationModalSelector).modal("hide");
			if (self.OnClosed != null)
				self.OnClosed();
		},
		Done: function (self, e) {
			// сохранить изменения
			model.SaveOrderOperation(self.CurrentItem());

			$(orderOperationModalSelector).modal("hide");
		}
	};

	// #endregion

}

$(function () {
	ko.applyBindings(new OrderOperationsViewModel(modelData, modelDictionaries, {
		GetNewOrderOperationUrl: app.urls.GetNewOrderOperation,
		SaveOrderOperationUrl: app.urls.SaveOrderOperation
	}), document.getElementById("ko-root"));
});