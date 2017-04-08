using System.Collections.Generic;
using Logisto.Models;

namespace Logisto.ViewModels
{
	public class ContractorViewModel
	{
		public Contractor Contractor { get; set; }

		public string SMDisplay { get; set; }
		public Participant SM { get; set; }

		public string SLDisplay { get; set; }
		public Participant SL { get; set; }

		public string ResponsibleAMDisplay { get; set; }
		public Participant ResponsibleAM { get; set; }

		public string DeputyAMDisplay { get; set; }
		public Participant DeputyAM { get; set; }

		public Dictionary<string, object> Dictionaries { get; set; }

		public ContractorViewModel()
		{
			Dictionaries = new Dictionary<string, object>();
		}
	}
}