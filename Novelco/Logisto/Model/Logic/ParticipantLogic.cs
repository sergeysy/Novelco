using System;
using System.Collections.Generic;
using System.Linq;
using LinqToDB;
using Logisto.Data;
using Logisto.Models;

namespace Logisto.BusinessLogic
{
	public class ParticipantLogic : IParticipantLogic
	{
		#region workgroup

		public int GetParticipantCountByContractor(int contractorId)
		{
			using (var db = new LogistoDb())
			{
				return db.Participants.Where(w => w.ContractorId == contractorId).Count();
			}
		}

		public IEnumerable<Participant> GetWorkgroupByContractor(int contractorId)
		{
			using (var db = new LogistoDb())
			{
				return db.Participants.Where(w => w.ContractorId == contractorId).OrderBy(o => o.ParticipantRoleId).ToList();
			}
		}

		public IEnumerable<Participant> GetWorkgroupByOrder(int orderId)
		{
			using (var db = new LogistoDb())
			{
				return db.Participants.Where(w => w.OrderId == orderId).OrderBy(o => o.ParticipantRoleId).ToList();
			}
		}

		public IEnumerable<Participant> GetWorkgroupByOrderAtDate(int orderId, DateTime date)
		{
			using (var db = new LogistoDb())
			{
				var wg = db.Participants.Where(w => w.OrderId == orderId).ToList();
				var result = new List<Participant>();
				foreach (var item in wg)
				{
					bool allowed = false;
					if (!item.FromDate.HasValue && !item.ToDate.HasValue)
						allowed = true;

					if ((item.FromDate.HasValue && !item.ToDate.HasValue) && (item.FromDate < date))
						allowed = true;

					if ((!item.FromDate.HasValue && item.ToDate.HasValue) && (item.ToDate > date))
						allowed = true;

					if ((item.FromDate.HasValue && item.ToDate.HasValue) && (item.FromDate < date) && (item.ToDate > date))
						allowed = true;

					if (allowed)
						result.Add(item);
				}

				return result;
			}
		}

		public Participant GetParticipant(int entityId)
		{
			using (var db = new LogistoDb())
			{
				return db.Participants.FirstOrDefault(w => w.ID == entityId);
			}
		}

		public int CreateParticipant(Participant entity)
		{
			using (var db = new LogistoDb())
			{
				if (entity.FromDate.HasValue)
					entity.FromDate = entity.FromDate.Value.Date;

				if (entity.ToDate.HasValue)
					entity.ToDate = entity.ToDate.Value.Date.AddDays(1).AddSeconds(-1);

				var id = Convert.ToInt32(db.InsertWithIdentity(entity));

				//if (entity.IsResponsible)
				//{
				//	// снять ответственность с другого участника
				//	var workgroup = db.Participants.First(w => w.ID == id);
				//	db.Participants.Where(w => w.ID != id && w.ContractorId == workgroup.ContractorId && w.OrderId == workgroup.OrderId)
				//	.Set(u => u.IsResponsible, false)
				//	.Update();
				//}

				return id;
			}
		}

		public void UpdateParticipant(Participant entity)
		{
			using (var db = new LogistoDb())
			{
				if (entity.FromDate.HasValue)
					entity.FromDate = entity.FromDate.Value.Date;

				if (entity.ToDate.HasValue)
					entity.ToDate = entity.ToDate.Value.Date.AddDays(1).AddSeconds(-1);

				db.Participants.Where(w => w.ID == entity.ID)
				.Set(u => u.ParticipantRoleId, entity.ParticipantRoleId)
				.Set(u => u.UserId, entity.UserId)
				.Set(u => u.IsDeputy, entity.IsDeputy)
				.Set(u => u.FromDate, entity.FromDate)
				.Set(u => u.ToDate, entity.ToDate)
				.Set(u => u.IsResponsible, entity.IsResponsible)
				.Update();

				//if (entity.IsResponsible)
				//{
				//	// снять ответственность с другого участника
				//	var workgroup = db.Participants.First(w => w.ID == entity.ID);

				//	db.Participants.Where(w => w.ID != entity.ID && w.ContractorId == workgroup.ContractorId && w.OrderId == workgroup.OrderId)
				//	.Set(u => u.IsResponsible, false)
				//	.Update();
				//}

				if (entity.IsDeputy)
				{
					// снять заместительность с другого участника
					var workgroup = db.Participants.First(w => w.ID == entity.ID);

					db.Participants.Where(w => w.ID != entity.ID && w.ContractorId == workgroup.ContractorId && w.OrderId == workgroup.OrderId)
					.Set(u => u.IsDeputy, false)
					.Update();
				}
			}
		}

		public void DeleteParticipant(int id)
		{
			using (var db = new LogistoDb())
			{
				db.Participants.Delete(w => w.ID == id);
			}
		}

		public void ClearWorkgroupByOrder(int orderId)
		{
			using (var db = new LogistoDb())
			{
				db.Participants.Delete(w => w.OrderId == orderId);
			}
		}

		#endregion

		public bool IsAllowedActionByContractor(int actionId, int contractorId, int userId)
		{
			using (var db = new LogistoDb())
			{
				var roles = (from p in db.ParticipantPermissions
							 from r in db.ParticipantRoles.Where(w => w.ID == p.ParticipantRoleId).DefaultIfEmpty()
							 where p.ActionId == actionId
							 select r.ID).ToList();

				DateTime date = DateTime.Now;
				var participants = db.Participants.Where(w => w.ContractorId == contractorId && w.UserId == userId && roles.Contains(w.ParticipantRoleId.Value)).ToList();
				foreach (var item in participants)
				{
					if (!item.FromDate.HasValue && !item.ToDate.HasValue)
						return true;

					if ((item.FromDate.HasValue && !item.ToDate.HasValue) && (item.FromDate < date))
						return true;

					if ((!item.FromDate.HasValue && item.ToDate.HasValue) && (item.ToDate > date))
						return true;

					if ((item.FromDate.HasValue && item.ToDate.HasValue) && (item.FromDate < date) && (item.ToDate > date))
						return true;
				}
			}

			return false;
		}

		public bool IsAllowedActionByOrder(int actionId, int orderId, int userId)
		{
			using (var db = new LogistoDb())
			{
				var roles = (from p in db.ParticipantPermissions
							 from r in db.ParticipantRoles.Where(w => w.ID == p.ParticipantRoleId).DefaultIfEmpty()
							 where p.ActionId == actionId
							 select r.ID).ToList();

				DateTime date = DateTime.Now;
				var participants = db.Participants.Where(w => w.OrderId == orderId && w.UserId == userId && roles.Contains(w.ParticipantRoleId.Value)).ToList();
				foreach (var item in participants)
				{
					if (!item.FromDate.HasValue && !item.ToDate.HasValue)
						return true;

					if ((item.FromDate.HasValue && !item.ToDate.HasValue) && (item.FromDate < date))
						return true;

					if ((!item.FromDate.HasValue && item.ToDate.HasValue) && (item.ToDate > date))
						return true;

					if ((item.FromDate.HasValue && item.ToDate.HasValue) && (item.FromDate < date) && (item.ToDate > date))
						return true;
				}
			}

			return false;
		}

		public IEnumerable<ParticipantRole> GetAllowedRoles(int actionId)
		{
			using (var db = new LogistoDb())
			{
				return (from p in db.ParticipantPermissions
						from r in db.ParticipantRoles.Where(w => w.ID == p.ParticipantRoleId).DefaultIfEmpty()
						where p.ActionId == actionId
						select r).ToList();
			}
		}

		public Models.Action GetAction(int actionId)
		{
			using (var db = new LogistoDb())
			{
				return db.Actions.FirstOrDefault(w => w.ID == actionId);
			}
		}

		public List<Models.Action> GetAllActions()
		{
			using (var db = new LogistoDb())
			{
				return db.Actions.ToList();
			}
		}

		public void AllowRole(int actionId, int participantRoleId, bool allow)
		{
			using (var db = new LogistoDb())
			{
				if (allow)
				{
					var permission = db.ParticipantPermissions.FirstOrDefault(w => w.ActionId == actionId && w.ParticipantRoleId == participantRoleId);
					if (permission == null)
						db.InsertWithIdentity(new ParticipantPermission { ActionId = actionId, ParticipantRoleId = participantRoleId });
				}
				else
					db.ParticipantPermissions.Delete(w => w.ActionId == actionId && w.ParticipantRoleId == participantRoleId);
			}
		}
	}
}