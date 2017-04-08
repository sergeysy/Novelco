var OrderViewModel = function (source, options)
{
	var model = this;

	model.Options = $.extend({
		//
		InitialSelectedAccounting: null,
		//
		UserDetailsUrl: null,
		OrderDetailsUrl: null,
		ContractorDetailsUrl: null,
		OrderAccountingDetailsUrl: null,

		GetMatchingsUrl: null,
		GetOrdersItemsUrl: null,
		GetWorkgroupItemsUrl: null,
		GetCargoSeatsItemsUrl: null,
		GetOperationsItemsUrl: null,
		GetOrderEventsItemsUrl: null,
		GetRoutePointsItemsUrl: null,
		GetMarksByOrderItemsUrl: null,
		GetRouteSegmentsItemsUrl: null,
		GetContactsByLegalItemsUrl: null,
		GetAccountingsItemsUrl: null,
		GetContractsByLegalItemsUrl: null,
		GetEmployeesByLegalItemsUrl: null,
		GetDocumentsByOrderItemsUrl: null,
		GetLegalsByContractorItemsUrl: null,
		GetOrderTemplatesByProductUrl: null,
		GetAccountingsByLegalItemsUrl: null,
		GetOrderTemplatesByContractUrl: null,
		GetBankAccountsByLegalItemsUrl: null,
		GetPaymentsByAccountingItemsUrl: null,
		GetServicesByAccountingItemsUrl: null,
		GetContractsByContractorItemsUrl: null,
		GetDocumentsByAccountingItemsUrl: null,
		GetRouteSegmentsByAccountingItemsUrl: null,
		GetTemplatedDocumentsByOrderItemsUrl: null,
		GetJointDocumentsByAccountingItemsUrl: null,
		GetJointDocumentsByOrderItemsUrl: null,

		ToggleAccountingInvoiceOkUrl: null,
		ToggleAccountingInvoiceCheckedUrl: null,
		ToggleAccountingInvoiceRejectedUrl: null,
		ToggleAccountingAccountingOkUrl: null,
		ToggleAccountingAccountingCheckedUrl: null,
		ToggleAccountingAccountingRejectedUrl: null,
		ToggleAccountingActOkUrl: null,
		ToggleAccountingActCheckedUrl: null,
		ToggleAccountingActRejectedUrl: null,

		GetBankUrl: null,
		GetPriceUrl: null,
		GetBanksUrl: null,
		GetLegalUrl: null,
		GetPlaceUrl: null,
		GetPlacesUrl: null,
		GetActionHintUrl: null,
		GetContractorUrl: null,
		GetNewServiceUrl: null,
		GetRoutePointUrl: null,
		GetNewDocumentUrl: null,
		GetNewOperationUrl: null,
		GetNewCargoSeatUrl: null,
		GetOrderBalanceUrl: null,
		GetCurrencyRateUrl: null,
		GetNewRoutePointUrl: null,
		GetNewAccountingUrl: null,
		GetAccountingMarksUrl: null,
		GetContractCurrenciesUrl: null,
		GetOrderStatusHistoryUrl: null,
		GetCurrentCurrencyRateUrl: null,

		CloneOrderUrl: null,
		CloseOrderUrl: null,
		ViewDocumentUrl: null,
		OpenDocumentUrl: null,
		ViewPricelistUrl: null,
		ChangeTemplateUrl: null,
		UploadDocumentUrl: null,
		UpdateDocumentUrl: null,
		IsHasVatInvoiceUrl: null,
		SetCurrencyRateUrl: null,
		ChangePayMethodUrl: null,
		RecalculateRouteUrl: null,
		EditRouteContactUrl: null,
		ChangeContractorUrl: null,
		DeleteAccountingUrl: null,
		CheckStatusRulesUrl: null,
		ChangeOrderStatusUrl: null,
		ChangeOrderProductUrl: null,
		UpdateAccountingSumUrl: null,
		DownloadPaymentsFileUrl: null,
		CalculateRouteLengthUrl: null,
		RecalculatePricelistUrl: null,
		ToggleDocumentIsPrintUrl: null,
		RecalculateCargoSeatsUrl: null,
		ViewTemplatedDocumentUrl: null,
		OpenTemplatedDocumentUrl: null,
		DeleteTemplatedDocumentUrl: null,
		UpdateTemplatedDocumentUrl: null,
		RecalculatePaymentPlanDateUrl: null,
		CheckDocumentsIsPrintLimitUrl: null,
		OpenClientAccountingDocumentsUrl: null,
		OpenMergedAccountingDocumentsUrl: null,
		RegenerateAccountingDocumentsUrl: null,

		CreateActUrl: null,
		CreateClaimUrl: null,
		CreateInvoiceUrl: null,
		CreateRequestUrl: null,
		CreateDetailingUrl: null,
		CreateVatInvoiceUrl: null,
		CreateShortLegalUrl: null,
		CreateRouteContactUrl: null,
		CreateAmpleDetailingUrl: null,

		SaveDocumentDeliveryInfoUrl: null,
		SaveRouteSegmentUrl: null,
		SaveRoutePointUrl: null,
		SaveAccountingUrl: null,
		SaveWorkgroupUrl: null,
		SaveOperationUrl: null,
		SaveDocumentUrl: null,
		SaveCargoSeatUrl: null,
		SaveServiceUrl: null,
		SaveOrderUrl: null
	}, options);

	// поля ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	model.ContractorId = ko.observable(source.ContractorId);
	model.PricelistId = ko.observable(source.PricelistId);
	model.Order = ko.mapping.fromJS(source.Order);
	model.FinRepCenter = ko.mapping.fromJS(source.FinRepCenter);
	// Количество связанных сущностей
	model.MinRentability = ko.mapping.fromJS(source.MinRentability);
	model.Rentability = ko.mapping.fromJS(source.Rentability);
	// Списки связанных сущностей
	model.Matchings = ko.observableArray();
	model.MarksItems = ko.observableArray();
	model.OrdersItems = ko.observableArray();
	model.ContractsItems = ko.observableArray();
	model.WorkgroupItems = ko.observableArray();
	model.CargoSeatsItems = ko.observableArray();
	model.OperationsItems = ko.observableArray();
	model.OrderEventsItems = ko.observableArray();
	model.RoutePointsItems = ko.observableArray();
	model.AccountingsItems = ko.observableArray();
	model.RouteSegmentsItems = ko.observableArray();
	model.JointDocumentsItems = ko.observableArray();
	model.ContractorLegalsItems = ko.observableArray();
	model.TemplatedDocumentsItems = ko.observableArray();
	// Справочники
	model.Dictionaries = ko.mapping.fromJS(source.Dictionaries);
	// Выбранное доход/расход из списка
	model.SelectedAccounting = ko.observable();
	// Есть несохраненные изменения
	model.IsDirty = ko.observable(false);
	// переключатель видимости полей
	model.IsEnVisible = ko.observable(false);

	// вычисляемое/подписки ///////////////////////////////////////////////////////////////////////////////////////////////////

	model.ClientContractsItems = ko.pureComputed(function ()
	{
		return ko.utils.arrayFilter(model.ContractsItems(), function (item) { return (item.ContractServiceTypeId() != 2) && (item.OurLegalId() == model.FinRepCenter.OurLegalId()) });
	});

	model.Incomes = ko.pureComputed(function ()
	{
		return ko.utils.arrayFilter(model.AccountingsItems(), function (item) { return item.IsIncome() });
	});

	model.Expenses = ko.pureComputed(function ()
	{
		return ko.utils.arrayFilter(model.AccountingsItems(), function (item) { return !item.IsIncome() });
	});

	model.AccountingDocumentTypeForIncome = ko.pureComputed(function ()
	{
		return ko.utils.arrayFilter(model.Dictionaries.AccountingDocumentType(), function (item) { return item.ID() != 2 });
	});

	model.AccountingDocumentTypeForExpense = ko.pureComputed(function ()
	{
		return ko.utils.arrayFilter(model.Dictionaries.AccountingDocumentType(), function (item) { return item.ID() != 1 });
	});

	model.ContractorId.subscribe(function (newValue)
	{
		model.ContractsItems(model.GetContractsByContractor(newValue));
		model.GetContractorLegals(model.ContractorId);
	});

	model.PaymentsItemsTotalOriginalSum = ko.computed(function ()
	{
		if (model.SelectedAccounting())
			if (model.SelectedAccounting().PaymentsItems().length)
			{
				var sum = 0;
				ko.utils.arrayForEach(model.SelectedAccounting().PaymentsItems(), function (item) { sum = sum + item.OriginalSum() });
				return app.utility.FormatDecimal(sum);
			}

		return 0;
	});

	model.PaymentsItemsTotalSum = ko.computed(function ()
	{
		if (model.SelectedAccounting())
			if (model.SelectedAccounting().PaymentsItems().length)
			{
				var sum = 0;
				ko.utils.arrayForEach(model.SelectedAccounting().PaymentsItems(), function (item) { sum = sum + item.Sum() });
				return app.utility.FormatDecimal(sum);
			}

		return 0;
	});

	model.AccountingsCount = ko.computed(function () { return model.AccountingsItems().length });
	model.RoutePointsCount = ko.computed(function () { return model.RoutePointsItems().length });
	model.CargoSeatsCount = ko.computed(function () { return model.CargoSeatsItems().length });
	model.DocumentsCount = ko.computed(function () { return model.JointDocumentsItems().length });

	// функции ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	model.UserDetailsUrl = function (id) { return model.Options.UserDetailsUrl + "/" + id; };
	model.ContractorDetailsUrl = function (id) { return model.Options.ContractorDetailsUrl + "/" + id; };
	model.OrderAccountingDetailsUrl = function (accountingId) { return model.Options.OrderAccountingDetailsUrl + /* mvc black magic "/" + model.Order.ID() +*/ "?selectedAccountingId=" + accountingId; };
	model.ViewPricelist = function () { window.open(model.Options.ViewPricelistUrl + "/" + model.PricelistId(), "_blank") };

	model.SelectAccounting = function (accounting)
	{
		// load related
		model.LoadAccountingJointDocuments(accounting);
		model.LoadAccountingPayments(accounting);
		model.LoadAccountingRouteSegments(accounting);
		model.LoadAccountingServices(accounting);
		model.LoadAccountingMarks(accounting);

		model.SelectedAccounting(accounting);
	};

	model.UnselectAccounting = function ()
	{
		model.SelectedAccounting(null);
	};

	//#region Extenders ///////////////////////////////////////////////////////////////////////////////////////////////////////

	model.ExtendOrder = function (order)
	{
		order.IsDirty = order.IsDirty || ko.observable(false);

		order.IsDirty.subscribe(function (newValue) { if (newValue) model.IsDirty(true) });

		order.ContractId.subscribe(function (newValue)
		{
			if (newValue)
			{
				var contract = ko.utils.arrayFirst(model.ContractsItems(), function (item) { return item.ID() == newValue });
				if (contract && !contract.IsActive())
					alert("Выбранный договор неактивен (просрочен или заблокирован)!");

				model.RecalculatePricelist();
			}
		});

		order.IsSubscribed = false;
		order.FieldsSubscription = function ()
		{
			var old = "";
			var result = ko.computed(function ()
			{
				if (!order.IsSubscribed)
				{
					//just for subscriptions
					old += order.RequestNumber();
					old += order.RequestDate();
					old += order.OrderTypeId();
					old += order.ContractId();
					old += order.SpecialCustody();
					old += order.EnSpecialCustody();
					old += order.VolumetricRatioId();
					old += order.Danger();
					old += order.EnDanger();
					old += order.TemperatureRegime();
					old += order.EnTemperatureRegime();
					old += order.InsurancePolicy();
					old += order.InsuranceTypeId();
					old += order.UninsuranceTypeId();
					old += order.InvoiceNumber();
					old += order.InvoiceDate();
					old += order.InvoiceSum();
					old += order.InvoiceCurrencyId();
					old += order.Cost();
					old += order.Comment();
					old += order.CargoInfo();
					old += order.EnCargoInfo();

					order.IsSubscribed = true;
					return;
				}

				var qq = ko.toJS(order);	// renew subscription
				order.IsDirty(true);
			});

			return result;
		};

		order.FieldsSubscription();
	};

	model.ExtendAccountings = function (array)
	{
		ko.utils.arrayForEach(array, function (item) { model.ExtendAccounting(item); })
	};

	model.ExtendAccounting = function (accounting)
	{
		accounting.IsDeleted = accounting.IsDeleted || ko.observable(false);
		accounting.IsDirty = accounting.IsDirty || ko.observable(false);
		accounting.IsHasVatInvoice = accounting.IsHasVatInvoice || ko.observable(false);

		accounting.ServicesItems = accounting.ServicesItems || ko.observableArray();
		accounting.JointDocumentsItems = accounting.JointDocumentsItems || ko.observableArray();
		accounting.PaymentsItems = accounting.PaymentsItems || ko.observableArray();
		accounting.RouteSegmentsItems = accounting.RouteSegmentsItems || ko.observableArray();
		accounting.ContractsItems = accounting.ContractsItems || ko.observableArray();

		accounting.PaymentsCount = ko.pureComputed(function () { return accounting.PaymentsItems().length });
		accounting.RouteSegmentsCount = ko.pureComputed(function () { return accounting.RouteSegmentsItems().length });

		if (accounting.ContractorId())
			accounting.ContractsItems(model.GetContractsByContractor(accounting.ContractorId()));

		accounting.ProviderContractsItems = ko.pureComputed(function ()
		{
			return ko.utils.arrayFilter(accounting.ContractsItems(), function (item) { return item.ContractServiceTypeId() != 1 });
		});

		accounting.Marks = accounting.Marks || ko.observable();

		accounting.IsDirty.subscribe(function (newValue) { if (newValue) model.IsDirty(true) });

		accounting.ContractorId.subscribe(function (newValue) { if (newValue) accounting.ContractsItems(model.GetContractsByContractor(newValue)); });
		accounting.ContractId.subscribe(function (newValue)
		{
			var contract = ko.utils.arrayFirst(accounting.ContractsItems(), function (item) { return item.ID() == newValue });
			if (contract)
			{
				accounting.ContractNumber(contract.Number());
				if (!contract.IsActive())
					alert("Выбранный договор неактивен (просрочен или заблокирован)!");

				var orderContract = ko.utils.arrayFirst(model.ContractsItems(), function (item) { return item.ID() == model.Order.ContractId() });
				if (orderContract.OurLegalId() != contract.OurLegalId())
					alert("Наше юрлицо в доходе и  расходе не совпадают!");

				accounting.PayMethodId(contract.PayMethodId());

				// HACK:
				$.ajax({
					type: "POST",
					url: model.Options.IsHasVatInvoiceUrl,
					data: { LegalId: contract.LegalId() },
					success: function (response) { accounting.IsHasVatInvoice(JSON.parse(response).IsHasVatInvoice); }
				});
			}
		});

		accounting.ActNumber.subscribe(function (newValue) { if (newValue != newValue.trim()) accounting.ActNumber(newValue.trim()); });
		accounting.InvoiceNumber.subscribe(function (newValue) { if (newValue != newValue.trim()) accounting.InvoiceNumber(newValue.trim()); });

		accounting.LastCurrencyRate = accounting.CurrencyRate();

		accounting.ActDate.subscribe(function (newValue) { model.ValidateActDate(newValue); });

		// HACK:
		var contract = ko.utils.arrayFirst(accounting.ContractsItems(), function (item) { return item.ID() == accounting.ContractId() });
		if (contract)
			$.ajax({
				type: "POST",
				url: model.Options.IsHasVatInvoiceUrl,
				data: { LegalId: contract.LegalId() },
				success: function (response) { accounting.IsHasVatInvoice(JSON.parse(response).IsHasVatInvoice); }
			});

		accounting.IsSubscribed = false;
		accounting.FieldsSubscription = function ()
		{
			var old = "";

			//one-time dirty flag that gives up its dependencies on first change
			var result = ko.computed(function ()
			{
				//just for subscriptions
				old = "";
				//old += accounting.No();
				old += accounting.AccountingDocumentTypeId();
				old += accounting.AccountingPaymentTypeId();
				old += accounting.AccountingPaymentMethodId();
				old += accounting.SecondSignerEmployeeId();
				old += accounting.VatInvoiceNumber();
				old += accounting.InvoiceNumber();
				old += accounting.ActNumber();
				old += accounting.InvoiceDate();
				old += accounting.ActDate();
				old += accounting.AccountingDate();
				old += accounting.RequestDate();
				old += accounting.CargoLegalId();
				old += accounting.PaymentPlanDate();
				old += accounting.OurLegalId();
				old += accounting.ContractId();
				old += accounting.LegalId();
				old += accounting.PayMethodId();

				old += accounting.IsDeleted();

				if (!accounting.IsSubscribed)
				{
					//next time return true and avoid ko.toJS
					accounting.IsSubscribed = true;
					return;
				}

				var newV = ko.toJS(accounting);	// TEMP:
				accounting.IsDirty(true);
			});

			return result;
		};

		accounting.FieldsSubscription();
	};

	model.ExtendServices = function (array)
	{
		ko.utils.arrayForEach(array, function (item) { model.ExtendService(item); })
	};

	model.ExtendService = function (service)
	{
		service.IsDeleted = service.IsDeleted || ko.observable(false);
	};

	model.ExtendLegals = function (array)
	{
		ko.utils.arrayForEach(array, function (item) { model.ExtendLegal(item); })
	};

	model.ExtendLegal = function (legal)
	{
		legal.IsDeleted = legal.IsDeleted || ko.observable(false);
		legal.IsDirty = legal.IsDirty || ko.observable(false);

		legal.EmployeesItems = legal.EmployeesItems || ko.observableArray();
		legal.ContractsItems = legal.ContractsItems || ko.observableArray();
		legal.BankAccountsItems = legal.BankAccountsItems || ko.observableArray();
		legal.AccountingsItems = legal.AccountingsItems || ko.observableArray();

		legal.IsDirty.subscribe(function (newValue) { if (newValue) model.IsDirty(true) });
		legal.TaxTypeId.subscribe(function () { legal.IsDirty(true); });
		legal.IsNotResident.subscribe(function () { legal.IsDirty(true); });
	};

	model.ExtendEmployees = function (array)
	{
		ko.utils.arrayForEach(array, function (item)
		{
			model.ExtendEmployee(item);
		})
	};

	model.ExtendEmployee = function (employee)
	{
		employee.IsDeleted = employee.IsDeleted || ko.observable(false);
		employee.IsDirty = employee.IsDirty || ko.observable(false);

		employee.IsDirty.subscribe(function (newValue) { model.IsDirty(true) });
		employee.IsDeleted.subscribe(function () { employee.IsDirty(true); });
	};

	model.ExtendCargoSeats = function (array)
	{
		ko.utils.arrayForEach(array, function (item) { model.ExtendCargoSeat(item); })
	};

	model.ExtendCargoSeat = function (seat)
	{
		seat.IsDeleted = seat.IsDeleted || ko.observable(false);
	};

	model.ExtendRoutePoints = function (array)
	{
		ko.utils.arrayForEach(array, function (item) { model.ExtendRoutePoint(item); })
	};

	model.ExtendRoutePoint = function (point)
	{
		point.IsDeleted = point.IsDeleted || ko.observable(false);
	};

	model.ExtendRouteSegments = function (array)
	{
		ko.utils.arrayForEach(array, function (item) { model.ExtendRouteSegment(item); })
	};

	model.ExtendRouteSegment = function (segment)
	{
		segment.IsDeleted = segment.IsDeleted || ko.observable(false);
	};

	model.ExtendDocuments = function (array, parent)
	{
		ko.utils.arrayForEach(array, function (item) { model.ExtendDocument(item, parent); })
	};

	model.ExtendDocument = function (document)
	{
		document.IsDeleted = document.IsDeleted || ko.observable(false);
		document.OrderAccountingName = document.OrderAccountingName || ko.observable("");	// HACK:
		// расширение до Joint document
		document.IsDocument = document.IsDocument || ko.observable(true);
		document.LegalId = document.LegalId || ko.observable(null);
	};

	model.ExtendMarks = function (mark, parent)
	{
		// Invoice
		mark.ToggleInvoiceOk = function ()
		{
			// если метка Проверен не стоит, а метка ОК  стоит, при попытке ее снять, выдавать вопрос "Действительно хотите снять метку ОК?"
			if (!mark.IsInvoiceOk() && !mark.IsInvoiceChecked())
				if (!confirm("Действительно хотите снять метку ОК?"))
					return;

			$.ajax({
				type: "POST",
				url: model.Options.ToggleAccountingInvoiceOkUrl,
				data: { AccountingId: mark.AccountingId() },
				success: function (response)
				{
					var r = JSON.parse(response);
					if (r.Message)
						alert(r.Message)
					else
						parent.Marks(model.ExtendMarks(ko.mapping.fromJSON(response), parent));
				}
			});
		};

		mark.ToggleInvoiceChecked = function ()
		{
			$.ajax({
				type: "POST",
				url: model.Options.ToggleAccountingInvoiceCheckedUrl,
				data: { AccountingId: mark.AccountingId() },
				success: function (response)
				{
					var r = JSON.parse(response);
					if (r.Message)
						alert(r.Message)
					else
						parent.Marks(model.ExtendMarks(ko.mapping.fromJSON(response), parent));
				}
			});
		};

		mark.ToggleInvoiceRejected = function ()
		{
			if (!mark.InvoiceRejectedComment())
			{
				alert("Укажите причину");
				mark.IsInvoiceRejected(false);
				return;
			}

			$.ajax({
				type: "POST",
				url: model.Options.ToggleAccountingInvoiceRejectedUrl,
				data: { AccountingId: mark.AccountingId(), Comment: mark.InvoiceRejectedComment() },
				success: function (response)
				{
					var r = JSON.parse(response);
					if (r.Message)
						alert(r.Message)
					else
						parent.Marks(model.ExtendMarks(ko.mapping.fromJSON(response), parent));
				}
			});
		};

		// Act
		mark.ToggleActOk = function ()
		{
			// если метка Проверен не стоит, а метка ОК  стоит, при попытке ее снять, выдавать вопрос "Действительно хотите снять метку ОК?"
			if (!mark.IsActOk() && !mark.IsActChecked())
				if (!confirm("Действительно хотите снять метку ОК?"))
					return;

			$.ajax({
				type: "POST",
				url: model.Options.ToggleAccountingActOkUrl,
				data: { AccountingId: mark.AccountingId() },
				success: function (response)
				{
					var r = JSON.parse(response);
					if (r.Message)
						alert(r.Message)
					else
					{
						parent.Marks(model.ExtendMarks(ko.mapping.fromJSON(response), parent));
						model.LoadAccountingJointDocuments(model.SelectedAccounting());
					}
				}
			});

			if (!model.SelectedAccounting().ActDate())
				alert("Введите дату акта!");

			if (mark.IsActOk())
				if (model.IsDirty())
				{
					alert("Есть несохраненные изменения, пожалуйста сохраните их.");
					return;
				}

			var contract = ko.utils.arrayFirst(model.ContractsItems(), function (item) { return item.ID() == model.Order.ContractId() });
			var service = model.SelectedAccounting().ServicesItems()[0];
			if (mark.IsActOk())
				if (service && service.CurrencyId() > 1)
					if (contract.OurLegalId() < 3)
						alert("На момент 100% оплаты счета сумма в рублях по Акту и Счет-фактуре может изменится из-за курсовых разниц");
		};

		mark.ToggleActChecked = function ()
		{
			// TEMP:
			//if (!model.SelectedAccounting().CurrencyRate())
			//{
			//	alert("Введите курс пересчета!");
			//	return;
			//}

			$.ajax({
				type: "POST",
				url: model.Options.ToggleAccountingActCheckedUrl,
				data: { AccountingId: mark.AccountingId() },
				success: function (response)
				{
					var r = JSON.parse(response);
					if (r.Message)
						alert(r.Message)
					else
						parent.Marks(model.ExtendMarks(ko.mapping.fromJSON(response), parent));
				}
			});
		};

		mark.ToggleActRejected = function ()
		{
			if (!mark.ActRejectedComment())
			{
				alert("Укажите причину");
				mark.IsActRejected(false);
				return;
			}

			$.ajax({
				type: "POST",
				url: model.Options.ToggleAccountingActRejectedUrl,
				data: { AccountingId: mark.AccountingId(), Comment: mark.ActRejectedComment() },
				success: function (response)
				{
					var r = JSON.parse(response);
					if (r.Message)
						alert(r.Message)
					else
						parent.Marks(model.ExtendMarks(ko.mapping.fromJSON(response), parent));
				}
			});
		};

		// Accounting
		mark.ToggleAccountingOk = function ()
		{
			var pm = ko.utils.arrayFirst(model.Dictionaries.PayMethod(), function (item) { return item.ID() == model.SelectedAccounting().PayMethodId() });
			if (pm && (app.utility.ParseDate(pm.To) < new Date()))
			{
				alert("Метод оплаты просрочен. Вам необходимо обратиться к Администратору для изменения настроек договора Контрагента");
				return;
			}

			$.ajax({
				type: "POST",
				url: model.Options.ToggleAccountingOkUrl,
				data: { AccountingId: mark.AccountingId() },
				success: function (response)
				{
					var r = JSON.parse(response);
					if (r.Message)
						alert(r.Message)
					else
						parent.Marks(model.ExtendMarks(ko.mapping.fromJSON(response), parent));
				}
			});
		};

		mark.ToggleAccountingChecked = function ()
		{
			$.ajax({
				type: "POST",
				url: model.Options.ToggleAccountingCheckedUrl,
				data: { AccountingId: mark.AccountingId() },
				success: function (response)
				{
					var r = JSON.parse(response);
					if (r.Message)
						alert(r.Message)
					else
						parent.Marks(model.ExtendMarks(ko.mapping.fromJSON(response), parent));
				}
			});
		};

		mark.ToggleAccountingRejected = function ()
		{
			if (!mark.AccountingRejectedComment())
			{
				alert("Укажите причину");
				mark.IsAccountingRejected(false);
				return;
			}

			$.ajax({
				type: "POST",
				url: model.Options.ToggleAccountingRejectedUrl,
				data: { AccountingId: mark.AccountingId(), Comment: mark.AccountingRejectedComment() },
				success: function (response)
				{
					var r = JSON.parse(response);
					if (r.Message)
						alert(r.Message)
					else
						parent.Marks(model.ExtendMarks(ko.mapping.fromJSON(response), parent));
				}
			});
		};

		return mark;
	};

	//#endregion

	//#region загрузка связанных сущностей ////////////////////////////////////////////////////////////////////////////////////

	model.GetCargoSeats = function (orderId)
	{
		var id = ko.unwrap(orderId);
		$.ajax({
			type: "POST",
			url: model.Options.GetCargoSeatsItemsUrl,
			data: { OrderId: id },
			success: function (response)
			{
				var list = ko.mapping.fromJSON(response).Items();
				model.ExtendCargoSeats(list);
				model.CargoSeatsItems(list);
			}
		});
	};

	model.GetRoutePoints = function (orderId)
	{
		var id = ko.unwrap(orderId);
		$.ajax({
			type: "POST",
			url: model.Options.GetRoutePointsItemsUrl,
			data: { OrderId: id },
			success: function (response)
			{
				var list = ko.mapping.fromJSON(response).Items()
				model.ExtendRoutePoints(list, model);
				model.RoutePointsItems(list);
			}
		});
	};

	model.GetRouteSegments = function (orderId)
	{
		var id = ko.unwrap(orderId);
		$.ajax({
			type: "POST",
			url: model.Options.GetRouteSegmentsItemsUrl,
			data: { OrderId: id },
			success: function (response)
			{
				var list = ko.mapping.fromJSON(response).Items();
				model.ExtendRouteSegments(list, model);
				model.RouteSegmentsItems(list);
			}
		});
	};

	model.GetJointDocumentsByOrder = function (orderId)
	{
		var id = ko.unwrap(orderId);
		$.ajax({
			type: "POST",
			url: model.Options.GetJointDocumentsByOrderItemsUrl,
			data: { OrderId: id },
			success: function (response)
			{
				var list = ko.mapping.fromJSON(response).Items();
				model.ExtendDocuments(list, model);
				model.JointDocumentsItems(list);
			}
		});
	};

	model.GetTemplatedDocumentsByOrder = function (orderId)
	{
		var id = ko.unwrap(orderId);
		$.ajax({
			type: "POST",
			url: model.Options.GetTemplatedDocumentsByOrderItemsUrl,
			data: { OrderId: id },
			success: function (response)
			{
				model.TemplatedDocumentsItems(ko.mapping.fromJSON(response).Items());
			}
		});
	};

	model.GetContractorLegals = function (contractorId)
	{
		var id = ko.unwrap(contractorId);
		if (id)
			$.ajax({
				type: "POST",
				url: model.Options.GetLegalsByContractorItemsUrl,
				data: { ContractorId: id },
				success: function (response) { model.ContractorLegalsItems(ko.mapping.fromJSON(response).Items()); }
			});
		else
			model.ContractorLegalsItems([]);
	};

	model.GetMarksByOrder = function (orderId)
	{
		var id = ko.unwrap(orderId);
		$.ajax({
			type: "POST",
			url: model.Options.GetMarksByOrderItemsUrl,
			data: { OrderId: id },
			success: function (response)
			{
				model.MarksItems(ko.mapping.fromJSON(response).Items());
			}
		});
	};

	model.GetOperations = function (orderId)
	{
		var id = ko.unwrap(orderId);
		$.ajax({
			type: "POST",
			url: model.Options.GetOperationsItemsUrl,
			data: { OrderId: id },
			success: function (response) { model.OperationsItems(ko.mapping.fromJSON(response).Items()); }
		});
	};

	model.GetOrderEvents = function (orderId)
	{
		var id = ko.unwrap(orderId);
		$.ajax({
			type: "POST",
			url: model.Options.GetOrderEventsItemsUrl,
			data: { OrderId: id },
			success: function (response)
			{
				model.OrderEventsItems(ko.mapping.fromJSON(response).Items());
			}
		});
	};

	model.GetMatchings = function (orderId)
	{
		var id = ko.unwrap(orderId);
		$.ajax({
			type: "POST",
			url: model.Options.GetMatchingsUrl,
			data: { OrderId: id },
			success: function (response) { model.Matchings(ko.mapping.fromJSON(response).Items()); }
		});
	};

	// #endregion

	model.GetWorkgroup = function (orderId)
	{
		var id = ko.unwrap(orderId);
		$.ajax({
			type: "POST",
			url: model.Options.GetWorkgroupItemsUrl,
			data: { OrderId: id },
			success: function (response) { model.WorkgroupItems(ko.mapping.fromJSON(response).Items()); }
		});
	};

	model.GetAccountings = function (orderId)
	{
		var id = ko.unwrap(orderId);
		$.ajax({
			type: "POST",
			url: model.Options.GetAccountingsItemsUrl,
			data: { OrderId: id },
			success: function (response)
			{
				model.AccountingsItems(ko.mapping.fromJSON(response).Items());
				model.ExtendAccountings(model.AccountingsItems());
			}
		});
	};

	model.GetContractsByLegal = function (legalId)
	{
		var id = ko.unwrap(legalId);
		var list;
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.GetContractsByLegalItemsUrl,
			data: { LegalId: id },
			success: function (response)
			{
				list = ko.mapping.fromJSON(response).Items();
			}
		});

		return list;
	};

	model.GetContractsByContractor = function (contractorId, isOnlyClient)
	{
		var id = ko.unwrap(contractorId);
		if (id)
		{
			var list;
			$.ajax({
				type: "POST",
				async: false,
				url: model.Options.GetContractsByContractorItemsUrl,
				data: { ContractorId: id, IsOnlyClient: isOnlyClient },
				success: function (response) { list = ko.mapping.fromJSON(response).Items(); }
			});

			ko.utils.arrayForEach(list, function (item) { item.Currencies = item.Currencies || ko.observableArray(); });

			return list;
		}
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

	//#region Load accounting related data

	model.LoadAccountingServices = function (accounting)
	{
		$.ajax({
			type: "POST",
			url: model.Options.GetServicesByAccountingItemsUrl,
			data: { AccountingId: accounting.ID() },
			success: function (response)
			{
				var list = ko.mapping.fromJSON(response).Items();
				model.ExtendServices(list);
				accounting.ServicesItems(list);

				model.UpdateAccountingCurrency(accounting);
			}
		});
	};

	model.LoadAccountingJointDocuments = function (accounting)
	{
		$.ajax({
			type: "POST",
			url: model.Options.GetJointDocumentsByAccountingItemsUrl,
			data: { AccountingId: accounting.ID() },
			success: function (response)
			{
				var list = ko.mapping.fromJSON(response).Items();
				model.ExtendDocuments(list, accounting);
				accounting.JointDocumentsItems(list);
			}
		});
	};

	model.LoadAccountingPayments = function (accounting)
	{
		$.ajax({
			type: "POST",
			url: model.Options.GetPaymentsByAccountingItemsUrl,
			data: { AccountingId: accounting.ID() },
			success: function (response) { accounting.PaymentsItems(ko.mapping.fromJSON(response).Items()); }
		});
	};

	model.LoadAccountingRouteSegments = function (accounting)
	{
		$.ajax({
			type: "POST",
			url: model.Options.GetRouteSegmentsByAccountingItemsUrl,
			data: { AccountingId: accounting.ID() },
			success: function (response) { accounting.RouteSegmentsItems(ko.mapping.fromJSON(response).Items()); }
		});
	};

	model.LoadAccountingMarks = function (accounting)
	{
		$.ajax({
			type: "POST",
			url: model.Options.GetAccountingMarksUrl,
			data: { AccountingId: accounting.ID() },
			success: function (response) { accounting.Marks(model.ExtendMarks(ko.mapping.fromJSON(response), accounting)); }
		});
	};

	//#endregion

	model.GetContactsByLegal = function (legalId)
	{
		var id = ko.unwrap(legalId);
		var list;
		if (id)
			$.ajax({
				type: "POST",
				async: false,
				url: model.Options.GetContactsByLegalItemsUrl,
				data: { LegalId: id },
				success: function (response) { list = ko.mapping.fromJSON(response).Items(); }
			});

		return list || [];
	};

	model.GetContactsByContractor = function (contractorId)
	{
		var id = ko.unwrap(contractorId);
		var list;
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.GetContactsByContractorItemsUrl,
			data: { ContractorId: id },
			success: function (response) { list = ko.mapping.fromJSON(response).Items(); }
		});

		return list;
	};

	model.GetContractor = function (contractorId)
	{
		var id = ko.unwrap(contractorId);
		var result;
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.GetContractorUrl,
			data: { Id: id },
			success: function (response) { result = ko.mapping.fromJSON(response); }
		});

		return result;
	};

	model.UpdateOrderSum = function ()
	{
		var id = ko.unwrap(model.Order.ID);
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.GetOrderBalanceUrl,
			data: { OrderId: id, Recalculate: true },
			success: function (response)
			{
				var resp = ko.mapping.fromJSON(response);
				if (resp.Message && resp.Message())
					alert(resp.Message());

				if (resp.Income && resp.Income())
					model.Order.Income(resp.Income());

				if (resp.Expense && resp.Expense())
					model.Order.Expense(resp.Expense());

				if (resp.Balance && resp.Balance())
					model.Order.Balance(resp.Balance());

				if (resp.Rentability && resp.Rentability())
					model.Rentability(resp.Rentability());

				if (resp.MinRentability && resp.MinRentability())
					model.MinRentability(resp.MinRentability());
			}
		});
	};

	model.UpdateAccountingSum = function (accounting)
	{
		var id = ko.unwrap(accounting.ID);
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.UpdateAccountingSumUrl,
			data: { AccountingId: id },
			success: function (response)
			{
				var resp = ko.mapping.fromJSON(response);
				if (resp.Message && resp.Message())
					alert(resp.Message());

				if (resp.OriginalSum)
					accounting.OriginalSum(resp.OriginalSum() || 0);

				if (resp.OriginalVat)
					accounting.OriginalVat(resp.OriginalVat() || 0);

				if (resp.Sum)
					accounting.Sum(resp.Sum() || 0);

				if (resp.Vat)
					accounting.Vat(resp.Vat() || 0);
			}
		});
	};

	model.ValidateOrder = function (order)
	{
		if (!order.ContractId())
		{
			alert("Не выбран договор!");
			return false;
		}

		return true;
	};

	//#region Save ////////////////////////////////////////////////////////////////////////////////////////////////////////////

	model.Save = function ()
	{
		if (!model.ValidateOrder(model.Order))
			return;
		var isCanContinue = true;

		// изменения в доход/расходах
		ko.utils.arrayForEach(model.AccountingsItems(), function (accounting)
		{
			if (accounting.IsDirty())
			{
				if (model.IsInsuranceContract(accounting.ContractId()) && !accounting.CargoLegalId())
				{
					alert("Поле грузоперевозчик должно быть заполнено.");
					isCanContinue = false;
					return;
				}

				var dto = {
					ID: accounting.ID(),
					//No: accounting.No(),
					IsIncome: accounting.IsIncome(),
					AccountingDocumentTypeId: accounting.AccountingDocumentTypeId(),
					AccountingPaymentTypeId: accounting.AccountingPaymentTypeId(),
					AccountingPaymentMethodId: accounting.AccountingPaymentMethodId(),
					SecondSignerEmployeeId: accounting.SecondSignerEmployeeId(),
					ContractId: accounting.ContractId(),
					OrderId: accounting.OrderId(),
					PayMethodId: accounting.PayMethodId(),
					CargoLegalId: accounting.CargoLegalId(),
					Number: accounting.Number(),
					ActNumber: accounting.ActNumber(),
					InvoiceNumber: accounting.InvoiceNumber(),
					VatInvoiceNumber: accounting.VatInvoiceNumber(),
					Route: accounting.Route(),
					Comment: accounting.Comment(),
					SecondSignerName: accounting.SecondSignerName(),
					SecondSignerPosition: accounting.SecondSignerPosition(),
					SecondSignerInitials: accounting.SecondSignerInitials(),
					OriginalSum: accounting.OriginalSum(),
					OriginalVat: accounting.OriginalVat(),
					CurrencyRate: (accounting.CurrencyRate() == undefined) ? "null" : accounting.CurrencyRate().toString().replace(/ /g, '').replace(/,/g, '.'),
					Sum: accounting.Sum(),
					Vat: accounting.Vat(),
					//
					InvoiceDate: app.utility.SerializeDateTime(accounting.InvoiceDate()),
					ActDate: app.utility.SerializeDateTime(accounting.ActDate()),
					AccountingDate: app.utility.SerializeDateTime(accounting.AccountingDate()),
					PaymentPlanDate: app.utility.SerializeDateTime(accounting.PaymentPlanDate()),
					RequestDate: app.utility.SerializeDateTime(accounting.RequestDate()),
					//
					//ServiceIdForDetails: accounting.ServiceIdForDetails(),
					OurLegalId: accounting.OurLegalId(),
					LegalId: accounting.LegalId(),

					IsDeleted: accounting.IsDeleted(),
					RouteSegments: []
				};

				// изменения в выбранных маршрутных плечах
				ko.utils.arrayForEach(accounting.RouteSegmentsItems(), function (segment)
				{
					dto.RouteSegments.push({
						AccountingId: segment.AccountingId(),
						RouteSegmentId: segment.RouteSegmentId(),
					});
				});

				$.ajax({
					type: "POST",
					async: false,
					url: model.Options.SaveAccountingUrl,
					data: dto,
					success: function (response)
					{
						var r = JSON.parse(response);
						if (r.Message)
						{
							alert(r.Message);
							isCanContinue = false;
						}
						else
						{
							if (accounting.IsDeleted())
								model.AccountingsItems.remove(accounting);
						}
					}
				});
			}
		});

		if (!isCanContinue)
			return;

		// формирование DTO
		var data = {
			ID: model.Order.ID(),
			ProductId: model.Order.ProductId(),
			ContractId: model.Order.ContractId(),
			RequestNumber: model.Order.RequestNumber(),
			RequestDate: app.utility.SerializeDateTime(model.Order.RequestDate()),
			OrderTypeId: model.Order.OrderTypeId(),
			SpecialCustody: model.Order.SpecialCustody(),
			EnSpecialCustody: model.Order.EnSpecialCustody(),
			VolumetricRatioId: model.Order.VolumetricRatioId(),
			Danger: model.Order.Danger(),
			EnDanger: model.Order.EnDanger(),
			PaidWeight: model.Order.PaidWeight(),
			TemperatureRegime: model.Order.TemperatureRegime(),
			EnTemperatureRegime: model.Order.EnTemperatureRegime(),
			InsurancePolicy: model.Order.InsurancePolicy(),
			InsuranceTypeId: model.Order.InsuranceTypeId(),
			UninsuranceTypeId: model.Order.UninsuranceTypeId(),
			InvoiceNumber: model.Order.InvoiceNumber(),
			InvoiceDate: app.utility.SerializeDateTime(model.Order.InvoiceDate()),
			InvoiceSum: (model.Order.InvoiceSum() == undefined) ? "null" : model.Order.InvoiceSum().toString().replace(/ /g, '').replace(/,/g, '.'),
			InvoiceCurrencyId: model.Order.InvoiceCurrencyId(),
			CargoInfo: model.Order.CargoInfo(),
			EnCargoInfo: model.Order.EnCargoInfo(),
			Cost: model.Order.Cost(),
			Comment: model.Order.Comment(),
			FinRepCenterId: model.Order.FinRepCenterId()
		};

		$.ajax({
			type: "POST",
			url: model.Options.SaveOrderUrl,
			data: JSON.stringify(data),
			dataType: "json",
			contentType: "application/json; charset=utf-8",
			success: function (response)
			{
				if (response.Message)
					model.OpenError(response);
				else
				{
					model.UpdateOrderSum();
					ko.utils.arrayForEach(model.AccountingsItems(), function (item) { item.IsDirty(false) });
					model.Order.IsSubscribed = false;	// reset subscription
					model.Order.IsDirty(false);
					model.IsDirty(false);
				}
			}
		});
	};

	model.SaveService = function (service)
	{
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.SaveServiceUrl,
			data: {
				ID: service.ID(),
				AccountingId: service.AccountingId(),
				ServiceTypeId: service.ServiceTypeId(),
				CurrencyId: service.CurrencyId(),
				VatId: service.VatId(),
				Sum: (service.Sum() == undefined) ? "null" : service.Sum().toString().replace(/ /g, '').replace(/,/g, '.'),
				Price: (service.Price() == undefined) ? "null" : service.Price().toString().replace(/ /g, '').replace(/,/g, '.'),
				Count: (service.Count() == undefined) ? "null" : service.Count().toString().replace(/ /g, '').replace(/,/g, '.'),
				OriginalSum: (service.OriginalSum() == undefined) ? "null" : service.OriginalSum().toString().replace(/ /g, '').replace(/,/g, '.'),
				IsForDetalization: service.IsForDetalization(),

				IsDeleted: service.IsDeleted()
			},
			success: function (response)
			{
				var r = JSON.parse(response);
				if (r.Message)
					alert(r.Message);
				else
				{
					if (r.Service)
					{
						service.ID(r.Service.ID);
						service.Sum(r.Service.Sum);
						service.OriginalSum(r.Service.OriginalSum);
					}
				}
			}
		});
	};

	model.SaveCargoSeat = function (entity)
	{
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.SaveCargoSeatUrl,
			data: {
				ID: entity.ID(),
				OrderId: entity.OrderId(),
				CargoDescriptionId: entity.CargoDescriptionId(),
				SeatCount: entity.SeatCount(),
				Height: (entity.Height() == undefined) ? "null" : entity.Height().toString().replace(/ /g, '').replace(/,/g, '.'),
				Width: (entity.Width() == undefined) ? "null" : entity.Width().toString().replace(/ /g, '').replace(/,/g, '.'),
				Length: (entity.Length() == undefined) ? "null" : entity.Length().toString().replace(/ /g, '').replace(/,/g, '.'),
				Volume: (entity.Volume() == undefined) ? "null" : entity.Volume().toString().replace(/ /g, '').replace(/,/g, '.'),
				GrossWeight: (entity.GrossWeight() == undefined) ? "null" : entity.GrossWeight().toString().replace(/ /g, '').replace(/,/g, '.'),
				PackageTypeId: entity.PackageTypeId(),
				IsStacking: entity.IsStacking(),

				IsDeleted: entity.IsDeleted()
			},
			success: function (response)
			{
				var r = JSON.parse(response);
				if (r.Message)
					alert(r.Message);
				else
				{
					if (entity.ID() == 0)
						entity.ID(r.ID);

					model.RecalculateCargoSeats();
					if (entity.IsDeleted())
						model.CargoSeatsItems.remove(entity);
				}
			},
		});
	};

	model.SaveRoutePoint = function (entity)
	{
		$.ajax({
			type: "POST",
			url: model.Options.SaveRoutePointUrl,
			data: {
				ID: entity.ID(),
				OrderId: entity.OrderId(),
				No: entity.No(),
				RoutePointTypeId: entity.RoutePointTypeId(),
				PlaceId: entity.PlaceId(),
				PlanDate: app.utility.SerializeDateTime(entity.PlanDate()),
				FactDate: app.utility.SerializeDateTime(entity.FactDate()),
				Address: entity.Address(),
				EnAddress: entity.EnAddress(),
				Contact: entity.Contact(),
				EnContact: entity.EnContact(),
				ParticipantLegalId: entity.ParticipantLegalId(),
				ParticipantComment: entity.ParticipantComment(),
				RouteContactID: entity.RouteContactID(),
				EnParticipantComment: entity.EnParticipantComment(),

				IsDeleted: entity.IsDeleted()
			},
			success: function (response)
			{
				var r = JSON.parse(response);
				if (r.Message)
					alert(r.Message);
				else
				{
					if (entity.ID() == 0)
						entity.ID(JSON.parse(response).ID);

					if (entity.IsDeleted())
						model.RoutePointsItems.remove(entity);

					model.EnumerateRoutePoints();
				}
			},
		});
	};

	model.SaveRouteSegment = function (entity)
	{
		$.ajax({
			type: "POST",
			url: model.Options.SaveRouteSegmentUrl,
			data: {
				ID: entity.ID(),
				No: entity.No(),
				FromRoutePointId: entity.FromRoutePointId(),
				ToRoutePointId: entity.ToRoutePointId(),
				TransportTypeId: entity.TransportTypeId(),
				IsAfterBorder: entity.IsAfterBorder(),
				Length: entity.Length(),
				Vehicle: entity.Vehicle(),
				VehicleNumber: entity.VehicleNumber(),
				DriverName: entity.DriverName(),
				DriverPhone: entity.DriverPhone(),

				IsDeleted: entity.IsDeleted()
			},
			success: function (response)
			{
				var r = JSON.parse(response);
				if (r.Message)
					alert(r.Message);
				else
				{
					if (entity.ID() == 0)
						entity.ID(r.ID);

					if (entity.IsDeleted())
						model.RouteSegmentsItems.remove(entity);

					model.RecalculateRoute();
				}
			}
		});
	}

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
				Date: app.utility.SerializeDateTime(entity.Date()),
				DocumentTypeId: entity.DocumentTypeId(),
				Filename: entity.Filename(),
				FileSize: entity.FileSize(),
				IsPrint: entity.IsPrint(),
				IsNipVisible: entity.IsNipVisible(),
				Number: entity.Number(),
				OriginalSentDate: entity.OriginalSentDate(),
				OriginalReceivedDate: entity.OriginalReceivedDate(),
				OrderId: entity.OrderId(),
				AccountingId: entity.AccountingId(),

				IsDeleted: entity.IsDeleted()
			},
			success: function (response)
			{
				var r = JSON.parse(response);
				if (r.Message)
					alert(r.Message);
				else
				{
					if (entity.IsDeleted())
					{
						// TODO:
						model.JointDocumentsItems.remove(entity);
						model.SelectedAccounting().JointDocumentsItems.remove(entity);
					}
					else if (entity.ID() == 0)
						entity.ID(JSON.parse(response).ID);
					else if (model.SelectedAccounting() && !model.SelectedAccounting().IsIncome() && (entity.DocumentTypeId() == 59))// счет от поставщика
					{
						model.SelectedAccounting().InvoiceNumber(entity.Number());
						model.SelectedAccounting().InvoiceDate(entity.Date());
					}
					else if (model.SelectedAccounting() && /*!model.SelectedAccounting().IsIncome() &&*/ (entity.DocumentTypeId() == 10))// акт от поставщика
					{
						model.SelectedAccounting().ActNumber(entity.Number());
						model.SelectedAccounting().ActDate(entity.Date());
					}
					else if (model.SelectedAccounting() && !model.SelectedAccounting().IsIncome() && (entity.DocumentTypeId() == 61))// Счет-фактура от поставщика
					{
						model.SelectedAccounting().VatInvoiceNumber(entity.Number());
					}

					if (model.SelectedAccounting() && (model.SelectedAccounting().ContractorId() == 48) && (entity.DocumentTypeId() == 20))	// Таможня, ДТ
					{
						model.SelectedAccounting().InvoiceNumber(entity.Number());
						model.SelectedAccounting().InvoiceDate(entity.Date());
						model.SelectedAccounting().ActNumber(entity.Number());
						model.SelectedAccounting().ActDate(entity.Date());
					}
				}
			}
		});
	};

	model.SaveOperation = function (entity)
	{
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.SaveOperationUrl,
			data: {
				Id: entity.ID(),
				No: entity.No(),
				Name: entity.Name(),
				OrderId: entity.OrderId(),
				OrderOperationId: entity.OrderOperationId(),
				OperationStatusId: entity.OperationStatusId(),
				ResponsibleUserId: entity.ResponsibleUserId(),
				StartPlanDate: app.utility.SerializeDateTime(entity.StartPlanDate()),
				StartFactDate: app.utility.SerializeDateTime(entity.StartFactDate()),
				FinishPlanDate: app.utility.SerializeDateTime(entity.FinishPlanDate()),
				FinishFactDate: app.utility.SerializeDateTime(entity.FinishFactDate())
			},
			success: function (response)
			{
				var r = JSON.parse(response);
				if (r.Message)
					alert(r.Message);
				else
				{
					entity.OperationStatusId(r.OperationStatusId);
					model.GetOrderEvents(entity.OrderId);
				}
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
				OrderId: model.Order.ID(),
				ParticipantRoleId: entity.ParticipantRoleId(),
				UserId: entity.UserId(),
				FromDate: app.utility.SerializeDateTime(entity.FromDate()),
				ToDate: app.utility.SerializeDateTime(entity.ToDate()),
				IsDeputy: entity.IsDeputy(),
				IsResponsible: entity.IsResponsible()
			},
			success: function (response)
			{
				var r = JSON.parse(response);
				if (r.Message)
					alert(r.Message);
			}
		});
	};

	model.SaveDocumentDeliveryInfo = function (entity, isDocument)
	{
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.SaveDocumentDeliveryInfoUrl,
			data: {
				ID: entity.ID(),
				IsDocument: isDocument,
				ReceivedBy: entity.ReceivedBy(),
				ReceivedNumber: entity.ReceivedNumber()
			},
			success: function (response)
			{
				var r = JSON.parse(response);
				if (r.Message)
					alert(r.Message);
			}
		});
	};

	//#endregion

	//#region Delete //////////////////////////////////////////////////////////////////////////////////////////////////////////

	model.DeleteOperation = function (entity)
	{
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.SaveOperationUrl,
			data: {
				ID: entity.ID(),
				OrderId: entity.OrderId(),
				IsDeleted: true
			},
			success: function (response)
			{
				var r = JSON.parse(response);
				if (r.Message)
					alert(r.Message);
				else
					model.OperationsItems.remove(entity);
			}
		});
	};

	model.DeleteWorkgroup = function (entity)
	{
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.SaveWorkgroupUrl,
			data: {
				ID: entity.ID(),
				IsDeleted: true
			},
			success: function (response)
			{
				var r = JSON.parse(response);
				if (r.Message)
					alert(r.Message);
				else
					model.WorkgroupItems.remove(entity);
			}
		});
	};

	model.DeleteAccounting = function (data)
	{
		$.ajax({
			type: "POST",
			url: model.Options.DeleteAccountingUrl,
			data: { AccountingId: data.ID() },
			success: function (response)
			{
				var counters = JSON.parse(response);
				model.OpenConfirm("Вы уверены, что хотите удалить этот доход/расход? Это приведет к удалению всех записей, содержащихся в нем: <br/> " + counters.ServicesCount + "  услуг <br/> " + counters.FilesCount + " финансовых документов <br/> " + counters.DocumentsCount + " прикрепленных документов <br/> " + counters.PaymentsCount + " поступлений", function ()
				{
					$.ajax({
						type: "POST",
						url: model.Options.DeleteAccountingUrl,
						data: { AccountingId: data.ID(), IsCascade: true },
						success: function (response)
						{
							var r = JSON.parse(response);
							if (r.Message)
								alert(r.Message);
							else
							{
								alert("Доход/расход удален.");
								if (model.SelectedAccounting())
									model.UpdateAccountingSum(model.SelectedAccounting());	// TODO: непонятно, возможно ненужно

								model.UpdateOrderSum();
								model.AccountingsItems.remove(data);
							}
						}
					})
				})
			}
		})
	};

	model.DeleteService = function (service)
	{
		service.IsDeleted(true);
		model.SaveService(service);
		model.UpdateAccountingSum(model.SelectedAccounting());
		model.UpdateAccountingCurrency(model.SelectedAccounting());
		model.UpdateOrderSum();
		model.SelectedAccounting().ServicesItems.remove(service);
	};

	model.DeleteCargoSeat = function (seat)
	{
		seat.IsDeleted(true);
		model.SaveCargoSeat(seat);
	};

	model.DeleteRoutePoint = function (point)
	{
		point.IsDeleted(true);
		model.SaveRoutePoint(point);
	};

	model.DeleteRouteSegment = function (segment)
	{
		segment.IsDeleted(true);
		model.SaveRouteSegment(segment);
	};

	model.DeleteDocument = function (document)
	{
		document.IsDeleted(true);
		model.SaveDocument(document);
	};

	model.DeleteTemplatedDocument = function (document)
	{
		$.ajax({
			type: "POST",
			url: model.Options.DeleteTemplatedDocumentUrl,
			data: { ID: document.ID() },
			success: function (response)
			{
				var r = JSON.parse(response);
				if (r.Message)
					model.OpenError(r);
				else
				{
					model.JointDocumentsItems.remove(document);
					if (model.SelectedAccounting())
						model.SelectedAccounting().JointDocumentsItems.remove(document);
				}
			}
		});
	};

	//#endregion

	model.GetEmployeeName = function (array, id)
	{
		var emp = ko.utils.arrayFirst(array(), function (item) { return item.ID() == id() });
		if (emp)
			return emp.Name();

		return "";
	};

	model.GetContractCurrenciesDisplay = function (contract)
	{
		var result = "";
		if (contract.Currencies)
			ko.utils.arrayForEach(contract.Currencies(), function (item) { result += app.utility.GetDisplay(model.Dictionaries.Currency, item.CurrencyId()) + " " });

		return result;
	};

	model.GetPointDisplay = function (pointId)
	{
		var id = ko.unwrap(pointId);
		var result = "";
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.GetRoutePointUrl,
			data: { RoutePointId: id },
			success: function (response) { result = JSON.parse(response).Place; }
		});

		return result;
	};

	model.GetServiceDisplay = function (sitem, data)
	{
		if (data)
		{
			var svc = ko.utils.arrayFirst(data.ServicesItems(), function (item) { return item.ID() == sitem.ID() });
			return (svc) ? app.utility.GetDisplay(model.Dictionaries.ServiceType, svc.ServiceTypeId()) : "";
		}

		return "";
	};

	model.GetPlace = function (placeId)
	{
		var id = ko.unwrap(placeId);
		var result;
		if (id)
			$.ajax({
				type: "POST",
				async: false,
				url: model.Options.GetPlaceUrl,
				data: { Id: id },
				success: function (response) { result = ko.mapping.fromJSON(response); }
			});

		return result;
	};

	model.GetPlaceDisplay = function (placeId)
	{
		var id = ko.unwrap(placeId);
		var result = "";
		if (id)
			$.ajax({
				type: "POST",
				async: false,
				url: model.Options.GetPlaceUrl,
				data: { Id: id },
				success: function (response) { result = ko.mapping.fromJSON(response).Name(); }
			});

		return result;
	};

	model.GetLegal = function (legalId)
	{
		var id = ko.unwrap(legalId);
		var result;
		if (id)
			$.ajax({
				type: "POST",
				async: false,
				url: model.Options.GetLegalUrl,
				data: { Id: id },
				success: function (response) { result = ko.mapping.fromJSON(response); }
			});

		return result;
	};

	model.GetRoutePoint = function (pointId)
	{
		var id = ko.unwrap(pointId);
		var result;
		if (id)
			$.ajax({
				type: "POST",
				async: false,
				url: model.Options.GetRoutePointUrl,
				data: { RoutePointId: id },
				success: function (response) { result = ko.mapping.fromJSON(response); }
			});

		return result;
	};

	model.GetServiceKind = function (serviceTypeId)
	{
		var id = ko.unwrap(serviceTypeId);
		var type = ko.utils.arrayFirst(model.Dictionaries.ServiceType(), function (item) { return item.ID() == id });
		if (type)
			return type.Kind();

		return "";
	};

	model.GetOperationKind = function (operationId)
	{
		var id = ko.unwrap(operationId);
		var kind = ko.utils.arrayFirst(model.Dictionaries.OrderOperation(), function (item) { return item.ID() == id });
		if (kind)
			return app.utility.GetDisplay(model.Dictionaries.OperationKind, kind.OperationKindId());

		return "";
	};

	model.IsAccountingIncome = function (accountingId)
	{
		var id = ko.unwrap(accountingId);
		var accounting = ko.utils.arrayFirst(model.AccountingsItems(), function (item) { return item.ID() == id });
		return accounting ? accounting.IsIncome() : true;	// HACK:
	};

	model.GetAccountingNumber = function (accountingId)
	{
		var id = ko.unwrap(accountingId);
		var accounting = ko.utils.arrayFirst(model.AccountingsItems(), function (item) { return item.ID() == id });
		return accounting ? accounting.Number() : "";	// HACK:
	};

	model.GotoAccounting = function (accountingId)
	{
		var id = ko.unwrap(accountingId);
		var selectedTab = $(".nav li a[href='#Accountings']");
		selectedTab.trigger('click', true);
		model.UnselectAccounting();
		model.SelectAccounting(ko.utils.arrayFirst(model.AccountingsItems(), function (item) { return item.ID() == id }));
	};

	model.ChangeProduct = function ()
	{
		window.location = model.Options.ChangeOrderProductUrl + "?orderId=" + model.Order.ID();
	};

	model.ChangeTemplate = function (orderId, templateId)
	{
		$.ajax({
			type: "POST",
			url: model.Options.ChangeTemplateUrl,
			data: { OrderId: orderId, OrderTemplateId: templateId },
			success: function (response)
			{
				var r = JSON.parse(response);
				if (r.Message)
					alert(r.Message);
				else
				{
					model.Order.OrderTemplateId(templateId);
					model.GetOperations(model.Order.ID());
				}
			}
		});
	};

	model.ChangeOrderStatus = function (orderId, statusId, reason, closeDate)
	{
		if (statusId == 7)	// мотивация
			if (ko.utils.arrayFirst(model.AccountingsItems(), function (item) { return item.IsIncome() && (app.utility.ParseDate(item.ActDate) > closeDate) }))
				if (!confirm("Дата статуса 'Мотивация' меньше даты акта в Доходе. Подтверждаете дату статуса 'Мотивация'?"))
					return;

		$.ajax({
			type: "POST",
			url: model.Options.ChangeOrderStatusUrl,
			data: { OrderId: orderId, OrderStatusId: statusId, CloseDate: app.utility.SerializeDateTime(closeDate), Reason: reason },
			success: function (response)
			{
				var r = JSON.parse(response);
				if (r.Message)
					alert(r.Message);
				else
					model.Order.OrderStatusId(statusId);
			}
		});
	};

	model.ChangeContractor = function (orderId, contractorId)
	{
		$.ajax({
			type: "POST",
			url: model.Options.ChangeContractorUrl,
			data: { OrderId: orderId, ContractorId: contractorId },
			success: function (response)
			{
				var r = JSON.parse(response);
				if (r.Message)
					alert(r.Message);
				else
				{
					model.ContractorId(contractorId);
					model.GetWorkgroup(model.Order.ID());
				}
			}
		});
	};

	model.ChangePayMethod = function (accountingId, payMethodId)
	{
		if (model.IsDirty())
		{
			alert("Есть несохраненные изменения, пожалуйста сохраните их.");
			return;
		}

		$.ajax({
			type: "POST",
			url: model.Options.ChangePayMethodUrl,
			data: { AccountingId: accountingId, PayMethodId: payMethodId },
			success: function (response)
			{
				var r = JSON.parse(response);
				if (r.Message)
					alert(r.Message);
				else
					model.SelectedAccounting().PayMethodId(payMethodId);
			}
		});
	};

	model.CloneOrder = function (orderId, isCopyCargo, isCopyRoute, isCopyWorkgroup)
	{
		$.ajax({
			type: "POST",
			url: model.Options.CloneOrderUrl,
			data: { OrderId: orderId, IsCopyCargo: isCopyCargo, IsCopyRoute: isCopyRoute, IsCopyWorkgroup: isCopyWorkgroup },
			success: function (response)
			{
				var r = JSON.parse(response);
				if (r.Message)
					alert(r.Message);
				else
					window.open(model.Options.OrderDetailsUrl + "/" + r.ID, "_blank");
			}
		});
	};

	model.IsInsuranceContract = function (contractId)
	{
		var id = ko.unwrap(contractId);
		var contract = ko.utils.arrayFirst(model.SelectedAccounting().ContractsItems(), function (item) { return item.ID() == id })
		if (contract)
			return contract.ContractTypeId() == 4;	// Генеральный договор страхования грузов

		return false;
	};

	model.UpdateCurrencyRate = function (accounting)
	{
		if (accounting.IsIncome())
			if (accounting.CurrencyRate() != accounting.LastCurrencyRate)
			{
				// проверить наличие отметки
				if (model.SelectedAccounting().Marks().IsActChecked())
				{
					alert("Нельзя, Акт уже проверен.");
					accounting.CurrencyRate(accounting.LastCurrencyRate);
					return;
				}

				$.ajax({
					type: "POST",
					url: model.Options.SetCurrencyRateUrl,
					data: { AccountingId: accounting.ID(), NewRate: accounting.CurrencyRate().toString().replace(/ /g, '').replace(/,/g, '.') },
					success: function (response)
					{
						var r = JSON.parse(response);
						if (r.Message)
							alert(r.Message);
						else
						{
							accounting.LastCurrencyRate = accounting.CurrencyRate();
							model.LoadAccountingServices(accounting);
							model.RegenerateAccountingDocuments(accounting);
						}
					}
				});
			}
	};

	// #region matching edit modal ////////////////////////////////////////////////////////////////////////////////////////////

	var matchingModalSelector = "#matchingEditModal";

	model.OpenMatchingEdit = function (data)
	{
		model.MatchingEditModal.CurrentItem(data);
		$(matchingModalSelector).modal("show");
		$(matchingModalSelector).draggable({ handle: ".modal-header" });;
		model.MatchingEditModal.Init();
	};

	model.MatchingEditModal = {
		CurrentItem: ko.observable(),
		OnClosed: null,
		Close: function (self, e)
		{
			$(matchingModalSelector).modal("hide");
			if (self.OnClosed != null)
				self.OnClosed();
		},
		AddIncomeMatch: function (self, e)
		{
			self.CurrentItem().Incomes.push({ ID: ko.observable(0), AccountingId: ko.observable(0), Sum: ko.observable(0) });
		},
		DeleteIncomeMatch: function () { },
		Init: function ()
		{
		},
		Validate: function (self)
		{
			return true;
		},
		Done: function (self, e)
		{
			if (!self.Validate(self))
				return;

			$(matchingModalSelector).modal("hide");
			//model.SaveRouteSegment(self.CurrentItem());
		}
	};

	// #endregion

	// #region workgroup create/edit modal ////////////////////////////////////////////////////////////////////////////////////

	var workgroupModalSelector = "#workgroupEditModal";

	model.OpenWorkgroupCreate = function ()
	{
		$.ajax({
			type: "POST",
			url: model.Options.GetNewWorkgroupUrl,
			data: { OrderId: model.Order.ID() },
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
					model.WorkgroupEditModal.OnClosed = function () { model.WorkgroupItems.remove(data); };
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
		Init: function ()
		{
		},
		OnClosed: null,
		Close: function (self, e)
		{
			$(workgroupModalSelector).modal("hide");
			if (self.OnClosed != null)
				self.OnClosed();
		},
		Done: function (self, e)
		{
			model.SaveWorkgroup(self.CurrentItem());
			self.CurrentItem(null);
			$(workgroupModalSelector).modal("hide");
		}
	};

	// #endregion

	// #region service create/edit modal //////////////////////////////////////////////////////////////////////////////////////

	var serviceModalSelector = "#serviceEditModal";

	model.OpenServiceCreate = function ()
	{
		if (model.IsDirty())
		{
			alert("Есть несохраненные изменения, пожалуйста сохраните их.");
			return;
		}

		$.ajax({
			type: "POST",
			url: model.Options.GetNewServiceUrl,
			data: { AccountingId: model.SelectedAccounting().ID() },
			success: function (response)
			{
				var r = JSON.parse(response);
				if (r.Message)
					model.OpenError(r);
				else
				{
					var data = ko.mapping.fromJSON(response);
					model.ExtendService(data);
					model.SelectedAccounting().ServicesItems.push(data);
					model.ServiceEditModal.CurrentItem(data);
					$(serviceModalSelector).modal("show");
					$(serviceModalSelector).draggable({ handle: ".modal-header" });
					model.ServiceEditModal.OnClosed = function ()
					{
						data.IsDeleted(true);
						model.SaveService(data);
						model.SelectedAccounting().ServicesItems.remove(data);
					};
					model.ServiceEditModal.Init();
				}
			}
		});
	};

	model.OpenServiceEdit = function (service)
	{
		if (model.IsDirty())
		{
			alert("Есть несохраненные изменения, пожалуйста сохраните их.");
			return;
		}

		model.ServiceEditModal.CurrentItem(service);
		$(serviceModalSelector).modal("show");
		$(serviceModalSelector).draggable({ handle: ".modal-header" });;
		model.ServiceEditModal.OnClosed = null;
		model.ServiceEditModal.Init();
	};

	model.ServiceEditModal = {
		CurrentItem: ko.observable(),
		CurrencyRates: ko.observableArray(),
		FixedRate: ko.observable(),
		CurrentRate: ko.observable(),
		OnClosed: null,
		Close: function (self, e)
		{
			$(serviceModalSelector).modal("hide");
			if (self.OnClosed != null)
				self.OnClosed();
		},
		Init: function ()
		{
			var self = model.ServiceEditModal;
			self.FixedRate(model.SelectedAccounting().CurrencyRate());
			self.CurrentItem().Count.subscribe(function () { self.RecalculateSum(); });
			self.CurrentItem().Price.subscribe(function () { self.RecalculateSum(); });
			self.CurrentItem().CurrencyId.subscribe(function () { self.RecalculateSum(); });
			self.CurrentItem().ServiceTypeId.subscribe(function (newValue)
			{
				$.ajax({
					type: "POST",
					url: model.Options.GetPriceUrl,
					data: { ServiceTypeId: newValue, AccountingId: model.SelectedAccounting().ID() },
					success: function (response)
					{
						var r = JSON.parse(response);
						if (r.Message)
							alert(r.Message);
						else
						{
							var data = JSON.parse(response);
							self.CurrentItem().VatId(data.VatId);
							self.CurrentItem().Price(data.Price);
							self.CurrentItem().Count(data.Count);
							if (data.CurrencyId)
								self.CurrentItem().CurrencyId(data.CurrencyId);
						}
					}
				});
			});

			$.ajax({
				type: "POST",
				url: model.Options.GetCurrencyRateUrl,
				data: { Date: app.utility.SerializeDateTime(model.SelectedAccounting().AccountingDate()) },
				success: function (response)
				{
					var r = JSON.parse(response);
					if (r.Message)
						alert(r.Message);
					else
					{
						self.CurrencyRates(ko.mapping.fromJSON(response).Items());
						self.RecalculateSum();
					}
				}
			});
		},
		Validate: function (self)
		{
			var contract;
			if (model.SelectedAccounting().IsIncome())
				contract = ko.utils.arrayFirst(model.ContractsItems(), function (item) { return item.ID() == model.Order.ContractId() });
			else
				contract = ko.utils.arrayFirst(model.SelectedAccounting().ContractsItems(), function (item) { return item.ID() == model.SelectedAccounting().ContractId() });

			var currencies = contract.Currencies();
			var hasRub = ko.utils.arrayFirst(currencies, function (item) { return item.CurrencyId() == 1 });
			if (!hasRub)
			{
				if (!ko.utils.arrayFirst(currencies, function (item) { return item.CurrencyId() == self.CurrentItem().CurrencyId() }))
				{
					alert("Выбранная валюта не соответствует валюте договора");
					return false;
				}
			}

			var fs = model.SelectedAccounting().ServicesItems()[0];
			if (fs && fs.CurrencyId() != self.CurrentItem().CurrencyId())
			{
				alert("Выбранная валюта не соответствует валюте других услуг");
				return false;
			}

			return true;
		},
		RecalculateSum: function ()
		{
			var self = model.ServiceEditModal;
			var count = app.utility.ParseDecimal(self.CurrentItem().Count());
			var price = app.utility.ParseDecimal(self.CurrentItem().Price());
			var originalSum = count * price;
			self.CurrentItem().OriginalSum(app.utility.FormatDecimal(originalSum));

			if (self.FixedRate())
			{
				self.CurrentItem().Sum(app.utility.FormatDecimal(originalSum * self.FixedRate()));
				return;
			}

			var rate = 1;
			if (self.CurrentItem().CurrencyId() > 1)	// если не рубль
				try
				{
					rate = ko.utils.arrayFirst(self.CurrencyRates(), function (item) { return item.CurrencyId() == self.CurrentItem().CurrencyId() }).Rate() * 1.015;
				}
				catch (ex)
				{
				}

			self.CurrentItem().Sum(app.utility.FormatDecimal(originalSum * rate));
			self.CurrentRate(rate);
		},
		Done: function (self, e)
		{
			if (!self.Validate(self))
				return;

			// сохранить изменения
			model.SaveService(self.CurrentItem());
			model.UpdateAccountingSum(model.SelectedAccounting());
			model.UpdateAccountingCurrency(model.SelectedAccounting());
			model.AskRegenerateAccountingDocuments(model.SelectedAccounting());
			model.UpdateOrderSum();
			self.CurrentItem(null);
			$(serviceModalSelector).modal("hide");
		}
	};

	// #endregion

	// #region cargo seat create/edit modal ///////////////////////////////////////////////////////////////////////////////////

	var cargoSeatModalSelector = "#cargoSeatEditModal";

	model.OpenCargoSeatCreate = function ()
	{
		$.ajax({
			type: "POST",
			url: model.Options.GetNewCargoSeatUrl,
			data: { OrderId: model.Order.ID() },
			success: function (response)
			{
				var r = JSON.parse(response);
				if (r.Message)
					model.OpenError(r);
				else
				{
					var data = ko.mapping.fromJSON(response);
					model.ExtendCargoSeat(data);
					model.CargoSeatsItems.push(data);
					model.CargoSeatEditModal.CurrentItem(data);
					$(cargoSeatModalSelector).modal("show");
					$(cargoSeatModalSelector).draggable({ handle: ".modal-header" });
					model.CargoSeatEditModal.OnClosed = function () { model.CargoSeatsItems.remove(data); };
					model.CargoSeatEditModal.Init();
				}
			}
		});
	};

	model.OpenCargoSeatEdit = function (seat)
	{
		model.CargoSeatEditModal.CurrentItem(seat);
		$(cargoSeatModalSelector).modal("show");
		$(cargoSeatModalSelector).draggable({ handle: ".modal-header" });
		model.CargoSeatEditModal.OnClosed = null;
		model.CargoSeatEditModal.Init();
	};

	model.CargoSeatEditModal = {
		CurrentItem: ko.observable(),
		Init: function () { },
		Validate: function (self)
		{
			if ((!self.CurrentItem().CargoDescriptionId()) ||
				(!self.CurrentItem().PackageTypeId()) ||
				(!self.CurrentItem().SeatCount()) ||
				(!self.CurrentItem().GrossWeight()) ||
				(!self.CurrentItem().PackageTypeId()))
			{
				alert("Не все обязательные поля заполнены!");
				return false;
			}

			return true;
		},
		OnClosed: null,
		Close: function (self, e)
		{
			$(cargoSeatModalSelector).modal("hide");
			if (self.OnClosed != null)
				self.OnClosed();
		},
		Done: function (self, e)
		{
			if (!self.Validate(self))
				return;

			// если заполнены ДШВ, рассчитывать объем автоматически 
			if (self.CurrentItem().Width() && self.CurrentItem().Height() && self.CurrentItem().Length() && !self.CurrentItem().Volume())
				self.CurrentItem().Volume(self.CurrentItem().Width() * self.CurrentItem().Height() * self.CurrentItem().Length());

			model.SaveCargoSeat(self.CurrentItem());
			self.CurrentItem(null);
			$(cargoSeatModalSelector).modal("hide");
		}
	};

	// #endregion

	// #region route point create/edit modal //////////////////////////////////////////////////////////////////////////////////

	var routePointModalSelector = "#routePointEditModal";

	model.OpenRoutePointCreate = function ()
	{
		$.ajax({
			type: "POST",
			url: model.Options.GetNewRoutePointUrl,
			data: { OrderId: model.Order.ID() },
			success: function (response)
			{
				var r = JSON.parse(response);
				if (r.Message)
					model.OpenError(r);
				else
				{
					data = ko.mapping.fromJSON(response);

					model.ExtendRoutePoint(data, model);
					model.RoutePointsItems.push(data);
					model.RoutePointEditModal.CurrentItem(data);
					$(routePointModalSelector).modal("show");
					$(routePointModalSelector).draggable({ handle: ".modal-header" });
					model.RoutePointEditModal.OnClosed = function () { model.RoutePointsItems.remove(data); };
					model.RoutePointEditModal.Init();
				}
			}
		});
	};

	model.OpenRoutePointEdit = function (point)
	{
		model.RoutePointEditModal.CurrentItem(point);
		$(routePointModalSelector).modal("show");
		$(routePointModalSelector).draggable({ handle: ".modal-header" });;
		model.RoutePointEditModal.OnClosed = null;
		model.RoutePointEditModal.Init();
	};

	model.RoutePointEditModal = {
		CurrentItem: ko.observable(),
		CurrentPlace: ko.observable(),
		CurrentLegal: ko.observable(),
		CurrentContacts: ko.observableArray([]),
		CurrentContact: ko.observable(),
		// переключатель видимости полей
		IsEnVisible: ko.observable(false),
		ToggleEnVisible: function () { model.RoutePointEditModal.IsEnVisible(!model.RoutePointEditModal.IsEnVisible()); },
		Init: function ()
		{
			var self = model.RoutePointEditModal;
			// получить место
			self.CurrentPlace(model.GetPlace(self.CurrentItem().PlaceId));
			// получить юрлицо
			self.CurrentLegal(model.GetLegal(self.CurrentItem().ParticipantLegalId));
			self.CurrentItem().ParticipantLegalId.subscribe(function (newValue)
			{
				self.CurrentLegal(model.GetLegal(self.CurrentItem().ParticipantLegalId))
				self.CurrentContacts(model.GetContactsByLegal(self.CurrentItem().ParticipantLegalId));
			});
			// получить контактные лица юрлица
			self.CurrentContacts(model.GetContactsByLegal(self.CurrentItem().ParticipantLegalId));
			// выбрать текущее контактное лицо
			if (self.CurrentContacts.length)
				self.CurrentContact(ko.utils.arrayFirst(self.CurrentContacts(), function (item) { return item.ID() == self.CurrentItem().RouteContactID() }));
			else
				self.CurrentContact(null);

			self.CurrentItem().RouteContactID.subscribe(function (newValue)
			{
				var selected = ko.utils.arrayFirst(self.CurrentContacts(), function (item) { return item.ID() == newValue });
				self.CurrentContact(selected);
				if (selected && selected.Address())
					self.CurrentItem().Address(selected.Address());

				if (selected && selected.EnAddress())
					self.CurrentItem().EnAddress(selected.EnAddress());
			});

			//
			var selected = ko.utils.arrayFirst(self.CurrentContacts(), function (item) { return item.ID() == self.CurrentItem().RouteContactID() });
			self.CurrentContact(selected);
			if (selected && selected.Address())
				self.CurrentItem().Address(selected.Address());

			if (selected && selected.EnAddress())
				self.CurrentItem().EnAddress(selected.EnAddress());

			$("#placeAutocomplete").autocomplete({
				source: model.Options.GetPlacesUrl,
				appendTo: routePointModalSelector,
				select: function (e, ui)
				{
					self.CurrentItem().PlaceId(ui.item.entity.ID);
					self.CurrentPlace(ui.item.entity);
				}
			});
		},
		LoadRouteContacts: function ()
		{
			var self = model.RoutePointEditModal;
			// получить контактные лица юрлица
			self.CurrentContacts(model.GetContactsByLegal(self.CurrentItem().ParticipantLegalId));
			// выбрать текущее контактное лицо
			if (self.CurrentContacts.length)
				self.CurrentContact(ko.utils.arrayFirst(self.CurrentContacts(), function (item) { return item.ID() == self.CurrentItem().RouteContactID() }));
		},
		Validate: function ()
		{
			// TODO:
			return true;
		},
		OnClosed: null,
		Close: function (self, e)
		{
			$(routePointModalSelector).modal("hide");
			if (self.OnClosed != null)
				self.OnClosed();

			self.CurrentItem(null);
			self.CurrentPlace(null);
			self.CurrentLegal(null);
			self.CurrentContacts([]);
			self.CurrentContact(null);
		},
		Done: function (self, e)
		{
			if (!self.Validate())
				return;

			$(routePointModalSelector).modal("hide");
			model.SaveRoutePoint(self.CurrentItem());
			self.CurrentItem(null);
			self.CurrentPlace(null);
			self.CurrentLegal(null);
			self.CurrentContacts([]);
			self.CurrentContact(null);
		}
	};

	// #endregion

	// #region route segment edit modal ///////////////////////////////////////////////////////////////////////////////////////

	var routeSegmentModalSelector = "#routeSegmentEditModal";

	model.OpenRouteSegmentEdit = function (segment)
	{
		model.RouteSegmentEditModal.CurrentItem(segment);
		$(routeSegmentModalSelector).modal("show");
		$(routeSegmentModalSelector).draggable({ handle: ".modal-header" });;
		model.RouteSegmentEditModal.Init();
	};

	model.RouteSegmentEditModal = {
		CurrentItem: ko.observable(),
		FromRoutePoint: ko.observable(),
		ToRoutePoint: ko.observable(),
		OnClosed: null,
		Close: function (self, e)
		{
			$(routeSegmentModalSelector).modal("hide");
			if (self.OnClosed != null)
				self.OnClosed();
		},
		Init: function ()
		{
			model.RouteSegmentEditModal.IsDone = false;
			// получить точку начала
			model.RouteSegmentEditModal.FromRoutePoint(model.GetRoutePoint(model.RouteSegmentEditModal.CurrentItem().FromRoutePointId()));
			// получить точку конца
			model.RouteSegmentEditModal.ToRoutePoint(model.GetRoutePoint(model.RouteSegmentEditModal.CurrentItem().ToRoutePointId()));
		},
		Validate: function (self)
		{
			if (!self.CurrentItem().TransportTypeId())
			{
				alert("Не выбран вид транспорта.");
				return false;
			}

			return true;
		},
		Done: function (self, e)
		{
			if (!self.Validate(self))
				return;

			$(routeSegmentModalSelector).modal("hide");
			model.SaveRouteSegment(model.RouteSegmentEditModal.CurrentItem());
		}
	};

	// #endregion

	// #region document create/edit modal /////////////////////////////////////////////////////////////////////////////////////

	var documentModalSelector = "#documentEditModal";

	model.OpenDocumentCreate = function ()
	{
		$.ajax({
			type: "POST",
			url: model.Options.GetNewDocumentUrl,
			data: { AccountingId: model.SelectedAccounting().ID(), OrderId: null },
			success: function (response)
			{
				var r = JSON.parse(response);
				if (r.Message)
					model.OpenError(r);
				else
				{
					var data = ko.mapping.fromJSON(response);
					model.ExtendDocument(data, model.SelectedAccounting());
					model.SelectedAccounting().JointDocumentsItems.push(data);
					model.DocumentEditModal.CurrentItem(data);
					$(documentModalSelector).modal("show");
					$(documentModalSelector).draggable({ handle: ".modal-header" });
					model.DocumentEditModal.OnClosed = function ()
					{
						data.IsDeleted(true);
						model.SaveDocument(data);
						model.SelectedAccounting().JointDocumentsItems.remove(data);
					};
					model.DocumentEditModal.Init();
				}
			}
		});
	};

	model.OpenOrderDocumentCreate = function ()
	{
		var orderId = model.Order.ID();
		$.ajax({
			type: "POST",
			url: model.Options.GetNewDocumentUrl,
			data: { OrderId: orderId },
			success: function (response)
			{
				var r = JSON.parse(response);
				if (r.Message)
					model.OpenError(r);
				else
				{
					var data = ko.mapping.fromJSON(response);
					model.ExtendDocument(data, model.Order);
					model.JointDocumentsItems.push(data);
					model.DocumentEditModal.CurrentItem(data);
					$(documentModalSelector).modal("show");
					$(documentModalSelector).draggable({ handle: ".modal-header" });
					model.DocumentEditModal.OnClosed = function ()
					{
						data.IsDeleted(true);
						model.SaveDocument(data);
						model.JointDocumentsItems.remove(data);
					};
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
		AlertSupress: false,
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
				var dtype = ko.utils.arrayFirst(model.Dictionaries.DocumentType(), function (item) { return item.ID() == newValue });
				if (dtype)
				{
					model.DocumentEditModal.AlertSupress = true;
					model.DocumentEditModal.CurrentItem().IsPrint(dtype.IsPrint());
					model.DocumentEditModal.CurrentItem().IsNipVisible(dtype.IsNipVisible());
					model.DocumentEditModal.AlertSupress = false;
				}
			});

			model.DocumentEditModal.CurrentItem().IsPrint.subscribe(function (newValue)
			{
				if (!model.DocumentEditModal.AlertSupress)
					if (newValue && ([59, 10, 61, 37, 38].indexOf(model.DocumentEditModal.CurrentItem().DocumentTypeId()) >= 0))
						alert("Вы уверены, что хотите выводить номер документа на печать? В этом случае номер документа будет показан в счете Клиенту.");
			});

			model.DocumentEditModal.CurrentItem().IsNipVisible.subscribe(function (newValue)
			{
				if (!model.DocumentEditModal.AlertSupress)
					if (newValue && ([59, 10, 61, 37, 38].indexOf(model.DocumentEditModal.CurrentItem().DocumentTypeId()) >= 0))
						alert("Вы уверены, что хотите выводить документ в NIP? В этом случае документ будет показан Клиенту в его личном кабинете.");
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
		Validate: function (self)
		{
			if (!self.CurrentItem().Number())
			{
				alert("Поле Номер обязательно для заполнения");
				return false;
			}

			if (!self.CurrentItem().Date())
			{
				alert("Поле Дата обязательно для заполнения");
				return false;
			}

			if ((!self.CurrentItem().Filename()) || (!self.CurrentItem().UploadedBy()))
			{
				alert("Файл не загружен!");
				return false;
			}

			return true;
		},
		Done: function (self, e)
		{
			if (!self.Validate(self))
				return;

			$(documentModalSelector).modal("hide");
			// сохранить изменения
			model.SaveDocument(self.CurrentItem());
			$('#documentUpload').parent("form")[0].reset();
			self.CurrentItem(null);
			self.OnClosed = null;
		}
	};

	// #endregion

	// #region route segment select modal /////////////////////////////////////////////////////////////////////////////////////

	var routeSegmentSelectModalSelector = "#routeSegmentSelectModal";

	model.OpenRouteSegmentsSelect = function (orderAccounting)
	{
		model.RouteSegmentSelectModal.CurrentItem(orderAccounting);
		model.RouteSegmentSelectModal.CurrentSelection(orderAccounting.RouteSegmentsItems());
		model.RouteSegmentSelectModal.CurrentList(model.RouteSegmentsItems());
		$(routeSegmentSelectModalSelector).modal("show");
		$(routeSegmentSelectModalSelector).draggable({ handle: ".modal-header" });
		model.RouteSegmentSelectModal.Init();
	};

	model.RouteSegmentSelectModal = {
		CurrentItem: ko.observable(),
		CurrentSelection: ko.observableArray(),
		CurrentList: ko.observableArray(),
		OnClosed: null,
		Close: function (self, e)
		{
			$(routeSegmentSelectModalSelector).modal("hide");
			if (self.OnClosed != null)
				self.OnClosed();
		},
		Init: function () { },
		IsChecked: function (context)
		{
			return ko.utils.arrayFirst(model.RouteSegmentSelectModal.CurrentSelection(), function (item) { return item.RouteSegmentId() == context.ID() });
		},
		ToggleSelected: function (context, e)
		{
			var selected = ko.utils.arrayFirst(model.RouteSegmentSelectModal.CurrentSelection(), function (item) { return item.RouteSegmentId() == context.ID() });
			if (selected)
			{
				// снята отметка, есть запись в CurrentSelection
				model.RouteSegmentSelectModal.CurrentSelection.remove(selected);
			}
			else
			{
				// отмечено, нет записи в CurrentSelection
				model.RouteSegmentSelectModal.CurrentSelection.push({ AccountingId: ko.observable(model.RouteSegmentSelectModal.CurrentItem().ID()), RouteSegmentId: ko.observable(context.ID()) });
			}

			return true;
		},
		Done: function (self, e)
		{
			self.CurrentItem().IsDirty(true);
			self.CurrentItem().RouteSegmentsItems(self.CurrentSelection().sort(function (l, r)
			{
				var ls = ko.utils.arrayFirst(model.RouteSegmentsItems(), function (item) { return item.ID() == l.RouteSegmentId() });
				var rs = ko.utils.arrayFirst(model.RouteSegmentsItems(), function (item) { return item.ID() == r.RouteSegmentId() });
				return ls.No() < rs.No() ? -1 : ls.No() > rs.No() ? 1 : 0;
			}));
			$(routeSegmentSelectModalSelector).modal("hide");
		}
	};

	// #endregion

	// #region confirmation modal /////////////////////////////////////////////////////////////////////////////////////////////

	var confirmModalSelector = "#confirmModal";

	model.OpenConfirm = function (text, confirmHandler)
	{
		model.ConfirmModal.Text(text);
		$(confirmModalSelector).modal("show");
		$(confirmModalSelector).draggable({ handle: ".modal-header" });
		model.ConfirmModal.OnConfirmed = confirmHandler;
		model.ConfirmModal.Init();
	};

	model.ConfirmModal = {
		Text: ko.observable(),
		Init: function ()
		{
		},
		OnConfirmed: null,
		Done: function (self, e)
		{
			$(confirmModalSelector).modal("hide");
			if (self.OnConfirmed != null)
				self.OnConfirmed();
		}
	};

	// #endregion

	// #region document datetime edit modal ///////////////////////////////////////////////////////////////////////////////////

	var documentDatetimeModalSelector = "#documentDatetimeEditModal";

	model.OpenDocumentDateTimeEdit = function (data, datetime)
	{
		model.DocumentDateTimeEditModal.CurrentEntity(data);
		model.DocumentDateTimeEditModal.CurrentItem(datetime);
		$(documentDatetimeModalSelector).modal("show");
		$(documentDatetimeModalSelector).draggable({ handle: ".modal-header" });;
		model.DocumentDateTimeEditModal.OnClosed = null;
		model.DocumentDateTimeEditModal.Init();
	};

	model.DocumentDateTimeEditModal = {
		CurrentEntity: ko.observable(),
		CurrentItem: ko.observable(),
		CurrentDate: ko.observable(),
		CurrentTime: ko.observable(),
		OnClosed: null,
		Close: function (self, e)
		{
			$(documentDatetimeModalSelector).modal("hide");
			if (self.OnClosed != null)
				self.OnClosed();
		},
		Init: function ()
		{
			model.DocumentDateTimeEditModal.CurrentDate(ko.unwrap(model.DocumentDateTimeEditModal.CurrentItem()));
			model.DocumentDateTimeEditModal.CurrentTime(app.utility.FormatTime(model.DocumentDateTimeEditModal.CurrentItem()) || "00:00");
		},
		Done: function (self, e)
		{
			// сохранить изменения
			self.CurrentDate(app.utility.ParseDate(self.CurrentDate()));
			self.CurrentDate().setHours(parseInt(self.CurrentTime().substring(0, 2)) || 0);
			self.CurrentDate().setMinutes(parseInt(self.CurrentTime().substring(3, 5)) || 0);
			self.CurrentItem()(self.CurrentDate());

			var document = self.CurrentEntity();
			// save document
			$.ajax({
				type: "POST",
				url: model.Options.SaveDocumentUrl,
				data: {
					ID: document.ID(),
					UploadedBy: document.UploadedBy(),
					UploadedDate: app.utility.SerializeDateTime(document.UploadedDate()),
					Date: app.utility.SerializeDateTime(document.Date()),
					DocumentTypeId: document.DocumentTypeId(),
					Filename: document.Filename(),
					FileSize: document.FileSize(),
					IsPrint: document.IsPrint(),
					IsNipVisible: document.IsNipVisible(),
					Number: document.Number(),
					OriginalSentDate: app.utility.SerializeDateTime(document.OriginalSentDate()),
					OriginalReceivedDate: app.utility.SerializeDateTime(document.OriginalReceivedDate()),
				},
				success: function (response)
				{
				}
			});

			$(documentDatetimeModalSelector).modal("hide");
		}
	};

	// #endregion

	// #region operation datetime edit modal //////////////////////////////////////////////////////////////////////////////////

	var datetimeModalSelector = "#datetimeEditModal";

	model.OpenDateTimeEdit = function (data, datetime)
	{
		if ((model.Order.OrderStatusId() != 2) && (model.Order.OrderStatusId() != 3))
		{
			alert("В этом статусе заказа даты менять нельзя");
			return;
		}

		model.DateTimeEditModal.CurrentEntity(data);
		model.DateTimeEditModal.CurrentItem(datetime);
		$(datetimeModalSelector).modal("show");
		$(datetimeModalSelector).draggable({ handle: ".modal-header" });;
		model.DateTimeEditModal.OnClosed = null;
		model.DateTimeEditModal.Init();
	};

	model.DateTimeEditModal = {
		CurrentEntity: ko.observable(),
		CurrentItem: ko.observable(),
		CurrentDate: ko.observable(),
		CurrentTime: ko.observable(),
		OnClosed: null,
		Close: function (self, e)
		{
			$(datetimeModalSelector).modal("hide");
			if (self.OnClosed != null)
				self.OnClosed();
		},
		Init: function ()
		{
			model.DateTimeEditModal.CurrentDate(ko.unwrap(model.DateTimeEditModal.CurrentItem()));
			model.DateTimeEditModal.CurrentTime(app.utility.FormatTime(model.DateTimeEditModal.CurrentItem()) || "00:00");
		},
		Done: function (self, e)
		{
			// сохранить изменения
			self.CurrentDate(app.utility.ParseDate(self.CurrentDate()));
			self.CurrentDate().setHours(parseInt(self.CurrentTime().substring(0, 2)) || 0);
			self.CurrentDate().setMinutes(parseInt(self.CurrentTime().substring(3, 5)) || 0);
			self.CurrentItem()(self.CurrentDate());

			// save operation
			$.ajax({
				type: "POST",
				url: model.Options.SaveOperationUrl,
				data: {
					Id: self.CurrentEntity().ID(),
					No: self.CurrentEntity().No(),
					Name: self.CurrentEntity().Name(),
					OrderId: self.CurrentEntity().OrderId(),
					OrderOperationId: self.CurrentEntity().OrderOperationId(),
					OperationStatusId: self.CurrentEntity().OperationStatusId(),
					ResponsibleUserId: self.CurrentEntity().ResponsibleUserId(),
					StartPlanDate: app.utility.SerializeDateTime(self.CurrentEntity().StartPlanDate()),
					StartFactDate: app.utility.SerializeDateTime(self.CurrentEntity().StartFactDate()),
					FinishPlanDate: app.utility.SerializeDateTime(self.CurrentEntity().FinishPlanDate()),
					FinishFactDate: app.utility.SerializeDateTime(self.CurrentEntity().FinishFactDate())
				},
				success: function (response)
				{
					var r = JSON.parse(response);
					if (r.Message)
						alert(r.Message);
					else
					{
						self.CurrentEntity().OperationStatusId(r.OperationStatusId);
						model.GetOrderEvents(self.CurrentEntity().OrderId);
					}
				}
			});

			$(datetimeModalSelector).modal("hide");
		}
	};

	// #endregion

	// #region Operation create/edit modal ////////////////////////////////////////////////////////////////////////////////////

	var operationModalSelector = "#operationEditModal";

	model.OpenOperationCreate = function ()
	{
		$.ajax({
			type: "POST",
			url: model.Options.GetNewOperationUrl,
			data: { OrderId: model.Order.ID() },
			success: function (response)
			{
				var r = JSON.parse(response);
				if (r.Message)
					model.OpenError(r);
				else
				{
					var data = ko.mapping.fromJSON(response);
					model.OperationsItems.push(data);
					model.OperationEditModal.CurrentItem(data);
					$(operationModalSelector).modal("show");
					$(operationModalSelector).draggable({ handle: ".modal-header" });
					model.OperationEditModal.OnClosed = function () { model.OperationsItems.remove(data); };
					model.OperationEditModal.Init();
				}
			}
		});
	};

	model.OpenOperationEdit = function (operation)
	{
		model.OperationEditModal.CurrentItem(operation);
		$(operationModalSelector).modal("show");
		$(operationModalSelector).draggable({ handle: ".modal-header" });;
		model.OperationEditModal.Init();
	};

	model.OperationEditModal = {
		CurrentItem: ko.observable(),
		Init: function () { },
		OnClosed: null,
		Close: function (self, e)
		{
			$(operationModalSelector).modal("hide");
			if (self.OnClosed != null)
				self.OnClosed();
		},
		Done: function (self, e)
		{
			// сохранить изменения
			self.CurrentItem().Name(app.utility.GetDisplay(model.Dictionaries.OrderOperation, self.CurrentItem().OrderOperationId()));
			model.SaveOperation(self.CurrentItem());
			model.GetOperations(model.Order.ID());
			$(operationModalSelector).modal("hide");
			self.CurrentItem(null);
		}
	};

	// #endregion

	// #region change orderTemplate modal /////////////////////////////////////////////////////////////////////////////////////

	var changeOrderTemplateSelector = "#changeOrderTemplateModal";

	model.OpenChangeOrderTemplate = function (contractId, productId)
	{
		model.ChangeOrderTemplateModal.ContractId(contractId);
		model.ChangeOrderTemplateModal.ProductId(productId);
		$(changeOrderTemplateSelector).modal("show");
		$(changeOrderTemplateSelector).draggable({ handle: ".modal-header" });;
		model.ChangeOrderTemplateModal.Init();
	};

	model.ChangeOrderTemplateModal = {
		ContractId: ko.observable(),
		ProductId: ko.observable(),
		SelectedOrderTemplate: ko.observable(),
		OrderTemplatesItems: ko.observableArray(),
		Init: function ()
		{
			var self = model.ChangeOrderTemplateModal;
			$.ajax({
				type: "POST",
				async: false,
				url: model.Options.GetOrderTemplatesByContractUrl,
				data: { ContractId: self.ContractId() },
				success: function (response)
				{
					self.OrderTemplatesItems(ko.mapping.fromJSON(response).Items());
				}
			});

			$.ajax({
				type: "POST",
				async: false,
				url: model.Options.GetOrderTemplatesByProductUrl,
				data: { ProductId: self.ProductId() },
				success: function (response)
				{
					ko.utils.arrayPushAll(self.OrderTemplatesItems, ko.mapping.fromJSON(response).Items());
				}
			});
		},
		OnClosed: null,
		Close: function (self, e)
		{
			$(changeOrderTemplateSelector).modal("hide");
			if (self.OnClosed != null)
				self.OnClosed();
		},
		Done: function (self, e)
		{
			$(changeOrderTemplateSelector).modal("hide");
			// сохранить изменения
			model.ChangeTemplate(model.Order.ID(), self.SelectedOrderTemplate());
		}
	};

	// #endregion

	// #region change order status modal //////////////////////////////////////////////////////////////////////////////////////

	var changeOrderStatusSelector = "#changeOrderStatusModal";

	model.OpenChangeOrderStatus = function ()
	{
		model.ChangeOrderStatusModal.SelectedStatus(model.Order.OrderStatusId());
		$(changeOrderStatusSelector).modal("show");
		$(changeOrderStatusSelector).draggable({ handle: ".modal-header" });;
		model.ChangeOrderStatusModal.Init();
	};

	model.ChangeOrderStatusModal = {
		SelectedStatus: ko.observable(),
		CloseDate: ko.observable(),
		Reason: ko.observable(),
		Errors: ko.observableArray(),
		Init: function () { },
		OnClosed: null,
		Close: function (self, e)
		{
			$(changeOrderStatusSelector).modal("hide");
			if (self.OnClosed != null)
				self.OnClosed();
		},
		Done: function (self, e)
		{
			// validate
			if (self.SelectedStatus() == 1 || self.SelectedStatus() == 8)
				if (!self.Reason())
				{
					alert("Укажите причину.");
					return;
				}

			if (self.SelectedStatus() == 7)
				if (!self.CloseDate())
				{
					alert("Укажите дату.");
					return;
				}

			self.Errors.removeAll();
			$.ajax({
				type: "POST",
				async: false,
				url: model.Options.CheckStatusRulesUrl,
				data: { OrderId: model.Order.ID(), OrderStatusId: self.SelectedStatus() },
				success: function (response)
				{
					var r = JSON.parse(response);
					if (r.Message)
						alert(r.Message);
					else
						self.Errors(r.Errors);
				}
			});

			if (self.Errors().length > 0)
				return;

			$(changeOrderStatusSelector).modal("hide");

			// сохранить изменения
			model.ChangeOrderStatus(model.Order.ID(), self.SelectedStatus(), self.Reason(), self.CloseDate());
			self.CloseDate(null);
			self.Reason(null);
			self.Errors([]);
		}
	};

	// #endregion

	// #region change order contractor modal //////////////////////////////////////////////////////////////////////////////////

	var changeOrderContractorSelector = "#changeOrderContractorModal";

	model.OpenChangeOrderContractor = function ()
	{
		model.ChangeOrderContractorModal.SelectedContractor(null);
		$(changeOrderContractorSelector).modal("show");
		$(changeOrderContractorSelector).draggable({ handle: ".modal-header" });;
		model.ChangeOrderContractorModal.Init();
	};

	model.ChangeOrderContractorModal = {
		SelectedContractor: ko.observable(),
		Init: function () { },
		OnClosed: null,
		Close: function (self, e)
		{
			$(changeOrderContractorSelector).modal("hide");
			if (self.OnClosed != null)
				self.OnClosed();
		},
		Done: function (self, e)
		{
			$(changeOrderContractorSelector).modal("hide");
			// сохранить изменения
			model.ChangeContractor(model.Order.ID(), self.SelectedContractor());
		}
	};

	// #endregion

	// #region change responsible user modal //////////////////////////////////////////////////////////////////////////////////

	var changeResponsibleUserSelector = "#userSelectModal";

	model.ChangeResponsibleUser = function (data)
	{
		model.UserSelectModal.CurrentItem(data);
		model.UserSelectModal.SelectedUser(data.ResponsibleUserId());
		$(changeResponsibleUserSelector).modal("show");
		$(changeResponsibleUserSelector).draggable({ handle: ".modal-header" });
		model.UserSelectModal.Init();
	};

	model.UserSelectModal = {
		CurrentItem: ko.observable(),
		SelectedUser: ko.observable(),
		Init: function () { },
		OnClosed: null,
		Close: function (self, e)
		{
			$(changeResponsibleUserSelector).modal("hide");
			if (self.OnClosed != null)
				self.OnClosed();
		},
		Select: function (value) { this.SelectedUser(value); },
		Done: function (self, e)
		{
			// сохранить изменения
			self.CurrentItem().ResponsibleUserId(self.SelectedUser());
			model.SaveOperation(self.CurrentItem());
			$(changeResponsibleUserSelector).modal("hide");
		}
	};

	// #endregion

	// #region clone order modal //////////////////////////////////////////////////////////////////////////////////////////////

	var cloneOrderSelector = "#cloneOrderModal";

	model.OpenCloneOrder = function ()
	{
		model.CloneOrderModal.CurrentItem(model.Order);
		$(cloneOrderSelector).modal("show");
		$(cloneOrderSelector).draggable({ handle: ".modal-header" });;
		model.CloneOrderModal.Init();
	};

	model.CloneOrderModal = {
		CurrentItem: ko.observable(),
		IsCopyCargo: ko.observable(true),
		IsCopyRoute: ko.observable(true),
		IsCopyWorkgroup: ko.observable(true),
		Init: function () { },
		OnClosed: null,
		Close: function (self, e)
		{
			$(cloneOrderSelector).modal("hide");
			if (self.OnClosed != null)
				self.OnClosed();
		},
		Done: function (self, e)
		{
			// сохранить изменения
			model.CloneOrder(self.CurrentItem().ID(), self.IsCopyCargo(), self.IsCopyRoute(), self.IsCopyWorkgroup());

			$(cloneOrderSelector).modal("hide");
		}
	};

	// #endregion

	// #region document delivery edit modal ///////////////////////////////////////////////////////////////////////////////////

	var documentDeliveryModalSelector = "#documentDeliveryEditModal";

	model.OpenDocumentDeliveryEdit = function (document)
	{
		model.DocumentDeliveryEditModal.CurrentItem(document);
		$(documentDeliveryModalSelector).modal("show");
		$(documentDeliveryModalSelector).draggable({ handle: ".modal-header" });;
		model.DocumentDeliveryEditModal.Init();
	};

	model.DocumentDeliveryEditModal = {
		CurrentItem: ko.observable(),
		OnClosed: null,
		Close: function (self, e)
		{
			$(documentDeliveryModalSelector).modal("hide");
			if (self.OnClosed != null)
				self.OnClosed();
		},
		Init: function () { },
		Validate: function (self)
		{
			return true;
		},
		Done: function (self, e)
		{
			if (!self.Validate(self))
				return;

			// сохранить изменения
			model.SaveDocumentDeliveryInfo(self.CurrentItem(), self.CurrentItem().IsDocument());
			$(documentDeliveryModalSelector).modal("hide");
			self.CurrentItem(null);
		}
	};

	// #endregion

	// #region document delivery number edit modal ////////////////////////////////////////////////////////////////////////////

	var documentDeliveryNumberModalSelector = "#documentDeliveryNumberEditModal";

	model.OpenDocumentDeliveryNumberEdit = function (document)
	{
		model.DocumentDeliveryNumberEditModal.CurrentItem(document);
		$(documentDeliveryNumberModalSelector).modal("show");
		$(documentDeliveryNumberModalSelector).draggable({ handle: ".modal-header" });;
		model.DocumentDeliveryNumberEditModal.Init();
	};

	model.DocumentDeliveryNumberEditModal = {
		CurrentItem: ko.observable(),
		OnClosed: null,
		Close: function (self, e)
		{
			$(documentDeliveryNumberModalSelector).modal("hide");
			if (self.OnClosed != null)
				self.OnClosed();
		},
		Init: function () { },
		Validate: function (self)
		{
			if (self.CurrentItem().ReceivedBy() && !self.CurrentItem().ReceivedNumber())
			{
				alert("Поле Номер обязательно для заполнения");
				return false;
			}

			return true;
		},
		Done: function (self, e)
		{
			if (!self.Validate(self))
				return;

			// сохранить изменения
			model.SaveDocumentDeliveryInfo(self.CurrentItem(), self.CurrentItem().IsDocument());
			$(documentDeliveryNumberModalSelector).modal("hide");
			self.CurrentItem(null);
		}
	};

	// #endregion

	// #region order status history modal /////////////////////////////////////////////////////////////////////////////////////

	var orderStatusHistoryModalSelector = "#orderStatusHistoryModal";

	model.OpenOrderStatusHistory = function (document)
	{
		model.OrderStatusHistoryModal.CurrentItem(document);
		$(orderStatusHistoryModalSelector).modal("show");
		$(orderStatusHistoryModalSelector).draggable({ handle: ".modal-header" });;
		model.OrderStatusHistoryModal.Init();
	};

	model.OrderStatusHistoryModal = {
		CurrentItem: ko.observable(),
		OnClosed: null,
		Close: function (self, e)
		{
			$(orderStatusHistoryModalSelector).modal("hide");
			if (self.OnClosed != null)
				self.OnClosed();
		},
		Init: function ()
		{
			$.ajax({
				type: "POST",
				url: model.Options.GetOrderStatusHistoryUrl,
				data: { OrderId: model.Order.ID() },
				success: function (response)
				{
					model.OrderStatusHistoryModal.CurrentItem({ Items: ko.mapping.fromJSON(response).Items() });
				}
			});
		},
		Done: function (self, e)
		{
			$(orderStatusHistoryModalSelector).modal("hide");
			self.CurrentItem(null);
		}
	};

	// #endregion

	// #region select date of currency rate modal /////////////////////////////////////////////////////////////////////////////

	var currencyRateDateModalSelector = "#currencyRateDateEditModal";

	model.OpenCurrencyRateDateSelect = function (data, datetime)
	{
		if (!model.SelectedAccounting().ServicesItems().length)
		{
			alert("Сначала добавьте услугу.");
			return;
		}

		//model.CurrencyRateDateEditModal.CurrentItem(datetime);
		$(currencyRateDateModalSelector).modal("show");
		$(currencyRateDateModalSelector).draggable({ handle: ".modal-header" });;
		model.CurrencyRateDateEditModal.OnClosed = null;
		model.CurrencyRateDateEditModal.Init();
	};

	model.CurrencyRateDateEditModal = {
		CurrentDate: ko.observable(),
		OnClosed: null,
		Close: function (self, e)
		{
			$(currencyRateDateModalSelector).modal("hide");
			if (self.OnClosed != null)
				self.OnClosed();
		},
		Init: function () { },
		Done: function (self, e)
		{
			// сохранить изменения
			self.CurrentDate(app.utility.ParseDate(self.CurrentDate()));

			if (self.CurrentDate())
			{
				$.ajax({
					type: "POST",
					async: false,
					url: model.Options.GetCurrencyRateUrl,
					data: { Date: app.utility.SerializeDateTime(self.CurrentDate()) },
					success: function (response)
					{
						var r = JSON.parse(response);
						if (r.Message)
							alert(r.Message);
						else
						{
							var rates = ko.mapping.fromJSON(response).Items();
							var rate = ko.utils.arrayFirst(rates, function (item) { return item.CurrencyId() == model.SelectedAccounting().ServicesItems()[0].CurrencyId() });
							model.SelectedAccounting().CurrencyRate(rate ? rate.Rate() : 1);
							model.UpdateCurrencyRate(model.SelectedAccounting());
						}
					}
				});
			}

			$(currencyRateDateModalSelector).modal("hide");
		}
	};

	// #endregion

	// #region act params edit modal //////////////////////////////////////////////////////////////////////////////////////////

	var actParamsModalSelector = "#actParamsEditModal";

	model.OpenActParamsEdit = function (data)
	{
		model.ActParamsEditModal.CurrentEntity(data);
		$(actParamsModalSelector).modal("show");
		$(actParamsModalSelector).draggable({ handle: ".modal-header" });;
		model.ActParamsEditModal.OnClosed = null;
		model.ActParamsEditModal.Init();
	};

	model.ActParamsEditModal = {
		CurrentEntity: ko.observable(),
		CurrentDate: ko.observable(),
		CurrentNumber: ko.observable(),
		OnClosed: null,
		Close: function (self, e)
		{
			$(actParamsModalSelector).modal("hide");
			if (self.OnClosed != null)
				self.OnClosed();
		},
		Init: function ()
		{
			model.ActParamsEditModal.CurrentDate(model.ActParamsEditModal.CurrentEntity().ActDate());
			model.ActParamsEditModal.CurrentNumber(model.ActParamsEditModal.CurrentEntity().ActNumber());
		},
		Done: function (self, e)
		{
			$(actParamsModalSelector).modal("hide");
			// сохранить изменения
			self.CurrentDate(app.utility.ParseDate(self.CurrentDate()));

			$.ajax({
				type: "POST",
				url: model.Options.CreateActUrl,
				data: { AccountingId: self.CurrentEntity().ID(), Number: self.CurrentNumber(), Date: app.utility.SerializeDateTime(self.CurrentDate()) },
				success: function (response)
				{
					var r = JSON.parse(response);
					if (r.Message)
						alert(r.Message);
					else
					{
						window.open(model.Options.ViewTemplatedDocumentUrl + "/" + r.FileId, "_blank");
						model.LoadAccountingJointDocuments(model.SelectedAccounting());
						model.SelectedAccounting().ActNumber(self.CurrentNumber());
						model.SelectedAccounting().ActDate(self.CurrentDate());
					}
				}
			});
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

	// #region change accounting pay method modal /////////////////////////////////////////////////////////////////////////////

	var changePayMethodSelector = "#changePayMethodModal";

	model.OpenChangePayMethod = function (data)
	{
		model.ChangePayMethodModal.CurrentEntity(data);
		$(changePayMethodSelector).modal("show");
		$(changePayMethodSelector).draggable({ handle: ".modal-header" });;
		model.ChangePayMethodModal.Init();
	};

	model.ChangePayMethodModal = {
		CurrentEntity: ko.observable(),
		SelectedID: ko.observable(),
		Init: function ()
		{
			model.ChangePayMethodModal.SelectedID(model.ChangePayMethodModal.CurrentEntity().PayMethodId());
		},
		OnClosed: null,
		Close: function (self, e)
		{
			$(changePayMethodSelector).modal("hide");
			if (self.OnClosed != null)
				self.OnClosed();
		},
		Done: function (self, e)
		{
			$(changePayMethodSelector).modal("hide");
			// сохранить изменения
			model.ChangePayMethod(self.CurrentEntity().ID(), self.SelectedID());
			self.CurrentEntity(null);
		}
	};

	// #endregion

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
			var func = function ()
			{
			};
			switch (field)
			{
				case "ID":
					if (sorter.Asc())
						func = function (l, r)
						{
							return l.ID() < r.ID() ? -1 : l.ID() > r.ID() ? 1 : 0;
						};
					else
						func = function (r, l)
						{
							return l.ID() < r.ID() ? -1 : l.ID() > r.ID() ? 1 : 0;
						};

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

	model.AccountingSorter = {
		Field: ko.observable(),
		Asc: ko.observable(true),
		Sort: function (field)
		{
			var sorter = this;
			if (sorter.Field() == field)
				sorter.Asc(!sorter.Asc());

			sorter.Field(field);
			var func = function ()
			{
			};
			switch (field)
			{
				case "ID":
					if (sorter.Asc())
						func = function (l, r) { return l.ID() < r.ID() ? -1 : l.ID() > r.ID() ? 1 : 0; };
					else
						func = function (r, l) { return l.ID() < r.ID() ? -1 : l.ID() > r.ID() ? 1 : 0; };

					break;

				case "Number":
					if (sorter.Asc())
						func = function (l, r)
						{
							return l.Number() < r.Number() ? -1 : l.Number() > r.Number() ? 1 : 0;
						};
					else
						func = function (r, l)
						{
							return l.Number() < r.Number() ? -1 : l.Number() > r.Number() ? 1 : 0;
						};

					break;

				case "OrderNumber":
					if (sorter.Asc())
						func = function (l, r)
						{
							return l.OrderNumber() < r.OrderNumber() ? -1 : l.OrderNumber() > r.OrderNumber() ? 1 : 0;
						};
					else
						func = function (r, l)
						{
							return l.OrderNumber() < r.OrderNumber() ? -1 : l.OrderNumber() > r.OrderNumber() ? 1 : 0;
						};

					break;

				case "ContractNumber":
					if (sorter.Asc())
						func = function (l, r)
						{
							return l.ContractNumber() < r.ContractNumber() ? -1 : l.ContractNumber() > r.ContractNumber() ? 1 : 0;
						};
					else
						func = function (r, l)
						{
							return l.ContractNumber() < r.ContractNumber() ? -1 : l.ContractNumber() > r.ContractNumber() ? 1 : 0;
						};

					break;

				case "InvoiceNumber":
					if (sorter.Asc())
						func = function (l, r)
						{
							return l.InvoiceNumber() < r.InvoiceNumber() ? -1 : l.InvoiceNumber() > r.InvoiceNumber() ? 1 : 0;
						};
					else
						func = function (r, l)
						{
							return l.InvoiceNumber() < r.InvoiceNumber() ? -1 : l.InvoiceNumber() > r.InvoiceNumber() ? 1 : 0;
						};

					break;

				case "InvoiceDate":
					if (sorter.Asc())
						func = function (l, r)
						{
							return l.InvoiceDate() < r.InvoiceDate() ? -1 : l.InvoiceDate() > r.InvoiceDate() ? 1 : 0;
						};
					else
						func = function (r, l)
						{
							return l.InvoiceDate() < r.InvoiceDate() ? -1 : l.InvoiceDate() > r.InvoiceDate() ? 1 : 0;
						};

					break;

				case "ActNumber":
					if (sorter.Asc())
						func = function (l, r)
						{
							return l.ActNumber() < r.ActNumber() ? -1 : l.ActNumber() > r.ActNumber() ? 1 : 0;
						};
					else
						func = function (r, l)
						{
							return l.ActNumber() < r.ActNumber() ? -1 : l.ActNumber() > r.ActNumber() ? 1 : 0;
						};

					break;

				case "ActDate":
					if (sorter.Asc())
						func = function (l, r)
						{
							return l.ActDate() < r.ActDate() ? -1 : l.ActDate() > r.ActDate() ? 1 : 0;
						};
					else
						func = function (r, l)
						{
							return l.ActDate() < r.ActDate() ? -1 : l.ActDate() > r.ActDate() ? 1 : 0;
						};

					break;

				case "Sum":
					if (sorter.Asc())
						func = function (l, r)
						{
							return l.Sum() < r.Sum() ? -1 : l.Sum() > r.Sum() ? 1 : 0;
						};
					else
						func = function (r, l)
						{
							return l.Sum() < r.Sum() ? -1 : l.Sum() > r.Sum() ? 1 : 0;
						};

					break;

				case "IsIncome":
					if (sorter.Asc())
						func = function (l, r)
						{
							return l.IsIncome() < r.IsIncome() ? -1 : l.IsIncome() > r.IsIncome() ? 1 : 0;
						};
					else
						func = function (r, l)
						{
							return l.IsIncome() < r.IsIncome() ? -1 : l.IsIncome() > r.IsIncome() ? 1 : 0;
						};

					break;

			}

			model.AccountingsItems.sort(func);
		},
		Css: function (field)
		{
			var sorter = this;
			if (sorter.Field() != field)
				return "";

			return sorter.Asc() ? "asc-sorted" : "desc-sorted";
		}
	};

	model.JointDocumentSorter = {
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
				case "DocumentType":
					if (sorter.Asc())
						func = function (l, r)
						{
							var lValue = (l.IsDocument() ? app.utility.GetDisplay(model.Dictionaries.DocumentType, l.DocumentTypeId()) : app.utility.GetDisplay(model.Dictionaries.Template, l.TemplateId()));
							var rValue = (r.IsDocument() ? app.utility.GetDisplay(model.Dictionaries.DocumentType, r.DocumentTypeId()) : app.utility.GetDisplay(model.Dictionaries.Template, r.TemplateId()));
							return lValue < rValue ? -1 : lValue > rValue ? 1 : 0;
						};
					else
						func = function (r, l)
						{
							var lValue = (l.IsDocument() ? app.utility.GetDisplay(model.Dictionaries.DocumentType, l.DocumentTypeId()) : app.utility.GetDisplay(model.Dictionaries.Template, l.TemplateId()));
							var rValue = (r.IsDocument() ? app.utility.GetDisplay(model.Dictionaries.DocumentType, r.DocumentTypeId()) : app.utility.GetDisplay(model.Dictionaries.Template, r.TemplateId()));
							return lValue < rValue ? -1 : lValue > rValue ? 1 : 0;
						};

					break;

				case "Number":
					if (sorter.Asc())
						func = function (l, r) { return l.Number() < r.Number() ? -1 : l.Number() > r.Number() ? 1 : 0; };
					else
						func = function (r, l) { return l.Number() < r.Number() ? -1 : l.Number() > r.Number() ? 1 : 0; };

					break;

				case "OrderAccountingName":
					if (sorter.Asc())
						func = function (l, r) { return l.OrderAccountingName() < r.OrderAccountingName() ? -1 : l.OrderAccountingName() > r.OrderAccountingName() ? 1 : 0; };
					else
						func = function (r, l) { return l.OrderAccountingName() < r.OrderAccountingName() ? -1 : l.OrderAccountingName() > r.OrderAccountingName() ? 1 : 0; };

					break;

				case "Date":
					if (sorter.Asc())
						func = function (l, r) { return l.Date() < r.Date() ? -1 : l.Date() > r.Date() ? 1 : 0; };
					else
						func = function (r, l) { return l.Date() < r.Date() ? -1 : l.Date() > r.Date() ? 1 : 0; };

					break;

				case "UploadedDate":
					if (sorter.Asc())
						func = function (l, r) { return l.UploadedDate() < r.UploadedDate() ? -1 : l.UploadedDate() > r.UploadedDate() ? 1 : 0; };
					else
						func = function (r, l) { return l.UploadedDate() < r.UploadedDate() ? -1 : l.UploadedDate() > r.UploadedDate() ? 1 : 0; };

					break;

				case "OriginalSentDate":
					if (sorter.Asc())
						func = function (l, r) { return (l.OriginalSentDate() === null) - (r.OriginalSentDate() === null) || -(l.OriginalSentDate() > r.OriginalSentDate()) || (l.OriginalSentDate() < r.OriginalSentDate()) };
					else
						func = function (r, l) { return (l.OriginalSentDate() === null) - (r.OriginalSentDate() === null) || -(l.OriginalSentDate() > r.OriginalSentDate()) || (l.OriginalSentDate() < r.OriginalSentDate()) };

					break;

				case "OriginalReceivedDate":
					if (sorter.Asc())
						func = function (l, r) { return l.OriginalReceivedDate() || "" < r.OriginalReceivedDate() || "" ? -1 : l.OriginalReceivedDate() || "" > r.OriginalReceivedDate() || "" ? 1 : 0; };
					else
						func = function (r, l) { return l.OriginalReceivedDate() || "" < r.OriginalReceivedDate() || "" ? -1 : l.OriginalReceivedDate() || "" > r.OriginalReceivedDate() || "" ? 1 : 0; };

					break;

				case "LegalId":
					if (sorter.Asc())
						func = function (l, r) { return l.LegalId() < r.LegalId() ? -1 : l.LegalId() > r.LegalId() ? 1 : 0; };
					else
						func = function (r, l) { return l.LegalId() < r.LegalId() ? -1 : l.LegalId() > r.LegalId() ? 1 : 0; };

					break;

				case "IsPrint":
					if (sorter.Asc())
						func = function (l, r) { return l.IsPrint() < r.IsPrint() ? -1 : l.IsPrint() > r.IsPrint() ? 1 : 0; };
					else
						func = function (r, l) { return l.IsPrint() < r.IsPrint() ? -1 : l.IsPrint() > r.IsPrint() ? 1 : 0; };

					break;

				case "IsNipVisible":
					if (sorter.Asc())
						func = function (l, r) { return l.IsNipVisible() < r.IsNipVisible() ? -1 : l.IsNipVisible() > r.IsNipVisible() ? 1 : 0; };
					else
						func = function (r, l) { return l.IsNipVisible() < r.IsNipVisible() ? -1 : l.IsNipVisible() > r.IsNipVisible() ? 1 : 0; };

					break;
			}

			model.JointDocumentsItems.sort(func);
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

	model.EditRouteContact = function (context)
	{
		if (!context.CurrentLegal())
		{
			alert("Не выбран грузоотправитель/грузополучатель.");
			return;
		}

		if (!context.CurrentPlace())
		{
			alert("Не выбран пункт.");
			return;
		}

		var url = model.Options.EditRouteContactUrl + "/" + context.CurrentItem().RouteContactID();
		window.open(url, "_blank");
	};

	// #region Create /////////////////////////////////////////////////////////////////////////////////////////////////////////

	model.CreateRouteContact = function (context)
	{
		if (!context.CurrentLegal())
		{
			alert("Не выбран грузоотправитель/грузополучатель.");
			return;
		}

		if (!context.CurrentPlace())
		{
			alert("Не выбран пункт.");
			return;
		}

		var url = model.Options.CreateRouteContactUrl + "?legalId=" + context.CurrentLegal().ID() + "&placeId=" + ko.unwrap(context.CurrentPlace().ID);
		window.open(url, "_blank");
	};

	model.CreateShortLegal = function ()
	{
		var url = model.Options.CreateShortLegalUrl + "?contractorId=" + model.ContractorId();
		window.open(url, "_blank");
	};

	model.CreateOrderAccountingIncome = function ()
	{
		model.CreateOrderAccounting(true);
	};

	model.CreateOrderAccountingExpense = function ()
	{
		model.CreateOrderAccounting(false);
	};

	model.CreateOrderAccounting = function (isIncome)
	{
		$.ajax({
			type: "POST",
			url: model.Options.GetNewAccountingUrl,
			data: { OrderId: model.Order.ID(), IsIncome: isIncome },
			success: function (response)
			{
				var r = JSON.parse(response);
				if (r.Message)
					model.OpenError(r);
				else
				{
					var data = ko.mapping.fromJSON(response);
					model.ExtendAccounting(data);
					model.AccountingsItems.push(data);
					model.SelectAccounting(data);
				}
			}
		});
	};

	model.CreateClaim = function ()
	{
		if (model.IsDirty())
		{
			alert("Есть несохраненные изменения, пожалуйста сохраните их, чтобы они попали в формируемый документ");
			return;
		}

		$.ajax({
			type: "POST",
			url: model.Options.CreateClaimUrl,
			data: { OrderId: model.Order.ID() },
			success: function (response)
			{
				var r = JSON.parse(response);
				if (r.Message)
					alert(r.Message);
				else
					window.open(model.Options.ViewTemplatedDocumentUrl + "/" + r.FileId, "_blank");
			}
		});
	};

	model.CreateRequest = function ()
	{
		if (model.IsDirty())
		{
			alert("Есть несохраненные изменения, пожалуйста сохраните их, чтобы они попали в формируемый документ");
			return;
		}

		$.ajax({
			type: "POST",
			url: model.Options.CreateRequestUrl,
			data: { AccountingId: model.SelectedAccounting().ID() },
			success: function (response)
			{
				var r = JSON.parse(response);
				if (r.Message)
					alert(r.Message);
				else
				{
					window.open(model.Options.ViewTemplatedDocumentUrl + "/" + r.FileId, "_blank");
					// TODO: ???
					//model.LoadAccountingFiles(model.SelectedAccounting());
				}
			}
		});
	};

	model.CreateVatInvoice = function ()
	{
		if (model.IsDirty())
		{
			alert("Есть несохраненные изменения, пожалуйста сохраните их, чтобы они попали в формируемый документ");
			return;
		}

		$.ajax({
			type: "POST",
			url: model.Options.CreateVatInvoiceUrl,
			data: { AccountingId: model.SelectedAccounting().ID() },
			success: function (response)
			{
				var r = JSON.parse(response);
				if (r.Message)
					alert(r.Message);
				else
					window.open(model.Options.ViewTemplatedDocumentUrl + "/" + r.FileId, "_blank");
			}
		});
	};

	model.CreateInvoice = function ()
	{
		if (model.IsDirty())
		{
			alert("Есть несохраненные изменения, пожалуйста сохраните их, чтобы они попали в формируемый документ");
			return;
		}

		$.ajax({
			type: "POST",
			url: model.Options.CreateInvoiceUrl,
			data: { AccountingId: model.SelectedAccounting().ID() },
			success: function (response)
			{
				var r = JSON.parse(response);
				if (r.Message)
					alert(r.Message);
				else
				{
					var existing = ko.utils.arrayFirst(model.SelectedAccounting().JointDocumentsItems(), function (item) { return (item.IsDocument() == false) && (item.TemplateId() == 6) });	// 6 - акт
					if (existing)
						if (confirm("Вы пересоздали счет. Пересоздать акт?"))
							model.CreateAct();

					window.open(model.Options.ViewTemplatedDocumentUrl + "/" + r.FileId, "_blank");
				}
			}
		});
	};

	model.CreateAct = function ()
	{
		if (model.IsDirty())
		{
			alert("Есть несохраненные изменения, пожалуйста сохраните их, чтобы они попали в формируемый документ");
			return;
		}

		if (!model.SelectedAccounting().IsIncome() && model.IsLegalNotResident())
			model.OpenActParamsEdit(model.SelectedAccounting());
		else
			$.ajax({
				type: "POST",
				url: model.Options.CreateActUrl,
				data: { AccountingId: model.SelectedAccounting().ID() },
				success: function (response)
				{
					var r = JSON.parse(response);
					if (r.Message)
						alert(r.Message);
					else
					{
						window.open(model.Options.ViewTemplatedDocumentUrl + "/" + r.FileId, "_blank");
						model.LoadAccountingJointDocuments(model.SelectedAccounting());
					}
				}
			});
	};

	model.CreateDetailing = function ()
	{
		if (model.IsDirty())
		{
			alert("Есть несохраненные изменения, пожалуйста сохраните их, чтобы они попали в формируемый документ");
			return;
		}

		$.ajax({
			type: "POST",
			url: model.Options.CreateDetailingUrl,
			data: { AccountingId: model.SelectedAccounting().ID() },
			success: function (response)
			{
				var r = JSON.parse(response);
				if (r.Message)
					alert(r.Message);
				else
					window.open(model.Options.ViewTemplatedDocumentUrl + "/" + r.FileId, "_blank");
			}
		});
	};

	model.CreateAmpleDetailing = function ()
	{
		if (model.IsDirty())
		{
			alert("Есть несохраненные изменения, пожалуйста сохраните их, чтобы они попали в формируемый документ");
			return;
		}

		$.ajax({
			type: "POST",
			url: model.Options.CreateAmpleDetailingUrl,
			data: { AccountingId: model.SelectedAccounting().ID() },
			success: function (response)
			{
				var r = JSON.parse(response);
				if (r.Message)
					alert(r.Message);
				else
					window.open(model.Options.ViewTemplatedDocumentUrl + "/" + r.FileId, "_blank");
			}
		});
	};
	//#endregion

	model.CloseOrder = function ()
	{
		model.OpenConfirm("Вы уверены, что хотите закрыть заказ? Это приведет к последствиям.", function ()
		{
			$.ajax({
				type: "POST",
				url: model.Options.CloseOrderUrl,
				data: {
					OrderId: model.Order.ID(), ClosedDate: app.utility.SerializeDateTime(model.Order.ClosedDate())
				},
				success: function (response)
				{
					alert("Готово! Страница будет обновлена.");
					window.location = window.location;
				}
			})
		})
	};

	model.OpenTemplatedDocument = function (data)
	{
		var id = ko.unwrap(data.ID());
		window.location = model.Options.OpenTemplatedDocumentUrl + "/" + id;
	};

	model.OpenDocument = function (data)
	{
		var id = ko.unwrap(data.ID());
		window.location = model.Options.OpenDocumentUrl + "/" + id;
	};

	model.OpenMergedAccountingDocuments = function (data)
	{
		if (!data.Marks().IsInvoiceOk() || !data.Marks().IsActOk())
		{
			alert("Должны быть проставлены метки 'Счет Ок' и 'Акт Ок'");
			return;
		}

		if (!data.Marks().IsInvoiceChecked() || !data.Marks().IsActChecked())
			alert("Должны быть проставлены метки 'Счет проверен' и 'Акт проверен'");

		var id = ko.unwrap(data.ID());
		window.open(model.Options.OpenMergedAccountingDocumentsUrl + "/?accountingId=" + id, "_blank");
	};

	model.OpenClientAccountingDocuments = function (data)
	{
		var id = ko.unwrap(data.ID());
		window.open(model.Options.OpenClientAccountingDocumentsUrl + "/?accountingId=" + id, "_blank");
	};

	model.DownloadPaymentsFile = function (entity)
	{
		window.open(model.Options.DownloadPaymentsFileUrl + "?AccountingId=" + entity.ID(), "_blank");
	};

	model.ViewJointDocument = function (data)
	{
		if (data.IsDocument())
			model.ViewDocument(data);
		else
			model.ViewTemplatedDocument(data);
	};

	model.ViewTemplatedDocument = function (data)
	{
		var id = ko.unwrap(data.ID());
		window.open(model.Options.ViewTemplatedDocumentUrl + "/" + id, "_blank");
	};

	model.ViewDocument = function (data)
	{
		var id = ko.unwrap(data.ID());
		window.open(model.Options.ViewDocumentUrl + "/" + id, "_blank");
	};

	model.ViewContract = function ()
	{
		window.open(model.Options.ContractDetailsUrl + "/" + model.Order.ContractId(), "_blank");
	};

	model.ViewContractById = function (id)
	{
		if (id)
			window.open(model.Options.ContractDetailsUrl + "/" + id, "_blank");
	};

	model.ViewContractor = function ()
	{
		window.open(model.Options.ContractorDetailsUrl + "/" + model.ContractorId(), "_blank");
	};

	model.ViewContractorById = function (id)
	{
		window.open(model.Options.ContractorDetailsUrl + "/" + id, "_blank");
	};

	model.MoveRoutePointUp = function (data)
	{
		var i = model.RoutePointsItems.indexOf(data);
		if (i >= 1)
		{
			var array = model.RoutePointsItems();
			model.RoutePointsItems.splice(i - 1, 2, array[i], array[i - 1]);
			model.SaveRoutePoint(data);
		}
	};

	model.MoveRoutePointDown = function (data)
	{
		var i = model.RoutePointsItems.indexOf(data);
		if (i < model.RoutePointsItems().length - 1)
		{
			var array = model.RoutePointsItems();
			model.RoutePointsItems.splice(i, 2, array[i + 1], array[i]);
			model.SaveRoutePoint(data);
		}
	};

	model.EnumerateRoutePoints = function ()
	{
		var mod = false;
		for (var i = 0, j = model.RoutePointsItems().length; i < j; i++)
		{
			var item = model.RoutePointsItems()[i];
			if (item.No() != (i + 1))
			{
				item.No(i + 1);
				model.SaveRoutePoint(item);
				mod = true;
				break;	// после сохранения снова вызовется model.EnumerateRoutePoints
			}
		}

		if (!mod)
			model.RecalculateRoute();
	};

	model.RecalculateRoute = function ()
	{
		$.ajax({
			type: "POST",
			url: model.Options.RecalculateRouteUrl,
			data: { OrderId: model.Order.ID() },
			success: function (response)
			{
				var r = JSON.parse(response);
				if (r.Message)
					alert(r.Message);
				else
				{
					var order = ko.mapping.fromJSON(response).Order;
					model.Order.From(order.From());
					model.Order.To(order.To());
					model.GetRouteSegments(model.Order.ID);
					model.CalculateRouteLength(true);
				}
			}
		});
	};

	model.CalculateRouteLength = function (silent)
	{
		$.ajax({
			type: "POST",
			url: model.Options.CalculateRouteLengthUrl,
			data: { OrderId: model.Order.ID() },
			success: function (response)
			{
				var resp = ko.mapping.fromJSON(response);
				if (silent !== true)
					if (resp.Message && resp.Message())
						alert(resp.Message());

				if (resp.RouteBeforeBoard && resp.RouteBeforeBoard())
					model.Order.RouteBeforeBoard(resp.RouteBeforeBoard());

				if (resp.RouteAfterBoard && resp.RouteAfterBoard())
					model.Order.RouteAfterBoard(resp.RouteAfterBoard());

				if (resp.RouteLengthBeforeBoard && resp.RouteLengthBeforeBoard())
					model.Order.RouteLengthBeforeBoard(resp.RouteLengthBeforeBoard());

				if (resp.RouteLengthAfterBoard && resp.RouteLengthAfterBoard())
					model.Order.RouteLengthAfterBoard(resp.RouteLengthAfterBoard());
			}
		});
	};

	model.GetRouteSegmentLocal = function (context)
	{
		return ko.utils.arrayFirst(model.RouteSegmentsItems(), function (item) { return item.ID() == context.RouteSegmentId() });
	};

	model.GetCurrentCurrencyRate = function (currencyId)
	{
		var id = ko.unwrap(currencyId);
		var rate = 0;
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.GetCurrentCurrencyRateUrl,
			data: { CurrencyId: id },
			success: function (response)
			{
				rate = response
			}
		});
		return rate;
	};

	model.GetMeasureDisplay = function (serviceTypeId)
	{
		if (serviceTypeId() == 0)
			return "";

		var type = ko.utils.arrayFirst(model.Dictionaries.ServiceType(), function (item) { return item.ID() == serviceTypeId() });
		if (type)
		{
			if (type.MeasureId() > 1)
				return ko.utils.arrayFirst(model.Dictionaries.Measure(), function (item) { return item.ID() == type.MeasureId() }).Display();
			else
				return "";
		}

		return "-";
	};

	model.RecalculateCargoSeats = function ()
	{
		$.ajax({
			type: "POST",
			url: model.Options.RecalculateCargoSeatsUrl,
			data: { OrderId: model.Order.ID() },
			success: function (response)
			{
				var r = JSON.parse(response);
				if (r.Message)
					alert(r.Message);
				else
				{
					model.Order.PaidWeight(r.PaidWeight);
					model.Order.GrossWeight(r.GrossWeight);
					model.Order.Volume(r.Volume);
					model.Order.SeatsCount(r.SeatsCount);
				}
			}
		});
	};

	model.RecalculatePaymentPlanDate = function ()
	{
		$.ajax({
			type: "POST",
			url: model.Options.RecalculatePaymentPlanDateUrl,
			data: { AccountingId: model.SelectedAccounting().ID() },
			success: function (response)
			{
				var r = JSON.parse(response);
				if (r.Message)
					alert(r.Message);
				else
				{
					var r = JSON.parse(response);
					model.SelectedAccounting().PaymentPlanDate(r.PaymentPlanDate);
				}
			}
		});
	};

	model.CheckDocumentsIsPrintLimit = function (orderId)
	{
		var id = ko.unwrap(orderId);
		$.ajax({
			type: "POST",
			url: model.Options.CheckDocumentsIsPrintLimitUrl,
			data: { OrderId: id },
			success: function (response)
			{
				var r = JSON.parse(response);
				if (r.Count > 7)
					alert("Установлено ограничение по количеству документов, данные по которым можно вывести в печатные формы - 7 шт. Чтобы данные по этому документу вывести в печатную форму Вам необходимо снять метку 'на печать' по одному из уже ранее отмеченных такой меткой документу.");
			}
		});
	};

	model.ToggleDocumentIsPrint = function (document)
	{
		document.IsPrint(!document.IsPrint());
		$.ajax({
			type: "POST",
			url: model.Options.ToggleDocumentIsPrintUrl,
			data: { DocumentId: document.ID(), IsPrint: document.IsPrint() },
			success: function (response)
			{
				var r = JSON.parse(response);
				if (r.Message)
					alert(r.Message);
				else
				{
					model.CheckDocumentsIsPrintLimit(model.Order.ID);
					// TODO: также проставить флаг в документах SelectedAccounting
				}
			}
		});
	};

	model.ToggleEnVisible = function ()
	{
		model.IsEnVisible(!model.IsEnVisible());
	};

	model.UpdateAccountingCurrency = function (accounting)
	{
		if (accounting.ServicesItems().length > 0)
		{
			accounting.ContractCurrencyId(accounting.ServicesItems()[0].CurrencyId());
			accounting.AccountingCurrencyId(accounting.ServicesItems()[0].CurrencyId());
		}
	};

	model.AskRegenerateAccountingDocuments = function (accounting)
	{
		var marks = ko.utils.arrayFirst(model.MarksItems(), function (item) { return item.AccountingId() == accounting.ID() });
		var invoiceExists = ko.utils.arrayFirst(model.SelectedAccounting().JointDocumentsItems(), function (item) { return (item.TemplateId() == 16) || (item.TemplateId() == 19) });	// 16,19 - счет
		var actExists = ko.utils.arrayFirst(model.SelectedAccounting().JointDocumentsItems(), function (item) { return (item.TemplateId() == 17) || (item.TemplateId() == 18) });	// 17,18 - акт

		if (accounting.IsIncome())
		{
			if (invoiceExists && !(marks && (marks.IsInvoiceChecked() || marks.IsInvoiceRejected())))
				if (confirm("Перегенерировать счет?"))
					model.CreateInvoice();

			if (actExists && !(marks && (marks.IsActChecked() || marks.IsActRejected())))
				if (confirm("Перегенерировать акт и счет-фактуру?"))
				{
					model.CreateAct();
					model.CreateVatInvoice();
				}
		}
		else
		{
			if (actExists && !(marks && (marks.IsAccountingChecked() || marks.IsAccountingRejected())))
				if (confirm("Перегенерировать акт?"))
					model.CreateAct();

			if (actExists && !(marks && (marks.IsAccountingChecked() || marks.IsAccountingRejected())))
				if (confirm("Перегенерировать запрос?"))
					model.CreateRequest();
		}
	};

	model.RegenerateAccountingDocuments = function (accounting)
	{
		if (confirm("Перегенерировать документы?"))
			$.ajax({
				type: "POST",
				url: model.Options.RegenerateAccountingDocumentsUrl,
				data: { AccountingId: accounting.ID() },
				success: function (response)
				{
					var r = JSON.parse(response);
					if (r.Message)
						alert(r.Message);
					else
					{
						alert("Документы обновлены.");
					}
				}
			});
	};

	//#region set/reset for documents /////////////////////////////////////////////////////////////////////////////////////////

	model.SetOriginalSent = function (data)
	{
		var document = data;
		// save Templated document
		$.ajax({
			type: "POST",
			url: model.Options.UpdateTemplatedDocumentUrl,
			data: {
				ID: document.ID(),
				OriginalSentDate: app.utility.SerializeDateTime(new Date()),
				OriginalReceivedDate: app.utility.SerializeDateTime(document.OriginalReceivedDate()),
			},
			success: function (response)
			{
				var r = JSON.parse(response);
				if (r.Message)
					alert(r.Message);
				else
				{
					document.OriginalSentDate(new Date());
					document.OriginalSentUserId(r.OriginalSentUserId);
					document.OriginalReceivedUserId(r.OriginalReceivedUserId);
					model.OpenDocumentDeliveryNumberEdit(document);
				}
			}
		});
	};

	model.ResetOriginalSent = function (data)
	{
		var document = data;
		// save Templated document
		$.ajax({
			type: "POST",
			url: model.Options.UpdateTemplatedDocumentUrl,
			data: {
				ID: document.ID(),
				OriginalSentDate: null,
				OriginalReceivedDate: app.utility.SerializeDateTime(document.OriginalReceivedDate()),
			},
			success: function (response)
			{
				var r = JSON.parse(response);
				if (r.Message)
					alert(r.Message);
				else
				{
					document.OriginalSentDate(null);
					document.OriginalSentUserId(r.OriginalSentUserId);
					document.OriginalReceivedUserId(r.OriginalReceivedUserId);
				}
			}
		});
	};

	model.SetOriginalReceived = function (data)
	{
		var document = data;
		// save Templated document
		$.ajax({
			type: "POST",
			url: model.Options.UpdateTemplatedDocumentUrl,
			data: {
				ID: document.ID(),
				OriginalSentDate: app.utility.SerializeDateTime(document.OriginalSentDate()),
				OriginalReceivedDate: app.utility.SerializeDateTime(new Date()),
			},
			success: function (response)
			{
				var r = JSON.parse(response);
				if (r.Message)
					alert(r.Message);
				else
				{
					document.OriginalSentUserId(r.OriginalSentUserId);
					document.OriginalReceivedDate(new Date());
					document.OriginalReceivedUserId(r.OriginalReceivedUserId);
					model.OpenDocumentDeliveryEdit(document);
				}
			}
		});
	};

	model.ResetOriginalReceived = function (data)
	{
		var document = data;
		// save Templated document
		$.ajax({
			type: "POST",
			url: model.Options.UpdateTemplatedDocumentUrl,
			data: {
				ID: document.ID(),
				OriginalSentDate: app.utility.SerializeDateTime(document.OriginalSentDate()),
				OriginalReceivedDate: null,
			},
			success: function (response)
			{
				var r = JSON.parse(response);
				if (r.Message)
					alert(r.Message);
				else
				{
					document.OriginalSentUserId(r.OriginalSentUserId);
					document.OriginalReceivedDate(null);
					document.OriginalReceivedUserId(r.OriginalReceivedUserId);
				}
			}
		});
	};

	//#endregion

	//#region set/reset for joint documents ///////////////////////////////////////////////////////////////////////////////////

	model.SetJointOriginalSent = function (data)
	{
		var document = data;
		$.ajax({
			type: "POST",
			url: document.IsDocument() ? model.Options.UpdateDocumentUrl : model.Options.UpdateTemplatedDocumentUrl,
			data: {
				ID: document.ID(),
				OriginalSentDate: app.utility.SerializeDateTime(new Date()),
				OriginalReceivedDate: app.utility.SerializeDateTime(document.OriginalReceivedDate()),
			},
			success: function (response)
			{
				var r = JSON.parse(response);
				if (r.Message)
					alert(r.Message);
				else
				{
					document.OriginalSentDate(new Date());
					document.OriginalSentBy(r.OriginalSentUserId);
					document.OriginalReceivedBy(r.OriginalReceivedUserId);
					model.OpenDocumentDeliveryNumberEdit(document);
				}
			}
		});
	};

	model.ResetJointOriginalSent = function (data)
	{
		var document = data;
		$.ajax({
			type: "POST",
			url: document.IsDocument() ? model.Options.UpdateDocumentUrl : model.Options.UpdateTemplatedDocumentUrl,
			data: {
				ID: document.ID(),
				OriginalSentDate: null,
				OriginalReceivedDate: app.utility.SerializeDateTime(document.OriginalReceivedDate()),
			},
			success: function (response)
			{
				var r = JSON.parse(response);
				if (r.Message)
					alert(r.Message);
				else
				{
					document.OriginalSentDate(null);
					document.OriginalSentBy(r.OriginalSentUserId);
					document.OriginalReceivedBy(r.OriginalReceivedUserId);
				}
			}
		});
	};

	model.SetJointOriginalReceived = function (data)
	{
		var document = data;
		$.ajax({
			type: "POST",
			url: document.IsDocument() ? model.Options.UpdateDocumentUrl : model.Options.UpdateTemplatedDocumentUrl,
			data: {
				ID: document.ID(),
				OriginalSentDate: app.utility.SerializeDateTime(document.OriginalSentDate()),
				OriginalReceivedDate: app.utility.SerializeDateTime(new Date()),
			},
			success: function (response)
			{
				var r = JSON.parse(response);
				if (r.Message)
					alert(r.Message);
				else
				{
					document.OriginalSentBy(r.OriginalSentUserId);
					document.OriginalReceivedDate(new Date());
					document.OriginalReceivedBy(r.OriginalReceivedUserId);
					model.OpenDocumentDeliveryEdit(document);
				}
			}
		});
	};

	model.ResetJointOriginalReceived = function (data)
	{
		var document = data;
		$.ajax({
			type: "POST",
			url: document.IsDocument() ? model.Options.UpdateDocumentUrl : model.Options.UpdateTemplatedDocumentUrl,
			data: {
				ID: document.ID(),
				OriginalSentDate: app.utility.SerializeDateTime(document.OriginalSentDate()),
				OriginalReceivedDate: null,
			},
			success: function (response)
			{
				var r = JSON.parse(response);
				if (r.Message)
					alert(r.Message);
				else
				{
					document.OriginalSentBy(r.OriginalSentUserId);
					document.OriginalReceivedDate(null);
					document.OriginalReceivedBy(r.OriginalReceivedUserId);
				}
			}
		});
	};

	//#endregion

	model.ContractOptionsAfterRender = function (option, data)
	{
		if (data && !data.IsActive())
			option.className = "text-danger";
		else
			option.className = "text-muted";
	};

	model.IsDanger = function (contractId)
	{
		var id = ko.unwrap(contractId);
		var contract = ko.utils.arrayFirst(model.ClientContractsItems(), function (item) { return item.ID() == id });
		if (contract && !contract.IsActive())
			return true;
	};

	model.GetAccountingMarkDisplay = function (data)
	{
		var emp = ko.utils.arrayFirst(model.MarksItems(), function (item) { return item.AccountingId() == data.ID() });
		if (emp)
			return (emp.IsAccountingChecked() ? "Расход проверен  " : (emp.IsAccountingOk() ? "Расход Ок " : ""))
				+ (emp.IsActRejected() ? "Акт отказан " : (emp.IsActChecked() ? "Акт проверен " : (emp.IsActOk() ? "Акт Ок " : (emp.IsInvoiceRejected() ? "Счет отказан " : (emp.IsInvoiceChecked() ? "Счет проверен " : (emp.IsInvoiceOk() ? "Счет Ok " : ""))))));

		return "";
	};

	model.GetAccountingDateDisplay = function (data)
	{
		var emp = ko.utils.arrayFirst(model.MarksItems(), function (item) { return item.AccountingId() == data.ID() });
		if (emp)
			return (emp.IsActChecked() ? app.utility.FormatDate(emp.ActCheckedDate) : "") + (emp.IsAccountingChecked() ? app.utility.FormatDate(emp.AccountingCheckedDate) : "");

		return "";
	};

	model.IsLegalNotResident = function ()
	{
		var contract = ko.utils.arrayFirst(model.SelectedAccounting().ContractsItems(), function (item) { return item.ID() == model.SelectedAccounting().ContractId() });
		var legal = model.GetLegal(contract.LegalId());
		return legal.IsNotResident();
	};

	model.RecalculatePricelist = function ()
	{
		$.ajax({
			type: "POST",
			url: model.Options.RecalculatePricelistUrl,
			data: { OrderId: model.Order.ID(), ContractId: model.Order.ContractId() },
			success: function (response)
			{
				var r = JSON.parse(response);
				if (r.Message)
					alert(r.Message);
				else
				{
					model.PricelistId(r.PricelistId);
				}
			}
		});
	};

	model.GetExpenseAccountingName = function (data)
	{
		var expenseId = data.ExpenseAccountingId();
		var accounting = ko.utils.arrayFirst(model.AccountingsItems(), function (item) { return item.ID() == expenseId });
		return (accounting) ? accounting.Number() : "";
	};

	//#region matching ////////////////////////////////////////////////////////////////////////////////////////////////////////

	model.GetIncomeAccountingNames = function (data)
	{
		var result = "";
		if (data.Incomes())
			ko.utils.arrayForEach(data.Incomes(), function (item)
			{
				var accounting = ko.utils.arrayFirst(model.AccountingsItems(), function (acc) { return acc.ID() == item.AccountingId() });
				result += (accounting) ? accounting.Number() + "<br />" : "";
			});

		return result;
	};

	model.GetIncomeAccountingSums = function (data)
	{
		var result = "";
		if (data.Incomes())
			ko.utils.arrayForEach(data.Incomes(), function (item)
			{
				var accounting = ko.utils.arrayFirst(model.AccountingsItems(), function (acc) { return acc.ID() == item.AccountingId() });
				result += (accounting) ? accounting.Sum() + "<br />" : "";
			});

		return result;
	};

	model.GetIncomeSums = function (data)
	{
		var result = "";
		if (data.Incomes())
			ko.utils.arrayForEach(data.Incomes(), function (item) { result += item.Sum() + "<br />"; });

		return result;
	};

	model.GetIncomeAccountingSum = function (data)
	{
		var incomeId = data.AccountingId();
		var accounting = ko.utils.arrayFirst(model.AccountingsItems(), function (item) { return item.ID() == incomeId });
		return (accounting) ? accounting.Sum() : "";
	};

	model.GetMatchingPercent = function (data)
	{
		var sum = 0;
		if (data.Incomes())
			ko.utils.arrayForEach(data.Incomes(), function (item) { sum = app.utility.ParseDecimal(item.Sum()); });

		return app.utility.FormatDecimal((sum * 100) / data.Sum());
	};

	//#endregion

	model.ValidateActDate = function (value)
	{
		var date = app.utility.ParseDate(value);
		var curr = new Date();
		var check1_limit = new Date(curr.getFullYear() - 1, 11, 31);
		var check1_limitNow = new Date(curr.getFullYear(), 0, 21);

		var check2_limit = new Date(curr.getFullYear(), 2, 31);
		var check2_limitNow = new Date(curr.getFullYear(), 3, 21);

		var check3_limit = new Date(curr.getFullYear(), 5, 30);
		var check3_limitNow = new Date(curr.getFullYear(), 6, 21);

		var check4_limit = new Date(curr.getFullYear(), 9, 30);
		var check4_limitNow = new Date(curr.getFullYear(), 10, 21);

		if (curr < check1_limitNow)
		{
		}
		else if (curr < check2_limitNow)
		{
			if (date < check1_limit)
			{
				alert("После 20го числа первого месяца текущего квартала выставление актов и счетов-фактур, датированных предыдущим кварталом, невозможно. Обратитесь в бухгалтерию.");
				return false;
			}
		}
		else if (curr < check3_limitNow)
		{
			if (date < check2_limit)
			{
				alert("После 20го числа первого месяца текущего квартала выставление актов и счетов-фактур, датированных предыдущим кварталом, невозможно. Обратитесь в бухгалтерию.");
				return false;
			}
		}
		else if (curr < check4_limitNow)
		{
			if (date < check3_limit)
			{
				alert("После 20го числа первого месяца текущего квартала выставление актов и счетов-фактур, датированных предыдущим кварталом, невозможно. Обратитесь в бухгалтерию.");
				return false;
			}
		}

		return true;
	};

	// инициализация //////////////////////////////////////////////////////////////////////////////////////////////////////////

	if (model.ContractorId())
		model.ContractsItems(model.GetContractsByContractor(model.ContractorId()));

	setTimeout(function ()
	{
		model.ExtendOrder(model.Order);

		model.GetCargoSeats(model.Order.ID);
		model.GetRoutePoints(model.Order.ID);
		model.GetRouteSegments(model.Order.ID);
		model.GetAccountings(model.Order.ID);
		model.GetJointDocumentsByOrder(model.Order.ID);
		model.GetTemplatedDocumentsByOrder(model.Order.ID);
		model.GetContractorLegals(model.ContractorId);
		model.GetMarksByOrder(model.Order.ID);
		model.GetOperations(model.Order.ID);
		model.GetOrderEvents(model.Order.ID);
		model.GetWorkgroup(model.Order.ID);
		model.CheckDocumentsIsPrintLimit(model.Order.ID);
		model.GetMatchings(model.Order.ID);

		setTimeout(function ()
		{
			if (model.Options.InitialSelectedAccounting)
			{
				$(".nav li a[href='#Accountings']").trigger("click", true);
				var accounting = ko.utils.arrayFirst(model.AccountingsItems(), function (item) { return item.ID() == model.Options.InitialSelectedAccounting });
				if (accounting)
					model.SelectAccounting(accounting);
				else
					setTimeout(function ()
					{
						if (model.Options.InitialSelectedAccounting)
						{
							var accounting = ko.utils.arrayFirst(model.AccountingsItems(), function (item) { return item.ID() == model.Options.InitialSelectedAccounting });
							if (accounting)
								model.SelectAccounting(accounting);
						}
					}, 300);
			}
		}, 200);
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
	};
}

$(function ()
{
	ko.applyBindings(new OrderViewModel(modelData, {
		InitialSelectedAccounting: initialSelectedAccountingId,
		GetMatchingsUrl: app.urls.GetMatchings,
		GetOrdersItemsUrl: app.urls.GetOrdersItems,
		GetWorkgroupItemsUrl: app.urls.GetWorkgroupItems,
		GetCargoSeatsItemsUrl: app.urls.GetCargoSeatsItems,
		GetOperationsItemsUrl: app.urls.GetOperationsItems,
		GetRoutePointsItemsUrl: app.urls.GetRoutePointsItems,
		GetOrderEventsItemsUrl: app.urls.GetOrderEventsItems,
		GetMarksByOrderItemsUrl: app.urls.GetMarksByOrderItems,
		GetRouteSegmentsItemsUrl: app.urls.GetRouteSegmentsItems,
		GetContactsByLegalItemsUrl: app.urls.GetContactsByLegalItems,
		GetAccountingsItemsUrl: app.urls.GetAccountingsItems,
		GetContractsByLegalItemsUrl: app.urls.GetContractsByLegalItems,
		GetEmployeesByLegalItemsUrl: app.urls.GetEmployeesByLegalItems,
		GetDocumentsByOrderItemsUrl: app.urls.GetDocumentsByOrderItems,
		GetAccountingsByLegalItemsUrl: app.urls.GetAccountingsByLegalItems,
		GetLegalsByContractorItemsUrl: app.urls.GetLegalsByContractorItems,
		GetBankAccountsByLegalItemsUrl: app.urls.GetBankAccountsByLegalItems,
		GetPaymentsByAccountingItemsUrl: app.urls.GetPaymentsByAccountingItems,
		GetServicesByAccountingItemsUrl: app.urls.GetServicesByAccountingItems,
		GetContractsByContractorItemsUrl: app.urls.GetContractsByContractorItems,
		GetJointDocumentsByOrderItemsUrl: app.urls.GetJointDocumentsByOrderItems,
		GetDocumentsByAccountingItemsUrl: app.urls.GetDocumentsByAccountingItems,
		GetRouteSegmentsByAccountingItemsUrl: app.urls.GetRouteSegmentsByAccountingItems,
		GetTemplatedDocumentsByOrderItemsUrl: app.urls.GetTemplatedDocumentsByOrderItems,
		GetJointDocumentsByAccountingItemsUrl: app.urls.GetJointDocumentsByAccountingItems,

		UserDetailsUrl: app.urls.UserDetails,
		OrderDetailsUrl: app.urls.OrderDetails,
		ContractDetailsUrl: app.urls.ContractDetails,
		ContractorDetailsUrl: app.urls.ContractorDetails,
		OrderAccountingDetailsUrl: app.urls.OrderAccountingDetails,

		GetNewServiceUrl: app.urls.GetNewService,
		GetRoutePointUrl: app.urls.GetRoutePoint,
		GetNewDocumentUrl: app.urls.GetNewDocument,
		GetNewCargoSeatUrl: app.urls.GetNewCargoSeat,
		GetNewOperationUrl: app.urls.GetNewOperation,
		GetNewWorkgroupUrl: app.urls.GetNewWorkgroup,
		GetNewRoutePointUrl: app.urls.GetNewRoutePoint,
		GetNewAccountingUrl: app.urls.GetNewAccounting,

		CreateActUrl: app.urls.CreateAct,
		CreateClaimUrl: app.urls.CreateClaim,
		CreateInvoiceUrl: app.urls.CreateInvoice,
		CreateVatInvoiceUrl: app.urls.CreateVatInvoice,
		CreateShortLegalUrl: app.urls.CreateShortLegal,
		CreateRequestUrl: app.urls.CreateRequest,
		CreateRouteContactUrl: app.urls.EditRouteContact,
		CreateDetailingUrl: app.urls.CreateDetailing,
		CreateAmpleDetailingUrl: app.urls.CreateAmpleDetailing,

		GetBankUrl: app.urls.GetBank,
		GetPriceUrl: app.urls.GetPrice,
		GetBanksUrl: app.urls.GetBanks,
		GetLegalUrl: app.urls.GetLegal,
		GetPlaceUrl: app.urls.GetPlace,
		GetPlacesUrl: app.urls.GetPlaces,
		GetPersonsUrl: app.urls.GetPersons,
		GetContractorUrl: app.urls.GetContractor,

		GetActionHintUrl: app.urls.GetActionHint,
		EditRouteContactUrl: app.urls.EditRouteContact,
		CheckStatusRulesUrl: app.urls.CheckStatusRules,
		CloneOrderUrl: app.urls.CloneOrder,
		CloseOrderUrl: app.urls.CloseOrder,
		ViewDocumentUrl: app.urls.ViewDocument,
		OpenDocumentUrl: app.urls.OpenDocument,
		UploadDocumentUrl: app.urls.UploadDocument,
		UpdateDocumentUrl: app.urls.UpdateDocument,
		ChangeTemplateUrl: app.urls.ChangeOrderTemplate,
		GetOrderBalanceUrl: app.urls.GetOrderBalance,
		GetCurrencyRateUrl: app.urls.GetCurrencyRate,
		RecalculateRouteUrl: app.urls.RecalculateRoute,
		ChangeContractorUrl: app.urls.ChangeContractor,
		DeleteAccountingUrl: app.urls.DeleteAccounting,
		ChangeOrderStatusUrl: app.urls.ChangeOrderStatus,
		ChangeOrderProductUrl: app.urls.ChangeOrderProduct,
		GetAccountingMarksUrl: app.urls.GetAccountingMarks,
		UpdateAccountingSumUrl: app.urls.UpdateAccountingSum,
		CalculateRouteLengthUrl: app.urls.CalculateRouteLength,
		RecalculatePricelistUrl: app.urls.RecalculatePricelist,
		RecalculateCargoSeatsUrl: app.urls.RecalculateCargoSeats,
		ToggleDocumentIsPrintUrl: app.urls.ToggleDocumentIsPrint,
		GetContractCurrenciesUrl: app.urls.GetContractCurrencies,
		ViewTemplatedDocumentUrl: app.urls.ViewTemplatedDocument,
		OpenTemplatedDocumentUrl: app.urls.OpenTemplatedDocument,
		GetCurrentCurrencyRateUrl: app.urls.GetCurrentCurrencyRate,
		GetOrderTemplatesByProductUrl: app.urls.GetOrderTemplatesByProduct,
		GetOrderTemplatesByContractUrl: app.urls.GetOrderTemplatesByContract,
		OpenMergedAccountingDocumentsUrl: app.urls.OpenMergedAccountingDocuments,
		OpenClientAccountingDocumentsUrl: app.urls.OpenClientAccountingDocuments,
		RegenerateAccountingDocumentsUrl: app.urls.RegenerateAccountingDocuments,
		CheckDocumentsIsPrintLimitUrl: app.urls.CheckDocumentsIsPrintLimit,
		RecalculatePaymentPlanDateUrl: app.urls.RecalculatePaymentPlanDate,
		DeleteTemplatedDocumentUrl: app.urls.DeleteTemplatedDocument,
		UpdateTemplatedDocumentUrl: app.urls.UpdateTemplatedDocument,
		GetOrderStatusHistoryUrl: app.urls.GetOrderStatusHistory,
		DownloadPaymentsFileUrl: app.urls.DownloadPaymentsFile,
		SetCurrencyRateUrl: app.urls.SetCurrencyRate,
		ChangePayMethodUrl: app.urls.ChangePayMethod,
		IsHasVatInvoiceUrl: app.urls.IsHasVatInvoice,
		ViewPricelistUrl: app.urls.ViewPricelist,

		ToggleAccountingInvoiceOkUrl: app.urls.ToggleAccountingInvoiceOk,
		ToggleAccountingInvoiceCheckedUrl: app.urls.ToggleAccountingInvoiceChecked,
		ToggleAccountingInvoiceRejectedUrl: app.urls.ToggleAccountingInvoiceRejected,
		ToggleAccountingActOkUrl: app.urls.ToggleAccountingActOk,
		ToggleAccountingActCheckedUrl: app.urls.ToggleAccountingActChecked,
		ToggleAccountingActRejectedUrl: app.urls.ToggleAccountingActRejected,
		ToggleAccountingOkUrl: app.urls.ToggleAccountingOk,
		ToggleAccountingCheckedUrl: app.urls.ToggleAccountingChecked,
		ToggleAccountingRejectedUrl: app.urls.ToggleAccountingRejected,

		SaveDocumentDeliveryInfoUrl: app.urls.SaveDocumentDeliveryInfo,
		SaveRouteSegmentUrl: app.urls.SaveRouteSegment,
		SaveRoutePointUrl: app.urls.SaveRoutePoint,
		SaveAccountingUrl: app.urls.SaveAccounting,
		SaveWorkgroupUrl: app.urls.SaveWorkgroup,
		SaveCargoSeatUrl: app.urls.SaveCargoSeat,
		SaveOperationUrl: app.urls.SaveOperation,
		SaveDocumentUrl: app.urls.SaveDocument,
		SaveServiceUrl: app.urls.SaveService,
		SaveOrderUrl: app.urls.SaveOrder
	}), document.getElementById("ko-root"));
});

