var ReportsViewModel = function (source, options) {
	var model = this;

	model.Options = $.extend({
		CreateDebtsReportUrl: null,
		CreateAccountingDetailedReportUrl: null,
		CreateAccountingPerOrderReportUrl: null
	}, options);

	// поля ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////



	// функции ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	model.CreateAccountingDetailedReport = function (data) {
		window.location = model.Options.CreateAccountingDetailedReportUrl;
	};

	model.CreateAccountingPerOrderReport = function (data) {
		window.location = model.Options.CreateAccountingPerOrderReportUrl;
	};

	model.CreateDebtsReport = function (data) {
		window.location = model.Options.CreateDebtsReportUrl;
	};
}

$(function () {
	ko.applyBindings(new ReportsViewModel(modelData, {
		CreateDebtsReportUrl: app.urls.CreateDebtsReport,
		CreateAccountingDetailedReportUrl: app.urls.CreateAccountingDetailedReport,
		CreateAccountingPerOrderReportUrl: app.urls.CreateAccountingPerOrderReport
	}), document.getElementById("ko-root"));
});