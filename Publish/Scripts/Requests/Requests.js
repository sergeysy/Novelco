var RequestsViewModel = function (source, options)
{
	var model = this;

	model.Options = $.extend({
		GetItemsUrl: null,
		GetNewRequestUrl: null,
		DownloadExcelUrl: null,
		DownloadRequestFileUrl: null,
		UploadFileUrl: null,
		SaveRequestUrl: null
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

	model.ResetUserSelection = function ()
	{
		model.Filter.UserId(0);
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
				UserId: model.Filter.UserId() || "",
				From: app.utility.SerializeDateTime(model.Filter.From()),
				To: app.utility.SerializeDateTime(model.Filter.To()),
				From2: app.utility.SerializeDateTime(model.Filter.From2()),
				To2: app.utility.SerializeDateTime(model.Filter.To2()),
				Statuses: $("#products li input:checkbox:checked").map(function () { return this.value; }).get(),
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

	// #region create/edit modal //////////////////////////////////////////////////////////////////////////////////////////////

	var requestModalSelector = "#requestEditModal";

	model.OpenRequestCreate = function ()
	{
		var data = null;
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.GetNewRequestUrl,
			success: function (response)
			{
				data = ko.mapping.fromJSON(response);
				if (!data.Message)
					model.Items.push(data);
			}
		});

		if (data.Message)
		{
			alert(data.Message());
			return;
		}

		model.RequestEditModal.CurrentItem(data);
		$(requestModalSelector).modal("show");
		$(requestModalSelector).draggable({ handle: ".modal-header" });
		model.RequestEditModal.Init();
	};

	model.OpenRequestEdit = function (request)
	{
		model.RequestEditModal.CurrentItem(request);
		$(requestModalSelector).modal("show");
		$(requestModalSelector).draggable({ handle: ".modal-header" });;
		model.RequestEditModal.Init();
	};

	model.RequestEditModal = {
		CurrentItem: ko.observable(),
		IsLocked: ko.observable(false),
		OnClosed: null,
		Close: function (self, e)
		{
			$(requestModalSelector).modal("hide");
			if (self.OnClosed != null)
				self.OnClosed();
		},
		Init: function ()
		{
			model.RequestEditModal.IsLocked(model.RequestEditModal.CurrentItem().ResponseDate() ? true : false);
		},
		Validate: function (data)
		{
			if (model.GetDatesDiff(data) && model.GetDatesDiff(data) < 0)
			{
				alert("Дата ответа не может быть меньше даты запроса.");
				return false;
			}

			if (!data.AccountUserId())
			{
				alert("Ответственный должен быть выбран.");
				return false;
			}

			return true;
		},
		Done: function (self, e)
		{
			if (!self.Validate(self.CurrentItem()))
				return;

			// сохранить изменения
			model.SaveRequest(self.CurrentItem());
			$(requestModalSelector).modal("hide");
			self.CurrentItem(null);
		}
	};

	model.SaveRequest = function (data)
	{
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.SaveRequestUrl,
			data: {
				ID: data.ID(),
				ClientName: data.ClientName(),
				SalesUserId: data.SalesUserId(),
				AccountUserId: data.AccountUserId(),
				ProductId: data.ProductId(),
				Text: data.Text(),
				CargoInfo: data.CargoInfo(),
				Contacts: data.Contacts(),
				Route: data.Route(),
				Date: app.utility.SerializeDateTime(data.Date()),
				ResponseDate: app.utility.SerializeDateTime(data.ResponseDate()),
				Comment: data.Comment()
			},
			success: function (response)
			{
				var r = JSON.parse(response);
				if (r.Message)
					alert(r.Message)
				else
				{
					if (r.ID)
						data.ID(r.ID);

					// upload file
					var formData = new FormData();
					formData.append("File", document.getElementById("fileUpload").files[0]);

					$.ajax({
						url: model.Options.UploadFileUrl + "/" + data.ID(),
						type: 'POST',
						data: formData,
						success: function (response) { alert("Готово"); },
						cache: false,
						contentType: false,
						processData: false
					});
				}
			}
		});
	};

	// #endregion

	model.GetDatesDiff = function (data)
	{
		var date1 = app.utility.ParseDate(data.Date());
		var date2 = app.utility.ParseDate(data.ResponseDate());
		if (date1 && date2)
			return ((date2 - date1) / (60 * 60 * 24 * 1000)) | 0;	// отбрасываем дробную часть

		return "";
	};

	model.IsYesterday = function (data)
	{
		var date1 = app.utility.ParseDate(data.Date());
		var date2 = new Date();
		if (date1 && date2)
			return (Math.floor((date2 - date1) / (60 * 60 * 24 * 1000))) > 0;

		return false;
	};

	model.DownloadExcel = function ()
	{
		// Build a form
		var form = $('<form></form>').attr('action', model.Options.DownloadExcelUrl).attr('method', 'POST');
		// Add key/value
		form.append($("<input></input>").attr('type', 'hidden').attr('name', "Context").attr('value', model.Filter.Context()));
		form.append($("<input></input>").attr('type', 'hidden').attr('name', "Sort").attr('value', model.Filter.Sort()));
		form.append($("<input></input>").attr('type', 'hidden').attr('name', "SortDirection").attr('value', model.Filter.SortDirection()));
		//send request
		form.appendTo('body').submit().remove();
	};

	model.OpenRequestFile = function (data)
	{
		// Build a form
		var form = $('<form></form>').attr('action', model.Options.DownloadRequestFileUrl + "/" + data.CurrentItem().ID()).attr('method', 'POST');
		//send request
		form.appendTo('body').submit().remove();
	};

	// инициализация //////////////////////////////////////////////////////////////////////////////////////////////////////////

	model.Filter.SortDirection(model.Filter.SortDirection() || "Asc");
	model.ApplyFilter();
}

$(function ()
{
	ko.applyBindings(new RequestsViewModel(modelData, {
		GetItemsUrl: app.urls.GetItems,
		UploadFileUrl: app.urls.UploadFile,
		GetNewRequestUrl: app.urls.GetNewRequest,
		DownloadExcelUrl: app.urls.DownloadExcel,
		DownloadRequestFileUrl: app.urls.DownloadRequestFile,
		SaveRequestUrl: app.urls.SaveRequest
	}), document.getElementById("ko-root"));
});