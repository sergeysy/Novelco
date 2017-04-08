var FinRepCentersViewModel = function (source, dictionaries, options)
{
	var model = this;

	model.Options = $.extend({
		GetNewFinRepCenterUrl: null,
		SaveFinRepCenterUrl: null
	}, options);

	// поля ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	// Список записей
	model.Items = ko.mapping.fromJS(source);
	// Справочники
	model.Dictionaries = ko.mapping.fromJS(dictionaries);

	// вычисляемое/подписки ///////////////////////////////////////////////////////////////////////////////////////////////////


	// инициализация //////////////////////////////////////////////////////////////////////////////////////////////////////////


	// функции ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	model.SaveFinRepCenter = function (data)
	{
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.SaveFinRepCenterUrl,
			data: {
				ID: data.ID(),
				Name: data.Name(),
				Code: data.Code(),
				Description: data.Description(),
				OurLegalId: data.OurLegalId()
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

	var finRepCenterModalSelector = "#finRepCenterEditModal";

	model.OpenFinRepCenterCreate = function ()
	{
		$.ajax({
			type: "POST",
			url: model.Options.GetNewFinRepCenterUrl,
			success: function (response)
			{
				var data = ko.mapping.fromJSON(response);
				model.Items.push(data);
				model.FinRepCenterEditModal.CurrentItem(data);
				$(finRepCenterModalSelector).modal("show");
				//$(finRepCenterModalSelector).draggable({ handle: ".modal-header" });
				model.FinRepCenterEditModal.OnClosed = function () { model.Items.remove(data); };
				model.FinRepCenterEditModal.Init();
			}
		});
	};

	model.OpenFinRepCenterEdit = function (data)
	{
		model.FinRepCenterEditModal.CurrentItem(data);
		$(finRepCenterModalSelector).modal("show");
		//$(finRepCenterModalSelector).draggable({ handle: ".modal-header" });;
		model.FinRepCenterEditModal.Init();
	};

	model.FinRepCenterEditModal = {
		CurrentItem: ko.observable(),
		Init: function () { },
		Done: function (data, e)
		{
			$(finRepCenterModalSelector).modal("hide");
			// сохранить изменения
			model.SaveFinRepCenter(model.FinRepCenterEditModal.CurrentItem());
		}
	};

	// #endregion
}

$(function ()
{
	ko.applyBindings(new FinRepCentersViewModel(modelData, modelDictionaries, {
		GetNewFinRepCenterUrl: app.urls.GetNewFinRepCenter,
		SaveFinRepCenterUrl: app.urls.SaveFinRepCenter
	}), document.getElementById("ko-root"));
});