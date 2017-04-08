var ContractorViewModel = function (source, options)
{
	var model = this;

	model.Options = $.extend({
		UserDetailsUrl: null,
		LegalDetailsUrl: null,
		OrderDetailsUrl: null,
		OrderTemplatesUrl: null,
		ContractDetailsUrl: null,
		ContractorDetailsUrl: null,
		OrderAccountingDetailsUrl: null,

		GetLegalsItemsUrl: null,
		GetOrdersItemsUrl: null,
		GetPaymentsItemsUrl: null,
		GetWorkgroupItemsUrl: null,
		GetContractsItemsUrl: null,
		GetPricelistsItemsUrl: null,
		GetAccountingsItemsUrl: null,
		GetContractsByLegalItemsUrl: null,
		GetEmployeesByLegalItemsUrl: null,
		GetAccountingsByLegalItemsUrl: null,
		GetDocumentsByContractItemsUrl: null,
		GetBankAccountsByLegalItemsUrl: null,
		GetEmployeesByContractorItemsUrl: null,

		GetBankUrl: null,
		GetBanksUrl: null,
		GetPersonUrl: null,
		EditPersonUrl: null,
		GetNewLegalUrl: null,
		ViewDocumentUrl: null,
		GetActionHintUrl: null,
		ToggleIsLockedUrl: null,
		GetNewEmployeeUrl: null,
		GetNewWorkgroupUrl: null,
		GetNewBankAccountUrl: null,
		GetContractCurrenciesUrl: null,
		DownloadAccountingsFileUrl: null,
		GetLockingContractorInfoUrl: null,
		ChangeEmployeePasswordUrl: null,

		SaveBankAccountUrl: null,
		SaveContractorUrl: null,
		CreateContractUrl: null,
		SaveWorkgroupUrl: null,
		SaveDocumentUrl: null,
		SaveEmployeeUrl: null,
		SaveContractUrl: null,
		CreatePersonUrl: null,
		SaveLegalUrl: null,

		ViewPricelistUrl: null
	}, options);

	// поля ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	model.Contractor = ko.mapping.fromJS(source.Contractor);
	// Списки связанных сущностей
	model.LegalsItems = ko.observableArray();
	model.OrdersItems = ko.observableArray();
	model.PaymentsItems = ko.observableArray();
	model.WorkgroupItems = ko.observableArray();
	model.ContractsItems = ko.observableArray();
	model.EmployeesItems = ko.observableArray();
	model.PricelistsItems = ko.observableArray();
	model.ContractMarksItems = ko.observableArray();
	model.AccountingsList = ko.observableArray();
	// Справочники
	model.Dictionaries = ko.mapping.fromJS(source.Dictionaries);
	// Выбранное юрлицо из списка
	model.SelectedLegal = ko.observable();
	// Есть несохраненные изменения
	model.IsDirty = ko.observable(false);
	model.LegalsSelectedFilter = ko.observable("All");
	model.ContractsSelectedFilter = ko.observable("All");

	// вычисляемое/подписки ///////////////////////////////////////////////////////////////////////////////////////////////////

	model.Contractor.Name.subscribe(function () { model.IsDirty(true) });

	model.ContractorIncome = ko.computed(function ()
	{
		var total = 0;
		for (var p = 0; p < model.AccountingsList().length; ++p)
			if (model.AccountingsList()[p].IsIncome)
				total += model.AccountingsList()[p].Sum;

		return total;
	});

	model.ContractorExpense = ko.computed(function ()
	{
		var total = 0;
		for (var p = 0; p < model.AccountingsList().length; ++p)
			if (!model.AccountingsList()[p].IsIncome)
				total += model.AccountingsList()[p].Sum;

		return total;
	});

	model.ContractorBalance = ko.computed(function ()
	{
		return model.ContractorIncome() - model.ContractorExpense();
	});


	model.AccountingsCount = ko.computed(function () { return model.AccountingsList().length });
	model.EmployeesCount = ko.computed(function () { return model.EmployeesItems().length });
	model.ContractsCount = ko.computed(function () { return model.ContractsItems().length });
	model.WorkgroupCount = ko.computed(function () { return model.WorkgroupItems().length });
	model.LegalsCount = ko.computed(function () { return model.LegalsItems().length });
	model.OrdersCount = ko.computed(function () { return model.OrdersItems().length });

	// функции ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	model.UserDetailsUrl = function (id) { return model.Options.UserDetailsUrl + "/" + id; };
	model.LegalDetailsUrl = function (id) { return model.Options.LegalDetailsUrl + "/" + id; };
	model.OrderDetailsUrl = function (id) { return model.Options.OrderDetailsUrl + "/" + id; };
	model.OrderTemplatesUrl = function (id) { return model.Options.OrderTemplatesUrl + "/?contractId=" + id; };
	model.ContractDetailsUrl = function (id) { return model.Options.ContractDetailsUrl + "/" + id; };
	model.OrderAccountingDetailsUrl = function (id) { return model.Options.OrderAccountingDetailsUrl + "/" + id; };
	model.ChangeEmployeePasswordUrl = function (id) { return model.Options.ChangeEmployeePasswordUrl + "/" + id; };

	model.GotoPricelist = function (data) { window.open(model.Options.ViewPricelistUrl + "/" + data.ID(), "_blank"); };

	model.ViewDocument = function (data)
	{
		var id = ko.unwrap(data.ID());
		window.open(model.Options.ViewDocumentUrl + "/" + id, "_blank");
	};

	model.SelectTaxType = function (data)
	{
		model.Contractor.TaxType(data);
		model.IsDirty(true);
	};

	model.SelectLegal = function (legal)
	{
		// [re]load related
		model.LoadEmployeesByLegal(legal);
		model.LoadContractsByLegal(legal);
		model.LoadBankAccountsByLegal(legal);
		model.LoadAccountingsByLegal(legal);
		model.SelectedLegal(legal);
	};

	model.ToggleLock = function ()
	{
		if (model.IsDirty())
		{
			alert("Есть несохраненные изменения, пожалуйста сохраните их.");
			return;
		}

		$.ajax({
			type: "POST",
			url: model.Options.ToggleIsLockedUrl,
			data: {
				ContractorID: model.Contractor.ID(),
				IsLocked: !model.Contractor.IsLocked()
			},
			success: function (response)
			{
				var r = JSON.parse(response);
				if (r.Message)
					model.OpenError(r);
				else
				{
					model.Contractor.IsLocked(!model.Contractor.IsLocked());

					if (model.Contractor.IsLocked())
					{
						// проверить на наличие незакрытых заказов
						$.ajax({
							type: "POST",
							url: model.Options.GetLockingContractorInfoUrl,
							data: { ContractorId: model.Contractor.ID() },
							success: function (response)
							{
								var r = JSON.parse(response);
								if (r.Message)
									alert(r.Message);
							}
						});
					}
				}
			}
		});
	};

	//#region extenders ///////////////////////////////////////////////////////////////////////////////////////////////////////

	model.ExtendLegals = function (array)
	{
		ko.utils.arrayForEach(array, function (item) { model.ExtendLegal(item); })
	};

	model.ExtendLegal = function (legal)
	{
		legal.IsDeleted = legal.IsDeleted || ko.observable(false);
		legal.IsDirty = legal.IsDirty || ko.observable(false);
		legal.IsDirty.subscribe(function (newValue) { if (newValue) model.IsDirty(true) });

		legal.EmployeesItems = legal.EmployeesItems || ko.observableArray();
		legal.ContractsItems = legal.ContractsItems || ko.observableArray();
		legal.BankAccountsItems = legal.BankAccountsItems || ko.observableArray();
		legal.AccountingsItems = legal.AccountingsItems || ko.observableArray();

		legal.IsSubscribed = false;
		legal.FieldsSubscription = function ()
		{
			var old = "";

			//one-time dirty flag that gives up its dependencies on first change
			var result = ko.computed(function ()
			{
				if (!legal.IsSubscribed)
				{
					//just for subscriptions
					old += legal.TaxTypeId();
					old += legal.DirectorId();
					old += legal.AccountantId();
					old += legal.Name();
					old += legal.DisplayName();
					old += legal.EnName();
					old += legal.EnShortName();
					old += legal.TIN();
					old += legal.OGRN();
					old += legal.KPP();
					old += legal.OKPO();
					old += legal.OKVED();
					old += legal.Address();
					old += legal.EnAddress();
					old += legal.AddressFact();
					old += legal.EnAddressFact();
					old += legal.PostAddress();
					old += legal.EnPostAddress();
					old += legal.WorkTime();
					old += legal.TimeZone();
					old += legal.IsNotResident();

					old += legal.IsDeleted();

					legal.IsSubscribed = true;
					return;
				}

				legal.IsDirty(true);
			});

			return result;
		};

		legal.FieldsSubscription();
	};

	model.ExtendEmployees = function (array, parent)
	{
		ko.utils.arrayForEach(array, function (item) { model.ExtendEmployee(item, parent); })
	};

	model.ExtendEmployee = function (employee, parent)
	{
		employee.IsSigning = employee.IsSigning || ko.observable(false);
		employee.IsDeleted = employee.IsDeleted || ko.observable(false);
		employee.IsDirty = employee.IsDirty || ko.observable(false);

		employee.IsDirty.subscribe(function (newValue) { if (newValue) parent.IsDirty(true) });
		employee.IsDeleted.subscribe(function () { employee.IsDirty(true); });

		employee.IsSubscribed = false;
		employee.FieldsSubscription = function ()
		{
			var old = "";

			//one-time dirty flag that gives up its dependencies on first change
			var result = ko.computed(function ()
			{
				if (!employee.IsSubscribed)
				{
					//just for subscriptions
					old += employee.PersonId();
					old += employee.Department();
					old += employee.Position();
					old += employee.GenitivePosition();
					old += employee.Comment();
					old += employee.BeginDate();
					old += employee.EndDate();
					old += employee.Basis();
					old += employee.EnPosition();
					old += employee.EnBasis();

					old += employee.IsDeleted();

					//next time return true and avoid ko.toJS
					employee.IsSubscribed = true;
					return;
				}

				//on subsequent changes, flag is now dirty
				employee.IsDirty(true);
			});

			return result;
		};

		employee.FieldsSubscription();
	};

	model.ExtendBankAccounts = function (array, legal)
	{
		ko.utils.arrayForEach(array, function (item) { model.ExtendBankAccount(item, legal); })
	};

	model.ExtendBankAccount = function (account, parent)
	{
		account.IsDeleted = account.IsDeleted || ko.observable(false);
		account.IsDirty = account.IsDirty || ko.observable(false);

		account.IsSubscribed = false;
		account.FieldsSubscription = function ()
		{
			var old = "";
			//one-time dirty flag that gives up its dependencies on first change
			var result = ko.computed(function ()
			{
				if (!account.IsSubscribed)
				{
					// for subscriptions
					old += account.Number();
					old += account.BankId();
					old += account.CurrencyId();
					old += account.CoBankName();
					old += account.CoBankAccount();
					old += account.CoBankSWIFT();
					old += account.IsDeleted();

					account.IsSubscribed = true;
				}
				else
				{
					account.IsDirty(true);
				}
			});

			return result;
		};

		account.FieldsSubscription();
		account.IsDirty.subscribe(function (newValue) { if (newValue) parent.IsDirty(true) });
	};

	model.ExtendContracts = function (array, legal)
	{
		ko.utils.arrayForEach(array, function (item) { model.ExtendContract(item, legal); })
	};

	model.ExtendContract = function (contract, parent)
	{
		contract.IsDeleted = contract.IsDeleted || ko.observable(false);
		contract.IsDirty = contract.IsDirty || ko.observable(false);
		contract.IsDirty.subscribe(function (newValue) { if (newValue) parent.IsDirty(true) });
		contract.Currencies = contract.Currencies || ko.observableArray();
		model.LoadContractCurrencies(contract.ID(), contract.Currencies);

		contract.IsDeleted.subscribe(function () { contract.IsDirty(true); });

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
					old += contract.BankAccountId();
					old += contract.ContractRoleId();
					old += contract.ContractTypeId();
					old += contract.PaymentTermsId();
					old += contract.OurBankAccountId();
					old += contract.OurContractRoleId();
					old += contract.ContractServiceTypeId();
					old += contract.Comment();
					old += contract.IsFixed();
					old += contract.IsProlongation();
					old += contract.Date();
					old += contract.BeginDate();
					old += contract.EndDate();

					contract.IsSubscribed = true;
				}
				else
				{
					var newV = ko.toJS(contract);	// TEMP:
					contract.IsDirty(true);
				}
			});

			return result;
		};

		contract.FieldsSubscription();
	};

	//#endregion

	model.GetPricelists = function (contractorId)
	{
		var id = ko.unwrap(contractorId);
		$.ajax({
			type: "POST",
			url: model.Options.GetPricelistsItemsUrl,
			data: { ContractorId: id },
			success: function (response) { model.PricelistsItems(ko.mapping.fromJSON(response).Items()); }
		});
	};

	model.GetLegals = function (contractorId)
	{
		var id = ko.unwrap(contractorId);

		$.ajax({
			type: "POST",
			url: model.Options.GetLegalsItemsUrl,
			data: { ContractorId: id },
			success: function (response)
			{
				var list = ko.mapping.fromJSON(response).Items()
				model.ExtendLegals(list);
				model.LegalsItems(list);
			}
		});
	};

	model.GetOrders = function (contractorId)
	{
		var id = ko.unwrap(contractorId);
		$.ajax({
			type: "POST",
			url: model.Options.GetOrdersItemsUrl,
			data: { ContractorId: id },
			success: function (response) { model.OrdersItems(JSON.parse(response).Items); }
		});
	};

	model.GetWorkgroup = function (contractorId)
	{
		var id = ko.unwrap(contractorId);

		$.ajax({
			type: "POST",
			url: model.Options.GetWorkgroupItemsUrl,
			data: { ContractorId: id },
			success: function (response) { model.WorkgroupItems(ko.mapping.fromJSON(response).Items()); }
		});
	};

	model.GetAccountings = function (contractorId)
	{
		var id = ko.unwrap(contractorId);

		$.ajax({
			type: "POST",
			url: model.Options.GetAccountingsItemsUrl,
			data: { ContractorId: id },
			success: function (response) { model.AccountingsList(JSON.parse(response).Items); }
		});
	};

	model.GetPayments = function (contractorId)
	{
		var id = ko.unwrap(contractorId);
		$.ajax({
			type: "POST",
			url: model.Options.GetPaymentsItemsUrl,
			data: { ContractorId: id },
			//success: function (response) { model.PaymentsItems(ko.mapping.fromJSON(response).Items()); }
			success: function (response) { model.PaymentsItems(JSON.parse(response).Items); }
		});
	};

	model.GetContracts = function (contractorId)
	{
		var id = ko.unwrap(contractorId);
		$.ajax({
			type: "POST",
			url: model.Options.GetContractsItemsUrl,
			data: { ContractorId: id },
			success: function (response)
			{
				//var list = ko.mapping.fromJSON(response).Items();
				var list = JSON.parse(response).Items;
				ko.utils.arrayForEach(list, function (item)
				{
					item.Currencies = ko.observableArray(item.Currencies);
					//item.Currencies = item.Currencies || ko.observableArray();
					//model.LoadContractCurrencies(item.ID(), item.Currencies);
					//model.LoadContractCurrencies(item.ID, item.Currencies);
				});
				model.ContractsItems(list);
			}
		});
	};

	model.LoadContractCurrencies = function (contractId, observableArray)
	{
		var id = ko.unwrap(contractId);
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.GetContractCurrenciesUrl,
			data: { ContractId: id },
			success: function (response) { observableArray(ko.mapping.fromJSON(response).Items()); }
		});
	};

	model.GetEmployees = function (contractorId)
	{
		var id = ko.unwrap(contractorId);
		$.ajax({
			type: "POST",
			url: model.Options.GetEmployeesByContractorItemsUrl,
			data: { ContractorId: id },
			success: function (response)
			{
				var list = ko.mapping.fromJSON(response).Items();
				model.ExtendEmployees(list, model);
				model.EmployeesItems(list);
			}
		});
	};

	model.LoadEmployeesByLegal = function (legal)
	{
		var id = legal.ID();
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.GetEmployeesByLegalItemsUrl,
			data: { LegalId: id },
			success: function (response)
			{
				var list = ko.mapping.fromJSON(response).Items();
				model.ExtendEmployees(list, legal);
				legal.EmployeesItems(list);
			}
		});
	};

	model.LoadContractsByLegal = function (legal)
	{
		var id = legal.ID();
		$.ajax({
			type: "POST",
			url: model.Options.GetContractsByLegalItemsUrl,
			data: { LegalId: id },
			success: function (response)
			{
				var list = ko.mapping.fromJSON(response).Items();
				model.ExtendContracts(list, legal);
				legal.ContractsItems(list);
			}
		});
	};

	model.LoadBankAccountsByLegal = function (legal)
	{
		var id = legal.ID();
		$.ajax({
			type: "POST",
			url: model.Options.GetBankAccountsByLegalItemsUrl,
			data: { LegalId: id },
			success: function (response)
			{
				legal.BankAccountsItems(ko.mapping.fromJSON(response).Items());
				model.ExtendBankAccounts(legal.BankAccountsItems(), legal);
			}
		});
	};

	model.LoadAccountingsByLegal = function (legal)
	{
		var id = legal.ID();
		$.ajax({
			type: "POST",
			url: model.Options.GetAccountingsByLegalItemsUrl,
			data: { LegalId: id },
			success: function (response) { legal.AccountingsItems(ko.mapping.fromJSON(response).Items()); }
		});
	};

	// #region save ///////////////////////////////////////////////////////////////////////////////////////////////////////////

	model.Save = function ()
	{
		$.ajax({
			type: "POST",
			url: model.Options.SaveContractorUrl,
			data: {
				ID: model.Contractor.ID(),
				Name: model.Contractor.Name()
			},
			success: function (response)
			{
				var r = JSON.parse(response);
				if (r.Message)
					model.OpenError(r);
				else
					model.IsDirty(false);
			}
		});

		ko.utils.arrayForEach(model.LegalsItems(), function (legal)
		{
			if (legal.IsDirty())
			{
				// создать модель данных Юрлица
				var data = {
					ID: legal.ID(),
					ContractorId: legal.ContractorId(),
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
					PostAddress: legal.PostAddress(),
					EnPostAddress: legal.EnPostAddress(),
					WorkTime: legal.WorkTime(),
					TimeZone: legal.TimeZone(),
					IsNotResident: legal.IsNotResident(),
					IsDeleted: legal.IsDeleted(),
					BankAccounts: [],
					Contracts: [],
					Employees: []
				};

				// добавить модифицированные банковские счета
				ko.utils.arrayForEach(legal.BankAccountsItems(), function (account)
				{
					if (account.IsDirty())
						data.BankAccounts.push({
							ID: account.ID(),
							LegalId: account.LegalId(),
							BankId: account.BankId(),
							CurrencyId: account.CurrencyId(),
							Number: account.Number(),
							CoBankName: account.CoBankName(),
							CoBankAccount: account.CoBankAccount(),
							CoBankSWIFT: account.CoBankSWIFT(),
							IsDeleted: account.IsDeleted()
						});
				});

				// добавить модифицированных сотрудников
				ko.utils.arrayForEach(legal.EmployeesItems(), function (employee)
				{
					if (employee.IsDirty())
						data.Employees.push({
							ID: employee.ID(),
							LegalId: employee.LegalId(),
							Department: employee.Department(),
							Position: employee.Position(),
							GenitivePosition: employee.GenitivePosition(),
							Comment: employee.Comment(),
							Basis: employee.Basis(),
							EnPosition: employee.EnPosition(),
							EnBasis: employee.EnBasis(),
							PersonId: employee.PersonId(),
							//SigningAuthorityId: employee.SigningAuthorityId(),
							IsSigning: employee.IsSigning(),
							BeginDate: employee.BeginDate(),
							EndDate: employee.EndDate(),
							IsDeleted: employee.IsDeleted()
						});
				});

				$.ajax({
					type: "POST",
					url: model.Options.SaveLegalUrl,
					data: JSON.stringify(data),
					dataType: "json",
					contentType: "application/json; charset=utf-8",
					success: function (response)
					{
						var r = JSON.parse(response || '""');
						if (r.Message)
							model.OpenError(r);
						else
						{
							legal.IsSubscribed = false;	//reset
							legal.IsDirty(false);
							// TODO: reload legal and relatives
						}
					}
				});
			}
		});
	};

	model.SaveWorkgroup = function (entity)
	{
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.SaveWorkgroupUrl,
			data: {
				ID: entity.ID(),
				ContractorId: model.Contractor.ID(),
				ParticipantRoleId: entity.ParticipantRoleId(),
				UserId: entity.UserId(),
				FromDate: app.utility.SerializeDateTime(entity.FromDate()),
				ToDate: app.utility.SerializeDateTime(entity.ToDate()),
				IsResponsible: entity.IsResponsible(),
				IsDeputy: entity.IsDeputy()
			},
			success: function (response)
			{
				var r = JSON.parse(response);
				if (r.Message)
					model.OpenError(r);
			}
		});
	};

	model.SaveEmployee = function (employee)
	{
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.SaveEmployeeUrl,
			data: {
				ID: employee.ID(),
				LegalId: employee.LegalId(),
				ContractorId: employee.ContractorId(),
				Department: employee.Department(),
				Position: employee.Position(),
				GenitivePosition: employee.GenitivePosition(),
				Comment: employee.Comment(),
				Basis: employee.Basis(),
				EnPosition: employee.EnPosition(),
				EnBasis: employee.EnBasis(),
				PersonId: employee.PersonId(),
				IsSigning: employee.IsSigning(),
				BeginDate: app.utility.SerializeDateTime(employee.BeginDate()),
				EndDate: app.utility.SerializeDateTime(employee.EndDate()),
				IsDeleted: employee.IsDeleted()
			},
			success: function (response) { employee.IsDirty(false); }
		});
	};

	model.SaveBankAccount = function (account)
	{
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
				CoBankIBAN: account.CoBankIBAN(),
				CoBankAddress: account.CoBankAddress(),
				IsDeleted: account.IsDeleted()
			},
			success: function (response)
			{
				var r = JSON.parse(response);
				if (r.Message)
					model.OpenError(r);
				else
					account.IsDirty(false);
			}
		});
	};

	model.SaveDocument = function (entity)
	{
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.SaveDocumentUrl,
			data: {
				ID: entity.ID(),
				UploadedBy: entity.UploadedBy(),
				UploadedDate: app.utility.SerializeDateTime(entity.UploadedDate()),
				Date: entity.Date(),
				DocumentTypeId: entity.DocumentTypeId(),
				Filename: entity.Filename(),
				FileSize: entity.FileSize(),
				IsPrint: entity.IsPrint(),
				IsNipVisible: entity.IsNipVisible(),
				Number: entity.Number(),
				OriginalSentDate: entity.OriginalSentDate(),
				OriginalReceivedDate: entity.OriginalReceivedDate(),

				IsDeleted: entity.IsDeleted()
			},
			success: function (response)
			{
				if (entity.ID() == 0)
					entity.ID(JSON.parse(response).ID);

				entity.IsDirty(false);
			},
		});
	};

	// #endregion

	model.CreateContract = function ()
	{
		var url = model.Options.CreateContractUrl + "?legalId=" + model.SelectedLegal().ID();
		window.open(url, "_blank");
	};

	model.CreatePerson = function () { window.open(model.Options.CreatePersonUrl, "_blank"); };

	model.EditPerson = function (context)
	{
		var url = model.Options.EditPersonUrl + "?personId=" + ko.unwrap(context.CurrentPerson().ID);
		window.open(url, "_blank");
	};

	model.CreateLegal = function ()
	{
		$.ajax({
			type: "POST",
			url: model.Options.GetNewLegalUrl,
			data: { ContractorId: model.Contractor.ID() },
			success: function (response)
			{
				var r = JSON.parse(response);
				if (r.Message)
					model.OpenError(r);
				else
				{
					var data = ko.mapping.fromJSON(response);
					model.ExtendLegal(data);
					model.LegalsItems.push(data);
					model.SelectLegal(data);
				}
			}
		});
	};

	model.DeleteLegal = function (entity)
	{
		$.ajax({
			type: "POST",
			url: model.Options.SaveLegalUrl,
			data: { ID: entity.ID(), IsDeleted: true, ContractorId: entity.ContractorId() },
			success: function (response)
			{
				var r = JSON.parse(response);
				if (r.Message)
					model.OpenError(r);
				else
					model.LegalsItems.remove(entity);
			}
		});
	};

	model.DeleteContract = function (entity)
	{
		$.ajax({
			type: "POST",
			url: model.Options.SaveContractUrl,
			data: { ID: entity.ID(), IsDeleted: true },
			success: function (response)
			{
				var r = JSON.parse(response);
				if (r.Message)
					model.OpenError(r);
				else
					model.SelectedLegal().ContractsItems.remove(entity);
			}
		});
	};

	model.SetDirector = function (data)
	{
		if (data)
			model.SelectedLegal().DirectorId(data.ID());
		else
			model.SelectedLegal().DirectorId(null);
	};

	// #region bank account create/edit modal /////////////////////////////////////////////////////////////////////////////////

	var bankAccountModalSelector = "#bankAccountEditModal";

	model.DeleteBankAccount = function (account)
	{
		account.IsDeleted(true);
		model.SaveBankAccount(account);
	};

	model.OpenBankAccountCreate = function ()
	{
		$.ajax({
			type: "POST",
			url: model.Options.GetNewBankAccountUrl,
			data: { LegalId: model.SelectedLegal().ID() },
			success: function (response)
			{
				var r = JSON.parse(response);
				if (r.Message)
					model.OpenError(r);
				else
				{
					var data = ko.mapping.fromJSON(response);
					model.ExtendBankAccount(data, model.SelectedLegal());
					model.SelectedLegal().BankAccountsItems.push(data);

					model.BankAccountEditModal.CurrentBank(null);
					model.BankAccountEditModal.CurrentItem(data);
					$(bankAccountModalSelector).modal("show");
					$(bankAccountModalSelector).draggable({ handle: ".modal-header" });
					model.BankAccountEditModal.OnClosed = function () { model.SelectedLegal().BankAccountsItems.remove(data); };
					model.BankAccountEditModal.Init();
				}
			}
		});
	};

	model.OpenBankAccountEdit = function (account)
	{
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
		Close: function (self, e)
		{
			$(bankAccountModalSelector).modal("hide");
			if (self.OnClosed != null)
				self.OnClosed();
		},
		Init: function ()
		{
			$("#bicAutocomplete").autocomplete({
				source: model.Options.GetBanksUrl,
				appendTo: bankAccountModalSelector,
				select: function (e, ui) { model.BankAccountEditModal.CurrentBank(ui.item.entity); }
			});
		},
		Done: function (self, e)
		{
			var bank = self.CurrentBank();
			if (bank)
			{
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

	// #region employee create/edit modal /////////////////////////////////////////////////////////////////////////////////////

	var employeeModalSelector = "#employeeEditModal";

	model.DeleteEmployee = function (entity)
	{
		entity.IsDeleted(true);
		if (model.SelectedLegal())
		{
			// убрать ссылки на этого сотрудника
			if (model.SelectedLegal().AccountantId() == entity.ID())
				model.SelectedLegal().AccountantId(null);	// TODO: более интеллектуально

			if (model.SelectedLegal().DirectorId() == entity.ID())
				model.SelectedLegal().DirectorId(null);	// TODO: более интеллектуально
		}

		model.IsDirty(true);
	};

	model.OpenContractorEmployeeCreate = function ()
	{
		$.ajax({
			type: "POST",
			url: model.Options.GetNewEmployeeUrl,
			data: { ContractorId: model.Contractor.ID() },
			success: function (response)
			{
				var r = JSON.parse(response);
				if (r.Message)
					model.OpenError(r);
				else
				{
					var data = ko.mapping.fromJSON(response);
					model.ExtendEmployee(data, model);
					model.EmployeesItems.push(data);

					model.EmployeeEditModal.CurrentItem(data);
					$(employeeModalSelector).modal("show");
					$(employeeModalSelector).draggable({ handle: ".modal-header" });
					model.EmployeeEditModal.OnClosed = function () { model.EmployeesItems.remove(data); };
					model.EmployeeEditModal.Init();
				}
			}
		});
	};

	model.OpenEmployeeCreate = function ()
	{
		var data = null;
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.GetNewEmployeeUrl,
			data: { LegalId: model.SelectedLegal().ID() },
			success: function (response)
			{
				data = ko.mapping.fromJSON(response);
				model.ExtendEmployee(data, model.SelectedLegal());
				model.SelectedLegal().EmployeesItems.push(data);
			}
		});

		model.EmployeeEditModal.CurrentItem(data);
		$(employeeModalSelector).modal("show");
		$(employeeModalSelector).draggable({ handle: ".modal-header" });
		model.EmployeeEditModal.OnClosed = function () { model.SelectedLegal().EmployeesItems.remove(data); };
		model.EmployeeEditModal.Init();
	};

	model.OpenEmployeeEdit = function (entity)
	{
		model.EmployeeEditModal.CurrentItem(entity);
		if (model.SelectedLegal() && (model.SelectedLegal().DirectorId() == entity.ID()))
			entity.IsSigning(true);

		$(employeeModalSelector).modal("show");
		$(employeeModalSelector).draggable({ handle: ".modal-header" });
		model.EmployeeEditModal.OnClosed = null;
		model.EmployeeEditModal.Init();
	};

	model.EmployeeEditModal = {
		CurrentItem: ko.observable(),
		CurrentName: ko.observable(),
		CurrentPerson: ko.observable(),
		SelectedTemplate: ko.observable(),
		IsSelectTemplateVisible: ko.observable(false),
		ShowSelectTemplate: function () { model.EmployeeEditModal.IsSelectTemplateVisible(true); },
		Init: function ()
		{
			var self = model.EmployeeEditModal;
			if (self.CurrentItem().PersonId())
			{
				$.ajax({
					type: "POST",
					async: false,
					url: model.Options.GetPersonUrl,
					data: { Id: self.CurrentItem().PersonId() },
					success: function (response)
					{
						self.CurrentPerson(JSON.parse(response));
						self.CurrentName(JSON.parse(response).DisplayName);
					}
				});
			}

			$("#personAutocomplete").autocomplete({
				source: model.Options.GetPersonsUrl,
				appendTo: employeeModalSelector,
				select: function (e, ui)
				{
					model.EmployeeEditModal.CurrentPerson(ui.item.entity);
					model.EmployeeEditModal.CurrentName(ui.item.entity.DisplayName);
				}
			});
			self.SelectedTemplate.subscribe(function (newValue)
			{
				var tmpl = ko.utils.arrayFirst(model.Dictionaries.PositionTemplate(), function (item) { return item.ID() == newValue });
				model.EmployeeEditModal.CurrentItem().Position(tmpl.Position());
				model.EmployeeEditModal.CurrentItem().EnPosition(tmpl.EnPosition());
				model.EmployeeEditModal.CurrentItem().GenitivePosition(tmpl.GenitivePosition());
				model.EmployeeEditModal.CurrentItem().Basis(tmpl.Basis());
				model.EmployeeEditModal.CurrentItem().EnBasis(tmpl.EnBasis());
				model.EmployeeEditModal.CurrentItem().Department(tmpl.Department());
				model.EmployeeEditModal.IsSelectTemplateVisible(false);
			});
		},
		OnClosed: null,
		Close: function (self, e)
		{
			$(employeeModalSelector).modal("hide");
			if (self.OnClosed != null)
				self.OnClosed();
		},
		Done: function (self, e)
		{
			var person = self.CurrentPerson();
			if (person)
			{
				self.CurrentItem().PersonId(person.ID);
				// ??? self.CurrentItem().Name(person.DisplayName);
			}

			if (self.CurrentItem().IsSigning())
				model.SetDirector(self.CurrentItem());
			else if (model.SelectedLegal().DirectorId() == self.CurrentItem().ID())
				model.SetDirector(null);

			model.SaveEmployee(self.CurrentItem());
			$(employeeModalSelector).modal("hide");
		}
	};

	// #endregion

	// #region workgroup create/edit modal ////////////////////////////////////////////////////////////////////////////////////

	var workgroupModalSelector = "#workgroupEditModal";

	model.DeleteWorkgroup = function (workgroup)
	{
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.SaveWorkgroupUrl,
			data: { ID: workgroup.ID(), IsDeleted: true },
			success: function (response)
			{
				var r = JSON.parse(response);
				if (r.Message)
					model.OpenError(r);
				else
					model.WorkgroupItems.remove(workgroup);
			}
		});
	};

	model.OpenWorkgroupCreate = function ()
	{
		$.ajax({
			type: "POST",
			url: model.Options.GetNewWorkgroupUrl,
			data: { ContractorId: model.Contractor.ID() },
			success: function (response)
			{
				var r = JSON.parse(response);
				if (r.Message)
					model.OpenError(r);
				else
				{
					var data = ko.mapping.fromJSON(response);
					model.WorkgroupItems.push(data);

					model.WorkgroupEditModal.CurrentItem(data);
					$(workgroupModalSelector).modal("show");
					$(workgroupModalSelector).draggable({ handle: ".modal-header" });
					model.WorkgroupEditModal.OnClosed = function () { model.WorkgroupItems.remove(data) };
					model.WorkgroupEditModal.Init();
				}
			}
		});
	};

	model.OpenWorkgroupEdit = function (workgroup)
	{
		if (!ko.utils.arrayFirst(model.Dictionaries.ActiveUser(), function (item) { return item.ID() == workgroup.UserId() }))
		{
			alert("Пользователь уже неактивен");
			return;
		}

		model.WorkgroupEditModal.CurrentItem(workgroup);
		$(workgroupModalSelector).modal("show");
		$(workgroupModalSelector).draggable({ handle: ".modal-header" });
		model.WorkgroupEditModal.OnClosed = null;
		model.WorkgroupEditModal.Init();
	};

	model.WorkgroupEditModal = {
		CurrentItem: ko.observable(),
		Init: function () { },
		OnClosed: null,
		Close: function (self, e)
		{
			$(workgroupModalSelector).modal("hide");
			if (self.OnClosed != null)
				self.OnClosed();
		},
		Validate: function (self)
		{
			if (self.CurrentItem().IsDeputy() && self.CurrentItem().IsResponsible())
			{
				alert("Участник не может быть одновременно ответственным и заместителем.");
				return false;
			}

			if (self.CurrentItem().IsDeputy() && (!self.CurrentItem().FromDate() || !self.CurrentItem().ToDate()))
			{
				alert("Для заместителя обязательно указание диапазона дат.");
				return false;
			}

			return true;
		},
		Done: function (self, e)
		{
			if (!self.Validate(self))
				return;

			$(workgroupModalSelector).modal("hide");
			model.SaveWorkgroup(self.CurrentItem());
			self.CurrentItem(null);
		}
	};

	// #endregion

	// #region document create/edit modal /////////////////////////////////////////////////////////////////////////////////////

	var documentModalSelector = "#documentEditModal";

	model.OpenContractDocumentCreate = function (contractId)
	{
		$.ajax({
			type: "POST",
			url: model.Options.GetNewDocumentUrl,
			data: { AccountingId: null, OrderId: null, ContractId: contractId },
			success: function (response)
			{
				var r = JSON.parse(response);
				if (r.Message)
					model.OpenError(r);
				else
				{
					var data = ko.mapping.fromJSON(response);
					model.ExtendDocument(data, model.Order);
					model.DocumentsItems.push(data);

					model.DocumentEditModal.CurrentItem(data);
					$(documentModalSelector).modal("show");
					$(documentModalSelector).draggable({ handle: ".modal-header" });
					model.DocumentEditModal.Init();
				}
			}
		});
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
			model.DocumentEditModal.CurrentItem().DocumentTypeId.subscribe(function (newValue)
			{
				var productId = model.Order.ProductId();
				var defaults = [];
				switch (productId)
				{
					case 1:	// авиа внутренние
						defaults = [2, 4, 23, 32, 55];
						break;
					case 2:	// авиа импорт
						defaults = [2, 4, 20, 23, 28, 32, 55];
						break;
					case 3:	// авиа экспорт
						defaults = [2, 4, 20, 23, 28, 32, 55];
						break;
					case 4:	// авто внутренние
						defaults = [23, 32, 55, 64];
						break;
					case 5:	// авто международные
						defaults = [1, 7, 20, 23, 28, 32, 55];
						break;
					case 6:	// море
						defaults = [3, 5, 13, 20, 23, 32, 55];
						break;
					case 10:	// торговое агентирование
						defaults = [20, 21, 23, 55];
						break;
					case 7:	// Жд
						defaults = [13, 20, 21, 32, 55];
						break;
					case 8:	// таможня авиа
						defaults = [20, 28, 32, 55];
						break;
					case 8:	// таможня наземная
						defaults = [20, 28, 32, 55];
						break;
				}

				model.DocumentEditModal.CurrentItem().IsPrint(defaults.indexOf(newValue) >= 0);
			});

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

	// #region contract info modal ////////////////////////////////////////////////////////////////////////////////////////////

	var contractModalSelector = "#contractInfoModal";

	model.OpenContractInfoById = function (contractId)
	{
		var id = ko.unwrap(contractId);
		model.OpenContractInfo(ko.utils.arrayFirst(model.ContractsItems(), function (item) { return item.ID == id }));
	};

	model.OpenContractInfo = function (contract)
	{
		model.ContractInfoModal.CurrentItem(contract);
		$(contractModalSelector).modal("show");
		$(contractModalSelector).draggable({ handle: ".modal-header" });
		model.ContractInfoModal.Init();
	};

	model.ContractInfoModal = {
		CurrentItem: ko.observable(),
		OurBankAccounts: ko.observableArray(),
		BankAccounts: ko.observableArray(),
		Documents: ko.observableArray(),
		OurBankAccountNumber: ko.pureComputed(function ()
		{
			var ba = ko.utils.arrayFirst(model.ContractInfoModal.OurBankAccounts(), function (item) { return item.ID() == ko.unwrap(model.ContractInfoModal.CurrentItem().OurBankAccountId()) });
			return ba ? ba.Number() : "";
		}),
		BankAccountNumber: ko.pureComputed(function ()
		{
			var ba = ko.utils.arrayFirst(model.ContractInfoModal.BankAccounts(), function (item) { return item.ID() == ko.unwrap(model.ContractInfoModal.CurrentItem().BankAccountId()) });
			return ba ? ba.Number() : "";
		}),
		Init: function ()
		{
			// получить счета по нашему юрлицу
			$.ajax({
				type: "POST",
				url: model.Options.GetBankAccountsByLegalItemsUrl,
				data: { LegalId: ko.unwrap(model.ContractInfoModal.CurrentItem().OurLegalId) },
				success: function (response) { model.ContractInfoModal.OurBankAccounts(ko.mapping.fromJSON(response).Items()); }
			});
			// получить счета по юрлицу
			$.ajax({
				type: "POST",
				url: model.Options.GetBankAccountsByLegalItemsUrl,
				data: { LegalId: ko.unwrap(model.ContractInfoModal.CurrentItem().LegalId) },
				success: function (response) { model.ContractInfoModal.BankAccounts(ko.mapping.fromJSON(response).Items()); }
			});
			// получить документы
			$.ajax({
				type: "POST",
				url: model.Options.GetDocumentsByContractItemsUrl,
				data: { ContractId: ko.unwrap(model.ContractInfoModal.CurrentItem().ID) },
				success: function (response) { model.ContractInfoModal.Documents(ko.mapping.fromJSON(response).Items()); }
			});
		},
		GotoContractor: function (data, e)
		{
			window.open(model.LegalDetailsUrl(ko.unwrap(data.CurrentItem().LegalId)), "_blank");
		},
		GotoContract: function (data, e)
		{
			window.open(model.ContractDetailsUrl(ko.unwrap(data.CurrentItem().ID)), "_blank");
		},
		Done: function (data, e)
		{
			$(contractModalSelector).modal("hide");
		}
	};

	// #endregion

	// #region error modal ////////////////////////////////////////////////////////////////////////////////////////////////////

	var errorModalSelector = "#errorModal";

	model.OpenError = function (response)
	{
		$(errorModalSelector).modal("show");
		model.ErrorModal.Init(response);
		$(errorModalSelector).draggable({ handle: ".modal-header" });
	};

	model.ErrorModal = {
		Message: ko.observable(),
		Hint: ko.observable(),
		Init: function (response)
		{
			model.ErrorModal.Message(ko.unwrap(response.Message));
			// получить текст подсказки по action id
			if (response.ActionId)
				$.ajax({
					type: "POST",
					url: model.Options.GetActionHintUrl,
					data: { ActionId: response.ActionId },
					success: function (response) { model.ErrorModal.Hint(response); }
				});
		},
		OnClosed: null,
		Close: function (self, e)
		{
			$(errorModalSelector).modal("hide");
			if (self.OnClosed != null)
				self.OnClosed();
		},
		Done: function (self, e)
		{
			$(errorModalSelector).modal("hide");
		}
	};

	// #endregion

	model.GetEmployeeName = function (array, id)
	{
		var emp = ko.utils.arrayFirst(array(), function (item) { return item.ID() == id() });
		if (emp)
			return emp.Name();

		return "";
	};

	model.GetContractDisplay = function (contractId)
	{
		var id = ko.unwrap(contractId);
		//var emp = ko.utils.arrayFirst(model.ContractsItems(), function (item) { return item.ID() == id });
		var emp = ko.utils.arrayFirst(model.ContractsItems(), function (item) { return item.ID == id });
		if (emp)
			//return emp.Number();
			return emp.Number;

		return "";
	};

	model.GetContractCurrenciesDisplay = function (contract)
	{
		var result = "";
		if (contract.Currencies)
			ko.utils.arrayForEach(contract.Currencies(), function (item) { result += app.utility.GetDisplay(model.Dictionaries.Currency, ko.unwrap(item.CurrencyId)) + ", " });

		return result.substring(0, result.lastIndexOf(", "));
	};

	model.DownloadAccountingsFile = function (entity)
	{
		window.open(model.Options.DownloadAccountingsFileUrl + "?LegalId=" + entity.ID(), "_blank");
	};

	// #region sorters ////////////////////////////////////////////////////////////////////////////////////////////////////////

	model.ContractSorter = {
		Field: ko.observable(),
		Asc: ko.observable(true),
		Sort: function (field)
		{
			var sorter = this;
			if (sorter.Field() == field)
				sorter.Asc(!sorter.Asc());

			sorter.Field(field);
			var func = function () { };
			switch (field)
			{
				case "LegalId":
					if (sorter.Asc())
						func = function (l, r) { return l.LegalId < r.LegalId ? -1 : l.LegalId > r.LegalId ? 1 : 0; };
					else
						func = function (r, l) { return l.LegalId < r.LegalId ? -1 : l.LegalId > r.LegalId ? 1 : 0; };

					break;

				case "OurLegalId":
					if (sorter.Asc())
						func = function (l, r) { return l.OurLegalId < r.OurLegalId ? -1 : l.OurLegalId > r.OurLegalId ? 1 : 0; };
					else
						func = function (r, l) { return l.OurLegalId < r.OurLegalId ? -1 : l.OurLegalId > r.OurLegalId ? 1 : 0; };

					break;

				case "ContractServiceTypeId":
					if (sorter.Asc())
						func = function (l, r) { return l.ContractServiceTypeId < r.ContractServiceTypeId ? -1 : l.ContractServiceTypeId > r.ContractServiceTypeId ? 1 : 0; };
					else
						func = function (r, l) { return l.ContractServiceTypeId < r.ContractServiceTypeId ? -1 : l.ContractServiceTypeId > r.ContractServiceTypeId ? 1 : 0; };

					break;

				case "ContractTypeId":
					if (sorter.Asc())
						func = function (l, r) { return l.ContractTypeId < r.ContractTypeId ? -1 : l.ContractTypeId > r.ContractTypeId ? 1 : 0; };
					else
						func = function (r, l) { return l.ContractTypeId < r.ContractTypeId ? -1 : l.ContractTypeId > r.ContractTypeId ? 1 : 0; };

					break;

				case "Number":
					if (sorter.Asc())
						func = function (l, r) { return l.Number < r.Number ? -1 : l.Number > r.Number ? 1 : 0; };
					else
						func = function (r, l) { return l.Number < r.Number ? -1 : l.Number > r.Number ? 1 : 0; };

					break;

				case "Date":
					if (sorter.Asc())
						func = function (l, r) { return l.Date < r.Date ? -1 : l.Date > r.Date ? 1 : 0; };
					else
						func = function (r, l) { return l.Date < r.Date ? -1 : l.Date > r.Date ? 1 : 0; };

					break;

				case "BeginDate":
					if (sorter.Asc())
						func = function (l, r) { return l.BeginDate < r.BeginDate ? -1 : l.BeginDate > r.BeginDate ? 1 : 0; };
					else
						func = function (r, l) { return l.BeginDate < r.BeginDate ? -1 : l.BeginDate > r.BeginDate ? 1 : 0; };

					break;

				case "EndDate":
					if (sorter.Asc())
						func = function (l, r) { return l.EndDate < r.EndDate ? -1 : l.EndDate > r.EndDate ? 1 : 0; };
					else
						func = function (r, l) { return l.EndDate < r.EndDate ? -1 : l.EndDate > r.EndDate ? 1 : 0; };

					break;

				case "IsProlongation":
					if (sorter.Asc())
						func = function (l, r) { return l.IsProlongation < r.IsProlongation ? -1 : l.IsProlongation > r.IsProlongation ? 1 : 0; };
					else
						func = function (r, l) { return l.IsProlongation < r.IsProlongation ? -1 : l.IsProlongation > r.IsProlongation ? 1 : 0; };

					break;
			}

			model.ContractsItems.sort(func);
		},
		Css: function (field)
		{
			var sorter = this;
			if (sorter.Field() != field)
				return "";

			return sorter.Asc() ? "asc-sorted" : "desc-sorted";
		}
	};

	model.LegalSorter = {
		Field: ko.observable(),
		Asc: ko.observable(true),
		Sort: function (field)
		{
			var sorter = this;
			if (sorter.Field() == field)
				sorter.Asc(!sorter.Asc());

			sorter.Field(field);
			var func = function () { };
			switch (field)
			{
				case "ID":
					if (sorter.Asc())
						func = function (l, r) { return l.ID() < r.ID() ? -1 : l.ID() > r.ID() ? 1 : 0; };
					else
						func = function (r, l) { return l.ID() < r.ID() ? -1 : l.ID() > r.ID() ? 1 : 0; };

					break;

				case "DisplayName":
					if (sorter.Asc())
						func = function (l, r) { return l.DisplayName() < r.DisplayName() ? -1 : l.DisplayName() > r.DisplayName() ? 1 : 0; };
					else
						func = function (r, l) { return l.DisplayName() < r.DisplayName() ? -1 : l.DisplayName() > r.DisplayName() ? 1 : 0; };

					break;

				case "EnName":
					if (sorter.Asc())
						func = function (l, r) { return l.EnName() < r.EnName() ? -1 : l.EnName() > r.EnName() ? 1 : 0; };
					else
						func = function (r, l) { return l.EnName() < r.EnName() ? -1 : l.EnName() > r.EnName() ? 1 : 0; };

					break;

				case "TIN":
					if (sorter.Asc())
						func = function (l, r) { return l.TIN() < r.TIN() ? -1 : l.TIN() > r.TIN() ? 1 : 0; };
					else
						func = function (r, l) { return l.TIN() < r.TIN() ? -1 : l.TIN() > r.TIN() ? 1 : 0; };

					break;

				case "OGRN":
					if (sorter.Asc())
						func = function (l, r) { return l.OGRN() < r.OGRN() ? -1 : l.OGRN() > r.OGRN() ? 1 : 0; };
					else
						func = function (r, l) { return l.OGRN() < r.OGRN() ? -1 : l.OGRN() > r.OGRN() ? 1 : 0; };

					break;

				case "IsNotResident":
					if (sorter.Asc())
						func = function (l, r) { return l.IsNotResident() < r.IsNotResident() ? -1 : l.IsNotResident() > r.IsNotResident() ? 1 : 0; };
					else
						func = function (r, l) { return l.IsNotResident() < r.IsNotResident() ? -1 : l.IsNotResident() > r.IsNotResident() ? 1 : 0; };

					break;

				case "Balance":
					if (sorter.Asc())
						func = function (l, r) { return l.Balance() < r.Balance() ? -1 : l.Balance() > r.Balance() ? 1 : 0; };
					else
						func = function (r, l) { return l.Balance() < r.Balance() ? -1 : l.Balance() > r.Balance() ? 1 : 0; };

					break;

				case "PaymentBalance":
					if (sorter.Asc())
						func = function (l, r) { return l.PaymentBalance() < r.PaymentBalance() ? -1 : l.PaymentBalance() > r.PaymentBalance() ? 1 : 0; };
					else
						func = function (r, l) { return l.PaymentBalance() < r.PaymentBalance() ? -1 : l.PaymentBalance() > r.PaymentBalance() ? 1 : 0; };

					break;
			}

			model.LegalsItems.sort(func);
		},
		Css: function (field)
		{
			var sorter = this;
			if (sorter.Field() != field)
				return "";

			return sorter.Asc() ? "asc-sorted" : "desc-sorted";
		}
	};

	model.AccountingSorter = {
		Field: ko.observable(),
		Asc: ko.observable(true),
		Sort: function (field)
		{
			var sorter = this;
			if (sorter.Field() == field)
				sorter.Asc(!sorter.Asc());

			sorter.Field(field);
			var func = function () { };
			switch (field)
			{
				case "ID":
					if (sorter.Asc())
						func = function (l, r) { return l.ID < r.ID ? -1 : l.ID > r.ID ? 1 : 0; };
					else
						func = function (r, l) { return l.ID < r.ID ? -1 : l.ID > r.ID ? 1 : 0; };

					break;

				case "OrderNumber":
					if (sorter.Asc())
						func = function (l, r) { return l.OrderNumber < r.OrderNumber ? -1 : l.OrderNumber > r.OrderNumber ? 1 : 0; };
					else
						func = function (r, l) { return l.OrderNumber < r.OrderNumber ? -1 : l.OrderNumber > r.OrderNumber ? 1 : 0; };

					break;

				case "Number":
					if (sorter.Asc())
						func = function (l, r) { return l.Number < r.Number ? -1 : l.Number > r.Number ? 1 : 0; };
					else
						func = function (r, l) { return l.Number < r.Number ? -1 : l.Number > r.Number ? 1 : 0; };

					break;

				case "OrderNumber":
					if (sorter.Asc())
						func = function (l, r) { return l.OrderNumber < r.OrderNumber ? -1 : l.OrderNumber > r.OrderNumber ? 1 : 0; };
					else
						func = function (r, l) { return l.OrderNumber < r.OrderNumber ? -1 : l.OrderNumber > r.OrderNumber ? 1 : 0; };

					break;

				case "ContractNumber":
					if (sorter.Asc())
						func = function (l, r) { return l.ContractNumber < r.ContractNumber ? -1 : l.ContractNumber > r.ContractNumber ? 1 : 0; };
					else
						func = function (r, l) { return l.ContractNumber < r.ContractNumber ? -1 : l.ContractNumber > r.ContractNumber ? 1 : 0; };

					break;

				case "InvoiceNumber":
					if (sorter.Asc())
						func = function (l, r) { return l.InvoiceNumber < r.InvoiceNumber ? -1 : l.InvoiceNumber > r.InvoiceNumber ? 1 : 0; };
					else
						func = function (r, l) { return l.InvoiceNumber < r.InvoiceNumber ? -1 : l.InvoiceNumber > r.InvoiceNumber ? 1 : 0; };

					break;

				case "ActNumber":
					if (sorter.Asc())
						func = function (l, r) { return (l.ActNumber === null) - (r.ActNumber === null) || -(l.ActNumber > r.ActNumber) || (l.ActNumber < r.ActNumber) };
					else
						func = function (r, l) { return (l.ActNumber === null) - (r.ActNumber === null) || -(l.ActNumber > r.ActNumber) || (l.ActNumber < r.ActNumber) };

					break;

				case "InvoiceDate":
					if (sorter.Asc())
						func = function (l, r) { return (l.InvoiceDate === null) - (r.InvoiceDate === null) || -(l.InvoiceDate > r.InvoiceDate) || (l.InvoiceDate < r.InvoiceDate) };
					else
						func = function (r, l) { return (l.InvoiceDate === null) - (r.InvoiceDate === null) || -(l.InvoiceDate > r.InvoiceDate) || (l.InvoiceDate < r.InvoiceDate) };

					break;

				case "ActDate":
					if (sorter.Asc())
						func = function (l, r) { return (l.ActDate === null) - (r.ActDate === null) || -(l.ActDate > r.ActDate) || (l.ActDate < r.ActDate) };
					else
						func = function (r, l) { return (l.ActDate === null) - (r.ActDate === null) || -(l.ActDate > r.ActDate) || (l.ActDate < r.ActDate) };

					break;

				case "Sum":
					if (sorter.Asc())
						func = function (l, r) { return l.Sum < r.Sum ? -1 : l.Sum > r.Sum ? 1 : 0; };
					else
						func = function (r, l) { return l.Sum < r.Sum ? -1 : l.Sum > r.Sum ? 1 : 0; };

					break;


				case "OriginalSum":
					if (sorter.Asc())
						func = function (l, r) { return l.OriginalSum < r.OriginalSum ? -1 : l.OriginalSum > r.OriginalSum ? 1 : 0; };
					else
						func = function (r, l) { return l.OriginalSum < r.OriginalSum ? -1 : l.OriginalSum > r.OriginalSum ? 1 : 0; };

					break;

				case "CreatedDate":
					if (sorter.Asc())
						func = function (l, r) { return l.CreatedDate < r.CreatedDate ? -1 : l.CreatedDate > r.CreatedDate ? 1 : 0; };
					else
						func = function (r, l) { return l.CreatedDate < r.CreatedDate ? -1 : l.CreatedDate > r.CreatedDate ? 1 : 0; };

					break;

			}

			model.AccountingsList.sort(func);
		},
		Css: function (field)
		{
			var sorter = this;
			if (sorter.Field() != field)
				return "";

			return sorter.Asc() ? "asc-sorted" : "desc-sorted";
		}
	};

	model.LegalAccountingSorter = {
		Field: ko.observable(),
		Asc: ko.observable(true),
		Sort: function (field)
		{
			var sorter = this;
			if (sorter.Field() == field)
				sorter.Asc(!sorter.Asc());

			sorter.Field(field);
			var func = function () { };
			switch (field)
			{
				case "ID":
					if (sorter.Asc())
						func = function (l, r) { return l.ID() < r.ID() ? -1 : l.ID() > r.ID() ? 1 : 0; };
					else
						func = function (r, l) { return l.ID() < r.ID() ? -1 : l.ID() > r.ID() ? 1 : 0; };

					break;

				case "OrderNumber":
					if (sorter.Asc())
						func = function (l, r) { return l.OrderNumber() < r.OrderNumber() ? -1 : l.OrderNumber() > r.OrderNumber() ? 1 : 0; };
					else
						func = function (r, l) { return l.OrderNumber() < r.OrderNumber() ? -1 : l.OrderNumber() > r.OrderNumber() ? 1 : 0; };

					break;

				case "OriginalSum":
					if (sorter.Asc())
						func = function (l, r) { return l.OriginalSum() < r.OriginalSum() ? -1 : l.OriginalSum() > r.OriginalSum() ? 1 : 0; };
					else
						func = function (r, l) { return l.OriginalSum() < r.OriginalSum() ? -1 : l.OriginalSum() > r.OriginalSum() ? 1 : 0; };

					break;

				case "Sum":
					if (sorter.Asc())
						func = function (l, r) { return l.Sum() < r.Sum() ? -1 : l.Sum() > r.Sum() ? 1 : 0; };
					else
						func = function (r, l) { return l.Sum() < r.Sum() ? -1 : l.Sum() > r.Sum() ? 1 : 0; };

					break;

				case "Vat":
					if (sorter.Asc())
						func = function (l, r) { return l.Vat() < r.Vat() ? -1 : l.Vat() > r.Vat() ? 1 : 0; };
					else
						func = function (r, l) { return l.Vat() < r.Vat() ? -1 : l.Vat() > r.Vat() ? 1 : 0; };

					break;

				case "AccountingDocumentTypeId":
					if (sorter.Asc())
						func = function (l, r) { return l.AccountingDocumentTypeId() < r.AccountingDocumentTypeId() ? -1 : l.AccountingDocumentTypeId() > r.AccountingDocumentTypeId() ? 1 : 0; };
					else
						func = function (r, l) { return l.AccountingDocumentTypeId() < r.AccountingDocumentTypeId() ? -1 : l.AccountingDocumentTypeId() > r.AccountingDocumentTypeId() ? 1 : 0; };

					break;

				case "CreatedDate":
					if (sorter.Asc())
						func = function (l, r) { return l.CreatedDate() < r.CreatedDate() ? -1 : l.CreatedDate() > r.CreatedDate() ? 1 : 0; };
					else
						func = function (r, l) { return l.CreatedDate() < r.CreatedDate() ? -1 : l.CreatedDate() > r.CreatedDate() ? 1 : 0; };

					break;

				case "Number":
					if (sorter.Asc())
						func = function (l, r) { return l.Number() < r.Number() ? -1 : l.Number() > r.Number() ? 1 : 0; };
					else
						func = function (r, l) { return l.Number() < r.Number() ? -1 : l.Number() > r.Number() ? 1 : 0; };

					break;

				case "InvoiceNumber":
					if (sorter.Asc())
						func = function (l, r) { return (l.InvoiceNumber() === null) - (r.InvoiceNumber() === null) || -(l.InvoiceNumber() > r.InvoiceNumber()) || (l.InvoiceNumber() < r.InvoiceNumber()) };
					else
						func = function (r, l) { return (l.InvoiceNumber() === null) - (r.InvoiceNumber() === null) || -(l.InvoiceNumber() > r.InvoiceNumber()) || (l.InvoiceNumber() < r.InvoiceNumber()) };

					break;

				case "ActNumber":
					if (sorter.Asc())
						func = function (l, r) { return (l.ActNumber() === null) - (r.ActNumber() === null) || -(l.ActNumber() > r.ActNumber()) || (l.ActNumber() < r.ActNumber()) };
					else
						func = function (r, l) { return (l.ActNumber() === null) - (r.ActNumber() === null) || -(l.ActNumber() > r.ActNumber()) || (l.ActNumber() < r.ActNumber()) };

					break;

				case "InvoiceDate":
					if (sorter.Asc())
						func = function (l, r) { return (l.InvoiceDate() === null) - (r.InvoiceDate() === null) || -(l.InvoiceDate() > r.InvoiceDate()) || (l.InvoiceDate() < r.InvoiceDate()) };
					else
						func = function (r, l) { return (l.InvoiceDate() === null) - (r.InvoiceDate() === null) || -(l.InvoiceDate() > r.InvoiceDate()) || (l.InvoiceDate() < r.InvoiceDate()) };

					break;

				case "ActDate":
					if (sorter.Asc())
						func = function (l, r) { return (l.ActDate() === null) - (r.ActDate() === null) || -(l.ActDate() > r.ActDate()) || (l.ActDate() < r.ActDate()) };
					else
						func = function (r, l) { return (l.ActDate() === null) - (r.ActDate() === null) || -(l.ActDate() > r.ActDate()) || (l.ActDate() < r.ActDate()) };

					break;
			}

			model.SelectedLegal().AccountingsItems.sort(func);
		},
		Css: function (field)
		{
			var sorter = this;
			if (sorter.Field() != field)
				return "";

			return sorter.Asc() ? "asc-sorted" : "desc-sorted";
		}
	};

	// #endregion

	// инициализация //////////////////////////////////////////////////////////////////////////////////////////////////////////

	setTimeout(function ()
	{
		model.GetLegals(model.Contractor.ID);
		model.GetWorkgroup(model.Contractor.ID);
		model.GetContracts(model.Contractor.ID);
		model.GetAccountings(model.Contractor.ID);
		model.GetPayments(model.Contractor.ID);
		model.GetEmployees(model.Contractor.ID);
		model.GetOrders(model.Contractor.ID);
		model.GetPricelists(model.Contractor.ID);
	}, 100);

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
	ko.applyBindings(new ContractorViewModel(modelData, {
		GetLegalsItemsUrl: app.urls.GetLegalsItems,
		GetOrdersItemsUrl: app.urls.GetOrdersItems,
		GetPaymentsItemsUrl: app.urls.GetPaymentsItems,
		GetWorkgroupItemsUrl: app.urls.GetWorkgroupItems,
		GetContractsItemsUrl: app.urls.GetContractsItems,
		GetPricelistsItemsUrl: app.urls.GetPricelistsItems,
		GetAccountingsItemsUrl: app.urls.GetAccountingsItems,
		GetContractsByLegalItemsUrl: app.urls.GetContractsByLegalItems,
		GetEmployeesByLegalItemsUrl: app.urls.GetEmployeesByLegalItems,
		GetAccountingsByLegalItemsUrl: app.urls.GetAccountingsByLegalItems,
		GetBankAccountsByLegalItemsUrl: app.urls.GetBankAccountsByLegalItems,
		GetDocumentsByContractItemsUrl: app.urls.GetDocumentsByContractItems,
		GetEmployeesByContractorItemsUrl: app.urls.GetEmployeesByContractorItems,

		EditPersonUrl: app.urls.EditPerson,
		UserDetailsUrl: app.urls.UserDetails,
		LegalDetailsUrl: app.urls.LegalDetails,
		OrderDetailsUrl: app.urls.OrderDetails,
		OrderTemplatesUrl: app.urls.OrderTemplates,
		ContractDetailsUrl: app.urls.ContractDetails,
		ContractorDetailsUrl: app.urls.ContractorDetails,
		OrderAccountingDetailsUrl: app.urls.OrderAccountingDetails,

		GetBankUrl: app.urls.GetBank,
		GetBanksUrl: app.urls.GetBanks,
		GetPersonUrl: app.urls.GetPerson,
		GetPersonsUrl: app.urls.GetPersons,
		GetActionHintUrl: app.urls.GetActionHint,
		GetContractCurrenciesUrl: app.urls.GetContractCurrencies,
		GetLockingContractorInfoUrl: app.urls.GetLockingContractorInfo,

		GetNewLegalUrl: app.urls.GetNewLegal,
		GetNewEmployeeUrl: app.urls.GetNewEmployee,
		GetNewWorkgroupUrl: app.urls.GetNewWorkgroup,
		GetNewBankAccountUrl: app.urls.GetNewBankAccount,

		ToggleIsLockedUrl: app.urls.ToggleIsLocked,

		CreatePersonUrl: app.urls.CreatePerson,
		CreateContractUrl: app.urls.CreateContract,
		DownloadAccountingsFileUrl: app.urls.DownloadAccountingsFile,
		ChangeEmployeePasswordUrl: app.urls.ChangeEmployeePassword,
		ViewDocumentUrl: app.urls.ViewDocument,

		SaveBankAccountUrl: app.urls.SaveBankAccount,
		SaveContractorUrl: app.urls.SaveContractor,
		SaveWorkgroupUrl: app.urls.SaveWorkgroup,
		SaveEmployeeUrl: app.urls.SaveEmployee,
		SaveDocumentUrl: app.urls.SaveDocument,
		SaveContractUrl: app.urls.SaveContract,
		SaveLegalUrl: app.urls.SaveLegal,

		ViewPricelistUrl: app.urls.ViewPricelist
	}), document.getElementById("ko-root"));
});