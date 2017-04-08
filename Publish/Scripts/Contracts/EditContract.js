var ContractViewModel = function (source, options)
{
	var model = this;

	model.Options = $.extend({
		GetBankAccountsByOurLegalItemsUrl: null,
		GetBankAccountsByLegalItemsUrl: null,
		GetDocumentsByContractItemsUrl: null,
		GetContractMarksHistoryUrl: null,
		GetContractMarksUrl: null,
		OrderTemplatesUrl: null,
		GetNewDocumentUrl: null,
		UploadDocumentUrl: null,
		ViewDocumentUrl: null,

		ToggleContractOkUrl: null,
		ToggleContractCheckedUrl: null,
		ToggleContractBlockedUrl: null,
		ToggleContractRejectedUrl: null,

		SaveDocumentUrl: null,
		SaveContractUrl: null
	}, options);

	// поля ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	model.Contract = ko.mapping.fromJS(source);
	model.IsDirty = ko.observable(false);
	// Списки связанных сущностей
	model.DocumentsItems = ko.observableArray();
	model.CurrentContractMarks = ko.observable();
	model.OurBankAccounts = ko.observableArray();
	model.BankAccounts = ko.observableArray();
	// Справочники
	model.Dictionaries = model.Contract.Dictionaries;

	// вычисляемое/подписки ///////////////////////////////////////////////////////////////////////////////////////////////////

	model.IsCurrencyRateUseVisible = ko.pureComputed(function () { return model.GetCurrencyChecked(1) || (model.Contract.ContractServiceTypeId() == 1) || (model.Contract.ContractServiceTypeId() == 3) });

	model.Contract.OurLegalId.subscribe(function (newValue)
	{
		// получить счета по нашему юрлицу
		$.ajax({
			type: "POST",
			url: model.Options.GetBankAccountsByOurLegalItemsUrl,
			data: { OurLegalId: newValue },
			success: function (response) { model.OurBankAccounts(ko.mapping.fromJSON(response).Items()); }
		});
	});

	model.Contract.ContractTypeId.subscribe(function (newValue)
	{
		var type = ko.utils.arrayFirst(model.Dictionaries.ContractType(), function (item) { return item.ID() == newValue });
		model.Contract.OurContractRoleId(type.OurContractRoleId());
		model.Contract.ContractRoleId(type.ContractRoleId());
	});

	// функции ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	model.OrderTemplatesUrl = function (id) { return model.Options.OrderTemplatesUrl + "/?contractId=" + id; };

	model.ExtendContract = function (contract)
	{
		contract.IsSubscribed = false;
		contract.FieldsSubscription = function ()
		{
			var old = "";
			//one-time dirty flag that gives up its dependencies on first change
			var result = ko.computed(function ()
			{
				if (!contract.IsSubscribed)
				{
					// for subscriptions
					old += contract.Number();
					old += contract.OurLegalId();
					old += contract.PayMethodId();
					old += contract.BankAccountId();
					old += contract.ContractRoleId();
					old += contract.ContractTypeId();
					old += contract.PaymentTermsId();
					old += contract.OurBankAccountId();
					old += contract.OurContractRoleId();
					old += contract.CurrencyRateUseId();
					old += contract.ContractServiceTypeId();
					old += contract.IsProlongation();
					old += contract.Comment();
					old += contract.IsFixed();
					old += contract.BeginDate();
					old += contract.EndDate();
					old += contract.Date();
					old += contract.AgentPercentage();

					contract.IsSubscribed = true;
				}
				else
				{
					var newV = ko.toJS(contract);	// TEMP:
					model.IsDirty(true);
				}
			});

			return result;
		};

		contract.FieldsSubscription();
	};

	model.ExtendCurrencies = function ()
	{
		ko.utils.arrayForEach(model.Dictionaries.Currency(), function (item)
		{
			var cc = ko.utils.arrayFirst(model.Contract.Currencies(), function (cur) { return cur.CurrencyId() == item.ID() });
			if (!cc)
				model.Contract.Currencies.push({ CurrencyId: ko.observable(item.ID()), OurBankAccountId: ko.observable(), BankAccountId: ko.observable(), IsChecked: ko.observable(false) });
			else
				cc.IsChecked = ko.observable(true);
		});
	};

	model.ExtendDocuments = function (array, parent)
	{
		ko.utils.arrayForEach(array, function (item) { model.ExtendDocument(item, parent); })
	};

	model.ExtendDocument = function (document, parent)
	{
		document.IsDeleted = document.IsDeleted || ko.observable(false);
		document.IsDirty = document.IsDirty || ko.observable(false);

		document.IsDeleted.subscribe(function () { document.IsDirty(true) });

		document.IsDirty.subscribe(function (newValue) { parent.IsDirty(true) });
	};

	model.GetDocumentsByContract = function (contractId)
	{
		var id = ko.unwrap(contractId);
		$.ajax({
			type: "POST",
			url: model.Options.GetDocumentsByContractItemsUrl,
			data: { ContractId: id },
			success: function (response)
			{
				var list = ko.mapping.fromJSON(response).Items();
				model.ExtendDocuments(list, model);
				model.DocumentsItems(list);
			}
		});
	};

	model.ViewDocument = function (data)
	{
		var id = ko.unwrap(data.ID());
		window.open(model.Options.ViewDocumentUrl + "/" + id, "_blank");
	};

	model.SaveDocument = function (entity)
	{
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.SaveDocumentUrl,
			data: {
				ID: entity.ID(),
				ContractId: entity.ContractId(),
				UploadedBy: entity.UploadedBy(),
				UploadedDate: app.utility.SerializeDateTime(entity.UploadedDate()),
				Date: app.utility.SerializeDateTime(entity.Date()),
				DocumentTypeId: entity.DocumentTypeId(),
				Filename: entity.Filename(),
				FileSize: entity.FileSize(),
				IsPrint: entity.IsPrint(),
				IsNipVisible: entity.IsNipVisible(),
				Number: entity.Number(),
				OriginalSentDate: app.utility.SerializeDateTime(entity.OriginalSentDate()),
				OriginalReceivedDate: app.utility.SerializeDateTime(entity.OriginalReceivedDate()),

				IsDeleted: entity.IsDeleted()
			},
			success: function (response)
			{
				if (entity.ID() == 0)
					entity.ID(JSON.parse(response).ID);

				if (entity.IsDeleted())
					model.DocumentsItems.remove(entity);
			},
		});
	};

	model.DeleteDocument = function (document)
	{
		document.IsDeleted(true);
		model.SaveDocument(document);
	};

	//#region toggle contract marks ///////////////////////////////////////////////////////////////////////////////////////////

	model.ToggleContractOk = function ()
	{
		if (model.IsDirty())
		{
			alert("Пожалуйста, сначала сохраните внесенные изменения.");
			return;
		}

		$.ajax({
			type: "POST",
			url: model.Options.ToggleContractOkUrl,
			data: { ContractId: model.Contract.ID() },
			success: function (response)
			{
				var r = JSON.parse(response);
				if (r.Message)
					alert(r.Message);
				else
					model.CurrentContractMarks(ko.mapping.fromJSON(response));
			}
		});
	};

	model.ToggleContractChecked = function ()
	{
		if (model.IsDirty())
		{
			alert("Пожалуйста, сначала сохраните внесенные изменения.");
			return;
		}

		$.ajax({
			type: "POST",
			url: model.Options.ToggleContractCheckedUrl,
			data: { ContractId: model.Contract.ID() },
			success: function (response)
			{
				var r = JSON.parse(response);
				if (r.Message)
					alert(r.Message);
				else
					model.CurrentContractMarks(ko.mapping.fromJSON(response));
			}
		});
	};

	model.ToggleContractRejected = function ()
	{
		if (model.IsDirty())
		{
			alert("Пожалуйста, сначала сохраните внесенные изменения.");
			return;
		}

		$.ajax({
			type: "POST",
			url: model.Options.ToggleContractRejectedUrl,
			data: { ContractId: model.Contract.ID(), Comment: model.CurrentContractMarks().ContractRejectedComment() },
			success: function (response)
			{
				var r = JSON.parse(response);
				if (r.Message)
					alert(r.Message);
				else
					model.CurrentContractMarks(ko.mapping.fromJSON(response));
			}
		});
	};

	model.ToggleContractBlocked = function ()
	{
		if (model.IsDirty())
		{
			alert("Пожалуйста, сначала сохраните внесенные изменения.");
			return;
		}

		$.ajax({
			type: "POST",
			url: model.Options.ToggleContractBlockedUrl,
			data: { ContractId: model.Contract.ID(), Comment: model.CurrentContractMarks().ContractBlockedComment() },
			success: function (response)
			{
				var r = JSON.parse(response);
				if (r.Message)
					alert(r.Message);
				else
					model.CurrentContractMarks(ko.mapping.fromJS(r.Mark));

				if (r.InfoMessage)
					alert(r.InfoMessage);
			}
		});
	};

	//#endregion

	model.ValidateContract = function (contract)
	{
		var failed = ko.utils.arrayFirst(contract.Currencies(), function (item)
		{
			if (item.IsChecked() && (!item.OurBankAccountId() || !item.BankAccountId()))
			{
				alert("Не указан счет для выбранной валюты");
				return true;
			}
		});

		if (!contract.ContractTypeId())
		{
			alert("Не указан тип договора");
			failed = true;
		}

		if (!contract.ContractServiceTypeId())
		{
			alert("Не указан вид договора");
			failed = true;
		}

		if (model.IsCurrencyRateUseVisible())
			if ((model.Contract.ContractServiceTypeId() == 2) && (model.Contract.CurrencyRateUseId() != 3))
			{
				alert("Для договоров 'с поставщиком' используемый курс должен быть 'ЦБ РФ на день счета'");
				failed = true;
			}

		return !failed;
	};

	model.Save = function ()
	{
		if (!model.ValidateContract(model.Contract))
			return;

		// формирование DTO
		var data = {
			ID: model.Contract.ID(),
			LegalId: model.Contract.LegalId(),
			OurLegalId: model.Contract.OurLegalId(),
			ContractRoleId: model.Contract.ContractRoleId(),
			ContractTypeId: model.Contract.ContractTypeId(),
			PaymentTermsId: model.Contract.PaymentTermsId(),
			PayMethodId: model.Contract.PayMethodId(),
			OurContractRoleId: model.Contract.OurContractRoleId(),
			ContractServiceTypeId: model.Contract.ContractServiceTypeId(),
			Number: model.Contract.Number(),
			IsFixed: model.Contract.IsFixed(),
			IsProlongation: model.Contract.IsProlongation(),
			Date: app.utility.SerializeDateTime(model.Contract.Date()),
			BeginDate: app.utility.SerializeDateTime(model.Contract.BeginDate()),
			EndDate: app.utility.SerializeDateTime(model.Contract.EndDate()),
			Comment: model.Contract.Comment(),
			CurrencyRateUseId: model.Contract.CurrencyRateUseId(),
			AgentPercentage: (model.Contract.AgentPercentage() == undefined) ? "null" : model.Contract.AgentPercentage().toString().replace(/ /g, '').replace(/,/g, '.'),
			Currencies: []
		};

		ko.utils.arrayForEach(model.Contract.Currencies(), function (item) { if (item.IsChecked()) data.Currencies.push({ CurrencyId: item.CurrencyId(), OurBankAccountId: item.OurBankAccountId(), BankAccountId: item.BankAccountId() }) });

		$.ajax({
			type: "POST",
			url: model.Options.SaveContractUrl,
			data: JSON.stringify(data),
			dataType: "json",
			contentType: "application/json; charset=utf-8",
			success: function (response)
			{
				if (response && response.Message)
					alert(response.Message);
				else
				{
					model.IsDirty(false);
					//window.close();
				}
			}
		});
	};

	// #region document create/edit modal /////////////////////////////////////////////////////////////////////////////////////

	var documentModalSelector = "#documentEditModal";

	model.OpenContractDocumentCreate = function ()
	{
		var contractId = model.Contract.ID();
		var data = null;
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.GetNewDocumentUrl,
			data: { ContractId: contractId },
			success: function (response)
			{
				var r = JSON.parse(response);
				if (r.Message)
					alert(r.Message);
				else
				{
					data = ko.mapping.fromJSON(response);
					model.ExtendDocument(data, model.Order);
					model.DocumentsItems.push(data);
				}
			}
		});

		if (!data)
			return;

		model.DocumentEditModal.CurrentItem(data);
		$(documentModalSelector).modal("show");
		$(documentModalSelector).draggable({ handle: ".modal-header" });
		model.DocumentEditModal.Init();
	};

	model.OpenDocumentEdit = function (document)
	{
		model.DocumentEditModal.CurrentItem(document);
		$(documentModalSelector).modal("show");
		$(documentModalSelector).draggable({ handle: ".modal-header" });;
		model.DocumentEditModal.Init();
	};

	model.DocumentEditModal = {
		CurrentItem: ko.observable(),
		FileSize: ko.pureComputed(function () { if (model.DocumentEditModal.CurrentItem().FileSize()) return (model.DocumentEditModal.CurrentItem().FileSize() / (1024 * 1024)).toFixed(2); else return ""; }),
		OnClosed: null,
		Close: function (self, e)
		{
			$(documentModalSelector).modal("hide");
			if (self.OnClosed != null)
				self.OnClosed();
		},
		Init: function ()
		{
			// Стандарный input для файлов
			$('#documentUpload').on("change", function (e)
			{
				var formData = new FormData();
				formData.append("File", e.currentTarget.files[0]);
				var filename = e.currentTarget.files[0].name;
				var filesize = e.currentTarget.files[0].size;

				$.ajax({
					url: model.Options.UploadDocumentUrl + "/" + model.DocumentEditModal.CurrentItem().ID(),
					type: 'POST',
					data: formData,
					success: function (response)
					{
						var r = JSON.parse(response);
						model.DocumentEditModal.CurrentItem().Filename(filename);
						model.DocumentEditModal.CurrentItem().FileSize(filesize);
						model.DocumentEditModal.CurrentItem().UploadedDate(new Date());
						model.DocumentEditModal.CurrentItem().UploadedBy(r.CurrentUserId);
						alert("Файл успешно загружен");
					},
					cache: false,
					contentType: false,
					processData: false
				});
			});

			// Контейнер, куда можно помещать файлы методом drag and drop
			var dropBox = $('.fileDropable');
			dropBox.on("dragenter", function ()
			{
				$(this).addClass('highlighted');
				return false;
			});
			dropBox.on("dragover", function () { return false; });
			dropBox.on("dragleave", function ()
			{
				$(this).removeClass('highlighted');
				return false;
			});
			dropBox.on("drop", function (e)
			{
				$(this).removeClass('highlighted');
				var dt = e.originalEvent.dataTransfer;
				var formData = new FormData();
				formData.append("File", dt.files[0]);
				var filename = dt.files[0].name;
				var filesize = dt.files[0].size;

				$.ajax({
					url: model.Options.UploadDocumentUrl + "/" + model.DocumentEditModal.CurrentItem().ID(),
					type: 'POST',
					data: formData,
					success: function (response)
					{
						var r = JSON.parse(response);
						model.DocumentEditModal.CurrentItem().Filename(filename);
						model.DocumentEditModal.CurrentItem().FileSize(filesize);
						model.DocumentEditModal.CurrentItem().UploadedDate(new Date());
						model.DocumentEditModal.CurrentItem().UploadedBy(r.CurrentUserId);
						alert("Файл успешно загружен");
					},
					cache: false,
					contentType: false,
					processData: false
				});

				return false;
			});
		},
		Done: function (self, e)
		{
			// сохранить изменения
			model.SaveDocument(self.CurrentItem());
			$(documentModalSelector).modal("hide");
			self.CurrentItem(null);
		}
	};

	// #endregion

	// #region contract marks history modal //////////////////////////////////////////////////////////////////////////////////////

	var contractMarksHistoryModalSelector = "#contractMarksHistoryModal";

	model.OpenContractMarksHistory = function (document)
	{
		model.ContractMarksHistoryModal.CurrentItem(document);
		$(contractMarksHistoryModalSelector).modal("show");
		$(contractMarksHistoryModalSelector).draggable({ handle: ".modal-header" });;
		model.ContractMarksHistoryModal.Init();
	};

	model.ContractMarksHistoryModal = {
		CurrentItem: ko.observable(),
		OnClosed: null,
		Close: function (self, e)
		{
			$(contractMarksHistoryModalSelector).modal("hide");
			if (self.OnClosed != null)
				self.OnClosed();
		},
		Init: function ()
		{
			$.ajax({
				type: "POST",
				url: model.Options.GetContractMarksHistoryUrl,
				data: { ContractId: model.Contract.ID() },
				success: function (response) { model.ContractMarksHistoryModal.CurrentItem({ Items: ko.mapping.fromJSON(response).Items() }); }
			});
		},
		Done: function (self, e)
		{
			$(contractMarksHistoryModalSelector).modal("hide");
			self.CurrentItem(null);
		}
	};

	// #endregion

	model.GetCurrency = function (currencyId)
	{
		return ko.utils.arrayFirst(model.Contract.Currencies(), function (cur) { return cur.CurrencyId() == currencyId });
	};

	model.GetCurrencyChecked = function (currencyId)
	{
		var cc = ko.utils.arrayFirst(model.Contract.Currencies(), function (cur) { return cur.CurrencyId() == currencyId });
		return cc.IsChecked();
	};

	model.GetOurBankAccountsByCurrency = function (currencyId)
	{
		return ko.utils.arrayFilter(model.OurBankAccounts(), function (item) { return item.CurrencyId() == currencyId });
	};

	model.GetBankAccountsByCurrency = function (currencyId)
	{
		return ko.utils.arrayFilter(model.BankAccounts(), function (item) { return item.CurrencyId() == currencyId });
	};

	// инициализация //////////////////////////////////////////////////////////////////////////////////////////////////////////

	if (model.Contract.OurLegalId())
	{
		// получить счета по нашему юрлицу
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.GetBankAccountsByOurLegalItemsUrl,
			data: { OurLegalId: model.Contract.OurLegalId() },
			success: function (response) { model.OurBankAccounts(ko.mapping.fromJSON(response).Items()); }
		});
	}

	// получить счета по юрлицу
	$.ajax({
		type: "POST",
		async: false,
		url: model.Options.GetBankAccountsByLegalItemsUrl,
		data: { LegalId: model.Contract.LegalId() },
		success: function (response) { model.BankAccounts(ko.mapping.fromJSON(response).Items()); }
	});

	// получить метки
	$.ajax({
		type: "POST",
		url: model.Options.GetContractMarksUrl,
		data: { ContractId: model.Contract.ID() },
		success: function (response) { model.CurrentContractMarks(ko.mapping.fromJSON(response)); }
	});

	model.GetDocumentsByContract(model.Contract.ID);
	model.ExtendContract(model.Contract);
	model.ExtendCurrencies();

	window.onbeforeunload = function (evt)
	{
		if (model.IsDirty())
		{
			var message = "Есть несохраненные изменения. Если просто так уйти со страницы, то они не сохранятся.";
			if (typeof evt == "undefined")
				evt = window.event;

			if (evt)
				evt.returnValue = message;

			return message;
		}
	}
}

$(function ()
{
	ko.applyBindings(new ContractViewModel(modelData, {
		GetBankAccountsByOurLegalItemsUrl: app.urls.GetBankAccountsByOurLegalItems,
		GetBankAccountsByLegalItemsUrl: app.urls.GetBankAccountsByLegalItems,
		GetDocumentsByContractItemsUrl: app.urls.GetDocumentsByContractItems,
		GetContractMarksHistoryUrl: app.urls.GetContractMarksHistory,
		GetContractMarksUrl: app.urls.GetContractMarks,
		OrderTemplatesUrl: app.urls.OrderTemplates,
		GetNewDocumentUrl: app.urls.GetNewDocument,
		UploadDocumentUrl: app.urls.UploadDocument,
		ViewDocumentUrl: app.urls.ViewDocument,

		ToggleContractOkUrl: app.urls.ToggleContractOk,
		ToggleContractCheckedUrl: app.urls.ToggleContractChecked,
		ToggleContractRejectedUrl: app.urls.ToggleContractRejected,
		ToggleContractBlockedUrl: app.urls.ToggleContractBlocked,

		SaveDocumentUrl: app.urls.SaveDocument,
		SaveContractUrl: app.urls.SaveContract
	}), document.getElementById("ko-root"));
});