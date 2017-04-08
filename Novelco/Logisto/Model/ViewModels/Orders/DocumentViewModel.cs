using System;
using System.Collections.Generic;
using Logisto.Models;

namespace Logisto.ViewModels
{
	public class DocumentViewModel : Document
	{
		public string OrderAccountingName { get; set; }

		// используются на странице отображения документа
		public Dictionary<string, object> Dictionaries { get; set; }

		public DocumentViewModel()
		{
			Dictionaries = new Dictionary<string, object>();
		}
	}
}