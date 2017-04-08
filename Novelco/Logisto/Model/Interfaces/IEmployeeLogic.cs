using System.Collections.Generic;
using Logisto.Models;

namespace Logisto.BusinessLogic
{
	public interface IEmployeeLogic
	{
		int CreateEmployee(Employee employee);
		Employee GetEmployee(int id);
		void UpdateEmployee(Employee employee);
		void UpdateEmployeeSignature(Employee employee);
		void DeleteEmployee(int employeeId);

		/// <summary>
		/// Получить список сотрудников с учетом фильтра
		/// </summary>
		IEnumerable<Employee> GetEmployees(ListFilter filter);

		/// <summary>
		/// Получить значение количества сотрудников с учетом фильтра
		/// </summary>
		int GetEmployeesCount(ListFilter filter);

		/// <summary>
		/// Получить список сотрудников для физлица
		/// </summary>
		IEnumerable<Employee> GetEmployeesByPerson(int personId);

		/// <summary>
		/// Получить список сотрудников для юрлица
		/// </summary>
		IEnumerable<Employee> GetEmployeesByLegal(int legalId);

		IEnumerable<Employee> GetEmployeesByContractor(int contractorId);
	}
}