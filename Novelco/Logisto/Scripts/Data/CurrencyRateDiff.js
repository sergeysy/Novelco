var CurrencyRateDiffViewModel = function (source, options) {
	var model = this;

	model.Options = $.extend({
		GetNewCurrencyRateDiffUrl: null,
		SaveCurrencyRateDiffUrl: null
	}, options);

	// поля ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	// Список записей
	model.Items = ko.mapping.fromJS(source);

	// вычисляемое/подписки ///////////////////////////////////////////////////////////////////////////////////////////////////

	// функции ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	model.SaveCurrencyRateDiff = function (data) {
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.SaveCurrencyRateDiffUrl,
			data: {
				ID: data.ID(),
				From: app.utility.SerializeDateTime(data.From()),
				To: app.utility.SerializeDateTime(data.To()),
				USD: data.USD(),
				EUR: data.EUR(),
				CNY: data.CNY(),
				GBP: data.GBP()
				
			},
			success: function (response) {
				var id = JSON.parse(response).ID;
				if (id)
					data.ID(id);
			}
		});
	};

	// #region create/edit modal //////////////////////////////////////////////////////////////////////////////////////////////

	var currencyRateDiffModalSelector = "#currencyRateDiffEditModal";
	
	model.OpenCurrencyRateDiffCreate = function () {
		var data = null;
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.GetNewCurrencyRateDiffUrl,
			success: function (response) {
				data = ko.mapping.fromJSON(response);
				model.Items.push(data);
			}
		});

		model.CurrencyRateDiffEditModal.CurrentItem(data);
		$(currencyRateDiffModalSelector).modal("show");
		$(currencyRateDiffModalSelector).draggable({ handle: ".modal-header" });
		model.CurrencyRateDiffEditModal.OnClosed = function () { model.Items.remove(data); };
		model.CurrencyRateDiffEditModal.Init();
	};
	
	model.OpenCurrencyRateDiffEdit = function (data) {
		model.CurrencyRateDiffEditModal.CurrentItem(data);
		$(currencyRateDiffModalSelector).modal("show");
		$(currencyRateDiffModalSelector).draggable({ handle: ".modal-header" });;
		model.CurrencyRateDiffEditModal.Init();
	};

	model.CurrencyRateDiffEditModal = {
		CurrentItem: ko.observable(),
		Init: function () {},
		OnClosed: null,
		Close: function (self, e) {
			$(currencyRateDiffModalSelector).modal("hide");
			if (self.OnClosed != null)
				self.OnClosed();
		},
		Done: function (self, e) {
			// сохранить изменения
			model.SaveCurrencyRateDiff(self.CurrentItem());

			$(currencyRateDiffModalSelector).modal("hide");
		}
	};

	// #endregion

	// инициализация //////////////////////////////////////////////////////////////////////////////////////////////////////////
	
}

$(function () {
	ko.applyBindings(new CurrencyRateDiffViewModel(modelData, {
		GetNewCurrencyRateDiffUrl: app.urls.GetNewCurrencyRateDiff,
		SaveCurrencyRateDiffUrl: app.urls.SaveCurrencyRateDiff
	}), document.getElementById("ko-root"));
});