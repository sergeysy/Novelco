var CargoDescriptionsViewModel = function (source, options) {
	var model = this;

	model.Options = $.extend({
		GetNewCargoDescriptionUrl: null,
		SaveCargoDescriptionUrl: null
	}, options);

	// поля ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	// Список записей
	model.Items = ko.mapping.fromJS(source);

	// вычисляемое/подписки ///////////////////////////////////////////////////////////////////////////////////////////////////


	// инициализация //////////////////////////////////////////////////////////////////////////////////////////////////////////


	// функции ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	model.SaveCargoDescription = function (cargoDescription) {
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.SaveCargoDescriptionUrl,
			data: {
				ID: cargoDescription.ID(),
				Display: cargoDescription.Display(),
				EnDisplay: cargoDescription.EnDisplay()

				//IsDeleted: service.IsDeleted()
			},
			success: function (response) {
				var id = JSON.parse(response).ID;
				if (id)
					cargoDescription.ID(id);
			}
		});
	};

	// #region template create/edit modal /////////////////////////////////////////////////////////////////////////////////////

	var cargoDescriptionModalSelector = "#cargoDescriptionEditModal";

	model.DeleteCargoDescription = function (service) {
		service.IsDeleted(true);
		model.IsDirty(true);
	};

	model.OpenCargoDescriptionCreate = function () {
		var data = null;
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.GetNewCargoDescriptionUrl,
			success: function (response) {
				data = ko.mapping.fromJSON(response);
				//model.ExtendTemplate(data, model.SelectedTemplate());
				model.Items.push(data);
			}
		});

		model.CargoDescriptionEditModal.CurrentItem(data);
		$(cargoDescriptionModalSelector).modal("show");		
		$(cargoDescriptionModalSelector).draggable({ handle: ".modal-header" });
		model.CargoDescriptionEditModal.OnClosed = function () { model.Items.remove(data); };
		model.CargoDescriptionEditModal.Init();
	};

	model.OpenCargoDescriptionEdit = function (service) {
		model.CargoDescriptionEditModal.CurrentItem(service);
		$(cargoDescriptionModalSelector).modal("show");
		$(cargoDescriptionModalSelector).draggable({ handle: ".modal-header" });;
		model.CargoDescriptionEditModal.Init();
	};

	model.CargoDescriptionEditModal = {
		CurrentItem: ko.observable(),
		IsDone: false,
		Init: function () {
			model.CargoDescriptionEditModal.IsDone = false;
		},
		OnClosed: null,
		Close: function (self, e) {
			$(cargoDescriptionModalSelector).modal("hide");
			if (self.OnClosed != null)
				self.OnClosed();
		},
		Done: function (data, e) {
			// сохранить изменения
			model.SaveCargoDescription(model.CargoDescriptionEditModal.CurrentItem());

			//model.IsDirty(true);
			model.CargoDescriptionEditModal.IsDone = true;
			$(cargoDescriptionModalSelector).modal("hide");
		}
	};

	// #endregion
}

$(function () {
	ko.applyBindings(new CargoDescriptionsViewModel(modelData, {
		GetNewCargoDescriptionUrl: app.urls.GetNewCargoDescription,
		SaveCargoDescriptionUrl: app.urls.SaveCargoDescription
	}), document.getElementById("ko-root"));
});