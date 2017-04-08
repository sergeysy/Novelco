using System;

namespace Logisto.Models
{
	public class PlaceListFilter
	{
		public string Context { get; set; }

		public string Sort { get; set; }
		public string SortDirection { get; set; }

		public int PageSize { get; set; }
		public int PageNumber { get; set; }

		public int CountryId { get; set; }
		public int RegionId { get; set; }
		public int SubRegionId { get; set; }
	}
}