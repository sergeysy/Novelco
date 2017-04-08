var VolumetricRatiosViewModel = function (source, options)
{
	var model = this;

	model.Options = $.extend({
		GetNewVolumetricRatioUrl: null,
		DeleteVolumetricRatioUrl: null,
		SaveVolumetricRatioUrl: null
	}, options);

	// поля ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	// Список записей
	model.Items = ko.mapping.fromJS(source);

	// вычисляемое/подписки ///////////////////////////////////////////////////////////////////////////////////////////////////


	// инициализация //////////////////////////////////////////////////////////////////////////////////////////////////////////


	// функции ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	model.SaveVolumetricRatio = function (data)
	{
		$.ajax({
			type: "POST",
			url: model.Options.SaveVolumetricRatioUrl,
			data: {
				ID: data.ID(),
				Display: data.Display(),
				Value: data.Value().toString().replace(/ /g, '').replace(/,/g, '.')
			},
			success: function (response)
			{
				var id = JSON.parse(response).ID;
				if (id)
					data.ID(id);
			}
		});
	};

	model.DeleteVolumetricRatio = function (data)
	{
		$.ajax({
			type: "POST",
			url: model.Options.DeleteVolumetricRatioUrl,
			data: { ID: data.ID() },
			success: function (response)
			{
				var r = JSON.parse(response);
				if (r.Message)
					alert(r.Message);
				else
					model.Items.remove(data);
			}
		});
	};

	// #region create/edit modal //////////////////////////////////////////////////////////////////////////////////////////////

	var volumetricRatioModalSelector = "#volumetricRatioEditModal";

	model.OpenVolumetricRatioCreate = function ()
	{
		$.ajax({
			type: "POST",
			url: model.Options.GetNewVolumetricRatioUrl,
			success: function (response)
			{
				var data = ko.mapping.fromJSON(response);
				model.Items.push(data);
				model.VolumetricRatioEditModal.CurrentItem(data);
				$(volumetricRatioModalSelector).modal("show");
				//$(paymentTermModalSelector).draggable({ handle: ".modal-header" });
				model.VolumetricRatioEditModal.OnClosed = function () { model.Items.remove(data); };
				model.VolumetricRatioEditModal.Init();
			}
		});
	};

	model.OpenVolumetricRatioEdit = function (data)
	{
		model.VolumetricRatioEditModal.CurrentItem(data);
		$(volumetricRatioModalSelector).modal("show");
		//$(paymentTermModalSelector).draggable({ handle: ".modal-header" });;
		model.VolumetricRatioEditModal.Init();
	};

	model.VolumetricRatioEditModal = {
		CurrentItem: ko.observable(),
		Init: function () { },
		Done: function (self, e)
		{
			$(volumetricRatioModalSelector).modal("hide");
			// сохранить изменения
			model.SaveVolumetricRatio(self.CurrentItem());
		}
	};

	// #endregion
}

$(function ()
{
	ko.applyBindings(new VolumetricRatiosViewModel(modelData, {
		GetNewVolumetricRatioUrl: app.urls.GetNewVolumetricRatio,
		DeleteVolumetricRatioUrl: app.urls.DeleteVolumetricRatio,
		SaveVolumetricRatioUrl: app.urls.SaveVolumetricRatio
	}), document.getElementById("ko-root"));
});