var ContractViewModel = function (source, options) {
	var model = this;

	model.Options = $.extend({
		ContractorDetailsUrl: null,
		ViewDocumentUrl: null,
		EditContractUrl: null
	}, options);

	// поля ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	model.Contract = ko.mapping.fromJS(source);


	// вычисляемое/подписки ///////////////////////////////////////////////////////////////////////////////////////////////////



	// функции ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	model.GotoContractorUrl = function (id) { return model.Options.ContractorDetailsUrl + "/" + id; };

	model.EditContract = function () {
		window.open(model.Options.EditContractUrl + "/" + model.Contract.ID(), "_blank");
	}
	
	model.ViewDocument = function (data)
	{
		var id = ko.unwrap(data.ID());
		window.open(model.Options.ViewDocumentUrl + "/" + id, "_blank");
	};

	// инициализация //////////////////////////////////////////////////////////////////////////////////////////////////////////
	
}

$(function () {
	ko.applyBindings(new ContractViewModel(modelData, {
		ContractorDetailsUrl: app.urls.ContractorDetails,
		ViewDocumentUrl: app.urls.ViewDocument,
		EditContractUrl: app.urls.EditContract
	}), document.getElementById("ko-root"));
});