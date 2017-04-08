using System.Collections.Generic;
using Logisto.Models;

namespace Logisto.ViewModels
{
	public class CreateOrderViewModel
	{
		public int ContractorId { get; set; }

		public Dictionary<string, object> Dictionaries { get; set; }

		public CreateOrderViewModel()
		{
			Dictionaries = new Dictionary<string, object>();
		}
	}
}