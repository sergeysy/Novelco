var EmployeeViewModel = function (source, options) {
	var model = this;

	model.Options = $.extend({
		GetNewPhoneUrl: null,
		SaveUrl: null
	}, options);

	// поля ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	model.Person = ko.mapping.fromJS(source);
	model.Login = ko.observable("");
	model.IsDirty = ko.observable(true);
	// Списки связанных сущностей
	model.PhonesItems = ko.observableArray();
	// Справочники
	model.Dictionaries = ko.mapping.fromJS(source.Dictionaries);

	// вычисляемое/подписки ///////////////////////////////////////////////////////////////////////////////////////////////////

	

	// функции ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	model.Validate = function (person)
	{
		// TODO:
		return true;
	};

	model.Save = function () {
		if (!model.Validate(model.Person))
			return;

		// формирование DTO
		var data = {
			Login: model.Login(),

			ID: model.Person.ID(),
			Family: model.Person.Family(),
			Name: model.Person.Name(),
			Patronymic: model.Person.Patronymic(),
			Initials: model.Person.Initials(),
			DisplayName: model.Person.DisplayName(),
			EnName: model.Person.EnName(),
			GenitiveFamily: model.Person.GenitiveFamily(),
			GenitiveName: model.Person.GenitiveName(),
			GenitivePatronymic: model.Person.GenitivePatronymic(),
			Address: model.Person.Address(),
			Email: model.Person.Email(),
			Comment: model.Person.Comment(),
			IsNotResident: model.Person.IsNotResident(),
			IsSubscribed: model.Person.IsSubscribed(),
			Birthday: model.Person.Birthday(),
			Phones: []
		};
		
		// телефоны
		ko.utils.arrayForEach(model.PhonesItems(), function (phone) {
				data.Phones.push({
					ID: phone.ID(),
					Name: phone.Name(),
					Number: phone.Number(),
					TypeId: phone.TypeId()
				});
		});

		$.ajax({
			type: "POST",
			url: model.Options.SaveUrl,
			data: JSON.stringify(data),
			dataType: "json",
			contentType: "application/json; charset=utf-8",
			success: function (response) {
				model.IsDirty(false);
				window.close();
			}
		});

	};

	// #region phone create/edit modal ////////////////////////////////////////////////////////////////////////////////////////

	var phoneModalSelector = "#phoneEditModal";

	model.DeletePhone = function (phone) {
		model.PhonesItems.remove(phone);
	};

	model.OpenPhoneCreate = function () {
		var data = null;
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.GetNewPhoneUrl,
			success: function (response) {
				data = ko.mapping.fromJSON(response);
				model.PhonesItems.push(data);
			}
		});

		model.PhoneEditModal.CurrentItem(data);
		$(phoneModalSelector).modal("show");
		$(phoneModalSelector).draggable({ handle: ".modal-header" });
		model.PhoneEditModal.OnClosed = function () { model.PhonesItems.remove(data); };
		model.PhoneEditModal.Init();
	};

	model.OpenPhoneEdit = function (phone) {
		model.PhoneEditModal.CurrentItem(phone);
		$(phoneModalSelector).modal("show");
		$(phoneModalSelector).draggable({ handle: ".modal-header" });;
		model.PhoneEditModal.Init();
	};

	model.PhoneEditModal = {
		CurrentItem: ko.observable(),
		Init: function () {	},
		OnClosed: null,
		Close: function (self, e) {
			$(phoneModalSelector).modal("hide");
			if (self.OnClosed != null)
				self.OnClosed();
		},
		Done: function (data, e) {
			$(phoneModalSelector).modal("hide");
		}
	};

	// #endregion

	// инициализация //////////////////////////////////////////////////////////////////////////////////////////////////////////

	window.onbeforeunload = function (evt) {
		if (model.IsDirty()) {
			var message = "Есть несохраненные изменения. Если просто так уйти со страницы, то они не сохранятся.";
			if (typeof evt == "undefined")
				evt = window.event;

			if (evt)
				evt.returnValue = message;

			return message;
		}
	}
}

$(function () {
	ko.applyBindings(new EmployeeViewModel(modelData, {
		GetNewPhoneUrl: app.urls.GetNewPhone,
		SaveUrl: app.urls.Save,
	}), document.getElementById("ko-root"));
});