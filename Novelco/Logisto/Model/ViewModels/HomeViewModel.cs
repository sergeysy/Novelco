using System;

namespace Logisto.ViewModels
{
	public class HomeViewModel
	{
		public double UsdRate { get; set; }
		public double EurRate { get; set; }
		public double CnyRate { get; set; }
		public double GbpRate { get; set; }

		public DateTime LastRateUpdated { get; set; }
	}
}