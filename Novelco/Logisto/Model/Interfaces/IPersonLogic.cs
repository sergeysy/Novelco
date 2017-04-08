using System.Collections.Generic;
using Logisto.Models;

namespace Logisto.BusinessLogic
{
	public interface IPersonLogic
	{
		int CreatePerson(Person person);
		Person GetPerson(int id);
		void UpdatePerson(Person person);
		void DeletePerson(int personId);

		/// <summary>
		/// Получить список персон с учетом фильтра
		/// </summary>
		IEnumerable<Person> GetPersons(ListFilter filter);

		/// <summary>
		/// Получить значение количества персон с учетом фильтра
		/// </summary>
		int GetPersonsCount(ListFilter filter);

		/// <summary>
		/// Получить список персон с учетом фильтра
		/// </summary>
		Person GetPersonByUser(int userId);

		#region phone

		int CreatePhone(Phone phone);

		/// <summary>
		/// Получить персону
		/// </summary>
		Phone GetPhone(int id);

		void UpdatePhone(Phone phone);

		void DeletePhone(int phoneId);

		/// <summary>
		/// Получить количество телефоны персоны
		/// </summary>
		int GetPhonesCountByPerson(int personId);

		/// <summary>
		/// Получить телефоны персоны
		/// </summary>
		IEnumerable<Phone> GetPhonesByPerson(int personId);

		#endregion

		#region person-phone

		int CreatePersonPhone(PersonPhone personPhone);

		/// <summary>
		/// Получить 
		/// </summary>
		PersonPhone GetPersonPhone(int id);
		PersonPhone GetPersonPhone(int personId, int phoneId);

		void DeletePersonPhone(int id);

		#endregion

		/// <summary>
		/// Поиск физлица по части ФИО
		/// </summary>
		IEnumerable<Person> SearchPersons(string term);
	}
}