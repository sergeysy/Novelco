using System;
using System.Collections.Generic;

namespace Logisto.ViewModels
{
	public class AccountingEditModel
	{
		public int ID { get; set; }
		public int OrderId { get; set; }
		//public int? No { get; set; }
		public int? AccountingDocumentTypeId { get; set; }
		public int? AccountingPaymentTypeId { get; set; }
		public int? AccountingPaymentMethodId { get; set; }
		public int? PayMethodId { get; set; }
		public int? SecondSignerEmployeeId { get; set; }
		public int? ContractId { get; set; }
		//public int? ecnNumberR { get; set; }
		public int? CargoLegalId { get; set; }
		public int? ServiceIdForDetails { get; set; }
		public int? OurLegalId { get; set; }
		public int? LegalId { get; set; }

		public string Number { get; set; }
		public string InvoiceNumber { get; set; }
		public string VatInvoiceNumber { get; set; }
		public string ActNumber { get; set; }
		public string Route { get; set; }
		public string Comment { get; set; }
		public string SecondSignerName { get; set; }
		public string SecondSignerPosition { get; set; }
		public string SecondSignerInitials { get; set; }
		public string ecnImportTO { get; set; }
		public string ecnExportTO { get; set; }
		public string ecnTransport { get; set; }
		public string ecnTS { get; set; }
		public string ecnGOnameINN { get; set; }
		public string ecnGOcontact { get; set; }
		public string ecnGOcontTel { get; set; }
		public string ecnGOtime { get; set; }
		public string ecnGOcomment { get; set; }
		public string ecnGPnameINN { get; set; }
		public string ecnGPcontact { get; set; }
		public string ecnGPconTel { get; set; }
		public string ecnGPtime { get; set; }
		public string ecnGPcomment { get; set; }
		public string ecnMarshrutEN { get; set; }
		public string ecnExportTOEN { get; set; }
		public string ecnImportTOEN { get; set; }
		public string ecnKommentEN { get; set; }
		public string ecnGOnameINNEN { get; set; }
		public string ecnGOcontactEN { get; set; }
		public string ecnGOcommentEN { get; set; }
		public string ecnGPnameINNEN { get; set; }
		public string ecnGPcontactEN { get; set; }
		public string ecnGPcommentEN { get; set; }
		public string strGOcontTelEN { get; set; }
		public string strGPcontTelEN { get; set; }
		public string ecnGOtimeEN { get; set; }
		public string ecnGPtimeEN { get; set; }
		public double? OriginalSum { get; set; }
		public double? OriginalVat { get; set; }
		public double? CurrencyRate { get; set; }
		public double? Sum { get; set; }
		public double? Vat { get; set; }
		public double? Payment { get; set; }
		public double? RouteLengthBeforeBorderForDetails { get; set; }
		public double? RouteLengthAfterBorderForDetails { get; set; }
		public DateTime CreatedDate { get; set; }
		public DateTime? InvoiceDate { get; set; }
		public DateTime? ActDate { get; set; }
		public DateTime? AccountingDate { get; set; }
		public DateTime? RequestDate { get; set; }
		public DateTime? ecnLoadingDate { get; set; }
		public DateTime? ecnUnloadingDate { get; set; }
		public DateTime? PaymentPlanDate { get; set; }

		public bool IsIncome { get; set; }
		
		public bool IsDeleted { get; set; }

		public List<DocumentEditModel> Documents { get; set; }
		public List<ServiceEditModel> Services { get; set; }
		public List<OrderAccountingRouteSegmentEditModel> RouteSegments { get; set; }

		public AccountingEditModel()
		{
			Documents = new List<DocumentEditModel>();
			Services = new List<ServiceEditModel>();
			RouteSegments = new List<OrderAccountingRouteSegmentEditModel>();
		}
	}
}