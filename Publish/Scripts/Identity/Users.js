var UsersViewModel = function (source, options) {
	var model = this;

	model.Options = $.extend({
		ResetUserPasswordUrl: null,
		GetUserRolesItemsUrl: null,
		UpdateUserRolesUrl: null,
		GetRolesItemsUrl: null,
		DeleteUserUrl: null,
		GetNewUserUrl: null,
		GetItemsUrl: null,
		SaveUserUrl: null
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

	model.DeleteUser = function (data) {
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.DeleteUserUrl,
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


	// #region create/edit modal //////////////////////////////////////////////////////////////////////////////////////////////

	var userModalSelector = "#userEditModal";

	model.OpenUserCreate = function () {
		var data = null;
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.GetNewUserUrl,
			success: function (response) {
				data = ko.mapping.fromJSON(response);
				model.Items.push(data);
			}
		});

		model.UserEditModal.CurrentItem(data);
		$(userModalSelector).modal("show");
		//$(userModalSelector).draggable({ handle: ".modal-header" });
		model.UserEditModal.OnClosed = function () { model.Items.remove(data); };
		model.UserEditModal.Init();
	};

	model.OpenUserEdit = function (data) {
		model.UserEditModal.CurrentItem(data);
		$(userModalSelector).modal("show");
		//$(userModalSelector).draggable({ handle: ".modal-header" });;
		model.UserEditModal.Init();
	};

	model.UserEditModal = {
		CurrentItem: ko.observable(),
		CurrentSelection: ko.observableArray(),
		Roles: ko.observableArray(),
		IsChecked: function (context) {
			return ko.utils.arrayFirst(model.UserEditModal.CurrentSelection(), function (item) { return item == context.Id() });
		},
		ToggleSelected: function (context, e) {
			var selected = ko.utils.arrayFirst(model.UserEditModal.CurrentSelection(), function (item) { return item == context.Id() });
			if (selected)
				model.UserEditModal.CurrentSelection.remove(selected);
			else
				model.UserEditModal.CurrentSelection.push(context.Id());

			return true;
		},
		ResetPassword: function (self) { model.ResetUserPassword(self.CurrentItem()) },
		Init: function () {
			$.ajax({
				type: "POST",
				async: false,
				url: model.Options.GetRolesItemsUrl,
				data: { Sort: "Name", SortDirection: "Asc" },
				success: function (response) { model.UserEditModal.Roles(ko.mapping.fromJSON(response).Items()); }
			});
			$.ajax({
				type: "POST",
				async: false,
				url: model.Options.GetUserRolesItemsUrl,
				data: { UserId: model.UserEditModal.CurrentItem().Id },
				success: function (response) { model.UserEditModal.CurrentSelection(ko.mapping.fromJSON(response)()); }
			});
		},
		OnClosed: null,
		Close: function (self, e) {
			$(userModalSelector).modal("hide");
			if (self.OnClosed != null)
				self.OnClosed();
		},
		Done: function (self, e) {
			// сохранить изменения
			model.SaveUser(self.CurrentItem(), self.CurrentSelection());

			$(userModalSelector).modal("hide");
		}
	};

	// #endregion

	model.SaveUser = function (data, roles) {
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.SaveUserUrl,
			data: {
				Id: data.Id(),
				Login: data.Login(),
				Name: data.Name(),
				Email: data.Email(),
				PersonId: data.PersonId()
			},
			success: function (response) {
				$.ajax({
					type: "POST",
					async: false,
					url: model.Options.UpdateUserRolesUrl,
					data: {
						UserId: data.Id(),
						Roles: roles
					}
				});
			}
		});
	};

	model.ResetUserPassword = function (data) {
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.ResetUserPasswordUrl,
			data: { UserId: data.Id() },
			success: function (response) {
				var r = JSON.parse(response);
				if (r.Message)
					alert(r.Message);
				else
					alert("Готово.");
			}
		});
	};

	// инициализация //////////////////////////////////////////////////////////////////////////////////////////////////////////

	model.Filter.SortDirection(model.Filter.SortDirection() || "Asc");
	model.ApplyFilter();

}

$(function () {
	ko.applyBindings(new UsersViewModel(modelData, {
		GetUserRolesItemsUrl: app.urls.GetUserRoles,
		ResetUserPasswordUrl: app.urls.ResetUserPassword,
		UpdateUserRolesUrl: app.urls.UpdateUserRoles,
		GetRolesItemsUrl: app.urls.GetRoles,
		DeleteUserUrl: app.urls.DeleteUser,
		GetNewUserUrl: app.urls.GetNewUser,
		GetItemsUrl: app.urls.GetItems,
		SaveUserUrl: app.urls.SaveUser
	}), document.getElementById("ko-root"));
});