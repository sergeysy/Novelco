using System;

namespace Logisto.ViewModels
{
	public class JointDocumentModel
	{
		public int ID { get; set; }
		// document / templated document
		public bool IsDocument { get; set; }
		public int? AccountingId { get; set; }
		public int? DocumentTypeId { get; set; }
		public int? TemplateId { get; set; }
		public string Number { get; set; }
		public string OrderAccountingName { get; set; }
		public bool? IsPrint { get; set; }
		public bool IsNipVisible { get; set; }
		public DateTime? Date { get; set; }
		public DateTime? UploadedDate { get; set; }
		public int? UploadedBy { get; set; }
		public DateTime? OriginalSentDate { get; set; }
		public int? OriginalSentBy { get; set; }
		public DateTime? OriginalReceivedDate { get; set; }
		public int? OriginalReceivedBy { get; set; }
		public string ReceivedBy { get; set; }
		public string ReceivedNumber { get; set; }
		public int LegalId { get; set; }
		public string Filename { get; set; }
		public string FileSize { get; set; }

		// HACK:
		public int? OrderId { get; set; }
		public string OrderNumber { get; set; }
	}
}