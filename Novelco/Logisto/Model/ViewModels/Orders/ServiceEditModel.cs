namespace Logisto.ViewModels
{
	public class ServiceEditModel
	{
		public int ID { get; set; }
		public int? AccountingId { get; set; }
		public int? ServiceTypeId { get; set; }
		public double? Count { get; set; }
		public double? Price { get; set; }
		public double? Sum { get; set; }
		public double? OriginalSum { get; set; }
		public int? CurrencyId { get; set; }
		public int? VatId { get; set; }
		public bool IsForDetalization { get; set; }

		public bool IsDeleted { get; set; }
	}
}