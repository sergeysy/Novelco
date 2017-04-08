var RolesViewModel = function (source, options) {
	var model = this;

	model.Options = $.extend({
		DeleteRoleUrl: null,
		GetNewRoleUrl: null,
		GetItemsUrl: null,
		SaveRoleUrl: null
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

	model.Filter.Context.subscribe(function () { model.ApplyFilter() });


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


	// #region create/edit modal //////////////////////////////////////////////////////////////////////////////////////////////

	var roleModalSelector = "#roleEditModal";

	model.OpenRoleCreate = function () {
		var data = null;
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.GetNewRoleUrl,
			success: function (response) {
				data = ko.mapping.fromJSON(response);
				model.Items.push(data);
			}
		});

		model.RoleEditModal.CurrentItem(data);
		$(roleModalSelector).modal("show");
		//$(roleModalSelector).draggable({ handle: ".modal-header" });
		model.RoleEditModal.OnClosed = function () { model.Items.remove(data); };
		model.RoleEditModal.Init();
	};

	model.OpenRoleEdit = function (data) {
		model.RoleEditModal.CurrentItem(data);
		$(roleModalSelector).modal("show");
		//$(bankModalSelector).draggable({ handle: ".modal-header" });;
		model.RoleEditModal.Init();
	};

	model.RoleEditModal = {
		CurrentItem: ko.observable(),
		Init: function () { },
		OnClosed: null,
		Close: function (self, e) {
			$(roleModalSelector).modal("hide");
			if (self.OnClosed != null)
				self.OnClosed();
		},
		Done: function (self, e) {
			// сохранить изменения
			model.SaveRole(self.CurrentItem());
			$(roleModalSelector).modal("hide");
		}
	};

	// #endregion

	model.DeleteRole = function (data) {
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.DeleteRoleUrl,
			data: { Id: data.Id() },
			success: function (response) {
				var r = JSON.parse(response);
				if (r.Message)
					alert(r.Message);
				else
					model.Items.remove(data);
			}
		});
	};

	model.SaveRole = function (data) {
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.SaveRoleUrl,
			data: {
				Id: data.Id(),
				Name: data.Name(),
				Description: data.Description()
			}
		});
	};



	// инициализация //////////////////////////////////////////////////////////////////////////////////////////////////////////

	model.Filter.SortDirection(model.Filter.SortDirection() || "Asc");
	model.ApplyFilter();

}

$(function () {
	ko.applyBindings(new RolesViewModel(modelData, {
		DeleteRoleUrl: app.urls.DeleteRole,
		GetNewRoleUrl: app.urls.GetNewRole,
		GetItemsUrl: app.urls.GetItems,
		SaveRoleUrl: app.urls.SaveRole
	}), document.getElementById("ko-root"));
});