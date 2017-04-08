using System.Collections.Generic;
using Logisto.Models;

namespace Logisto.ViewModels
{
	public class UsersViewModel : IndexViewModel
	{
		public List<User> Items { get; set; }
	}
}