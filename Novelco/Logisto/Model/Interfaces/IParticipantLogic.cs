using System;
using System.Collections.Generic;
using Logisto.Models;

namespace Logisto.BusinessLogic
{
	public interface IParticipantLogic
	{
		#region Participants

		int CreateParticipant(Participant entity);
		Participant GetParticipant(int entityId);
		void UpdateParticipant(Participant entity);
		void DeleteParticipant(int entityId);

		/// <summary>
		/// Получить количество участников рабочей группы контрагента
		/// </summary>
		int GetParticipantCountByContractor(int contractorId);

		/// <summary>
		/// Получить участников рабочей группы контрагента
		/// </summary>
		IEnumerable<Participant> GetWorkgroupByContractor(int contractorId);

		/// <summary>
		/// Получить участников рабочей группы заказа
		/// </summary>
		IEnumerable<Participant> GetWorkgroupByOrder(int orderId);
		IEnumerable<Participant> GetWorkgroupByOrderAtDate(int orderId, DateTime date);

		void ClearWorkgroupByOrder(int orderId);

		#endregion

		bool IsAllowedActionByContractor(int actionId, int contractorId, int userId);
		bool IsAllowedActionByOrder(int actionId, int orderId, int userId);

		IEnumerable<ParticipantRole> GetAllowedRoles(int actionId);

		Models.Action GetAction(int actionId);

		List<Models.Action> GetAllActions();

		void AllowRole(int actionId, int participantRoleId, bool allow);
	}
}