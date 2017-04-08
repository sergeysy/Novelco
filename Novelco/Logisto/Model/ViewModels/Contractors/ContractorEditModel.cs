using System.Collections.Generic;
using Logisto.Models;

namespace Logisto.ViewModels
{
	public class ContractorEditModel
	{
		public int ID { get; set; }
		public string Name { get; set; }
		public int Manager { get; set; }
		public bool IsLocked { get; set; }
	}
}