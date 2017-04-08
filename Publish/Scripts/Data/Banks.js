var BanksViewModel = function (source, options) {
	var model = this;

	model.Options = $.extend({
		UploadBicSwiftDataUrl: null,
		UploadBicDataUrl: null,
		GetItemsUrl: null,
		SaveBankUrl: null
	}, options);

	// поля ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	// Фильтр
	model.Filter = ko.mapping.fromJS(source);
	// Список записей
	model.Items = ko.observableArray();
	// Общее количество записей для текущего фильтра
	model.TotalItemsCount = ko.observable(0);

	// вычисляемое/подписки ///////////////////////////////////////////////////////////////////////////////////////////////////

	model.TotalPageCount = ko.computed(function () {
		return Math.ceil(model.TotalItemsCount() / model.Filter.PageSize());
	});

	model.Filter.Context.subscribe(function () { model.ApplyFilter() });


	// функции ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	model.SortBy = function (field) {
		var f = ko.unwrap(field);
		if (model.Filter.Sort() == f)
			model.Filter.SortDirection(model.Filter.SortDirection() == "Asc" ? "Desc" : "Asc");
		else
			model.Filter.Sort(f);

		model.ApplyFilter();
	};

	model.ApplyFilter = function () {
		$.ajax({
			type: "POST",
			url: model.Options.GetItemsUrl,
			data: {
				Context: model.Filter.Context() || "",
				PageSize: model.Filter.PageSize(),
				PageNumber: model.Filter.PageNumber(),
				Sort: model.Filter.Sort(),
				SortDirection: model.Filter.SortDirection()
			},
			success: function (response) {
				var temp = ko.mapping.fromJSON(response);
				model.Items(temp.Items());
				model.TotalItemsCount(temp.TotalCount());
				if (model.Filter.PageNumber() > model.TotalPageCount())
					model.Filter.PageNumber(0);
			}
		});
	};

	model.FirstPage = function () {
		if (model.Filter.PageNumber() > 0) {
			model.Filter.PageNumber(0);
			model.ApplyFilter();
		}
	};

	model.PrevPage = function () {
		if (model.Filter.PageNumber() > 0) {
			model.Filter.PageNumber(model.Filter.PageNumber() - 1);
			model.ApplyFilter();
		}
	};

	model.NextPage = function () {
		if (model.Filter.PageNumber() < (model.TotalPageCount() - 1)) {
			model.Filter.PageNumber(model.Filter.PageNumber() + 1);
			model.ApplyFilter();
		}
	};

	model.LastPage = function () {
		if (model.Filter.PageNumber() < (model.TotalPageCount() - 1)) {
			model.Filter.PageNumber(model.TotalPageCount() - 1);
			model.ApplyFilter();
		}
	};

	// #region create/edit modal //////////////////////////////////////////////////////////////////////////////////////////////

	var bankModalSelector = "#bankEditModal";

	model.OpenBankEdit = function (data) {
		model.BankEditModal.CurrentItem(data);
		$(bankModalSelector).modal("show");
		//$(bankModalSelector).draggable({ handle: ".modal-header" });;
		model.BankEditModal.Init();
	};

	model.BankEditModal = {
		CurrentItem: ko.observable(),
		IsDone: false,
		Init: function () {
			model.BankEditModal.IsDone = false;
		},
		Done: function (data, e) {
			// сохранить изменения
			model.SaveBank(model.BankEditModal.CurrentItem());

			//model.IsDirty(true);
			model.BankEditModal.IsDone = true;
			$(bankModalSelector).modal("hide");
		}
	};

	model.SaveBank = function (data) {
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.SaveBankUrl,
			data: {
				ID: data.ID(),
				Name: data.Name(),
				BIC: data.BIC(),
				KSNP: data.KSNP(),
				NNP: data.NNP(),
				PZN: data.PZN(),
				SWIFT: data.SWIFT(),
				TNP: data.TNP(),
				UER: data.UER()

				//IsDeleted: data.IsDeleted()
			}
		});
	};

	// #endregion

	model.ShowUploadForm = function (context, e) {
		$("#uploadForm").show();
		$("#uploadForm").attr('data-url', model.Options.UploadBicDataUrl);
		$("#buttonBanks").hide();
		$("#buttonSwift").hide();
	};

	model.ShowSwiftUploadForm = function (context, e) {
		$("#uploadForm").show();
		$("#uploadForm").attr('data-url', model.Options.UploadBicSwiftDataUrl);
		$("#buttonBanks").hide();
		$("#buttonSwift").hide();
	};

	// инициализация //////////////////////////////////////////////////////////////////////////////////////////////////////////

	model.Filter.SortDirection(model.Filter.SortDirection() || "Asc");
	model.ApplyFilter();

	$('#upload').on('click', function () {
		if (document.getElementById("fileUpload").files.length == 0) {
			alert("Файл не выбран");
			return;
		}

		var formData = new FormData();
		formData.append("File", document.getElementById("fileUpload").files[0]);

		$.ajax({
			url: $("#uploadForm").attr('data-url'),
			type: 'POST',
			data: formData,
			success: function (response) { alert("Готово"); },
			cache: false,
			contentType: false,
			processData: false
		});
	});
}

$(function () {
	ko.applyBindings(new BanksViewModel(modelData, {
		UploadBicSwiftDataUrl: app.urls.UploadBicSwiftData,
		UploadBicDataUrl: app.urls.UploadBicData,
		GetItemsUrl: app.urls.GetItems,
		SaveBankUrl: app.urls.SaveBank
	}), document.getElementById("ko-root"));
});