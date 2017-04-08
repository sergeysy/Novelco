var ScheduleViewModel = function (source, options)
{
	var model = this;

	model.Options = $.extend({
		GetNewScheduleUrl: null,
		DeleteScheduleUrl: null,
		SaveScheduleUrl: null
	}, options);

	// поля ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	// Список записей
	model.Items = ko.mapping.fromJS(source);
	//
	model.Reports = ko.observableArray([{ ID: 1, Display: "Контроль дебиторской задолженности" },{ ID: 2, Display: "Рассылка уведомлений клиентам" }]);
	//
	model.Weekdays = ko.observableArray([
		{ ID: 1, Display: "Понедельник" },
		{ ID: 2, Display: "Вторник" },
		{ ID: 3, Display: "Среда" },
		{ ID: 4, Display: "Четверг" },
		{ ID: 5, Display: "Пятница" },
		{ ID: 6, Display: "Суббота" },
		{ ID: 0, Display: "Воскресенье" }
	]);

	// вычисляемое/подписки ///////////////////////////////////////////////////////////////////////////////////////////////////

	// функции ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	model.ExtendSchedules = function (array)
	{
		ko.utils.arrayForEach(array, function (item) { model.ExtendSchedule(item); })
	};

	model.ExtendSchedule = function (schedule)
	{
		schedule.WeekdayDisplay = schedule.WeekdayDisplay || ko.computed(function () { return ko.utils.arrayFirst(model.Weekdays(), function (item) { return item.ID == schedule.Weekday() }).Display });
		schedule.TimeDisplay = schedule.TimeDisplay || ko.computed(function () { return app.utility.Pad(schedule.Hour(), 2) + ":" + app.utility.Pad(schedule.Minute(), 2) });
	};

	model.SaveSchedule = function (data)
	{
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.SaveScheduleUrl,
			data: {
				ID: data.ID(),
				ReportName: data.ReportName(),
				Weekday: data.Weekday(),
				Hour: data.Hour(),
				Minute: data.Minute()
			},
			success: function (response)
			{
				var id = JSON.parse(response).ID;
				if (id)
					data.ID(id);
			}
		});
	};

	model.DeleteSchedule = function (data)
	{
		$.ajax({
			type: "POST",
			url: model.Options.DeleteScheduleUrl,
			data: { ID: data.ID() },
			success: function (response) { model.Items.remove(data); }
		});
	};

	// #region create/edit modal //////////////////////////////////////////////////////////////////////////////////////////////

	var ScheduleModalSelector = "#scheduleEditModal";

	model.OpenScheduleCreate = function ()
	{
		$.ajax({
			type: "POST",
			url: model.Options.GetNewScheduleUrl,
			success: function (response)
			{
				var data = ko.mapping.fromJSON(response);
				model.ExtendSchedule(data);
				model.Items.push(data);
				model.ScheduleEditModal.CurrentItem(data);
				$(ScheduleModalSelector).modal("show");
				$(ScheduleModalSelector).draggable({ handle: ".modal-header" });
				model.ScheduleEditModal.OnClosed = function () { model.Items.remove(data); };
				model.ScheduleEditModal.Init();
			}
		});
	};

	model.OpenScheduleEdit = function (data)
	{
		model.ScheduleEditModal.CurrentItem(data);
		$(ScheduleModalSelector).modal("show");
		$(ScheduleModalSelector).draggable({ handle: ".modal-header" });;
		model.ScheduleEditModal.Init();
	};

	model.ScheduleEditModal = {
		CurrentItem: ko.observable(),
		CurrentReportId: ko.observable(1),
		Init: function () { },
		OnClosed: null,
		Close: function (self, e)
		{
			$(ScheduleModalSelector).modal("hide");
			if (self.OnClosed != null)
				self.OnClosed();
		},
		Done: function (self, e)
		{
			$(ScheduleModalSelector).modal("hide");
			self.CurrentItem().ReportName(ko.utils.arrayFirst(model.Reports(), function (item) { return item.ID == self.CurrentReportId() }).Display);
			// сохранить изменения
			model.SaveSchedule(self.CurrentItem());
		}
	};

	// #endregion

	// инициализация //////////////////////////////////////////////////////////////////////////////////////////////////////////

	model.ExtendSchedules(model.Items());
}

$(function ()
{
	ko.applyBindings(new ScheduleViewModel(modelData, {
		GetNewScheduleUrl: app.urls.GetNewSchedule,
		DeleteScheduleUrl: app.urls.DeleteSchedule,
		SaveScheduleUrl: app.urls.SaveSchedule
	}), document.getElementById("ko-root"));
});