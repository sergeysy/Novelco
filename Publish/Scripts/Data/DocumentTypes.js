var DocumentTypesViewModel = function (source, options)
{
	var model = this;

	model.Options = $.extend({
		GetNewDocumentTypeUrl: null,
		GetProductPrintsUrl: null,
		SaveDocumentTypeUrl: null
	}, options);

	// поля ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	// Список записей
	model.Items = ko.mapping.fromJS(source);
	model.CurrentSelection = ko.observableArray();


	// функции ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	model.ExtendDocumentType = function (item)
	{
		item.ProductPrints = ko.observableArray();

		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.GetProductPrintsUrl,
			data: { DocumentTypeId: item.ID() },
			success: function (response) { item.ProductPrints(ko.mapping.fromJSON(response).Items()); }
		});

		item.IsPrint = ko.computed(function ()
		{
			var result = "";
			ko.utils.arrayForEach(item.ProductPrints(), function (e)
			{
				if (e.Checked())
					if (!result)
						result = e.Display();
					else
						result = result + ', ' + e.Display();
			});
			return result;
		});
	};
	
	model.SaveDocumentType = function (documentType, prints)
	{
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.SaveDocumentTypeUrl,
			data: {
				ID: documentType.ID(),
				Display: documentType.Display(),
				Description: documentType.Description(),
				EnDescription: documentType.EnDescription(),
				IsNipVisible: documentType.IsNipVisible(),
				Prints: prints
			},
			success: function (response)
			{
				var id = JSON.parse(response).ID;
				if (id)
					documentType.ID(id);

				// update prints display
				$.ajax({
					type: "POST",
					url: model.Options.GetProductPrintsUrl,
					data: { DocumentTypeId: documentType.ID() },
					success: function (response) { documentType.ProductPrints(ko.mapping.fromJSON(response).Items()); }
				});
			}
		});
	};

	// #region template create/edit modal /////////////////////////////////////////////////////////////////////////////////////

	var documentTypeModalSelector = "#documentTypeEditModal";

	model.OpenDocumentTypeCreate = function ()
	{
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.GetNewDocumentTypeUrl,
			success: function (response)
			{
				var data = ko.mapping.fromJSON(response);
				model.ExtendDocumentType(data);
				model.Items.push(data);
				model.DocumentTypeEditModal.CurrentItem(data);
				$(documentTypeModalSelector).modal("show");
				$(documentTypeModalSelector).draggable({ handle: ".modal-header" });
				model.DocumentTypeEditModal.OnClosed = function () { model.Items.remove(data); };
				model.DocumentTypeEditModal.Init();
			}
		});

	};

	model.OpenDocumentTypeEdit = function (service)
	{
		model.DocumentTypeEditModal.CurrentItem(service);
		$(documentTypeModalSelector).modal("show");
		$(documentTypeModalSelector).draggable({ handle: ".modal-header" });;
		model.DocumentTypeEditModal.Init();
	};

	model.DocumentTypeEditModal = {
		CurrentItem: ko.observable(),
		CurrentSelection: ko.observableArray(),
		ProductPrints: ko.observableArray(),
		IsChecked: function (data) { return ko.utils.arrayFirst(model.DocumentTypeEditModal.CurrentSelection(), function (item) { return item == data.ID() }); },
		ToggleSelected: function (data, e)
		{
			var selected = ko.utils.arrayFirst(model.DocumentTypeEditModal.CurrentSelection(), function (item) { return item == data.ID() });
			if (selected)
				model.DocumentTypeEditModal.CurrentSelection.remove(selected);
			else
				model.DocumentTypeEditModal.CurrentSelection.push(data.ID());

			return true;
		},
		IsDone: false,
		Init: function ()
		{
			$.ajax({
				type: "POST",
				async: false,
				url: model.Options.GetProductPrintsUrl,
				data: { DocumentTypeId: model.DocumentTypeEditModal.CurrentItem().ID() },
				success: function (response)
				{
					model.DocumentTypeEditModal.ProductPrints(ko.mapping.fromJSON(response).Items());
					ko.utils.arrayForEach(model.DocumentTypeEditModal.ProductPrints(), function (item) { if (item.Checked()) model.DocumentTypeEditModal.CurrentSelection.push(item.ID()); });
				}
			});
		},
		OnClosed: null,
		Close: function (self, e)
		{
			$(documentTypeModalSelector).modal("hide");
			if (self.OnClosed != null)
				self.OnClosed();
		},
		Done: function (self, e)
		{
			$(documentTypeModalSelector).modal("hide");
			// сохранить изменения
			model.SaveDocumentType(self.CurrentItem(), self.CurrentSelection());
			self.CurrentItem(null);
			self.CurrentSelection([]);
			self.ProductPrints([]);
		}
	};

	// #endregion

	// вычисляемое/подписки ///////////////////////////////////////////////////////////////////////////////////////////////////

	ko.utils.arrayForEach(model.Items(), function (item)
	{
		model.ExtendDocumentType(item);
	});

	// инициализация //////////////////////////////////////////////////////////////////////////////////////////////////////////

}

$(function ()
{
	ko.applyBindings(new DocumentTypesViewModel(modelData, {
		GetNewDocumentTypeUrl: app.urls.GetNewDocumentType,
		GetProductPrintsUrl: app.urls.GetProductPrints,
		SaveDocumentTypeUrl: app.urls.SaveDocumentType
	}), document.getElementById("ko-root"));
});