var ServicesViewModel = function (source, dictionaries, options) {
	var model = this;

	model.Options = $.extend({
		GetOrderTemplatesItemsUrl: null,
		GetServiceKindsItemsUrl: null,
		GetServiceTypesItemsUrl: null,
		SaveServiceKindUrl: null,
		SaveServiceTypeUrl: null,
		GetNewServiceTypeUrl: null,
		GetNewServiceKindUrl: null,
	GetNewProductUrl: null,
		SaveProductUrl: null
	}, options);

	// поля ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	// Список записей
	model.ProductsItems = ko.mapping.fromJS(source);
	model.ServiceKindsItems = ko.observableArray();
	model.ServiceTypesItems = ko.observableArray();
	model.OrderTemplatesItems = ko.observableArray();
	// Справочники
	model.Dictionaries = ko.mapping.fromJS(dictionaries);

	model.SelectedProduct = ko.observable();
	model.SelectedServiceKind = ko.observable();

	// вычисляемое/подписки ///////////////////////////////////////////////////////////////////////////////////////////////////


	// функции ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	model.SelectProduct = function (product) {
		model.SelectedProduct(product);
		model.SelectedServiceKind(null);
		model.ServiceTypesItems([]);
		model.GetServiceKinds(product.ID);
		model.GetOrderTemplates(product.ID);
	};

	model.SelectServiceKind = function (kind) {
		model.SelectedServiceKind(kind);
		model.ServiceTypesItems([]);

		model.GetServiceTypes(kind.ID);
	};

	model.GetOrderTemplates = function (productId) {
		var id = ko.unwrap(productId);
		$.ajax({
			type: "POST",
			url: model.Options.GetOrderTemplatesItemsUrl,
			data: { ProductId: id },
			success: function (response) { model.OrderTemplatesItems(ko.mapping.fromJSON(response).Items()); }
		});
	};

	model.GetServiceKinds = function (productId) {
		var id = ko.unwrap(productId);

		$.ajax({
			type: "POST",
			url: model.Options.GetServiceKindsItemsUrl,
			data: { ProductId: id },
			success: function (response) {
				var list = ko.mapping.fromJSON(response).Items()
				//model.ExtendLegals(list);
				model.ServiceKindsItems(list);
			}
		});
	};

	model.GetServiceTypes = function (serviceKindId) {
		var id = ko.unwrap(serviceKindId);

		$.ajax({
			type: "POST",
			url: model.Options.GetServiceTypesItemsUrl,
			data: { ServiceKindId: id },
			success: function (response) {
				var list = ko.mapping.fromJSON(response).Items()
				//model.ExtendLegals(list);
				model.ServiceTypesItems(list);
			}
		});
	};

	model.SaveProduct = function (data) {
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.SaveProductUrl,
			data: {
				ID: data.ID(),
				Display: data.Display(),
				IsWorking: data.IsWorking(),
				ManagerUserId: data.ManagerUserId(),
				DeputyUserId: data.DeputyUserId(),
				VolumetricRatioId: data.VolumetricRatioId()
			},
			success: function (response) {
				var r = JSON.parse(response);
				if (r.Message)
					alert(r.Message);
				else
					data.ID(r.ID);
			}
		});
	};

	model.SaveServiceKind = function (data) {
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.SaveServiceKindUrl,
			data: {
				ID: data.ID(),
				ProductId: data.ProductId(),
				Name: data.Name(),
				EnName: data.EnName(),
				VatId: data.VatId()

				//IsDeleted: data.IsDeleted()
			},
			success: function (response) {
				var r = JSON.parse(response);
				if (r.Message)
					alert(r.Message);
				else
					data.ID(r.ID);
			}
		});
	};

	model.SaveServiceType = function (data) {
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.SaveServiceTypeUrl,
			data: {
				ID: data.ID(),
				ServiceKindId: data.ServiceKindId(),
				Name: data.Name(),
				EnName: data.EnName(),
				Count: data.Count(),
				MeasureId: data.MeasureId(),
				Price: data.Price()
			},
			success: function (response) {
				var r = JSON.parse(response);
				if (r.Message)
					alert(r.Message);
				else
					data.ID(r.ID);
			}
		});
	};

	// #region product create/edit modal //////////////////////////////////////////////////////////////////////////////////////

	var productModalSelector = "#productEditModal";

	model.OpenProductCreate = function (data) {
		var data = null;
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.GetNewProductUrl,
			success: function (response) {
				var r = JSON.parse(response);
				if (r.Message)
					alert(r.Message);
				else {
					data = ko.mapping.fromJSON(response);
					model.ProductsItems.push(data);
				}
			}
		});

		if (!data)
			return;

		model.ProductEditModal.CurrentItem(data);
		$(productModalSelector).modal("show");
		//$(productModalSelector).draggable({ handle: ".modal-header" });
		model.ProductEditModal.Init();
	};

	model.OpenProductEdit = function (data) {
		model.ProductEditModal.CurrentItem(data);
		$(productModalSelector).modal("show");
		//$(contractTypeModalSelector).draggable({ handle: ".modal-header" });;
		model.ProductEditModal.Init();
	};

	model.ProductEditModal = {
		CurrentItem: ko.observable(),
		Init: function () { },
		Done: function (self, e) {
			// сохранить изменения
			model.SaveProduct(self.CurrentItem());

			//model.IsDirty(true);
			$(productModalSelector).modal("hide");
		}
	};

	// #endregion

	// #region service kind create/edit modal /////////////////////////////////////////////////////////////////////////////////

	var serviceKindModalSelector = "#serviceKindEditModal";

	model.OpenServiceKindCreate = function (data) {
		var data = null;
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.GetNewServiceKindUrl,
			data: { ProductId: model.SelectedProduct().ID() },
			success: function (response) {
				var r = JSON.parse(response);
				if (r.Message)
					alert(r.Message);
				else {
					data = ko.mapping.fromJSON(response);
					model.ServiceKindsItems.push(data);
				}
			}
		});

		if (!data)
			return;

		model.ServiceKindEditModal.CurrentItem(data);
		$(serviceKindModalSelector).modal("show");
		//$(serviceKindModalSelector).draggable({ handle: ".modal-header" });
		model.ServiceKindEditModal.Init();
	};

	model.OpenServiceKindEdit = function (data) {
		model.ServiceKindEditModal.CurrentItem(data);
		$(serviceKindModalSelector).modal("show");
		//$(contractTypeModalSelector).draggable({ handle: ".modal-header" });;
		model.ServiceKindEditModal.Init();
	};

	model.ServiceKindEditModal = {
		CurrentItem: ko.observable(),
		Init: function () { },
		Done: function (self, e) {
			// сохранить изменения
			model.SaveServiceKind(self.CurrentItem());
			$(serviceKindModalSelector).modal("hide");
		}
	};

	// #endregion

	// #region service type create/edit modal /////////////////////////////////////////////////////////////////////////////////

	var serviceTypeModalSelector = "#serviceTypeEditModal";

	model.OpenServiceTypeCreate = function (data) {
		var data = null;
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.GetNewServiceTypeUrl,
			data: { ServiceKindId: model.SelectedServiceKind().ID() },
			success: function (response) {
				var r = JSON.parse(response);
				if (r.Message)
					alert(r.Message);
				else {
					data = ko.mapping.fromJSON(response);
					model.ServiceTypesItems.push(data);
				}
			}
		});

		if (!data)
			return;

		model.ServiceTypeEditModal.CurrentItem(data);
		$(serviceTypeModalSelector).modal("show");
		//$(serviceTypeModalSelector).draggable({ handle: ".modal-header" });
		model.ServiceTypeEditModal.Init();
	};

	model.OpenServiceTypeEdit = function (data) {
		model.ServiceTypeEditModal.CurrentItem(data);
		$(serviceTypeModalSelector).modal("show");
		//$(contractTypeModalSelector).draggable({ handle: ".modal-header" });;
		model.ServiceTypeEditModal.Init();
	};

	model.ServiceTypeEditModal = {
		CurrentItem: ko.observable(),
		Init: function () {
		},
		Done: function (self, e) {
			// сохранить изменения
			model.SaveServiceType(self.CurrentItem());
			$(serviceTypeModalSelector).modal("hide");
		}
	};

	// #endregion

	// инициализация //////////////////////////////////////////////////////////////////////////////////////////////////////////

}

$(function () {
	ko.applyBindings(new ServicesViewModel(modelData, modelDictionaries, {
		GetOrderTemplatesItemsUrl: app.urls.GetOrderTemplatesItems,
		GetServiceKindsItemsUrl: app.urls.GetServiceKindsItems,
		GetServiceTypesItemsUrl: app.urls.GetServiceTypesItems,
		SaveServiceKindUrl: app.urls.SaveServiceKind,
		SaveServiceTypeUrl: app.urls.SaveServiceType,
		GetNewServiceTypeUrl: app.urls.GetNewServiceType,
		GetNewServiceKindUrl: app.urls.GetNewServiceKind,
		GetNewProductUrl: app.urls.GetNewProduct,
		SaveProductUrl: app.urls.SaveProduct
	}), document.getElementById("ko-root"));
});