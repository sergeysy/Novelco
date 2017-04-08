var PackageTypesViewModel = function (source, options) {
	var model = this;

	model.Options = $.extend({
		SavePackageTypeUrl: null
	}, options);

	// поля ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	// Список записей
	model.Items = ko.mapping.fromJS(source);

	// вычисляемое/подписки ///////////////////////////////////////////////////////////////////////////////////////////////////


	// инициализация //////////////////////////////////////////////////////////////////////////////////////////////////////////


	// функции ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	model.SavePackageType = function (data) {
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.SavePackageTypeUrl,
			data: {
				ID: data.ID(),
				Display: data.Display(),
				EnDisplay: data.EnDisplay()

				//IsDeleted: data.IsDeleted()
			}
		});
	};

	// #region create/edit modal //////////////////////////////////////////////////////////////////////////////////////////////

	var packageTypeModalSelector = "#packageTypeEditModal";

	model.OpenPackageTypeEdit = function (data) {
		model.PackageTypeEditModal.CurrentItem(data);
		$(packageTypeModalSelector).modal("show");
		$(packageTypeModalSelector).draggable({ handle: ".modal-header" });;
		model.PackageTypeEditModal.Init();
	};

	model.PackageTypeEditModal = {
		CurrentItem: ko.observable(),
		IsDone: false,
		Init: function () {
			model.PackageTypeEditModal.IsDone = false;
		},
		Done: function (data, e) {
			// сохранить изменения
			model.SavePackageType(model.PackageTypeEditModal.CurrentItem());

			//model.IsDirty(true);
			model.PackageTypeEditModal.IsDone = true;
			$(packageTypeModalSelector).modal("hide");
		}
	};

	// #endregion
}

$(function () {
	ko.applyBindings(new PackageTypesViewModel(modelData, {
		SavePackageTypeUrl: app.urls.SavePackageType
	}), document.getElementById("ko-root"));
});