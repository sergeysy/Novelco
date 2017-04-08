using System;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using Logisto.Models;
using Logisto.ViewModels;
using Newtonsoft.Json;
using Logisto.BusinessLogic;

namespace Logisto.Controllers
{
	[Authorize]
	public class EmployeesController : BaseController
	{
		#region Pages

		public ActionResult Index()
		{
			//
			RecalculateEmployeeStatus();

			if (!IsSuperUser())
				return RedirectToAction("NotAuthorized", "Home");

			var crmLogic = new CrmLogic();
			var pageSize = int.Parse(ConfigurationManager.AppSettings["App_PageSize"]);
			var filter = new ListFilter { PageSize = pageSize, Type = "Our" };
			if (CurrentUserId > 0)
			{
				filter.Sort = userLogic.GetSetting(CurrentUserId, "Employees.Sort");
				filter.SortDirection = userLogic.GetSetting(CurrentUserId, "Employees.SortDirection");
				filter.PageNumber = Convert.ToInt32(userLogic.GetSetting(CurrentUserId, "Employees.PageNumber"));
			}

			var totalCount = employeeLogic.GetEmployeesCount(filter);
			var list = employeeLogic.GetEmployees(filter);
			var persons = dataLogic.GetPersons();
			var users = userLogic.GetUsers(new ListFilter());

			var viewModel = new EmployeesViewModel
			{
				Filter = filter,
				Items = list.Select(item => new EmployeeViewModel
				{
					ID = item.ID,
					LegalId = item.LegalId,
					Department = item.Department,
					Position = item.Position,
					Basis = item.Basis,
					BeginDate = item.BeginDate,
					Comment = item.Comment,
					EnBasis = item.EnBasis,
					EndDate = item.EndDate,
					EnPosition = item.EnPosition,
					GenitivePosition = item.GenitivePosition,
					PersonId = item.PersonId,
					EmployeeStatusId = item.EmployeeStatusId,
					FinRepCenterId = item.FinRepCenterId,

					UserId = users.Where(w => w.PersonId == item.PersonId).Select(s => s.ID).FirstOrDefault(),
					Name = persons.Where(w => w.ID == item.PersonId).Select(s => s.Display).FirstOrDefault()
				}).ToList(),
				TotalItemsCount = totalCount
			};

			viewModel.Dictionaries.Add("PositionTemplate", dataLogic.GetPositionTemplates());
			viewModel.Dictionaries.Add("EmployeeStatus", dataLogic.GetEmployeeStatuses());
			viewModel.Dictionaries.Add("FinRepCenter", dataLogic.GetFinRepCenters());
			viewModel.Dictionaries.Add("CrmManager", crmLogic.GetAllManagers());
			viewModel.Dictionaries.Add("OurLegal", legalLogic.GetOurLegals().Select(s => new { s.ID, s.LegalId, s.Name }));
			viewModel.Dictionaries.Add("Legal", dataLogic.GetLegals());

			return View(viewModel);
		}

		public ActionResult Create()
		{
			var person = new Person { Comment = "" };

			var model = new PersonViewModel
			{
				ID = person.ID,
				Email = person.Email,
				Name = person.Name,
				Address = person.Address,
				Comment = person.Comment,
				DisplayName = person.DisplayName,
				EnName = person.EnName,
				Family = person.Family,
				GenitiveFamily = person.GenitiveFamily,
				GenitiveName = person.GenitiveName,
				GenitivePatronymic = person.GenitivePatronymic,
				Initials = person.Initials,
				IsNotResident = person.IsNotResident,
				IsSubscribed = person.IsSubscribed,
				Patronymic = person.Patronymic
			};

			model.Dictionaries.Add("FinRepCenter", dataLogic.GetFinRepCenters());
			model.Dictionaries.Add("PhoneType", dataLogic.GetPhoneTypes());
			model.Dictionaries.Add("OurLegal", dataLogic.GetOurLegals());

			return View(model);
		}

		#endregion

		public ContentResult GetItems(ListFilter filter)
		{
			userLogic.SetSetting(CurrentUserId, "Employees.Sort", filter.Sort);
			userLogic.SetSetting(CurrentUserId, "Employees.PageNumber", filter.PageNumber.ToString());
			userLogic.SetSetting(CurrentUserId, "Employees.SortDirection", filter.SortDirection);

			var totalCount = employeeLogic.GetEmployeesCount(filter);
			var list = employeeLogic.GetEmployees(filter);
			var persons = dataLogic.GetPersons();

			var viewlist = list.Select(item => new EmployeeViewModel
			{
				ID = item.ID,
				LegalId = item.LegalId,
				Department = item.Department,
				Position = item.Position,
				Basis = item.Basis,
				BeginDate = item.BeginDate,
				Comment = item.Comment,
				EnBasis = item.EnBasis,
				EndDate = item.EndDate,
				EnPosition = item.EnPosition,
				GenitivePosition = item.GenitivePosition,
				PersonId = item.PersonId,
				EmployeeStatusId = item.EmployeeStatusId,
				FinRepCenterId = item.FinRepCenterId,

				Name = persons.Where(w => w.ID == item.PersonId).Select(s => s.Display).FirstOrDefault()
			}).ToList();

			return Content(JsonConvert.SerializeObject(new { Items = viewlist, TotalCount = totalCount }));
		}

		public ContentResult GetNewPhone()
		{
			// подстановка значений по-умолчанию
			var phone = new Phone { };
			return Content(JsonConvert.SerializeObject(phone));
		}

		public ContentResult Save(PersonEditModel model, string login)
		{
			// create prerson
			var person = new Person();
			person.Name = model.Name;
			person.DisplayName = model.DisplayName;
			person.EnName = model.EnName;
			person.Address = model.Address;
			person.Email = model.Email;
			person.IsNotResident = model.IsNotResident;
			person.Comment = model.Comment;
			person.Family = (string.IsNullOrEmpty(model.Family)) ? "" : model.Family.ToUpperInvariant();
			person.GenitiveFamily = model.GenitiveFamily;
			person.GenitiveName = model.GenitiveName;
			person.GenitivePatronymic = model.GenitivePatronymic;
			person.Initials = model.Initials;
			person.IsSubscribed = model.IsSubscribed;
			person.Patronymic = model.Patronymic;

			var personId = personLogic.CreatePerson(person);

			// phones
			foreach (var phone in model.Phones)
				SavePhone(phone, personId);

			// create user
			var user = new IdentityUser();
			user.Login = login;
			user.Name = model.Name;
			user.Email = model.Email;
			user.PersonId = personId;
			var userId = identityLogic.CreateUser(user);

			// create employee for НОВЕЛКО

			var employee = new Employee();
			employee.PersonId = personId;
			employee.LegalId = 1;
			employee.Comment = model.Comment;

			employeeLogic.CreateEmployee(employee);

			//// create employee for НОВЕЛ логистик

			//employee = new Employee();
			//employee.PersonId = personId;
			//employee.LegalId = 2;
			//employee.Comment = model.Comment;

			//employeeLogic.CreateEmployee(employee);

			return Content(JsonConvert.SerializeObject(""));
		}

		[HttpPost]
		public ContentResult UploadPhoto(int id)
		{
			if (id <= 0 || Request.Files.Count == 0)
				return Content(JsonConvert.SerializeObject(new { Message = "Неверные входные данные" }));

			var file = Request.Files[0];

			var user = userLogic.GetUser(id);
			user.Photo = new byte[file.InputStream.Length];
			file.InputStream.Read(user.Photo, 0, (int)file.InputStream.Length);

			userLogic.UpdatePhoto(user);

			return Content(JsonConvert.SerializeObject(new { CurrentUserId = CurrentUserId }));
		}

		[HttpPost]
		public ContentResult UploadSign(int id)
		{
			if (id <= 0 || Request.Files.Count == 0)
				return Content(JsonConvert.SerializeObject(new { Message = "Неверные входные данные" }));

			var file = Request.Files[0];

			var employee = employeeLogic.GetEmployee(id);
			employee.Signature = new byte[file.InputStream.Length];
			file.InputStream.Read(employee.Signature, 0, (int)file.InputStream.Length);

			employeeLogic.UpdateEmployeeSignature(employee);

			return Content(JsonConvert.SerializeObject(new { CurrentUserId = CurrentUserId }));
		}

		public ContentResult SaveEmployee(EmployeeEditModel model)
		{
			if (model.ContractorId.HasValue && contractorLogic.GetContractor(model.ContractorId.Value).IsLocked)
				return Content(JsonConvert.SerializeObject(new { Message = "Контрагент зафиксирован, менять его данные нельзя." }));

			if ((model.ID == 0) && !model.IsDeleted)
			{
				// новый сотрудник
				var employee = new Employee();
				employee.PersonId = model.PersonId;
				employee.LegalId = model.LegalId;
				employee.ContractorId = model.ContractorId;
				employee.BeginDate = model.BeginDate;
				employee.EndDate = model.EndDate;
				employee.Department = model.Department;
				employee.Position = model.Position;
				employee.GenitivePosition = model.GenitivePosition;
				employee.Comment = model.Comment;
				employee.Basis = model.Basis;
				employee.EnPosition = model.EnPosition;
				employee.EnBasis = model.EnBasis;
				employee.FinRepCenterId = model.FinRepCenterId;

				employeeLogic.CreateEmployee(employee);
			}
			else
			{
				if (model.IsDeleted)
				{
					// удалить сотрудника
					employeeLogic.DeleteEmployee(model.ID);
				}
				else
				{
					// обновить сотрудника
					var employee = employeeLogic.GetEmployee(model.ID);
					employee.PersonId = model.PersonId;
					employee.LegalId = model.LegalId;
					employee.ContractorId = model.ContractorId;
					employee.BeginDate = model.BeginDate;
					employee.EndDate = model.EndDate;
					employee.Department = model.Department;
					employee.Position = model.Position;
					employee.GenitivePosition = model.GenitivePosition;
					employee.Comment = model.Comment;
					employee.Basis = model.Basis;
					employee.EnPosition = model.EnPosition;
					employee.EnBasis = model.EnBasis;
					employee.FinRepCenterId = model.FinRepCenterId;

					employeeLogic.UpdateEmployee(employee);

					// пробросить дату окончания работы по всем его заказам если это финальное ограничение для этого пользователя
					if (model.EndDate.HasValue)
					{
						if (!employeeLogic.GetEmployees(new ListFilter { Type = "Our" }).Any(w => w.ID != employee.ID && w.PersonId == employee.PersonId && !w.PositionEndDate.HasValue))
						{
							var user = userLogic.GetUserByEmployee(employee.ID);
							var orders = orderLogic.GetAllOrders();
							foreach (var order in orders)
								foreach (var participant in participantLogic.GetWorkgroupByOrder(order.ID).Where(w => w.UserId == user.ID))
									if (!participant.ToDate.HasValue)
									{
										participant.ToDate = employee.EndDate.Value;
										participantLogic.UpdateParticipant(participant);
									}

							var contractors = contractorLogic.GetContractors(new ListFilter());
							foreach (var contractor in contractors)
								foreach (var participant in participantLogic.GetWorkgroupByContractor(contractor.ID).Where(w => w.UserId == user.ID))
									if (!participant.ToDate.HasValue)
									{
										participant.ToDate = employee.EndDate.Value;
										participantLogic.UpdateParticipant(participant);
									}
						}
					}
				}
			}

			return Content(string.Empty);
		}

		[OutputCache(NoStore = true, Duration = 0)]
		public FileResult GetSign(int id)
		{
			var employee = employeeLogic.GetEmployee(id);
			var contentType = "application/octet-stream";
			if (employee == null || employee.Signature == null)
				return null;

			return File(employee.Signature, contentType);
		}

		void SavePhone(PhoneEditModel model, int personId)
		{
			if ((model.ID == 0) && !model.IsDeleted)
			{
				// новый 
				var phone = new Phone();
				phone.Name = model.Name;
				phone.Number = model.Number;
				phone.TypeId = model.TypeId;

				var phoneId = personLogic.CreatePhone(phone);

				var rel = new PersonPhone();
				rel.PersonId = personId;
				rel.PhoneId = phoneId;
				personLogic.CreatePersonPhone(rel);
			}
			else
			{
				if (model.IsDeleted)
				{
					// удалить 
					personLogic.DeletePhone(model.ID);
				}
				else
				{
					// обновить 
					var phone = personLogic.GetPhone(model.ID);
					phone.Name = model.Name;
					phone.Number = model.Number;
					phone.TypeId = model.TypeId;

					personLogic.UpdatePhone(phone);
				}
			}
		}

		void RecalculateEmployeeStatus()
		{
			var filter = new ListFilter { Type = "Our" };
			var ours = legalLogic.GetOurLegals();
			foreach (var our in ours)
			{
				var list = employeeLogic.GetEmployees(filter).Where(w => w.LegalId == our.LegalId).ToList();
				foreach (var employee in list)
				{
					var currStatus = employee.EmployeeStatusId;
					if ((employee.EndDate.HasValue) && (employee.EndDate.Value < DateTime.Now))
						employee.EmployeeStatusId = 2;  // неактивен
					else
						employee.EmployeeStatusId = 1;  // активен

					if (employee.EmployeeStatusId != currStatus)
						employeeLogic.UpdateEmployee(employee);
				}
			}
		}
	}
}






