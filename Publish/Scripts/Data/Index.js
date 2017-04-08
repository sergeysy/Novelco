var DataViewModel = function (source, options) {
	var model = this;
	
	// поля ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	// Количество 
	model.BanksCount = source.BanksCount || 0;
	model.TemplatesCount = source.TemplatesCount || 0;
	model.PackageTypesCount = source.PackageTypesCount || 0;
	model.PaymentTermsCount = source.PaymentTermsCount || 0;
	model.ContractRolesCount = source.ContractRolesCount || 0;
	model.ContractTypesCount = source.ContractTypesCount || 0;
	model.CargoDescriptionsCount = source.CargoDescriptionsCount || 0;
	model.OurLegalsCount = source.OurLegalsCount || 0;
	model.UsersCount = source.UsersCount || 0;
	model.RolesCount = source.RolesCount || 0;
	model.OrderOperationsCount = source.OrderOperationsCount || 0;
	model.OrderTemplatesCount = source.OrderTemplatesCount || 0;
	model.PositionTemplatesCount = source.PositionTemplatesCount || 0;
	model.FinRepCentersCount = source.FinRepCentersCount || 0;

	// функции ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

}

$(function () {
	ko.applyBindings(new DataViewModel(modelData, {}), document.getElementById("ko-root"));
});