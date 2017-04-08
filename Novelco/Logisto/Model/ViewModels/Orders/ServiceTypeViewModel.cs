using System;
using System.Collections.Generic;
using Logisto.Models;

namespace Logisto.ViewModels
{
	public class ServiceTypeViewModel
	{
		public int ID { get; set; }
		public int? ServiceKindId { get; set; }
		public int? MeasureId { get; set; }
		public string Name { get; set; }
		public int? Count { get; set; }
		public double? Price { get; set; }

		public int? VatId { get; set; }
		public string Kind { get; set; }

		public int? ProductId { get; set; }
	}
}