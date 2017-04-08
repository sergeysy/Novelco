using System.Collections.Generic;
using Logisto.Models;

namespace Logisto.ViewModels
{
	public class OrderViewModel
	{
		public Order Order { get; set; }
		public OurLegal OurLegal { get; set; }
		public FinRepCenter FinRepCenter { get; set; }

		public int PricelistId { get; set; }
		public int ContractorId { get; set; }

		public double Rentability { get; set; }
		public double MinRentability { get; set; }

		public Dictionary<string, object> Dictionaries { get; set; }

		public OrderViewModel()
		{
			Dictionaries = new Dictionary<string, object>();
		}
	}

	public class MatchingViewModel
	{
		public int ExpenseAccountingId { get; set; }
		public double Sum { get; set; }
		public double Percent { get; set; }

		public List<IdSum> Incomes { get; set; }

		public MatchingViewModel()
		{
			Incomes = new List<IdSum>();
		}
	}

	public class IdSum
	{
		public int ID { get; set; }
		public int AccountingId { get; set; }
		public double Sum { get; set; }
	}
}