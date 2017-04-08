var TemplatesViewModel = function (source, options) {
	var model = this;

	model.Options = $.extend({
		GetItemsUrl: null,
		GetTemplateFieldsItemsUrl: null,
		UploadTemplateUrl: null,
		SaveTemplateUrl: null,
		SaveTemplateFieldUrl: null,
		GetTemplateDataUrl: null
	}, options);

	// поля ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	// Список записей
	model.Items = ko.mapping.fromJS(source.Items);
	model.FieldsItems = ko.observableArray();

	// Справочники
	model.Dictionaries = ko.mapping.fromJS(source.Dictionaries);

	model.SelectedTemplate = ko.observable();

	// вычисляемое/подписки ///////////////////////////////////////////////////////////////////////////////////////////////////


	// функции ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	model.SelectTemplate = function (template) {
		model.SelectedTemplate(template);
		model.GetTemplateFields(template.ID);
	};

	model.GetTemplateFields = function (templateId) {
		var id = ko.unwrap(templateId);
		$.ajax({
			type: "POST",
			url: model.Options.GetTemplateFieldsItemsUrl,
			data: { TemplateId: id },
			success: function (response) { model.FieldsItems(ko.mapping.fromJSON(response).Items()); }
		});
	};

	model.SaveTemplate = function (template) {
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.SaveTemplateUrl,
			data: {
				ID: template.ID(),
				Filename: template.Filename(),
				FileSize: template.FileSize(),
				//SqlDataSource: template.SqlDataSource(),
				Name: template.Name(),
				Suffix: template.Suffix(),
				ListRow: template.ListRow(),
				ListFirstColumn: template.ListFirstColumn(),
				ListLastColumn: template.ListLastColumn(),
				xlfColumns: template.xlfColumns()
			}
		});
	};

	model.SaveTemplateField = function (field) {
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.SaveTemplateFieldUrl,
			data: {
				ID: field.ID(),
				Name: field.Name(),
				FieldName: field.FieldName(),
				IsAtable: field.IsAtable(),
				Range: field.Range()
			}
		});
	};

	// #region template create/edit modal /////////////////////////////////////////////////////////////////////////////////////

	var templateModalSelector = "#templateEditModal";

	model.DeleteTemplate = function (service) {
		service.IsDeleted(true);
		model.IsDirty(true);
	};

	model.OpenTemplateCreate = function () {
		var data = null;
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.GetNewTemplateUrl,
			data: { TemplateId: model.SelectedTemplate().ID() },
			success: function (response) {	}
		});

		model.TemplateEditModal.CurrentItem(data);
		$(templateModalSelector).modal("show");
		$(templateModalSelector).on("hide.bs.modal", function (e) {
			if (!model.TemplateEditModal.IsDone) 				
				model.SelectedAccounting().ServicesItems.remove(data);// удалить создаваемую запись

			$(templateModalSelector).off("hide.bs.modal");
		});

		$(templateModalSelector).draggable({ handle: ".modal-header" });
		model.TemplateEditModal.Init();
	};

	model.OpenTemplateEdit = function (service) {
		model.TemplateEditModal.CurrentItem(service);
		$(templateModalSelector).modal("show");
		$(templateModalSelector).draggable({ handle: ".modal-header" });;
		model.TemplateEditModal.Init();
	};

	model.TemplateEditModal = {
		CurrentItem: ko.observable(),
		IsDone: false,
		Init: function () {
			model.TemplateEditModal.IsDone = false;
			$('#upload').on('click', function () {
				if (document.getElementById("fileUpload").files.length == 0) {
					alert("Файл не выбран");
					return;
				}

				var formData = new FormData();
				formData.append("File", document.getElementById("fileUpload").files[0]);
				var filename = document.getElementById("fileUpload").files[0].name;
				var filesize = document.getElementById("fileUpload").files[0].size;

				$.ajax({
					url: model.Options.UploadTemplateUrl + "/" + model.TemplateEditModal.CurrentItem().ID(),
					type: 'POST',
					data: formData,
					success: function (response) {
						model.TemplateEditModal.CurrentItem().Filename(filename);
						model.TemplateEditModal.CurrentItem().FileSize(filesize);
					},
					cache: false,
					contentType: false,
					processData: false
				});
			});
		},
		Download: function () {
			window.location = model.Options.GetTemplateDataUrl + "/" + model.TemplateEditModal.CurrentItem().ID();
		},
		Done: function (data, e) {
			// сохранить изменения
			model.SaveTemplate(model.TemplateEditModal.CurrentItem());
			model.TemplateEditModal.IsDone = true;
			$(templateModalSelector).modal("hide");
		}
	};

	// #endregion

	// #region template field create/edit modal ///////////////////////////////////////////////////////////////////////////////

	var templateFieldModalSelector = "#templateFieldEditModal";

	model.OpenTemplateFieldEdit = function (data) {
		model.TemplateFieldEditModal.CurrentItem(data);
		$(templateFieldModalSelector).modal("show");
		//$(contractTypeModalSelector).draggable({ handle: ".modal-header" });;
		model.TemplateFieldEditModal.OnClosed = null;
		model.TemplateFieldEditModal.Init();
	};

	model.TemplateFieldEditModal = {
		CurrentItem: ko.observable(),
		OnClosed: null,
		Close: function (self, e) {
			$(templateFieldModalSelector).modal("hide");
			if (self.OnClosed != null)
				self.OnClosed();
		},
		Init: function () { },
		Done: function (self, e) {
			// сохранить изменения
			model.SaveTemplateField(self.CurrentItem());
			self.CurrentItem(null);
			$(templateFieldModalSelector).modal("hide");
		}
	};

	// #endregion

	// инициализация //////////////////////////////////////////////////////////////////////////////////////////////////////////
	
}

$(function () {
	ko.applyBindings(new TemplatesViewModel(modelData, {
		GetItemsUrl: app.urls.GetItems,
		GetTemplateFieldsItemsUrl: app.urls.GetTemplateFieldsItems,
		UploadTemplateUrl: app.urls.UploadTemplate,
		GetTemplateDataUrl: app.urls.GetTemplateData,
		SaveTemplateUrl: app.urls.SaveTemplate,
		SaveTemplateFieldUrl: app.urls.SaveTemplateField
	}), document.getElementById("ko-root"));
});