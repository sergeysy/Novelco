var OurLegalsViewModel = function (source, dictionaries, options) {
	var model = this;

	model.Options = $.extend({
		GetBankAccountsByLegalItemsUrl: null,
		GetEmployeesByLegalItemsUrl: null,
		GetNewBankAccountUrl: null,
		DownloadOurLegalUrl: null,
		SaveBankAccountUrl: null,
		UploadOurLegalUrl: null,
		LegalDetailsUrl: null,
		SaveOurLegalUrl: null,
		SaveLegalUrl: null,
		GetLegalUrl: null,
		GetBanksUrl: null,
		GetBankUrl: null
	}, options);

	// поля ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	// Список записей
	model.Items = ko.mapping.fromJS(source);
	model.EmployeesItems = ko.observableArray();
	model.BankAccountsItems = ko.observableArray();
	// Справочники
	model.Dictionaries = ko.mapping.fromJS(dictionaries);
	//
	model.SelectedOurLegal = ko.observable();
	model.SelectedLegal = ko.observable();

	// вычисляемое/подписки ///////////////////////////////////////////////////////////////////////////////////////////////////


	// функции ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	model.SelectOurLegal = function (entity) {
		model.SelectedOurLegal(entity);
		model.SelectLegal(entity.ID, entity.LegalId);
	};

	model.SelectLegal = function (ourLegalId, legalId) {
		var id = ko.unwrap(legalId);

		$.ajax({
			type: "POST",
			url: model.Options.GetLegalUrl,
			data: { Id: id },
			success: function (response) {
				model.SelectedLegal(ko.mapping.fromJSON(response));
				$("#sign").html("<img src='" + model.Options.DownloadOurLegalUrl + "/" + ko.unwrap(ourLegalId) + "' />");
			}
		});

		model.EmployeesItems(model.GetEmployeesByLegal(id));
		model.LoadBankAccountsByLegal(id);
	};

	model.SaveOurLegal = function (data) {
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.SaveOurLegalUrl,
			data: {
				ID: data.ID(),
				Name: data.Name(),
				LegalId: data.LegalId()
			}
		});
	};

	model.SaveLegal = function (legal) {
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.SaveLegalUrl,
			data: {
				ID: legal.ID(),
				TaxTypeId: legal.TaxTypeId(),
				DirectorId: legal.DirectorId(),
				AccountantId: legal.AccountantId(),
				Name: legal.Name(),
				DisplayName: legal.DisplayName(),
				EnName: legal.EnName(),
				EnShortName: legal.EnShortName(),
				TIN: legal.TIN(),
				OGRN: legal.OGRN(),
				KPP: legal.KPP(),
				OKPO: legal.OKPO(),
				OKVED: legal.OKVED(),
				Address: legal.Address(),
				EnAddress: legal.EnAddress(),
				AddressFact: legal.AddressFact(),
				EnAddressFact: legal.EnAddressFact(),
				WorkTime: legal.WorkTime(),
				IsNotResident: legal.IsNotResident()
			},
			success: function () { alert("Готово.") }
		});
	};

	model.SaveBankAccount = function (account) {
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.SaveBankAccountUrl,
			data: {
				ID: account.ID(),
				LegalId: account.LegalId(),
				BankId: account.BankId(),
				CurrencyId: account.CurrencyId(),
				Number: account.Number(),
				CoBankName: account.CoBankName(),
				CoBankAccount: account.CoBankAccount(),
				CoBankSWIFT: account.CoBankSWIFT(),
				CoBankAddress: account.CoBankAddress(),
				CoBankIBAN: account.CoBankIBAN(),
				IsDeleted: account.IsDeleted()
			},
			success: function (response) { account.IsDirty(false); }
		});
	};

	model.GetEmployeesByLegal = function (legalId) {
		var id = ko.unwrap(legalId);
		var result = null;
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.GetEmployeesByLegalItemsUrl,
			data: { LegalId: id },
			success: function (response) {
				result = ko.mapping.fromJSON(response).Items();
				model.ExtendEmployees(result, ko.utils.arrayFirst(model.LegalsItems(), function (item) { return item.ID() == id }));
			}
		});
		return result;
	};
	
	model.GetEmployeeName = function (array, id) {
		var emp = ko.utils.arrayFirst(array(), function (item) { return item.ID() == id() });
		if (emp)
			return emp.Name();

		return "";
	};

	model.LoadBankAccountsByLegal = function (legalId) {
		var id = ko.unwrap(legalId);
		$.ajax({
			type: "POST",
			url: model.Options.GetBankAccountsByLegalItemsUrl,
			data: { LegalId: id },
			success: function (response) { model.BankAccountsItems(ko.mapping.fromJSON(response).Items()); }
		});
	};

	model.GotoLegalEdit = function (data) {
		window.open(model.Options.LegalDetailsUrl + "/" + data.LegalId(), "_blank");
	};

	// #region our legal create/edit modal ////////////////////////////////////////////////////////////////////////////////////

	var ourLegalModalSelector = "#ourLegalEditModal";

	model.OpenOurLegalEdit = function (data) {
		model.OurLegalEditModal.CurrentItem(data);
		$(ourLegalModalSelector).modal("show");
		//$(contractTypeModalSelector).draggable({ handle: ".modal-header" });;
		model.OurLegalEditModal.Init();
	};

	model.OurLegalEditModal = {
		CurrentItem: ko.observable(),
		Init: function () {
			$('#upload').on('click', function () {
				if (document.getElementById("fileUpload").files.length == 0) {
					alert("Файл не выбран");
					return;
				}

				var formData = new FormData();
				formData.append("File", document.getElementById("fileUpload").files[0]);
				var filename = document.getElementById("fileUpload").files[0].name;
				var filesize = document.getElementById("fileUpload").files[0].size;

				$.ajax({
					type: 'POST',
					url: model.Options.UploadOurLegalUrl + "/" + model.OurLegalEditModal.CurrentItem().ID(),
					data: formData,
					cache: false,
					contentType: false,
					processData: false,
					success: function () { alert("Готово!") }
				});
			});
		},
		Download: function () {
			window.location = model.Options.DownloadOurLegalUrl + "/" + model.OurLegalEditModal.CurrentItem().ID();
		},
		Done: function (self, e) {
			$(ourLegalModalSelector).modal("hide");
			// сохранить изменения
			model.SaveOurLegal(self.CurrentItem());
		}
	};

	// #endregion

	// #region bank account create/edit modal /////////////////////////////////////////////////////////////////////////////////

	var bankAccountModalSelector = "#bankAccountEditModal";

	model.DeleteBankAccount = function (account) {
		account.IsDeleted(true);
		model.SaveBankAccount(account);
	};

	model.OpenBankAccountCreate = function () {
		var data = null;
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.GetNewBankAccountUrl,
			data: { LegalId: model.SelectedLegal().ID() },
			success: function (response) {
				data = ko.mapping.fromJSON(response);
				model.ExtendBankAccount(data, model.SelectedLegal());
				model.SelectedLegal().BankAccountsItems.push(data);
			}
		});

		model.BankAccountEditModal.CurrentBank(null);
		model.BankAccountEditModal.CurrentItem(data);
		$(bankAccountModalSelector).modal("show");
		$(bankAccountModalSelector).draggable({ handle: ".modal-header" });
		model.BankAccountEditModal.OnClosed = function () { model.SelectedLegal().BankAccountsItems.remove(data); };
		model.BankAccountEditModal.Init();
	};

	model.OpenBankAccountEdit = function (account) {
		// получить текущий банк
		if (account.BankId())
			$.ajax({
				type: "POST",
				url: model.Options.GetBankUrl,
				data: { Id: account.BankId() },
				success: function (response) { model.BankAccountEditModal.CurrentBank(JSON.parse(response)); }
			});
		else
			model.BankAccountEditModal.CurrentBank(null);

		model.BankAccountEditModal.CurrentItem(account);
		$(bankAccountModalSelector).modal("show");
		$(bankAccountModalSelector).draggable({ handle: ".modal-header" });;
		model.BankAccountEditModal.OnClosed = null;
		model.BankAccountEditModal.Init();
	};

	model.BankAccountEditModal = {
		CurrentItem: ko.observable(),
		CurrentBank: ko.observable(),
		OnClosed: null,
		Close: function (self, e) {
			$(bankAccountModalSelector).modal("hide");
			if (self.OnClosed != null)
				self.OnClosed();
		},
		Init: function () {
			$("#bicAutocomplete").autocomplete({
				source: model.Options.GetBanksUrl,
				appendTo: bankAccountModalSelector,
				select: function (e, ui) { model.BankAccountEditModal.CurrentBank(ui.item.entity); }
			});
		},
		Done: function (self, e) {
			var bank = self.CurrentBank();
			if (bank) {
				self.CurrentItem().BankId(bank.ID);
				self.CurrentItem().BankName(bank.Name);
				self.CurrentItem().BIC(bank.BIC);
			}

			// сохранить изменения
			model.SaveBankAccount(self.CurrentItem());
			$(bankAccountModalSelector).modal("hide");
		}
	};

	// #endregion


	// инициализация //////////////////////////////////////////////////////////////////////////////////////////////////////////

}

$(function () {
	ko.applyBindings(new OurLegalsViewModel(modelData, modelDictionaries, {
		GetBankAccountsByLegalItemsUrl: app.urls.GetBankAccountsByLegalItems,
		GetEmployeesByLegalItemsUrl: app.urls.GetEmployeesByLegalItems,
		DownloadOurLegalUrl: app.urls.DownloadOurLegalSign,
		GetNewBankAccountUrl: app.urls.GetNewBankAccount,
		UploadOurLegalUrl: app.urls.UploadOurLegalSign,
		SaveBankAccountUrl: app.urls.SaveBankAccount,
		LegalDetailsUrl: app.urls.LegalDetails,
		SaveOurLegalUrl: app.urls.SaveOurLegal,
		SaveLegalUrl: app.urls.SaveLegal,
		GetLegalUrl: app.urls.GetLegal,
		GetBanksUrl: app.urls.GetBanks,
		GetBankUrl: app.urls.GetBank
	}), document.getElementById("ko-root"));
});