using System.Collections.Generic;

namespace Logisto.ViewModels
{
	public class EmployeesViewModel : IndexViewModel
	{
		public List<EmployeeViewModel> Items { get; set; }
	}
}