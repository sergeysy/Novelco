var EmployeesViewModel = function (source, options)
{
	var model = this;

	model.Options = $.extend({
		CreateEmployeeUrl: null,
		GetNewEmployeeUrl: null,
		SaveEmployeeUrl: null,
		CreatePersonUrl: null,
		EditPersonUrl: null,
		GetPersonsUrl: null,
		GetPersonUrl: null,
		GetItemsUrl: null,
		ResetUserPasswordUrl: null,
		GetUserRolesItemsUrl: null,
		UpdateUserRolesUrl: null,
		GetRolesItemsUrl: null,
		GetSignImageUrl: null,
		UploadPhotoUrl: null,
		UploadSignUrl: null,
		DeleteUserUrl: null,
		GetNewUserUrl: null,
		SaveUserUrl: null,
		GetUserUrl: null
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

	model.Filter.Context.subscribe(function () { model.ApplyFilter() });

	// функции ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	model.EmployeeDetailsUrl = function (id) { return model.Options.EmployeeDetailsUrl + "/" + id; };
	model.CreateEmployeeUrl = function () { return model.Options.CreateEmployeeUrl; };
	model.SignImageUrl = function (id) { return model.Options.GetSignImageUrl + "/" + id; };

	model.ExtendEmployees = function (array)
	{
		ko.utils.arrayForEach(array, function (item) { model.ExtendEmployee(item); })
	};

	model.ExtendEmployee = function (employee)
	{
		employee.IsDeleted = employee.IsDeleted || ko.observable(false);
		employee.IsDirty = employee.IsDirty || ko.observable(false);

		employee.IsDirty.subscribe(function (newValue) { if (newValue) model.IsDirty(true) });
		employee.IsDeleted.subscribe(function () { employee.IsDirty(true); });
	};

	model.ValidateEmployee = function (employee)
	{
		if (!employee.FinRepCenterId())
		{
			alert("Не выбран ЦФО!");
			return false;
		}

		return true;
	};

	model.SaveEmployee = function (employee)
	{
		if (!model.ValidateEmployee(employee))
			return;

		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.SaveEmployeeUrl,
			data: {
				ID: employee.ID(),
				LegalId: employee.LegalId(),
				ContractorId: employee.ContractorId(),
				FinRepCenterId: employee.FinRepCenterId(),
				Department: employee.Department(),
				Position: employee.Position(),
				GenitivePosition: employee.GenitivePosition(),
				Comment: employee.Comment(),
				Basis: employee.Basis(),
				EnPosition: employee.EnPosition(),
				EnBasis: employee.EnBasis(),
				PersonId: employee.PersonId(),
				BeginDate: app.utility.SerializeDateTime(employee.BeginDate()),
				EndDate: app.utility.SerializeDateTime(employee.EndDate()),
				IsDeleted: employee.IsDeleted()
			},
			success: function (response) { employee.IsDirty(false); }
		});
	};

	// #region employee create/edit modal /////////////////////////////////////////////////////////////////////////////////////

	var employeeModalSelector = "#employeeEditModal";

	model.DeleteEmployee = function (entity)
	{
		entity.IsDeleted(true);
		model.SaveEmployee(entity);
	};

	model.OpenEmployeeCreate = function ()
	{
		var data = null;
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.GetNewEmployeeUrl,
			data: { LegalId: 1 },	// TODO: выбор юрлица
			success: function (response)
			{
				data = ko.mapping.fromJSON(response);
				model.ExtendEmployee(data);
				model.Items.push(data);
			}
		});

		model.EmployeeEditModal.CurrentItem(data);
		$(employeeModalSelector).modal("show");
		//$(employeeModalSelector).draggable({ handle: ".modal-header" });
		model.EmployeeEditModal.OnClosed = function () { model.Items.remove(data); };
		model.EmployeeEditModal.Init();
	};

	model.OpenEmployeeEdit = function (entity)
	{
		model.EmployeeEditModal.CurrentItem(entity);
		$(employeeModalSelector).modal("show");
		//$(employeeModalSelector).draggable({ handle: ".modal-header" });
		model.EmployeeEditModal.OnClosed = null;
		model.EmployeeEditModal.Init();
	};

	model.EmployeeEditModal = {
		CurrentItem: ko.observable(),
		CurrentName: ko.observable(),
		CurrentPerson: ko.observable(),
		SelectedTemplate: ko.observable(),
		IsSelectTemplateVisible: ko.observable(false),
		ShowSelectTemplate: function () { model.EmployeeEditModal.IsSelectTemplateVisible(true); },
		Init: function ()
		{
			var self = model.EmployeeEditModal;
			if (self.CurrentItem().PersonId())
			{
				$.ajax({
					type: "POST",
					async: false,
					url: model.Options.GetPersonUrl,
					data: { Id: self.CurrentItem().PersonId() },
					success: function (response)
					{
						self.CurrentPerson(JSON.parse(response));
						self.CurrentName(JSON.parse(response).DisplayName);
					}
				});
			}

			$("#personAutocomplete").autocomplete({
				source: model.Options.GetPersonsUrl,
				appendTo: employeeModalSelector,
				select: function (e, ui)
				{
					model.EmployeeEditModal.CurrentPerson(ui.item.entity);
					model.EmployeeEditModal.CurrentName(ui.item.entity.DisplayName);
				}
			});
			self.SelectedTemplate.subscribe(function (newValue)
			{
				if (!newValue)
					return;

				var tmpl = ko.utils.arrayFirst(model.Dictionaries.PositionTemplate(), function (item) { return item.ID() == newValue });
				model.EmployeeEditModal.CurrentItem().Position(tmpl.Position());
				model.EmployeeEditModal.CurrentItem().EnPosition(tmpl.EnPosition());
				model.EmployeeEditModal.CurrentItem().GenitivePosition(tmpl.GenitivePosition());
				model.EmployeeEditModal.CurrentItem().Basis(tmpl.Basis());
				model.EmployeeEditModal.CurrentItem().EnBasis(tmpl.EnBasis());
				model.EmployeeEditModal.CurrentItem().Department(tmpl.Department());
				model.EmployeeEditModal.IsSelectTemplateVisible(false);
			});

			// Стандарный input для файлов
			$('#signUpload').on("change", function (e)
			{
				var formData = new FormData();
				formData.append("File", e.currentTarget.files[0]);
				var filename = e.currentTarget.files[0].name;
				var filesize = e.currentTarget.files[0].size;

				$.ajax({
					url: model.Options.UploadSignUrl + "/" + model.EmployeeEditModal.CurrentItem().ID(),
					type: 'POST',
					data: formData,
					success: function (response) { alert("Файл успешно загружен"); },
					cache: false,
					contentType: false,
					processData: false
				});
			});

			// Контейнер, куда можно помещать файлы методом drag and drop
			var dropBox = $('.fileDropable');
			dropBox.on("dragenter", function ()
			{
				$(this).addClass('highlighted');
				return false;
			});
			dropBox.on("dragover", function () { return false; });
			dropBox.on("dragleave", function ()
			{
				$(this).removeClass('highlighted');
				return false;
			});
			dropBox.on("drop", function (e)
			{
				$(this).removeClass('highlighted');
				var dt = e.originalEvent.dataTransfer;
				var formData = new FormData();
				formData.append("File", dt.files[0]);
				var filename = dt.files[0].name;
				var filesize = dt.files[0].size;

				$.ajax({
					url: model.Options.UploadPhotoUrl + "/" + model.UserEditModal.CurrentItem().Id,
					type: 'POST',
					data: formData,
					success: function (response)
					{
						alert("Файл успешно загружен");
					},
					cache: false,
					contentType: false,
					processData: false
				});

				return false;
			});
		},
		OnClosed: null,
		Close: function (self, e)
		{
			$(employeeModalSelector).modal("hide");
			if (self.OnClosed != null)
				self.OnClosed();
		},
		Done: function (self, e)
		{
			var person = self.CurrentPerson();
			if (person)
				self.CurrentItem().PersonId(person.ID);

			model.SaveEmployee(self.CurrentItem());
			self.CurrentItem(null);
			self.CurrentName(null);
			self.CurrentPerson(null);
			self.SelectedTemplate(null);
			$(employeeModalSelector).modal("hide");
		}
	};

	// #endregion

	model.CreatePerson = function (context) { window.open(model.Options.CreatePersonUrl, "_blank"); };

	model.EditPerson = function (context)
	{
		var url = model.Options.EditPersonUrl + "?personId=" + ko.unwrap(context.CurrentPerson().ID);
		window.open(url, "_blank");
	};

	// #region user create/edit modal /////////////////////////////////////////////////////////////////////////////////////////

	var userModalSelector = "#userEditModal";

	model.OpenUserEdit = function (data)
	{
		if (data.UserId())
		{
			$.ajax({
				type: "POST",
				url: model.Options.GetUserUrl + "/" + data.UserId(),
				success: function (response)
				{
					var data = ko.mapping.fromJSON(response);
					model.UserEditModal.CurrentItem(data);
					$(userModalSelector).modal("show");
					//$(userModalSelector).draggable({ handle: ".modal-header" });
					model.UserEditModal.Init();
				}
			});
		}
		else
			alert("Для этого сотрудника не задан пользователь (не выбрано соответствующее физлицо).");
	};

	model.UserEditModal = {
		CurrentItem: ko.observable(),
		CurrentSelection: ko.observableArray(),
		Roles: ko.observableArray(),
		IsChecked: function (context) { return ko.utils.arrayFirst(model.UserEditModal.CurrentSelection(), function (item) { return item == context.Id() }); },
		ToggleSelected: function (context, e)
		{
			var selected = ko.utils.arrayFirst(model.UserEditModal.CurrentSelection(), function (item) { return item == context.Id() });
			if (selected)
				model.UserEditModal.CurrentSelection.remove(selected);
			else
				model.UserEditModal.CurrentSelection.push(context.Id());

			return true;
		},
		ResetPassword: function (self) { model.ResetUserPassword(self.CurrentItem()) },
		Init: function ()
		{
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
				data: { UserId: model.UserEditModal.CurrentItem().Id() },
				success: function (response) { model.UserEditModal.CurrentSelection(ko.mapping.fromJSON(response)()); }
			});

			// Стандарный input для файлов
			$('#photoUpload').on("change", function (e)
			{
				var formData = new FormData();
				formData.append("File", e.currentTarget.files[0]);
				var filename = e.currentTarget.files[0].name;
				var filesize = e.currentTarget.files[0].size;

				$.ajax({
					url: model.Options.UploadPhotoUrl + "/" + model.UserEditModal.CurrentItem().Id(),
					type: 'POST',
					data: formData,
					success: function (response) { alert("Файл успешно загружен"); },
					cache: false,
					contentType: false,
					processData: false
				});
			});

			// Контейнер, куда можно помещать файлы методом drag and drop
			var dropBox = $('.fileDropable');
			dropBox.on("dragenter", function ()
			{
				$(this).addClass('highlighted');
				return false;
			});
			dropBox.on("dragover", function () { return false; });
			dropBox.on("dragleave", function ()
			{
				$(this).removeClass('highlighted');
				return false;
			});
			dropBox.on("drop", function (e)
			{
				$(this).removeClass('highlighted');
				var dt = e.originalEvent.dataTransfer;
				var formData = new FormData();
				formData.append("File", dt.files[0]);
				var filename = dt.files[0].name;
				var filesize = dt.files[0].size;

				$.ajax({
					url: model.Options.UploadPhotoUrl + "/" + model.UserEditModal.CurrentItem().Id,
					type: 'POST',
					data: formData,
					success: function (response)
					{
						alert("Файл успешно загружен");
					},
					cache: false,
					contentType: false,
					processData: false
				});

				return false;
			});
		},
		OnClosed: null,
		Close: function (self, e)
		{
			$(userModalSelector).modal("hide");
			if (self.OnClosed != null)
				self.OnClosed();
		},
		Done: function (self, e)
		{
			$(userModalSelector).modal("hide");
			self.OnClosed = null;
			// сохранить изменения
			model.SaveUser(self.CurrentItem(), self.CurrentSelection());
		}
	};

	// #endregion

	model.SaveUser = function (data, roles)
	{
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.SaveUserUrl,
			data: {
				Id: data.Id(),
				Login: data.Login(),
				Name: data.Name(),
				Email: data.Email(),
				PersonId: data.PersonId(),
				CrmId: data.CrmId()
			},
			success: function (response)
			{
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

	model.ResetUserPassword = function (data)
	{
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.ResetUserPasswordUrl,
			data: { UserId: data.Id() },
			success: function (response)
			{
				var r = JSON.parse(response);
				if (r.Message)
					alert(r.Message);
				else
					alert("Готово.");
			}
		});
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
				PageSize: model.Filter.PageSize(),
				PageNumber: model.Filter.PageNumber(),
				Type: model.Filter.Type(),
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

	// инициализация //////////////////////////////////////////////////////////////////////////////////////////////////////////

	model.Filter.SortDirection(model.Filter.SortDirection() || "Asc");
}

$(function ()
{
	ko.applyBindings(new EmployeesViewModel(modelData, {
		CreateEmployeeUrl: app.urls.CreateEmployee,
		GetNewEmployeeUrl: app.urls.GetNewEmployee,
		SaveEmployeeUrl: app.urls.SaveEmployee,
		CreatePersonUrl: app.urls.CreatePerson,
		EditPersonUrl: app.urls.EditPerson,
		GetPersonsUrl: app.urls.GetPersons,
		GetPersonUrl: app.urls.GetPerson,
		GetItemsUrl: app.urls.GetItems,
		GetUserRolesItemsUrl: app.urls.GetUserRoles,
		ResetUserPasswordUrl: app.urls.ResetUserPassword,
		UpdateUserRolesUrl: app.urls.UpdateUserRoles,
		GetRolesItemsUrl: app.urls.GetRoles,
		GetSignImageUrl: app.urls.GetSignImage,
		UploadPhotoUrl: app.urls.UploadPhoto,
		UploadSignUrl: app.urls.UploadSign,
		DeleteUserUrl: app.urls.DeleteUser,
		GetNewUserUrl: app.urls.GetNewUser,
		SaveUserUrl: app.urls.SaveUser,
		GetUserUrl: app.urls.GetUser
	}), document.getElementById("ko-root"));
});