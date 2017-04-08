var CurrencyRatesViewModel = function (source, options) {
	var model = this;

	model.Options = $.extend({
		RefreshRatesUrl:null,
		GetItemsUrl: null
	}, options);

	// поля ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	// Фильтр
	model.Filter = ko.mapping.fromJS(source);
	// Список записей
	model.Items = ko.observableArray();
	// Общее количество записей для текущего фильтра
	model.TotalItemsCount = ko.observable(0);

	// вычисляемое/подписки ///////////////////////////////////////////////////////////////////////////////////////////////////

	model.TotalPageCount = ko.computed(function () {
		return Math.ceil(model.TotalItemsCount() / model.Filter.PageSize());
	});

	model.Filter.From.subscribe(function () { model.ApplyFilter() });
	model.Filter.To.subscribe(function () { model.ApplyFilter() });


	// функции ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	model.SortBy = function (field) {
		var f = ko.unwrap(field);
		if (model.Filter.Sort() == f)
			model.Filter.SortDirection(model.Filter.SortDirection() == "Asc" ? "Desc" : "Asc");
		else
			model.Filter.Sort(f);

		model.ApplyFilter();
	};

	model.ApplyFilter = function () {
		$.ajax({
			type: "POST",
			url: model.Options.GetItemsUrl,
			data: {
				From: app.utility.SerializeDateTime(model.Filter.From()),
				To: app.utility.SerializeDateTime(model.Filter.To()),
				Context: model.Filter.Context() || "",
				PageSize: model.Filter.PageSize(),
				PageNumber: model.Filter.PageNumber(),
				Sort: model.Filter.Sort(),
				SortDirection: model.Filter.SortDirection()
			},
			success: function (response) {
				var temp = ko.mapping.fromJSON(response);
				model.Items(temp.Items());
				model.TotalItemsCount(temp.TotalCount());
				if (model.Filter.PageNumber() > model.TotalPageCount())
					model.Filter.PageNumber(0);
			}
		});
	};

	model.FirstPage = function () {
		if (model.Filter.PageNumber() > 0) {
			model.Filter.PageNumber(0);
			model.ApplyFilter();
		}
	};

	model.PrevPage = function () {
		if (model.Filter.PageNumber() > 0) {
			model.Filter.PageNumber(model.Filter.PageNumber() - 1);
			model.ApplyFilter();
		}
	};

	model.NextPage = function () {
		if (model.Filter.PageNumber() < (model.TotalPageCount() - 1)) {
			model.Filter.PageNumber(model.Filter.PageNumber() + 1);
			model.ApplyFilter();
		}
	};

	model.LastPage = function () {
		if (model.Filter.PageNumber() < (model.TotalPageCount() - 1)) {
			model.Filter.PageNumber(model.TotalPageCount() - 1);
			model.ApplyFilter();
		}
	};

	model.RefreshRates = function () {
		$.ajax({
			type: "POST",
			url: model.Options.RefreshRatesUrl,
			success: function (response) {
				var resp = ko.mapping.fromJSON(response);
				if (resp.Message && resp.Message())
					alert(resp.Message());
			}
		});
	};

	// инициализация //////////////////////////////////////////////////////////////////////////////////////////////////////////

	model.ApplyFilter();
}

$(function () {
	ko.applyBindings(new CurrencyRatesViewModel(modelData, {
		RefreshRatesUrl: app.urls.RefreshRates,
		GetItemsUrl: app.urls.GetItems
	}), document.getElementById("ko-root"));
});