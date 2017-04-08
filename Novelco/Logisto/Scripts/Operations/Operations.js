var OperationsViewModel = function (source, options)
{
	var model = this;

	model.Options = $.extend({
		GetItemsUrl: null,
		GetPlacesUrl: null,
		OrderDetailsUrl: null,
		SaveOperationUrl: null
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

	// вычисляемое/подписки ///////////////////////////////////////////////////////////////////////////////////////////////////

	model.TotalPageCount = ko.computed(function ()
	{
		return Math.ceil(model.TotalItemsCount() / model.Filter.PageSize());
	});

	// функции ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	model.ContractorDetailsUrl = function (id) { return model.Options.ContractorDetailsUrl + "/" + id; };
	model.OrderDetailsUrl = function (id) { return model.Options.OrderDetailsUrl + "/" + id; };
	model.UserDetailsUrl = function (id) { return model.Options.UserDetailsUrl + "/" + id; };

	model.GetOperationKind = function (operationId)
	{
		var id = ko.unwrap(operationId);
		var kind = ko.utils.arrayFirst(model.Dictionaries.OrderOperation(), function (item) { return item.ID() == id });
		if (kind)
			return app.utility.GetDisplay(model.Dictionaries.OperationKind, kind.OperationKindId());

		return "";
	};

	model.Collapse = function ()
	{
		$(".panel-body>div").hide();
		$(".panel-body>a").show();
	};

	model.Expand = function ()
	{
		$(".panel-body>div").show();
		$(".panel-body>a").hide();
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
				OrderNumber: model.Filter.OrderNumber() || "",
				Statuses: $("#statuses li input:checkbox:checked").map(function () { return this.value; }).get(),
				Responsibles: $("#users li input:checkbox:checked").map(function () { return this.value; }).get(),
				StartPlanFrom: app.utility.SerializeDateTime(model.Filter.StartPlanFrom()),
				StartPlanTo: app.utility.SerializeDateTime(model.Filter.StartPlanTo()),
				StartFactFrom: app.utility.SerializeDateTime(model.Filter.StartFactFrom()),
				StartFactTo: app.utility.SerializeDateTime(model.Filter.StartFactTo()),
				FinishPlanFrom: app.utility.SerializeDateTime(model.Filter.FinishPlanFrom()),
				FinishPlanTo: app.utility.SerializeDateTime(model.Filter.FinishPlanTo()),
				FinishFactFrom: app.utility.SerializeDateTime(model.Filter.FinishFactFrom()),
				FinishFactTo: app.utility.SerializeDateTime(model.Filter.FinishFactTo()),
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
				if (model.Filter.PageNumber() >= model.TotalPageCount())
				{
					model.Filter.PageNumber(0);
					model.ApplyFilter();
				}
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

	// #region operation datetime edit modal //////////////////////////////////////////////////////////////////////////////////

	var datetimeModalSelector = "#datetimeEditModal";

	model.OpenDateTimeEdit = function (data, datetime)
	{
		model.DateTimeEditModal.CurrentEntity(data);
		model.DateTimeEditModal.CurrentItem(datetime);
		$(datetimeModalSelector).modal("show");
		$(datetimeModalSelector).draggable({ handle: ".modal-header" });;
		model.DateTimeEditModal.OnClosed = null;
		model.DateTimeEditModal.Init();
	};

	model.DateTimeEditModal = {
		CurrentEntity: ko.observable(),
		CurrentItem: ko.observable(),
		CurrentDate: ko.observable(),
		CurrentTime: ko.observable(),
		OnClosed: null,
		Close: function (self, e)
		{
			$(datetimeModalSelector).modal("hide");
			if (self.OnClosed != null)
				self.OnClosed();
		},
		Init: function ()
		{
			model.DateTimeEditModal.CurrentDate(ko.unwrap(model.DateTimeEditModal.CurrentItem()));
			model.DateTimeEditModal.CurrentTime(app.utility.FormatTime(model.DateTimeEditModal.CurrentItem()) || "00:00");
		},
		Done: function (self, e)
		{
			// сохранить изменения
			self.CurrentDate(app.utility.ParseDate(self.CurrentDate()));
			if (self.CurrentDate())
			{
				self.CurrentDate().setHours(parseInt(self.CurrentTime().substring(0, 2)) || 0);
				self.CurrentDate().setMinutes(parseInt(self.CurrentTime().substring(3, 5)) || 0);
			}

			self.CurrentItem()(self.CurrentDate());

			// save operation
			$.ajax({
				type: "POST",
				url: model.Options.SaveOperationUrl,
				data: {
					Id: self.CurrentEntity().ID(),
					No: self.CurrentEntity().No(),
					Name: self.CurrentEntity().Name(),
					OrderId: self.CurrentEntity().OrderId(),
					OrderOperationId: self.CurrentEntity().OrderOperationId(),
					OperationStatusId: self.CurrentEntity().OperationStatusId(),
					ResponsibleUserId: self.CurrentEntity().ResponsibleUserId(),
					StartPlanDate: app.utility.SerializeDateTime(self.CurrentEntity().StartPlanDate()),
					StartFactDate: app.utility.SerializeDateTime(self.CurrentEntity().StartFactDate()),
					FinishPlanDate: app.utility.SerializeDateTime(self.CurrentEntity().FinishPlanDate()),
					FinishFactDate: app.utility.SerializeDateTime(self.CurrentEntity().FinishFactDate())
				},
				success: function (response)
				{
					var r = JSON.parse(response);
					if (r.Message)
						alert(r.Message);
					else
					{
						self.CurrentEntity().OperationStatusId(r.OperationStatusId);
					}
				}
			});

			$(datetimeModalSelector).modal("hide");
		}
	};

	// #endregion

	// #region finish operation datetime edit modal ///////////////////////////////////////////////////////////////////////////

	var finishOperationDateTimeModalSelector = "#finishOperationDateTimeEditModal";

	model.OpenFinishDateTimeEdit = function (data, datetime)
	{
		model.FinishOperationDateTimeEditModal.CurrentEntity(data);
		model.FinishOperationDateTimeEditModal.CurrentItem(datetime);
		$(finishOperationDateTimeModalSelector).modal("show");
		$(finishOperationDateTimeModalSelector).draggable({ handle: ".modal-header" });;
		model.FinishOperationDateTimeEditModal.OnClosed = null;
		model.FinishOperationDateTimeEditModal.Init();
	};

	model.FinishOperationDateTimeEditModal = {
		CurrentEntity: ko.observable(),
		CurrentItem: ko.observable(),
		CurrentDate: ko.observable(),
		CurrentTime: ko.observable(),
		CurrentCity: ko.observable(),
		CurrentComment: ko.observable(),
		CurrentPlaces: ko.observableArray([]),
		OnClosed: null,
		Close: function (self, e)
		{
			$(finishOperationDateTimeModalSelector).modal("hide");
			if (self.OnClosed != null)
				self.OnClosed();
		},
		Init: function ()
		{
			model.FinishOperationDateTimeEditModal.CurrentDate(ko.unwrap(model.FinishOperationDateTimeEditModal.CurrentItem()));
			model.FinishOperationDateTimeEditModal.CurrentTime(app.utility.FormatTime(model.FinishOperationDateTimeEditModal.CurrentItem()) || "00:00");

			$("#placeAutocomplete").autocomplete({
				source: model.Options.GetPlacesUrl,
				select: function (e, ui) { model.FinishOperationDateTimeEditModal.CurrentCity(ui.item.entity); }
			});
		},
		Done: function (self, e)
		{
			// сохранить изменения
			self.CurrentDate(app.utility.ParseDate(self.CurrentDate()));
			if (self.CurrentDate())
			{
				self.CurrentDate().setHours(parseInt(self.CurrentTime().substring(0, 2)) || 0);
				self.CurrentDate().setMinutes(parseInt(self.CurrentTime().substring(3, 5)) || 0);
			}

			self.CurrentItem()(self.CurrentDate());

			// save operation
			$.ajax({
				type: "POST",
				url: model.Options.SaveOperationUrl,
				data: {
					Id: self.CurrentEntity().ID(),
					No: self.CurrentEntity().No(),
					Name: self.CurrentEntity().Name(),
					OrderId: self.CurrentEntity().OrderId(),
					OrderOperationId: self.CurrentEntity().OrderOperationId(),
					OperationStatusId: self.CurrentEntity().OperationStatusId(),
					ResponsibleUserId: self.CurrentEntity().ResponsibleUserId(),
					StartPlanDate: app.utility.SerializeDateTime(self.CurrentEntity().StartPlanDate()),
					StartFactDate: app.utility.SerializeDateTime(self.CurrentEntity().StartFactDate()),
					FinishPlanDate: app.utility.SerializeDateTime(self.CurrentEntity().FinishPlanDate()),
					FinishFactDate: app.utility.SerializeDateTime(self.CurrentEntity().FinishFactDate()),
					City: self.CurrentCity().Name,
					Comment: self.CurrentComment()
				},
				success: function (response)
				{
					var r = JSON.parse(response);
					if (r.Message)
						alert(r.Message);
					else
					{
						self.CurrentEntity().OperationStatusId(r.OperationStatusId);
					}
				}
			});

			$(finishOperationDateTimeModalSelector).modal("hide");
		}
	};

	// #endregion

	// инициализация //////////////////////////////////////////////////////////////////////////////////////////////////////////

	ko.utils.arrayForEach(model.Filter.Responsibles(), function (item)
	{
		$("#users li input:checkbox[value='" + item + "']").prop('checked', true);
	});
}

$(function ()
{
	ko.applyBindings(new OperationsViewModel(modelData, {
		GetItemsUrl: app.urls.GetItems,
		GetPlacesUrl: app.urls.GetPlaces,
		SaveOperationUrl: app.urls.SaveOperation,
		OrderDetailsUrl: app.urls.OrderDetails
	}), document.getElementById("ko-root"));
});