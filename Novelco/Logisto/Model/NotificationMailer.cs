using System;
using System.IO;
using System.Net.Mail;
using Logisto.Data;
using Logisto.Models;
using System.Linq;
using System.Net;
using Logisto.BusinessLogic;

namespace Logisto.Model
{
	public class NotificationMailer
	{
		public void SendMail(string email, string subject, string message, string email2 = null)
		{
			try
			{
				var mailMessage = new MailMessage("CarMan <carman@novelco.ru>", email);
				if (!string.IsNullOrWhiteSpace(email2))
					mailMessage.CC.Add(email2);

				mailMessage.Subject = subject;
				mailMessage.Body = message;
				mailMessage.IsBodyHtml = true;

				int port = 25;
				string host = "srvmail02.corp.local";

				using (var server = new SmtpClient(host, port))
				{
					server.UseDefaultCredentials = false;
					server.Credentials = new NetworkCredential("carman@novelco.ru", "NVLP@ssw0rd");
					server.Send(mailMessage);
				}

				new DataLogic().CreateMailingLog(new MailingLog { Date = DateTime.Now, To = email, Subject = subject, Text = message });
			}
			catch (Exception ex)
			{
				// TODO: context null
				if (System.Web.HttpContext.Current != null)
				{
					string filename = System.Web.HttpContext.Current.Server.MapPath("~/Temp/" + DateTime.UtcNow.ToString("yyyy-MM-dd_HHmmss") + ".log");
					using (var writer = new StreamWriter(filename, true))
					{
						writer.WriteLine(ex.ToString());
						writer.Close();
					}
				}
			}
		}

		public void SendNotificationOf_Event(int contractorId, Order order, Event @event)
		{
			using (var db = new LogistoDb())
			{
				var employees = from l in db.Legals
								join e in db.Employees on l.ID equals e.LegalId
								join p in db.Persons on e.PersonId equals p.ID
								join s in db.ContractorEmployeeSettings on e.ID equals s.EmployeeId
								where l.ContractorId == contractorId
								select new { e, s, p };

				foreach (var item in employees.ToList())
					if (!item.s.IsEnUI && item.s.NotifyEventCreated)
					{
						string subject = "NIP: Новое событие в заказе";
						string message = "Здравствуйте, <br /> в заказе <a href='http://nip.novelco.ru/Orders/View/{1}'>{0}</a> добавлено новое событие '{2}'. <br /> Спасибо. <br /><br /> Если вы не желаете получать эти сообщения, Вам необходимо внести соответствующие изменения в раздел Настройки Вашего личного кабинета по ссылке http://nip.novelco.ru/ ";
						SendMail(item.p.Email, subject, string.Format(message, order.Number, order.ID, @event.Name));
					}
			}
		}

		public void SendNotificationOf_DocumentChanged(int contractorId, Order order, Document document, Event @event)
		{
			using (var db = new LogistoDb())
			{
				var employees = from l in db.Legals
								join e in db.Employees on l.ID equals e.LegalId
								join p in db.Persons on e.PersonId equals p.ID
								join s in db.ContractorEmployeeSettings on e.ID equals s.EmployeeId
								where l.ContractorId == contractorId
								select new { e, s, p };

				foreach (var item in employees.ToList())
					if (!item.s.IsEnUI && item.s.NotifyDocumentChanged)
					{
						string subject = "NIP: Изменения в документах заказа";
						string message = "Здравствуйте, <br /> в заказе <a href='http://nip.novelco.ru/Orders/View/{0}'>{1}</a> следующий документ был изменен пользователем '{6}'. <br /> Тип документа {5} Документ № <a href='http://nip.novelco.ru/Documents/View/{2}'>{3}</a> Дата документа {4} <br /> Спасибо. <br /><br /> Если вы не желаете получать эти сообщения, Вам необходимо внести соответствующие изменения в раздел Настройки Вашего личного кабинета по ссылке http://nip.novelco.ru/ ";
						var userName = "";
						var documentType = "";
						SendMail(item.p.Email, subject, string.Format(message, order.ID, order.Number, document.ID, document.Number, document.Date, documentType, userName, @event.Name));
					}
			}
		}

		public void SendNotificationOf_StatusChanged(int contractorId, Order order, int userId)
		{
			using (var db = new LogistoDb())
			{
				var employees = from l in db.Legals
								join e in db.Employees on l.ID equals e.LegalId
								join p in db.Persons on e.PersonId equals p.ID
								join s in db.ContractorEmployeeSettings on e.ID equals s.EmployeeId
								where l.ContractorId == contractorId
								select new { e, s, p };

				foreach (var item in employees.ToList())
					if (!item.s.IsEnUI && item.s.NotifyStatusChanged)
					{
						string subject = "NIP: Изменения в статусе заказа";
						string message = "Здравствуйте, <br /> заказ <a href='http://nip.novelco.ru/Orders/View/{0}'>{1}</a> был переведен в статус '{2}' пользователем {3}. <br /> Спасибо. <br /><br /> Если вы не желаете получать эти сообщения, Вам необходимо внести соответствующие изменения в раздел Настройки Вашего личного кабинета по ссылке http://nip.novelco.ru/ ";
						var status = db.OrderStatuses.First(w => w.ID == order.OrderStatusId).Display;
						var user = db.Persons.First(wp => wp.ID == (db.Users.First(w => w.ID == userId).PersonId)).DisplayName;
						SendMail(item.p.Email, subject, string.Format(message, order.ID, order.Number, status, user));
					}
			}
		}

		public void SendNotificationOf_ChangeDocument(DateTime date)
		{
			using (var db = new LogistoDb())
			{
				var employees = from l in db.Legals
								join e in db.Employees on l.ID equals e.LegalId
								join p in db.Persons on e.PersonId equals p.ID
								join s in db.ContractorEmployeeSettings on e.ID equals s.EmployeeId
								where !s.IsEnUI && s.NotifyDocumentChanged
								select new { e, s, p };

				foreach (var employee in employees.ToList())
				{
					var start = date.Date;
					var end = date.Date.AddDays(1);
					var docs = from d in db.Documents
							   where (d.UploadedDate >= start) && (d.UploadedDate < end)
							   select d;
				}
			}
		}
	}
}