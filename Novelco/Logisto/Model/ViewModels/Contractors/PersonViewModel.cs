using System.Collections.Generic;
using Logisto.Models;

namespace Logisto.ViewModels
{
	public class PersonViewModel : Person
	{
		public Dictionary<string, object> Dictionaries { get; set; }

		public PersonViewModel()
		{
			Dictionaries = new Dictionary<string, object>();
		}
	}
}