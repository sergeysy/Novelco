using System.Collections.Generic;
using Logisto.Models;

namespace Logisto.BusinessLogic
{
	public interface ILegalLogic
	{
		int CreateLegal(Legal legal);
		Legal GetLegal(int id);
		void UpdateLegal(Legal legal);
		void DeleteLegal(int legalId);

		Legal GetLegalByOurLegal(int id);
		Dictionary<string, object> GetLegalInfo(int id);


		/// <summary>
		/// Получить список юрлиц с учетом фильтра
		/// </summary>
		IEnumerable<Legal> GetLegals(ListFilter filter);

		/// <summary>
		/// Получить телефоны юрлица
		/// </summary>
		IEnumerable<Phone> GetPhonesByLegal(int legalId);

		#region Route contact

		int CreateRouteContact(RouteContact routeContact);

		RouteContact GetRouteContact(int routeContactId);

		void UpdateRouteContact(RouteContact routeContact);

		/// <summary>
		/// Получить контакты юрлица
		/// </summary>
		IEnumerable<RouteContact> GetRouteContactsByLegal(int legalId);

		#endregion

		int CreateOurLegal(OurLegal legal);
		OurLegal GetOurLegal(int id);
		void UpdateOurLegal(OurLegal legal);
		IEnumerable<OurLegal> GetOurLegals();

		/// <summary>
		/// Получить количество юрлиц контрагента
		/// </summary>
		int GetLegalsCountByContractor(int contractorId);

		/// <summary>
		/// Получить юрлица контрагента
		/// </summary>
		IEnumerable<Legal> GetLegalsByContractor(int contractorId);

		Legal GetLegalByAccounting(int accountingId);
	}
}