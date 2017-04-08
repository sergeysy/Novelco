using System;
using System.Collections.Generic;
using Logisto.Models;

namespace Logisto.ViewModels
{
	public class ContractInfoModel
	{
		public int ID { get; set; }
		public int LegalId { get; set; }
		public int OurLegalId { get; set; }
		public int CurrencyId { get; set; }
		public int? PayMethodId { get; set; }
		public int CurrencyRateUseId { get; set; }
		public int ContractTypeId { get; set; }
		public int ContractServiceTypeId { get; set; }
		public string Number { get; set; }
		public string Currency { get; set; }
		public string Legal { get; set; }
		public string OurLegal { get; set; }
		public string Type { get; set; }
		public DateTime? Date { get; set; }
		public bool IsActive { get; set; }

		public IEnumerable<ContractCurrency> Currencies { get; set; }

		public ContractInfoModel()
		{
			Currencies = new List<ContractCurrency>();
		}
	}
}




















