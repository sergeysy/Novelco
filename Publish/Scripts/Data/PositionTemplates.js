var PositionTemplatesViewModel = function (source, options) {
	var model = this;

	model.Options = $.extend({
		GetNewPositionTemplateUrl: null,
		DeletePositionTemplateUrl: null,
		SavePositionTemplateUrl: null
	}, options);

	// поля ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	// Список записей
	model.Items = ko.mapping.fromJS(source);
	// поиск/фильтр
	model.FilterContext = ko.observable("");

	// вычисляемое/подписки ///////////////////////////////////////////////////////////////////////////////////////////////////


	// функции ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	model.IsVisible = function (item) {
		if (!model.FilterContext())
			return true;

		var context = model.FilterContext().toLowerCase();
		if (item.Position())
			if (item.Position().toLowerCase().indexOf(context) > -1)
				return true;

		if (item.Basis())
			if (item.Basis().toLowerCase().indexOf(context) > -1)
				return true;

		if (item.EnPosition())
			if (item.EnPosition().toLowerCase().indexOf(context) > -1)
				return true;

		if (item.EnBasis())
			if (item.EnBasis().toLowerCase().indexOf(context) > -1)
				return true;

		if (item.Department())
			if (item.Department().toLowerCase().indexOf(context) > -1)
				return true;

		return false;
	};

	model.SavePositionTemplate = function (data) {
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.SavePositionTemplateUrl,
			data: {
				ID: data.ID(),
				Position: data.Position(),
				Basis: data.Basis(),
				EnBasis: data.EnBasis(),
				EnPosition: data.EnPosition(),
				Department: data.Department(),
				GenitivePosition: data.GenitivePosition()
			}
		});
	};

	model.DeletePositionTemplate = function (data) {
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.DeletePositionTemplateUrl,
			data: { id: data.ID() },
			success: function (response) {
				var r = JSON.parse(response);
				if (r.Message)
					alert(r.Message);
				else
					model.Items.remove(data);
			}
		});
	};

	// #region PositionTemplate create/edit modal /////////////////////////////////////////////////////////////////////////////

	var positionTemplateModalSelector = "#positionTemplateEditModal";

	model.OpenPositionTemplateCreate = function () {
		var data = null;
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.GetNewPositionTemplateUrl,
			success: function (response) {
				data = ko.mapping.fromJSON(response);
				model.Items.push(data);
			}
		});

		model.PositionTemplateEditModal.CurrentItem(data);
		$(positionTemplateModalSelector).modal("show");
		//$().draggable({ handle: ".modal-header" });
		model.PositionTemplateEditModal.OnClosed = function () { model.Items.remove(data); };
		model.PositionTemplateEditModal.Init();
	};

	model.OpenPositionTemplateEdit = function (operation) {
		model.PositionTemplateEditModal.CurrentItem(operation);
		$(positionTemplateModalSelector).modal("show");
		//$().draggable({ handle: ".modal-header" });;
		model.PositionTemplateEditModal.Init();
	};

	model.PositionTemplateEditModal = {
		CurrentItem: ko.observable(),
		Init: function () { },
		OnClosed: null,
		Close: function (self, e) {
			$(positionTemplateModalSelector).modal("hide");
			if (self.OnClosed != null)
				self.OnClosed();
		},
		Done: function (self, e) {
			// сохранить изменения
			model.SavePositionTemplate(self.CurrentItem());

			$(positionTemplateModalSelector).modal("hide");
		}
	};

	// #endregion


	// инициализация //////////////////////////////////////////////////////////////////////////////////////////////////////////

}

$(function () {
	ko.applyBindings(new PositionTemplatesViewModel(modelData, {
		GetNewPositionTemplateUrl: app.urls.GetNewPositionTemplate,
		DeletePositionTemplateUrl: app.urls.DeletePositionTemplate,
		SavePositionTemplateUrl: app.urls.SavePositionTemplate
	}), document.getElementById("ko-root"));
});