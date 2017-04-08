var PersonViewModel = function (source, options) {
	var model = this;

	model.Options = $.extend({
		GetNewPhoneUrl: null,
		GetPhonesItemsUrl: null,
		SavePersonUrl: null
	}, options);

	// поля ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	model.Person = ko.mapping.fromJS(source);
	model.IsDirty = ko.observable(true);
	// Списки связанных сущностей
	model.PhonesItems = ko.observableArray();
	// Справочники
	model.Dictionaries = ko.mapping.fromJS(source.Dictionaries);

	// вычисляемое/подписки ///////////////////////////////////////////////////////////////////////////////////////////////////

	

	// функции ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	model.GetPhones = function (personId) {
		var id = ko.unwrap(personId);
		$.ajax({
			type: "POST",
			url: model.Options.GetPhonesItemsUrl,
			data: { PersonId: id },
			success: function (response) {
				var list = ko.mapping.fromJSON(response).Items();
				model.ExtendPhones(list);
				model.PhonesItems(list);
			}
		});
	};

	model.Save = function () {
		// формирование DTO
		var data = {
			ID: model.Person.ID(),
			Family: model.Person.Family(),
			Name: model.Person.Name(),
			Patronymic: model.Person.Patronymic(),
			Initials: model.Person.Initials(),
			DisplayName: model.Person.DisplayName(),
			GenitiveFamily: model.Person.GenitiveFamily(),
			GenitiveName: model.Person.GenitiveName(),
			GenitivePatronymic: model.Person.GenitivePatronymic(),
			Address: model.Person.Address(),
			Email: model.Person.Email(),
			Comment: model.Person.Comment(),
			EnName: model.Person.EnName(),
			IsNotResident: model.Person.IsNotResident(),
			IsSubscribed: model.Person.IsSubscribed(),
			Birthday: model.Person.Birthday(),
			Phones: []
		};
		
		// телефоны
		ko.utils.arrayForEach(model.PhonesItems(), function (phone) {
			if (phone.IsDirty())
				data.Phones.push({
					ID: phone.ID(),
					Name: phone.Name(),
					Number: phone.Number(),
					TypeId: phone.TypeId(),
					
					IsDeleted: phone.IsDeleted()
				});
		});

		$.ajax({
			type: "POST",
			url: model.Options.SavePersonUrl,
			data: JSON.stringify(data),
			dataType: "json",
			contentType: "application/json; charset=utf-8",
			success: function (response) {
				model.IsDirty(false);
				window.close();
			}
		});

	};

	//#region Extenders ///////////////////////////////////////////////////////////////////////////////////////////////////////

	model.ExtendPhones = function (array, parent) {
		ko.utils.arrayForEach(array, function (item) { model.ExtendPhone(item, parent); })
	};

	model.ExtendPhone = function (phone) {
		phone.IsDeleted = phone.IsDeleted || ko.observable(false);
		phone.IsDirty = phone.IsDirty || ko.observable(false);

		phone.IsDirty.subscribe(function (newValue) { model.IsDirty(true) });

		phone.IsSubscribed = false;
		phone.FieldsSubscription = function () {
			var old = "";
			//one-time dirty flag that gives up its dependencies on first change
			var result = ko.computed(function () {
				if (!phone.IsSubscribed) {
					//just for subscriptions
					old += phone.Name();
					old += phone.Number();
					old += phone.TypeId();					

					old += phone.IsDeleted();

					//next time return true and avoid ko.toJS
					phone.IsSubscribed = true;
					return;
				}

				phone.IsDirty(true);
			});

			return result;
		};

		phone.FieldsSubscription();
	};

	// #endregion

	// #region phone create/edit modal ////////////////////////////////////////////////////////////////////////////////////////

	var phoneModalSelector = "#phoneEditModal";

	model.DeletePhone = function (phone) {
		phone.IsDeleted(true);
		model.IsDirty(true);
	};

	model.OpenPhoneCreate = function () {
		var data = null;
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.GetNewPhoneUrl,
			success: function (response) {
				data = ko.mapping.fromJSON(response);
				model.ExtendPhone(data);
				model.PhonesItems.push(data);
				data.IsDirty(true);
			}
		});

		model.PhoneEditModal.CurrentItem(data);
		$(phoneModalSelector).modal("show");
		$(phoneModalSelector).on("hide.bs.modal", function (e) {
			if (!model.PhoneEditModal.IsDone) {
				// удалить создаваемую запись
				model.PhonesItems.remove(data);
			}

			$(phoneModalSelector).off("hide.bs.modal");
		});
		$(phoneModalSelector).draggable({ handle: ".modal-header" });
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
		IsDone: false,
		Init: function () {
			model.PhoneEditModal.IsDone = false;

		},
		Done: function (data, e) {
			model.IsDirty(true);
			model.PhoneEditModal.IsDone = true;
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

	model.GetPhones(model.Person.ID());
}

$(function () {
	ko.applyBindings(new PersonViewModel(modelData, {
		GetNewPhoneUrl: app.urls.GetNewPhone,
		GetPhonesItemsUrl: app.urls.GetPhonesItems,
		SavePersonUrl: app.urls.SavePerson,
	}), document.getElementById("ko-root"));
});