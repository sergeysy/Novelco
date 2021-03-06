﻿var SyncQueueViewModel = function (source, options)
{
	var model = this;

	model.Options = $.extend({
		DeleteItemUrl: null
	}, options);

	// поля ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	// Список записей
	model.Items = ko.mapping.fromJS(source);

	// вычисляемое/подписки ///////////////////////////////////////////////////////////////////////////////////////////////////


	// инициализация //////////////////////////////////////////////////////////////////////////////////////////////////////////


	// функции ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	model.Delete = function (data)
	{
		$.ajax({
			type: "POST",
			url: model.Options.DeleteItemUrl,
			data: { ID: data.ID },
			success: function (response) { model.Items.remove(data); }
		});
	};
}

$(function ()
{
	ko.applyBindings(new SyncQueueViewModel(modelData, {
		DeleteItemUrl: app.urls.DeleteItem
	}), document.getElementById("ko-root"));
});