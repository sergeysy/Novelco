using System.Collections.Generic;
using Logisto.Models;

namespace Logisto.ViewModels
{
	public class CountriesViewModel : IndexViewModel
	{
		public List<Country> Items { get; set; }
	}
}