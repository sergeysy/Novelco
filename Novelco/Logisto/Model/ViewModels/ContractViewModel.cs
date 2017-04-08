using System.Collections.Generic;
using Logisto.Models;

namespace Logisto.ViewModels
{
	public class ContractViewModel : Contract
	{
		public string Currency { get; set; }
		public string OurLegal { get; set; }
		public string Legal { get; set; }
		public string Marks { get; set; }
		public string Type { get; set; }

		public bool IsActive { get; set; }

		public IEnumerable<ContractCurrency> Currencies { get; set; }
		public Dictionary<string, object> Dictionaries { get; set; }

		public ContractViewModel()
		{
			Currencies = new List<ContractCurrency>();
			Dictionaries = new Dictionary<string, object>();
		}
	}
}




















