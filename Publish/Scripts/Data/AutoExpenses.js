var AutoExpenseViewModel = function (source, options)
{
	var model = this;

	model.Options = $.extend({
		GetNewAutoExpenseUrl: null,
		SaveAutoExpenseUrl: null
	}, options);

	// поля ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	// Список записей
	model.Items = ko.mapping.fromJS(source);

	// вычисляемое/подписки ///////////////////////////////////////////////////////////////////////////////////////////////////

	// функции ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	model.SaveAutoExpense = function (data)
	{
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.SaveAutoExpenseUrl,
			data: {
				ID: data.ID(),
				From: app.utility.SerializeDateTime(data.From()),
				To: app.utility.SerializeDateTime(data.To()),
				USD: data.USD(),
				EUR: data.EUR(),
				CNY: data.CNY(),
				GBP: data.GBP()
			},
			success: function (response)
			{
				var id = JSON.parse(response).ID;
				if (id)
					data.ID(id);
			}
		});
	};

	// #region create/edit modal //////////////////////////////////////////////////////////////////////////////////////////////

	var autoExpenseModalSelector = "#autoExpenseEditModal";

	model.OpenAutoExpenseCreate = function ()
	{
		$.ajax({
			type: "POST",
			url: model.Options.GetNewAutoExpenseUrl,
			success: function (response)
			{
				var data = ko.mapping.fromJSON(response);
				model.Items.push(data);
				model.AutoExpenseEditModal.CurrentItem(data);
				$(autoExpenseModalSelector).modal("show");
				$(autoExpenseModalSelector).draggable({ handle: ".modal-header" });
				model.AutoExpenseEditModal.OnClosed = function () { model.Items.remove(data); };
				model.AutoExpenseEditModal.Init();
			}
		});
	};

	model.OpenAutoExpenseEdit = function (data)
	{
		model.AutoExpenseEditModal.CurrentItem(data);
		$(autoExpenseModalSelector).modal("show");
		$(autoExpenseModalSelector).draggable({ handle: ".modal-header" });;
		model.AutoExpenseEditModal.Init();
	};

	model.AutoExpenseEditModal = {
		CurrentItem: ko.observable(),
		Init: function () { },
		OnClosed: null,
		Close: function (self, e)
		{
			$(autoExpenseModalSelector).modal("hide");
			if (self.OnClosed != null)
				self.OnClosed();
		},
		Done: function (self, e)
		{
			$(autoExpenseModalSelector).modal("hide");
			// сохранить изменения
			model.SaveAutoExpense(self.CurrentItem());
		}
	};

	// #endregion

	// инициализация //////////////////////////////////////////////////////////////////////////////////////////////////////////

}

$(function ()
{
	ko.applyBindings(new AutoExpenseViewModel(modelData, {
		GetNewAutoExpenseUrl: app.urls.GetNewAutoExpense,
		SaveAutoExpenseUrl: app.urls.SaveAutoExpense
	}), document.getElementById("ko-root"));
});