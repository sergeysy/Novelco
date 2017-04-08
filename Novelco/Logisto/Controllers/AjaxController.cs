using System;
using System.Linq;
using System.Web.Mvc;
using Logisto.Models;
using Newtonsoft.Json;

namespace Logisto.Controllers
{
	public class AjaxController : BaseController
	{
		public ContentResult GetPlace(int id)
		{
			var place = dataLogic.GetPlace(id);
			return Content(JsonConvert.SerializeObject(place));
		}

		public ContentResult GetLegal(int id)
		{
			var legal = legalLogic.GetLegal(id);
			return Content(JsonConvert.SerializeObject(legal));
		}

		public ContentResult GetBank(int id)
		{
			var result = bankLogic.GetBank(id);
			return Content(JsonConvert.SerializeObject(result));
		}

		public ContentResult GetCurrencyRate(DateTime? date)
		{
			var list = dataLogic.GetCurrencyRates(date ?? DateTime.Today);
			return Content(JsonConvert.SerializeObject(new { Items = list }));
		}

		public ContentResult GetUser(int id)
		{
			var result = userLogic.GetUser(id);
			return Content(JsonConvert.SerializeObject(result));
		}

		public ContentResult GetIdentityUser(int id)
		{
			var result = identityLogic.GetUser(id);
			return Content(JsonConvert.SerializeObject(result));
		}

		public ContentResult GetPerson(int id)
		{
			var result = personLogic.GetPerson(id);
			return Content(JsonConvert.SerializeObject(result));
		}

		public ContentResult GetContract(int id)
		{
			var s = contractLogic.GetContract(id);
			return Content(JsonConvert.SerializeObject(s));
		}

		public ContentResult GetContractor(int id)
		{
			var s = contractorLogic.GetContractor(id);
			return Content(JsonConvert.SerializeObject(s));
		}

		public ContentResult GetContractorByContract(int contractId)
		{
			var contract = contractLogic.GetContract(contractId);
			var legal = legalLogic.GetLegal(contract.LegalId);
			var contractor = contractorLogic.GetContractor(legal.ContractorId.Value);
			return Content(JsonConvert.SerializeObject(contractor));
		}

		public ActionResult GetOrderOperations()
		{
			var list = dataLogic.GetOrderOperations();
			return Content(JsonConvert.SerializeObject(new { Items = list }));
		}

		public ContentResult GetLegalsByContractor(int contractorId)
		{
			var list = legalLogic.GetLegalsByContractor(contractorId);
			return Content(JsonConvert.SerializeObject(new { Items = list }));
		}

		public ContentResult GetEmployeesByLegal(int legalId)
		{
			var list = employeeLogic.GetEmployeesByLegal(legalId);
			return Content(JsonConvert.SerializeObject(new { Items = list }));
		}

		public ContentResult GetEmployeesByPerson(int personId)
		{
			var list = employeeLogic.GetEmployeesByPerson(personId);
			return Content(JsonConvert.SerializeObject(new { Items = list }));
		}

		public ContentResult GetEmployeesByContractor(int contractorId)
		{
			var list = employeeLogic.GetEmployeesByContractor(contractorId);
			return Content(JsonConvert.SerializeObject(new { Items = list }));
		}

		public ContentResult GetOrdersByContractor(int contractorId)
		{
			var list = orderLogic.GetOrdersByContractor(contractorId).ToList();
			return Content(JsonConvert.SerializeObject(new { Items = list }));
		}

		public ContentResult GetWorkgroupByContractor(int contractorId)
		{
			var list = participantLogic.GetWorkgroupByContractor(contractorId);
			return Content(JsonConvert.SerializeObject(new { Items = list }));
		}

		public ContentResult GetWorkgroupByOrder(int orderId)
		{
			var list = participantLogic.GetWorkgroupByOrder(orderId);
			return Content(JsonConvert.SerializeObject(new { Items = list }));
		}

		public ContentResult GetAccountingsByContractor(int contractorId)
		{
			var list = accountingLogic.GetAccountingsByContractor(contractorId);
			return Content(JsonConvert.SerializeObject(new { Items = list }));
		}

		public ContentResult GetAccountingsByLegal(int legalId)
		{
			var list = accountingLogic.GetAccountingsByLegal(legalId).ToList();
			return Content(JsonConvert.SerializeObject(new { Items = list }));
		}

		public ContentResult GetContractsByContractor(int contractorId)
		{
			var list = contractLogic.GetContractsByContractor(contractorId);
			return Content(JsonConvert.SerializeObject(new { Items = list }));
		}

		public ContentResult GetClientContractsByContractor(int contractorId)
		{
			var list = contractLogic.GetContractsByContractor(contractorId).Where(w => w.ContractServiceTypeId != 2).ToList();
			return Content(JsonConvert.SerializeObject(new { Items = list }));
		}

		public ContentResult GetProviderContractsByContractor(int contractorId)
		{
			var list = contractLogic.GetContractsByContractor(contractorId).Where(w => w.ContractServiceTypeId != 1).ToList();
			return Content(JsonConvert.SerializeObject(new { Items = list }));
		}

		public ContentResult GetContractsByLegal(int legalId)
		{
			var list = contractLogic.GetContractsByLegal(legalId).ToList();
			return Content(JsonConvert.SerializeObject(new { Items = list }));
		}

		public ContentResult GetContactsByLegal(int legalId)
		{
			var list = legalLogic.GetRouteContactsByLegal(legalId).ToList();
			return Content(JsonConvert.SerializeObject(new { Items = list }));
		}

		public ContentResult GetBankAccountsByLegal(int legalId)
		{
			var list = bankLogic.GetBankAccountsByLegal(legalId).ToList();
			return Content(JsonConvert.SerializeObject(new { Items = list }));
		}

		/// <summary>
		/// Поиск (для автокомплита) по БИК
		/// </summary>
		public ContentResult SearchBanks(string term)
		{
			var list = bankLogic.SearchBanks(term).OrderBy(o => o.Name).Take(16);
			return Content(JsonConvert.SerializeObject(list.Select(s => new { label = s.Name, value = s.BIC, entity = s })));
		}

		/// <summary>
		/// Поиск (для автокомплита) по фио
		/// </summary>
		[OutputCache(NoStore = true, Duration = 0)]
		public ContentResult SearchPersons(string term)
		{
			var list = personLogic.SearchPersons(term).OrderBy(o => o.DisplayName);
			return Content(JsonConvert.SerializeObject(list.Select(s => new { label = s.DisplayName, value = s.DisplayName, entity = s })));
		}

		/// <summary>
		/// Поиск (для автокомплита) по месту
		/// </summary>
		public ContentResult SearchPlaces(string term)
		{
			var list = dataLogic.SearchPlaces(term).Take(32).ToList();
			var countries = dataLogic.GetCountries(new ListFilter()).OrderBy(o => o.Name);
			return Content(JsonConvert.SerializeObject(list.Select(s => new { label = countries.First(w => w.ID == s.CountryId).Name + " IATA:" + s.IataCode + " ICAO:" + s.IcaoCode + " " + s.Name + "/" + s.EnName, value = s.Name, entity = s })));
		}

		public ContentResult GetActionHint(int actionId)
		{
			var action = participantLogic.GetAction(actionId);
			var roles = string.Join(", ", participantLogic.GetAllowedRoles(actionId).Select(s => s.Name).ToArray());
			return Content("<li>Это действие разрешено участникам с ролями: <span style='color:#ff6a00'>" + roles + "</span></li>" + action.Hint);
		}

		public ContentResult GetPricelistsByContractor(int contractorId)
		{
			var pricelists = pricelistLogic.GetPricelists(new ListFilter { Type = "NonStandart" });
			var contracts = contractLogic.GetContractsByContractor(contractorId).Select(s => s.ID).ToList();

			return Content(JsonConvert.SerializeObject(new { Items = pricelists.Where(w => contracts.Contains(w.ContractId.Value)) }));
		}

		public ContentResult GetOrderTemplateOperations(int orderTemplateId)
		{
			var list = dataLogic.GetOrderTemplateOperations().Where(w => w.OrderTemplateId == orderTemplateId);
			return Content(JsonConvert.SerializeObject(new { Items = list }));
		}
	}
}