using System.Collections.Generic;
using Logisto.Models;

namespace Logisto.ViewModels
{
	public class OrdersViewModel : IndexViewModel
	{
		public double TotalItemsSum { get; set; }
		public IEnumerable<Order> Items { get; set; }
	}
}