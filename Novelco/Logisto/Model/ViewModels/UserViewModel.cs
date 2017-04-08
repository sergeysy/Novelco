using System.Collections.Generic;
using Logisto.Models;

namespace Logisto.ViewModels
{
	public class UserViewModel : User
	{
		public Person Person { get; set; }

		public Dictionary<string, object> Dictionaries { get; set; }

		public UserViewModel()
		{
			Dictionaries = new Dictionary<string, object>();
		}
	}
}