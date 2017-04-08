var DocumentViewModel = function (source, options)
{
	var model = this;

	model.Options = $.extend({
		GetDocumentPreviewUrl: null,
		GetDocumentDataUrl: null
	}, options);

	// поля ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	model.Document = ko.mapping.fromJS(source);

	// Справочники
	model.Dictionaries = ko.mapping.fromJS(source.Dictionaries);

	// вычисляемое/подписки ///////////////////////////////////////////////////////////////////////////////////////////////////



	// функции ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	model.LoadContent = function ()
	{
		var filename = model.Document.Filename();
		var ext = filename.split('.').pop();
		switch (ext.toLowerCase())
		{
			case "jpg":
			case "jpeg":
			case "png":
				$("#documentContent").html("<img src='" + model.Options.GetDocumentDataUrl + "/" + model.Document.ID() + "' style='max-width:1300px' />");
				break;

			case "xls":
			case "xlsx":
			case "pdf":
			case "doc":
			case "docx":
				$("#documentContent").html("<img src='" + model.Options.GetDocumentPreviewUrl + "/" + model.Document.ID() + "' style='max-width:1300px' />");

				// страница 1
				var img1 = $("<img />").attr('src', model.Options.GetDocumentPreviewUrl + "/" + model.Document.ID() + "?pageNo=1")
										.on('load', function ()
										{
											if (this.complete && typeof this.naturalWidth !== "undefined" && this.naturalWidth !== 0)
												$("#documentContent").append(img1);
										});

				// страница 2
				var img2 = $("<img />").attr('src', model.Options.GetDocumentPreviewUrl + "/" + model.Document.ID() + "?pageNo=2")
										.on('load', function ()
										{
											if (this.complete && typeof this.naturalWidth !== "undefined" && this.naturalWidth !== 0)
												$("#documentContent").append(img2);
										});

				// страница 3
				var img3 = $("<img />").attr('src', model.Options.GetDocumentPreviewUrl + "/" + model.Document.ID() + "?pageNo=3")
										.on('load', function ()
										{
											if (this.complete && typeof this.naturalWidth !== "undefined" && this.naturalWidth !== 0)
												$("#documentContent").append(img3);
										});

				// страница 4
				var img4 = $("<img />").attr('src', model.Options.GetDocumentPreviewUrl + "/" + model.Document.ID() + "?pageNo=4")
										.on('load', function ()
										{
											if (this.complete && typeof this.naturalWidth !== "undefined" && this.naturalWidth !== 0)
												$("#documentContent").append(img4);
										});

				// страница 5
				var img5 = $("<img />").attr('src', model.Options.GetDocumentPreviewUrl + "/" + model.Document.ID() + "?pageNo=5")
										.on('load', function ()
										{
											if (this.complete && typeof this.naturalWidth !== "undefined" && this.naturalWidth !== 0)
												$("#documentContent").append(img5);
										});
				// страница 6
				var img6 = $("<img />").attr('src', model.Options.GetDocumentPreviewUrl + "/" + model.Document.ID() + "?pageNo=6")
										.on('load', function ()
										{
											if (this.complete && typeof this.naturalWidth !== "undefined" && this.naturalWidth !== 0)
												$("#documentContent").append(img6);
										});
				// страница 7
				var img7 = $("<img />").attr('src', model.Options.GetDocumentPreviewUrl + "/" + model.Document.ID() + "?pageNo=7")
										.on('load', function ()
										{
											if (this.complete && typeof this.naturalWidth !== "undefined" && this.naturalWidth !== 0)
												$("#documentContent").append(img7);
										});
				// страница 8
				var img8 = $("<img />").attr('src', model.Options.GetDocumentPreviewUrl + "/" + model.Document.ID() + "?pageNo=8")
										.on('load', function ()
										{
											if (this.complete && typeof this.naturalWidth !== "undefined" && this.naturalWidth !== 0)
												$("#documentContent").append(img8);
										});
				// страница 9
				var img9 = $("<img />").attr('src', model.Options.GetDocumentPreviewUrl + "/" + model.Document.ID() + "?pageNo=9")
										.on('load', function ()
										{
											if (this.complete && typeof this.naturalWidth !== "undefined" && this.naturalWidth !== 0)
												$("#documentContent").append(img9);
										});

				break;

			default:
				$("#unsupported").css("display", "");
				break;
		}

		setTimeout(function ()
		{
			$("#documentContent img").draggable();
			$("#documentContent img").click(function ()
			{
				if (!$(this).hasClass("rotated90"))
					$(this).addClass("rotated90");
				else if (!$(this).hasClass("rotated180"))
					$(this).addClass("rotated180");
				else if (!$(this).hasClass("rotated270"))
					$(this).addClass("rotated270");
				else
					$(this).removeClass("rotated90 rotated180 rotated270");
			});
		}, 2000);
	};

	model.IsPdf = function ()
	{
		var filename = model.Document.Filename();
		var ext = filename.split('.').pop();
		return ext.toLowerCase() == "pdf";
	};

	model.Download = function ()
	{
		window.location = model.Options.GetDocumentDataUrl + "/" + model.Document.ID();
	};

	model.Print = function ()
	{
		window.open(model.Options.GetPrintDocumentUrl + "/" + model.Document.ID(), "_blank");
	};

	// инициализация //////////////////////////////////////////////////////////////////////////////////////////////////////////

	model.LoadContent();
}

$(function ()
{
	ko.applyBindings(new DocumentViewModel(modelData, {
		GetDocumentPreviewUrl: app.urls.GetDocumentPreview,
		GetPrintDocumentUrl: app.urls.GetPrintDocument,
		GetDocumentDataUrl: app.urls.GetDocumentData
	}), document.getElementById("ko-root"));
});