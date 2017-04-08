using System;
using System.Collections.Generic;
using Logisto.Models;

namespace Logisto.ViewModels
{
	public partial class LegalViewModel
	{
		public int ID { get; set; }
		public int? ContractorId { get; set; }
		public int? TaxTypeId { get; set; }
		public int? DirectorId { get; set; }
		public int? AccountantId { get; set; }

		public string Name { get; set; }
		public string DisplayName { get; set; }
		public string EnName { get; set; }
		public string EnShortName { get; set; }
		public string TIN { get; set; }
		public string OGRN { get; set; }
		public string KPP { get; set; }
		public string OKPO { get; set; }
		public string OKVED { get; set; }
		public string Address { get; set; }
		public string EnAddress { get; set; }
		public string AddressFact { get; set; }
		public string EnAddressFact { get; set; }
		public string PostAddress { get; set; }
		public string EnPostAddress { get; set; }
		public string WorkTime { get; set; }
		public string TimeZone { get; set; }

		public int? CreatedBy { get; set; }
		public DateTime? CreatedDate { get; set; }
		public int? UpdatedBy { get; set; }
		public DateTime? UpdatedDate { get; set; }

		public bool IsNotResident { get; set; }

		public double? Income { get; set; }
		public double? Expense { get; set; }
		public double? Balance { get; set; }
		public double? PaymentIncome { get; set; }
		public double? PaymentExpense { get; set; }
		public double? PaymentBalance { get; set; }

		public int ContractCount { get; set; }
	}
}