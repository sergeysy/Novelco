using System;
using Logisto.Models;

namespace Logisto.ViewModels
{
	public class BankAccountEditModel : BankAccount
	{
		public int? CurrencyRateUseId { get; set; }
		public bool IsDeleted { get; set; }
	}
}