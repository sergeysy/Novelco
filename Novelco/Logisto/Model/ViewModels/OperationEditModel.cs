using System;

namespace Logisto.ViewModels
{
	public class OperationEditModel
	{
		public int ID { get; set; }
		public int OrderId { get; set; }
		public int OrderOperationId { get; set; }
		public int OperationStatusId { get; set; }
		public int? ResponsibleUserId { get; set; }
		public int No { get; set; }
		public string Name { get; set; }
		public string City { get; set; }
		public string Comment { get; set; }
		public DateTime? StartPlanDate { get; set; }
		public DateTime? FinishPlanDate { get; set; }
		public DateTime? StartFactDate { get; set; }
		public DateTime? FinishFactDate { get; set; }

		public bool IsDeleted { get; set; }
	}
}