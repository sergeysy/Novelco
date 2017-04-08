using System;
using System.Collections.Generic;
using Logisto.Models;

namespace Logisto.ViewModels
{
	public class RouteSegmentEditModel
	{
		public int ID { get; set; }
		public int OrderId { get; set; }
		public int No { get; set; }
		public int? FromRoutePointId { get; set; }
		public int? ToRoutePointId { get; set; }
		public int? TransportTypeId { get; set; }
		public bool IsAfterBorder { get; set; }
		public double? Length { get; set; }
		public string Vehicle { get; set; }
		public string VehicleNumber { get; set; }
		public string DriverName { get; set; }
		public string DriverPhone { get; set; }

		public bool IsDeleted { get; set; }
	}
}