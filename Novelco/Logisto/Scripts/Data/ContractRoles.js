var ContractRolesViewModel = function (source, options)
{
	var model = this;

	model.Options = $.extend({
		GetNewContractRoleUrl: null,
		DeleteContractRoleUrl: null,
		SaveContractRoleUrl: null
	}, options);

	// поля ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	// Список записей
	model.Items = ko.mapping.fromJS(source);

	// вычисляемое/подписки ///////////////////////////////////////////////////////////////////////////////////////////////////


	// инициализация //////////////////////////////////////////////////////////////////////////////////////////////////////////


	// функции ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	model.SaveContractRole = function (data)
	{
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.SaveContractRoleUrl,
			data: {
				ID: data.ID(),
				Display: data.Display(),
				AblativeName: data.AblativeName(),
				DativeName: data.DativeName(),
				EnName: data.EnName()

				//IsDeleted: data.IsDeleted()
			}
		});
	};

	model.DeleteContractRole = function (data)
	{
		if (!confirm("Проверьте что удаляемая роль не используется в договорах, иначе это может вызвать ошибки при их открытии."))
			return;

		$.ajax({
			type: "POST",
			url: model.Options.DeleteContractRoleUrl,
			data: { ID: data.ID() },
			success: function (response) { model.Items.remove(data); }
		});
	};

	// #region create/edit modal //////////////////////////////////////////////////////////////////////////////////////////////

	var contractRoleModalSelector = "#contractRoleEditModal";

	model.OpenContractRoleCreate = function ()
	{
		$.ajax({
			type: "POST",
			url: model.Options.GetNewContractRoleUrl,
			success: function (response)
			{
				var data = ko.mapping.fromJSON(response);
				model.Items.push(data);
				model.ContractRoleEditModal.CurrentItem(data);
				$(contractRoleModalSelector).modal("show");
				//$(contractRoleModalSelector).draggable({ handle: ".modal-header" });
				model.ContractRoleEditModal.OnClosed = function () { model.Items.remove(data); };
				model.ContractRoleEditModal.Init();
			}
		});
	};

	model.OpenContractRoleEdit = function (data)
	{
		model.ContractRoleEditModal.CurrentItem(data);
		$(contractRoleModalSelector).modal("show");
		//$(paymentTermModalSelector).draggable({ handle: ".modal-header" });;
		model.ContractRoleEditModal.Init();
	};

	model.ContractRoleEditModal = {
		CurrentItem: ko.observable(),
		IsDone: false,
		Init: function ()
		{
			model.ContractRoleEditModal.IsDone = false;
		},
		Done: function (data, e)
		{
			// сохранить изменения
			model.SaveContractRole(model.ContractRoleEditModal.CurrentItem());

			//model.IsDirty(true);
			model.ContractRoleEditModal.IsDone = true;
			$(contractRoleModalSelector).modal("hide");
		}
	};

	// #endregion
}

$(function ()
{
	ko.applyBindings(new ContractRolesViewModel(modelData, {
		GetNewContractRoleUrl: app.urls.GetNewContractRole,
		DeleteContractRoleUrl: app.urls.DeleteContractRole,
		SaveContractRoleUrl: app.urls.SaveContractRole
	}), document.getElementById("ko-root"));
});