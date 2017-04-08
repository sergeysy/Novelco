using System;
using System.Collections.Generic;
using Logisto.Models;

namespace Logisto.ViewModels
{
	public class DocumentEditModel : Document
	{
		public bool IsDeleted { get; set; }
	}
}