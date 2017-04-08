using Logisto.Models;

namespace Logisto.ViewModels
{
	public class PaymentViewModel : Payment
	{
		public double OriginalSum { get; set; }
		public double CurrencyRateCB { get; set; }
		public double CurrencyRate { get; set; }

		public int InvoiceCurrencyId { get; set; }
	}
}




















