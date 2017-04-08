using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Logisto.Data;
using Logisto.Identity;
using Logisto.Models;
using Logisto.ViewModels;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using OfficeOpenXml;
using Logisto.BusinessLogic;
using System.Globalization;
using System.Net.Mail;
using System.Diagnostics;

namespace Logisto.Controllers
{
    /// <summary>
    /// Reports
    /// </summary>
    [Authorize]
    public class RController : BaseController
    {
        #region Pages

        public ActionResult Index()
        {
            return View();
        }

        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult CreateAccountingDetailedReport()
        {
            var userManager = new UserManager(new UserStore(new LogistoDb()), Startup.IdentityFactoryOptions, new EmailService());
            if (!userManager.IsInRole(CurrentUserId, "GM"))
                return RedirectToAction("NotAuthorized", "Home");

            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            var template = Server.MapPath("~/App_Data/IncomeExpenseDetailedTemplate.xlsx");
            var filename = Server.MapPath("~/Temp/r" + new Random(Environment.TickCount).Next(999999) + ".xlsx");

            GenerateAccountingDetailedReport(template, filename);
            return File(filename, contentType, "IncomeExpenseDetailed.xlsx");
        }

        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult CreateAccountingPerOrderReport()
        {
            var userManager = new UserManager(new UserStore(new LogistoDb()), Startup.IdentityFactoryOptions, new EmailService());
            if (!userManager.IsInRole(CurrentUserId, "GM"))
                return RedirectToAction("NotAuthorized", "Home");

            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            var template = Server.MapPath("~/App_Data/IncomeExpensePerOrderTemplate.xlsx");
            var filename = Server.MapPath("~/Temp/r" + new Random(Environment.TickCount).Next(999999) + ".xlsx");

            GenerateAccountingPerOrderReport(template, filename);
            return File(filename, contentType, "IncomeExpensePerOrder.xlsx");
        }

        [OutputCache(NoStore = true, Duration = 0)]
        public FileResult CreateDebtsReport()
        {
            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            var template = Server.MapPath("~/App_Data/DebtsTemplate.xlsx");
            var filename = Server.MapPath("~/Temp/r" + new Random(Environment.TickCount).Next(999999) + ".xlsx");

            var userManager = new UserManager(new UserStore(new LogistoDb()), Startup.IdentityFactoryOptions, new EmailService());
            if (userManager.IsInRole(CurrentUserId, "GM"))
                GenerateDebtsReport(template, filename);
            else
                GenerateDebtsReport_Roled(template, filename);

            return File(filename, contentType, "AccountsReceivablePayable.xlsx");
        }

        [OutputCache(NoStore = true, Duration = 0)]
        public FileResult CreateAccountsReceivableReport()
        {
            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            var template = Server.MapPath("~/App_Data/AccountsReceivableTemplate.xlsx");
            var filename = Server.MapPath("~/Temp/r" + new Random(Environment.TickCount).Next(999999) + ".xlsx");

            GenerateAccountsReceivable(template, filename, CurrentUserId);

            return File(filename, contentType, "AccountsReceivable.xlsx");
        }

        public ContentResult SendAccountsReceivableReport(int userId)
        {
            var template = HttpContext.Server.MapPath("~/App_Data/AccountsReceivableTemplate.xlsx");
            var filename = HttpContext.Server.MapPath("~/Temp/r" + new Random(Environment.TickCount).Next(999999) + ".xlsx");

            System.Diagnostics.Debug.WriteLine("Prepare to generate report to " + userId);
            bool hasRecords = GenerateAccountsReceivable(template, filename, userId);
            System.Diagnostics.Debug.WriteLine("Generated...");

            if (!hasRecords)
                return Content(JsonConvert.SerializeObject(""));

            var user = identityLogic.GetUser(userId);
            try
            {
                System.Diagnostics.Debug.WriteLine("Sending to " + user.Email);
                var mailMessage = new MailMessage("CarMan <accounting@novelco.ru>", user.Email);
                //mailMessage.CC.Add("shulapov@novelco.ru");
                mailMessage.Subject = "Дебиторская задолженность";
                mailMessage.Body = "Здравствуйте, " + user.Name + @"
<br />
<br />
по Вашим клиентам имеется непогашенная дебиторская задолженность - см вложенный файл.
<br />
Срок оплаты в соответствии с условиями договора отображен в колонке 'План. Дата Оплаты'(ПДО):
<br />
-при просрочке оплаты более трех дней, ячейка ПДО помечена желтым. Вам необходимо связаться с Клиентом и определить планируемый срок оплаты.
<br />
-при просрочке оплаты более четырнадцати дней, ячейка ПДО помечена красным. Вам необходимо начать претензионный порядок возмещения дебиторской задолженности.";
                mailMessage.IsBodyHtml = true;
                mailMessage.Attachments.Add(new Attachment(filename));
                // Если порт не задан в настройках, то устанавливаем 25
                int port = 25;
                string host = "srvmail02.corp.local";

                using (var server = new SmtpClient(host, port))
                {
                    server.UseDefaultCredentials = false;
                    server.EnableSsl = false;
                    server.Send(mailMessage);
                }
            }
            catch (Exception ex)
            {
                // TODO: context null
                if (System.Web.HttpContext.Current != null)
                {
                    string filename1 = HttpContext.Server.MapPath("~/Temp/" + DateTime.UtcNow.ToString("yyyy-MM-dd_HHmmss") + ".log");
                    using (var writer = new StreamWriter(filename1, true))
                    {
                        writer.WriteLine(ex.ToString());
                        writer.Close();
                    }
                }
            }

            return Content(JsonConvert.SerializeObject(""));
        }

        public ActionResult MailingLog()
        {
            var pageSize = 100;
            var filter = new ListFilter { PageSize = pageSize };
            var viewModel = new IndexViewModel { Filter = filter };

            return View(viewModel);
        }

        public ActionResult Motivation()
        {
            var pageSize = int.Parse(ConfigurationManager.AppSettings["App_PageSize"]);
            var filter = new ListFilter { PageSize = pageSize, Type = "Current", UserId = CurrentUserId, Statuses = { { 1 }, { 3 }, { 5 } } };

            var totalCount = orderLogic.GetMotivationOrdersCount(filter);
            var totalSum = orderLogic.GetMotivationOrdersSum(filter);
            var viewModel = new OrdersViewModel { Filter = filter, Items = orderLogic.GetMotivationOrders(filter).ToList(), TotalItemsCount = totalCount, TotalItemsSum = totalSum };

            viewModel.Dictionaries.Add("User", dataLogic.GetUsers());
            viewModel.Dictionaries.Add("ActiveUser", dataLogic.GetActiveUsers());
            viewModel.Dictionaries.Add("ParticipantRole", dataLogic.GetParticipantRoles().Where(w => w.ID == 1 || w.ID == 3 || w.ID == 5).OrderBy(o => o.Name));
            viewModel.Dictionaries.Add("Legal", dataLogic.GetLegals());
            viewModel.Dictionaries.Add("OurLegal", dataLogic.GetOurLegals());
            viewModel.Dictionaries.Add("ContractType", dataLogic.GetContractTypes());
            viewModel.Dictionaries.Add("ContractRole", dataLogic.GetContractRoles());
            viewModel.Dictionaries.Add("ContractServiceType", dataLogic.GetContractServiceTypes());
            viewModel.Dictionaries.Add("PaymentTerm", dataLogic.GetPaymentTerms());
            viewModel.Dictionaries.Add("LegalByContract", dataLogic.GetLegalsByContract());
            viewModel.Dictionaries.Add("ContractorByOrder", dataLogic.GetContractorsByOrder());
            viewModel.Dictionaries.Add("OrderStatus", dataLogic.GetOrderStatuses());
            viewModel.Dictionaries.Add("OrderType", dataLogic.GetOrderTypes());
            viewModel.Dictionaries.Add("Product", dataLogic.GetProducts());
            viewModel.Dictionaries.Add("ServiceType", dataLogic.GetServiceTypes());
            viewModel.Dictionaries.Add("Currency", dataLogic.GetCurrencies());
            viewModel.Dictionaries.Add("Contract", dataLogic.GetContracts());
            viewModel.Dictionaries.Add("OrderTemplate", dataLogic.GetOrderTemplates());

            return View(viewModel);
        }

        public ActionResult Declarations()
        {
            var pageSize = int.Parse(ConfigurationManager.AppSettings["App_PageSize"]);
            var filter = new ListFilter { PageSize = pageSize, UserId = CurrentUserId };

            var totalCount = documentLogic.GetDeclarationDocumentsCount(filter);
            var viewModel = new DeclarationOrdersViewModel { Filter = filter, Items = new List<DeclarationViewModel>(), TotalItemsCount = totalCount };

            viewModel.Dictionaries.Add("User", dataLogic.GetUsers());
            viewModel.Dictionaries.Add("Legal", dataLogic.GetLegals());
            viewModel.Dictionaries.Add("OurLegal", dataLogic.GetOurLegals());
            viewModel.Dictionaries.Add("ContractType", dataLogic.GetContractTypes());
            viewModel.Dictionaries.Add("ContractRole", dataLogic.GetContractRoles());
            viewModel.Dictionaries.Add("ContractServiceType", dataLogic.GetContractServiceTypes());
            viewModel.Dictionaries.Add("PaymentTerm", dataLogic.GetPaymentTerms());
            viewModel.Dictionaries.Add("LegalByContract", dataLogic.GetLegalsByContract());
            viewModel.Dictionaries.Add("ContractorByOrder", dataLogic.GetContractorsByOrder());
            viewModel.Dictionaries.Add("OrderStatus", dataLogic.GetOrderStatuses());
            viewModel.Dictionaries.Add("OrderType", dataLogic.GetOrderTypes());
            viewModel.Dictionaries.Add("Product", dataLogic.GetProducts());
            viewModel.Dictionaries.Add("ServiceType", dataLogic.GetServiceTypes());
            viewModel.Dictionaries.Add("Currency", dataLogic.GetCurrencies());
            viewModel.Dictionaries.Add("Contract", dataLogic.GetContracts());
            viewModel.Dictionaries.Add("OrderTemplate", dataLogic.GetOrderTemplates());

            return View(viewModel);
        }

        public ActionResult Clients()
        {
            if (!IsSuperUser())
                return RedirectToAction("NotAuthorized", "Home");

            var pageSize = int.Parse(ConfigurationManager.AppSettings["App_PageSize"]);
            var filter = new ListFilter { PageSize = pageSize, Type = "Client" };

            var totalCount = contractorLogic.GetContractorsCount(filter);
            var list = contractorLogic.GetContractors(filter);
            var resultList = list.Select(s => new ContractorViewModel { Contractor = s }).ToList();

            foreach (var item in resultList)
            {
                var wg = participantLogic.GetWorkgroupByContractor(item.Contractor.ID);
                wg = FilterByDates(wg);

                var sm = wg.FirstOrDefault(w => w.ParticipantRoleId == (int)ParticipantRoles.SM);
                if ((sm != null) && sm.UserId.HasValue)
                {
                    item.SM = sm;
                    item.SMDisplay = personLogic.GetPerson(userLogic.GetUser(sm.UserId.Value).PersonId.Value).DisplayName;
                }

                var am = wg.FirstOrDefault(w => (w.ParticipantRoleId == (int)ParticipantRoles.AM) && w.IsResponsible);
                if ((am != null) && am.UserId.HasValue)
                {
                    item.ResponsibleAM = am;
                    item.ResponsibleAMDisplay = personLogic.GetPerson(userLogic.GetUser(am.UserId.Value).PersonId.Value).DisplayName;
                }

                var dam = wg.FirstOrDefault(w => (w.ParticipantRoleId == (int)ParticipantRoles.AM) && w.IsDeputy);
                if ((dam != null) && dam.UserId.HasValue)
                {
                    item.DeputyAM = dam;
                    item.DeputyAMDisplay = personLogic.GetPerson(userLogic.GetUser(dam.UserId.Value).PersonId.Value).DisplayName;
                }

                var sl = wg.FirstOrDefault(w => w.ParticipantRoleId == (int)ParticipantRoles.SL);
                if ((sl != null) && sl.UserId.HasValue)
                {
                    item.SL = sl;
                    item.SLDisplay = personLogic.GetPerson(userLogic.GetUser(sl.UserId.Value).PersonId.Value).DisplayName;
                }
            }

            var viewModel = new Contractors2ViewModel { Filter = filter, Items = resultList, TotalItemsCount = totalCount };

            viewModel.Dictionaries.Add("User", dataLogic.GetUsers());
            viewModel.Dictionaries.Add("ActiveUser", dataLogic.GetActiveUsers());
            viewModel.Dictionaries.Add("ParticipantRole", dataLogic.GetParticipantRoles().Where(w => w.ID == 1 || w.ID == 3 || w.ID == 5).OrderBy(o => o.Name));
            viewModel.Dictionaries.Add("Legal", dataLogic.GetLegals());
            viewModel.Dictionaries.Add("OurLegal", dataLogic.GetOurLegals());
            viewModel.Dictionaries.Add("ContractType", dataLogic.GetContractTypes());
            viewModel.Dictionaries.Add("ContractRole", dataLogic.GetContractRoles());
            viewModel.Dictionaries.Add("ContractServiceType", dataLogic.GetContractServiceTypes());
            viewModel.Dictionaries.Add("PaymentTerm", dataLogic.GetPaymentTerms());
            viewModel.Dictionaries.Add("LegalByContract", dataLogic.GetLegalsByContract());
            viewModel.Dictionaries.Add("ContractorByOrder", dataLogic.GetContractorsByOrder());
            viewModel.Dictionaries.Add("OrderStatus", dataLogic.GetOrderStatuses());
            viewModel.Dictionaries.Add("OrderType", dataLogic.GetOrderTypes());
            viewModel.Dictionaries.Add("Product", dataLogic.GetProducts());
            viewModel.Dictionaries.Add("ServiceType", dataLogic.GetServiceTypes());
            viewModel.Dictionaries.Add("Currency", dataLogic.GetCurrencies());
            viewModel.Dictionaries.Add("Contract", dataLogic.GetContracts());
            viewModel.Dictionaries.Add("OrderTemplate", dataLogic.GetOrderTemplates());

            return View(viewModel);
        }

        public ActionResult CrmReport()
        {
            var crmLogic = new CrmLogic();
            var pageSize = int.Parse(ConfigurationManager.AppSettings["App_PageSize"]);
            var from = DateTime.Today;
            while (from.DayOfWeek != DayOfWeek.Monday)
                from = from.AddDays(-1);

            var filter = new ListFilter { PageSize = pageSize, From = from, To = DateTime.Now };

            if (!IsSuperUser())
                filter.UserId = CurrentUserId;

            var totalCount = crmLogic.GetCallsCount(filter);
            var viewModel = new CrmViewModel { Filter = filter, Items = crmLogic.GetCalls(filter).ToList(), TotalItemsCount = totalCount };

            var users = userLogic.GetUsers(new ListFilter());
            foreach (var item in viewModel.Items)
            {
                item.UserId = users.Where(w => w.CrmId == item.ID_MANAGER).Select(s => s.ID).FirstOrDefault();
                if (item.To.HasValue)
                    item.Duration = (item.To.Value - item.From).ToString();
            }

            viewModel.Dictionaries.Add("User", dataLogic.GetUsers());
            viewModel.Dictionaries.Add("ActiveUser", dataLogic.GetActiveUsers());

            //  UGLY:
            var total = crmLogic.GetCalls(new ListFilter { Context = filter.Context, From = filter.From, To = filter.To, Type = filter.Type, UserId = filter.UserId });
            TimeSpan totalDuration = new TimeSpan();
            foreach (var item in total)
                if (item.To.HasValue)
                    totalDuration += (item.To.Value - item.From);

            viewModel.TotalDuration = totalDuration.ToString();

            return View(viewModel);
        }

        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult DownloadCrmReport(ListFilter filter)
        {
            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            var template = Server.MapPath("~/App_Data/CrmReportTemplate.xlsx");
            var filename = Server.MapPath("~/Temp/r" + new Random(Environment.TickCount).Next(999999) + ".xlsx");

            if (!IsSuperUser())
                filter.UserId = CurrentUserId;

            GenerateCrmReport(filter, template, filename);
            return File(filename, contentType, "CrmReport.xlsx");
        }

        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult DownloadClientsReport(ListFilter filter)
        {
            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            var template = Server.MapPath("~/App_Data/ClientsTemplate.xlsx");
            var filename = Server.MapPath("~/Temp/r" + new Random(Environment.TickCount).Next(999999) + ".xlsx");

            GenerateClientsReport(filter, template, filename);
            return File(filename, contentType, "ClientsReport.xlsx");
        }

        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult DownloadDeclarationOrdersReport(ListFilter filter)
        {
            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            var template = Server.MapPath("~/App_Data/DeclarationsTemplate.xlsx");
            var filename = Server.MapPath("~/Temp/r" + new Random(Environment.TickCount).Next(999999) + ".xlsx");

            GenerateDeclarationsReport(filter, template, filename);
            return File(filename, contentType, "DeclarationsReport.xlsx");
        }

        public ActionResult CrmLegals()
        {
            if (!IsSuperUser())
                return RedirectToAction("NotAuthorized", "Home");

            var crmLogic = new CrmLogic();

            var pageSize = int.Parse(ConfigurationManager.AppSettings["App_PageSize"]);
            var filter = new ListFilter { PageSize = pageSize, UserId = CurrentUserId };

            var totalCount = crmLogic.GetLegalsCount(filter);
            var viewModel = new CrmLegalsViewModel { Filter = filter, Items = new List<CrmLegal>(), TotalItemsCount = totalCount };

            return View(viewModel);
        }

        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult DownloadCrmLegalsReport(ListFilter filter)
        {
            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            var template = Server.MapPath("~/App_Data/CrmLegalsTemplate.xlsx");
            var filename = Server.MapPath("~/Temp/r" + new Random(Environment.TickCount).Next(999999) + ".xlsx");

            GenerateCrmLegalsReport(filter, template, filename);
            return File(filename, contentType, "CrmLegals.xlsx");
        }

        #endregion

        public ContentResult GetItems(ListFilter filter)
        {
            var totalSum = orderLogic.GetMotivationOrdersSum(filter);
            var totalCount = orderLogic.GetMotivationOrdersCount(filter);
            var list = orderLogic.GetMotivationOrders(filter);
            return Content(JsonConvert.SerializeObject(new { Items = list, TotalCount = totalCount, TotalSum = totalSum }));
        }

        public ContentResult GetCrmLegalItems(ListFilter filter)
        {
            var crmLogic = new CrmLogic();
            var totalCount = crmLogic.GetLegalsCount(filter);
            var list = crmLogic.GetLegals(filter);
            return Content(JsonConvert.SerializeObject(new { Items = list, TotalCount = totalCount }));
        }

        public ContentResult GetMailingLogItems(ListFilter filter)
        {
            var list = dataLogic.GetMailingLog(filter.Context);
            return Content(JsonConvert.SerializeObject(new { Items = list }));
        }

        public ContentResult GetCallsItems(ListFilter filter)
        {
            if (!IsSuperUser())
                filter.UserId = CurrentUserId;

            var crmLogic = new CrmLogic();
            filter.From = filter.From?.Date;
            filter.To = filter.To?.Date;
            var totalCount = crmLogic.GetCallsCount(filter);
            var list = crmLogic.GetCalls(filter).ToList();
            var users = userLogic.GetUsers(new ListFilter());
            foreach (var item in list)
            {
                item.UserId = users.Where(w => w.CrmId == item.ID_MANAGER).Select(s => s.ID).FirstOrDefault();
                if (item.To.HasValue)
                    item.Duration = (item.To.Value - item.From).ToString();
            }

            //  UGLY:
            var total = crmLogic.GetCalls(new ListFilter { Context = filter.Context, From = filter.From, To = filter.To, Type = filter.Type, UserId = filter.UserId });
            TimeSpan totalDuration = new TimeSpan();
            foreach (var item in total)
                if (item.To.HasValue)
                    totalDuration += (item.To.Value - item.From);

            return Content(JsonConvert.SerializeObject(new { Items = list, TotalCount = totalCount, TotalDuration = totalDuration.ToString() }));
        }

        public ContentResult GetContractorItems(ListFilter filter)
        {
            var totalCount = contractorLogic.GetContractorsCount(filter);
            var list = contractorLogic.GetContractors(filter);
            var resultList = list.Select(s => new ContractorViewModel { Contractor = s }).ToList();

            foreach (var item in resultList)
            {
                var wg = participantLogic.GetWorkgroupByContractor(item.Contractor.ID);
                wg = FilterByDates(wg);

                var sm = wg.FirstOrDefault(w => w.ParticipantRoleId == (int)ParticipantRoles.SM);
                if ((sm != null) && sm.UserId.HasValue)
                {
                    item.SM = sm;
                    item.SMDisplay = personLogic.GetPerson(userLogic.GetUser(sm.UserId.Value).PersonId.Value).DisplayName;
                }

                var am = wg.FirstOrDefault(w => (w.ParticipantRoleId == (int)ParticipantRoles.AM) && w.IsResponsible);
                if ((am != null) && am.UserId.HasValue)
                {
                    item.ResponsibleAM = am;
                    item.ResponsibleAMDisplay = personLogic.GetPerson(userLogic.GetUser(am.UserId.Value).PersonId.Value).DisplayName;
                }

                var dam = wg.FirstOrDefault(w => (w.ParticipantRoleId == (int)ParticipantRoles.AM) && w.IsDeputy);
                if ((dam != null) && dam.UserId.HasValue)
                {
                    item.DeputyAM = dam;
                    item.DeputyAMDisplay = personLogic.GetPerson(userLogic.GetUser(dam.UserId.Value).PersonId.Value).DisplayName;
                }

                var sl = wg.FirstOrDefault(w => w.ParticipantRoleId == (int)ParticipantRoles.SL);
                if ((sl != null) && sl.UserId.HasValue)
                {
                    item.SL = sl;
                    item.SLDisplay = personLogic.GetPerson(userLogic.GetUser(sl.UserId.Value).PersonId.Value).DisplayName;
                }
            }

            return Content(JsonConvert.SerializeObject(new { Items = resultList, TotalCount = totalCount }));
        }

        public ContentResult GetDeclarationOrdersItems(ListFilter filter)
        {
            // TODO:
            filter.UserId = CurrentUserId;
            var totalCount = documentLogic.GetDeclarationDocumentsCount(filter);
            var list = documentLogic.GetDeclarationDocuments(filter);
            var orderStatuses = dataLogic.GetOrderStatuses();

            var resultList = list.Select(s => new DeclarationViewModel
            {
                DocumentId = s.ID,
                DeclarationNumber = s.Number,
                IsWeekend = s.IsWeekend,
                WeekendMarkDate = s.WeekendMarkDate,
                WeekendMarkUserId = s.WeekendMarkUserId,
                WeekendMarkUser = s.WeekendMarkUserId.HasValue ? personLogic.GetPersonByUser(s.WeekendMarkUserId.Value)?.DisplayName : "",
                ContractNumber = GetContractNumber(s),
                ContractType = GetContractType(s),
                ContractId = GetContractId(s),
                LegalName = GetLegalName(s),
                OrderId = GetOrderId(s)
            }).ToList();

            foreach (var item in resultList)
                if (item.OrderId > 0)
                {
                    var order = orderLogic.GetOrder(item.OrderId);
                    item.OrderNumber = order.Number;
                    item.OrderStatus = orderStatuses.First(w => w.ID == order.OrderStatusId).Display;
                    item.MotivationDate = order.ClosedDate;
                }

            return Content(JsonConvert.SerializeObject(new { Items = resultList, TotalCount = totalCount }));
        }

        public ContentResult ToggleDocumentIsWeekend(int documentId, bool IsWeekend)
        {
            var doc = documentLogic.GetDocument(documentId);
            doc.IsWeekend = IsWeekend;
            doc.WeekendMarkDate = IsWeekend ? DateTime.Now : (DateTime?)null;
            doc.WeekendMarkUserId = IsWeekend ? CurrentUserId : (int?)null;
            documentLogic.UpdateDocument(doc, CurrentUserId);
            return Content(JsonConvert.SerializeObject(new { WeekendMarkDate = doc.WeekendMarkDate, WeekendMarkUserId = doc.WeekendMarkUserId }));
        }

        string GetLegalName(Document s)
        {
            if (s.OrderId.HasValue)
            {
                var order = orderLogic.GetOrder(s.OrderId.Value);
                var contract = contractLogic.GetContract(order.ContractId.Value);
                var legal = legalLogic.GetLegal(contract.LegalId);
                return legal.DisplayName;
            }

            if (s.AccountingId.HasValue)
            {
                var acc = accountingLogic.GetAccounting(s.AccountingId.Value);
                var order = orderLogic.GetOrder(acc.OrderId);
                var contract = contractLogic.GetContract(order.ContractId.Value);
                var legal = legalLogic.GetLegal(contract.LegalId);
                return legal.DisplayName;

                //if (acc.IsIncome)
                //{
                //	var order = orderLogic.GetOrder(acc.OrderId);
                //	var contract = contractLogic.GetContract(order.ContractId.Value);
                //	var legal = legalLogic.GetLegal(contract.LegalId);
                //	return legal.DisplayName;
                //}
                //else
                //{
                //	var contract = contractLogic.GetContract(acc.ContractId.Value);
                //	var legal = legalLogic.GetLegal(contract.LegalId);
                //	return legal.DisplayName;
                //}
            }

            // TODO:
            return "";
        }

        string GetContractNumber(Document s)
        {
            if (s.OrderId.HasValue)
            {
                var order = orderLogic.GetOrder(s.OrderId.Value);
                var contract = contractLogic.GetContract(order.ContractId.Value);
                return contract.Number;
            }

            if (s.AccountingId.HasValue)
            {
                var acc = accountingLogic.GetAccounting(s.AccountingId.Value);
                //if (acc.IsIncome)
                {
                    var order = orderLogic.GetOrder(acc.OrderId);
                    var contract = contractLogic.GetContract(order.ContractId.Value);
                    return contract.Number;
                }
                //else
                //{
                //	var contract = contractLogic.GetContract(acc.ContractId.Value);
                //	return contract.Number;
                //}
            }

            // TODO:
            return "";
        }

        string GetContractType(Document s)
        {
            if (s.OrderId.HasValue)
            {
                var order = orderLogic.GetOrder(s.OrderId.Value);
                var contract = contractLogic.GetContract(order.ContractId.Value);
                return dataLogic.GetContractType(contract.ContractTypeId.Value).Display;
            }

            if (s.AccountingId.HasValue)
            {
                var acc = accountingLogic.GetAccounting(s.AccountingId.Value);
                //if (acc.IsIncome)
                {
                    var order = orderLogic.GetOrder(acc.OrderId);
                    var contract = contractLogic.GetContract(order.ContractId.Value);
                    return dataLogic.GetContractType(contract.ContractTypeId.Value).Display;
                }
                //else
                //{
                //	var contract = contractLogic.GetContract(acc.ContractId.Value);
                //	return dataLogic.GetContractType(contract.ContractTypeId.Value).Display;
                //}
            }

            // TODO:
            return "";
        }

        int? GetContractId(Document s)
        {
            if (s.OrderId.HasValue)
            {
                var order = orderLogic.GetOrder(s.OrderId.Value);
                return order.ContractId;
            }

            if (s.AccountingId.HasValue)
            {
                var acc = accountingLogic.GetAccounting(s.AccountingId.Value);
                //if (acc.IsIncome)
                {
                    var order = orderLogic.GetOrder(acc.OrderId);
                    return order.ContractId;
                }
                //else
                //{
                //	return acc.ContractId;
                //}
            }

            // TODO:
            return null;
        }

        int GetOrderId(Document s)
        {
            if (s.OrderId.HasValue)
                return s.OrderId.Value;

            if (s.AccountingId.HasValue)
                return accountingLogic.GetAccounting(s.AccountingId.Value).OrderId;

            return 0;   // например это документ договора
        }

        void GenerateAccountingDetailedReport(string templateFilename, string resultFilename)
        {
            Debug.WriteLine("Gnrt finres detailed");
            Debug.WriteLine(DateTime.Now.ToString("mm:ss.ms"));
            var fi = new FileInfo(templateFilename);
            var ex = new ExcelPackage(fi);
            var sheet = ex.Workbook.Worksheets[1];
            int currentRow = 3;

            var legals = legalLogic.GetLegals(new ListFilter()).Select(s => new { ID = s.ID, Name = s.DisplayName, ContractorId = s.ContractorId });
            var contracts = contractLogic.GetContracts(new ListFilter()).Select(s => new { ID = s.ID, Number = s.Number, LegalId = s.LegalId, OurLegalId = s.OurLegalId });
            var contractors = contractorLogic.GetContractors(new ListFilter());
            var products = dataLogic.GetProducts();
            var currencies = dataLogic.GetCurrencies();
            var users = dataLogic.GetUsers();
            var finrep = dataLogic.GetFinRepCenters();
            var oull = dataLogic.GetOurLegalLegals();

            IEnumerable<Participant> workgroups;
            using (var db = new LogistoDb())
            {
                workgroups = db.Participants.Where(w => w.ParticipantRoleId == (int)ParticipantRoles.AM || w.ParticipantRoleId == (int)ParticipantRoles.SM || w.ParticipantRoleId == (int)ParticipantRoles.SL).ToList();

                var accountings = db.Accountings.OrderBy(o => o.OrderId).ToList();
                foreach (var accounting in accountings)
                {
                    var order = db.Orders.First(w => w.ID == accounting.OrderId);
                    var contract = contracts.FirstOrDefault(w => w.ID == (accounting.ContractId ?? order.ContractId));
                    var contractorId = legals.Where(w => w.ID == contracts.Where(w2 => w2.ID == (accounting.ContractId ?? order.ContractId)).Select(con => con.LegalId).FirstOrDefault()).Select(con => con.ContractorId ?? 0).FirstOrDefault();
                    var legalId = contract.LegalId;
                    var ourLegalId = contract.OurLegalId;
                    var SL = GetParticipantUser(workgroups, accounting.OrderId, ParticipantRoles.SL, false);
                    var SM = GetParticipantUser(workgroups, accounting.OrderId, ParticipantRoles.SM, false);
                    var AM = GetResponsibleParticipantUser(workgroups, order.ID, order.ClosedDate);
                    var marks = db.AccountingMarks.FirstOrDefault(w => w.AccountingId == accounting.ID) ?? new AccountingMark();
                    var currencyId = db.Services.Where(wde => wde.AccountingId == accounting.ID).Select(s => s.CurrencyId).FirstOrDefault() ?? 1;

                    sheet.Cells[currentRow, 1].Value = (ourLegalId > 0) ? oull.First(w => w.ID == ourLegalId).Display : "";
                    sheet.Cells[currentRow, 2].Value = finrep.Where(w => w.ID == order.FinRepCenterId).Select(s => s.Name).FirstOrDefault();
                    sheet.Cells[currentRow, 3].Value = order.Number;
                    sheet.Cells[currentRow, 4].Value = products.First(w => w.ID == order.ProductId).Display;
                    sheet.Cells[currentRow, 5].Value = accounting.IsIncome ? "Доход" : "Расход";
                    sheet.Cells[currentRow, 6].Value = accounting.Number;
                    sheet.Cells[currentRow, 7].Value = (contractorId > 0) ? contractors.Where(w => w.ID == contractorId).Select(s => s.Name).FirstOrDefault() : "";
                    sheet.Cells[currentRow, 8].Value = contract.Number;
                    sheet.Cells[currentRow, 9].Value = (legalId > 0) ? legals.Where(w => w.ID == legalId).Select(s => s.Name).FirstOrDefault() : "";
                    sheet.Cells[currentRow, 10].Value = accounting.InvoiceNumber;
                    sheet.Cells[currentRow, 11].Value = accounting.InvoiceDate;
                    sheet.Cells[currentRow, 12].Value = accounting.ActDate;
                    sheet.Cells[currentRow, 13].Value = (currencyId > 0) ? currencies.Where(w => w.ID == currencyId).Select(s => s.Display).FirstOrDefault() : "";
                    sheet.Cells[currentRow, 14].Value = accounting.OriginalSum;
                    sheet.Cells[currentRow, 15].Value = accounting.Sum;
                    sheet.Cells[currentRow, 16].Value = order.ClosedDate;
                    sheet.Cells[currentRow, 17].Value = marks.IsInvoiceOk ? marks.InvoiceOkDate : null;
                    sheet.Cells[currentRow, 18].Value = marks.IsInvoiceChecked ? marks.InvoiceCheckedDate : null;

                    sheet.Cells[currentRow, 19].Value = (marks.IsActOk ? marks.ActOkDate : null) ?? (marks.IsAccountingOk ? marks.AccountingOkDate : null);
                    sheet.Cells[currentRow, 20].Value = (marks.IsActChecked ? marks.ActCheckedDate : null) ?? (marks.IsAccountingChecked ? marks.AccountingCheckedDate : null);

                    sheet.Cells[currentRow, 21].Value = (SL > 0) ? users.First(w => w.ID == SL).Display : "";
                    sheet.Cells[currentRow, 22].Value = (SM > 0) ? users.First(w => w.ID == SM).Display : "";
                    sheet.Cells[currentRow, 23].Value = (AM > 0) ? users.First(w => w.ID == AM).Display : "";

                    currentRow++;
                }
            }

            ex.SaveAs(new FileInfo(resultFilename));
            Debug.WriteLine(DateTime.Now.ToString("mm:ss.ms"));
        }

        void GenerateAccountingPerOrderReport(string templateFilename, string resultFilename)
        {
            var fi = new FileInfo(templateFilename);
            var ex = new ExcelPackage(fi);
            var sheet = ex.Workbook.Worksheets[1];
            var legals = legalLogic.GetLegals(new ListFilter()).Select(s => new { ID = s.ID, Name = s.DisplayName, ContractorId = s.ContractorId });
            var contracts = contractLogic.GetContracts(new ListFilter()).Select(s => new { ID = s.ID, Number = s.Number, LegalId = s.LegalId, OurLegalId = s.OurLegalId });
            var contractors = contractorLogic.GetContractors(new ListFilter());
            var products = dataLogic.GetProducts();
            var legalByContract = dataLogic.GetLegalsByContract();
            var accountings = accountingLogic.GetAllAccountings().Select(s => new { s.OrderId, s.IsIncome, s.Sum }).ToList();
            var users = dataLogic.GetUsers();
            var rentabilities = dataLogic.GetOrdersRentability();
            var statuses = dataLogic.GetOrderStatuses();
            var finrep = dataLogic.GetFinRepCenters();
            var oull = dataLogic.GetOurLegalLegals();
            var templates = dataLogic.GetOrderTemplates();

            using (var db = new LogistoDb())
            {
                var workgroups = db.Participants.Where(w => w.ParticipantRoleId == (int)ParticipantRoles.AM || w.ParticipantRoleId == (int)ParticipantRoles.SM || w.ParticipantRoleId == (int)ParticipantRoles.SL).ToList();

                var orders = db.Orders.OrderBy(o => o.ID).ToList();
                int currentRow = 3;
                foreach (var order in orders)
                {
                    var contractorId = legals.Where(w => w.ID == contracts.Where(w2 => w2.ID == order.ContractId).Select(con => con.LegalId).FirstOrDefault()).Select(con => con.ContractorId ?? 0).FirstOrDefault();
                    var ourLegalId = contracts.Where(w2 => w2.ID == order.ContractId).Select(con => con.OurLegalId).FirstOrDefault();
                    var expense = accountings.Where(w => w.OrderId == order.ID && !w.IsIncome).Sum(s => s.Sum);
                    var income = accountings.Where(w => w.OrderId == order.ID && w.IsIncome).Sum(s => s.Sum);
                    var SL = GetParticipantUser(workgroups, order.ID, ParticipantRoles.SL, false);
                    var SM = GetParticipantUser(workgroups, order.ID, ParticipantRoles.SM, false);
                    var AM = GetResponsibleParticipantUser(workgroups, order.ID, order.ClosedDate);
                    var minR = rentabilities.Where(w => w.OrderTemplateId == order.OrderTemplateId && w.FinRepCenterId == order.FinRepCenterId).Select(s => s.Rentability).FirstOrDefault();

                    int index = 1;
                    sheet.Cells[currentRow, index++].Value = (ourLegalId > 0) ? oull.First(w => w.ID == ourLegalId).Display : "";
                    sheet.Cells[currentRow, index++].Value = finrep.Where(w => w.ID == order.FinRepCenterId).Select(s => s.Name).FirstOrDefault();
                    sheet.Cells[currentRow, index++].Value = order.Number;
                    sheet.Cells[currentRow, index++].Value = order.CreatedDate;
                    sheet.Cells[currentRow, index++].Value = statuses.Where(w => w.ID == order.OrderStatusId).Select(s => s.Display).FirstOrDefault();
                    sheet.Cells[currentRow, index++].Value = products.First(w => w.ID == order.ProductId).Display;
                    sheet.Cells[currentRow, index++].Value = templates.Where(w => w.ID == order.OrderTemplateId).Select(s => s.Name).FirstOrDefault();
                    sheet.Cells[currentRow, index++].Value = (contractorId > 0) ? contractors.Where(w => w.ID == contractorId).Select(s => s.Name).FirstOrDefault() : "";
                    sheet.Cells[currentRow, index++].Value = contracts.Where(w => w.ID == order.ContractId).Select(s => s.Number).FirstOrDefault();
                    sheet.Cells[currentRow, index++].Value = legalByContract.Where(w => w.ID == order.ContractId).Select(s => s.Display).FirstOrDefault();
                    sheet.Cells[currentRow, index++].Value = order.From;
                    sheet.Cells[currentRow, index++].Value = order.To;
                    sheet.Cells[currentRow, index++].Value = order.ClosedDate;
                    sheet.Cells[currentRow, index++].Value = income;
                    sheet.Cells[currentRow, index++].Value = expense;
                    sheet.Cells[currentRow, index++].Value = income - expense;
                    sheet.Cells[currentRow, index++].Value = ((income - expense) / (income ?? 1));
                    sheet.Cells[currentRow, index++].Value = minR / 100;
                    sheet.Cells[currentRow, index++].Value = (SL > 0) ? users.First(w => w.ID == SL).Display : "";
                    sheet.Cells[currentRow, index++].Value = (SM > 0) ? users.First(w => w.ID == SM).Display : "";
                    sheet.Cells[currentRow, index++].Value = (AM > 0) ? users.First(w => w.ID == AM).Display : "";

                    currentRow++;
                }
            }

            ex.SaveAs(new FileInfo(resultFilename));
        }

        void GenerateDebtsReport(string templateFilename, string resultFilename)
        {
            Debug.WriteLine("Gnrt");
            Debug.WriteLine(DateTime.Now.ToString("mm:ss.ms"));
            var fi = new FileInfo(templateFilename);
            var ex = new ExcelPackage(fi);
            var sheet = ex.Workbook.Worksheets[1];
            int currentRow = 3;

            var legals = legalLogic.GetLegals(new ListFilter()).Select(s => new { ID = s.ID, Name = s.DisplayName, ContractorId = s.ContractorId }).ToList();
            var contracts = contractLogic.GetContracts(new ListFilter()).Select(s => new { ID = s.ID, Number = s.Number, LegalId = s.LegalId, OurLegalId = s.OurLegalId }).ToList();
            var contractors = contractorLogic.GetContractors(new ListFilter());
            var currencies = dataLogic.GetCurrencies();
            var finrep = dataLogic.GetFinRepCenters();
            var payMethods = dataLogic.GetPayMethods();
            var paymentMethods = dataLogic.GetAccountingPaymentMethods();
            var oull = dataLogic.GetOurLegalLegals();

            // доходы
            using (var db = new LogistoDb())
            {
                var list = db.Accountings.Where(w => w.IsIncome).ToList();
                foreach (var accounting in list)
                {
                    var finRepCenterId = db.Orders.Where(w => w.ID == accounting.OrderId).Select(s => s.FinRepCenterId).First();
                    var contractId = db.Orders.Where(w => w.ID == accounting.OrderId).Select(s => s.ContractId.Value).First();
                    var contract = contracts.FirstOrDefault(w => w.ID == contractId);
                    var contractorId = legals.Where(w => w.ID == contract.LegalId).Select(con => con.ContractorId ?? 0).FirstOrDefault();
                    var legalId = accounting.LegalId;
                    var ourLegalId = contract.OurLegalId;
                    var marks = db.AccountingMarks.FirstOrDefault(w => w.AccountingId == accounting.ID) ?? new AccountingMark();
                    var currencyId = db.Services.Where(wde => wde.AccountingId == accounting.ID).Select(s => s.CurrencyId).FirstOrDefault();
                    if (currencyId == null)
                        continue;

                    var col = 1;
                    sheet.Cells[currentRow, col++].Value = (ourLegalId > 0) ? oull.First(w => w.ID == ourLegalId).Display : "";
                    sheet.Cells[currentRow, col++].Value = finrep.Where(w => w.ID == finRepCenterId).Select(s => s.Name).FirstOrDefault();
                    sheet.Cells[currentRow, col++].Value = paymentMethods.Where(w => w.ID == accounting.AccountingPaymentMethodId).Select(s => s.Display).FirstOrDefault();
                    sheet.Cells[currentRow, col++].Value = (contractorId > 0) ? contractors.Where(w => w.ID == contractorId).Select(s => s.Name).FirstOrDefault() : "";
                    sheet.Cells[currentRow, col++].Value = contract.Number;
                    sheet.Cells[currentRow, col++].Value = (legalId > 0) ? legals.Where(w => w.ID == legalId).Select(s => s.Name).FirstOrDefault() : "";
                    sheet.Cells[currentRow, col++].Value = accounting.Number;
                    sheet.Cells[currentRow, col++].Value = accounting.InvoiceDate;
                    sheet.Cells[currentRow, col++].Value = currencies.Where(w => w.ID == currencyId).Select(s => s.Display).FirstOrDefault();
                    sheet.Cells[currentRow, col++].Value = accounting.OriginalSum;
                    sheet.Cells[currentRow, col++].Value = accounting.Payment;
                    col++;
                    sheet.Cells[currentRow, col++].Value = accounting.PaymentPlanDate;
                    sheet.Cells[currentRow, col++].Value = marks.InvoiceOkDate;
                    sheet.Cells[currentRow, col++].Value = marks.InvoiceCheckedDate;
                    sheet.Cells[currentRow, col++].Value = marks.ActOkDate;
                    sheet.Cells[currentRow, col++].Value = marks.ActCheckedDate;

                    currentRow++;
                }
            }

            sheet = ex.Workbook.Worksheets[2];
            currentRow = 3;
            // расходы
            using (var db = new LogistoDb())
            {
                var list = db.Accountings.Where(w => !w.IsIncome).ToList();
                foreach (var accounting in list)
                {
                    if (!accounting.ContractId.HasValue)
                        continue;

                    var finRepCenterId = db.Orders.Where(w => w.ID == accounting.OrderId).Select(s => s.FinRepCenterId).First();
                    var contractId = accounting.ContractId.Value;
                    var contract = contracts.FirstOrDefault(w => w.ID == contractId);
                    var contractorId = legals.Where(w => w.ID == contract.LegalId).Select(con => con.ContractorId ?? 0).FirstOrDefault();
                    var legalId = contract.LegalId;
                    var ourLegalId = contract.OurLegalId;
                    var marks = db.AccountingMarks.FirstOrDefault(w => w.AccountingId == accounting.ID) ?? new AccountingMark();
                    var currencyId = db.Services.Where(wde => wde.AccountingId == accounting.ID).Select(s => s.CurrencyId).FirstOrDefault();
                    if (currencyId == null)
                        continue;

                    var col = 1;
                    sheet.Cells[currentRow, col++].Value = (ourLegalId > 0) ? legals.Where(w => w.ID == ourLegalId).Select(s => s.Name).FirstOrDefault() : "";
                    sheet.Cells[currentRow, col++].Value = finrep.Where(w => w.ID == finRepCenterId).Select(s => s.Name).FirstOrDefault();
                    sheet.Cells[currentRow, col++].Value = payMethods.Where(w => w.ID == accounting.PayMethodId).Select(s => s.Display).FirstOrDefault();
                    sheet.Cells[currentRow, col++].Value = (contractorId > 0) ? contractors.Where(w => w.ID == contractorId).Select(s => s.Name).FirstOrDefault() : "";
                    sheet.Cells[currentRow, col++].Value = contract.Number;
                    sheet.Cells[currentRow, col++].Value = (legalId > 0) ? legals.Where(w => w.ID == legalId).Select(s => s.Name).FirstOrDefault() : "";
                    sheet.Cells[currentRow, col++].Value = accounting.Number;
                    sheet.Cells[currentRow, col++].Value = accounting.InvoiceNumber;
                    sheet.Cells[currentRow, col++].Value = accounting.InvoiceDate;
                    sheet.Cells[currentRow, col++].Value = currencies.Where(w => w.ID == currencyId).Select(s => s.Display).FirstOrDefault();
                    sheet.Cells[currentRow, col++].Value = accounting.OriginalSum;
                    sheet.Cells[currentRow, col++].Value = accounting.Payment;
                    col++;
                    sheet.Cells[currentRow, col++].Value = accounting.PaymentPlanDate;
                    sheet.Cells[currentRow, col++].Value = marks.AccountingOkDate;
                    sheet.Cells[currentRow, col++].Value = marks.AccountingCheckedDate;

                    currentRow++;
                }
            }

            ex.SaveAs(new FileInfo(resultFilename));
            Debug.WriteLine(DateTime.Now.ToString("mm:ss.ms"));
        }

        void GenerateDebtsReport_Roled(string templateFilename, string resultFilename)
        {
            var fi = new FileInfo(templateFilename);
            var ex = new ExcelPackage(fi);
            var sheet = ex.Workbook.Worksheets[1];
            int currentRow = 3;

            var legals = legalLogic.GetLegals(new ListFilter()).Select(s => new { ID = s.ID, Name = s.DisplayName, ContractorId = s.ContractorId }).ToList();
            var contracts = contractLogic.GetContracts(new ListFilter()).Select(s => new { ID = s.ID, Number = s.Number, LegalId = s.LegalId, OurLegalId = s.OurLegalId }).ToList();
            var contractors = contractorLogic.GetContractors(new ListFilter());
            var currencies = dataLogic.GetCurrencies();
            var userId = CurrentUserId;
            var finrep = dataLogic.GetFinRepCenters();
            var oull = dataLogic.GetOurLegalLegals();
            var payMethods = dataLogic.GetPayMethods();
            var paymentMethods = dataLogic.GetAccountingPaymentMethods();

            // доходы
            using (var db = new LogistoDb())
            {
                var accountings = db.Accountings.Where(w => w.IsIncome).ToList();
                foreach (var accounting in accountings)
                {
                    var wg = db.Participants.Where(w => w.OrderId == accounting.OrderId && w.UserId == userId).Select(s => s.ParticipantRoleId).ToList();
                    if (wg.Count() == 0)
                        continue;

                    if (!wg.Exists(w => w == 1 || w == 2 || w == 3 || w == 4 || w == 6))
                        continue;

                    var order = db.Orders.First(w => w.ID == accounting.OrderId);
                    var contractId = db.Orders.Where(w => w.ID == accounting.OrderId).Select(s => s.ContractId.Value).First();
                    var contract = contracts.FirstOrDefault(w => w.ID == contractId);
                    var contractorId = legals.Where(w => w.ID == contracts.Where(w2 => w2.ID == contractId).Select(con => con.LegalId).FirstOrDefault()).Select(con => con.ContractorId ?? 0).FirstOrDefault();
                    var legalId = accounting.LegalId;
                    var ourLegalId = contract.OurLegalId;
                    var service = db.Services.Where(wde => wde.AccountingId == accounting.ID).FirstOrDefault();
                    if (service == null)
                        continue;

                    var currencyId = service.CurrencyId ?? 1;
                    //var contractCurrencies = db.ContractCurrencies.Where(w => w.ContractId == contractId).ToList();
                    //var currency = contractCurrencies.FirstOrDefault(w => w.CurrencyId == currencyId);
                    //if (currency == null)
                    //	currency = contractCurrencies.FirstOrDefault(w => w.CurrencyId == 1);   // рубли присутствуют обязательно в этом случае

                    //if (currency == null)
                    //	continue;

                    var marks = accountingLogic.GetAccountingMarkByAccounting(accounting.ID) ?? new AccountingMark();
                    var col = 1;
                    sheet.Cells[currentRow, col++].Value = (ourLegalId > 0) ? oull.First(w => w.ID == ourLegalId).Display : "";
                    sheet.Cells[currentRow, col++].Value = finrep.Where(w => w.ID == order.FinRepCenterId).Select(s => s.Name).FirstOrDefault();
                    sheet.Cells[currentRow, col++].Value = paymentMethods.Where(w => w.ID == accounting.AccountingPaymentMethodId).Select(s => s.Display).FirstOrDefault();
                    sheet.Cells[currentRow, col++].Value = (contractorId > 0) ? contractors.Where(w => w.ID == contractorId).Select(s => s.Name).FirstOrDefault() : "";
                    sheet.Cells[currentRow, col++].Value = contract.Number;
                    sheet.Cells[currentRow, col++].Value = (legalId > 0) ? legals.Where(w => w.ID == legalId).Select(s => s.Name).FirstOrDefault() : "";
                    sheet.Cells[currentRow, col++].Value = accounting.Number;
                    sheet.Cells[currentRow, col++].Value = accounting.InvoiceDate;
                    sheet.Cells[currentRow, col++].Value = currencies.Where(w => w.ID == currencyId).Select(s => s.Display).FirstOrDefault();
                    sheet.Cells[currentRow, col++].Value = accounting.OriginalSum;
                    sheet.Cells[currentRow, col++].Value = accounting.Payment;
                    col++;
                    sheet.Cells[currentRow, col++].Value = accounting.PaymentPlanDate;
                    sheet.Cells[currentRow, col++].Value = marks.InvoiceOkDate;
                    sheet.Cells[currentRow, col++].Value = marks.InvoiceCheckedDate;
                    sheet.Cells[currentRow, col++].Value = marks.ActOkDate;
                    sheet.Cells[currentRow, col++].Value = marks.ActCheckedDate;

                    currentRow++;
                }
            }

            sheet = ex.Workbook.Worksheets[2];
            currentRow = 4;
            // расходы
            using (var db = new LogistoDb())
            {
                var accountings = db.Accountings.Where(w => !w.IsIncome).ToList();
                foreach (var accounting in accountings)
                {
                    var wg = db.Participants.Where(w => w.OrderId == accounting.OrderId && w.UserId == userId).Select(s => s.ParticipantRoleId).ToList();
                    if (wg.Count() == 0)
                        continue;

                    if (!wg.Exists(w => w == 2 || w == 3 || w == 4 || w == 6))
                        continue;

                    if (!accounting.ContractId.HasValue)
                        continue;

                    var order = db.Orders.First(w => w.ID == accounting.OrderId);
                    var contractId = accounting.ContractId.Value;
                    var contract = contracts.FirstOrDefault(w => w.ID == contractId);
                    var contractorId = legals.Where(w => w.ID == contracts.Where(w2 => w2.ID == contractId).Select(con => con.LegalId).FirstOrDefault()).Select(con => con.ContractorId ?? 0).FirstOrDefault();
                    var legalId = contract.LegalId;
                    var ourLegalId = contract.OurLegalId;
                    var service = db.Services.Where(wde => wde.AccountingId == accounting.ID).FirstOrDefault();
                    if (service == null)
                        continue;

                    var currencyId = service.CurrencyId ?? 1;

                    var marks = accountingLogic.GetAccountingMarkByAccounting(accounting.ID) ?? new AccountingMark();
                    var col = 1;
                    sheet.Cells[currentRow, col++].Value = (ourLegalId > 0) ? legals.Where(w => w.ID == ourLegalId).Select(s => s.Name).FirstOrDefault() : "";
                    sheet.Cells[currentRow, col++].Value = finrep.Where(w => w.ID == order.FinRepCenterId).Select(s => s.Name).FirstOrDefault();
                    sheet.Cells[currentRow, col++].Value = payMethods.Where(w => w.ID == accounting.PayMethodId).Select(s => s.Display).FirstOrDefault();
                    sheet.Cells[currentRow, col++].Value = (contractorId > 0) ? contractors.Where(w => w.ID == contractorId).Select(s => s.Name).FirstOrDefault() : "";
                    sheet.Cells[currentRow, col++].Value = contract.Number;
                    sheet.Cells[currentRow, col++].Value = (legalId > 0) ? legals.Where(w => w.ID == legalId).Select(s => s.Name).FirstOrDefault() : "";
                    sheet.Cells[currentRow, col++].Value = accounting.Number;
                    sheet.Cells[currentRow, col++].Value = accounting.InvoiceNumber;
                    sheet.Cells[currentRow, col++].Value = accounting.InvoiceDate;
                    sheet.Cells[currentRow, col++].Value = currencies.Where(w => w.ID == currencyId).Select(s => s.Display).FirstOrDefault();
                    sheet.Cells[currentRow, col++].Value = accounting.OriginalSum;
                    sheet.Cells[currentRow, col++].Value = accounting.Payment;
                    col++;
                    sheet.Cells[currentRow, col++].Value = accounting.PaymentPlanDate;
                    sheet.Cells[currentRow, col++].Value = marks.AccountingOkDate;
                    sheet.Cells[currentRow, col++].Value = marks.AccountingCheckedDate;

                    currentRow++;
                }
            }

            ex.SaveAs(new FileInfo(resultFilename));
        }

        bool GenerateAccountsReceivable(string templateFilename, string resultFilename, int userId)
        {
            bool hasRecords = false;
            var fi = new FileInfo(templateFilename);
            var ex = new ExcelPackage(fi);
            var sheet = ex.Workbook.Worksheets[1];
            int currentRow = 3;

            var legals = legalLogic.GetLegals(new ListFilter()).Select(s => new { ID = s.ID, Name = s.DisplayName, ContractorId = s.ContractorId }).ToList();
            var contracts = contractLogic.GetContracts(new ListFilter()).Select(s => new { ID = s.ID, Number = s.Number, LegalId = s.LegalId, OurLegalId = s.OurLegalId }).ToList();
            var contractors = contractorLogic.GetContractors(new ListFilter());
            var currencies = dataLogic.GetCurrencies();
            var oull = dataLogic.GetOurLegalLegals();

            // доходы
            using (var db = new LogistoDb())
            {
                var accountings = db.Accountings.Where(w => w.IsIncome).ToList();
                foreach (var accounting in accountings)
                {
                    var wg = db.Participants.Where(w => w.OrderId == accounting.OrderId && w.UserId == userId).ToList();
                    wg = FilterByDates(wg).ToList();
                    if (wg.Count() == 0)
                        continue;

                    if (!wg.Exists(w => ((w.ParticipantRoleId == (int)ParticipantRoles.AM) && w.IsResponsible) ||
                                    ((w.ParticipantRoleId == (int)ParticipantRoles.AM) && w.IsDeputy) ||
                                    (w.ParticipantRoleId == (int)ParticipantRoles.SM) ||
                                    (w.ParticipantRoleId == (int)ParticipantRoles.SL)
                                    ))
                        continue;

                    if (Math.Round(accounting.Payment ?? 0, 4) >= Math.Round(accounting.OriginalSum ?? 0, 4))
                        continue;

                    var orderStatusId = db.Orders.Where(w => w.ID == accounting.OrderId).Select(s => s.OrderStatusId).First();
                    if (orderStatusId == (int)OrderStatuses.Motivation)   // кроме "Мотивация"
                        continue;

                    var contractId = db.Orders.Where(w => w.ID == accounting.OrderId).Select(s => s.ContractId.Value).First();
                    var contract = contracts.FirstOrDefault(w => w.ID == contractId);
                    var contractorId = legals.Where(w => w.ID == contracts.Where(w2 => w2.ID == contractId).Select(con => con.LegalId).FirstOrDefault()).Select(con => con.ContractorId ?? 0).FirstOrDefault();
                    var legalId = accounting.LegalId;
                    var ourLegalId = contract.OurLegalId;
                    var service = db.Services.Where(w => w.AccountingId == accounting.ID).FirstOrDefault();
                    if (service == null)
                        continue;

                    var currencyId = service.CurrencyId ?? 1;

                    var marks = accountingLogic.GetAccountingMarkByAccounting(accounting.ID) ?? new AccountingMark();

                    var col = 1;
                    sheet.Cells[currentRow, col++].Value = (ourLegalId > 0) ? oull.First(w => w.ID == ourLegalId).Display : "";
                    sheet.Cells[currentRow, col++].Value = (contractorId > 0) ? contractors.Where(w => w.ID == contractorId).Select(s => s.Name).FirstOrDefault() : "";
                    sheet.Cells[currentRow, col++].Value = contract.Number;
                    sheet.Cells[currentRow, col++].Value = (legalId > 0) ? legals.Where(w => w.ID == legalId).Select(s => s.Name).FirstOrDefault() : "";
                    sheet.Cells[currentRow, col++].Value = accounting.Number;
                    sheet.Cells[currentRow, col++].Value = accounting.InvoiceDate;
                    sheet.Cells[currentRow, col++].Value = currencies.Where(w => w.ID == currencyId).Select(s => s.Display).FirstOrDefault();
                    sheet.Cells[currentRow, col++].Value = accounting.OriginalSum;
                    sheet.Cells[currentRow, col++].Value = accounting.Payment;
                    col++;
                    sheet.Cells[currentRow, col++].Value = accounting.PaymentPlanDate;
                    sheet.Cells[currentRow, col++].Value = marks.InvoiceOkDate;
                    sheet.Cells[currentRow, col++].Value = marks.InvoiceCheckedDate;
                    sheet.Cells[currentRow, col++].Value = marks.ActOkDate;
                    sheet.Cells[currentRow, col++].Value = marks.ActCheckedDate;

                    currentRow++;
                    hasRecords = true;
                }
            }

            ex.SaveAs(new FileInfo(resultFilename));
            return hasRecords;
        }

        void GenerateCrmReport(ListFilter filter, string templateFilename, string resultFilename)
        {
            var crmLogic = new CrmLogic();
            var fi = new FileInfo(templateFilename);
            var ex = new ExcelPackage(fi);
            var sheet = ex.Workbook.Worksheets[1];
            int currentRow = 6;

            sheet.Cells[1, 2].Value = DateTime.Now;

            filter.PageNumber = 0;
            filter.PageSize = 0;
            filter.From = filter.From?.Date;
            filter.To = filter.To?.Date;

            // полный список звонков для выбранного пользователя / всех пользователей
            var list = crmLogic.GetCalls(filter).ToList();
            var users = userLogic.GetUsers(new ListFilter());
            foreach (var item in list)
            {
                item.UserId = users.Where(w => w.CrmId == item.ID_MANAGER).Select(s => s.ID).FirstOrDefault();
                if (item.To.HasValue)
                    item.Duration = (item.To.Value - item.From).ToString();
            }

            // разбиение на надельные блоки
            var rangeFrom = filter.From.Value;
            var rangeTo = filter.From.Value;
            while (rangeTo.Date <= filter.To.Value.Date)
            {
                while ((rangeTo.DayOfWeek != DayOfWeek.Sunday) && (rangeTo <= filter.To.Value))
                    rangeTo = rangeTo.AddDays(1);

                var days = (rangeTo - rangeFrom).Days;
                days = (rangeTo.DayOfWeek == DayOfWeek.Sunday) ? days - 1 : days;
                // список звонков для всех пользователей в этом периоде
                var range = list.Where(w => w.Date >= rangeFrom && w.Date <= rangeTo).ToList();
                var contracts = contractLogic.GetContracts(new ListFilter { From = rangeFrom, To = rangeTo });
                var managers = range.Where(w => w.UserId > 0).Select(s => s.UserId.Value).Distinct().ToList();
                foreach (var managerId in managers)
                {
                    // по каждому менеджеру
                    var managerCalls = range.Where(w => w.UserId == managerId).ToList();
                    var calls = managerCalls.Where(w => w.ID_COMPANY > 0).GroupBy(g => g.ID_COMPANY).Select(m => m.OrderBy(o => o.ID).First()).ToList();
                    int newClients = 0;
                    foreach (var item in calls)
                        if (crmLogic.IsFirstCall(item))
                            newClients++;

                    var requests = requestLogic.GetRequests(new ListFilter { From = rangeFrom, To = rangeTo, UserId = managerId });
                    int newContracts = 0;
                    foreach (var contract in contracts)
                    {
                        var legal = legalLogic.GetLegal(contract.LegalId);
                        var wg = participantLogic.GetWorkgroupByContractor(legal.ContractorId.Value).Where(w => w.UserId == managerId);
                        if (IsParticipantAt(wg, ParticipantRoles.SM, contract.Date.Value))
                            newContracts++;
                    }

                    days = managerCalls.Select(s => s.Date.Date).Distinct().Count();

                    var col = 1;
                    sheet.Cells[currentRow, col++].Value = personLogic.GetPerson(users.First(w => w.ID == managerId).PersonId.Value).DisplayName;
                    sheet.Cells[currentRow, col++].Value = GetIso8601WeekOfYear(rangeFrom);
                    sheet.Cells[currentRow, col++].Value = calls.Count();
                    sheet.Cells[currentRow, col++].Value = newClients;
                    sheet.Cells[currentRow, col++].Value = managerCalls.Count();
                    sheet.Cells[currentRow, col++].Value = managerCalls.Count() / (double)days;
                    sheet.Cells[currentRow, col++].Value = days;
                    sheet.Cells[currentRow, col++].Value = TimeSpan.FromTicks(managerCalls.Sum(s => TimeSpan.Parse(s.Duration).Ticks)).TotalMinutes;
                    sheet.Cells[currentRow, col++].Value = calls.Count() == 0 ? 0 : TimeSpan.FromTicks(managerCalls.Sum(s => TimeSpan.Parse(s.Duration).Ticks) / calls.Count()).TotalMinutes;
                    sheet.Cells[currentRow, col++].Value = requests.Count();
                    sheet.Cells[currentRow, col++].Value = calls.Count() == 0 ? 0 : ((float)requests.Count() / calls.Count());
                    sheet.Cells[currentRow, col++].Value = newContracts;

                    currentRow++;
                }

                sheet.Row(currentRow - 1).Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                rangeFrom = rangeTo;
                rangeFrom = rangeFrom.AddDays(1);
                rangeTo = rangeTo.AddDays(1);
            }

            ex.SaveAs(new FileInfo(resultFilename));
        }

        void GenerateClientsReport(ListFilter filter, string templateFilename, string resultFilename)
        {
            var fi = new FileInfo(templateFilename);
            var ex = new ExcelPackage(fi);
            var sheet = ex.Workbook.Worksheets[1];
            int currentRow = 2;

            filter.PageNumber = 0;
            filter.PageSize = 999999;

            #region получение данных

            var list = contractorLogic.GetContractors(filter);
            var resultList = list.Select(s => new ContractorViewModel { Contractor = s }).ToList();

            foreach (var item in resultList)
            {
                var wg = participantLogic.GetWorkgroupByContractor(item.Contractor.ID);
                wg = FilterByDates(wg);

                var sm = wg.FirstOrDefault(w => w.ParticipantRoleId == (int)ParticipantRoles.SM);
                if ((sm != null) && sm.UserId.HasValue)
                {
                    item.SM = sm;
                    item.SMDisplay = personLogic.GetPersonByUser(sm.UserId.Value).DisplayName;
                }

                var am = wg.FirstOrDefault(w => (w.ParticipantRoleId == (int)ParticipantRoles.AM) && w.IsResponsible);
                if ((am != null) && am.UserId.HasValue)
                {
                    item.ResponsibleAM = am;
                    item.ResponsibleAMDisplay = personLogic.GetPersonByUser(am.UserId.Value).DisplayName;
                }

                var dam = wg.FirstOrDefault(w => (w.ParticipantRoleId == (int)ParticipantRoles.AM) && w.IsDeputy);
                if ((dam != null) && dam.UserId.HasValue)
                {
                    item.DeputyAM = dam;
                    item.DeputyAMDisplay = personLogic.GetPersonByUser(dam.UserId.Value).DisplayName;
                }

                var sl = wg.FirstOrDefault(w => w.ParticipantRoleId == (int)ParticipantRoles.SL);
                if ((sl != null) && sl.UserId.HasValue)
                {
                    item.SL = sl;
                    item.SLDisplay = personLogic.GetPersonByUser(sl.UserId.Value).DisplayName;
                }
            }

            #endregion

            foreach (var item in resultList)
            {
                sheet.Cells[currentRow, 1].Value = item.Contractor.Name;
                sheet.Cells[currentRow, 2].Value = item.SLDisplay;
                sheet.Cells[currentRow, 3].Value = item.SL?.FromDate;
                sheet.Cells[currentRow, 4].Value = item.SL?.ToDate;
                sheet.Cells[currentRow, 5].Value = item.SMDisplay;
                sheet.Cells[currentRow, 6].Value = item.SM?.FromDate;
                sheet.Cells[currentRow, 7].Value = item.SM?.ToDate;
                sheet.Cells[currentRow, 8].Value = item.ResponsibleAMDisplay;
                sheet.Cells[currentRow, 9].Value = item.ResponsibleAM?.FromDate;
                sheet.Cells[currentRow, 10].Value = item.ResponsibleAM?.ToDate;
                sheet.Cells[currentRow, 11].Value = item.DeputyAMDisplay;
                sheet.Cells[currentRow, 12].Value = item.DeputyAM?.FromDate;
                sheet.Cells[currentRow, 13].Value = item.DeputyAM?.ToDate;

                currentRow++;
            }

            ex.SaveAs(new FileInfo(resultFilename));
        }

        void GenerateCrmLegalsReport(ListFilter filter, string templateFilename, string resultFilename)
        {
            var fi = new FileInfo(templateFilename);
            var ex = new ExcelPackage(fi);
            var sheet = ex.Workbook.Worksheets[1];
            int currentRow = 2;

            filter.PageNumber = 0;
            filter.PageSize = 999999;

            #region получение данных

            var crmLogic = new CrmLogic();
            var list = crmLogic.GetLegals(filter);

            #endregion

            foreach (var item in list)
            {
                sheet.Cells[currentRow, 1].Value = item.CompanyFullName;
                sheet.Cells[currentRow, 2].Value = item.CompanyName;
                sheet.Cells[currentRow, 3].Value = item.TIN;
                sheet.Cells[currentRow, 4].Value = item.KPP;
                sheet.Cells[currentRow, 5].Value = item.OGRN;
                sheet.Cells[currentRow, 6].Value = item.LegalAddress;
                sheet.Cells[currentRow, 7].Value = item.Address;
                sheet.Cells[currentRow, 8].Value = item.PostAddress;

                currentRow++;
            }

            ex.SaveAs(new FileInfo(resultFilename));
        }

        void GenerateDeclarationsReport(ListFilter filter, string templateFilename, string resultFilename)
        {
            var fi = new FileInfo(templateFilename);
            var ex = new ExcelPackage(fi);
            var sheet = ex.Workbook.Worksheets[1];
            int currentRow = 2;

            filter.PageNumber = 0;
            filter.PageSize = 0;

            #region получение данных

            var list = documentLogic.GetDeclarationDocuments(filter);
            var orderStatuses = dataLogic.GetOrderStatuses();
            var resultList = list.Select(s => new DeclarationViewModel
            {
                DocumentId = s.ID,
                DeclarationNumber = s.Number,
                IsWeekend = s.IsWeekend,
                WeekendMarkDate = s.WeekendMarkDate,
                WeekendMarkUserId = s.WeekendMarkUserId,
                WeekendMarkUser = s.WeekendMarkUserId.HasValue ? personLogic.GetPersonByUser(s.WeekendMarkUserId.Value)?.DisplayName : "",
                ContractNumber = GetContractNumber(s),
                ContractType = GetContractType(s),
                LegalName = GetLegalName(s),
                OrderId = GetOrderId(s)
            }).GroupBy(g => new { g.DeclarationNumber, g.OrderId }).Select(s => s.First()).ToList();

            foreach (var item in resultList)
            {
                if (item.OrderId > 0)
                {
                    var order = orderLogic.GetOrder(item.OrderId);
                    item.OrderNumber = order.Number;
                    item.OrderStatus = orderStatuses.First(w => w.ID == order.OrderStatusId).Display;
                    item.MotivationDate = order.ClosedDate;
                }
            }

            #endregion

            foreach (var item in resultList)
            {
                sheet.Cells[currentRow, 1].Value = item.OrderNumber;
                sheet.Cells[currentRow, 2].Value = item.OrderStatus;
                sheet.Cells[currentRow, 3].Value = item.MotivationDate;
                sheet.Cells[currentRow, 4].Value = item.LegalName;
                sheet.Cells[currentRow, 5].Value = item.ContractNumber;
                sheet.Cells[currentRow, 6].Value = item.ContractType;
                sheet.Cells[currentRow, 7].Value = item.DeclarationNumber;
                sheet.Cells[currentRow, 8].Value = item.IsWeekend ? "ДА" : "нет";
                sheet.Cells[currentRow, 9].Value = item.WeekendMarkUser;
                sheet.Cells[currentRow, 10].Value = item.WeekendMarkDate;

                currentRow++;
            }

            ex.SaveAs(new FileInfo(resultFilename));
        }

        int GetParticipantUser(IEnumerable<Participant> workgroup, int orderId, ParticipantRoles role, bool onlyResponsible, bool dontCheckDates = false)
        {
            var oms = workgroup.Where(w => w.OrderId == orderId && w.ParticipantRoleId == (int)role);
            foreach (var item in oms)
            {
                if (onlyResponsible && !item.IsResponsible)
                    continue;

                if (dontCheckDates)
                    return item.UserId.Value;

                if (!item.FromDate.HasValue && !item.ToDate.HasValue)
                    return item.UserId.Value;

                if ((item.FromDate.HasValue && !item.ToDate.HasValue) && (item.FromDate < DateTime.Now))
                    return item.UserId.Value;

                if ((!item.FromDate.HasValue && item.ToDate.HasValue) && (item.ToDate > DateTime.Now))
                    return item.UserId.Value;

                if ((item.FromDate.HasValue && item.ToDate.HasValue) && (item.FromDate < DateTime.Now) && (item.ToDate > DateTime.Now))
                    return item.UserId.Value;
            }

            return 0;
        }

        int GetResponsibleParticipantUser(IEnumerable<Participant> workgroup, int orderId, DateTime? date = null)
        {
            var checkDate = date ?? DateTime.Now;
            var oms = workgroup.Where(w => w.IsResponsible && (w.OrderId == orderId) && (w.ParticipantRoleId == (int)ParticipantRoles.AM));
            foreach (var item in oms)
            {
                if (!item.FromDate.HasValue && !item.ToDate.HasValue)
                    return item.UserId.Value;

                if ((item.FromDate.HasValue && !item.ToDate.HasValue) && (item.FromDate < checkDate))
                    return item.UserId.Value;

                if ((!item.FromDate.HasValue && item.ToDate.HasValue) && (item.ToDate > checkDate))
                    return item.UserId.Value;

                if ((item.FromDate.HasValue && item.ToDate.HasValue) && (item.FromDate < checkDate) && (item.ToDate > checkDate))
                    return item.UserId.Value;
            }

            return 0;
        }

        int GetIso8601WeekOfYear(DateTime time)
        {
            // Seriously cheat.  If its Monday, Tuesday or Wednesday, then it'll 
            // be the same week# as whatever Thursday, Friday or Saturday are,
            // and we always get those right
            DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(time);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
                time = time.AddDays(3);

            // Return the week of our adjusted day
            return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(time, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }
    }
}






