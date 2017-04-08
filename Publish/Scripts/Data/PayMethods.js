var PayMethodViewModel = function (source, options)
{
	var model = this;

	model.Options = $.extend({
		GetNewPayMethodUrl: null,
		SavePayMethodUrl: null
	}, options);

	// поля ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	// Список записей
	model.Items = ko.mapping.fromJS(source);

	// вычисляемое/подписки ///////////////////////////////////////////////////////////////////////////////////////////////////

	// функции ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	model.SavePayMethod = function (data)
	{
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.SavePayMethodUrl,
			data: {
				ID: data.ID(),
				From: app.utility.SerializeDateTime(data.From()),
				To: app.utility.SerializeDateTime(data.To()),
				Display: data.Display()
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

	var PayMethodModalSelector = "#payMethodEditModal";

	model.OpenPayMethodCreate = function ()
	{
		$.ajax({
			type: "POST",
			url: model.Options.GetNewPayMethodUrl,
			success: function (response)
			{
				var data = ko.mapping.fromJSON(response);
				model.Items.push(data);
				model.PayMethodEditModal.CurrentItem(data);
				$(PayMethodModalSelector).modal("show");
				$(PayMethodModalSelector).draggable({ handle: ".modal-header" });
				model.PayMethodEditModal.OnClosed = function () { model.Items.remove(data); };
				model.PayMethodEditModal.Init();
			}
		});
	};

	model.OpenPayMethodEdit = function (data)
	{
		model.PayMethodEditModal.CurrentItem(data);
		$(PayMethodModalSelector).modal("show");
		$(PayMethodModalSelector).draggable({ handle: ".modal-header" });;
		model.PayMethodEditModal.Init();
	};

	model.PayMethodEditModal = {
		CurrentItem: ko.observable(),
		Init: function () { },
		OnClosed: null,
		Close: function (self, e)
		{
			$(PayMethodModalSelector).modal("hide");
			if (self.OnClosed != null)
				self.OnClosed();
		},
		Done: function (self, e)
		{
			$(PayMethodModalSelector).modal("hide");
			// сохранить изменения
			model.SavePayMethod(self.CurrentItem());
		}
	};

	// #endregion

	// инициализация //////////////////////////////////////////////////////////////////////////////////////////////////////////

}

$(function ()
{
	ko.applyBindings(new PayMethodViewModel(modelData, {
		GetNewPayMethodUrl: app.urls.GetNewPayMethod,
		SavePayMethodUrl: app.urls.SavePayMethod
	}), document.getElementById("ko-root"));
});