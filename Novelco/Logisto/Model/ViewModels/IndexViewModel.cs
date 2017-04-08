using System.Collections.Generic;
using Logisto.Models;

namespace Logisto.ViewModels
{
	public class IndexViewModel
	{
		public int TotalItemsCount { get; set; }
		public ListFilter Filter { get; set; }
		public Dictionary<string, object> Dictionaries { get; set; }

		public IndexViewModel()
		{
			Dictionaries = new Dictionary<string, object>();
		}
	}
}