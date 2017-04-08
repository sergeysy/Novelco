using System.Collections.Generic;
using Logisto.Models;

namespace Logisto.ViewModels
{
	public class ContractsViewModel : IndexViewModel
	{
		public IEnumerable<Contract> Items { get; set; }
	}
}