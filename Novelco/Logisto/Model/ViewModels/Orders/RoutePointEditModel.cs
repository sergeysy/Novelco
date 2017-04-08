using System;
using System.Collections.Generic;
using Logisto.Models;

namespace Logisto.ViewModels
{
	public class RoutePointEditModel
	{
		public int ID { get; set; }
		public int OrderId { get; set; }
		public int? No { get; set; }
		public int? RoutePointTypeId { get; set; }
		public int? PlaceId { get; set; }
		public DateTime PlanDate { get; set; }
		public DateTime? FactDate { get; set; }
		public string Address { get; set; }
		public string EnAddress { get; set; }
		public string Contact { get; set; }
		public string EnContact { get; set; }
		public int? ParticipantLegalId { get; set; }
		public string ParticipantComment { get; set; }
		public int? RouteContactID { get; set; }
		public string EnParticipantComment { get; set; }

		public bool IsDeleted { get; set; }
	}
}