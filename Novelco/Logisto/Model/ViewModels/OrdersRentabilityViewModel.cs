using System.Collections.Generic;
using Logisto.Models;

namespace Logisto.ViewModels
{
	public class OrdersRentabilityViewModel : IndexViewModel
	{
		public List<OrderRentability> Items { get; set; }
	}
}