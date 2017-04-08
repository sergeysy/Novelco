using System;
using System.Collections.Generic;
using Logisto.Models;

namespace Logisto.ViewModels
{
	public class RouteContactViewModel : RouteContact
	{
		public string LegalName { get; set; }
		public bool IsDeleted { get; set; }
	}
}