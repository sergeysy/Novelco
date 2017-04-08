using System.Collections.Generic;
using Logisto.Models;

namespace Logisto.ViewModels
{
	public class PricelistViewModel : Pricelist
	{
		public int ContractorId { get; set; }
		public bool IsDataExists { get; set; }
		public Dictionary<string, object> Dictionaries { get; set; }
		public List<Price> Prices { get; set; }
		public List<PriceKind> PriceKinds { get; set; }

		public PricelistViewModel()
		{
			Dictionaries = new Dictionary<string, object>();
			Prices = new List<Price>();
			PriceKinds = new List<PriceKind>();
		}
	}
}




















