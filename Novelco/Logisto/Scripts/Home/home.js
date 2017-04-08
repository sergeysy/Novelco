var HomeViewModel = function (source, options) {
	var model = this;
	
	// поля ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	model.UsdRate = source.UsdRate || 0;
	model.EurRate = source.EurRate || 0;
	model.CnyRate = source.CnyRate || 0;
	model.GbpRate = source.GbpRate || 0;

	model.LastRateUpdated = source.LastRateUpdated;

	// функции ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

}

$(function () {
	ko.applyBindings(new HomeViewModel(modelData, {}), document.getElementById("ko-root"));
});