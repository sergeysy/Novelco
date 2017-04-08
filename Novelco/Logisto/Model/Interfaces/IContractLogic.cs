using System.Collections.Generic;
using Logisto.Models;

namespace Logisto.BusinessLogic
{
	public interface IContractLogic
	{
		#region Contract

		/// <summary>
		/// Создать договор
		/// </summary>
		int CreateContract(Contract contract);

		/// <summary>
		/// Получить договор
		/// </summary>
		Contract GetContract(int id);
		
		/// <summary>
		/// Обновить данные договора
		/// </summary>
		void UpdateContract(Contract contract);

		/// <summary>
		/// Удалить договор
		/// </summary>
		void DeleteContract(int id);

		/// <summary>
		/// Получить список договоров с учетом фильтра
		/// </summary>
		IEnumerable<Contract> GetContracts(ListFilter filter);

		int GetContractsCount(ListFilter filter);

		/// <summary>
		/// Получить список договоров для юрлица
		/// </summary>
		IEnumerable<Contract> GetContractsByLegal(int legalId);

		/// <summary>
		/// Получить список договоров для контрагента
		/// </summary>
		IEnumerable<Contract> GetContractsByContractor(int contractorId);

		/// <summary>
		/// Получить строковые пары полей договора
		/// </summary>
		Dictionary<string, object> GetContractInfo(int id);
		
		#endregion

		ContractCurrency GetContractCurrency(int contractId, int currencyId);
		IEnumerable<Currency> GetCurrenciesByContract(int contractId);
		IEnumerable<ContractCurrency> GetContractCurrencies(int contractId);
		void CreateContractCurrency(ContractCurrency entity);
		void UpdateContractCurrency(ContractCurrency entity);
		void DeleteContractCurrency(int contractId, int currencyId);

		#region marks

		int CreateContractMark(ContractMark mark);
		ContractMark GetContractMark(int id);
		ContractMark GetContractMarkByContract(int contractId);
		void UpdateContractMark(ContractMark mark);

		#endregion

		#region order status history

		int CreateContractMarksHistory(ContractMarksHistory entity);
		IEnumerable<ContractMarksHistory> GetContractMarksHistory(int contractId);

		#endregion
	}
}