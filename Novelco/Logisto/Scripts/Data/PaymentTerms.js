var PaymentTermsViewModel = function (source, options)
{
	var model = this;

	model.Options = $.extend({
		GetNewPaymentTermUrl: null,
		SavePaymentTermUrl: null
	}, options);

	// поля ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	// Список записей
	model.Items = ko.mapping.fromJS(source);

	// вычисляемое/подписки ///////////////////////////////////////////////////////////////////////////////////////////////////


	// инициализация //////////////////////////////////////////////////////////////////////////////////////////////////////////


	// функции ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	model.SavePaymentTerm = function (data)
	{
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.SavePaymentTermUrl,
			data: {
				ID: data.ID(),
				Display: data.Display(),
				EnName: data.EnName(),

				Condition1_Percent: data.Condition1_Percent(),
				Condition1_From: data.Condition1_From(),
				Condition1_Days: data.Condition1_Days(),
				Condition1_BankDays: data.Condition1_BankDays(),
				Condition1_OrdersFrom: data.Condition1_OrdersFrom(),
				Condition1_OrdersTo: data.Condition1_OrdersTo(),

				Condition2_Percent: data.Condition2_Percent(),
				Condition2_From: data.Condition2_From(),
				Condition2_Days: data.Condition2_Days(),
				Condition2_BankDays: data.Condition2_BankDays(),
				Condition2_OrdersFrom: data.Condition2_OrdersFrom(),
				Condition2_OrdersTo: data.Condition2_OrdersTo()
				//IsDeleted: data.IsDeleted()
			},
			success: function (response)
			{
				var id = JSON.parse(response).ID;
				if (id)
					data.ID(id);
			}
		});
	};

	// #region create/edit modal //////////////////////////////////////////////////////////////////////////////////////////////

	var paymentTermModalSelector = "#paymentTermEditModal";

	model.OpenPaymentTermCreate = function ()
	{
		var data = null;
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.GetNewPaymentTermUrl,
			success: function (response)
			{
				data = ko.mapping.fromJSON(response);
				//model.ExtendTemplate(data, model.SelectedTemplate());
				model.Items.push(data);
			}
		});

		model.PaymentTermEditModal.CurrentItem(data);
		$(paymentTermModalSelector).modal("show");
		//$(paymentTermModalSelector).draggable({ handle: ".modal-header" });
		model.PaymentTermEditModal.OnClosed = function () { model.Items.remove(data); };
		model.PaymentTermEditModal.Init();
	};

	model.OpenPaymentTermEdit = function (data)
	{
		model.PaymentTermEditModal.CurrentItem(data);
		$(paymentTermModalSelector).modal("show");
		//$(paymentTermModalSelector).draggable({ handle: ".modal-header" });;
		model.PaymentTermEditModal.Init();
	};

	model.PaymentTermEditModal = {
		CurrentItem: ko.observable(),
		Init: function ()	{	},
		Done: function (data, e)
		{
			$(paymentTermModalSelector).modal("hide");
			// сохранить изменения
			model.SavePaymentTerm(model.PaymentTermEditModal.CurrentItem());
		}
	};

	// #endregion
}

$(function ()
{
	ko.applyBindings(new PaymentTermsViewModel(modelData, {
		GetNewPaymentTermUrl: app.urls.GetNewPaymentTerm,
		SavePaymentTermUrl: app.urls.SavePaymentTerm
	}), document.getElementById("ko-root"));
});