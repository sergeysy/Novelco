var OrdersViewModel = function (source, options)
{
	var model = this;

	model.Options = $.extend({
		GetItemsUrl: null,
		DownloadExcelUrl: null,
		ContractorDetailsUrl: null
	}, options);

	// поля ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	// Фильтр
	model.Filter = ko.mapping.fromJS(source.Filter);
	// Список записей
	model.Items = ko.mapping.fromJS(source.Items);
	// Общее количество записей для текущего фильтра
	model.TotalItemsCount = ko.observable(source.TotalItemsCount);
	// Справочники
	model.Dictionaries = ko.mapping.fromJS(source.Dictionaries);
	// идет запрос фильтра
	model.DelayHandler = null;

	// вычисляемое/подписки ///////////////////////////////////////////////////////////////////////////////////////////////////

	model.TotalPageCount = ko.computed(function () { return Math.ceil(model.TotalItemsCount() / model.Filter.PageSize()); });

	model.Filter.Context.subscribe(function () { model.QueryFilter() });
	model.Filter.Type.subscribe(function () { model.ApplyFilter() });

	// инициализация //////////////////////////////////////////////////////////////////////////////////////////////////////////

	model.Filter.SortDirection(model.Filter.SortDirection() || "Asc");

	// функции ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	model.ContractorDetailsUrl = function (id) { return model.Options.ContractorDetailsUrl + "/" + id; };

	model.GetDatesDisplay = function (participant)
	{
		var p = ko.unwrap(participant);

		if (!p)
			return "";

		var result = "";
		if (p.FromDate())
			result += "с " + app.utility.FormatDate(p.FromDate());

		if (p.ToDate())
			result += " по " + app.utility.FormatDate(p.ToDate());

		return result;
	};

	model.GetFromDisplay = function (participant)
	{
		var p = ko.unwrap(participant);

		if (!p)
			return "";

		var result = "";
		if (p.FromDate())
			result += app.utility.FormatDate(p.FromDate());

		return result;
	};

	model.GetToDisplay = function (participant)
	{
		var p = ko.unwrap(participant);

		if (!p)
			return "";

		var result = "";
		if (p.ToDate())
			result += app.utility.FormatDate(p.ToDate());

		return result;
	};

	model.DownloadReport = function ()
	{
		// Build a form
		var form = $('<form></form>').attr('action', model.Options.DownloadExcelUrl).attr('method', 'POST');
		// Add key/value
		form.append($("<input></input>").attr('type', 'hidden').attr('name', "Context").attr('value', model.Filter.Context()));
		form.append($("<input></input>").attr('type', 'hidden').attr('name', "Sort").attr('value', model.Filter.Sort()));
		form.append($("<input></input>").attr('type', 'hidden').attr('name', "UserId").attr('value', model.Filter.UserId()));
		form.append($("<input></input>").attr('type', 'hidden').attr('name', "From").attr('value', app.utility.SerializeDateTime(model.Filter.From())));
		form.append($("<input></input>").attr('type', 'hidden').attr('name', "To").attr('value', app.utility.SerializeDateTime(model.Filter.To())));
		form.append($("<input></input>").attr('type', 'hidden').attr('name', "Type").attr('value', model.Filter.Type()));
		//send request
		form.appendTo('body').submit().remove();
	};

	model.QueryFilter = function ()
	{
		if (model.DelayHandler)
			clearTimeout(model.DelayHandler);

		model.DelayHandler = setTimeout(function () { model.ApplyFilter() }, 1000);
	};

	model.SortBy = function (field)
	{
		var f = ko.unwrap(field);
		if (model.Filter.Sort() == f)
			model.Filter.SortDirection(model.Filter.SortDirection() == "Asc" ? "Desc" : "Asc");
		else
			model.Filter.Sort(f);

		model.ApplyFilter();
	};

	model.ApplyFilter = function ()
	{
		model.DelayHandler = null;
		$.ajax({
			type: "POST",
			url: model.Options.GetItemsUrl,
			data: {
				Context: model.Filter.Context() || "",
				Type: model.Filter.Type() || "",
				UserId: model.Filter.UserId() || "",
				From: app.utility.SerializeDateTime(model.Filter.From()),
				To: app.utility.SerializeDateTime(model.Filter.To()),
				PageSize: model.Filter.PageSize(),
				PageNumber: model.Filter.PageNumber(),
				Sort: model.Filter.Sort(),
				SortDirection: model.Filter.SortDirection(),
				Statuses: $("#roles li input:checkbox:checked").map(function () { return this.value; }).get()
			},
			success: function (response)
			{
				var temp = ko.mapping.fromJSON(response);
				model.Items(temp.Items());
				model.TotalItemsCount(temp.TotalCount());
				if ((model.Filter.PageNumber() > 0) && (model.Filter.PageNumber() >= model.TotalPageCount()))
				{
					model.Filter.PageNumber(0);
					model.ApplyFilter();
				}
			},
			error: function () { }
		});
	};

	model.FirstPage = function ()
	{
		if (model.Filter.PageNumber() > 0)
		{
			model.Filter.PageNumber(0);
			model.ApplyFilter();
		}
	};

	model.PrevPage = function ()
	{
		if (model.Filter.PageNumber() > 0)
		{
			model.Filter.PageNumber(model.Filter.PageNumber() - 1);
			model.ApplyFilter();
		}
	};

	model.NextPage = function ()
	{
		if (model.Filter.PageNumber() < (model.TotalPageCount() - 1))
		{
			model.Filter.PageNumber(model.Filter.PageNumber() + 1);
			model.ApplyFilter();
		}
	};

	model.LastPage = function ()
	{
		if (model.Filter.PageNumber() < (model.TotalPageCount() - 1))
		{
			model.Filter.PageNumber(model.TotalPageCount() - 1);
			model.ApplyFilter();
		}
	};

}

$(function ()
{
	ko.applyBindings(new OrdersViewModel(modelData, {
		GetItemsUrl: app.urls.GetItems,
		DownloadExcelUrl: app.urls.DownloadExcel,
		ContractorDetailsUrl: app.urls.ContractorDetails
}), document.getElementById("ko-root"));
});