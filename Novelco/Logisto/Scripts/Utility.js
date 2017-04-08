
var app = app || {};
app.utility = app.utility || {};

app.utility.FormatDecimal = function (data, n)
{
	var value = ko.unwrap(data);
	if ((value === undefined) || (value === null))
		return "";

	if (value === 0)
		return "0";

	if (typeof (value) == 'string')
		return value;

	var str = value.toFixed(n ? n : 2);
	var p = str.indexOf('.');
	return str.replace(/\d(?=(?:\d{3})+(?:\.|$))/g, function ($0, i)
	{
		return p < 0 || i < p ? ($0 + ' ') : $0;
	});
};

app.utility.ParseDecimal = function (data)
{
	var value = ko.unwrap(data);
	if ((value === undefined) || (value === null))
		return NaN;

	if (value === 0)
		return 0;

	if (typeof (value) == 'number')
		return value;

	if (typeof (value) == 'float')
		return value;

	return parseFloat(value.replace(',', '.'));
};

app.utility.ParseDate = function (data)
{
	if (!data)
		return "";

	var value = ko.unwrap(data);
	if (!value)
		return "";

	var jsDateTime;
	if (value instanceof Date)
		jsDateTime = value;
	else
		jsDateTime = JsonDateToJavascriptDate(value);

	// проверить на Invalid date
	if (isNaN(jsDateTime.getDate()))
		return "";

	return jsDateTime;
};

app.utility.FormatDate = function (data)
{
	if (!data)
		return "";

	var value = ko.unwrap(data);
	if (!value)
		return "";

	var jsDateTime;
	if (value instanceof Date)
		jsDateTime = value;
	else
		jsDateTime = JsonDateToJavascriptDate(value);

	// проверить на Invalid date
	if (isNaN(jsDateTime.getDate()))
		return ".";

	//var format = jsDateTime.getFullYear() == new Date().getFullYear() ? 'D MMMM' : 'D MMMM YYYY';
	//return moment(jsDateTime).format(format);

	return app.utility.Pad(jsDateTime.getDate(), 2) + "-"
		+ app.utility.Pad((jsDateTime.getMonth() + 1), 2) + "-"
		+ jsDateTime.getFullYear();
};

app.utility.FormatDateTime = function (data)
{
	if (!data)
		return "";

	var value = ko.unwrap(data);
	if (!value)
		return "";

	var jsDateTime;
	if (value instanceof Date)
		jsDateTime = value;
	else
		jsDateTime = JsonDateToJavascriptDate(value);

	// проверить на Invalid date
	if (isNaN(jsDateTime.getDate()))
		return ".";

	//var format = jsDateTime.getFullYear() == new Date().getFullYear() ? 'D MMMM' : 'D MMMM YYYY';
	//return moment(jsDateTime).format(format);

	return app.utility.Pad(jsDateTime.getDate(), 2) + "-"
		+ app.utility.Pad((jsDateTime.getMonth() + 1), 2) + "-"
		+ jsDateTime.getFullYear() + " "
		+ app.utility.Pad(jsDateTime.getHours(), 2) + ":"
		+ app.utility.Pad(jsDateTime.getMinutes(), 2);
};

app.utility.FormatTime = function (data)
{
	if (!data)
		return "";

	var value = ko.unwrap(data);
	if (!value)
		return "";

	var jsDateTime;
	if (value instanceof Date)
		jsDateTime = value;
	else
		jsDateTime = JsonDateToJavascriptDate(value);

	// проверить на Invalid date
	if (isNaN(jsDateTime.getDate()))
		return ".";

	return app.utility.Pad(jsDateTime.getHours(), 2) + ":" + app.utility.Pad(jsDateTime.getMinutes(), 2);
};

// Функция отображения Display по значению ID (типизированные справочники)
app.utility.GetDisplay = function (dictionary, value)
{
	var values = ko.unwrap(dictionary);
	var target = ko.unwrap(value);
	var found = ko.utils.arrayFirst(values, function (item) { return ko.unwrap(item.ID) == target });
	if (found === null)
		return "";
	else
		return ko.unwrap(found.Display || found.Name);
};

app.utility.GetDisplaySP = function (dictionary, value)
{
	var values = ko.unwrap(dictionary);
	var target = ko.unwrap(value);
	var found = ko.utils.arrayFirst(values, function (item) { return ko.unwrap(item.ID) == target });
	if (found === null)
		return "";
	else
		return ko.unwrap(found.Display || found.Name).replace(/ /g, '&nbsp;');
};


app.utility.Pad = function (n, width, z)
{
	z = z || '0';
	n = n + '';
	return n.length >= width ? n : new Array(width - n.length + 1).join(z) + n;
}


//Функция конвертации JSON-UTC-даты в Javascript-дату часового пояса пользователя
function JsonUtcDateToJavascriptClientDate(jsonUtcDate)
{
	if (!jsonUtcDate)
	{
		return null;
	}
	var s = jsonUtcDate.split(/[-T:Z]/ig); //JSON date in ISO-8601 format
	if (s.length == 6)
	{
		return new Date(Date.UTC(s[0], --s[1], s[2], s[3], s[4], s[5]));
	} else
	{
		return JsonDateToJavascriptDate(jsonUtcDate);
	}
}

//Функция конвертации JSON-даты в Javascript-дату
function JsonDateToJavascriptDate(jsonDate)
{

	try
	{
		var m = jsonDate.match(/^(\d+)(-|\/)(\d+)(-|\/)(\d+)$/);

		if (jsonDate.indexOf("/Date(") == 0)
		{
			return new Date(parseInt(jsonDate.substring(6)));
		}
		else if (m)
		{
			return new Date(+m[5], +m[3] - 1, +m[1]);
		}
		else
		{
			if ((app.currentBrowser == "InternetExplorer") || (app.currentBrowser == "Firefox"))
				return new Date(jsonDate);
			else
			{
				var date = new Date(jsonDate);
				var offset = date.getTimezoneOffset();
				return new Date(date.setMinutes(date.getMinutes() + offset));
			}
		}
	}
	catch (ex)
	{
		return new Date();
	}
}

app.utility.SerializeDateTime = function (data)
{
	if (!data)
		return "";

	var value = ko.unwrap(data);
	if (!value)
		return "";

	var jsDateTime;
	if (value instanceof Date)
		jsDateTime = value;
	else
		jsDateTime = JsonDateToJavascriptDate(value);

	// проверить на Invalid date
	if (isNaN(jsDateTime.getDate()))
		return ".";

	//return jsDateTime.toISOString();

	// формирование строки без учета смещения
	return jsDateTime.getFullYear() + "-"
	+ app.utility.Pad((jsDateTime.getMonth() + 1), 2) + "-"
	+ app.utility.Pad(jsDateTime.getDate(), 2) + "T"
	+ app.utility.Pad(jsDateTime.getHours(), 2) + ":"
	+ app.utility.Pad(jsDateTime.getMinutes(), 2) + "Z";
};


ko.bindingHandlers.datepicker = {
	init: function (element, valueAccessor, allBindingsAccessor)
	{
		//initialize datepicker with some optional options
		var options = allBindingsAccessor().datepickerOptions || {};

		var funcOnSelectdate = function ()
		{
			var observable = valueAccessor();
			observable($(element).datepicker("getDate"));
		}

		options.onSelect = funcOnSelectdate;
		options.changeMonth = true;
		options.changeYear = true;

		$(element).datepicker(options);

		//handle the field changing
		ko.utils.registerEventHandler(element, "change", funcOnSelectdate);

		//handle disposal (if KO removes by the template binding)
		ko.utils.domNodeDisposal.addDisposeCallback(element, function ()
		{
			$(element).datepicker("destroy");
		});

	},
	update: function (element, valueAccessor)
	{
		var value = ko.utils.unwrapObservable(valueAccessor());
		if (value == null)
		{
			$(element).datepicker("setDate", null);
			return;
		}

		if (value instanceof Date)
		{
			var current = $(element).datepicker("getDate");

			if (value - current !== 0)
				$(element).datepicker("setDate", value);

			return;
		}

		if (typeof (value) === "string")  // JSON string from server
			value = value.split("T")[0]; // Removes time

		var current = $(element).datepicker("getDate");

		if (value - current !== 0)
		{
			var parsedDate = $.datepicker.parseDate('yy-mm-dd', value);
			$(element).datepicker("setDate", parsedDate);
		}
	}
};


app.utility.AjaxErrorHandler = function ()
{
	//Глобальная обработка ajax-ошибок
	$(document).ajaxError(function (e, xhr)
	{
		if (app.utility.AjaxErrorHandler.ErrorCallback != null)
			app.utility.AjaxErrorHandler.ErrorCallback();
		else
		{
			switch (xhr.status)
			{
				case 400:
					if (app.utility.AjaxErrorHandler.Error400Callback == null)
						alert("Страница не найдена(плохой запрос)");
					else
						app.utility.AjaxErrorHandler.Error400Callback();

					break;
				case 403:
					window.location = "/Login";
					break;
				case 404:
					alert("Страница не найдена");
					break;
				case 500:
					alert("Ошибка обработки запроса");
					break;
			}
		}
	});
}