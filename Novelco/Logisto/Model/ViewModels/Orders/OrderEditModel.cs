using System;

namespace Logisto.ViewModels
{
	public class OrderEditModel
	{
		public int ID { get; set; }
		public int? CreatedBy { get; set; }
		public int? OrderTypeId { get; set; }
		public int? OrderStatusId { get; set; }
		public int? ContractId { get; set; }
		public int ProductId { get; set; }
		public int? SenderLegalId { get; set; }
		public int? ReceiverLegalId { get; set; }
		public int? InsuranceTypeId { get; set; }
		public int? UninsuranceTypeId { get; set; }
		public int? InvoiceCurrencyId { get; set; }
		public int? SeatsCount { get; set; }
		public int? zkzPoluchContactID { get; set; }
		public int? zkzOtpravitelContactID { get; set; }
		public int? zkzGruzPriceCurrID { get; set; }
		public int? FinRepCenterId { get; set; }
		public int? VolumetricRatioId { get; set; }

		public string Number { get; set; }
		public string CargoInfo { get; set; }
		public string EnCargoInfo { get; set; }
		public string SpecialCustody { get; set; }
		public string Danger { get; set; }
		public string TemperatureRegime { get; set; }
		public double? NetWeight { get; set; }
		public double? GrossWeight { get; set; }
		public double? PaidWeight { get; set; }
		public double? Volume { get; set; }
		public double? CargoPrice { get; set; }
		public string Cost { get; set; }
		public string InsurancePolicy { get; set; }
		public string From { get; set; }
		public string To { get; set; }
		public string VehicleNumbers { get; set; }
		public string RequestNumber { get; set; }
		public string InvoiceNumber { get; set; }
		public double? InvoiceSum { get; set; }
		public double? Expense { get; set; }
		public double? Income { get; set; }
		public double? Balance { get; set; }
		public DateTime? LoadingDate { get; set; }
		public DateTime? UnloadingDate { get; set; }
		public DateTime? InvoiceDate { get; set; }
		public DateTime? CreatedDate { get; set; }
		public bool? IsPrintDetails { get; set; }
		public string RouteBeforeBoard { get; set; }
		public string RouteAfterBoard { get; set; }
		public double? zkzForDetailsVATSumBefore { get; set; }
		public double? zkzForDetailsVATSumAfter { get; set; }
		public int? zkzForDetailsVAT { get; set; }
		public string zkzInsurUpak { get; set; }
		public double? zkzEconomicaBalance { get; set; }
		public string InvoiceCurrencyDisplay { get; set; }
		public int? zkzInvoiceCurrNew { get; set; }
		public int? ExtraDoc1Id { get; set; }
		public int? ExtraDoc2Id { get; set; }
		public int? ExtraDoc3Id { get; set; }
		public int? zkzZayavkaID { get; set; }
		public string zkzGOnameINN { get; set; }
		public string zkzGOcontact { get; set; }
		public string zkzGOcontTel { get; set; }
		public string zkzGOtime { get; set; }
		public string zkzGOcomment { get; set; }
		public string zkzGPnameINN { get; set; }
		public string zkzGPcontact { get; set; }
		public string zkzGPconTel { get; set; }
		public string zkzGPtime { get; set; }
		public string zkzGPcomment { get; set; }
		public string zkzGOname { get; set; }
		public string zkzGPname { get; set; }
		public string zkzMarshrut { get; set; }
		public string zkzExpTO { get; set; }
		public string zkzImpTO { get; set; }
		public DateTime RequestDate { get; set; }
		public string Comment { get; set; }
		public double? RouteLengthBeforeBoard { get; set; }
		public double? RouteLengthAfterBoard { get; set; }
		public string EnSpecialCustody { get; set; }
		public string EnDanger { get; set; }
		public string EnTemperatureRegime { get; set; }
		public double? zkzKursValue { get; set; }
		public DateTime? ClosedDate { get; set; }
	}
}