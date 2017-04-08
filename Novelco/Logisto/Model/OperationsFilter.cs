using System;
using System.Collections.Generic;

namespace Logisto.Models
{
	public class OperationsFilter
	{
		public string OrderNumber { get; set; }
		public string Context { get; set; }

		public DateTime? StartPlanFrom { get; set; }
		public DateTime? StartPlanTo { get; set; }

		public DateTime? StartFactFrom { get; set; }
		public DateTime? StartFactTo { get; set; }

		public DateTime? FinishPlanFrom { get; set; }
		public DateTime? FinishPlanTo { get; set; }

		public DateTime? FinishFactFrom { get; set; }
		public DateTime? FinishFactTo { get; set; }

		public string Sort { get; set; }
		public string SortDirection { get; set; }

		public int PageSize { get; set; }
		public int PageNumber { get; set; }

		public List<int> Statuses { get; set; }
		public List<int> Responsibles { get; set; }

		public OperationsFilter()
		{
			Statuses = new List<int>();
			Responsibles = new List<int>();
		}
	}
}