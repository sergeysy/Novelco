using Logisto.Models;

namespace Logisto.ViewModels
{
	public class EmployeeViewModel : Employee
	{
		public string Name { get; set; }
		public string Email { get; set; }
		public string PhoneMobile { get; set; }
		public string PhoneWork { get; set; }
		public int UserId { get; set; }
		public bool IsDeleted { get; set; }
	}
}