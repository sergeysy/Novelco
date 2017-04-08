using System;
using System.Collections.Generic;

namespace Logisto.Models
{
	public class ListFilter
	{
		public DateTime? From { get; set; }
		public DateTime? To { get; set; }
		public DateTime? From2 { get; set; }
		public DateTime? To2 { get; set; }
		public string Context { get; set; }
		public string Type { get; set; }
		public string Sort { get; set; }
		public string SortDirection { get; set; }

		public int PageSize { get; set; }
		public int PageNumber { get; set; }
		public int UserId { get; set; }
		public int ParentId { get; set; }

		public List<int> Statuses { get; set; }

		public ListFilter()
		{
			Statuses = new List<int>();
		}
	}
}