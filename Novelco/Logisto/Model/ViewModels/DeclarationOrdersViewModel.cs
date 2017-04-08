using System;
using System.Collections.Generic;

namespace Logisto.ViewModels
{
	public class DeclarationOrdersViewModel : IndexViewModel
	{
		public IEnumerable<DeclarationViewModel> Items { get; set; }
	}

	public class DeclarationViewModel
	{
		public int DocumentId { get; set; }
		public int OrderId { get; set; }
		public int? ContractId { get; set; }
		public string OrderNumber { get; set; }
		public string OrderStatus { get; set; }
		public string LegalName { get; set; }
		public string ContractNumber { get; set; }
		public string DeclarationNumber { get; set; }
		public string ContractType { get; set; }
		public DateTime? MotivationDate { get; set; }
		public bool IsWeekend { get; set; }
		public string WeekendMarkUser { get; set; }
		public int? WeekendMarkUserId { get; set; }
		public DateTime? WeekendMarkDate { get; set; }
	}
}