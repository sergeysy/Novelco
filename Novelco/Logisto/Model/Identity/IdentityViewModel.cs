
namespace Logisto.ViewModels
{
	public class LoginViewModel
	{
		public string Login { get; set; }
		public string Password { get; set; }
	}

	public class ChangePasswordViewModel
	{
		public string Login { get; set; }
		public string Password { get; set; }
		public string Password2 { get; set; }
		public string Tip { get; set; }
	}

	public class ProfileViewModel
	{
		public string Login { get; set; }
		public string UserName { get; set; }
		public string Email { get; set; }
		public string Roles { get; set; }

		public string Family { get; set; }
		public string Name { get; set; }
		public string Patronymic { get; set; }
		public string Initials { get; set; }
		public string DisplayName { get; set; }
		public string GenitiveFamily { get; set; }
		public string GenitiveName { get; set; }
		public string GenitivePatronymic { get; set; }
		public string Address { get; set; }
		public string Comment { get; set; }
		public string EnName { get; set; }
		public bool IsNotResident { get; set; }
		public bool IsSubscribed { get; set; }
	}

	public class ChangeContractorPasswordViewModel
	{
		public int ContractorId { get; set; }
		public string Contractor { get; set; }
		public string Password { get; set; }
		public string Password2 { get; set; }
	}

	public class ChangeEmployeePasswordViewModel : ChangeContractorPasswordViewModel
	{
		public int EmployeeId { get; set; }
		public string Employee { get; set; }
	}
}