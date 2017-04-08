using System.Collections.Generic;
using Logisto.Models;

namespace Logisto.BusinessLogic
{
	public interface IBankLogic
	{
		#region bank account

		/// <summary>
		/// Создать банковский счет
		/// </summary>
		int CreateBankAccount(BankAccount account);

		/// <summary>
		/// Получить банковский счет
		/// </summary>
		BankAccount GetBankAccount(int id);

		/// <summary>
		/// Обновить банковский счет
		/// </summary>
		void UpdateBankAccount(BankAccount account);

		/// <summary>
		/// Удалить банковский счет
		/// </summary>
		void DeleteBankAccount(int id);

		/// <summary>
		/// Получить банковские счета для юридического лица
		/// </summary>
		IEnumerable<BankAccount> GetBankAccountsByLegal(int legalId);

		#endregion

		#region bank

		int CreateBank(Bank bank);
		Bank GetBank(int id);
		void UpdateBank(Bank bank);

		/// <summary>
		/// Получить список банков с учетом фильтра
		/// </summary>
		IEnumerable<Bank> GetBanks(ListFilter filter);

		/// <summary>
		/// Получить значение количества банков с учетом фильтра
		/// </summary>
		int GetBanksCount(ListFilter filter);

		/// <summary>
		/// Поиск банка по БИК
		/// </summary>
		IEnumerable<Bank> SearchBanks(string bic);

		#endregion
	}
}