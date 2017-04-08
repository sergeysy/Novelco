using System;

namespace Logisto.ViewModels
{
	public class CargoSeatEditModel
	{
		public int ID { get; set; }
		public int? OrderId { get; set; }
		public int? CargoDescriptionId { get; set; }
		public int? PackageTypeId { get; set; }
		public int? SeatCount { get; set; }
		public double? Length { get; set; }
		public double? Width { get; set; }
		public double? Height { get; set; }
		public double? Volume { get; set; }
		public double? GrossWeight { get; set; }

		public string InvoiceNumber { get; set; }
		public DateTime? InvoiceDate { get; set; }

		public bool IsStacking { get; set; }

		public bool IsDeleted { get; set; }
	}
}