using System;
using System.Collections.Generic;
using System.Linq;
using Logisto.Data;
using Logisto.Models;
using LinqToDB;

namespace Logisto.BusinessLogic
{
	public class EmployeeLogic : IEmployeeLogic
	{
		public int GetEmployeesCount(ListFilter filter)
		{
			using (var db = new LogistoDb())
			{
				var query = db.Employees.AsQueryable();

				// условия поиска

				if (!string.IsNullOrWhiteSpace(filter.Context))
				{
					var persons = db.Persons.Where(w => w.DisplayName.Contains(filter.Context)).Select(s => s.ID).ToList();

					query = query.Where(w => w.Position.Contains(filter.Context) ||
						w.Comment.Contains(filter.Context) ||
						//
						persons.Contains(w.PersonId.Value)
						);
				}

				if (!string.IsNullOrWhiteSpace(filter.Type))
					switch (filter.Type)
					{
						case "Our":
							query = query.Where(w => w.LegalId == 1 || w.LegalId == 2);
							break;
					}

				return query.Count();
			}
		}

		public IEnumerable<Employee> GetEmployees(ListFilter filter)
		{
			using (var db = new LogistoDb())
			{
				var query = from e in db.Employees
							join p in db.Persons on e.PersonId equals p.ID
							select new { e, p };

				// условия поиска

				if (!string.IsNullOrWhiteSpace(filter.Context))
				{
					query = query.Where(w => w.e.Position.Contains(filter.Context) ||
						w.e.Comment.Contains(filter.Context) ||
						//
						w.p.DisplayName.Contains(filter.Context)
						);
				}

				if (!string.IsNullOrWhiteSpace(filter.Type))
					switch (filter.Type)
					{
						case "Our":
							var ours = db.OurLegals.Select(s=>s.LegalId).ToList();
							query = query.Where(w => ours.Contains(w.e.LegalId));
							break;
					}

				// сортировка
				if (!string.IsNullOrWhiteSpace(filter.Sort))
				{
					if (string.IsNullOrWhiteSpace(filter.SortDirection))
						filter.SortDirection = "Asc";

					switch (filter.Sort)
					{
						case "ID":
							if (filter.SortDirection == "Asc")
								query = query.OrderBy(o => o.e.ID);
							else
								query = query.OrderByDescending(o => o.e.ID);

							break;

						case "EmployeeStatusId":
							if (filter.SortDirection == "Asc")
								query = query.OrderBy(o => o.e.EmployeeStatusId);
							else
								query = query.OrderByDescending(o => o.e.EmployeeStatusId);

							break;

						case "Name":
							if (filter.SortDirection == "Asc")
								query = query.OrderBy(o => o.p.DisplayName);
							else
								query = query.OrderByDescending(o => o.p.DisplayName);

							break;

						case "BeginDate":
							if (filter.SortDirection == "Asc")
								query = query.OrderBy(o => o.e.BeginDate);
							else
								query = query.OrderByDescending(o => o.e.BeginDate);

							break;

						case "EndDate":
							if (filter.SortDirection == "Asc")
								query = query.OrderBy(o => o.e.EndDate);
							else
								query = query.OrderByDescending(o => o.e.EndDate);

							break;
					}
				}

				// пейджинг

				if (filter.PageNumber > 0)
					query = query.Skip(filter.PageNumber * filter.PageSize);

				if (filter.PageSize > 0)
					query = query.Take(filter.PageSize);

				return query.Select(s => s.e).ToList();
			}
		}

		public int CreateEmployee(Employee employee)
		{
			using (var db = new LogistoDb())
			{
				var id = Convert.ToInt32(db.InsertWithIdentity(employee));
				return id;
			}
		}

		public Employee GetEmployee(int id)
		{
			using (var db = new LogistoDb())
			{
				return db.Employees.First(w => w.ID == id);
			}
		}

		public void UpdateEmployee(Employee employee)
		{
			using (var db = new LogistoDb())
			{
				db.Employees.Where(w => w.ID == employee.ID)
				.Set(u => u.PersonId, employee.PersonId)
				.Set(u => u.LegalId, employee.LegalId)
				.Set(u => u.FinRepCenterId, employee.FinRepCenterId)
				.Set(u => u.BeginDate, employee.BeginDate)
				.Set(u => u.EndDate, employee.EndDate)
				.Set(u => u.Department, employee.Department)
				.Set(u => u.Position, employee.Position)
				.Set(u => u.GenitivePosition, employee.GenitivePosition)
				.Set(u => u.Comment, employee.Comment)
				.Set(u => u.Basis, employee.Basis)
				.Set(u => u.EnPosition, employee.EnPosition)
				.Set(u => u.EnBasis, employee.EnBasis)
				.Set(u => u.EmployeeStatusId, employee.EmployeeStatusId)
				.Update();
			}
		}

		public void UpdateEmployeeSignature(Employee employee)
		{
			using (var db = new LogistoDb())
			{
				db.Employees.Where(w => w.ID == employee.ID)
				.Set(u => u.Signature, employee.Signature)
				.Update();
			}
		}

		public void DeleteEmployee(int employeeId)
		{
			using (var db = new LogistoDb())
			{
				db.Employees.Delete(w => w.ID == employeeId);
			}
		}

		public IEnumerable<Employee> GetEmployeesByPerson(int personId)
		{
			using (var db = new LogistoDb())
			{
				return db.Employees.Where(w => w.PersonId == personId).ToList();
			}
		}

		public IEnumerable<Employee> GetEmployeesByLegal(int legalId)
		{
			using (var db = new LogistoDb())
			{
				return db.Employees.Where(w => w.LegalId == legalId).ToList();
			}
		}

		public IEnumerable<Employee> GetEmployeesByContractor(int contractorId)
		{
			using (var db = new LogistoDb())
			{
				var query = from e in db.Employees
							join l in db.Legals on e.LegalId equals l.ID
							where l.ContractorId == contractorId || e.ContractorId == contractorId
							select e;

				return query;
			}
		}
	}
}