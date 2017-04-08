using System;
using Logisto.Models;

namespace Logisto.ViewModels
{
	public class AccountingViewModel : Accounting
	{
		public int ContractorId { get; set; }
		public string ContractorName { get; set; }
		public string OrderNumber { get; set; }
		public string ContractNumber { get; set; }
		[Obsolete]
		public int? ContractCurrencyId { get; set; }
		public int? AccountingCurrencyId { get; set; }

		public int? InvoiceSum { get; set; }
	}
}