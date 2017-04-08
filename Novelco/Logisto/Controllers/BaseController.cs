using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using LinqToDB;
using Logisto.BusinessLogic;
using Logisto.Data;
using Logisto.Model;
using Logisto.Models;

namespace Logisto.Controllers
{
	[Authorize]
	public class BaseController : Controller
	{
		internal IDataLogic dataLogic;
		internal IBankLogic bankLogic;
		internal IUserLogic userLogic;
		internal IOrderLogic orderLogic;
		internal ILegalLogic legalLogic;
		internal IPersonLogic personLogic;
		internal IdentityLogic identityLogic;
		internal IRequestLogic requestLogic;
		internal IEmployeeLogic employeeLogic;
		internal IContractLogic contractLogic;
		internal IDocumentLogic documentLogic;
		internal IPricelistLogic pricelistLogic;
		internal IContractorLogic contractorLogic;
		internal IAccountingLogic accountingLogic;
		internal IParticipantLogic participantLogic;

		internal int CurrentUserId
		{
			get { return int.Parse(HttpContext.GetOwinContext().Authentication.User.Claims.First(w => w.Type == ClaimTypes.NameIdentifier).Value); }
		}

		internal const string DOCUMENTS_ROOT = @"\\corpserv03.corp.local\Common\Carman\Documents\";
		internal const string TEMPLATED_DOCUMENTS_ROOT = @"\\corpserv03.corp.local\Common\Carman\TemplatedDocuments\";

		public BaseController()
		{
			// TODO: DI
			dataLogic = new DataLogic();
			bankLogic = new BankLogic();
			userLogic = new UserLogic();
			orderLogic = new OrderLogic();
			legalLogic = new LegalLogic();
			personLogic = new PersonLogic();
			requestLogic = new RequestLogic();
			identityLogic = new IdentityLogic();
			employeeLogic = new EmployeeLogic();
			documentLogic = new DocumentLogic();
			contractLogic = new ContractLogic();
			pricelistLogic = new PricelistLogic();
			contractorLogic = new ContractorLogic();
			accountingLogic = new AccountingLogic();
			participantLogic = new ParticipantLogic();
		}

		internal static List<int> GetParticipantUsers(IEnumerable<Participant> workgroup, ParticipantRoles role, ParticipantRoles? role2 = null, ParticipantRoles? role3 = null)
		{
			var result = new List<int>();
			var oms = workgroup.Where(w => w.ParticipantRoleId == (int)role);
			DateTime date = DateTime.Now;
			foreach (var item in oms)
			{
				if (!item.FromDate.HasValue && !item.ToDate.HasValue)
					result.Add(item.UserId.Value);

				if ((item.FromDate.HasValue && !item.ToDate.HasValue) && (item.FromDate < date))
					result.Add(item.UserId.Value);

				if ((!item.FromDate.HasValue && item.ToDate.HasValue) && (item.ToDate > date))
					result.Add(item.UserId.Value);

				if ((item.FromDate.HasValue && item.ToDate.HasValue) && (item.FromDate < date) && (item.ToDate > date))
					result.Add(item.UserId.Value);
			}

			if (role2.HasValue)
			{
				oms = workgroup.Where(w => w.ParticipantRoleId == (int)role2);
				foreach (var item in oms)
				{
					if (!item.FromDate.HasValue && !item.ToDate.HasValue)
						result.Add(item.UserId.Value);

					if ((item.FromDate.HasValue && !item.ToDate.HasValue) && (item.FromDate < date))
						result.Add(item.UserId.Value);

					if ((!item.FromDate.HasValue && item.ToDate.HasValue) && (item.ToDate > date))
						result.Add(item.UserId.Value);

					if ((item.FromDate.HasValue && item.ToDate.HasValue) && (item.FromDate < date) && (item.ToDate > date))
						result.Add(item.UserId.Value);
				}

				if (role3.HasValue)
				{
					oms = workgroup.Where(w => w.ParticipantRoleId == (int)role3);
					foreach (var item in oms)
					{
						if (!item.FromDate.HasValue && !item.ToDate.HasValue)
							result.Add(item.UserId.Value);

						if ((item.FromDate.HasValue && !item.ToDate.HasValue) && (item.FromDate < date))
							result.Add(item.UserId.Value);

						if ((!item.FromDate.HasValue && item.ToDate.HasValue) && (item.ToDate > date))
							result.Add(item.UserId.Value);

						if ((item.FromDate.HasValue && item.ToDate.HasValue) && (item.FromDate < date) && (item.ToDate > date))
							result.Add(item.UserId.Value);
					}
				}
			}

			return result.Distinct().ToList();
		}

		internal List<int> GetParticipantUsers(IEnumerable<Participant> workgroup, int participantRoleId, int? participantRoleId2 = null, int? participantRoleId3 = null)
		{
			var result = new List<int>();
			var oms = workgroup.Where(w => w.ParticipantRoleId == participantRoleId);
			DateTime date = DateTime.Now;
			foreach (var item in oms)
			{
				if (!item.FromDate.HasValue && !item.ToDate.HasValue)
					result.Add(item.UserId.Value);

				if ((item.FromDate.HasValue && !item.ToDate.HasValue) && (item.FromDate < date))
					result.Add(item.UserId.Value);

				if ((!item.FromDate.HasValue && item.ToDate.HasValue) && (item.ToDate > date))
					result.Add(item.UserId.Value);

				if ((item.FromDate.HasValue && item.ToDate.HasValue) && (item.FromDate < date) && (item.ToDate > date))
					result.Add(item.UserId.Value);
			}

			if (participantRoleId2.HasValue)
			{
				oms = workgroup.Where(w => w.ParticipantRoleId == participantRoleId2);
				foreach (var item in oms)
				{
					if (!item.FromDate.HasValue && !item.ToDate.HasValue)
						result.Add(item.UserId.Value);

					if ((item.FromDate.HasValue && !item.ToDate.HasValue) && (item.FromDate < date))
						result.Add(item.UserId.Value);

					if ((!item.FromDate.HasValue && item.ToDate.HasValue) && (item.ToDate > date))
						result.Add(item.UserId.Value);

					if ((item.FromDate.HasValue && item.ToDate.HasValue) && (item.FromDate < date) && (item.ToDate > date))
						result.Add(item.UserId.Value);
				}

				if (participantRoleId3.HasValue)
				{
					oms = workgroup.Where(w => w.ParticipantRoleId == participantRoleId3);
					foreach (var item in oms)
					{
						if (!item.FromDate.HasValue && !item.ToDate.HasValue)
							result.Add(item.UserId.Value);

						if ((item.FromDate.HasValue && !item.ToDate.HasValue) && (item.FromDate < date))
							result.Add(item.UserId.Value);

						if ((!item.FromDate.HasValue && item.ToDate.HasValue) && (item.ToDate > date))
							result.Add(item.UserId.Value);

						if ((item.FromDate.HasValue && item.ToDate.HasValue) && (item.FromDate < date) && (item.ToDate > date))
							result.Add(item.UserId.Value);
					}
				}
			}

			return result.Distinct().ToList();
		}

		internal bool IsAllowed(IEnumerable<Participant> workgroup, ParticipantRoles role, ParticipantRoles? role2 = null, ParticipantRoles? role3 = null, ParticipantRoles? role4 = null)
		{
			var allowed = false;
			var oms = workgroup.Where(w => w.ParticipantRoleId == (int)role);
			DateTime date = DateTime.Now;
			foreach (var item in oms)
			{
				if (!item.FromDate.HasValue && !item.ToDate.HasValue)
					allowed = true;

				if ((item.FromDate.HasValue && !item.ToDate.HasValue) && (item.FromDate < date))
					allowed = true;

				if ((!item.FromDate.HasValue && item.ToDate.HasValue) && (item.ToDate > date))
					allowed = true;

				if ((item.FromDate.HasValue && item.ToDate.HasValue) && (item.FromDate < date) && (item.ToDate > date))
					allowed = true;
			}

			if (role2.HasValue)
			{
				oms = workgroup.Where(w => w.ParticipantRoleId == (int)role2);
				foreach (var item in oms)
				{
					if (!item.FromDate.HasValue && !item.ToDate.HasValue)
						allowed = true;

					if ((item.FromDate.HasValue && !item.ToDate.HasValue) && (item.FromDate < date))
						allowed = true;

					if ((!item.FromDate.HasValue && item.ToDate.HasValue) && (item.ToDate > date))
						allowed = true;

					if ((item.FromDate.HasValue && item.ToDate.HasValue) && (item.FromDate < date) && (item.ToDate > date))
						allowed = true;
				}

				if (role3.HasValue)
				{
					oms = workgroup.Where(w => w.ParticipantRoleId == (int)role3);
					foreach (var item in oms)
					{
						if (!item.FromDate.HasValue && !item.ToDate.HasValue)
							allowed = true;

						if ((item.FromDate.HasValue && !item.ToDate.HasValue) && (item.FromDate < date))
							allowed = true;

						if ((!item.FromDate.HasValue && item.ToDate.HasValue) && (item.ToDate > date))
							allowed = true;

						if ((item.FromDate.HasValue && item.ToDate.HasValue) && (item.FromDate < date) && (item.ToDate > date))
							allowed = true;
					}

					if (role4.HasValue)
					{
						oms = workgroup.Where(w => w.ParticipantRoleId == (int)role4);
						foreach (var item in oms)
						{
							if (!item.FromDate.HasValue && !item.ToDate.HasValue)
								allowed = true;

							if ((item.FromDate.HasValue && !item.ToDate.HasValue) && (item.FromDate < date))
								allowed = true;

							if ((!item.FromDate.HasValue && item.ToDate.HasValue) && (item.ToDate > date))
								allowed = true;

							if ((item.FromDate.HasValue && item.ToDate.HasValue) && (item.FromDate < date) && (item.ToDate > date))
								allowed = true;
						}
					}
				}
			}

			return allowed;
		}

		internal bool IsUserAllowed(IEnumerable<Participant> workgroup, int userId, ParticipantRoles role, ParticipantRoles? role2 = null, ParticipantRoles? role3 = null)
		{
			var allowed = false;
			var oms = workgroup.Where(w => w.UserId == userId && w.ParticipantRoleId == (int)role);
			DateTime date = DateTime.Now;
			foreach (var item in oms)
			{
				if (!item.FromDate.HasValue && !item.ToDate.HasValue)
					allowed = true;

				if ((item.FromDate.HasValue && !item.ToDate.HasValue) && (item.FromDate < date))
					allowed = true;

				if ((!item.FromDate.HasValue && item.ToDate.HasValue) && (item.ToDate > date))
					allowed = true;

				if ((item.FromDate.HasValue && item.ToDate.HasValue) && (item.FromDate < date) && (item.ToDate > date))
					allowed = true;
			}

			if (role2.HasValue)
			{
				oms = workgroup.Where(w => w.UserId == userId && w.ParticipantRoleId == (int)role2);
				foreach (var item in oms)
				{
					if (!item.FromDate.HasValue && !item.ToDate.HasValue)
						allowed = true;

					if ((item.FromDate.HasValue && !item.ToDate.HasValue) && (item.FromDate < date))
						allowed = true;

					if ((!item.FromDate.HasValue && item.ToDate.HasValue) && (item.ToDate > date))
						allowed = true;

					if ((item.FromDate.HasValue && item.ToDate.HasValue) && (item.FromDate < date) && (item.ToDate > date))
						allowed = true;
				}

				if (role3.HasValue)
				{
					oms = workgroup.Where(w => w.UserId == userId && w.ParticipantRoleId == (int)role3);
					foreach (var item in oms)
					{
						if (!item.FromDate.HasValue && !item.ToDate.HasValue)
							allowed = true;

						if ((item.FromDate.HasValue && !item.ToDate.HasValue) && (item.FromDate < date))
							allowed = true;

						if ((!item.FromDate.HasValue && item.ToDate.HasValue) && (item.ToDate > date))
							allowed = true;

						if ((item.FromDate.HasValue && item.ToDate.HasValue) && (item.FromDate < date) && (item.ToDate > date))
							allowed = true;
					}
				}
			}

			return allowed;
		}

		internal bool IsParticipantAt(IEnumerable<Participant> workgroup, ParticipantRoles role, DateTime date)
		{
			var allowed = false;
			var oms = workgroup.Where(w => w.ParticipantRoleId == (int)role);
			foreach (var item in oms)
			{
				if (!item.FromDate.HasValue && !item.ToDate.HasValue)
					allowed = true;

				if ((item.FromDate.HasValue && !item.ToDate.HasValue) && (item.FromDate < date))
					allowed = true;

				if ((!item.FromDate.HasValue && item.ToDate.HasValue) && (item.ToDate > date))
					allowed = true;

				if ((item.FromDate.HasValue && item.ToDate.HasValue) && (item.FromDate < date) && (item.ToDate > date))
					allowed = true;
			}

			return allowed;
		}

		internal IEnumerable<Participant> FilterByDates(IEnumerable<Participant> workgroup)
		{
			var result = new List<Participant>();
			var date = DateTime.Now;
			foreach (var item in workgroup)
			{
				if (!item.FromDate.HasValue && !item.ToDate.HasValue)
					result.Add(item);

				if ((item.FromDate.HasValue && !item.ToDate.HasValue) && (item.FromDate < date))
					result.Add(item);

				if ((!item.FromDate.HasValue && item.ToDate.HasValue) && (item.ToDate > date))
					result.Add(item);

				if ((item.FromDate.HasValue && item.ToDate.HasValue) && (item.FromDate < date) && (item.ToDate > date))
					result.Add(item);
			}

			return result;
		}

		internal void SendMail(string email, string subject, string message, string email2 = null)
		{
			try
			{
				var mailMessage = new MailMessage("CarMan <carman@novelco.ru>", email);
				if (!string.IsNullOrWhiteSpace(email2))
					mailMessage.CC.Add(email2);

				mailMessage.Subject = subject;
				mailMessage.Body = message;
				mailMessage.IsBodyHtml = true;

				// Если порт не задан в настройках, то устанавливаем 25
				int port = 25;
				string host = "srvmail02.corp.local";   //string host = "192.168.1.7";    //string host = "mail.novelco.ru";

				using (var server = new SmtpClient(host, port))
				{
					server.UseDefaultCredentials = false;
					//Credentials = new NetworkCredential(_settings.Login, _settings.Password),
					server.EnableSsl = false;

					server.Send(mailMessage);
				}
			}
			catch (Exception ex)
			{
				// TODO: context null
				if (System.Web.HttpContext.Current != null)
				{
					string filename = HttpContext.Server.MapPath("~/Temp/" + DateTime.UtcNow.ToString("yyyy-MM-dd_HHmmss") + ".log");
					using (var writer = new StreamWriter(filename, true))
					{
						writer.WriteLine(ex.ToString());
						writer.Close();
					}
				}
			}
		}

		internal bool IsSuperUser()
		{
			return identityLogic.IsUserInRole(CurrentUserId, 1);    // GM
		}


		#region Schedule r

		internal void ProcessJob()
		{
			using (var db = new LogistoDb())
			{
				var lastJobDate = db.Job.Max(w => w.Date);
				while (lastJobDate.Date < DateTime.Today)
				{
					lastJobDate = lastJobDate.AddDays(1);
					var schedules = db.Schedule.Where(w => w.Weekday == (int)lastJobDate.DayOfWeek).ToList();
					if (schedules.Count > 0)
						foreach (var item in schedules) // создать задания на день
							db.InsertWithIdentity(new Job { Date = new DateTime(lastJobDate.Year, lastJobDate.Month, lastJobDate.Day, item.Hour, item.Minute, 0), ScheduleId = item.ID });
					else
						db.InsertWithIdentity(new Job { Date = lastJobDate, IsDone = true });   // создать пустое задание
				}
				
				var job = db.Job.FirstOrDefault(w => !w.IsDone && (w.Date < DateTime.Now));
				if (job != null)
				{
					db.Job.Where(w => w.ID == job.ID).Set(s => s.IsDone, true).Update();
					if (job.ScheduleId > 0)
					{
						var schedule = db.Schedule.First(w => w.ID == job.ScheduleId);
						switch (schedule.ReportName)
						{
							case "Контроль дебиторской задолженности":
								var r = new RController();
								r.ControllerContext = this.ControllerContext;
								var users = db.Users.ToList();
								foreach (var user in users)
									r.SendAccountsReceivableReport(user.ID);

								break;

							case "Рассылка уведомлений клиентам":

								new NotificationMailer().SendNotificationOf_ChangeDocument(job.Date);

								break;

						}
					}

				}
			}
		}

		#endregion
	}
}






