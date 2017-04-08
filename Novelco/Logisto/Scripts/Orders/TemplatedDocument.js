var TemplatedDocumentViewModel = function (source, options)
{
	var model = this;

	model.Options = $.extend({
		AccountingDetailsUrl: null,
		GetTemplatedDocumentPreviewUrl: null,
		GetPrintTemplatedDocumentUrl: null,
		GetTemplatedDocumentDataUrl: null
	}, options);

	// поля ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	model.TemplatedDocument = ko.mapping.fromJS(source);

	// Справочники
	model.Dictionaries = ko.mapping.fromJS(source.Dictionaries);

	// вычисляемое/подписки ///////////////////////////////////////////////////////////////////////////////////////////////////


	// функции ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	model.AccountingUrl = function (id) { return model.Options.AccountingDetailsUrl + "/" + id; };

	model.LoadContent = function ()
	{
		$("#documentContent").html("<img src='" + model.Options.GetTemplatedDocumentPreviewUrl + "/" + model.TemplatedDocument.ID() + "' style='max-width:1300px' />");
	};

	model.Download = function ()
	{
		window.location = model.Options.GetTemplatedDocumentDataUrl + "/" + model.TemplatedDocument.ID() + "?type=xlsx";
	};

	model.DownloadPdf = function ()
	{
		window.location = model.Options.GetTemplatedDocumentDataUrl + "/" + model.TemplatedDocument.ID() + "?type=pdf";
	};

	model.DownloadCleanPdf = function ()
	{
		window.location = model.Options.GetTemplatedDocumentDataUrl + "/" + model.TemplatedDocument.ID() + "?type=cleanpdf";
	};

	model.DownloadCutPdf = function ()
	{
		window.location = model.Options.GetTemplatedDocumentDataUrl + "/" + model.TemplatedDocument.ID() + "?type=cutpdf";
	};

	model.PrintPdf = function ()
	{
		window.open(model.Options.GetPrintTemplatedDocumentUrl + "/" + model.TemplatedDocument.ID(), "_blank");
	};

	model.PrintCleanPdf = function ()
	{
		window.open(model.Options.GetPrintTemplatedDocumentUrl + "/" + model.TemplatedDocument.ID() + "?type=cleanpdf", "_blank");
	};

	model.PrintCutPdf = function ()
	{
		window.open(model.Options.GetPrintTemplatedDocumentUrl + "/" + model.TemplatedDocument.ID() + "?type=cutpdf", "_blank");
	};

	// инициализация //////////////////////////////////////////////////////////////////////////////////////////////////////////

	model.LoadContent();
}

$(function ()
{
	ko.applyBindings(new TemplatedDocumentViewModel(modelData, {
		AccountingDetailsUrl: app.urls.AccountingDetails,
		GetTemplatedDocumentPreviewUrl: app.urls.GetTemplatedDocumentPreview,
		GetPrintTemplatedDocumentUrl: app.urls.GetPrintTemplatedDocument,
		GetTemplatedDocumentDataUrl: app.urls.GetTemplatedDocumentData
	}), document.getElementById("ko-root"));
});