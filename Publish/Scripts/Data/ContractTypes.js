var ContractTypesViewModel = function (source, dictionaries, options) {
	var model = this;

	model.Options = $.extend({
		GetNewContractTypeUrl: null,
		SaveContractTypeUrl: null
	}, options);

	// поля ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	// Список записей
	model.Items = ko.mapping.fromJS(source);
	// Справочники
	model.Dictionaries = ko.mapping.fromJS(dictionaries);

	// вычисляемое/подписки ///////////////////////////////////////////////////////////////////////////////////////////////////


	// инициализация //////////////////////////////////////////////////////////////////////////////////////////////////////////


	// функции ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	model.SaveContractType = function (data) {
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.SaveContractTypeUrl,
			data: {
				ID: data.ID(),
				Display: data.Display(),
				OurContractRoleId: data.OurContractRoleId(),
				ContractRoleId: data.ContractRoleId()
			},
			success: function (response) {
				var id = JSON.parse(response).ID;
				if (id)
					data.ID(id);
			}
		});
	};

	// #region create/edit modal //////////////////////////////////////////////////////////////////////////////////////////////

	var contractTypeModalSelector = "#contractTypeEditModal";

	model.OpenContractTypeCreate = function () {
		var data = null;
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.GetNewContractTypeUrl,
			success: function (response) {
				data = ko.mapping.fromJSON(response);
				model.Items.push(data);
			}
		});

		model.ContractTypeEditModal.CurrentItem(data);
		$(contractTypeModalSelector).modal("show");
		//$(contractTypeModalSelector).draggable({ handle: ".modal-header" });
		model.ContractTypeEditModal.OnClosed = function () { model.Items.remove(data); };
		model.ContractTypeEditModal.Init();
	};

	model.OpenContractTypeEdit = function (data) {
		model.ContractTypeEditModal.CurrentItem(data);
		$(contractTypeModalSelector).modal("show");
		//$(contractTypeModalSelector).draggable({ handle: ".modal-header" });;
		model.ContractTypeEditModal.Init();
	};

	model.ContractTypeEditModal = {
		CurrentItem: ko.observable(),
		Init: function () { },
		Done: function (self, e) {
			// сохранить изменения
			model.SaveContractType(self.CurrentItem());

			//model.IsDirty(true);
			$(contractTypeModalSelector).modal("hide");
		}
	};

	// #endregion
}

$(function () {
	ko.applyBindings(new ContractTypesViewModel(modelData, modelDictionaries, {
		GetNewContractTypeUrl: app.urls.GetNewContractType,
		SaveContractTypeUrl: app.urls.SaveContractType
	}), document.getElementById("ko-root"));
});