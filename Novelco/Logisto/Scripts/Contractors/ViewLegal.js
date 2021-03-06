﻿var LegalViewModel = function (source, options) {
	var model = this;

	model.Options = $.extend({
		EditLegalUrl: null
	}, options);

	// поля ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	model.Legal = ko.mapping.fromJS(source);


	// вычисляемое/подписки ///////////////////////////////////////////////////////////////////////////////////////////////////



	// функции ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	model.EditLegal = function () {
		window.open(model.Options.EditLegalUrl + "/" + model.Legal.ID(), "_blank");
	}

	// инициализация //////////////////////////////////////////////////////////////////////////////////////////////////////////



}

$(function () {
	ko.applyBindings(new LegalViewModel(modelData, {
		EditLegalUrl: app.urls.EditLegal
	}), document.getElementById("ko-root"));
});