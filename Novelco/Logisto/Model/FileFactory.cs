using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using GemBox.Spreadsheet;
using Logisto.BusinessLogic;
using OfficeOpenXml;
using IO = System.IO;

namespace Logisto.Models
{
	public class FileFactory
	{
		BankLogic bankLogic;
		DataLogic dataLogic;
		OrderLogic orderLogic;
		LegalLogic legalLogic;
		PersonLogic personLogic;
		EmployeeLogic employeeLogic;
		ContractLogic contractLogic;
		DocumentLogic documentLogic;
		PricelistLogic pricelistLogic;
		AccountingLogic accountingLogic;
		ParticipantLogic participantLogic;

		public FileFactory()
		{
			// TODO: DI
			bankLogic = new BankLogic();
			dataLogic = new DataLogic();
			orderLogic = new OrderLogic();
			legalLogic = new LegalLogic();
			personLogic = new PersonLogic();
			employeeLogic = new EmployeeLogic();
			contractLogic = new ContractLogic();
			documentLogic = new DocumentLogic();
			pricelistLogic = new PricelistLogic();
			accountingLogic = new AccountingLogic();
			participantLogic = new ParticipantLogic();
		}

		#region формирование документов

		public string GenerateInvoice(TemplatedDocument entity, TemplatedDocumentFormat format)
		{
			#region подготовка данных и проверки

			var accounting = accountingLogic.GetAccounting(entity.AccountingId.Value);
			if (!accounting.LegalId.HasValue)
				return "Не заполнено Юрлицо в доходе";

			var services = accountingLogic.GetServicesByAccounting(accounting.ID);
			if (services.Count() == 0)
				return "Нет услуг";

			var order = orderLogic.GetOrder(accounting.OrderId);
			var contract = contractLogic.GetContract(order.ContractId.Value);
			var legal = legalLogic.GetLegal(accounting.LegalId.Value);
			var ourLegal = legalLogic.GetLegalByOurLegal(accounting.OurLegalId.Value);
			var director = employeeLogic.GetEmployee(ourLegal.DirectorId.Value);
			var directorPerson = personLogic.GetPerson(director.PersonId.Value);

			var accountant = participantLogic.GetWorkgroupByOrderAtDate(order.ID, accounting.InvoiceDate.Value).Where(w => w.ParticipantRoleId == (int)ParticipantRoles.BUH).FirstOrDefault();
			var accountantPerson = personLogic.GetPersonByUser(accountant.UserId.Value);

			var docs = documentLogic.GetDocumentsByOrder(order.ID).Where(w => w.IsPrint == true);
			var currencyId = services.First().CurrencyId ?? 1;
			var contractCurrencies = contractLogic.GetContractCurrencies(contract.ID);
			var currency = contractCurrencies.FirstOrDefault(w => w.CurrencyId == currencyId);
			currency = currency ?? contractCurrencies.FirstOrDefault(w => w.CurrencyId == 1);   // рубли присутствуют обязательно в этом случае
			if (currency == null)
				return "Нет валют в договоре";

			var ourBankAccount = bankLogic.GetBankAccount(currency.OurBankAccountId);
			var ourBank = bankLogic.GetBank(ourBankAccount.BankId.Value);

			RoutePoint fromPoint = null;
			RoutePoint toPoint = null;
			List<RouteSegment> segments = new List<RouteSegment>();
			var selectedSegments = accountingLogic.GetAccountingRouteSegments(accounting.ID);
			if (selectedSegments.Count() != 0)
			{
				var fromSegment = orderLogic.GetRouteSegment(selectedSegments.First().RouteSegmentId.Value);
				fromPoint = orderLogic.GetRoutePoint(fromSegment.FromRoutePointId);
				var toSegment = orderLogic.GetRouteSegment(selectedSegments.Last().RouteSegmentId.Value);
				toPoint = orderLogic.GetRoutePoint(toSegment.ToRoutePointId);

				foreach (var ss in selectedSegments)
					segments.Add(orderLogic.GetRouteSegment(ss.RouteSegmentId.Value));
			}
			else
			{
				var points = orderLogic.GetRoutePoints(order.ID);
				if (points.Count() < 2)
					return "Недостаточно маршрутных точек";

				fromPoint = points.First();
				toPoint = points.Last();
				segments = orderLogic.GetRouteSegments(order.ID).ToList();
			}

			string vehicleNumbers = "";
			foreach (var segment in segments)
				if (!string.IsNullOrEmpty(segment.VehicleNumber))
					vehicleNumbers += segment.VehicleNumber + "; ";

			bool isAviaProduct = order.ProductId.In(1, 2, 3, 8);
			bool isAutoProduct = order.ProductId.In(4, 5, 9);

			// справочники
			var docTypes = dataLogic.GetDocumentTypes();
			var serviceTypes = dataLogic.GetServiceTypes();
			var serviceKinds = dataLogic.GetServiceKinds();
			var currencies = dataLogic.GetCurrencies();
			var currencyRateUses = dataLogic.GetCurrencyRateUses();

			#endregion

			var filename = HttpContext.Current.Server.MapPath("~\\Temp\\" + Environment.TickCount + ".xlsx");
			DownloadTemplateData(entity.TemplateId.Value, filename);

			#region process

			var sw = new Stopwatch();
			sw.Start();

			using (var xl = new ExcelPackage(new IO.FileInfo(filename)))
			{
				xl.DoAdjustDrawings = true;
				xl.Workbook.CalcMode = ExcelCalcMode.Automatic;

				var sheet = xl.Workbook.Worksheets[1];
				var col = 15;
				var row = 1;
				sheet.Cells[row++, col].Value = accounting.InvoiceNumber;
				sheet.Cells[row++, col].Value = FormatDate(accounting.InvoiceDate);
				sheet.Cells[row++, col].Value = accounting.OriginalSum;
				sheet.Cells[row++, col].Value = accounting.OriginalVat;
				sheet.Cells[row++, col].Value = (format != TemplatedDocumentFormat.CutPdf) ? accountantPerson.DisplayName : " ";
				sheet.Cells[row++, col].Value = (format != TemplatedDocumentFormat.CutPdf) ? directorPerson.DisplayName : " ";
				sheet.Cells[row++, col].Value = order.Number;
				sheet.Cells[row++, col].Value = order.RequestNumber;
				sheet.Cells[row++, col].Value = FormatDate(order.LoadingDate);
				sheet.Cells[row++, col].Value = (order.SeatsCount > 0) ? order.SeatsCount.Value.ToString() : " ";
				sheet.Cells[row++, col].Value = (order.GrossWeight > 0) ? order.GrossWeight.Value.ToString("0.00") : " ";
				sheet.Cells[row++, col].Value = order.InvoiceSum;

				if (isAviaProduct)
					sheet.Cells[row++, col].Value = (order.PaidWeight > 0) ? order.PaidWeight.Value.ToString("0.00") : " ";
				else if (isAutoProduct)
					sheet.Cells[row++, col].Value = (!string.IsNullOrWhiteSpace(vehicleNumbers)) ? vehicleNumbers : " ";
				else
					row++;

				sheet.Cells[row++, col].Value = FormatPlace(fromPoint.PlaceId.Value) + " ";
				sheet.Cells[row++, col].Value = FormatPlace(toPoint.PlaceId.Value) + " ";
				sheet.Cells[row++, col].Value = contract.Number;
				sheet.Cells[row++, col].Value = FormatDate(contract.Date);
				sheet.Cells[row++, col].Value = legal.DisplayName;
				sheet.Cells[row++, col].Value = legal.TIN;
				sheet.Cells[row++, col].Value = legal.KPP;
				sheet.Cells[row++, col].Value = legal.Address;
				sheet.Cells[row++, col].Value = legal.PostAddress;
				sheet.Cells[row++, col].Value = ourLegal.DisplayName;
				sheet.Cells[row++, col].Value = ourLegal.TIN;
				sheet.Cells[row++, col].Value = ourLegal.KPP;
				sheet.Cells[row++, col].Value = ourLegal.OGRN;
				sheet.Cells[row++, col].Value = ourLegal.Address;
				sheet.Cells[row++, col].Value = ourBankAccount.Number;
				sheet.Cells[row++, col].Value = ourBank.Name;
				sheet.Cells[row++, col].Value = ourBank.Address;
				sheet.Cells[row++, col].Value = ourBank.BIC;
				sheet.Cells[row++, col].Value = ourBank.KSNP;
				sheet.Cells[row++, col].Value = currencies.First(w => w.ID == currencyId).Display;
				sheet.Cells[row++, col].Value = (currencyId > 1) ? "Оплата по курсу " + currencyRateUses.First(w => w.ID == (contract.CurrencyRateUseId ?? 1)).Display : " ";

				// HACK: плохой хак - сокрытие/модификация заголовков
				if (!order.LoadingDate.HasValue)
					sheet.Cells["E9"].Value = "";
				if (!order.SeatsCount.HasValue || (order.SeatsCount.Value == 0))
					sheet.Cells["E10"].Value = "";
				if (!order.GrossWeight.HasValue || (order.GrossWeight.Value == 0))
					sheet.Cells["E11"].Value = "";

				if (isAviaProduct && (order.PaidWeight > 0))
					sheet.Cells["E12"].Value = "Оплачиваемый вес, кг :";
				else if (isAutoProduct && (!string.IsNullOrWhiteSpace(vehicleNumbers)))
					sheet.Cells["E12"].Value = "Номер ТС :";
				else
					sheet.Cells["E12"].Value = "";

				//if (string.IsNullOrWhiteSpace(order.From))
				//	sheet.Cells["E13"].Value = "";
				//if (string.IsNullOrWhiteSpace(order.To))
				//	sheet.Cells["E14"].Value = "";

				// документы
				foreach (var doc in docs)
				{
					sheet.Cells[row++, col].Value = docTypes.Where(w => w.ID == doc.DocumentTypeId).Select(s => s.Display).FirstOrDefault() + " № :";
					sheet.Cells[row++, col].Value = doc.Number;
				}

				var pricelist = GetPricelist(order) ?? new Pricelist(); // HACK:
				var priceKinds = pricelistLogic.GetPriceKinds(pricelist.ID);
				// строки
				bool isFirst = true;
				int srow = 20;
				var serviceB = new List<IdTuple>();
				foreach (var service in services)
					if (service.ServiceTypeId.HasValue)
					{
						int serviceKindId = serviceTypes.First(w => w.ID == service.ServiceTypeId).ServiceKindId;
						var resultName = serviceKinds.First(w => w.ID == serviceKindId).Name;
						if (pricelist.ID > 0)
							resultName = priceKinds.First(w => w.ServiceKindId == serviceKindId).Name;

						// вычисление НДС
						double tax = 0;
						if (service.VatId.HasValue)
						{
							var vat = dataLogic.GetVats().First(w => w.ID == service.VatId.Value);
							tax = service.OriginalSum.Value - service.OriginalSum.Value / (100 + vat.Percent) * 100;
						}

						var sbase = serviceB.FirstOrDefault(w => w.Id == serviceKindId);
						if (sbase == null)
							serviceB.Add(new IdTuple { Id = serviceKindId, Name = resultName, Value = service.OriginalSum.Value, Value2 = tax });
						else
						{
							sbase.Value += service.OriginalSum.Value;
							sbase.Value2 += tax;
						}
					}

				foreach (var service in serviceB)
				{
					if (!isFirst)
					{
						sheet.InsertRow(srow, 1);
						var r1 = sheet.Cells["A" + srow + ":" + "M" + srow];
						var r2 = sheet.Cells["A" + (srow - 1) + ":" + "M" + (srow - 1)];
						r2.Copy(r1);
						sheet.Row(46).Height = 1;
					}

					sheet.Cells[srow, 2].Value = service.Name;
					sheet.Cells[srow, 11].Value = Math.Round(service.Value2, 2);
					sheet.Cells[srow, 12].Value = Math.Round(service.Value, 2);
					isFirst = false;
					srow++;
				}

				// вычисление значений во всех ячейках
				var allCells = sheet.Cells;
				foreach (var item in allCells)
					item.Calculate();

				if (format == TemplatedDocumentFormat.Pdf)
				{
					var ourLegalE = legalLogic.GetOurLegals().First(w => w.ID == (accounting.OurLegalId ?? 1));

					// вставляем печать
					var stampFilename = HttpContext.Current.Server.MapPath("~\\temp\\" + ourLegalE.ID + "stamp.png");
					SaveBlob(stampFilename, ourLegalE.Sign);

					var fi = new IO.FileInfo(stampFilename);
					var pic = sheet.Drawings.AddPicture("CompanyStamp", fi);
					pic.SetPosition(410 + serviceB.Count * 20, 850);
					pic.SetSize(80);

					// вставляем первую подпись
					var signFilename = HttpContext.Current.Server.MapPath("~\\temp\\" + director.ID + "director.png");
					SaveBlob(signFilename, director.Signature);

					fi = new IO.FileInfo(signFilename);
					pic = sheet.Drawings.AddPicture("DirectorSign", fi);
					pic.SetPosition(390 + serviceB.Count * 20, 1040);
					pic.SetSize(57);// HACK:

					// вставляем вторую подпись
					if (accountant != null)
					{
						var accEmp = employeeLogic.GetEmployeesByLegal(ourLegal.ID).Where(w => w.PersonId == accountantPerson.ID).FirstOrDefault();
						if (accEmp != null)
						{
							signFilename = HttpContext.Current.Server.MapPath("~\\temp\\" + accountant.ID + "accountant.png");
							SaveBlob(signFilename, accEmp.Signature);

							fi = new IO.FileInfo(signFilename);
							pic = sheet.Drawings.AddPicture("AccountantSign", fi);
							pic.SetPosition(520 + serviceB.Count * 20, 1050);
							pic.SetSize(55);
						}
					}
				}

				xl.Save();
			}

			sw.Stop();
			Debug.WriteLine("EPP - " + sw.Elapsed);

			#endregion

			//#region process

			//var sw1 = new Stopwatch();
			//sw1.Start();

			//SpreadsheetInfo.SetLicense("FREE-LIMITED-KEY");
			//var xlf = ExcelFile.Load(filename);
			//var sheet = xlf.Worksheets[0];
			//var col = 14;
			//var row = 0;
			//sheet.Cells[row++, col].Value = accounting.InvoiceNumber;
			//sheet.Cells[row++, col].Value = FormatDate(accounting.InvoiceDate);
			//sheet.Cells[row++, col].Value = accounting.OriginalSum;
			//sheet.Cells[row++, col].Value = accounting.OriginalVat;
			//sheet.Cells[row++, col].Value = (format != TemplatedDocumentFormat.CutPdf) ? accountantPerson.DisplayName : " ";
			//sheet.Cells[row++, col].Value = (format != TemplatedDocumentFormat.CutPdf) ? directorPerson.DisplayName : " ";
			//sheet.Cells[row++, col].Value = order.Number;
			//sheet.Cells[row++, col].Value = order.RequestNumber;
			//sheet.Cells[row++, col].Value = FormatDate(order.LoadingDate);
			//sheet.Cells[row++, col].Value = (order.SeatsCount > 0) ? order.SeatsCount.Value.ToString() : " ";
			//sheet.Cells[row++, col].Value = (order.GrossWeight > 0) ? order.GrossWeight.Value.ToString("0.00") : " ";
			//sheet.Cells[row++, col].Value = order.InvoiceSum;
			//sheet.Cells[row++, col].Value = (!string.IsNullOrWhiteSpace(order.VehicleNumbers)) ? order.VehicleNumbers : " ";
			//sheet.Cells[row++, col].Value = FormatPlace(fromPoint.PlaceId.Value) + " ";
			//sheet.Cells[row++, col].Value = FormatPlace(toPoint.PlaceId.Value) + " ";
			//sheet.Cells[row++, col].Value = contract.Number;
			//sheet.Cells[row++, col].Value = FormatDate(contract.Date);
			//sheet.Cells[row++, col].Value = legal.DisplayName;
			//sheet.Cells[row++, col].Value = legal.TIN;
			//sheet.Cells[row++, col].Value = legal.KPP;
			//sheet.Cells[row++, col].Value = legal.Address;
			//sheet.Cells[row++, col].Value = legal.PostAddress;
			//sheet.Cells[row++, col].Value = ourLegal.DisplayName;
			//sheet.Cells[row++, col].Value = ourLegal.TIN;
			//sheet.Cells[row++, col].Value = ourLegal.KPP;
			//sheet.Cells[row++, col].Value = ourLegal.OGRN;
			//sheet.Cells[row++, col].Value = ourLegal.Address;
			//sheet.Cells[row++, col].Value = ourBankAccount.Number;
			//sheet.Cells[row++, col].Value = ourBank.Name;
			//sheet.Cells[row++, col].Value = ourBank.Address;
			//sheet.Cells[row++, col].Value = ourBank.BIC;
			//sheet.Cells[row++, col].Value = ourBank.KSNP;
			//sheet.Cells[row++, col].Value = currencies.First(w => w.ID == currencyId).Display;
			//sheet.Cells[row++, col].Value = (currencyId > 1) ? "Оплата по курсу " + currencyRateUses.First(w => w.ID == (contract.CurrencyRateUseId ?? 1)).Display : " ";

			//// HACK: плохой хак - сокрытие заголовков
			//if (!order.LoadingDate.HasValue)
			//	sheet.Cells["E9"].Value = "";
			//if (!order.SeatsCount.HasValue || (order.SeatsCount.Value == 0))
			//	sheet.Cells["E10"].Value = "";
			//if (!order.GrossWeight.HasValue || (order.GrossWeight.Value == 0))
			//	sheet.Cells["E11"].Value = "";
			//if (string.IsNullOrWhiteSpace(order.VehicleNumbers))
			//	sheet.Cells["E12"].Value = "";
			//if (string.IsNullOrWhiteSpace(order.From))
			//	sheet.Cells["E13"].Value = "";
			//if (string.IsNullOrWhiteSpace(order.To))
			//	sheet.Cells["E14"].Value = "";

			//// документы
			//foreach (var doc in docs)
			//{
			//	sheet.Cells[row++, col].Value = docTypes.Where(w => w.ID == doc.DocumentTypeId).Select(s => s.Display).FirstOrDefault() + " № :";
			//	sheet.Cells[row++, col].Value = doc.Number;
			//}

			//// строки
			//bool isFirst = true;
			//int srow = 19;  // 20 строка
			//var serviceB = new List<IdTuple>();
			//foreach (var service in services)
			//	if (service.ServiceTypeId.HasValue)
			//	{
			//		int serviceKindId = serviceTypes.First(w => w.ID == service.ServiceTypeId).ServiceKindId;
			//		var serviceKind = serviceKinds.First(w => w.ID == serviceKindId);
			//		// вычисление НДС
			//		double tax = 0;
			//		if (service.VatId.HasValue)
			//		{
			//			var vat = dataLogic.GetVats().First(w => w.ID == service.VatId.Value);
			//			tax = Math.Round(service.OriginalSum.Value - service.OriginalSum.Value / (100 + vat.Percent) * 100, 2);
			//		}

			//		var sbase = serviceB.FirstOrDefault(w => w.Id == serviceKindId);
			//		if (sbase == null)
			//			serviceB.Add(new IdTuple { Id = serviceKindId, Name = serviceKind.Name, Value = service.OriginalSum.Value, Value2 = tax });
			//		else
			//		{
			//			sbase.Value += service.OriginalSum.Value;
			//			sbase.Value2 += tax;
			//		}
			//	}

			//foreach (var service in serviceB)
			//{
			//	if (!isFirst)
			//	{
			//		sheet.Rows.InsertEmpty(srow);
			//		//var r1 = sheet.Cells["A" + srow + ":" + "M" + srow];
			//		//var r2 = sheet.Cells["A" + (srow - 1) + ":" + "M" + (srow - 1)];
			//		for (int i = 1; i < 14; i++)
			//		{
			//			if (sheet.Cells[srow - 1, i].MergedRange != null)
			//									sheet.Cells.GetSubrange(srow, ).Merged = true;

			//			sheet.Cells[srow, i].Style = sheet.Cells[srow - 1, i].Style;
			//			sheet.Cells[srow, i].Value = sheet.Cells[srow - 1, i].Value;
			//		}

			//		sheet.Rows[45].Height = 1;
			//	}

			//	sheet.Cells[srow, 1].Value = service.Name;
			//	sheet.Cells[srow, 10].Value = service.Value2;
			//	sheet.Cells[srow, 11].Value = service.Value;
			//	isFirst = false;
			//	srow++;
			//}

			//// вычисление значений во всех ячейках
			//for (int r = 1; r < 64; r++)
			//	for (int c = 1; c < 16; c++)
			//		sheet.Cells[r, c].Calculate();

			//if (format == TemplatedDocumentFormat.Pdf)
			//{
			//	var ourLegalE = legalLogic.GetOurLegals().First(w => w.ID == (accounting.OurLegalId ?? 1));

			//	// вставляем печать
			//	var stampFilename = HttpContext.Current.Server.MapPath("~\\temp\\stamp.png");
			//	SaveBlob(stampFilename, ourLegalE.Sign);
			//	sheet.Pictures.Add(stampFilename, 850, 408 + serviceB.Count * 20, 200, 200, LengthUnit.Pixel);

			//	// вставляем первую подпись
			//	var signFilename = HttpContext.Current.Server.MapPath("~\\temp\\director.png");
			//	SaveBlob(signFilename, director.Signature);
			//	sheet.Pictures.Add(signFilename, 1050, 390 + serviceB.Count * 20, 162, 109, LengthUnit.Pixel);

			//	// вставляем вторую подпись
			//	if ((accountant != null) && (accountant.Signature != null))
			//	{
			//		signFilename = HttpContext.Current.Server.MapPath("~\\temp\\accountant.png");
			//		SaveBlob(signFilename, accountant.Signature);
			//		sheet.Pictures.Add(signFilename, 1050, 510 + serviceB.Count * 20, 200, 91, LengthUnit.Pixel);
			//	}
			//}

			//xlf.Save(filename);

			//sw1.Stop();
			//Debug.WriteLine("sps - " + sw1.Elapsed);

			//#endregion

			var culture = Thread.CurrentThread.CurrentCulture;
			Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("ru-RU");

			SpreadsheetInfo.SetLicense("FREE-LIMITED-KEY");
			ExcelFile ef = ExcelFile.Load(filename, LoadOptions.XlsxDefault);
			var pdfFilename = HttpContext.Current.Server.MapPath("~\\Temp\\" + entity.Filename + ".pdf");
			ef.Save(pdfFilename);

			//var pdfFilename = HttpContext.Current.Server.MapPath("~\\Temp\\" + entity.Filename + ".pdf");
			//xlf.Save(pdfFilename);
			Thread.CurrentThread.CurrentCulture = culture;

			switch (format)
			{
				case TemplatedDocumentFormat.Pdf:
					UploadTemplatedDocumentData(entity.ID, filename, "xlsx");
					UploadTemplatedDocumentData(entity.ID, pdfFilename, "pdf");
					break;
				case TemplatedDocumentFormat.CleanPdf:
					UploadTemplatedDocumentData(entity.ID, pdfFilename, "cleanpdf");
					break;
				case TemplatedDocumentFormat.CutPdf:
					UploadTemplatedDocumentData(entity.ID, pdfFilename, "cutpdf");
					break;
			}

			return string.Empty;
		}

		public string GenerateEnInvoice(TemplatedDocument entity, TemplatedDocumentFormat format)
		{
			#region подготовка данных и проверки

			var accounting = accountingLogic.GetAccounting(entity.AccountingId.Value);
			if (!accounting.LegalId.HasValue)
				return "Не заполнено Юрлицо в доходе";

			var services = accountingLogic.GetServicesByAccounting(accounting.ID);
			if (services.Count() == 0)
				return "Нет услуг";

			var order = orderLogic.GetOrder(accounting.OrderId);
			var contract = contractLogic.GetContract(order.ContractId.Value);
			var legal = legalLogic.GetLegal(accounting.LegalId.Value);
			var ourLegalE = legalLogic.GetOurLegals().First(w => w.ID == accounting.OurLegalId.Value);
			var ourLegal = legalLogic.GetLegal(ourLegalE.LegalId.Value);

			var director = employeeLogic.GetEmployee(ourLegal.DirectorId.Value);
			var directorPerson = personLogic.GetPerson(director.PersonId.Value);
			var accountant = participantLogic.GetWorkgroupByOrderAtDate(order.ID, accounting.InvoiceDate.Value).Where(w => w.ParticipantRoleId == (int)ParticipantRoles.BUH).FirstOrDefault();
			var accountantPerson = personLogic.GetPersonByUser(accountant.UserId.Value);

			var docTypes = dataLogic.GetDocumentTypes();
			var serviceTypes = dataLogic.GetServiceTypes();
			var serviceKinds = dataLogic.GetServiceKinds();
			var currencies = dataLogic.GetCurrencies();
			var docs = documentLogic.GetDocumentsByOrder(order.ID).Where(w => w.IsPrint == true);

			var currencyId = services.First().CurrencyId ?? 1;
			var contractCurrencies = contractLogic.GetContractCurrencies(contract.ID);
			var currency = contractCurrencies.FirstOrDefault(w => w.CurrencyId == currencyId);
			if (currency == null)
				currency = contractCurrencies.FirstOrDefault(w => w.CurrencyId == 1);   // рубли присутствуют обязательно в этом случае

			RoutePoint fromPoint = null;
			RoutePoint toPoint = null;
			var segments = accountingLogic.GetAccountingRouteSegments(accounting.ID);
			if (segments.Count() != 0)
			{
				var fromSegment = orderLogic.GetRouteSegment(segments.First().RouteSegmentId.Value);
				fromPoint = orderLogic.GetRoutePoint(fromSegment.FromRoutePointId);
				var toSegment = orderLogic.GetRouteSegment(segments.Last().RouteSegmentId.Value);
				toPoint = orderLogic.GetRoutePoint(toSegment.ToRoutePointId);
			}
			else
			{
				var points = orderLogic.GetRoutePoints(order.ID);
				fromPoint = points.FirstOrDefault();
				toPoint = points.LastOrDefault();
			}

			var ourBankAccount = bankLogic.GetBankAccount(currency.OurBankAccountId);
			var ourBank = bankLogic.GetBank(ourBankAccount.BankId.Value);

			#endregion

			var filename = HttpContext.Current.Server.MapPath("~\\Temp\\" + entity.Filename + ".xlsx");

			DownloadTemplateData(entity.TemplateId.Value, filename);

			#region process

			using (var xl = new ExcelPackage(new IO.FileInfo(filename)))
			{
				xl.DoAdjustDrawings = true;
				xl.Workbook.CalcMode = ExcelCalcMode.Automatic;

				var sheet = xl.Workbook.Worksheets[1];
				var col = 15;
				var row = 1;
				sheet.Cells[row++, col].Value = accounting.InvoiceNumber;
				sheet.Cells[row++, col].Value = FormatDate(accounting.InvoiceDate);
				sheet.Cells[row++, col].Value = accounting.OriginalSum;
				sheet.Cells[row++, col].Value = accounting.OriginalVat;
				sheet.Cells[row++, col].Value = (format != TemplatedDocumentFormat.CutPdf) ? accountantPerson.EnName : " ";
				sheet.Cells[row++, col].Value = (format != TemplatedDocumentFormat.CutPdf) ? directorPerson.EnName : " ";
				sheet.Cells[row++, col].Value = order.Number;
				sheet.Cells[row++, col].Value = order.RequestNumber;
				sheet.Cells[row++, col].Value = FormatDate(order.LoadingDate);
				sheet.Cells[row++, col].Value = order.SeatsCount.HasValue ? order.SeatsCount.Value.ToString() : " ";
				sheet.Cells[row++, col].Value = (order.GrossWeight > 0) ? order.GrossWeight.Value.ToString("0.00") : " ";
				sheet.Cells[row++, col].Value = order.InvoiceSum;
				sheet.Cells[row++, col].Value = order.VehicleNumbers;
				sheet.Cells[row++, col].Value = (fromPoint != null) ? FormatEnPlace(fromPoint.PlaceId.Value) + " " : " ";
				sheet.Cells[row++, col].Value = (toPoint != null) ? FormatEnPlace(toPoint.PlaceId.Value) + " " : " ";
				sheet.Cells[row++, col].Value = contract.Number;
				sheet.Cells[row++, col].Value = FormatDate(contract.Date);
				sheet.Cells[row++, col].Value = legal.EnName;
				sheet.Cells[row++, col].Value = legal.TIN;
				sheet.Cells[row++, col].Value = legal.KPP;
				sheet.Cells[row++, col].Value = legal.EnAddress;
				sheet.Cells[row++, col].Value = legal.EnPostAddress;
				sheet.Cells[row++, col].Value = ourLegal.EnShortName;
				sheet.Cells[row++, col].Value = ourLegal.TIN;
				sheet.Cells[row++, col].Value = ourLegal.KPP;
				sheet.Cells[row++, col].Value = ourLegal.OGRN;
				sheet.Cells[row++, col].Value = ourLegal.EnAddress;
				sheet.Cells[row++, col].Value = ourBankAccount.Number;
				sheet.Cells[row++, col].Value = ourBankAccount.CoBankAccount;
				sheet.Cells[row++, col].Value = ourBankAccount.CoBankName;
				sheet.Cells[row++, col].Value = ourBankAccount.CoBankSWIFT;
				sheet.Cells[row++, col].Value = ourBank.Name;
				sheet.Cells[row++, col].Value = ourBank.BIC;
				sheet.Cells[row++, col].Value = ourBank.KSNP;
				sheet.Cells[row++, col].Value = ourBank.EnName;
				sheet.Cells[row++, col].Value = ourBank.SWIFT;
				sheet.Cells[row++, col].Value = currencies.First(w => w.ID == currencyId).Display;
				sheet.Cells[row++, col].Value = "";

				// HACK: плохой хак
				if (string.IsNullOrWhiteSpace(legal.TIN))
					sheet.Cells["B10"].Value = "";
				if (string.IsNullOrWhiteSpace(legal.KPP))
					sheet.Cells["B11"].Value = "";
				if (string.IsNullOrWhiteSpace(order.From))
					sheet.Cells["E13"].Value = "";
				if (string.IsNullOrWhiteSpace(order.To))
					sheet.Cells["E14"].Value = "";
				if (string.IsNullOrWhiteSpace(order.VehicleNumbers))
					sheet.Cells["E12"].Value = "";
				if (!order.SeatsCount.HasValue || (order.SeatsCount.Value == 0))
					sheet.Cells["E10"].Value = "";
				if (!order.GrossWeight.HasValue || (order.GrossWeight.Value == 0))
					sheet.Cells["E11"].Value = "";
				if (!order.LoadingDate.HasValue)
					sheet.Cells["E9"].Value = "";

				// документы
				foreach (var doc in docs)
				{
					sheet.Cells[row++, col].Value = docTypes.Where(w => w.ID == doc.DocumentTypeId).Select(s => s.EnDescription).FirstOrDefault() + " № :";
					sheet.Cells[row++, col].Value = doc.Number;
				}

				var pricelist = GetPricelist(order) ?? new Pricelist(); // HACK:
				var priceKinds = pricelistLogic.GetPriceKinds(pricelist.ID);
				// строки
				bool isFirst = true;
				int srow = 20;  // 20 строка
				var serviceB = new List<IdTuple>();
				foreach (var service in services)
				{
					if (service.ServiceTypeId.HasValue)
					{
						int serviceKindId = serviceTypes.First(w => w.ID == service.ServiceTypeId).ServiceKindId;
						var resultName = serviceKinds.First(w => w.ID == serviceKindId).EnName;
						if (pricelist.ID > 0)
							resultName = priceKinds.First(w => w.ServiceKindId == serviceKindId).EnName;

						// вычисление НДС
						double tax = 0;
						if (service.VatId.HasValue)
						{
							var vat = dataLogic.GetVats().First(w => w.ID == service.VatId.Value);
							tax = service.OriginalSum.Value - service.OriginalSum.Value / (100 + vat.Percent) * 100;
						}

						var sbase = serviceB.FirstOrDefault(w => w.Id == serviceKindId);
						if (sbase == null)
							serviceB.Add(new IdTuple { Id = serviceKindId, EnName = resultName, Value = service.OriginalSum.Value, Value2 = tax });
						else
						{
							sbase.Value += service.OriginalSum.Value;
							sbase.Value2 += tax;
						}
					}
				}

				foreach (var service in serviceB)
				{
					if (!isFirst)
					{
						sheet.InsertRow(srow, 1);
						var r1 = sheet.Cells["A" + srow + ":" + "M" + srow];
						var r2 = sheet.Cells["A" + (srow - 1) + ":" + "M" + (srow - 1)];
						r2.Copy(r1);
						sheet.Row(46).Height = 1;
					}

					sheet.Cells[srow, 2].Value = service.EnName;
					sheet.Cells[srow, 11].Value = Math.Round(service.Value2, 2);
					sheet.Cells[srow, 12].Value = Math.Round(service.Value, 2);
					isFirst = false;
					srow++;
				}

				// вычисление значений во всех ячейках
				var allCells = sheet.Cells;
				foreach (var item in allCells)
					item.Calculate();

				if (format == TemplatedDocumentFormat.Pdf)
				{
					// вставляем печать
					var stampFilename = HttpContext.Current.Server.MapPath("~\\temp\\" + ourLegalE.ID + "stamp.png");
					SaveBlob(stampFilename, ourLegalE.Sign);

					var fi = new IO.FileInfo(stampFilename);
					var pic = sheet.Drawings.AddPicture("CompanyStamp", fi);
					pic.SetPosition(410 + serviceB.Count * 20, 850);
					pic.SetSize(80);

					// вставляем первую подпись
					var signFilename = HttpContext.Current.Server.MapPath("~\\temp\\" + director.ID + "director.png");
					SaveBlob(signFilename, director.Signature);

					fi = new IO.FileInfo(signFilename);
					pic = sheet.Drawings.AddPicture("DirectorSign", fi);
					pic.SetPosition(390 + serviceB.Count * 20, 1000);
					pic.SetSize(57);

					// вставляем вторую подпись
					if (accountant != null)
					{
						var accEmp = employeeLogic.GetEmployeesByLegal(ourLegal.ID).Where(w => w.PersonId == accountantPerson.ID).FirstOrDefault();
						if (accEmp != null)
						{
							signFilename = HttpContext.Current.Server.MapPath("~\\temp\\" + accountant.ID + "accountant.png");
							SaveBlob(signFilename, accEmp.Signature);

							fi = new IO.FileInfo(signFilename);
							pic = sheet.Drawings.AddPicture("AccountantSign", fi);
							pic.SetPosition(520 + serviceB.Count * 20, 1050);
							pic.SetSize(55);
						}
					}
				}

				xl.Save();
			}

			#endregion

			try
			{
				SpreadsheetInfo.SetLicense("FREE-LIMITED-KEY");
				var culture = Thread.CurrentThread.CurrentCulture;
				Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("ru-RU");
				ExcelFile ef = ExcelFile.Load(filename, LoadOptions.XlsxDefault);
				var pdfFilename = HttpContext.Current.Server.MapPath("~\\Temp\\" + entity.Filename + ".pdf");
				ef.Save(pdfFilename);
				Thread.CurrentThread.CurrentCulture = culture;

				switch (format)
				{
					case TemplatedDocumentFormat.Pdf:
						UploadTemplatedDocumentData(entity.ID, filename, "xlsx");
						UploadTemplatedDocumentData(entity.ID, pdfFilename, "pdf");
						break;
					case TemplatedDocumentFormat.CleanPdf:
						UploadTemplatedDocumentData(entity.ID, pdfFilename, "cleanpdf");
						break;
					case TemplatedDocumentFormat.CutPdf:
						UploadTemplatedDocumentData(entity.ID, pdfFilename, "cutpdf");
						break;
				}
			}
			catch { }

			return string.Empty;
		}

		public string GenerateNonresidentInvoice(TemplatedDocument entity, TemplatedDocumentFormat format)
		{
			var accounting = accountingLogic.GetAccounting(entity.AccountingId.Value);
			if (!accounting.LegalId.HasValue)
				return "Не заполнено Юрлицо в доходе";

			var order = orderLogic.GetOrder(accounting.OrderId);
			var contract = contractLogic.GetContract(accounting.IsIncome ? order.ContractId.Value : accounting.ContractId.Value);
			var legal = legalLogic.GetLegal(accounting.LegalId.Value);
			var ourLegal = legalLogic.GetLegalByOurLegal(accounting.OurLegalId.Value);
			var director = employeeLogic.GetEmployee(ourLegal.DirectorId.Value);
			var directorPerson = personLogic.GetPerson(director.PersonId.Value);
			var accountant = employeeLogic.GetEmployee(ourLegal.AccountantId.Value);
			var accountantPerson = personLogic.GetPerson(accountant.PersonId.Value);
			var docs = documentLogic.GetDocumentsByOrder(order.ID).Where(w => w.IsPrint == true);
			var services = accountingLogic.GetServicesByAccounting(accounting.ID);
			if (services.Count() == 0)
				return "Нет услуг";

			var currencyId = services.First().CurrencyId ?? 1;
			var contractCurrencies = contractLogic.GetContractCurrencies(contract.ID);
			var currency = contractCurrencies.FirstOrDefault(w => w.CurrencyId == currencyId);
			if (currency == null)
			{
				if (ourLegal.IsNotResident)
					return "Счет нашего нерезидента можно выставить только в валюте договора.";
				else
					currency = contractCurrencies.FirstOrDefault(w => w.CurrencyId == 1);   // рубли присутствуют обязательно в этом случае
			}

			RoutePoint fromPoint = null;
			RoutePoint toPoint = null;
			var segments = accountingLogic.GetAccountingRouteSegments(accounting.ID);
			if (segments.Count() != 0)
			{
				var fromSegment = orderLogic.GetRouteSegment(segments.First().RouteSegmentId.Value);
				fromPoint = orderLogic.GetRoutePoint(fromSegment.FromRoutePointId);
				var toSegment = orderLogic.GetRouteSegment(segments.Last().RouteSegmentId.Value);
				toPoint = orderLogic.GetRoutePoint(toSegment.ToRoutePointId);
			}
			else
			{
				var points = orderLogic.GetRoutePoints(order.ID);
				fromPoint = points.FirstOrDefault();
				toPoint = points.LastOrDefault();
			}

			var ourBankAccount = bankLogic.GetBankAccount(currency.OurBankAccountId);
			var ourBank = bankLogic.GetBank(ourBankAccount.BankId ?? 0);
			// справочники
			var docTypes = dataLogic.GetDocumentTypes();
			var serviceTypes = dataLogic.GetServiceTypes();
			var serviceKinds = dataLogic.GetServiceKinds();
			var currencies = dataLogic.GetCurrencies();
			var currencyRateUses = dataLogic.GetCurrencyRateUses();
			var filename = HttpContext.Current.Server.MapPath("~\\Temp\\" + entity.Filename + ".xlsx");
			DownloadTemplateData(entity.TemplateId.Value, filename);

			#region process

			using (var xl = new ExcelPackage(new IO.FileInfo(filename)))
			{
				xl.DoAdjustDrawings = true;
				xl.Workbook.CalcMode = ExcelCalcMode.Automatic;

				var sheet = xl.Workbook.Worksheets[1];
				var col = 15;
				var row = 1;
				sheet.Cells[row++, col].Value = accounting.InvoiceNumber;
				sheet.Cells[row++, col].Value = FormatDate(accounting.InvoiceDate);
				sheet.Cells[row++, col].Value = accounting.OriginalSum;
				sheet.Cells[row++, col].Value = accounting.OriginalVat;
				sheet.Cells[row++, col].Value = (format != TemplatedDocumentFormat.CutPdf) ? accountantPerson.DisplayName ?? accountantPerson.EnName : " ";
				sheet.Cells[row++, col].Value = (format != TemplatedDocumentFormat.CutPdf) ? directorPerson.DisplayName ?? directorPerson.EnName : " ";
				sheet.Cells[row++, col].Value = order.Number;
				sheet.Cells[row++, col].Value = order.RequestNumber;
				sheet.Cells[row++, col].Value = FormatDate(order.LoadingDate);
				sheet.Cells[row++, col].Value = order.SeatsCount.HasValue ? order.SeatsCount.Value.ToString() : " ";
				sheet.Cells[row++, col].Value = order.GrossWeight.HasValue ? order.GrossWeight.Value.ToString("0.00") : " ";
				sheet.Cells[row++, col].Value = order.InvoiceSum;
				sheet.Cells[row++, col].Value = order.VehicleNumbers;
				sheet.Cells[row++, col].Value = order.From;
				sheet.Cells[row++, col].Value = order.To;
				sheet.Cells[row++, col].Value = contract.Number;
				sheet.Cells[row++, col].Value = FormatDate(contract.Date);
				sheet.Cells[row++, col].Value = legal.Name;
				sheet.Cells[row++, col].Value = legal.TIN;
				sheet.Cells[row++, col].Value = legal.KPP;
				sheet.Cells[row++, col].Value = legal.Address;
				sheet.Cells[row++, col].Value = legal.PostAddress;
				sheet.Cells[row++, col].Value = ourLegal.Name;
				sheet.Cells[row++, col].Value = ourLegal.TIN + " ";
				sheet.Cells[row++, col].Value = ourLegal.KPP + " ";
				sheet.Cells[row++, col].Value = ourLegal.OGRN + " ";
				sheet.Cells[row++, col].Value = ourLegal.Address;
				sheet.Cells[row++, col].Value = ourBankAccount.Number;
				sheet.Cells[row++, col].Value = (ourBank != null) ? ourBank.Name : ourBankAccount.CoBankName;
				sheet.Cells[row++, col].Value = (ourBank != null) ? ourBank.BIC : " ";
				sheet.Cells[row++, col].Value = (ourBank != null) ? ourBank.KSNP : " ";
				sheet.Cells[row++, col].Value = currencies.First(w => w.ID == currencyId).Display;
				sheet.Cells[row++, col].Value = (currencyId > 1) ? "Оплата по курсу " + currencyRateUses.First(w => w.ID == (contract.CurrencyRateUseId ?? 1)).Display : " ";

				// документы
				foreach (var doc in docs)
				{
					sheet.Cells[row, col].Value = docTypes.Where(w => w.ID == doc.DocumentTypeId).Select(s => s.EnDescription).FirstOrDefault() + " № :";
					sheet.Cells[row++, col + 1].Value = doc.Number;
				}

				var pricelist = GetPricelist(order) ?? new Pricelist(); // HACK:
				var priceKinds = pricelistLogic.GetPriceKinds(pricelist.ID);
				// строки
				bool isFirst = true;
				int srow = 20;
				var serviceB = new List<IdTuple>();
				foreach (var service in services)
				{
					if (service.ServiceTypeId.HasValue)
					{
						int serviceKindId = serviceTypes.First(w => w.ID == service.ServiceTypeId).ServiceKindId;
						var resultName = serviceKinds.First(w => w.ID == serviceKindId).Name;
						if (pricelist.ID > 0)
							resultName = priceKinds.First(w => w.ServiceKindId == serviceKindId).Name;

						// вычисление НДС
						double tax = 0;
						if (service.VatId.HasValue)
						{
							var vat = dataLogic.GetVats().First(w => w.ID == service.VatId.Value);
							tax = service.OriginalSum.Value - service.OriginalSum.Value / (100 + vat.Percent) * 100;
						}

						var sbase = serviceB.FirstOrDefault(w => w.Id == serviceKindId);
						if (sbase == null)
							serviceB.Add(new IdTuple { Id = serviceKindId, Name = resultName, Value = service.OriginalSum.Value, Value2 = tax });
						else
						{
							sbase.Value += service.OriginalSum.Value;
							sbase.Value2 += tax;
						}
					}
				}

				foreach (var service in serviceB)
				{
					if (!isFirst)
					{
						sheet.InsertRow(srow, 1);
						var r1 = sheet.Cells["A" + srow + ":" + "M" + srow];
						var r2 = sheet.Cells["A" + (srow - 1) + ":" + "M" + (srow - 1)];
						r2.Copy(r1);
						sheet.Row(46).Height = 1;
					}

					sheet.Cells[srow, 2].Value = service.Name;
					sheet.Cells[srow, 11].Value = Math.Round(service.Value2, 2);
					sheet.Cells[srow, 12].Value = Math.Round(service.Value, 2);
					isFirst = false;
					srow++;
				}

				// вычисление значений во всех ячейках
				var allCells = sheet.Cells;
				foreach (var item in allCells)
					item.Calculate();

				if (format == TemplatedDocumentFormat.Pdf)
				{
					var ourLegalE = legalLogic.GetOurLegal(accounting.OurLegalId.Value);

					// вставляем печать
					var stampFilename = HttpContext.Current.Server.MapPath("~\\temp\\" + ourLegalE.ID + "stamp.png");
					SaveBlob(stampFilename, ourLegalE.Sign);

					var fi = new IO.FileInfo(stampFilename);
					var pic = sheet.Drawings.AddPicture("CompanyStamp", fi);
					pic.SetPosition(410 + serviceB.Count * 20, 850);
					pic.SetSize(80);// HACK:

					// вставляем первую подпись
					var signFilename = HttpContext.Current.Server.MapPath("~\\temp\\" + director.ID + "director.png");
					SaveBlob(signFilename, director.Signature);

					fi = new IO.FileInfo(signFilename);
					pic = sheet.Drawings.AddPicture("DirectorSign", fi);
					pic.SetPosition(390 + serviceB.Count * 20, 1000);
					pic.SetSize(57);// HACK:

					// вставляем вторую подпись
					if ((accountant != null) && (accountant.Signature != null))
					{
						signFilename = HttpContext.Current.Server.MapPath("~\\temp\\" + accountant.ID + "accountant.png");
						SaveBlob(signFilename, accountant.Signature);

						fi = new IO.FileInfo(signFilename);
						pic = sheet.Drawings.AddPicture("AccountantSign", fi);
						pic.SetPosition(520 + serviceB.Count * 20, 1050);
						pic.SetSize(50);// HACK:
					}
				}

				xl.Save();
			}

			#endregion

			try
			{
				SpreadsheetInfo.SetLicense("FREE-LIMITED-KEY");
				var culture = Thread.CurrentThread.CurrentCulture;
				Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("ru-RU");
				ExcelFile ef = ExcelFile.Load(filename, LoadOptions.XlsxDefault);
				var pdfFilename = HttpContext.Current.Server.MapPath("~\\Temp\\" + entity.Filename + ".pdf");
				ef.Save(pdfFilename);
				Thread.CurrentThread.CurrentCulture = culture;

				switch (format)
				{
					case TemplatedDocumentFormat.Pdf:
						UploadTemplatedDocumentData(entity.ID, filename, "xlsx");
						UploadTemplatedDocumentData(entity.ID, pdfFilename, "pdf");
						break;
					case TemplatedDocumentFormat.CleanPdf:
						UploadTemplatedDocumentData(entity.ID, pdfFilename, "cleanpdf");
						break;
					case TemplatedDocumentFormat.CutPdf:
						UploadTemplatedDocumentData(entity.ID, pdfFilename, "cutpdf");
						break;
				}
			}
			catch { }

			return string.Empty;
		}

		public string GenerateNonresidentEnInvoice(TemplatedDocument entity, TemplatedDocumentFormat format)
		{
			var accounting = accountingLogic.GetAccounting(entity.AccountingId.Value);
			if (!accounting.LegalId.HasValue)
				return "Не заполнено Юрлицо в доходе";

			var order = orderLogic.GetOrder(accounting.OrderId);
			var contract = contractLogic.GetContract(accounting.IsIncome ? order.ContractId.Value : accounting.ContractId.Value);
			var legal = legalLogic.GetLegal(accounting.LegalId.Value);
			var ourLegal = legalLogic.GetLegalByOurLegal(accounting.OurLegalId.Value);
			var director = employeeLogic.GetEmployee(ourLegal.DirectorId.Value);
			var directorPerson = personLogic.GetPerson(director.PersonId.Value);
			var accountant = employeeLogic.GetEmployee(ourLegal.AccountantId.Value);
			var accountantPerson = personLogic.GetPerson(accountant.PersonId.Value);
			var docs = documentLogic.GetDocumentsByOrder(order.ID).Where(w => w.IsPrint == true);
			var services = accountingLogic.GetServicesByAccounting(accounting.ID);
			if (services.Count() == 0)
				return "Нет услуг";

			var currencyId = services.First().CurrencyId ?? 1;
			var contractCurrencies = contractLogic.GetContractCurrencies(contract.ID);
			var currency = contractCurrencies.FirstOrDefault(w => w.CurrencyId == currencyId);
			if (currency == null)
			{
				if (ourLegal.IsNotResident)
					return "Счет нашего нерезидента можно выставить только в валюте договора.";
				else
					currency = contractCurrencies.FirstOrDefault(w => w.CurrencyId == 1);   // рубли присутствуют обязательно в этом случае
			}

			RoutePoint fromPoint = null;
			RoutePoint toPoint = null;
			var segments = accountingLogic.GetAccountingRouteSegments(accounting.ID);
			if (segments.Count() != 0)
			{
				var fromSegment = orderLogic.GetRouteSegment(segments.First().RouteSegmentId.Value);
				fromPoint = orderLogic.GetRoutePoint(fromSegment.FromRoutePointId);
				var toSegment = orderLogic.GetRouteSegment(segments.Last().RouteSegmentId.Value);
				toPoint = orderLogic.GetRoutePoint(toSegment.ToRoutePointId);
			}
			else
			{
				var points = orderLogic.GetRoutePoints(order.ID);
				fromPoint = points.FirstOrDefault();
				toPoint = points.LastOrDefault();
			}

			var ourBankAccount = bankLogic.GetBankAccount(currency.OurBankAccountId);
			var ourBank = bankLogic.GetBank(ourBankAccount.BankId ?? 0);
			// справочники
			var docTypes = dataLogic.GetDocumentTypes();
			var serviceTypes = dataLogic.GetServiceTypes();
			var serviceKinds = dataLogic.GetServiceKinds();
			var currencies = dataLogic.GetCurrencies();
			var filename = HttpContext.Current.Server.MapPath("~\\Temp\\" + entity.Filename + ".xlsx");
			DownloadTemplateData(entity.TemplateId.Value, filename);

			#region process

			using (var xl = new ExcelPackage(new IO.FileInfo(filename)))
			{
				xl.DoAdjustDrawings = true;
				xl.Workbook.CalcMode = ExcelCalcMode.Automatic;

				var sheet = xl.Workbook.Worksheets[1];
				var col = 20;
				var row = 2;
				sheet.Cells[row++, col].Value = ourLegal.EnShortName;
				sheet.Cells[row++, col].Value = ourLegal.EnAddress;
				sheet.Cells[row++, col].Value = ourLegal.TIN;
				sheet.Cells[row++, col].Value = accounting.InvoiceNumber;
				sheet.Cells[row++, col].Value = FormatDate(accounting.InvoiceDate);
				sheet.Cells[row++, col].Value = legal.EnName;
				sheet.Cells[row++, col].Value = legal.EnAddress;
				sheet.Cells[row++, col].Value = legal.TIN;
				sheet.Cells[row++, col].Value = contract.Number;
				sheet.Cells[row++, col].Value = FormatDate(contract.Date);
				sheet.Cells[row++, col].Value = order.RequestNumber;
				sheet.Cells[row++, col].Value = FormatDate(order.RequestDate);
				sheet.Cells[row++, col].Value = order.GrossWeight.HasValue ? order.GrossWeight.Value.ToString("0.00") : " ";
				sheet.Cells[row++, col].Value = order.Volume.HasValue ? order.Volume.Value.ToString("0.00") : " ";
				sheet.Cells[row++, col].Value = (fromPoint != null) ? FormatEnPlace(fromPoint.PlaceId.Value) + " " : " ";
				sheet.Cells[row++, col].Value = (toPoint != null) ? FormatEnPlace(toPoint.PlaceId.Value) + " " : " ";
				sheet.Cells[row++, col].Value = currencies.First(w => w.ID == currencyId).Display;
				sheet.Cells[row++, col].Value = accounting.OriginalSum;
				sheet.Cells[row++, col].Value = accounting.OriginalVat;

				row = 31;
				sheet.Cells[row++, col].Value = (ourBank != null) ? ourBank.EnName : ourBankAccount.CoBankName;
				sheet.Cells[row++, col].Value = (ourBank != null) ? ourBank.EnAddress : ourBankAccount.CoBankAddress;
				sheet.Cells[row++, col].Value = (ourBank != null) ? ourBank.SWIFT : ourBankAccount.CoBankSWIFT;
				sheet.Cells[row++, col].Value = ourBankAccount.Number;
				sheet.Cells[row++, col].Value = ourBankAccount.CoBankAccount;
				sheet.Cells[row++, col].Value = ourBankAccount.CoBankName;
				sheet.Cells[row++, col].Value = ourBankAccount.CoBankSWIFT;
				sheet.Cells[row++, col].Value = (ourBank != null) ? ourBank.Name : ourBankAccount.CoBankName;
				sheet.Cells[row++, col].Value = (ourBank != null) ? ourBank.BIC : "";
				sheet.Cells[row++, col].Value = (ourBank != null) ? ourBank.KSNP : "";
				sheet.Cells[row++, col].Value = accounting.OriginalSum;
				sheet.Cells[row++, col].Value = accounting.OriginalVat;
				sheet.Cells[row++, col].Value = (format != TemplatedDocumentFormat.CutPdf) ? accountantPerson.DisplayName ?? accountantPerson.EnName : " ";
				sheet.Cells[row++, col].Value = (format != TemplatedDocumentFormat.CutPdf) ? directorPerson.DisplayName ?? directorPerson.EnName : " ";
				sheet.Cells[row++, col].Value = order.SeatsCount.HasValue ? order.SeatsCount.Value.ToString() : " ";
				sheet.Cells[row++, col].Value = order.InvoiceSum;
				sheet.Cells[row++, col].Value = order.VehicleNumbers;
				sheet.Cells[row++, col].Value = legal.KPP;
				sheet.Cells[row++, col].Value = legal.EnPostAddress;
				sheet.Cells[row++, col].Value = ourLegal.KPP;
				sheet.Cells[row++, col].Value = ourLegal.OGRN;

				row = 20;
				// документы
				foreach (var doc in docs)
				{
					sheet.Cells[row, col].Value = docTypes.Where(w => w.ID == doc.DocumentTypeId).Select(s => s.EnDescription).FirstOrDefault() + " № :";
					sheet.Cells[row++, col + 1].Value = doc.Number;
				}

				var pricelist = GetPricelist(order) ?? new Pricelist(); // HACK:
				var priceKinds = pricelistLogic.GetPriceKinds(pricelist.ID);
				// строки
				bool isFirst = true;
				int srow = 36;
				var serviceB = new List<IdTuple>();
				foreach (var service in services)
				{
					if (service.ServiceTypeId.HasValue)
					{
						int serviceKindId = serviceTypes.First(w => w.ID == service.ServiceTypeId).ServiceKindId;
						var resultName = serviceKinds.First(w => w.ID == serviceKindId).EnName;
						if (pricelist.ID > 0)
							resultName = priceKinds.First(w => w.ServiceKindId == serviceKindId).EnName;

						// вычисление НДС
						double tax = 0;
						if (service.VatId.HasValue)
						{
							var vat = dataLogic.GetVats().First(w => w.ID == service.VatId.Value);
							tax = service.OriginalSum.Value - service.OriginalSum.Value / (100 + vat.Percent) * 100;
						}

						var sbase = serviceB.FirstOrDefault(w => w.Id == serviceKindId);
						if (sbase == null)
							serviceB.Add(new IdTuple { Id = serviceKindId, EnName = resultName, Value = service.OriginalSum.Value, Value2 = tax });
						else
						{
							sbase.Value += service.OriginalSum.Value;
							sbase.Value2 += tax;
						}
					}
				}

				foreach (var service in serviceB)
				{
					if (!isFirst)
					{
						sheet.InsertRow(srow, 1);
						var r1 = sheet.Cells["A" + srow + ":" + "R" + srow];
						var r2 = sheet.Cells["A" + (srow - 1) + ":" + "R" + (srow - 1)];
						r2.Copy(r1);
						sheet.Row(54).Height = 1;
					}

					sheet.Cells[srow, 2].Value = service.EnName;
					sheet.Cells[srow, 14].Value = Math.Round(service.Value2, 2);
					sheet.Cells[srow, 16].Value = Math.Round(service.Value, 2);
					isFirst = false;
					srow++;
				}

				// вычисление значений во всех ячейках
				var allCells = sheet.Cells;
				foreach (var item in allCells)
					item.Calculate();

				if (format == TemplatedDocumentFormat.Pdf)
				{
					var ourLegalE = legalLogic.GetOurLegal(accounting.OurLegalId.Value);
					// вставляем печать
					var stampFilename = HttpContext.Current.Server.MapPath("~\\temp\\" + ourLegalE.ID + "stamp.png");
					SaveBlob(stampFilename, ourLegalE.Sign);

					var fi = new IO.FileInfo(stampFilename);
					var pic = sheet.Drawings.AddPicture("CompanyStamp", fi);
					pic.SetPosition(1450 + serviceB.Count * 20, 780);
					pic.SetSize(90);// HACK:

					// вставляем первую подпись
					var signFilename = HttpContext.Current.Server.MapPath("~\\temp\\" + director.ID + "director.png");
					SaveBlob(signFilename, director.Signature);

					fi = new IO.FileInfo(signFilename);
					pic = sheet.Drawings.AddPicture("DirectorSign", fi);
					pic.SetPosition(1500 + serviceB.Count * 20, 900);
					pic.SetSize(57);// HACK:
				}

				xl.Save();
			}

			#endregion

			try
			{
				SpreadsheetInfo.SetLicense("FREE-LIMITED-KEY");
				var culture = Thread.CurrentThread.CurrentCulture;
				Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("ru-RU");
				ExcelFile ef = ExcelFile.Load(filename, LoadOptions.XlsxDefault);
				var pdfFilename = HttpContext.Current.Server.MapPath("~\\Temp\\" + entity.Filename + ".pdf");
				ef.Save(pdfFilename);
				Thread.CurrentThread.CurrentCulture = culture;

				switch (format)
				{
					case TemplatedDocumentFormat.Pdf:
						UploadTemplatedDocumentData(entity.ID, filename, "xlsx");
						UploadTemplatedDocumentData(entity.ID, pdfFilename, "pdf");
						break;
					case TemplatedDocumentFormat.CleanPdf:
						UploadTemplatedDocumentData(entity.ID, pdfFilename, "cleanpdf");
						break;
					case TemplatedDocumentFormat.CutPdf:
						UploadTemplatedDocumentData(entity.ID, pdfFilename, "cutpdf");
						break;
				}
			}
			catch { }

			return string.Empty;
		}

		//public string GenerateVatInvoice(TemplatedDocument entity, TemplatedDocumentFormat format)
		//{
		//	var accounting = accountingLogic.GetAccounting(entity.OrderAccountingId.Value);
		//	var order = orderLogic.GetOrder(accounting.OrderId);
		//	var contract = contractLogic.GetContract(order.ContractId.Value);
		//	var legal = legalLogic.GetLegal(accounting.LegalId ?? contract.LegalId);
		//	var ourLegalE = legalLogic.GetOurLegals().First(w => w.ID == (accounting.OurLegalId ?? contract.OurLegalId));
		//	var ourLegal = legalLogic.GetLegal(ourLegalE.LegalId.Value);
		//	var director = employeeLogic.GetEmployee(ourLegal.DirectorId.Value);
		//	var directorPerson = personLogic.GetPerson(director.PersonId.Value);
		//	var accountant = employeeLogic.GetEmployee(ourLegal.AccountantId.Value);
		//	var accountantPerson = personLogic.GetPerson(accountant.PersonId.Value);
		//	var serviceTypes = dataLogic.GetServiceTypes();
		//	var serviceKinds = dataLogic.GetServiceKinds();
		//	var services = accountingLogic.GetServicesByAccounting(accounting.ID);
		//	var contractCurrencies = contractLogic.GetContractCurrencies(contract.ID);
		//	var currencies = dataLogic.GetCurrencies();
		//	var currency = contractCurrencies.FirstOrDefault(w => w.CurrencyId == services.First().CurrencyId);
		//	if (currency == null)
		//		currency = contractCurrencies.FirstOrDefault(w => w.CurrencyId == 1);   // рубли присутствуют обязательно в этом случае

		//	var ourBankAccount = bankLogic.GetBankAccount(currency.OurBankAccountId);
		//	var ourBank = bankLogic.GetBank(ourBankAccount.BankId.Value);

		//	var filename = HttpContext.Current.Server.MapPath("~\\Temp\\" + entity.Filename + ".xlsx");

		//	DownloadTemplateData(entity.TemplateId.Value, filename);

		//	#region process

		//	using (var xl = new ExcelPackage(new IO.FileInfo(filename)))
		//	{
		//		xl.DoAdjustDrawings = false;
		//		xl.Workbook.CalcMode = ExcelCalcMode.Automatic;

		//		var sheet = xl.Workbook.Worksheets[1];
		//		var col = 18;
		//		var row = 1;
		//		sheet.Cells[row++, col].Value = accounting.ActNumber;
		//		sheet.Cells[row++, col].Value = accounting.ActDate.HasValue ? accounting.ActDate.Value.ToString("dd MMMM yyyy г.", CultureInfo.GetCultureInfo("ru-RU")) : " ";
		//		sheet.Cells[row++, col].Value = accounting.OriginalSum;
		//		sheet.Cells[row++, col].Value = accounting.OriginalVat;
		//		sheet.Cells[row++, col].Value = (format != TemplatedDocumentFormat.CutPdf) ? accountantPerson.DisplayName : " ";
		//		sheet.Cells[row++, col].Value = (format != TemplatedDocumentFormat.CutPdf) ? directorPerson.DisplayName : " ";
		//		sheet.Cells[row++, col].Value = order.Number;
		//		sheet.Cells[row++, col].Value = order.RequestNumber;
		//		sheet.Cells[row++, col].Value = FormatDate(order.LoadingDate);
		//		sheet.Cells[row++, col].Value = order.SeatsCount;
		//		sheet.Cells[row++, col].Value = order.GrossWeight;
		//		sheet.Cells[row++, col].Value = order.InvoiceSum;
		//		sheet.Cells[row++, col].Value = order.VehicleNumbers;
		//		sheet.Cells[row++, col].Value = order.From;
		//		sheet.Cells[row++, col].Value = order.To;
		//		sheet.Cells[row++, col].Value = contract.Number;
		//		sheet.Cells[row++, col].Value = FormatDate(contract.Date);
		//		sheet.Cells[row++, col].Value = legal.DisplayName;
		//		sheet.Cells[row++, col].Value = legal.TIN;
		//		sheet.Cells[row++, col].Value = legal.KPP;
		//		sheet.Cells[row++, col].Value = legal.Address;
		//		sheet.Cells[row++, col].Value = ourLegal.DisplayName;
		//		sheet.Cells[row++, col].Value = ourLegal.TIN;
		//		sheet.Cells[row++, col].Value = ourLegal.KPP;
		//		sheet.Cells[row++, col].Value = ourLegal.OGRN;
		//		sheet.Cells[row++, col].Value = ourLegal.Address;
		//		sheet.Cells[row++, col].Value = ourBankAccount.Number;
		//		sheet.Cells[row++, col].Value = ourBank.Name;
		//		sheet.Cells[row++, col].Value = ourBank.BIC;
		//		sheet.Cells[row++, col].Value = ourBank.KSNP;
		//		sheet.Cells[row++, col].Value = currencies.First(w => w.ID == currency.CurrencyId).Name + ", " + currencies.First(w => w.ID == currency.CurrencyId).Code;

		//		// строки
		//		bool isFirst = true;
		//		int srow = 20;  // 20 строка
		//		var serviceB = new List<IdTuple>();
		//		foreach (var service in services)
		//		{
		//			int serviceKindId = serviceTypes.First(w => w.ID == service.ServiceTypeId).ServiceKindId.Value;
		//			var serviceKind = serviceKinds.First(w => w.ID == serviceKindId);
		//			// вычисление НДС
		//			double tax = 0;
		//			string taxRate = "";
		//			if (service.VatId.HasValue)
		//			{
		//				var vat = dataLogic.GetVats().First(w => w.ID == service.VatId.Value);
		//				tax = Math.Round(service.OriginalSum.Value - service.OriginalSum.Value / (100 + vat.Percent.Value) * 100, 2);
		//				taxRate = vat.Display;
		//			}

		//			var sbase = serviceB.FirstOrDefault(w => w.Id == serviceKindId);
		//			if (sbase == null)
		//				serviceB.Add(new IdTuple { Id = serviceKindId, Name = serviceKind.Name, Value = service.OriginalSum.Value, Value2 = tax, TaxRate = taxRate });
		//			else
		//			{
		//				sbase.Value += service.OriginalSum.Value;
		//				sbase.Value2 += tax;
		//			}
		//		}

		//		foreach (var service in serviceB)
		//		{
		//			if (!isFirst)
		//			{
		//				sheet.InsertRow(srow, 1);
		//				var r1 = sheet.Cells["A" + srow + ":" + "O" + srow];
		//				var r2 = sheet.Cells["A" + (srow - 1) + ":" + "O" + (srow - 1)];
		//				r2.Copy(r1);
		//				sheet.Row(srow).Height = service.Name.Length > 50 ? 30 : 15;
		//			}

		//			sheet.Cells[srow, 2].Value = service.Name;
		//			sheet.Cells[srow, 10].Value = service.TaxRate;
		//			sheet.Cells[srow, 11].Value = service.Value2;
		//			sheet.Cells[srow, 12].Value = service.Value;
		//			isFirst = false;
		//			srow++;
		//		}

		//		// вычисление значений во всех ячейках
		//		var allCells = sheet.Cells;
		//		foreach (var item in allCells)
		//			item.Calculate();

		//		if (format == TemplatedDocumentFormat.Pdf)
		//		{
		//			// вставляем первую подпись
		//			var signFilename = HttpContext.Current.Server.MapPath("~\\temp\\director.png");
		//			SaveBlob(signFilename, director.Signature);

		//			var fi = new IO.FileInfo(signFilename);
		//			var pic = sheet.Drawings.AddPicture("DirectorSign", fi);
		//			pic.SetPosition(410 + serviceB.Count * 30, 400);
		//			pic.SetSize(70);    // HACK:

		//			// вставляем вторую подпись
		//			if ((accountant != null) && (accountant.Signature != null))
		//			{
		//				signFilename = HttpContext.Current.Server.MapPath("~\\temp\\accountant.png");
		//				SaveBlob(signFilename, accountant.Signature);

		//				fi = new IO.FileInfo(signFilename);
		//				pic = sheet.Drawings.AddPicture("AccountantSign", fi);
		//				pic.SetPosition(430 + serviceB.Count * 30, 1000);
		//				pic.SetSize(80);    // HACK:
		//			}
		//		}

		//		xl.Save();
		//	}

		//	#endregion

		//	try
		//	{
		//		SpreadsheetInfo.SetLicense("FREE-LIMITED-KEY");
		//		var culture = Thread.CurrentThread.CurrentCulture;
		//		Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("ru-RU");
		//		ExcelFile ef = ExcelFile.Load(filename, LoadOptions.XlsxDefault);
		//		var pdfFilename = HttpContext.Current.Server.MapPath("~\\Temp\\" + entity.Filename + ".pdf");
		//		ef.Save(pdfFilename);
		//		Thread.CurrentThread.CurrentCulture = culture;

		//		switch (format)
		//		{
		//			case TemplatedDocumentFormat.Pdf:
		//				UploadTemplatedDocumentData(entity.ID, filename, "xlsx");
		//				UploadTemplatedDocumentData(entity.ID, pdfFilename, "pdf");
		//				break;
		//			case TemplatedDocumentFormat.CleanPdf:
		//				UploadTemplatedDocumentData(entity.ID, pdfFilename, "cleanpdf");
		//				break;
		//			case TemplatedDocumentFormat.CutPdf:
		//				UploadTemplatedDocumentData(entity.ID, pdfFilename, "cutpdf");
		//				break;
		//		}
		//	}
		//	catch { }

		//	return string.Empty;
		//}

		public string GenerateVatInvoice(TemplatedDocument entity, TemplatedDocumentFormat format)
		{
			#region подготовка данных и проверки

			var accounting = accountingLogic.GetAccounting(entity.AccountingId.Value);
			var order = orderLogic.GetOrder(accounting.OrderId);
			var contract = contractLogic.GetContract(order.ContractId.Value);
			var legal = legalLogic.GetLegal(accounting.LegalId ?? contract.LegalId);
			var ourLegal = legalLogic.GetLegalByOurLegal(accounting.OurLegalId ?? contract.OurLegalId);

			var director = employeeLogic.GetEmployee(ourLegal.DirectorId.Value);
			var directorPerson = personLogic.GetPerson(director.PersonId.Value);
			var accountant = participantLogic.GetWorkgroupByOrderAtDate(order.ID, accounting.ActDate.Value).Where(w => w.ParticipantRoleId == (int)ParticipantRoles.BUH).FirstOrDefault();
			var accountantPerson = personLogic.GetPersonByUser(accountant.UserId.Value);

			var docTypes = dataLogic.GetDocumentTypes();
			var serviceTypes = dataLogic.GetServiceTypes();
			var serviceKinds = dataLogic.GetServiceKinds();
			var docs = documentLogic.GetDocumentsByOrder(order.ID).Where(w => w.IsPrint == true);
			var payments = accountingLogic.GetPayments(accounting.ID);
			var services = accountingLogic.GetServicesByAccounting(accounting.ID);
			var contractCurrencies = contractLogic.GetContractCurrencies(contract.ID);
			var currencies = dataLogic.GetCurrencies();
			var contractCurrency = contractCurrencies.FirstOrDefault(w => w.CurrencyId == services.First().CurrencyId);
			if (contractCurrency == null)
				contractCurrency = contractCurrencies.FirstOrDefault(w => w.CurrencyId == 1);   // рубли присутствуют обязательно в этом случае

			var ourBankAccount = bankLogic.GetBankAccount(contractCurrency.OurBankAccountId);
			var ourBank = bankLogic.GetBank(ourBankAccount.BankId.Value);

			#endregion

			var filename = HttpContext.Current.Server.MapPath("~\\Temp\\" + entity.Filename + ".xlsx");
			DownloadTemplateData(entity.TemplateId.Value, filename);

			#region process

			using (var xl = new ExcelPackage(new IO.FileInfo(filename)))
			{
				xl.DoAdjustDrawings = false;
				xl.Workbook.CalcMode = ExcelCalcMode.Automatic;

				var sheet = xl.Workbook.Worksheets[1];
				var col = 18;
				var row = 1;
				sheet.Cells[row++, col].Value = accounting.ActNumber;
				sheet.Cells[row++, col].Value = accounting.ActDate.HasValue ? accounting.ActDate.Value.ToString("dd MMMM yyyy г.", CultureInfo.GetCultureInfo("ru-RU")) : " ";
				sheet.Cells[row++, col].Value = (contractCurrency.CurrencyId == 1) ? accounting.Sum : accounting.OriginalSum;
				sheet.Cells[row++, col].Value = (contractCurrency.CurrencyId == 1) ? accounting.Vat : accounting.OriginalVat;
				sheet.Cells[row++, col].Value = (format != TemplatedDocumentFormat.CutPdf) ? accountantPerson.DisplayName : " ";
				sheet.Cells[row++, col].Value = (format != TemplatedDocumentFormat.CutPdf) ? directorPerson.DisplayName : " ";
				sheet.Cells[row++, col].Value = order.Number;
				sheet.Cells[row++, col].Value = order.RequestNumber;
				sheet.Cells[row++, col].Value = FormatDate(order.LoadingDate);
				sheet.Cells[row++, col].Value = order.SeatsCount;
				sheet.Cells[row++, col].Value = order.GrossWeight;
				sheet.Cells[row++, col].Value = order.InvoiceSum;
				sheet.Cells[row++, col].Value = order.VehicleNumbers;
				sheet.Cells[row++, col].Value = order.From;
				sheet.Cells[row++, col].Value = order.To;
				sheet.Cells[row++, col].Value = contract.Number;
				sheet.Cells[row++, col].Value = FormatDate(contract.Date);
				sheet.Cells[row++, col].Value = legal.DisplayName;
				sheet.Cells[row++, col].Value = legal.TIN;
				sheet.Cells[row++, col].Value = legal.KPP;
				sheet.Cells[row++, col].Value = legal.Address;
				sheet.Cells[row++, col].Value = ourLegal.DisplayName;
				sheet.Cells[row++, col].Value = ourLegal.TIN;
				sheet.Cells[row++, col].Value = ourLegal.KPP;
				sheet.Cells[row++, col].Value = ourLegal.OGRN;
				sheet.Cells[row++, col].Value = ourLegal.Address;
				sheet.Cells[row++, col].Value = ourBankAccount.Number;
				sheet.Cells[row++, col].Value = ourBank.Name;
				sheet.Cells[row++, col].Value = ourBank.BIC;
				sheet.Cells[row++, col].Value = ourBank.KSNP;
				sheet.Cells[row++, col].Value = currencies.First(w => w.ID == contractCurrency.CurrencyId).Name + ", " + currencies.First(w => w.ID == contractCurrency.CurrencyId).Code;

				// документы
				foreach (var doc in docs)
					if (doc.DocumentTypeId == 20)   // ДТ
					{
						sheet.Cells[row++, col].Value = docTypes.Where(w => w.ID == doc.DocumentTypeId).Select(s => s.Display).FirstOrDefault() + " № :";
						sheet.Cells[row++, col].Value = doc.Number;
					}

				row = 46;
				if (payments.Count() > 0)
					sheet.Cells[row, col].Value = String.Join(", ", payments.Select(s => "№ " + s.Number + " от " + s.Date.ToString("dd.MM.yyyy")).ToArray());

				var pricelist = GetPricelist(order) ?? new Pricelist(); // HACK:
				var priceKinds = pricelistLogic.GetPriceKinds(pricelist.ID);
				// строки
				bool isFirst = true;
				int srow = 20;  // 20 строка
				var serviceB = new List<IdTuple>();
				foreach (var service in services)
				{
					int serviceKindId = serviceTypes.First(w => w.ID == service.ServiceTypeId).ServiceKindId;
					var resultName = serviceKinds.First(w => w.ID == serviceKindId).Name;
					if (pricelist.ID > 0)
						resultName = priceKinds.First(w => w.ServiceKindId == serviceKindId).Name;

					// вычисление НДС
					double tax = 0;
					string taxRate = "";
					if (service.VatId.HasValue)
					{
						var vat = dataLogic.GetVats().First(w => w.ID == service.VatId.Value);
						taxRate = vat.Display;
						if (contractCurrency.CurrencyId == 1)   // рубли
							tax = service.Sum.Value - service.Sum.Value / (100 + vat.Percent) * 100;
						else
							tax = service.OriginalSum.Value - service.OriginalSum.Value / (100 + vat.Percent) * 100;
					}

					var sbase = serviceB.FirstOrDefault(w => w.Id == serviceKindId);
					if (sbase == null)
					{
						if (contractCurrency.CurrencyId == 1)   // рубли
							serviceB.Add(new IdTuple { Id = serviceKindId, Name = resultName, Value = service.Sum.Value, Value2 = tax, TaxRate = taxRate });
						else
							serviceB.Add(new IdTuple { Id = serviceKindId, Name = resultName, Value = service.OriginalSum.Value, Value2 = tax, TaxRate = taxRate });
					}
					else
					{
						sbase.Value2 += tax;
						sbase.Value += (contractCurrency.CurrencyId == 1) ? service.Sum.Value : service.OriginalSum.Value;  // рубли
					}
				}

				foreach (var service in serviceB)
				{
					if (!isFirst)
					{
						sheet.InsertRow(srow, 1);
						var r1 = sheet.Cells["A" + srow + ":" + "O" + srow];
						var r2 = sheet.Cells["A" + (srow - 1) + ":" + "O" + (srow - 1)];
						r2.Copy(r1);
					}

					sheet.Row(srow).Height = service.Name.Length > 52 ? 30 : 15;
					sheet.Cells[srow, 2].Value = service.Name;
					sheet.Cells[srow, 10].Value = service.TaxRate;
					sheet.Cells[srow, 11].Value = Math.Round(service.Value2, 2);
					sheet.Cells[srow, 12].Value = Math.Round(service.Value, 2);
					isFirst = false;
					srow++;
				}

				// вычисление значений во всех ячейках
				var allCells = sheet.Cells;
				foreach (var item in allCells)
					item.Calculate();

				if (format == TemplatedDocumentFormat.Pdf)
				{
					// вставляем первую подпись
					var signFilename = HttpContext.Current.Server.MapPath("~\\temp\\" + director.ID + "director.png");
					SaveBlob(signFilename, director.Signature);

					var fi = new IO.FileInfo(signFilename);
					var pic = sheet.Drawings.AddPicture("DirectorSign", fi);
					pic.SetPosition(400 + serviceB.Count * 30, 400);
					pic.SetSize(80);    // HACK:

					// вставляем вторую подпись
					if (accountant != null)
					{
						var accEmp = employeeLogic.GetEmployeesByLegal(ourLegal.ID).Where(w => w.PersonId == accountantPerson.ID).FirstOrDefault();
						if (accEmp != null)
						{
							signFilename = HttpContext.Current.Server.MapPath("~\\temp\\" + accountant.ID + "accountant.png");
							SaveBlob(signFilename, accEmp.Signature);

							fi = new IO.FileInfo(signFilename);
							pic = sheet.Drawings.AddPicture("AccountantSign", fi);
							pic.SetPosition(440 + serviceB.Count * 20, 1050);
							pic.SetSize(55);
						}
					}
				}

				xl.Save();
			}

			#endregion

			try
			{
				SpreadsheetInfo.SetLicense("FREE-LIMITED-KEY");
				var culture = Thread.CurrentThread.CurrentCulture;
				Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("ru-RU");
				ExcelFile ef = ExcelFile.Load(filename, LoadOptions.XlsxDefault);
				var pdfFilename = HttpContext.Current.Server.MapPath("~\\Temp\\" + entity.Filename + ".pdf");
				ef.Save(pdfFilename);
				Thread.CurrentThread.CurrentCulture = culture;

				switch (format)
				{
					case TemplatedDocumentFormat.Pdf:
						UploadTemplatedDocumentData(entity.ID, filename, "xlsx");
						UploadTemplatedDocumentData(entity.ID, pdfFilename, "pdf");
						break;
					case TemplatedDocumentFormat.CleanPdf:
						UploadTemplatedDocumentData(entity.ID, pdfFilename, "cleanpdf");
						break;
					case TemplatedDocumentFormat.CutPdf:
						UploadTemplatedDocumentData(entity.ID, pdfFilename, "cutpdf");
						break;
				}
			}
			catch { }

			return string.Empty;
		}

		public string GenerateAct(TemplatedDocument entity, TemplatedDocumentFormat format)
		{
			#region подготовка данных и проверки

			var accounting = accountingLogic.GetAccounting(entity.AccountingId.Value);
			if (!accounting.LegalId.HasValue)
				return "Не заполнено Юрлицо в доходе";

			var services = accountingLogic.GetServicesByAccounting(accounting.ID);
			if (services.Count() == 0)
				return "Нет услуг";

			var order = orderLogic.GetOrder(accounting.OrderId);
			var contract = contractLogic.GetContract(order.ContractId.Value);
			var legal = legalLogic.GetLegal(accounting.LegalId.Value);
			var ourLegal = legalLogic.GetLegalByOurLegal(accounting.OurLegalId ?? contract.OurLegalId);
			var ourDirector = employeeLogic.GetEmployee(ourLegal.DirectorId.Value);
			var ourDirectorPerson = personLogic.GetPerson(ourDirector.PersonId.Value);
			if (!legal.DirectorId.HasValue)
				return "Не указан подписант клиента";

			var director = employeeLogic.GetEmployee(legal.DirectorId.Value);
			if (!director.PersonId.HasValue)
				return "Не указано физлицо подписанта клиента";

			var directorPerson = personLogic.GetPerson(director.PersonId.Value);
			var serviceTypes = dataLogic.GetServiceTypes();
			var serviceKinds = dataLogic.GetServiceKinds();
			var docTypes = dataLogic.GetDocumentTypes();
			var docs = documentLogic.GetDocumentsByOrder(order.ID).Where(w => w.IsPrint == true);

			#endregion

			var filename = HttpContext.Current.Server.MapPath("~\\Temp\\" + entity.Filename + ".xlsx");
			DownloadTemplateData(entity.TemplateId.Value, filename);

			#region process

			using (var xl = new ExcelPackage(new IO.FileInfo(filename)))
			{
				xl.DoAdjustDrawings = false;
				xl.Workbook.CalcMode = ExcelCalcMode.Automatic;

				// Акт формируется только в рублях
				var sheet = xl.Workbook.Worksheets[1];
				var col = 17;
				var row = 1;
				sheet.Cells[row++, col].Value = accounting.ActNumber;
				sheet.Cells[row++, col].Value = FormatDate(accounting.ActDate);
				sheet.Cells[row++, col].Value = accounting.Sum;
				sheet.Cells[row++, col].Value = accounting.Vat;
				sheet.Cells[row++, col].Value = (format != TemplatedDocumentFormat.CutPdf) ? ourDirector.Position : " ";
				sheet.Cells[row++, col].Value = (format != TemplatedDocumentFormat.CutPdf) ? ourDirectorPerson.DisplayName : " ";
				sheet.Cells[row++, col].Value = (format != TemplatedDocumentFormat.CutPdf) ? director.Position : " ";
				sheet.Cells[row++, col].Value = (format != TemplatedDocumentFormat.CutPdf) ? directorPerson.DisplayName : " ";
				sheet.Cells[row++, col].Value = order.Number;
				sheet.Cells[row++, col].Value = order.RequestNumber;
				sheet.Cells[row++, col].Value = accounting.InvoiceNumber;
				sheet.Cells[row++, col].Value = FormatDate(accounting.InvoiceDate);
				sheet.Cells[row++, col].Value = contract.Number;
				sheet.Cells[row++, col].Value = FormatDate(contract.Date);
				sheet.Cells[row++, col].Value = legal.DisplayName;
				sheet.Cells[row++, col].Value = legal.TIN;
				sheet.Cells[row++, col].Value = legal.KPP;
				sheet.Cells[row++, col].Value = legal.Address;
				sheet.Cells[row++, col].Value = ourLegal.DisplayName;
				sheet.Cells[row++, col].Value = ourLegal.TIN;
				sheet.Cells[row++, col].Value = ourLegal.KPP;
				sheet.Cells[row++, col].Value = ourLegal.OGRN;
				sheet.Cells[row++, col].Value = ourLegal.Address;

				// документы
				foreach (var doc in docs)
				{
					sheet.Cells[row++, col].Value = docTypes.Where(w => w.ID == doc.DocumentTypeId).Select(s => s.Display).FirstOrDefault() + " № :";
					sheet.Cells[row++, col].Value = doc.Number;
				}

				var pricelist = GetPricelist(order) ?? new Pricelist(); // HACK:
				var priceKinds = pricelistLogic.GetPriceKinds(pricelist.ID);
				// строки
				bool isFirst = true;
				int srow = 19;  // 19 строка
				var serviceB = new List<IdTuple>();
				foreach (var service in services)
				{
					int serviceKindId = serviceTypes.First(w => w.ID == service.ServiceTypeId).ServiceKindId;
					var resultName = serviceKinds.First(w => w.ID == serviceKindId).Name;
					if (pricelist.ID > 0)
						resultName = priceKinds.First(w => w.ServiceKindId == serviceKindId).Name;

					// вычисление НДС
					double tax = 0;
					if (service.VatId.HasValue)
					{
						var vat = dataLogic.GetVats().First(w => w.ID == service.VatId.Value);
						tax = service.Sum.Value - service.Sum.Value / (100 + vat.Percent) * 100;
					}

					var sbase = serviceB.FirstOrDefault(w => w.Id == serviceKindId);
					if (sbase == null)
						serviceB.Add(new IdTuple { Id = serviceKindId, Name = resultName, Value = service.Sum.Value, Value2 = tax });
					else
					{
						sbase.Value += service.Sum.Value;
						sbase.Value2 += tax;
					}
				}

				foreach (var service in serviceB)
				{
					if (!isFirst)
					{
						sheet.InsertRow(srow, 1);
						var r1 = sheet.Cells["A" + srow + ":" + "N" + srow];
						var r2 = sheet.Cells["A" + (srow - 1) + ":" + "N" + (srow - 1)];
						r2.Copy(r1);
						sheet.Row(43).Height = 1;   // нижняя строка обжасти сжатия
					}

					sheet.Cells[srow, 2].Value = service.Name;
					sheet.Cells[srow, 11].Value = Math.Round(service.Value2, 2);
					sheet.Cells[srow, 13].Value = Math.Round(service.Value, 2);
					isFirst = false;
					srow++;
				}

				// вычисление значений во всех ячейках
				var allCells = sheet.Cells;
				foreach (var item in allCells)
					item.Calculate();

				xl.Save();
			}

			#endregion

			try
			{
				SpreadsheetInfo.SetLicense("FREE-LIMITED-KEY");
				var culture = Thread.CurrentThread.CurrentCulture;
				Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("ru-RU");
				ExcelFile ef = ExcelFile.Load(filename, LoadOptions.XlsxDefault);
				var pdfFilename = HttpContext.Current.Server.MapPath("~\\Temp\\" + entity.Filename + ".pdf");
				ef.Save(pdfFilename);
				Thread.CurrentThread.CurrentCulture = culture;

				switch (format)
				{
					case TemplatedDocumentFormat.Pdf:
						UploadTemplatedDocumentData(entity.ID, filename, "xlsx");
						UploadTemplatedDocumentData(entity.ID, pdfFilename, "pdf");
						break;
					case TemplatedDocumentFormat.CleanPdf:
						UploadTemplatedDocumentData(entity.ID, pdfFilename, "cleanpdf");
						break;
					case TemplatedDocumentFormat.CutPdf:
						UploadTemplatedDocumentData(entity.ID, pdfFilename, "cutpdf");
						break;
				}
			}
			catch { }

			return string.Empty;
		}

		public string GenerateEnAct(TemplatedDocument entity, TemplatedDocumentFormat format)
		{
			var accounting = accountingLogic.GetAccounting(entity.AccountingId.Value);
			var order = orderLogic.GetOrder(accounting.OrderId);
			var contract = contractLogic.GetContract(accounting.ContractId ?? order.ContractId.Value);
			var legal = legalLogic.GetLegal(contract.LegalId);
			var ourLegal = legalLogic.GetLegalByOurLegal(contract.OurLegalId);
			var ourDirector = employeeLogic.GetEmployee(ourLegal.DirectorId.Value);
			var ourDirectorPerson = personLogic.GetPerson(ourDirector.PersonId.Value);
			if (!legal.DirectorId.HasValue)
				return "Не указан подписант клиента";

			var director = employeeLogic.GetEmployee(legal.DirectorId.Value);
			var directorPerson = personLogic.GetPerson(director.PersonId.Value);
			var serviceTypes = dataLogic.GetServiceTypes();
			var serviceKinds = dataLogic.GetServiceKinds();
			var currencies = dataLogic.GetCurrencies();
			var contractRoles = dataLogic.GetContractRoles();
			var services = accountingLogic.GetServicesByAccounting(accounting.ID);
			if (services.Count() == 0)
				return "Нет услуг";

			var currencyId = services.First().CurrencyId ?? 1;

			var docTypes = dataLogic.GetDocumentTypes();
			var docs = documentLogic.GetDocumentsByOrder(order.ID).Where(w => w.IsPrint == true);

			var filename = HttpContext.Current.Server.MapPath("~\\Temp\\" + entity.Filename + ".xlsx");

			DownloadTemplateData(entity.TemplateId.Value, filename);

			#region process

			using (var xl = new ExcelPackage(new IO.FileInfo(filename)))
			{
				xl.DoAdjustDrawings = false;
				xl.Workbook.CalcMode = ExcelCalcMode.Automatic;

				var sheet = xl.Workbook.Worksheets[1];
				var col = 31;
				var row = 1;
				sheet.Cells[row++, col].Value = accounting.ActNumber;
				sheet.Cells[row++, col].Value = FormatDate(accounting.ActDate);
				sheet.Cells[row++, col].Value = accounting.OriginalSum;
				sheet.Cells[row++, col].Value = accounting.OriginalVat;
				sheet.Cells[row++, col].Value = ourDirector.Position;
				sheet.Cells[row++, col].Value = ourDirector.EnPosition;
				sheet.Cells[row++, col].Value = ourDirectorPerson.DisplayName;
				sheet.Cells[row++, col].Value = ourDirectorPerson.EnName;
				sheet.Cells[row++, col].Value = director.Position;
				sheet.Cells[row++, col].Value = director.EnPosition;
				sheet.Cells[row++, col].Value = directorPerson.DisplayName;
				sheet.Cells[row++, col].Value = string.IsNullOrEmpty(directorPerson.EnName) ? directorPerson.DisplayName : directorPerson.EnName;
				sheet.Cells[row++, col].Value = order.Number;
				sheet.Cells[row++, col].Value = accounting.Number;
				sheet.Cells[row++, col].Value = accounting.InvoiceNumber;
				sheet.Cells[row++, col].Value = FormatDate(accounting.InvoiceDate);
				sheet.Cells[row++, col].Value = contract.Number;
				sheet.Cells[row++, col].Value = FormatDate(contract.Date);
				sheet.Cells[row++, col].Value = legal.DisplayName;
				sheet.Cells[row++, col].Value = legal.EnName;
				sheet.Cells[row++, col].Value = legal.TIN + " ";
				sheet.Cells[row++, col].Value = legal.KPP + " ";
				sheet.Cells[row++, col].Value = legal.Address;
				sheet.Cells[row++, col].Value = legal.EnAddress;
				sheet.Cells[row++, col].Value = ourLegal.DisplayName;
				sheet.Cells[row++, col].Value = ourLegal.EnShortName;
				sheet.Cells[row++, col].Value = ourLegal.TIN;
				sheet.Cells[row++, col].Value = ourLegal.KPP;
				sheet.Cells[row++, col].Value = ourLegal.OGRN;
				sheet.Cells[row++, col].Value = ourLegal.Address;
				sheet.Cells[row++, col].Value = ourLegal.EnAddress;
				sheet.Cells[row++, col].Value = currencies.First(w => w.ID == currencyId).Display;
				sheet.Cells[row++, col].Value = contractRoles.First(w => w.ID == contract.OurContractRoleId).Display;
				sheet.Cells[row++, col].Value = contractRoles.First(w => w.ID == contract.OurContractRoleId).EnName;
				sheet.Cells[row++, col].Value = contractRoles.First(w => w.ID == contract.ContractRoleId).Display;
				sheet.Cells[row++, col].Value = contractRoles.First(w => w.ID == contract.ContractRoleId).EnName;

				// документы
				if (accounting.IsIncome)
					foreach (var doc in docs)
						if (doc.DocumentTypeId == 20)   // ДТ
						{
							sheet.Cells[row, col].Value = docTypes.Where(w => w.ID == doc.DocumentTypeId).Select(s => s.Display).FirstOrDefault() + " № :";
							sheet.Cells[row++, col + 2].Value = docTypes.Where(w => w.ID == doc.DocumentTypeId).Select(s => s.EnDescription).FirstOrDefault() + " № :";
							sheet.Cells[row++, col].Value = doc.Number;
						}

				// HACK: плохой хак
				if (string.IsNullOrWhiteSpace(legal.KPP))
					sheet.Cells["L38"].Value = "";
				if (string.IsNullOrWhiteSpace(legal.KPP))
					sheet.Cells["Z38"].Value = "";
				if (string.IsNullOrWhiteSpace(legal.TIN))
					sheet.Cells["I38"].Value = "";
				if (string.IsNullOrWhiteSpace(legal.TIN))
					sheet.Cells["W38"].Value = "";

				var pricelist = GetPricelist(order) ?? new Pricelist(); // HACK:
				var priceKinds = pricelistLogic.GetPriceKinds(pricelist.ID);
				// строки
				bool isFirst = true;
				int srow = 16;  // 16 строка
				var serviceB = new List<IdTuple>();
				foreach (var service in services)
				{
					int serviceKindId = serviceTypes.First(w => w.ID == service.ServiceTypeId).ServiceKindId;
					var resultName = serviceKinds.First(w => w.ID == serviceKindId).Name;
					if (pricelist.ID > 0)
						resultName = priceKinds.First(w => w.ServiceKindId == serviceKindId).Name;

					var resultEnName = serviceKinds.First(w => w.ID == serviceKindId).EnName;
					if (pricelist.ID > 0)
						resultEnName = priceKinds.First(w => w.ServiceKindId == serviceKindId).EnName;

					// вычисление НДС
					double tax = 0;
					if (service.VatId.HasValue)
					{
						var vat = dataLogic.GetVats().First(w => w.ID == service.VatId.Value);
						tax = service.OriginalSum.Value - service.OriginalSum.Value / (100 + vat.Percent) * 100;
					}

					var sbase = serviceB.FirstOrDefault(w => w.Id == serviceKindId);
					if (sbase == null)
						serviceB.Add(new IdTuple { Id = serviceKindId, Name = resultName, EnName = resultEnName, Value = service.OriginalSum.Value, Value2 = tax });
					else
					{
						sbase.Value += service.OriginalSum.Value;
						sbase.Value2 += tax;
					}
				}

				foreach (var service in serviceB)
				{
					if (!isFirst)
					{
						sheet.InsertRow(srow, 1);
						var r1 = sheet.Cells["A" + srow + ":" + "N" + srow];
						var r2 = sheet.Cells["A" + (srow - 1) + ":" + "N" + (srow - 1)];
						r2.Copy(r1);
						sheet.Row(40).Height = 1;   // нижняя строка сжимающегося блока
					}

					sheet.Cells[srow, 2].Value = service.Name;
					sheet.Cells[srow, 11].Value = Math.Round(service.Value2, 2);
					sheet.Cells[srow, 13].Value = Math.Round(service.Value, 2);
					sheet.Cells[srow, 16].Value = service.EnName;
					sheet.Cells[srow, 25].Value = Math.Round(service.Value2, 2);
					sheet.Cells[srow, 27].Value = Math.Round(service.Value, 2);
					isFirst = false;
					srow++;
				}

				// вычисление значений во всех ячейках
				var allCells = sheet.Cells;
				foreach (var item in allCells)
					item.Calculate();

				xl.Save();
			}

			#endregion

			try
			{
				SpreadsheetInfo.SetLicense("FREE-LIMITED-KEY");
				var culture = Thread.CurrentThread.CurrentCulture;
				Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("ru-RU");
				ExcelFile ef = ExcelFile.Load(filename, LoadOptions.XlsxDefault);
				var pdfFilename = HttpContext.Current.Server.MapPath("~\\Temp\\" + entity.Filename + ".pdf");
				ef.Save(pdfFilename);
				Thread.CurrentThread.CurrentCulture = culture;

				switch (format)
				{
					case TemplatedDocumentFormat.Pdf:
						UploadTemplatedDocumentData(entity.ID, filename, "xlsx");
						UploadTemplatedDocumentData(entity.ID, pdfFilename, "pdf");
						break;
					case TemplatedDocumentFormat.CleanPdf:
						UploadTemplatedDocumentData(entity.ID, pdfFilename, "cleanpdf");
						break;
					case TemplatedDocumentFormat.CutPdf:
						UploadTemplatedDocumentData(entity.ID, pdfFilename, "cutpdf");
						break;
				}
			}
			catch { }

			return string.Empty;
		}

		public string GenerateClaim(TemplatedDocument entity, TemplatedDocumentFormat format)
		{
			var order = orderLogic.GetOrder(entity.OrderId.Value);
			var contract = contractLogic.GetContract(order.ContractId.Value);
			var legal = legalLogic.GetLegal(contract.LegalId);
			var ourLegal = legalLogic.GetLegal(contract.OurLegalId);
			var ourDirector = employeeLogic.GetEmployee(ourLegal.DirectorId.Value);
			var ourDirectorPerson = personLogic.GetPerson(ourDirector.PersonId.Value);
			if (!legal.DirectorId.HasValue)
				return "Не указан подписант клиента";

			var director = employeeLogic.GetEmployee(legal.DirectorId.Value);
			var directorPerson = personLogic.GetPerson(director.PersonId.Value);
			var fromPoint = orderLogic.GetRoutePoints(order.ID).OrderBy(o => o.No).FirstOrDefault();
			if (fromPoint == null)
				return "Нет маршрутных точек";

			if (!fromPoint.ParticipantLegalId.HasValue)
				return "Не указан ГО";

			var fromPointLegal = legalLogic.GetLegal(fromPoint.ParticipantLegalId.Value);
			if (!fromPoint.RouteContactID.HasValue)
				return "Не указан контакт в конечной точке";

			var fromPointContact = legalLogic.GetRouteContact(fromPoint.RouteContactID.Value);
			var toPoint = orderLogic.GetRoutePoints(order.ID).OrderBy(o => o.No).Last();
			if (!toPoint.ParticipantLegalId.HasValue)
				return "Не указан ГП";

			var toPointLegal = legalLogic.GetLegal(toPoint.ParticipantLegalId.Value);
			var toPointContact = legalLogic.GetRouteContact(toPoint.RouteContactID.Value);
			var seats = orderLogic.GetCargoSeats(order.ID);
			var contractRoles = dataLogic.GetContractRoles();
			var cargoDescriptions = dataLogic.GetCargoDescriptions();
			var packageTypes = dataLogic.GetPackageTypes();
			var filename = HttpContext.Current.Server.MapPath("~\\Temp\\" + entity.Filename + ".xlsx");

			DownloadTemplateData(entity.TemplateId.Value, filename);

			#region process

			using (var xl = new ExcelPackage(new IO.FileInfo(filename)))
			{
				xl.DoAdjustDrawings = false;
				xl.Workbook.CalcMode = ExcelCalcMode.Automatic;

				var sheet = xl.Workbook.Worksheets[1];
				var col = 17;
				var row = 1;
				sheet.Cells[row++, col].Value = order.RequestNumber;
				sheet.Cells[row++, col].Value = FormatDate(order.RequestDate);
				sheet.Cells[row++, col].Value = contract.Number;
				sheet.Cells[row++, col].Value = FormatDate(contract.Date);
				sheet.Cells[row++, col].Value = ourDirectorPerson.DisplayName;
				sheet.Cells[row++, col].Value = ourDirectorPerson.GenitiveFamily + " " + ourDirectorPerson.Initials;
				sheet.Cells[row++, col].Value = ourDirector.Position;
				sheet.Cells[row++, col].Value = ourDirector.GenitivePosition;
				sheet.Cells[row++, col].Value = ourDirector.Basis;
				sheet.Cells[row++, col].Value = directorPerson.DisplayName;
				sheet.Cells[row++, col].Value = directorPerson.GenitiveFamily + " " + directorPerson.Initials;
				sheet.Cells[row++, col].Value = director.Position;
				sheet.Cells[row++, col].Value = director.GenitivePosition;
				sheet.Cells[row++, col].Value = director.Basis;
				sheet.Cells[row++, col].Value = legal.DisplayName;
				sheet.Cells[row++, col].Value = legal.TIN + " ";
				sheet.Cells[row++, col].Value = legal.KPP + " ";
				sheet.Cells[row++, col].Value = legal.Address;
				sheet.Cells[row++, col].Value = ourLegal.DisplayName;
				sheet.Cells[row++, col].Value = ourLegal.TIN;
				sheet.Cells[row++, col].Value = ourLegal.KPP;
				sheet.Cells[row++, col].Value = ourLegal.OGRN;
				sheet.Cells[row++, col].Value = ourLegal.Address;
				sheet.Cells[row++, col].Value = contractRoles.First(w => w.ID == contract.OurContractRoleId).Display;
				sheet.Cells[row++, col].Value = contractRoles.First(w => w.ID == contract.OurContractRoleId).AblativeName;
				sheet.Cells[row++, col].Value = contractRoles.First(w => w.ID == contract.ContractRoleId).Display;
				sheet.Cells[row++, col].Value = contractRoles.First(w => w.ID == contract.ContractRoleId).DativeName;
				sheet.Cells[row++, col].Value = order.Danger + " ";
				sheet.Cells[row++, col].Value = order.SpecialCustody + " ";
				sheet.Cells[row++, col].Value = order.TemperatureRegime + " ";
				sheet.Cells[row++, col].Value = order.zkzMarshrut + " ";
				sheet.Cells[row++, col].Value = FormatDate(order.LoadingDate);
				sheet.Cells[row++, col].Value = FormatDate(order.UnloadingDate);
				sheet.Cells[row++, col].Value = order.Comment + " ";
				sheet.Cells[row++, col].Value = order.zkzExpTO + " ";
				sheet.Cells[row++, col].Value = order.zkzImpTO + " ";
				sheet.Cells[row++, col].Value = order.Cost;

				sheet.Cells[row++, col].Value = fromPointLegal.DisplayName;
				sheet.Cells[row++, col].Value = fromPointLegal.TIN;
				sheet.Cells[row++, col].Value = fromPointLegal.WorkTime;
				sheet.Cells[row++, col].Value = fromPointContact.Name;
				sheet.Cells[row++, col].Value = fromPointContact.Phones;
				sheet.Cells[row++, col].Value = fromPoint.ParticipantComment + " ";

				sheet.Cells[row++, col].Value = toPointLegal.DisplayName;
				sheet.Cells[row++, col].Value = toPointLegal.TIN;
				sheet.Cells[row++, col].Value = toPointLegal.WorkTime;
				sheet.Cells[row++, col].Value = toPointContact.Name;
				sheet.Cells[row++, col].Value = toPointContact.Phones;
				sheet.Cells[row++, col].Value = toPoint.ParticipantComment + " ";

				// строки
				bool isFirst = true;
				int srow = 15;  // 15 строка
				int seatTotal = 0;
				double seatTotalVolume = 0;
				double seatTotalWeight = 0;
				foreach (var seat in seats)
				{
					if (!isFirst)
					{
						sheet.InsertRow(srow, 1);
						var r1 = sheet.Cells["A" + srow + ":" + "O" + srow];
						var r2 = sheet.Cells["A" + (srow - 1) + ":" + "O" + (srow - 1)];
						r2.Copy(r1);
						sheet.Row(62).Height = 1;
					}

					sheet.Cells[srow, 2].Value = cargoDescriptions.First(w => w.ID == seat.CargoDescriptionId).Display;
					sheet.Cells[srow, 5].Value = seat.SeatCount;
					sheet.Cells[srow, 6].Value = seat.Length;
					sheet.Cells[srow, 7].Value = seat.Width;
					sheet.Cells[srow, 8].Value = seat.Height;
					sheet.Cells[srow, 9].Value = seat.Volume;
					sheet.Cells[srow, 10].Value = seat.GrossWeight;
					sheet.Cells[srow, 12].Value = packageTypes.First(w => w.ID == seat.PackageTypeId).Display;

					sheet.Row(srow).Height = cargoDescriptions.First(w => w.ID == seat.CargoDescriptionId).Display.Length > 50 ? 26 : 15;
					isFirst = false;
					srow++;
					seatTotal += seat.SeatCount ?? 0;
					seatTotalVolume += seat.Volume ?? 0;
					seatTotalWeight += seat.GrossWeight ?? 0;
				}

				sheet.Cells[srow, 5].Value = seatTotal;
				sheet.Cells[srow, 9].Value = seatTotalVolume;
				sheet.Cells[srow, 10].Value = seatTotalWeight;

				// вычисление значений во всех ячейках
				var allCells = sheet.Cells;
				foreach (var item in allCells)
					item.Calculate();

				xl.Save();
			}

			#endregion

			try
			{
				SpreadsheetInfo.SetLicense("FREE-LIMITED-KEY");
				var culture = Thread.CurrentThread.CurrentCulture;
				Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("ru-RU");
				ExcelFile ef = ExcelFile.Load(filename, LoadOptions.XlsxDefault);
				var pdfFilename = HttpContext.Current.Server.MapPath("~\\Temp\\" + entity.Filename + ".pdf");
				ef.Save(pdfFilename);
				Thread.CurrentThread.CurrentCulture = culture;

				switch (format)
				{
					case TemplatedDocumentFormat.Pdf:
						UploadTemplatedDocumentData(entity.ID, filename, "xlsx");
						UploadTemplatedDocumentData(entity.ID, pdfFilename, "pdf");
						break;
					case TemplatedDocumentFormat.CleanPdf:
						UploadTemplatedDocumentData(entity.ID, pdfFilename, "cleanpdf");
						break;
					case TemplatedDocumentFormat.CutPdf:
						UploadTemplatedDocumentData(entity.ID, pdfFilename, "cutpdf");
						break;
				}
			}
			catch { }

			return string.Empty;
		}

		public string GenerateRequest(TemplatedDocument entity, TemplatedDocumentFormat format)
		{
			// только для расхода
			var accounting = accountingLogic.GetAccounting(entity.AccountingId.Value);
			var order = orderLogic.GetOrder(accounting.OrderId);
			var contract = contractLogic.GetContract(accounting.ContractId.Value);
			var legal = legalLogic.GetLegal(contract.LegalId);
			var ourLegal = legalLogic.GetLegalByOurLegal(contract.OurLegalId);

			var ourDirector = employeeLogic.GetEmployee(ourLegal.DirectorId.Value);
			var ourDirectorPerson = personLogic.GetPerson(ourDirector.PersonId.Value);
			if (!legal.DirectorId.HasValue)
				return "Не указан подписант клиента";

			var director = employeeLogic.GetEmployee(legal.DirectorId.Value);
			var directorPerson = personLogic.GetPerson(director.PersonId.Value);

			var seats = orderLogic.GetCargoSeats(order.ID);
			var contractRoles = dataLogic.GetContractRoles();
			var cargoDescriptions = dataLogic.GetCargoDescriptions();
			var packageTypes = dataLogic.GetPackageTypes();
			var currencies = dataLogic.GetCurrencies();
			var currencyRateUses = dataLogic.GetCurrencyRateUses();
			var services = accountingLogic.GetServicesByAccounting(accounting.ID);
			if (services.Count() == 0)
				return "Нет услуг";

			var currencyId = services.First().CurrencyId ?? 1;

			RoutePoint fromPoint = null;
			RoutePoint toPoint = null;
			List<RouteSegment> segments = new List<RouteSegment>();
			var selectedSegments = accountingLogic.GetAccountingRouteSegments(accounting.ID);
			if (selectedSegments.Count() != 0)
			{
				var fromSegment = orderLogic.GetRouteSegment(selectedSegments.First().RouteSegmentId.Value);
				fromPoint = orderLogic.GetRoutePoint(fromSegment.FromRoutePointId);
				var toSegment = orderLogic.GetRouteSegment(selectedSegments.Last().RouteSegmentId.Value);
				toPoint = orderLogic.GetRoutePoint(toSegment.ToRoutePointId);

				foreach (var ss in selectedSegments)
					segments.Add(orderLogic.GetRouteSegment(ss.RouteSegmentId.Value));
			}
			else
			{
				var points = orderLogic.GetRoutePoints(order.ID);
				if (points.Count() < 2)
					return "Недостаточно маршрутных точек";

				fromPoint = points.First();
				toPoint = points.Last();
				segments = orderLogic.GetRouteSegments(order.ID).ToList();
			}

			if (fromPoint == null)
				return "Нет маршрутных точек";

			if (!fromPoint.ParticipantLegalId.HasValue)
				return "Не указан ГО (в первой точке)";

			if (!toPoint.ParticipantLegalId.HasValue)
				return "Не указан ГП (в последней точке)";


			var fromPointLegal = legalLogic.GetLegal(fromPoint.ParticipantLegalId.Value);
			var fromPointContact = legalLogic.GetRouteContact(fromPoint.RouteContactID.Value);
			var toPointLegal = legalLogic.GetLegal(toPoint.ParticipantLegalId.Value);
			if (!toPoint.RouteContactID.HasValue)
				return "Не указан контакт в точке " + toPoint.No;

			var toPointContact = legalLogic.GetRouteContact(toPoint.RouteContactID.Value);

			var segmentsRoute = "";
			var segmentsExpTo = "";
			var segmentsImpTo = "";
			var segmentsPoints = new List<RoutePoint>();
			foreach (var routeSegment in segments)
			{
				if (!segmentsPoints.Exists(f => f.ID == routeSegment.FromRoutePointId))
					segmentsPoints.Add(orderLogic.GetRoutePoint(routeSegment.FromRoutePointId));

				if (!segmentsPoints.Exists(f => f.ID == routeSegment.ToRoutePointId))
					segmentsPoints.Add(orderLogic.GetRoutePoint(routeSegment.ToRoutePointId));
			}

			foreach (var point in segmentsPoints)
			{
				//if (point.RoutePointTypeId == 1 || point.RoutePointTypeId == 3) // Пункт загрузки || Пункт разгрузки
				//{
				if (!string.IsNullOrEmpty(segmentsRoute))
					segmentsRoute = segmentsRoute + "; \n";

				var type = dataLogic.GetRoutePointTypes().First(w => w.ID == point.RoutePointTypeId);
				var place = dataLogic.GetPlace(point.PlaceId.Value);
				var country = dataLogic.GetCountry(place.CountryId.Value);
				segmentsRoute = segmentsRoute + type.Display + ": " + country.Name + ", " + place.Name + ", " + point.Address;
				//}

				if (point.RoutePointTypeId == 5)    // Экспортное ТО
				{
					if (segmentsExpTo != "")
						segmentsExpTo = segmentsExpTo + "; ";

					segmentsExpTo = segmentsExpTo + point.Address;
				}

				if (point.RoutePointTypeId == 6)    // Импортное ТО
				{
					if (segmentsImpTo != "")
						segmentsImpTo = segmentsImpTo + "; ";

					segmentsImpTo = segmentsImpTo + point.Address;
				}
			}

			var filename = HttpContext.Current.Server.MapPath("~\\Temp\\" + entity.Filename + ".xlsx");
			DownloadTemplateData(entity.TemplateId.Value, filename);

			#region process

			using (var xl = new ExcelPackage(new IO.FileInfo(filename)))
			{
				xl.DoAdjustDrawings = false;
				xl.Workbook.CalcMode = ExcelCalcMode.Automatic;

				var sheet = xl.Workbook.Worksheets[1];
				var col = 17;
				var row = 1;
				sheet.Cells[row++, col].Value = accounting.Number; // order.RequestNumber;
				sheet.Cells[row++, col].Value = FormatDate(accounting.RequestDate);
				sheet.Cells[row++, col].Value = contract.Number;
				sheet.Cells[row++, col].Value = FormatDate(contract.Date);
				sheet.Cells[row++, col].Value = ourDirectorPerson.DisplayName;
				sheet.Cells[row++, col].Value = ourDirectorPerson.GenitiveFamily + " " + ourDirectorPerson.Initials;
				sheet.Cells[row++, col].Value = ourDirector.Position;
				sheet.Cells[row++, col].Value = ourDirector.GenitivePosition;
				sheet.Cells[row++, col].Value = ourDirector.Basis;
				sheet.Cells[row++, col].Value = directorPerson.DisplayName;
				sheet.Cells[row++, col].Value = directorPerson.GenitiveFamily + " " + directorPerson.Initials;
				sheet.Cells[row++, col].Value = director.Position;
				sheet.Cells[row++, col].Value = director.GenitivePosition;
				sheet.Cells[row++, col].Value = director.Basis;
				sheet.Cells[row++, col].Value = legal.DisplayName;
				sheet.Cells[row++, col].Value = legal.TIN;
				sheet.Cells[row++, col].Value = legal.KPP;
				sheet.Cells[row++, col].Value = legal.Address;
				sheet.Cells[row++, col].Value = ourLegal.DisplayName;
				sheet.Cells[row++, col].Value = ourLegal.TIN;
				sheet.Cells[row++, col].Value = ourLegal.KPP;
				sheet.Cells[row++, col].Value = ourLegal.OGRN;
				sheet.Cells[row++, col].Value = ourLegal.Address;
				sheet.Cells[row++, col].Value = contractRoles.First(w => w.ID == contract.OurContractRoleId).Display;
				sheet.Cells[row++, col].Value = contractRoles.First(w => w.ID == contract.OurContractRoleId).AblativeName;
				sheet.Cells[row++, col].Value = contractRoles.First(w => w.ID == contract.ContractRoleId).Display;
				sheet.Cells[row++, col].Value = contractRoles.First(w => w.ID == contract.ContractRoleId).DativeName;
				sheet.Cells[row++, col].Value = order.Danger + " ";
				sheet.Cells[row++, col].Value = order.SpecialCustody + " ";
				sheet.Cells[row++, col].Value = order.TemperatureRegime + " ";
				sheet.Cells[row++, col].Value = segmentsRoute + " ";
				sheet.Cells[row++, col].Value = FormatDate(fromPoint.PlanDate);
				sheet.Cells[row++, col].Value = FormatDate(toPoint.PlanDate);
				sheet.Cells[row++, col].Value = order.Comment + " ";
				sheet.Cells[row++, col].Value = segmentsExpTo + " ";
				sheet.Cells[row++, col].Value = segmentsImpTo + " ";
				sheet.Cells[row++, col].Value = order.Cost;
				sheet.Cells[row++, col].Value = fromPointLegal.DisplayName;
				sheet.Cells[row++, col].Value = fromPointLegal.TIN;
				sheet.Cells[row++, col].Value = fromPointLegal.WorkTime + " ";
				sheet.Cells[row++, col].Value = fromPointContact.Name;
				sheet.Cells[row++, col].Value = fromPointContact.Phones + " ";
				sheet.Cells[row++, col].Value = fromPoint.ParticipantComment + " ";
				sheet.Cells[row++, col].Value = toPointLegal.DisplayName;
				sheet.Cells[row++, col].Value = toPointLegal.TIN;
				sheet.Cells[row++, col].Value = toPointLegal.WorkTime + " ";
				sheet.Cells[row++, col].Value = toPointContact.Name;
				sheet.Cells[row++, col].Value = toPointContact.Phones + " ";
				sheet.Cells[row++, col].Value = toPoint.ParticipantComment + " ";
				sheet.Cells[row++, col].Value = accounting.OriginalSum;
				sheet.Cells[row++, col].Value = currencies.First(w => w.ID == currencyId).Display;
				sheet.Cells[row++, col].Value = (currencyId > 1) ? "Оплата по курсу " + currencyRateUses.First(w => w.ID == (contract.CurrencyRateUseId ?? 1)).Display : " ";

				// строки
				bool isFirst = true;
				int srow = 15;  // 15 строка
				int seatTotal = 0;
				double seatTotalVolume = 0;
				double seatTotalWeight = 0;
				foreach (var seat in seats)
				{
					if (!isFirst)
					{
						sheet.InsertRow(srow, 1);
						var r1 = sheet.Cells["A" + srow + ":" + "N" + srow];
						var r2 = sheet.Cells["A" + (srow - 1) + ":" + "N" + (srow - 1)];
						r2.Copy(r1);
						sheet.Row(41).Height = 1;
					}

					sheet.Cells[srow, 2].Value = cargoDescriptions.First(w => w.ID == seat.CargoDescriptionId).Display;
					sheet.Cells[srow, 5].Value = seat.SeatCount;
					sheet.Cells[srow, 6].Value = seat.Length;
					sheet.Cells[srow, 7].Value = seat.Width;
					sheet.Cells[srow, 8].Value = seat.Height;
					sheet.Cells[srow, 9].Value = seat.Volume;
					sheet.Cells[srow, 10].Value = seat.GrossWeight;
					sheet.Cells[srow, 12].Value = packageTypes.First(w => w.ID == seat.PackageTypeId).Display;

					sheet.Row(srow).Height = cargoDescriptions.First(w => w.ID == seat.CargoDescriptionId).Display.Length > 50 ? 26 : 15;
					isFirst = false;
					srow++;
					seatTotal += seat.SeatCount ?? 0;
					seatTotalVolume += seat.Volume ?? 0;
					seatTotalWeight += seat.GrossWeight ?? 0;
				}

				sheet.Cells[srow, 5].Value = seatTotal;
				sheet.Cells[srow, 9].Value = seatTotalVolume;
				sheet.Cells[srow, 10].Value = seatTotalWeight;

				// вычисление значений во всех ячейках
				var allCells = sheet.Cells;
				foreach (var item in allCells)
					item.Calculate();

				xl.Save();
			}

			#endregion

			try
			{
				SpreadsheetInfo.SetLicense("FREE-LIMITED-KEY");
				var culture = Thread.CurrentThread.CurrentCulture;
				Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("ru-RU");
				ExcelFile ef = ExcelFile.Load(filename, LoadOptions.XlsxDefault);
				var pdfFilename = HttpContext.Current.Server.MapPath("~\\Temp\\" + entity.Filename + ".pdf");
				ef.Save(pdfFilename);
				Thread.CurrentThread.CurrentCulture = culture;

				switch (format)
				{
					case TemplatedDocumentFormat.Pdf:
						UploadTemplatedDocumentData(entity.ID, filename, "xlsx");
						UploadTemplatedDocumentData(entity.ID, pdfFilename, "pdf");
						break;
					case TemplatedDocumentFormat.CleanPdf:
						UploadTemplatedDocumentData(entity.ID, pdfFilename, "cleanpdf");
						break;
					case TemplatedDocumentFormat.CutPdf:
						UploadTemplatedDocumentData(entity.ID, pdfFilename, "cutpdf");
						break;
				}
			}
			catch { }

			return string.Empty;
		}

		public string GenerateEnRequest(TemplatedDocument entity, TemplatedDocumentFormat format)
		{
			var accounting = accountingLogic.GetAccounting(entity.AccountingId.Value);
			var order = orderLogic.GetOrder(accounting.OrderId);
			var contract = contractLogic.GetContract(accounting.IsIncome ? order.ContractId.Value : accounting.ContractId.Value);
			var legal = legalLogic.GetLegal(contract.LegalId);
			var ourLegal = legalLogic.GetLegalByOurLegal(contract.OurLegalId);
			var ourDirector = employeeLogic.GetEmployee(ourLegal.DirectorId.Value);
			var ourDirectorPerson = personLogic.GetPerson(ourDirector.PersonId.Value);
			if (!legal.DirectorId.HasValue)
				return "Не указан подписант клиента";

			var director = employeeLogic.GetEmployee(legal.DirectorId.Value);
			var directorPerson = personLogic.GetPerson(director.PersonId.Value);
			var fromPoint = orderLogic.GetRoutePoints(order.ID).OrderBy(o => o.No).FirstOrDefault();
			if (fromPoint == null)
				return "Нет маршрутных точек";

			if (!fromPoint.ParticipantLegalId.HasValue)
				return "Не указан ГО";

			var fromPointLegal = legalLogic.GetLegal(fromPoint.ParticipantLegalId.Value);
			var fromPointContact = legalLogic.GetRouteContact(fromPoint.RouteContactID.Value);
			var toPoint = orderLogic.GetRoutePoints(order.ID).OrderBy(o => o.No).Last();
			if (!toPoint.ParticipantLegalId.HasValue)
				return "Не указан ГП";

			var toPointLegal = legalLogic.GetLegal(toPoint.ParticipantLegalId.Value);
			var toPointContact = legalLogic.GetRouteContact(toPoint.RouteContactID.Value);
			var seats = orderLogic.GetCargoSeats(order.ID);
			var contractRoles = dataLogic.GetContractRoles();
			var cargoDescriptions = dataLogic.GetCargoDescriptions();
			var packageTypes = dataLogic.GetPackageTypes();
			var services = accountingLogic.GetServicesByAccounting(accounting.ID);
			if (services.Count() == 0)
				return "Нет услуг";

			var serviceCurrencyId = services.First().CurrencyId ?? 1;
			var currencies = dataLogic.GetCurrencies();
			var contractCurrencies = contractLogic.GetContractCurrencies(contract.ID);
			var currency = contractCurrencies.FirstOrDefault(w => w.CurrencyId == serviceCurrencyId);
			if (currency == null)
				currency = contractCurrencies.FirstOrDefault(w => w.CurrencyId == 1);   // рубли присутствуют обязательно в этом случае

			var ourBankAccount = bankLogic.GetBankAccount(currency.OurBankAccountId);

			var filename = HttpContext.Current.Server.MapPath("~\\Temp\\" + entity.Filename + ".xlsx");

			DownloadTemplateData(entity.TemplateId.Value, filename);

			#region process

			using (var xl = new ExcelPackage(new IO.FileInfo(filename)))
			{
				xl.DoAdjustDrawings = false;
				xl.Workbook.CalcMode = ExcelCalcMode.Automatic;

				var sheet = xl.Workbook.Worksheets[1];
				var col = 32;
				var row = 1;
				sheet.Cells[row++, col].Value = order.RequestNumber;
				sheet.Cells[row++, col].Value = FormatDate(order.RequestDate);
				sheet.Cells[row++, col].Value = contract.Number;
				sheet.Cells[row++, col].Value = FormatDate(contract.Date);
				sheet.Cells[row++, col].Value = ourDirectorPerson.DisplayName;
				sheet.Cells[row++, col].Value = ourDirector.Position;
				sheet.Cells[row++, col].Value = ourDirector.GenitivePosition;
				sheet.Cells[row++, col].Value = ourDirector.Basis;
				sheet.Cells[row++, col].Value = directorPerson.DisplayName;
				sheet.Cells[row++, col].Value = director.Position;
				sheet.Cells[row++, col].Value = director.GenitivePosition;
				sheet.Cells[row++, col].Value = director.Basis;
				sheet.Cells[row++, col].Value = legal.DisplayName;
				sheet.Cells[row++, col].Value = legal.TIN + " ";
				sheet.Cells[row++, col].Value = legal.KPP + " ";
				sheet.Cells[row++, col].Value = legal.Address;
				sheet.Cells[row++, col].Value = ourLegal.DisplayName;
				sheet.Cells[row++, col].Value = ourLegal.TIN + " ";
				sheet.Cells[row++, col].Value = ourLegal.KPP + " ";
				sheet.Cells[row++, col].Value = ourLegal.OGRN;
				sheet.Cells[row++, col].Value = ourLegal.Address;
				sheet.Cells[row++, col].Value = contractRoles.First(w => w.ID == contract.OurContractRoleId).Display;
				sheet.Cells[row++, col].Value = contractRoles.First(w => w.ID == contract.OurContractRoleId).AblativeName;
				sheet.Cells[row++, col].Value = contractRoles.First(w => w.ID == contract.ContractRoleId).Display;
				sheet.Cells[row++, col].Value = contractRoles.First(w => w.ID == contract.ContractRoleId).DativeName;
				sheet.Cells[row++, col].Value = order.Danger + " ";
				sheet.Cells[row++, col].Value = order.SpecialCustody + " ";
				sheet.Cells[row++, col].Value = order.TemperatureRegime + " ";
				sheet.Cells[row++, col].Value = accounting.Route + " ";
				sheet.Cells[row++, col].Value = order.LoadingDate;
				sheet.Cells[row++, col].Value = order.UnloadingDate;
				sheet.Cells[row++, col].Value = order.Comment + " ";
				sheet.Cells[row++, col].Value = order.zkzExpTO + " ";
				sheet.Cells[row++, col].Value = order.zkzImpTO + " ";
				sheet.Cells[row++, col].Value = order.Cost;
				sheet.Cells[row++, col].Value = fromPointLegal.DisplayName + " ";
				sheet.Cells[row++, col].Value = fromPointLegal.TIN + " ";
				sheet.Cells[row++, col].Value = fromPointLegal.WorkTime + " ";
				sheet.Cells[row++, col].Value = fromPointContact.Name + " ";
				sheet.Cells[row++, col].Value = fromPointContact.Phones + " ";
				sheet.Cells[row++, col].Value = fromPoint.ParticipantComment + " ";
				sheet.Cells[row++, col].Value = toPointLegal.DisplayName + " ";
				sheet.Cells[row++, col].Value = toPointLegal.TIN + " ";
				sheet.Cells[row++, col].Value = toPointLegal.WorkTime + " ";
				sheet.Cells[row++, col].Value = toPointContact.Contact + " ";
				sheet.Cells[row++, col].Value = toPointContact.Phones + " ";
				sheet.Cells[row++, col].Value = toPoint.ParticipantComment + " ";
				sheet.Cells[row++, col].Value = ourDirector.EnPosition;
				sheet.Cells[row++, col].Value = ourDirectorPerson.EnName;
				sheet.Cells[row++, col].Value = director.EnPosition;
				sheet.Cells[row++, col].Value = directorPerson.EnName;
				sheet.Cells[row++, col].Value = legal.EnName;
				sheet.Cells[row++, col].Value = legal.EnAddress;
				sheet.Cells[row++, col].Value = ourLegal.EnName;
				sheet.Cells[row++, col].Value = ourLegal.EnAddress;
				sheet.Cells[row++, col].Value = contractRoles.First(w => w.ID == contract.OurContractRoleId).EnName;
				sheet.Cells[row++, col].Value = contractRoles.First(w => w.ID == contract.ContractRoleId).EnName;
				sheet.Cells[row++, col].Value = order.EnDanger + " ";
				sheet.Cells[row++, col].Value = order.EnSpecialCustody + " ";
				sheet.Cells[row++, col].Value = order.EnTemperatureRegime + " ";
				sheet.Cells[row++, col].Value = accounting.ecnMarshrutEN + " ";
				sheet.Cells[row++, col].Value = accounting.ecnExportTOEN + " ";
				sheet.Cells[row++, col].Value = accounting.ecnImportTOEN + " ";
				sheet.Cells[row++, col].Value = accounting.ecnGOcontactEN + " ";
				sheet.Cells[row++, col].Value = accounting.ecnGOnameINNEN + " ";
				sheet.Cells[row++, col].Value = accounting.ecnGOtimeEN + " ";
				sheet.Cells[row++, col].Value = accounting.ecnGOcommentEN + " ";
				sheet.Cells[row++, col].Value = accounting.strGOcontTelEN + " ";
				sheet.Cells[row++, col].Value = accounting.ecnGPcontactEN + " ";
				sheet.Cells[row++, col].Value = accounting.ecnGPnameINNEN + " ";
				sheet.Cells[row++, col].Value = accounting.ecnGPtimeEN + " ";
				sheet.Cells[row++, col].Value = accounting.ecnGPcommentEN + " ";
				sheet.Cells[row++, col].Value = accounting.strGPcontTelEN + " ";
				sheet.Cells[row++, col].Value = accounting.ecnKommentEN + " ";
				sheet.Cells[row++, col].Value = (currency.CurrencyId == 1) ? accounting.Sum : accounting.OriginalSum;
				sheet.Cells[row++, col].Value = currencies.First(w => w.ID == currency.CurrencyId).Display;

				// строки
				bool isFirst = true;
				int srow = 15;  // 15 строка
				int seatTotal = 0;
				double seatTotalVolume = 0;
				double seatTotalWeight = 0;
				foreach (var seat in seats)
				{
					if (!isFirst)
					{
						sheet.InsertRow(srow, 1);
						var r1 = sheet.Cells["A" + srow + ":" + "AC" + srow];
						var r2 = sheet.Cells["A" + (srow - 1) + ":" + "AC" + (srow - 1)];
						r2.Copy(r1);
						sheet.Row(66).Height = 1;
					}

					// ru
					sheet.Cells[srow, 2].Value = cargoDescriptions.First(w => w.ID == seat.CargoDescriptionId).Display;
					sheet.Cells[srow, 5].Value = seat.SeatCount;
					sheet.Cells[srow, 6].Value = seat.Length;
					sheet.Cells[srow, 7].Value = seat.Width;
					sheet.Cells[srow, 8].Value = seat.Height;
					sheet.Cells[srow, 9].Value = seat.Volume;
					sheet.Cells[srow, 10].Value = seat.GrossWeight;
					sheet.Cells[srow, 12].Value = packageTypes.First(w => w.ID == seat.PackageTypeId).Display;
					// en
					sheet.Cells[srow, 16].Value = cargoDescriptions.First(w => w.ID == seat.CargoDescriptionId).EnDisplay;
					sheet.Cells[srow, 19].Value = seat.SeatCount;
					sheet.Cells[srow, 20].Value = seat.Length;
					sheet.Cells[srow, 21].Value = seat.Width;
					sheet.Cells[srow, 22].Value = seat.Height;
					sheet.Cells[srow, 23].Value = seat.Volume;
					sheet.Cells[srow, 24].Value = seat.GrossWeight;
					sheet.Cells[srow, 26].Value = packageTypes.First(w => w.ID == seat.PackageTypeId).EnDisplay;

					sheet.Row(srow).Height = cargoDescriptions.First(w => w.ID == seat.CargoDescriptionId).Display.Length > 50 ? 26 : 15;
					isFirst = false;
					srow++;
					seatTotal += seat.SeatCount ?? 0;
					seatTotalVolume += seat.Volume ?? 0;
					seatTotalWeight += seat.GrossWeight ?? 0;
				}

				sheet.Cells[srow, 5].Value = seatTotal;
				sheet.Cells[srow, 9].Value = seatTotalVolume;
				sheet.Cells[srow, 10].Value = seatTotalWeight;

				// вычисление значений во всех ячейках
				var allCells = sheet.Cells;
				foreach (var item in allCells)
					item.Calculate();

				xl.Save();
			}

			#endregion

			try
			{
				SpreadsheetInfo.SetLicense("FREE-LIMITED-KEY");
				var culture = Thread.CurrentThread.CurrentCulture;
				Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("ru-RU");
				ExcelFile ef = ExcelFile.Load(filename, LoadOptions.XlsxDefault);
				var pdfFilename = HttpContext.Current.Server.MapPath("~\\Temp\\" + entity.Filename + ".pdf");
				ef.Save(pdfFilename);
				Thread.CurrentThread.CurrentCulture = culture;

				switch (format)
				{
					case TemplatedDocumentFormat.Pdf:
						UploadTemplatedDocumentData(entity.ID, filename, "xlsx");
						UploadTemplatedDocumentData(entity.ID, pdfFilename, "pdf");
						break;
					case TemplatedDocumentFormat.CleanPdf:
						UploadTemplatedDocumentData(entity.ID, pdfFilename, "cleanpdf");
						break;
					case TemplatedDocumentFormat.CutPdf:
						UploadTemplatedDocumentData(entity.ID, pdfFilename, "cutpdf");
						break;
				}
			}
			catch { }

			return string.Empty;
		}

		public string GenerateInsurance(TemplatedDocument entity, TemplatedDocumentFormat format)
		{
			var accounting = accountingLogic.GetAccounting(entity.AccountingId.Value);
			if (!accounting.InvoiceDate.HasValue)
				return "Не указана дата счета";

			var order = orderLogic.GetOrder(accounting.OrderId);
			if (string.IsNullOrEmpty(order.InvoiceNumber))
				return "Не указан номер инвойса";

			if (!order.InvoiceDate.HasValue)
				return "Не указана дата инвойса";

			if (!order.InvoiceSum.HasValue)
				return "Не указана сумма инвойса";

			var seats = orderLogic.GetCargoSeats(order.ID);
			foreach (var item in seats)
			{
				if (!item.SeatCount.HasValue)
					return "Не указано количество грузовых мест";

				if (!item.CargoDescriptionId.HasValue)
					return "Не указано наименование груза";

				if (!item.GrossWeight.HasValue)
					return "Не указан брутто вес";
			}

			var package = "";
			var packages = seats.Select(s => s.PackageTypeId ?? 0).Distinct();
			foreach (var item in packages)
				package += dataLogic.GetPackageType(item).Display + "; ";

			var contract = contractLogic.GetContract(accounting.ContractId ?? order.ContractId.Value);
			var orderContract = contractLogic.GetContract(order.ContractId.Value);
			var legal = legalLogic.GetLegal(orderContract.LegalId);
			var ourLegal = legalLogic.GetLegalByOurLegal(orderContract.OurLegalId);

			var ourDirector = employeeLogic.GetEmployee(ourLegal.DirectorId.Value);
			var ourDirectorPerson = personLogic.GetPerson(ourDirector.PersonId.Value);
			if (!legal.DirectorId.HasValue)
				return "Не указан подписант клиента";

			if (!accounting.CargoLegalId.HasValue)
				return "Не указан грузоперевозчик";

			var cargoLegal = legalLogic.GetLegal(accounting.CargoLegalId.Value);

			RoutePoint fromPoint = null;
			RoutePoint toPoint = null;
			List<RouteSegment> segments = new List<RouteSegment>();
			var selectedSegments = accountingLogic.GetAccountingRouteSegments(accounting.ID);
			if (selectedSegments.Count() != 0)
			{
				var fromSegment = orderLogic.GetRouteSegment(selectedSegments.First().RouteSegmentId.Value);
				fromPoint = orderLogic.GetRoutePoint(fromSegment.FromRoutePointId);
				var toSegment = orderLogic.GetRouteSegment(selectedSegments.Last().RouteSegmentId.Value);
				toPoint = orderLogic.GetRoutePoint(toSegment.ToRoutePointId);

				foreach (var ss in selectedSegments)
					segments.Add(orderLogic.GetRouteSegment(ss.RouteSegmentId.Value));
			}
			else
			{
				var points = orderLogic.GetRoutePoints(order.ID);
				if (points.Count() < 2)
					return "Недостаточно маршрутных точек";

				fromPoint = points.First();
				toPoint = points.Last();
				segments = orderLogic.GetRouteSegments(order.ID).ToList();
			}

			if (fromPoint == null)
				return "Нет маршрутных точек";

			if (!fromPoint.ParticipantLegalId.HasValue)
				return "Не указан ГО (в первой точке)";

			if (!toPoint.ParticipantLegalId.HasValue)
				return "Не указан ГП (в последней точке)";

			var transportTypes = dataLogic.GetTransportTypes();
			var transport = "";
			foreach (var item in segments.Where(w => w.TransportTypeId.HasValue).Select(s => s.TransportTypeId).Distinct())
				transport += transportTypes.First(w => w.ID == item).Display + ";";

			var transportNumbers = "";
			foreach (var item in segments.Where(w => w.TransportTypeId.HasValue).Select(s => s.VehicleNumber).Distinct())
				transportNumbers += item + ";";

			var currencies = dataLogic.GetCurrencies();
			var services = accountingLogic.GetServicesByAccounting(accounting.ID);
			if (services.Count() == 0)
				return "Нет услуг";

			var currencyId = order.InvoiceCurrencyId;   //services.First().CurrencyId ?? 1;
			var rate = dataLogic.GetCurrencyRates(accounting.InvoiceDate.Value).Where(w => w.CurrencyId == currencyId).FirstOrDefault();
			var filename = HttpContext.Current.Server.MapPath("~\\Temp\\" + entity.Filename + ".xlsx");

			DownloadTemplateData(entity.TemplateId.Value, filename);

			#region process

			using (var xl = new ExcelPackage(new IO.FileInfo(filename)))
			{
				xl.DoAdjustDrawings = false;
				xl.Workbook.CalcMode = ExcelCalcMode.Automatic;

				var sheet = xl.Workbook.Worksheets[1];
				var col = 12;
				var row = 1;
				sheet.Cells[row++, col].Value = accounting.Number;
				sheet.Cells[row++, col].Value = FormatDate(order.RequestDate);
				sheet.Cells[row++, col].Value = FormatDate(order.LoadingDate);
				sheet.Cells[row++, col].Value = FormatDate(order.UnloadingDate);
				sheet.Cells[row++, col].Value = order.Cost;
				sheet.Cells[row++, col].Value = order.CargoInfo + " ";
				sheet.Cells[row++, col].Value = order.SeatsCount;
				sheet.Cells[row++, col].Value = order.GrossWeight;
				sheet.Cells[row++, col].Value = order.InvoiceNumber + " ";
				sheet.Cells[row++, col].Value = FormatDate(order.InvoiceDate);
				sheet.Cells[row++, col].Value = order.InvoiceSum;
				sheet.Cells[row++, col].Value = order.zkzMarshrut + " ";
				sheet.Cells[row++, col].Value = order.zkzGOname + " ";
				sheet.Cells[row++, col].Value = order.zkzGPname + " ";
				sheet.Cells[row++, col].Value = package + " ";
				sheet.Cells[row++, col].Value = contract.Number + " ";
				sheet.Cells[row++, col].Value = FormatDate(contract.Date);
				sheet.Cells[row++, col].Value = currencies.First(w => w.ID == currencyId).Display;
				sheet.Cells[row++, col].Value = (rate != null) ? rate.Rate : 1;
				sheet.Cells[row++, col].Value = ourDirectorPerson.DisplayName + " ";
				sheet.Cells[row++, col].Value = ourDirector.Position + " ";
				sheet.Cells[row++, col].Value = ourLegal.DisplayName + " ";
				sheet.Cells[row++, col].Value = ourLegal.TIN + " ";
				sheet.Cells[row++, col].Value = ourLegal.KPP + " ";
				sheet.Cells[row++, col].Value = legal.DisplayName + " ";
				sheet.Cells[row++, col].Value = legal.TIN + " ";
				sheet.Cells[row++, col].Value = transport + " ";
				sheet.Cells[row++, col].Value = transportNumbers + " ";
				sheet.Cells[row++, col].Value = accounting.Route;
				sheet.Cells[row++, col].Value = order.Comment + " ";
				sheet.Cells[row++, col].Value = cargoLegal.DisplayName + " ";
				sheet.Cells[row++, col].Value = FormatDate(DateTime.Now);
				sheet.Cells[row++, col].Value = !string.IsNullOrWhiteSpace(order.TemperatureRegime);    // реф риски

				// вычисление значений во всех ячейках
				var allCells = sheet.Cells;
				foreach (var item in allCells)
					item.Calculate();

				xl.Save();
			}

			#endregion

			try
			{
				SpreadsheetInfo.SetLicense("FREE-LIMITED-KEY");
				var culture = Thread.CurrentThread.CurrentCulture;
				Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("ru-RU");
				ExcelFile ef = ExcelFile.Load(filename, LoadOptions.XlsxDefault);
				var pdfFilename = HttpContext.Current.Server.MapPath("~\\Temp\\" + entity.Filename + ".pdf");
				ef.Save(pdfFilename);
				Thread.CurrentThread.CurrentCulture = culture;

				switch (format)
				{
					case TemplatedDocumentFormat.Pdf:
						UploadTemplatedDocumentData(entity.ID, filename, "xlsx");
						UploadTemplatedDocumentData(entity.ID, pdfFilename, "pdf");
						break;
					case TemplatedDocumentFormat.CleanPdf:
						UploadTemplatedDocumentData(entity.ID, pdfFilename, "cleanpdf");
						break;
					case TemplatedDocumentFormat.CutPdf:
						UploadTemplatedDocumentData(entity.ID, pdfFilename, "cutpdf");
						break;
				}
			}
			catch { }

			return string.Empty;
		}

		public string GenerateDetails(TemplatedDocument entity, TemplatedDocumentFormat format)
		{
			var accounting = accountingLogic.GetAccounting(entity.AccountingId.Value);
			if (!accounting.IsIncome)
				return "Для РАСХОДА детализация не формируются.";

			if (!accounting.LegalId.HasValue)
				return "Не заполнено Юрлицо в доходе";

			var order = orderLogic.GetOrder(accounting.OrderId);
			var contract = contractLogic.GetContract(order.ContractId.Value);
			var legal = legalLogic.GetLegal(accounting.LegalId.Value);
			var ourLegal = legalLogic.GetLegalByOurLegal(accounting.OurLegalId.Value);
			var director = employeeLogic.GetEmployee(ourLegal.DirectorId.Value);
			var directorPerson = personLogic.GetPerson(director.PersonId.Value);
			var accountant = employeeLogic.GetEmployee(ourLegal.AccountantId.Value);
			var accountantPerson = personLogic.GetPerson(accountant.PersonId.Value);
			var docs = documentLogic.GetDocumentsByOrder(order.ID).Where(w => w.IsPrint == true);
			var services = accountingLogic.GetServicesByAccounting(accounting.ID);
			if (services.Count() == 0)
				return "Нет услуг";

			var sum = services.Where(w => w.IsForDetalization).Sum(s => s.OriginalSum ?? 0);
			if (sum == 0)
				return "Не выбраны услуги для детализации.";

			var currencyId = services.First().CurrencyId ?? 1;
			var contractCurrencies = contractLogic.GetContractCurrencies(contract.ID);
			var currency = contractCurrencies.FirstOrDefault(w => w.CurrencyId == currencyId);
			currency = currency ?? contractCurrencies.FirstOrDefault(w => w.CurrencyId == 1);   // рубли присутствуют обязательно в этом случае
			var ourBankAccount = bankLogic.GetBankAccount(currency.OurBankAccountId);
			var ourBank = bankLogic.GetBank(ourBankAccount.BankId.Value);

			RoutePoint fromPoint = null;
			RoutePoint toPoint = null;
			var segments = accountingLogic.GetAccountingRouteSegments(accounting.ID);
			if (segments.Count() != 0)
			{
				var fromSegment = orderLogic.GetRouteSegment(segments.First().RouteSegmentId.Value);
				fromPoint = orderLogic.GetRoutePoint(fromSegment.FromRoutePointId);
				var toSegment = orderLogic.GetRouteSegment(segments.Last().RouteSegmentId.Value);
				toPoint = orderLogic.GetRoutePoint(toSegment.ToRoutePointId);
			}
			else
			{
				var points = orderLogic.GetRoutePoints(order.ID);
				fromPoint = points.First();
				toPoint = points.Last();
			}

			#region вычисление длин сегментов / сумм

			var routeSegments = orderLogic.GetRouteSegments(order.ID);
			if (segments.Count() > 0)
				routeSegments = routeSegments.Where(w => segments.Select(s => s.RouteSegmentId).ToList().Contains(w.ID)).OrderBy(o => o.No).ToList();

			double routeLengthBefore = 0;
			double routeLengthAfter = 0;
			string routeBefore = "";
			string routeAfter = "";
			foreach (var segment in routeSegments)
			{
				if (segment.IsAfterBorder)
				{
					routeLengthAfter += segment.Length ?? 0;
					if (string.IsNullOrEmpty(routeAfter))
					{
						var point = orderLogic.GetRoutePoint(segment.FromRoutePointId);
						var place = dataLogic.GetPlace(point.PlaceId.Value);
						var country = dataLogic.GetCountry(place.CountryId.Value);
						routeAfter = country.Name + ", " + place.Name;
					}

					var pointTo = orderLogic.GetRoutePoint(segment.ToRoutePointId);
					var placeTo = dataLogic.GetPlace(pointTo.PlaceId.Value);
					var countryTo = dataLogic.GetCountry(placeTo.CountryId.Value);
					routeAfter += " - " + countryTo.Name + ", " + placeTo.Name;
				}
				else
				{
					routeLengthBefore += segment.Length ?? 0;
					if (string.IsNullOrEmpty(routeBefore))
					{
						var point = orderLogic.GetRoutePoint(segment.FromRoutePointId);
						var place = dataLogic.GetPlace(point.PlaceId.Value);
						var country = dataLogic.GetCountry(place.CountryId.Value);
						routeBefore = country.Name + ", " + place.Name;
					}

					var pointTo = orderLogic.GetRoutePoint(segment.ToRoutePointId);
					var placeTo = dataLogic.GetPlace(pointTo.PlaceId.Value);
					var countryTo = dataLogic.GetCountry(placeTo.CountryId.Value);
					routeBefore += " - " + countryTo.Name + ", " + placeTo.Name;
				}
			}

			#endregion

			var filename = HttpContext.Current.Server.MapPath("~\\Temp\\" + entity.Filename + ".xlsx");

			// справочники
			var docTypes = dataLogic.GetDocumentTypes();
			var currencies = dataLogic.GetCurrencies();
			DownloadTemplateData(entity.TemplateId.Value, filename);

			#region process

			using (var xl = new ExcelPackage(new IO.FileInfo(filename)))
			{
				xl.DoAdjustDrawings = false;
				xl.Workbook.CalcMode = ExcelCalcMode.Automatic;

				var sheet = xl.Workbook.Worksheets[1];
				var col = 14;
				var row = 1;
				sheet.Cells[row++, col].Value = accounting.InvoiceNumber;
				sheet.Cells[row++, col].Value = FormatDate(accounting.InvoiceDate);
				sheet.Cells[row++, col].Value = accounting.OriginalSum;
				sheet.Cells[row++, col].Value = accounting.OriginalVat;
				//sheet.Cells[row++, col].Value = accountantPerson.DisplayName;
				//sheet.Cells[row++, col].Value = directorPerson.DisplayName;
				sheet.Cells[row++, col].Value = order.Number;
				sheet.Cells[row++, col].Value = order.RequestNumber;
				sheet.Cells[row++, col].Value = FormatDate(order.LoadingDate);
				sheet.Cells[row++, col].Value = order.SeatsCount;
				sheet.Cells[row++, col].Value = order.GrossWeight;
				//sheet.Cells[row++, col].Value = order.PaidWeight;
				sheet.Cells[row++, col].Value = order.InvoiceSum;
				sheet.Cells[row++, col].Value = currencies.First(w => w.ID == order.InvoiceCurrencyId).Display;
				//sheet.Cells[row++, col].Value = order.VehicleNumbers + " ";
				sheet.Cells[row++, col].Value = FormatPlace(fromPoint.PlaceId.Value) + " ";
				sheet.Cells[row++, col].Value = FormatPlace(toPoint.PlaceId.Value) + " ";
				sheet.Cells[row++, col].Value = routeBefore + " ";
				sheet.Cells[row++, col].Value = routeAfter + " ";
				sheet.Cells[row++, col].Value = Math.Round(sum / (routeLengthAfter + routeLengthBefore) * routeLengthBefore, 2);
				sheet.Cells[row++, col].Value = Math.Round(sum / (routeLengthAfter + routeLengthBefore) * routeLengthAfter, 2);
				sheet.Cells[row++, col].Value = contract.Number;
				sheet.Cells[row++, col].Value = FormatDate(contract.Date);
				sheet.Cells[row++, col].Value = legal.DisplayName;
				sheet.Cells[row++, col].Value = legal.TIN;
				sheet.Cells[row++, col].Value = legal.KPP;
				sheet.Cells[row++, col].Value = legal.Address;
				sheet.Cells[row++, col].Value = legal.PostAddress;
				//sheet.Cells[row++, col].Value = ourLegal.DisplayName;
				//sheet.Cells[row++, col].Value = ourLegal.TIN;
				//sheet.Cells[row++, col].Value = ourLegal.KPP;
				//sheet.Cells[row++, col].Value = ourLegal.OGRN;
				//sheet.Cells[row++, col].Value = ourLegal.Address;
				//sheet.Cells[row++, col].Value = ourBankAccount.Number;
				sheet.Cells[row++, col].Value = ourBank.Name;
				sheet.Cells[row++, col].Value = ourBank.BIC;
				sheet.Cells[row++, col].Value = ourBank.KSNP;
				sheet.Cells[row++, col].Value = currencies.First(w => w.ID == currencyId).Display;

				// HACK:
				if (!order.InvoiceSum.HasValue)
					sheet.Cells["E12"].Value = "";

				// документы
				foreach (var doc in docs)
				{
					sheet.Cells[row++, col].Value = docTypes.Where(w => w.ID == doc.DocumentTypeId).Select(s => s.Display).FirstOrDefault() + " № :";
					sheet.Cells[row++, col].Value = doc.Number;
				}

				// вычисление значений во всех ячейках
				var allCells = sheet.Cells;
				foreach (var item in allCells)
					item.Calculate();

				xl.Save();
			}

			#endregion

			try
			{
				SpreadsheetInfo.SetLicense("FREE-LIMITED-KEY");
				var culture = Thread.CurrentThread.CurrentCulture;
				Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("ru-RU");
				ExcelFile ef = ExcelFile.Load(filename, LoadOptions.XlsxDefault);
				var pdfFilename = HttpContext.Current.Server.MapPath("~\\Temp\\" + entity.Filename + ".pdf");
				ef.Save(pdfFilename);
				Thread.CurrentThread.CurrentCulture = culture;

				switch (format)
				{
					case TemplatedDocumentFormat.Pdf:
						UploadTemplatedDocumentData(entity.ID, filename, "xlsx");
						UploadTemplatedDocumentData(entity.ID, pdfFilename, "pdf");
						break;
					case TemplatedDocumentFormat.CleanPdf:
						UploadTemplatedDocumentData(entity.ID, pdfFilename, "cleanpdf");
						break;
					case TemplatedDocumentFormat.CutPdf:
						UploadTemplatedDocumentData(entity.ID, pdfFilename, "cutpdf");
						break;
				}
			}
			catch { }

			return string.Empty;
		}

		public string GenerateAmpleDetails(TemplatedDocument entity, TemplatedDocumentFormat format)
		{
			var accounting = accountingLogic.GetAccounting(entity.AccountingId.Value);
			if (!accounting.LegalId.HasValue)
				return "Не заполнено Юрлицо в доходе";

			var order = orderLogic.GetOrder(accounting.OrderId);
			var contract = contractLogic.GetContract(order.ContractId.Value);
			var legal = legalLogic.GetLegal(accounting.LegalId.Value);
			var ourLegal = legalLogic.GetLegalByOurLegal(accounting.OurLegalId.Value);
			var director = employeeLogic.GetEmployee(ourLegal.DirectorId.Value);
			var directorPerson = personLogic.GetPerson(director.PersonId.Value);
			var accountant = employeeLogic.GetEmployee(ourLegal.AccountantId.Value);
			var accountantPerson = personLogic.GetPerson(accountant.PersonId.Value);
			var docs = documentLogic.GetDocumentsByOrder(order.ID).Where(w => w.IsPrint == true);
			var services = accountingLogic.GetServicesByAccounting(accounting.ID);
			if (services.Count() == 0)
				return "Нет услуг";

			var currencyId = services.First().CurrencyId ?? 1;
			var contractCurrencies = contractLogic.GetContractCurrencies(contract.ID);
			var currency = contractCurrencies.FirstOrDefault(w => w.CurrencyId == currencyId);
			currency = currency ?? contractCurrencies.FirstOrDefault(w => w.CurrencyId == 1);   // рубли присутствуют обязательно в этом случае
			var ourBankAccount = bankLogic.GetBankAccount(currency.OurBankAccountId);
			var ourBank = bankLogic.GetBank(ourBankAccount.BankId.Value);

			RoutePoint fromPoint = null;
			RoutePoint toPoint = null;
			var segments = accountingLogic.GetAccountingRouteSegments(accounting.ID);
			if (segments.Count() != 0)
			{
				var fromSegment = orderLogic.GetRouteSegment(segments.First().RouteSegmentId.Value);
				fromPoint = orderLogic.GetRoutePoint(fromSegment.FromRoutePointId);
				var toSegment = orderLogic.GetRouteSegment(segments.Last().RouteSegmentId.Value);
				toPoint = orderLogic.GetRoutePoint(toSegment.ToRoutePointId);
			}
			else
			{
				var points = orderLogic.GetRoutePoints(order.ID);
				fromPoint = points.First();
				toPoint = points.Last();
			}

			#region вычисление длин сегментов / сумм

			var routeSegments = orderLogic.GetRouteSegments(order.ID);
			if (segments.Count() > 0)
				routeSegments = routeSegments.Where(w => segments.Select(s => s.RouteSegmentId).ToList().Contains(w.ID)).OrderBy(o => o.No).ToList();

			double routeLengthBefore = 0;
			double routeLengthAfter = 0;
			string routeBefore = "";
			string routeAfter = "";
			foreach (var segment in routeSegments)
			{
				if (segment.IsAfterBorder)
				{
					routeLengthAfter += segment.Length ?? 0;
					if (string.IsNullOrEmpty(routeAfter))
					{
						var point = orderLogic.GetRoutePoint(segment.FromRoutePointId);
						var place = dataLogic.GetPlace(point.PlaceId.Value);
						var country = dataLogic.GetCountry(place.CountryId.Value);
						routeAfter = country.Name + ", " + place.Name;
					}

					var pointTo = orderLogic.GetRoutePoint(segment.ToRoutePointId);
					var placeTo = dataLogic.GetPlace(pointTo.PlaceId.Value);
					var countryTo = dataLogic.GetCountry(placeTo.CountryId.Value);
					routeAfter += " - " + countryTo.Name + ", " + placeTo.Name;
				}
				else
				{
					routeLengthBefore += segment.Length ?? 0;
					if (string.IsNullOrEmpty(routeBefore))
					{
						var point = orderLogic.GetRoutePoint(segment.FromRoutePointId);
						var place = dataLogic.GetPlace(point.PlaceId.Value);
						var country = dataLogic.GetCountry(place.CountryId.Value);
						routeBefore = country.Name + ", " + place.Name;
					}

					var pointTo = orderLogic.GetRoutePoint(segment.ToRoutePointId);
					var placeTo = dataLogic.GetPlace(pointTo.PlaceId.Value);
					var countryTo = dataLogic.GetCountry(placeTo.CountryId.Value);
					routeBefore += " - " + countryTo.Name + ", " + placeTo.Name;
				}
			}

			#endregion

			var filename = HttpContext.Current.Server.MapPath("~\\Temp\\" + entity.Filename + ".xlsx");

			// справочники
			var docTypes = dataLogic.GetDocumentTypes();
			var currencies = dataLogic.GetCurrencies();
			var serviceTypes = dataLogic.GetServiceTypes();
			var serviceKinds = dataLogic.GetServiceKinds();
			var vats = dataLogic.GetVats();
			DownloadTemplateData(entity.TemplateId.Value, filename);

			#region process

			using (var xl = new ExcelPackage(new IO.FileInfo(filename)))
			{
				xl.DoAdjustDrawings = false;
				xl.Workbook.CalcMode = ExcelCalcMode.Automatic;

				var sheet = xl.Workbook.Worksheets[1];
				var col = 15;
				var row = 1;
				sheet.Cells[row++, col].Value = accounting.InvoiceNumber;
				sheet.Cells[row++, col].Value = FormatDate(accounting.InvoiceDate);
				sheet.Cells[row++, col].Value = accounting.OriginalSum;
				sheet.Cells[row++, col].Value = accounting.OriginalVat;
				sheet.Cells[row++, col].Value = "";
				sheet.Cells[row++, col].Value = "";
				sheet.Cells[row++, col].Value = order.Number;
				sheet.Cells[row++, col].Value = order.RequestNumber;
				sheet.Cells[row++, col].Value = FormatDate(order.LoadingDate);
				sheet.Cells[row++, col].Value = order.SeatsCount;
				sheet.Cells[row++, col].Value = order.GrossWeight;
				//sheet.Cells[row++, col].Value = order.PaidWeight;
				sheet.Cells[row++, col].Value = order.InvoiceSum;
				sheet.Cells[row++, col].Value = currencies.First(w => w.ID == order.InvoiceCurrencyId).Display;
				sheet.Cells[row++, col].Value = FormatPlace(fromPoint.PlaceId.Value) + " ";
				sheet.Cells[row++, col].Value = FormatPlace(toPoint.PlaceId.Value) + " ";
				sheet.Cells[row++, col].Value = contract.Number;
				sheet.Cells[row++, col].Value = FormatDate(contract.Date);
				sheet.Cells[row++, col].Value = legal.DisplayName;
				sheet.Cells[row++, col].Value = legal.TIN;
				sheet.Cells[row++, col].Value = legal.KPP;
				sheet.Cells[row++, col].Value = legal.Address;
				sheet.Cells[row++, col].Value = legal.PostAddress;
				sheet.Cells[row++, col].Value = "";
				sheet.Cells[row++, col].Value = "";
				sheet.Cells[row++, col].Value = "";
				sheet.Cells[row++, col].Value = "";
				sheet.Cells[row++, col].Value = "";
				sheet.Cells[row++, col].Value = "";
				sheet.Cells[row++, col].Value = "";
				sheet.Cells[row++, col].Value = "";
				sheet.Cells[row++, col].Value = "";
				sheet.Cells[row++, col].Value = "";
				sheet.Cells[row++, col].Value = currencies.First(w => w.ID == currencyId).Display;
				sheet.Cells[row++, col].Value = "";

				// HACK:
				if (string.IsNullOrWhiteSpace(order.VehicleNumbers))
					sheet.Cells["C14"].Value = "";
				if (!order.InvoiceSum.HasValue)
					sheet.Cells["C13"].Value = "";

				// документы
				foreach (var doc in docs)
				{
					sheet.Cells[row++, col].Value = docTypes.Where(w => w.ID == doc.DocumentTypeId).Select(s => s.Display).FirstOrDefault() + " № :";
					sheet.Cells[row++, col].Value = doc.Number;
				}

				// Услуги
				bool isFirst = true;
				int srow = 20;  // 20 строка
				foreach (var service in services)
					if (service.ServiceTypeId.HasValue)
					{
						var serviceType = serviceTypes.First(w => w.ID == service.ServiceTypeId);
						// вычисление НДС
						double tax = 0;
						if (service.VatId.HasValue)
						{
							var vat = vats.First(w => w.ID == service.VatId.Value);
							tax = service.OriginalSum.Value - service.OriginalSum.Value / (100 + vat.Percent) * 100;
						}

						if (!isFirst)
						{
							sheet.InsertRow(srow, 1);
							var r1 = sheet.Cells["A" + srow + ":" + "M" + srow];
							var r2 = sheet.Cells["A" + (srow - 1) + ":" + "M" + (srow - 1)];
							r2.Copy(r1);
						}

						sheet.Cells[srow, 2].Value = serviceType.Name;
						sheet.Cells[srow, 11].Value = Math.Round(tax, 2);
						sheet.Cells[srow, 12].Value = Math.Round(service.OriginalSum.Value, 2);
						isFirst = false;
						srow++;
					}

				// вычисление значений во всех ячейках
				var allCells = sheet.Cells;
				foreach (var item in allCells)
					item.Calculate();

				xl.Save();
			}

			#endregion

			try
			{
				SpreadsheetInfo.SetLicense("FREE-LIMITED-KEY");
				var culture = Thread.CurrentThread.CurrentCulture;
				Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("ru-RU");
				ExcelFile ef = ExcelFile.Load(filename, LoadOptions.XlsxDefault);
				var pdfFilename = HttpContext.Current.Server.MapPath("~\\Temp\\" + entity.Filename + ".pdf");
				ef.Save(pdfFilename);
				Thread.CurrentThread.CurrentCulture = culture;

				switch (format)
				{
					case TemplatedDocumentFormat.Pdf:
						UploadTemplatedDocumentData(entity.ID, filename, "xlsx");
						UploadTemplatedDocumentData(entity.ID, pdfFilename, "pdf");
						break;
					case TemplatedDocumentFormat.CleanPdf:
						UploadTemplatedDocumentData(entity.ID, pdfFilename, "cleanpdf");
						break;
					case TemplatedDocumentFormat.CutPdf:
						UploadTemplatedDocumentData(entity.ID, pdfFilename, "cutpdf");
						break;
				}
			}
			catch { }

			return string.Empty;
		}

		#endregion

		void DownloadTemplateData(int templateId, string filename)
		{
			var template = dataLogic.GetTemplate(templateId);

			using (var fs = new IO.FileStream(filename, IO.FileMode.OpenOrCreate, IO.FileAccess.Write))
			{
				using (var writer = new IO.BinaryWriter(fs))
				{
					writer.Write(template.Data);
					writer.Flush();
					writer.Close();
					fs.Close();
				}
			}
		}

		void UploadTemplatedDocumentData(int documentId, string filename, string type)
		{
			string filePath = filename;
			byte[] rawData;

			using (var fs = new IO.FileStream(filePath, IO.FileMode.Open, IO.FileAccess.Read))
			{
				using (var reader = new IO.BinaryReader(fs))
				{
					rawData = reader.ReadBytes((int)fs.Length);
					fs.Close();
					fs.Dispose();
				}
			}

			var docData = documentLogic.GetTemplatedDocumentData(documentId, type);
			if (docData == null)
				documentLogic.CreateTemplatedDocumentData(new TemplatedDocumentData { TemplatedDocumentId = documentId, Data = rawData, Type = type });
			else
			{
				docData.Data = rawData;
				documentLogic.UpdateTemplatedDocumentData(docData);
			}
		}

		Pricelist GetPricelist(Order order)
		{
			var contractId = order.ContractId.Value;
			var finRepCenterId = order.FinRepCenterId.Value;
			var productId = order.ProductId;

			var pricelists = pricelistLogic.GetValidPricelists().Where(w => (w.FinRepCenterId == finRepCenterId) && (w.ProductId == productId));

			var pricelist = pricelists.FirstOrDefault(w => w.ContractId == contractId);
			if (pricelist == null)
				pricelist = pricelists.FirstOrDefault(w => !w.ContractId.HasValue);

			return pricelist;
		}

		void SaveBlob(string filename, byte[] blob)
		{
			using (var fs = new IO.FileStream(filename, IO.FileMode.OpenOrCreate, IO.FileAccess.Write))
			{
				using (var writer = new IO.BinaryWriter(fs))
				{
					writer.Write(blob);
					writer.Flush();
					writer.Close();
					fs.Close();
				}
			}
		}

		string FormatDate(DateTime? date)
		{
			if (!date.HasValue)
				return " ";

			return date.Value.ToString("dd.MM.yyyy");
		}

		string FormatPlace(int placeId)
		{
			var place = dataLogic.GetPlace(placeId);
			var country = dataLogic.GetCountry(place.CountryId.Value);
			return country.Name + ", " + place.Name;
		}

		string FormatEnPlace(int placeId)
		{
			var place = dataLogic.GetPlace(placeId);
			var country = dataLogic.GetCountry(place.CountryId.Value);
			return country.EnName + ", " + place.EnName;
		}
	}
}