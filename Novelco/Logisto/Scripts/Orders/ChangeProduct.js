var ChangeProductViewModel = function (source, options)
{
	var model = this;

	model.Options = $.extend({
		GetOrderTemplatesByProductUrl: null,
		GetServicesByOrderUrl: null,
		ChangeOrderProductUrl: null
	}, options);

	// поля ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////


	model.Order = ko.mapping.fromJS(source.Order);
	model.SelectedOrderTemplate = ko.observable();
	// Списки связанных сущностей
	model.TemplatesItems = ko.observableArray();
	model.ServicesItems = ko.observableArray();
	// Справочники
	model.Dictionaries = ko.mapping.fromJS(source.Dictionaries);

	// вычисляемое/подписки ///////////////////////////////////////////////////////////////////////////////////////////////////


	// функции ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	model.ExtendOrder = function (order)
	{
		order.ProductId.subscribe(function (newValue)
		{
			// reset new services
			ko.utils.arrayForEach(model.ServicesItems(), function (item) { item.NewServiceTypeId(undefined) });
			model.LoadTemplates(newValue);
		});
	};

	model.ExtendServices = function (array)
	{
		ko.utils.arrayForEach(array, function (item) { model.ExtendService(item); })
	};

	model.ExtendService = function (service)
	{
		service.IsDeleted = service.IsDeleted || ko.observable(false);
		service.NewServiceTypeId = service.NewServiceTypeId || ko.observable(false);
	};

	model.LoadServices = function (orderId)
	{
		var id = ko.unwrap(orderId);
		$.ajax({
			type: "POST",
			url: model.Options.GetServicesByOrderUrl,
			data: { OrderId: id },
			success: function (response)
			{
				var list = ko.mapping.fromJSON(response).Items();
				model.ExtendServices(list);
				model.ServicesItems(list);
			}
		});
	};

	model.LoadTemplates = function (productId)
	{
		var id = ko.unwrap(productId);
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.GetOrderTemplatesByProductUrl,
			data: { ProductId: id },
			success: function (response) { model.TemplatesItems(ko.mapping.fromJSON(response).Items()); }
		});
	};

	model.Save = function ()
	{
		if (ko.utils.arrayFirst(model.ServicesItems(), function (item) { return !item.NewServiceTypeId() }))
		{
			alert("Не во всех услугах исправлен тип");
			return;
		}

		// формирование DTO
		var data = {
			OrderID: model.Order.ID(),
			ProductId: model.Order.ProductId(),
			TemplateId: model.SelectedOrderTemplate(),
			Services: []
		};

		ko.utils.arrayForEach(model.ServicesItems(), function (item) { data.Services.push({ Id: item.ID(), Id2: item.NewServiceTypeId() }); });

		$.ajax({
			type: "POST",
			url: model.Options.ChangeOrderProductUrl,
			data: JSON.stringify(data),
			dataType: "json",
			contentType: "application/json; charset=utf-8",
			success: function (response)
			{
				if (response.Message)
					alert(response.Message);
				else
					window.location = response.Url;
			}
		});
	};

	model.GetServiceDisplay = function (sitem, data)
	{
		if (data)
		{
			var svc = ko.utils.arrayFirst(data.ServicesItems(), function (item) { return item.ID() == sitem.ID() });
			return (svc) ? app.utility.GetDisplay(model.Dictionaries.ServiceType, svc.ServiceTypeId()) : '';
		}

		return '';
	};

	model.GetServiceKind = function (serviceTypeId)
	{
		var id = ko.unwrap(serviceTypeId);
		var type = ko.utils.arrayFirst(model.Dictionaries.ServiceType(), function (item) { return item.ID() == id });
		if (type)
			return type.Kind();

		return "";
	};

	model.GetMeasureDisplay = function (serviceTypeId)
	{
		if (serviceTypeId() == 0)
			return "";

		var type = ko.utils.arrayFirst(model.Dictionaries.ServiceType(), function (item) { return item.ID() == serviceTypeId() });
		if (type)
		{
			if (type.MeasureId() > 1)
				return ko.utils.arrayFirst(model.Dictionaries.Measure(), function (item) { return item.ID() == type.MeasureId() }).Display();
			else
				return "";
		}

		return "-";
	};

	// #region service create/edit modal //////////////////////////////////////////////////////////////////////////////////////

	var serviceTypeModalSelector = "#serviceTypeSelectModal";

	model.OpenServiceTypeSelect = function (service)
	{
		model.ServiceTypeSelectModal.CurrentItem(service);
		$(serviceTypeModalSelector).modal("show");
		$(serviceTypeModalSelector).draggable({ handle: ".modal-header" });;
		model.ServiceTypeSelectModal.OnClosed = null;
		model.ServiceTypeSelectModal.Init();
	};

	model.ServiceTypeSelectModal = {
		CurrentItem: ko.observable(),
		CurrentServiceTypeId: ko.observable(),
		SelectedServiceTypeId: ko.observable(),
		CurrentServiceTypes: ko.observableArray(),
		OnClosed: null,
		Close: function (self, e)
		{
			$(serviceTypeModalSelector).modal("hide");
			if (self.OnClosed != null)
				self.OnClosed();
		},
		Init: function ()
		{
			model.ServiceTypeSelectModal.CurrentServiceTypeId(model.ServiceTypeSelectModal.CurrentItem().ServiceTypeId());
			model.ServiceTypeSelectModal.CurrentServiceTypes(ko.utils.arrayFilter(model.Dictionaries.ServiceType(), function (item) { return item.ProductId() == model.Order.ProductId() }));
		},
		Done: function (self, e)
		{
			// сохранить изменения
			self.CurrentItem().NewServiceTypeId(self.SelectedServiceTypeId());
			self.CurrentItem(null);
			$(serviceTypeModalSelector).modal("hide");
		}
	};

	// #endregion


	// инициализация //////////////////////////////////////////////////////////////////////////////////////////////////////////

	setTimeout(function ()
	{
		model.ExtendOrder(model.Order);
		model.LoadServices(model.Order.ID);
		model.LoadTemplates(model.Order.ProductId);
	}, 500);
}

$(function ()
{
	ko.applyBindings(new ChangeProductViewModel(modelData, {
		GetOrderTemplatesByProductUrl: app.urls.GetOrderTemplatesByProduct,
		GetServicesByOrderUrl: app.urls.GetServicesByOrder,
		ChangeOrderProductUrl: app.urls.ChangeOrderProduct
	}), document.getElementById("ko-root"));
});