using System;
using System.Collections.Generic;
using Logisto.Models;

namespace Logisto.ViewModels
{
	public class EmployeeEditModel
	{
		public int ID { get; set; }
		public int? LegalId { get; set; }
		public int? PersonId { get; set; }
		public int? ContractorId { get; set; }
		public int? FinRepCenterId { get; set; }
		//public int? SigningAuthorityId { get; set; }
		public bool IsSigning { get; set; }
		public DateTime? BeginDate { get; set; }
		public DateTime? EndDate { get; set; }
		public string Department { get; set; }
		public string Position { get; set; }
		public string GenitivePosition { get; set; }
		public string Comment { get; set; }
		public string Basis { get; set; }
		public string EnPosition { get; set; }
		public string EnBasis { get; set; }
		
		public bool IsDeleted { get; set; }
	}
}