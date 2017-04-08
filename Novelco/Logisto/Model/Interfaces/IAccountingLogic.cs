using System.Collections.Generic;
using Logisto.Models;

namespace Logisto.BusinessLogic
{
	public interface IAccountingLogic
	{
		#region Accounting

		IEnumerable<Accounting> GetAllAccountings();

		/// <summary>
		/// Получить список записей учета заказа с учетом фильтра
		/// </summary>
		IEnumerable<Accounting> GetAccountings(ListFilter filter);

		int CreateAccounting(Accounting accounting);
		Accounting GetAccounting(int id);
		void UpdateAccounting(Accounting accounting);
		void DeleteAccounting(int accountingId);
		void DeleteAccounting(int accountingId, bool isCascade);

		/// <summary>
		/// Получить записи учета по заказу
		/// </summary>
		IEnumerable<Accounting> GetAccountingsByOrder(int orderId);

		/// <summary>
		/// Получить количество записей учета по контрагенту
		/// </summary>
		int GetAccountingsCountByContractor(int contractorId);

		/// <summary>
		/// Получить записи учета по контрагенту
		/// </summary>
		IEnumerable<Accounting> GetAccountingsByContractor(int contractorId);

		/// <summary>
		/// Получить записи учета по юрлицу
		/// </summary>
		IEnumerable<Accounting> GetAccountingsByLegal(int legalId);

		void AppendAccountingRejectHistory(int accountingId, string message);

		IEnumerable<Accounting> GetAccountingsByContract(int contractId);
		Accounting GetAccountingByNumber(string number);
		Accounting GetAeAccounting(string number);
		Accounting GetApAccounting(string number);
		IEnumerable<Accounting> GetAccountingsByInvoiceNumber(string number);

		#endregion

		#region services

		int CreateService(Service service);
		Service GetService(int serviceId);
		void UpdateService(Service service);
		void DeleteService(int serviceId);

		/// <summary>
		/// Получить список услуг для записи доход/расхода
		/// </summary>
		IEnumerable<Service> GetServicesByAccounting(int accountingId);

		/// <summary>
		/// Получить валюту по первой услуге
		/// </summary>
		int? GetAccountingCurrencyId(int accountingId);

		#endregion

		/// <summary>
		/// Получить список платежей (пп) для записи учета
		/// </summary>
		IEnumerable<Payment> GetPayments(int orderAccontingId);
		IEnumerable<Payment> GetPayments(ListFilter filter);
		IEnumerable<Payment> GetAllPayments();
		IEnumerable<Payment> GetPaymentsByContractor(int contractorId);
		int CreatePayment(Payment payment);
		Payment GetPayment(int paymentId);
		void UpdatePayment(Payment payment);
		void DeletePayment(int paymentId);

		Payment GetPaymentByFinReference(string reference);
		Dictionary<string, object> GetPaymentInfo(int id);

		/// <summary>
		/// Пересчитать сумму доходов/расходов для контрагента и его юрлиц
		/// </summary>
		void CalculateContractorBalance(int contractorId);
		void CalculateAccountingBalance(int accountingId);
		string CalculateServiceBalance(int serviceId);
		string CalculatePaymentPlanDate(int accountingId);

		int CreateAccountingRouteSegment(OrderAccountingRouteSegment segment);
		OrderAccountingRouteSegment GetAccountingRouteSegment(int segmentId);
		void UpdateAccountingRouteSegment(OrderAccountingRouteSegment segment);
		void DeleteAccountingRouteSegment(int segmentId);

		IEnumerable<OrderAccountingRouteSegment> GetAccountingRouteSegments(int accountingId);

		#region marks

		int CreateAccountingMark(AccountingMark mark);
		AccountingMark GetAccountingMarkByAccounting(int accountingId);
		void UpdateAccountingMark(AccountingMark mark);

		IEnumerable<AccountingMark> GetAccountingMarksByOrder(int orderId);
		IEnumerable<AccountingMark> GetAllAccountingMarks();

		#endregion

		#region Matchings

		int CreateAccountingMatching(AccountingMatching entity);
		IEnumerable<AccountingMatching> GetAccountingMatchingsByAccounting(int accountingId);
		void UpdateAccountingMatching(AccountingMatching entity);

		#endregion
	}
}