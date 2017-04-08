using System.Collections.Generic;
using Logisto.Models;

namespace Logisto.ViewModels
{
	public class CrmViewModel : IndexViewModel
	{
		public IEnumerable<CrmCall> Items { get; set; }
		public string TotalDuration { get; set; }
	}
}