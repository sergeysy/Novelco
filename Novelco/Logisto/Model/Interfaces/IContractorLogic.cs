using System.Collections.Generic;
using Logisto.Models;

namespace Logisto.BusinessLogic
{
	public interface IContractorLogic
	{
		#region Contractor

		int CreateContractor(Contractor contractor);

		/// <summary>
		/// Получить контрагента
		/// </summary>
		Contractor GetContractor(int id);
		Contractor GetContractorByAccounting(int accountingId);
		Contractor GetContractorByContract(int contractId);
		Contractor GetContractorByLegal(int legalId);

		/// <summary>
		/// Обновить данные контрагента
		/// </summary>
		void UpdateContractor(Contractor contractor);

		/// <summary>
		/// Получить список контрагентов с учетом фильтра
		/// </summary>
		IEnumerable<Contractor> GetContractors(ListFilter filter);

		/// <summary>
		/// Получить значение количества контрагентов с учетом фильтра
		/// </summary>
		int GetContractorsCount(ListFilter filter);

		#endregion

		ContractorEmployeeSettings GetContractorEmployeeSettings(int employeeId);

		void UpdateContractorEmployeeSettings(ContractorEmployeeSettings entity);
	}
}