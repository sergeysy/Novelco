using System.Collections.Generic;
using Logisto.Models;

namespace Logisto.ViewModels
{
	public class OperationsViewModel
	{
		public OperationsFilter Filter { get; set; }

		public List<Operation> Items { get; set; }

		public int TotalItemsCount { get; set; }

		public Dictionary<string, object> Dictionaries { get; set; }

		public OperationsViewModel()
		{
			Dictionaries = new Dictionary<string, object>();
		}
	}
}