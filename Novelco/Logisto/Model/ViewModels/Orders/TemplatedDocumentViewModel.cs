using System;
using System.Collections.Generic;
using Logisto.Models;

namespace Logisto.ViewModels
{
	public class TemplatedDocumentViewModel : TemplatedDocument
	{
		public bool HasPdf { get; set; }
		public bool HasCleanPdf { get; set; }
		public bool HasCutPdf { get; set; }

		public DateTime Date { get; set; }
		public string AccountingNumber { get; set; }

		// используются на странице отображения документа
		public Dictionary<string, object> Dictionaries { get; set; }

		public TemplatedDocumentViewModel()
		{
			Dictionaries = new Dictionary<string, object>();
		}
	}
}