using System;
using System.Collections.Generic;
using System.Linq;
using Logisto.Data;
using Logisto.Models;
using LinqToDB;

namespace Logisto.BusinessLogic
{
	public class PersonLogic : IPersonLogic
	{
		public int GetPersonsCount(ListFilter filter)
		{
			using (var db = new LogistoDb())
			{
				var query = db.Persons.AsQueryable();

				// условия поиска

				if (!string.IsNullOrWhiteSpace(filter.Context))
					query = query.Where(w => w.Family.Contains(filter.Context) ||
						w.Patronymic.Contains(filter.Context) ||
						w.Name.Contains(filter.Context) ||
						w.EnName.Contains(filter.Context) ||
						w.Address.Contains(filter.Context) ||
						w.Comment.Contains(filter.Context)
						);

				return query.Count();
			}

		}

		public IEnumerable<Person> GetPersons(ListFilter filter)
		{
			using (var db = new LogistoDb())
			{
				var query = db.Persons.AsQueryable();

				// условия поиска

				if (!string.IsNullOrWhiteSpace(filter.Context))
					query = query.Where(w => w.Family.Contains(filter.Context) ||
										w.Patronymic.Contains(filter.Context) ||
										w.Name.Contains(filter.Context) ||
										w.EnName.Contains(filter.Context) ||
										w.Address.Contains(filter.Context) ||
										w.Comment.Contains(filter.Context)
										);
				// TODO: sort

				// пейджинг

				if (filter.PageNumber > 0)
					query = query.Skip(filter.PageNumber * filter.PageSize);

				if (filter.PageSize > 0)
					query = query.Take(filter.PageSize);

				return query.ToList();
			}

		}

		public Person GetPerson(int id)
		{
			using (var db = new LogistoDb())
			{
				return db.Persons.First(w => w.ID == id);
			}
		}

		public int CreatePerson(Person person)
		{
			// PreProcessing
			if (person.IsNotResident)
			{
				person.GenitiveFamily = person.Family;
				person.GenitiveName = person.Name;
				person.DisplayName = person.Family + " " + person.Name;
				person.Initials = person.Name;
			}
			else
			{
				person.Initials = (string.IsNullOrWhiteSpace(person.Name)) ? "" : person.Name.Substring(0, 1) + ".";
				person.Initials += (string.IsNullOrWhiteSpace(person.Patronymic)) ? "" : person.Patronymic.Substring(0, 1) + ".";
				person.DisplayName = person.Family + " " + person.Initials;
				person.DisplayName = person.DisplayName.ToUpperInvariant();
			}

			using (var db = new LogistoDb())
			{
				return Convert.ToInt32(db.InsertWithIdentity(person));
			}
		}

		public void UpdatePerson(Person person)
		{
			// PreProcessing
			if (person.IsNotResident)
			{
				person.GenitiveFamily = person.Family;
				person.GenitiveName = person.Name;
				person.DisplayName = person.Family + " " + person.Name;
				person.Initials = person.Name;
			}
			else
			{
				person.Initials = (string.IsNullOrWhiteSpace(person.Name)) ? "" : person.Name.Substring(0, 1) + ".";
				person.Initials += (string.IsNullOrWhiteSpace(person.Patronymic)) ? "" : person.Patronymic.Substring(0, 1) + ".";
				person.DisplayName = person.Family + " " + person.Initials;
				person.DisplayName = person.DisplayName.ToUpper();
			}

			using (var db = new LogistoDb())
			{
				db.Persons.Where(w => w.ID == person.ID)
					.Set(u => u.Address, person.Address)
					.Set(u => u.Comment, person.Comment)
					.Set(u => u.DisplayName, person.DisplayName)
					.Set(u => u.Email, person.Email)
					.Set(u => u.EnName, person.EnName)
					.Set(u => u.Family, person.Family)
					.Set(u => u.GenitiveFamily, person.GenitiveFamily)
					.Set(u => u.GenitiveName, person.GenitiveName)
					.Set(u => u.GenitivePatronymic, person.GenitivePatronymic)
					.Set(u => u.Initials, person.Initials)
					.Set(u => u.IsNotResident, person.IsNotResident)
					.Set(u => u.IsSubscribed, person.IsSubscribed)
					.Set(u => u.Name, person.Name)
					.Set(u => u.Patronymic, person.Patronymic)
				.Update();
			}
		}

		public void DeletePerson(int personId)
		{
			using (var db = new LogistoDb())
			{
				db.Persons.Delete(w => w.ID == personId);
			}
		}

		public Person GetPersonByUser(int userId)
		{
			using (var db = new LogistoDb())
			{
				var user = db.Users.FirstOrDefault(w => w.ID == userId);
				if ((user != null) && (user.PersonId.HasValue))
					return db.Persons.First(w => w.ID == user.PersonId);
			}

			return null;
		}

		#region phone

		public Phone GetPhone(int id)
		{
			using (var db = new LogistoDb())
			{
				return db.Phones.First(w => w.ID == id);
			}
		}

		public int CreatePhone(Phone phone)
		{
			using (var db = new LogistoDb())
			{
				return Convert.ToInt32(db.InsertWithIdentity(phone));
			}
		}

		public void UpdatePhone(Phone phone)
		{
			using (var db = new LogistoDb())
			{
				db.Phones.Where(w => w.ID == phone.ID)
					.Set(u => u.Name, phone.Name)
					.Set(u => u.Number, phone.Number)
					.Set(u => u.TypeId, phone.TypeId)
				.Update();
			}
		}

		public void DeletePhone(int phoneId)
		{
			using (var db = new LogistoDb())
			{
				db.Phones.Delete(w => w.ID == phoneId);
			}
		}

		public int GetPhonesCountByPerson(int personId)
		{
			using (var db = new LogistoDb())
			{
				var phoneIds = db.PersonPhones.Where(w => w.PersonId == personId).Select(s => s.PhoneId);
				return db.Phones.Where(w => phoneIds.Contains(w.ID)).Count();
			}
		}

		public IEnumerable<Phone> GetPhonesByPerson(int personId)
		{
			using (var db = new LogistoDb())
			{
				var phoneIds = db.PersonPhones.Where(w => w.PersonId == personId).Select(s => s.PhoneId);
				return db.Phones.Where(w => phoneIds.Contains(w.ID));
			}
		}

		#endregion

		#region person-phone

		public PersonPhone GetPersonPhone(int id)
		{
			using (var db = new LogistoDb())
			{
				return db.PersonPhones.First(w => w.ID == id);
			}
		}

		public PersonPhone GetPersonPhone(int personId, int phoneId)
		{
			using (var db = new LogistoDb())
			{
				return db.PersonPhones.FirstOrDefault(w => w.PersonId == personId && w.PhoneId == phoneId);
			}
		}

		public int CreatePersonPhone(PersonPhone phone)
		{
			using (var db = new LogistoDb())
			{
				return Convert.ToInt32(db.InsertWithIdentity(phone));
			}
		}

		public void DeletePersonPhone(int id)
		{
			using (var db = new LogistoDb())
			{
				db.PersonPhones.Delete(w => w.ID == id);
			}
		}

		#endregion

		public IEnumerable<Person> SearchPersons(string term)
		{
			using (var db = new LogistoDb())
			{
				return db.Persons.Where(w => w.DisplayName.Contains(term)).ToList();
			}
		}
	}
}