var RoleViewModel = function (source, options)
{
	var model = this;

	model.Options = $.extend({
		SaveRoleUrl: null
	}, options);

	// поля ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	model.Role = ko.mapping.fromJS(source);
	model.IsDirty = ko.observable(true);

	// вычисляемое/подписки ///////////////////////////////////////////////////////////////////////////////////////////////////


	// инициализация //////////////////////////////////////////////////////////////////////////////////////////////////////////

	window.onbeforeunload = function (evt)
	{
		if (model.IsDirty())
		{
			var message = "Есть несохраненные изменения. Если просто так уйти со страницы, то они не сохранятся.";
			if (typeof evt == "undefined")
				evt = window.event;

			if (evt)
				evt.returnValue = message;

			return message;
		}
	}

	// функции ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	model.Save = function ()
	{
		// формирование DTO
		var data = {
			ID: model.Role.ID(),
			Name: model.Role.Name(),
			Description: model.Role.Description()
		};

		$.ajax({
			type: "POST",
			url: model.Options.SaveRoleUrl,
			data: JSON.stringify(data),
			dataType: "json",
			contentType: "application/json; charset=utf-8",
			success: function (response)
			{
				model.IsDirty(false);
				window.close();
			}
		});
	};
}

$(function ()
{
	ko.applyBindings(new RoleViewModel(modelData, {
		SaveRoleUrl: app.urls.SaveRole,
	}), document.getElementById("ko-root"));
});