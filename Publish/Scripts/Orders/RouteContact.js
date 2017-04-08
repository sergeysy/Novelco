var RouteContactViewModel = function (source, options) {
	var model = this;

	model.Options = $.extend({
		GetPlaceUrl: null,
		GetPlacesUrl: null,
		SaveRouteContactUrl: null
	}, options);

	// поля ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	model.RouteContact = ko.mapping.fromJS(source);
	model.CurrentPlace = ko.observable();
	model.IsDirty = ko.observable(true);
	// Справочники
	model.Dictionaries = ko.mapping.fromJS(source.Dictionaries);

	// вычисляемое/подписки ///////////////////////////////////////////////////////////////////////////////////////////////////


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

	if (model.RouteContact.PlaceId()) {
		$.ajax({
			type: "POST",
			url: model.Options.GetPlaceUrl,
			data: { Id: model.RouteContact.PlaceId() },
			success: function (response) { $("#placeAutocomplete").val(JSON.parse(response).Name); }
		});
	}

	$("#placeAutocomplete").autocomplete({
		source: model.Options.GetPlacesUrl,
		select: function (e, ui) {
			model.RouteContact.PlaceId(ui.item.entity.ID);
			model.CurrentPlace(ui.item.entity);
		}
	});

	// функции ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	model.Save = function () {
		// формирование DTO
		var data = {
			ID: model.RouteContact.ID(),
			LegalId: model.RouteContact.LegalId(),
			PlaceId: model.RouteContact.PlaceId(),
			Name: model.RouteContact.Name(),
			Contact: model.RouteContact.Contact(),
			EnContact: model.RouteContact.EnContact(),
			Address: model.RouteContact.Address(),
			EnAddress: model.RouteContact.EnAddress(),
			Phones: model.RouteContact.Phones(),
			Email: model.RouteContact.Email()
		};

		$.ajax({
			type: "POST",
			url: model.Options.SaveRouteContactUrl,
			data: data,
			success: function (response) {
				model.IsDirty(false);
				window.close();
			}
		});
	};
}

$(function () {
	ko.applyBindings(new RouteContactViewModel(modelData, {
		GetPlaceUrl: app.urls.GetPlace,
		GetPlacesUrl: app.urls.GetPlaces,
		SaveRouteContactUrl: app.urls.SaveRouteContact,
	}), document.getElementById("ko-root"));
});