var LegalViewModel = function (source, dictionaries, options) {
	var model = this;

	model.Options = $.extend({
		SaveLegalUrl: null
	}, options);

	// поля ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	model.Legal = ko.mapping.fromJS(source);
	model.IsDirty = ko.observable(true);
	// Справочники
	model.Dictionaries = ko.mapping.fromJS(dictionaries);

	// вычисляемое/подписки ///////////////////////////////////////////////////////////////////////////////////////////////////

	// функции ////////////////////////////////////////////////////////////////////////////////////////////////////////////////



	model.Save = function () {
		var legal = model.Legal;

		var data = {
			ID: legal.ID(),
			ContractorId: legal.ContractorId(),
			TaxTypeId: legal.TaxTypeId(),
			DirectorId: legal.DirectorId(),
			AccountantId: legal.AccountantId(),
			Name: legal.Name(),
			DisplayName: legal.DisplayName(),
			EnName: legal.EnName(),
			EnShortName: legal.EnShortName(),
			TIN: legal.TIN(),
			OGRN: legal.OGRN(),
			KPP: legal.KPP(),
			OKPO: legal.OKPO(),
			OKVED: legal.OKVED(),
			Address: legal.Address(),
			EnAddress: legal.EnAddress(),
			AddressFact: legal.AddressFact(),
			EnAddressFact: legal.EnAddressFact(),
			WorkTime: legal.WorkTime(),
			TimeZone: legal.TimeZone(),
			IsNotResident: legal.IsNotResident()
		};

		$.ajax({
			type: "POST",
			url: model.Options.SaveLegalUrl,
			data: JSON.stringify(data),
			dataType: "json",
			contentType: "application/json; charset=utf-8",
			success: function (response) {
				model.IsDirty(false);
				window.close();
			}
		});

	};

	model.GetContractorDisplay = function () {
		var id = model.Legal.ContractorId();
		var result = "";
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.GetContractorUrl,
			data: { Id: id },
			success: function (response) { result = JSON.parse(response).Name; }
		});

		return result;
	};

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
	ko.applyBindings(new LegalViewModel(modelData, modelDictionaries, {
		SaveLegalUrl: app.urls.SaveLegal,
		GetContractorUrl: app.urls.GetContractor
	}), document.getElementById("ko-root"));
});