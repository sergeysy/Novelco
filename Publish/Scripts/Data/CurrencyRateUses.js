var CurrencyRateUsesViewModel = function (source, options) {
	var model = this;

	model.Options = $.extend({
		GetNewCurrencyRateUseUrl: null,
		SaveCurrencyRateUseUrl: null
	}, options);

	// поля ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	// Список записей
	model.Items = ko.mapping.fromJS(source);

	// вычисляемое/подписки ///////////////////////////////////////////////////////////////////////////////////////////////////

	// функции ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	model.SaveCurrencyRateUse = function (data) {
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.SaveCurrencyRateUseUrl,
			data: {
				ID: data.ID(),
				Display: data.Display(),
				Value: data.Value(),
				IsDocumentDate: data.IsDocumentDate()
			},
			success: function (response) {
				var id = JSON.parse(response).ID;
				if (id)
					data.ID(id);
			}
		});
	};

	// #region create/edit modal //////////////////////////////////////////////////////////////////////////////////////////////

	var currencyRateUseModalSelector = "#currencyRateUseEditModal";
	
	model.OpenCurrencyRateUseCreate = function () {
		var data = null;
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.GetNewCurrencyRateUseUrl,
			success: function (response) {
				data = ko.mapping.fromJSON(response);
				model.Items.push(data);
			}
		});

		model.CurrencyRateUseEditModal.CurrentItem(data);
		$(currencyRateUseModalSelector).modal("show");
		//$(currencyRateUseModalSelector).draggable({ handle: ".modal-header" });
		model.CurrencyRateUseEditModal.OnClosed = function () { model.Items.remove(data); };
		model.CurrencyRateUseEditModal.Init();
	};
	
	model.OpenCurrencyRateUseEdit = function (data) {
		model.CurrencyRateUseEditModal.CurrentItem(data);
		$(currencyRateUseModalSelector).modal("show");
		//$(currencyRateUseModalSelector).draggable({ handle: ".modal-header" });;
		model.CurrencyRateUseEditModal.Init();
	};

	model.CurrencyRateUseEditModal = {
		CurrentItem: ko.observable(),
		Init: function () {		},
		OnClosed: null,
		Close: function (self, e) {
			$(currencyRateUseModalSelector).modal("hide");
			if (self.OnClosed != null)
				self.OnClosed();
		},
		Done: function (self, e) {
			// сохранить изменения
			model.SaveCurrencyRateUse(self.CurrentItem());

			$(currencyRateUseModalSelector).modal("hide");
		}
	};

	// #endregion

	// инициализация //////////////////////////////////////////////////////////////////////////////////////////////////////////
	
}

$(function () {
	ko.applyBindings(new CurrencyRateUsesViewModel(modelData, {
		GetNewCurrencyRateUseUrl: app.urls.GetNewCurrencyRateUse,
		SaveCurrencyRateUseUrl: app.urls.SaveCurrencyRateUse
	}), document.getElementById("ko-root"));
});