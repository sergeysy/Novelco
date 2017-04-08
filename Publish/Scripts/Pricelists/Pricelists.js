var PricelistsViewModel = function (source, options)
{
	var model = this;

	model.Options = $.extend({
		GetItemsUrl: null,
		ContractUrl: null,
		ContractorUrl: null,
		CreatePricelistUrl: null,
		ViewPricelistUrl: null
	}, options);

	// поля ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	// Фильтр
	model.Filter = ko.mapping.fromJS(source.Filter);
	// Список записей
	model.Items = ko.observableArray();
	// Общее количество записей для текущего фильтра
	model.TotalItemsCount = ko.observable(0);
	// Справочники
	model.Dictionaries = ko.mapping.fromJS(source.Dictionaries);

	// вычисляемое/подписки ///////////////////////////////////////////////////////////////////////////////////////////////////

	model.TotalPageCount = ko.computed(function ()
	{
		return Math.ceil(model.TotalItemsCount() / model.Filter.PageSize());
	});

	model.Filter.Context.subscribe(function () { model.ApplyFilter() });

	// функции ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	model.GotoPricelist = function (data) { window.location = model.Options.ViewPricelistUrl + "/" + data.ID(); };
	model.GetContractUrl = function (data) { return model.Options.ContractUrl + "/" + data.ContractId(); };
	model.GetContractorUrl = function (data)
	{
		var q = ko.utils.arrayFirst(model.Dictionaries.ContractorByContract(), function (item) { return item.ID() == data.ContractId() });
		if (q)
			return model.Options.ContractorUrl + "/" + q.TargetId();
		else
			return "#";
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
		$.ajax({
			type: "POST",
			url: model.Options.GetItemsUrl,
			data: {
				Context: model.Filter.Context() || "",
				UserId: model.Filter.UserId() || "",
				From: app.utility.SerializeDateTime(model.Filter.From()),
				To: app.utility.SerializeDateTime(model.Filter.To()),
				PageSize: model.Filter.PageSize(),
				PageNumber: model.Filter.PageNumber(),
				Sort: model.Filter.Sort(),
				SortDirection: model.Filter.SortDirection()
			},
			success: function (response)
			{
				var temp = ko.mapping.fromJSON(response);
				model.Items(temp.Items());
				model.TotalItemsCount(temp.TotalCount());
				if (model.Filter.PageNumber() > model.TotalPageCount())
					model.Filter.PageNumber(0);
			}
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

	// инициализация //////////////////////////////////////////////////////////////////////////////////////////////////////////

	model.Filter.SortDirection(model.Filter.SortDirection() || "Asc");
	model.ApplyFilter();
}

$(function ()
{
	ko.applyBindings(new PricelistsViewModel(modelData, {
		GetItemsUrl: app.urls.GetItems,
		ContractUrl: app.urls.ContractDetails,
		ContractorUrl: app.urls.ContractorDetails,
		CreatePricelistUrl: app.urls.CreatePricelist,
		ViewPricelistUrl: app.urls.ViewPricelist
	}), document.getElementById("ko-root"));
});