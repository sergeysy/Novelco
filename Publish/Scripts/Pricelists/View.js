var PricelistViewModel = function (source, options)
{
	var model = this;

	model.Options = $.extend({
		GetPricelistDataUrl: null,
		EditPricelistUrl: null
	}, options);

	// поля ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	model.Pricelist = ko.mapping.fromJS(source);
	// Справочники
	model.Dictionaries = ko.mapping.fromJS(source.Dictionaries);


	// вычисляемое/подписки ///////////////////////////////////////////////////////////////////////////////////////////////////



	// функции ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	model.EditPricelist = function ()
	{
		window.open(model.Options.EditPricelistUrl + "/" + model.Pricelist.ID(), "_blank");
	}

	model.OpenData = function ()
	{
		window.open(model.Options.GetPricelistDataUrl + "/" + model.Pricelist.ID(), "_blank");
	}

	// инициализация //////////////////////////////////////////////////////////////////////////////////////////////////////////

}

$(function ()
{
	ko.applyBindings(new PricelistViewModel(modelData, {
		GetPricelistDataUrl: app.urls.GetPricelistData,
		EditPricelistUrl: app.urls.EditPricelist
	}), document.getElementById("ko-root"));
});