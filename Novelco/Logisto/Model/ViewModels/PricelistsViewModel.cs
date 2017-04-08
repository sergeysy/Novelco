using System.Collections.Generic;
using Logisto.Models;

namespace Logisto.ViewModels
{
	public class PricelistsViewModel : IndexViewModel
	{
		public IEnumerable<Pricelist> Items { get; set; }
	}
}