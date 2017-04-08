using System;
using System.Collections.Generic;
using Logisto.Models;

namespace Logisto.ViewModels
{
	public class ContractEditModel
	{
		public int ID { get; set; }
		public int LegalId { get; set; }
		public int OurLegalId { get; set; }
		public int? CurrencyId { get; set; }
		public int? CurrencyRateUseId { get; set; }
		public int? BankAccountId { get; set; }
		public int? ContractRoleId { get; set; }
		public int ContractTypeId { get; set; }
		public int? PayMethodId { get; set; }
		public int? PaymentTermsId { get; set; }
		public int? OurBankAccountId { get; set; }
		public int? OurContractRoleId { get; set; }
		public int ContractServiceTypeId { get; set; }
		public double AgentPercentage { get; set; }

		public string Number { get; set; }
		public string Comment { get; set; }
		public bool? IsFixed { get; set; }
		public bool IsProlongation { get; set; }
		public DateTime? Date { get; set; }
		public DateTime? BeginDate { get; set; }
		public DateTime? EndDate { get; set; }

		public IEnumerable<ContractCurrency> Currencies { get; set; }

		public bool IsDeleted { get; set; }
		
		public ContractEditModel()
		{
			Currencies = new List<ContractCurrency>();
		}
	}
}




















