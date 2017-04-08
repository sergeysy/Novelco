using System.Threading.Tasks;
using Microsoft.AspNet.Identity;

namespace Logisto.Identity
{
	public class EmailService : IIdentityMessageService
	{
		public EmailService()
		{
		}

		public Task SendAsync(IdentityMessage message)
		{
			// TODO:
			return Task.FromResult(0);
		}
	}
}
