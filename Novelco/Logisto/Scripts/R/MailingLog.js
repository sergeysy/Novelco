var MailingLogViewModel = function (source, options)
{
	var model = this;

	model.Options = $.extend({
		GetItemsUrl: null
	}, options);

	// поля ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	// Фильтр
	model.Filter = ko.mapping.fromJS(source.Filter);
	// Список записей
	model.Items = ko.observableArray(source.Items);
	// идет запрос фильтра
	model.DelayHandler = null;

	// вычисляемое/подписки ///////////////////////////////////////////////////////////////////////////////////////////////////

	model.Filter.Context.subscribe(function () { model.QueryFilter() });

	// инициализация //////////////////////////////////////////////////////////////////////////////////////////////////////////

	model.Filter.SortDirection(model.Filter.SortDirection() || "Asc");

	// функции ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	model.QueryFilter = function ()
	{
		if (model.DelayHandler)
			clearTimeout(model.DelayHandler);

		model.DelayHandler = setTimeout(function () { model.ApplyFilter() }, 500);
	};

	model.ApplyFilter = function ()
	{
		model.DelayHandler = null;
		$.ajax({
			type: "POST",
			url: model.Options.GetItemsUrl,
			data: { Context: model.Filter.Context() || "" },
			success: function (response) { model.Items(JSON.parse(response).Items); }
		});
	};
}

$(function ()
{
	ko.applyBindings(new MailingLogViewModel(modelData, {
		GetItemsUrl: app.urls.GetItems
	}), document.getElementById("ko-root"));
});