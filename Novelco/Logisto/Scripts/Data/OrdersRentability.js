var OrdersRentabilityViewModel = function (source, options)
{
	var model = this;

	model.Options = $.extend({
		GetNewOrderRentabilityUrl: null,
		SaveOrderRentabilityUrl: null
	}, options);

	// поля ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	// Список записей
	model.Items = ko.mapping.fromJS(source.Items);
	// Справочники
	model.Dictionaries = ko.mapping.fromJS(source.Dictionaries);

	// вычисляемое/подписки ///////////////////////////////////////////////////////////////////////////////////////////////////

	// инициализация //////////////////////////////////////////////////////////////////////////////////////////////////////////


	// функции ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	model.SaveOrderRentability = function (data)
	{
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.SaveOrderRentabilityUrl,
			data: {
				ID: data.ID(),
				OrderTemplateId: data.OrderTemplateId(),
				FinRepCenterId: data.FinRepCenterId(),
				Rentability: data.Rentability(),
				ProductId: data.ProductId(),
				Year: data.Year()
			},
			success: function (response)
			{
				var id = JSON.parse(response).ID;
				if (id)
					data.ID(id);
			}
		});
	};

	// #region template create/edit modal /////////////////////////////////////////////////////////////////////////////////////

	var orderRentabilityModalSelector = "#orderRentabilityEditModal";

	model.DeleteOrderRentability = function (service)
	{
		service.IsDeleted(true);
		model.IsDirty(true);
	};

	model.OpenOrderRentabilityCreate = function ()
	{
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.GetNewOrderRentabilityUrl,
			success: function (response)
			{
				var data = ko.mapping.fromJSON(response);
				model.Items.push(data);
				model.OrderRentabilityEditModal.CurrentItem(data);
				$(orderRentabilityModalSelector).modal("show");
				//$(orderRentabilityModalSelector).draggable({ handle: ".modal-header" });
				model.OrderRentabilityEditModal.OnClosed = function () { model.Items.remove(data); };
				model.OrderRentabilityEditModal.Init();
			}
		});
	};

	model.OpenOrderRentabilityEdit = function (service)
	{
		model.OrderRentabilityEditModal.CurrentItem(service);
		$(orderRentabilityModalSelector).modal("show");
		//$(orderRentabilityModalSelector).draggable({ handle: ".modal-header" });;
		model.OrderRentabilityEditModal.Init();
	};

	model.OrderRentabilityEditModal = {
		CurrentItem: ko.observable(),
		OrderTemplates: ko.pureComputed(function () { return ko.utils.arrayFilter(model.Dictionaries.OrderTemplate(), function (item) { return item.ProductId() == model.OrderRentabilityEditModal.CurrentItem().ProductId() }) }),
		Init: function () { },
		Validate: function (self)
		{
			// TODO:
			return true;
		},
		OnClosed: null,
		Close: function (self, e)
		{
			$(orderRentabilityModalSelector).modal("hide");
			if (self.OnClosed != null)
				self.OnClosed();
		},
		Done: function (self, e)
		{
			if (!self.Validate(self))
				return;

			$(orderRentabilityModalSelector).modal("hide");
			// сохранить изменения
			model.SaveOrderRentability(self.CurrentItem());
			self.CurrentItem(null);
		}
	};

	// #endregion
}

$(function ()
{
	ko.applyBindings(new OrdersRentabilityViewModel(modelData, {
		GetNewOrderRentabilityUrl: app.urls.GetNewOrderRentability,
		SaveOrderRentabilityUrl: app.urls.SaveOrderRentability
	}), document.getElementById("ko-root"));
});