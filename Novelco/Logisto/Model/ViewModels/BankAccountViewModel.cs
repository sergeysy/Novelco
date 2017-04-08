using Logisto.Models;

namespace Logisto.ViewModels
{
	public class BankAccoundViewModel : BankAccount
	{
		public string BankName { get; set; }
		public string BIC { get; set; }
		public bool IsDeleted { get; set; }
	}
}