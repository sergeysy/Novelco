using System;
using System.Collections.Generic;
using Logisto.Models;

namespace Logisto.ViewModels
{
	public class DocumentTypeEditModel : DocumentType
	{
		public List<int> Prints { get; set; }

		public DocumentTypeEditModel()
		{
			Prints = new List<int>();
		}
	}
}