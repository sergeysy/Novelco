using System;
using System.Collections.Generic;
using Logisto.Models;

namespace Logisto.ViewModels
{
	public class PersonEditModel: Person
	{
		public bool IsDeleted { get; set; }

		public List<PhoneEditModel> Phones { get; set; }

		public PersonEditModel()
		{
			Phones = new List<PhoneEditModel>();
		}
	}
}