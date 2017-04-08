var ParticipantPermissionsViewModel = function (source, options)
{
	var model = this;

	model.Options = $.extend({
		SetPermissionUrl: null,
		IsAllowedUrl: null
	}, options);

	// поля ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	// Список записей
	model.Roles = ko.mapping.fromJS(source.Roles);
	model.Actions = ko.mapping.fromJS(source.Actions);

	// вычисляемое/подписки ///////////////////////////////////////////////////////////////////////////////////////////////////


	// функции ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	model.SetPermission = function (actionId, roleId, element)
	{
		$.ajax({
			type: "POST",
			url: model.Options.SetPermissionUrl,
			data: { ActionId: actionId, ParticipantRoleId: roleId, Allow: true },
			success: function (response) { $(element).parent("td").addClass("allowed"); }
		});
	};

	model.ResetPermission = function (actionId, roleId, element)
	{
		$.ajax({
			type: "POST",
			url: model.Options.SetPermissionUrl,
			data: { ActionId: actionId, ParticipantRoleId: roleId, Allow: false },
			success: function (response) { $(element).parent("td").removeClass("allowed"); }
		});
	};

	model.IsAllowed = function (actionId, roleId)
	{
		var result = false;
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.IsAllowedUrl,
			data: { ActionId: actionId, ParticipantRoleId: roleId },
			success: function (response) { result = response == "True"; }
		});

		return result;
	};


	// инициализация //////////////////////////////////////////////////////////////////////////////////////////////////////////

}

$(function ()
{
	ko.applyBindings(new ParticipantPermissionsViewModel(modelData, {
		SetPermissionUrl: app.urls.SetPermission,
		IsAllowedUrl: app.urls.IsAllowed
	}), document.getElementById("ko-root"));
});