using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LinqToDB;
using Logisto.Data;
using Logisto.Models;

namespace Logisto.BusinessLogic
{
	public class DocumentLogic : IDocumentLogic
	{
		const string DOCUMENTS_ROOT = @"\\corpserv03.corp.local\Common\Carman\Documents\";
		const string TEMPLATED_DOCUMENTS_ROOT = @"\\corpserv03.corp.local\Common\Carman\TemplatedDocuments\";

		#region documents

		public int CreateDocument(Document document)
		{
			using (var db = new LogistoDb())
			{
				return Convert.ToInt32(db.InsertWithIdentity(document));
			}
		}

		public Document GetDocument(int documentId)
		{
			using (var db = new LogistoDb())
			{
				return db.Documents.FirstOrDefault(w => w.ID == documentId);
			}
		}

		public void UpdateDocument(Document document, int userId)
		{
			using (var db = new LogistoDb())
			{
				db.Documents.Where(w => w.ID == document.ID)
				.Set(u => u.UploadedBy, document.UploadedBy)
				.Set(u => u.UploadedDate, document.UploadedDate)
				.Set(u => u.Date, document.Date)
				.Set(u => u.DocumentTypeId, document.DocumentTypeId)
				.Set(u => u.Filename, document.Filename)
				.Set(u => u.FileSize, document.FileSize)
				.Set(u => u.IsPrint, document.IsPrint)
				.Set(u => u.IsNipVisible, document.IsNipVisible)
				.Set(u => u.Number, document.Number)
				.Set(u => u.OriginalSentDate, document.OriginalSentDate)
				.Set(u => u.OriginalSentUserId, document.OriginalSentUserId)
				.Set(u => u.OriginalReceivedDate, document.OriginalReceivedDate)
				.Set(u => u.OriginalReceivedUserId, document.OriginalReceivedUserId)
				.Set(u => u.ReceivedBy, document.ReceivedBy)
				.Set(u => u.ReceivedNumber, document.ReceivedNumber)
				.Set(u => u.IsWeekend, document.IsWeekend)
				.Set(u => u.WeekendMarkDate, document.WeekendMarkDate)
				.Set(u => u.WeekendMarkUserId, document.WeekendMarkUserId)
				.Update();

				#region запись в лог

				if (!document.ContractId.HasValue)
				{
					if (!document.AccountingId.HasValue || db.Accountings.First(w => w.ID == document.AccountingId.Value).IsIncome)   // только для доходов и документов заказа
					{
						int orderId = document.OrderId ?? db.Accountings.First(w => w.ID == document.AccountingId.Value).OrderId;
						var order = db.Orders.First(w => w.ID == orderId);
						var contract = db.Contracts.First(w => w.ID == order.ContractId);
						int contractorId = db.Legals.First(w => w.ID == contract.LegalId).ContractorId.Value;

						var entity = db.DocumentLog.FirstOrDefault(w => (w.DocumentId == document.ID) && (w.Action == "created"));
						if (entity == null)
							db.InsertWithIdentity(new DocumentLog { DocumentId = document.ID, DocumentDate = document.Date ?? DateTime.Now, DocumentNumber = document.Number, DocumentTypeId = document.DocumentTypeId ?? 0, Date = DateTime.Now, UserId = userId, OrderId = orderId, ContractorId = contractorId, Action = "created" });
						else
						{
							entity = db.DocumentLog.FirstOrDefault(w => (w.DocumentId == document.ID) && (w.Action == "updated") && (w.Date > DateTime.Today));
							if (entity == null)
								db.InsertWithIdentity(new DocumentLog { DocumentId = document.ID, DocumentDate = document.Date ?? DateTime.Now, DocumentNumber = document.Number, DocumentTypeId = document.DocumentTypeId ?? 0, Date = DateTime.Now, UserId = userId, OrderId = orderId, ContractorId = contractorId, Action = "updated" });
							else
								db.DocumentLog.Where(w => w.ID == entity.ID).Set(u => u.Date, DateTime.Now).Set(u => u.DocumentDate, document.Date).Set(u => u.DocumentNumber, document.Number).Set(u => u.DocumentTypeId, document.DocumentTypeId).Set(u => u.UserId, userId).Update();
						}
					}
				}

				#endregion
			}
		}

		public void DeleteDocument(int documentId, int userId)
		{
			using (var db = new LogistoDb())
			{
				#region запись в лог
				var document = db.Documents.First(w => w.ID == documentId);

				if (!document.ContractId.HasValue)
				{
					if (!document.AccountingId.HasValue || db.Accountings.First(w => w.ID == document.AccountingId.Value).IsIncome)   // только для доходов и документов заказа
					{
						int orderId = document.OrderId ?? db.Accountings.First(w => w.ID == document.AccountingId.Value).OrderId;
						var order = db.Orders.First(w => w.ID == orderId);
						var contract = db.Contracts.First(w => w.ID == order.ContractId);
						int contractorId = db.Legals.First(w => w.ID == contract.LegalId).ContractorId.Value;


						var entity = db.DocumentLog.FirstOrDefault(w => (w.DocumentId == document.ID) && (w.Action == "created"));
						if ((entity != null) && (entity.Date.Date == DateTime.Today))
							db.DocumentLog.Delete(w => w.DocumentId == documentId); // если документ был создан сегодня, уведомления клиенту не нужны
						else
							db.InsertWithIdentity(new DocumentLog { DocumentId = document.ID, DocumentDate = document.Date ?? DateTime.Now, DocumentNumber = document.Number ?? "", DocumentTypeId = document.DocumentTypeId ?? 0, Date = DateTime.Now, UserId = userId, OrderId = orderId, ContractorId = contractorId, Action = "deleted" });
					}
				}

				#endregion

				db.DocumentData.Delete(w => w.DocumentId == documentId);
				db.Documents.Delete(w => w.ID == documentId);
				// TODO: delete files
			}
		}


		public IEnumerable<Document> GetDocuments(ListFilter filter)
		{
			using (var db = new LogistoDb())
			{
				var query = db.Documents.AsQueryable();

				// условия поиска

				if (!string.IsNullOrWhiteSpace(filter.Context))
					query = query.Where(w => w.Number.Contains(filter.Context) ||
						w.ReceivedNumber.Contains(filter.Context)
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

		public IEnumerable<Document> GetDocumentsByAccounting(int accountingId)
		{
			using (var db = new LogistoDb())
			{
				return db.Documents.Where(w => w.AccountingId == accountingId).ToList();
			}
		}

		public IEnumerable<Document> GetDocumentsByOrder(int orderId)
		{
			using (var db = new LogistoDb())
			{
				return (from d in db.Documents
						from a in db.Accountings.Where(w => w.ID == d.AccountingId).DefaultIfEmpty()
						where a.OrderId == orderId || d.OrderId == orderId
						select d).ToList();
			}
		}

		public IEnumerable<Document> GetDocumentsByContract(int contractId)
		{
			using (var db = new LogistoDb())
			{
				return db.Documents.Where(w => w.ContractId == contractId).ToList();
			}
		}

		public int CreateDocumentData(DocumentData data)
		{
			using (var db = new LogistoDb())
			{
				var id = Convert.ToInt32(db.InsertWithIdentity(data));
				if (data.Data != null)
					File.WriteAllBytes(DOCUMENTS_ROOT + id, data.Data);

				return id;
			}
		}

		public DocumentData GetDocumentDataByDocument(int documentId)
		{
			using (var db = new LogistoDb())
			{
				var data = db.DocumentData.Where(w => w.DocumentId == documentId).Select(s => new DocumentData { ID = s.ID, DocumentId = s.DocumentId }).FirstOrDefault();
				try
				{
					if (data != null)
						data.Data = File.ReadAllBytes(DOCUMENTS_ROOT + data.ID);
				}
				catch
				{
				}

				return data;
			}
		}

		public void UpdateDocumentData(DocumentData data)
		{
			using (var db = new LogistoDb())
			{
				//db.DocumentData.Where(w => w.ID == data.ID)
				//.Set(u => u.Data, data.Data)
				//.Update();

				File.WriteAllBytes(DOCUMENTS_ROOT + data.ID, data.Data);
			}
		}


		public int CreateOrUpdateDocumentLog(DocumentLog data)
		{
			using (var db = new LogistoDb())
			{
				// TODO:
				var id = Convert.ToInt32(db.InsertWithIdentity(data));
				return id;
			}
		}

		#endregion

		#region Templated document

		public int CreateTemplatedDocument(TemplatedDocument file)
		{
			using (var db = new LogistoDb())
			{
				return Convert.ToInt32(db.InsertWithIdentity(file));
			}
		}

		public TemplatedDocument GetTemplatedDocument(int fileId)
		{
			using (var db = new LogistoDb())
			{
				return db.TemplatedDocuments.FirstOrDefault(w => w.ID == fileId);
			}
		}

		public void UpdateTemplatedDocument(TemplatedDocument document)
		{
			using (var db = new LogistoDb())
			{
				db.TemplatedDocuments.Where(w => w.ID == document.ID)
				.Set(u => u.ChangedBy, document.ChangedBy)
				.Set(u => u.ChangedDate, document.ChangedDate)
				.Set(u => u.Filename, document.Filename)
				.Set(u => u.OriginalSentDate, document.OriginalSentDate)
				.Set(u => u.OriginalSentUserId, document.OriginalSentUserId)
				.Set(u => u.OriginalReceivedDate, document.OriginalReceivedDate)
				.Set(u => u.OriginalReceivedUserId, document.OriginalReceivedUserId)
				.Set(u => u.ReceivedBy, document.ReceivedBy)
				.Set(u => u.ReceivedNumber, document.ReceivedNumber)
				.Update();
			}
		}

		public void DeleteTemplatedDocument(int id)
		{
			using (var db = new LogistoDb())
			{
				db.TemplatedDocumentData.Delete(w => w.TemplatedDocumentId == id);
				db.TemplatedDocuments.Delete(w => w.ID == id);
				// TODO: удалить файлы
			}
		}

		public IEnumerable<TemplatedDocument> GetTemplatedDocuments(ListFilter filter)
		{
			using (var db = new LogistoDb())
			{
				var query = db.TemplatedDocuments.AsQueryable();

				// условия поиска

				if (!string.IsNullOrWhiteSpace(filter.Context))
					query = query.Where(w => w.ReceivedNumber.Contains(filter.Context));

				// TODO: sort

				// пейджинг

				if (filter.PageNumber > 0)
					query = query.Skip(filter.PageNumber * filter.PageSize);

				if (filter.PageSize > 0)
					query = query.Take(filter.PageSize);

				return query.ToList();
			}
		}

		public IEnumerable<TemplatedDocument> GetTemplatedDocumentsByAccounting(int accountingId)
		{
			using (var db = new LogistoDb())
			{
				return db.TemplatedDocuments.Where(w => w.AccountingId == accountingId).ToList();
			}
		}

		public IEnumerable<TemplatedDocument> GetTemplatedDocumentsByOrder(int orderId)
		{
			using (var db = new LogistoDb())
			{
				return db.TemplatedDocuments.Where(w => w.OrderId == orderId).ToList();
			}
		}

		public IEnumerable<TemplatedDocument> GetAllTemplatedDocumentsByOrder(int orderId)
		{
			using (var db = new LogistoDb())
			{
				return (from d in db.TemplatedDocuments
						from a in db.Accountings.Where(w => w.ID == d.AccountingId).DefaultIfEmpty()
						where a.OrderId == orderId || d.OrderId == orderId
						select d).ToList();
			}
		}

		public void ClearTemplatedDocument(int id)
		{
			using (var db = new LogistoDb())
			{
				db.TemplatedDocumentData.Delete(w => w.TemplatedDocumentId == id);
				// TODO: delete files
			}
		}

		public int CreateTemplatedDocumentData(TemplatedDocumentData data)
		{
			using (var db = new LogistoDb())
			{
				var id = Convert.ToInt32(db.InsertWithIdentity(data));
				if (data.Data != null)
					File.WriteAllBytes(TEMPLATED_DOCUMENTS_ROOT + id, data.Data);

				return id;
			}
		}

		public TemplatedDocumentData GetTemplatedDocumentData(int id)
		{
			using (var db = new LogistoDb())
			{
				var data = db.TemplatedDocumentData.Where(w => w.ID == id).Select(s => new TemplatedDocumentData { ID = s.ID, TemplatedDocumentId = s.TemplatedDocumentId, Type = s.Type }).FirstOrDefault();
				try
				{
					if (data != null)
						data.Data = File.ReadAllBytes(TEMPLATED_DOCUMENTS_ROOT + data.ID);
				}
				catch
				{
				}

				return data;
			}
		}

		public void UpdateTemplatedDocumentData(TemplatedDocumentData data)
		{
			using (var db = new LogistoDb())
			{
				//db.TemplatedDocumentData.Where(w => w.ID == data.ID)
				//.Set(u => u.Data, data.Data)
				//.Update();

				File.WriteAllBytes(TEMPLATED_DOCUMENTS_ROOT + data.ID, data.Data);
			}
		}

		public TemplatedDocumentData GetTemplatedDocumentData(int documentId, string type)
		{
			using (var db = new LogistoDb())
			{
				var data = db.TemplatedDocumentData.Where(w => w.TemplatedDocumentId == documentId && w.Type == type).Select(s => new TemplatedDocumentData { ID = s.ID, TemplatedDocumentId = s.TemplatedDocumentId, Type = s.Type }).FirstOrDefault();
				try
				{
					if (data != null)
						data.Data = File.ReadAllBytes(TEMPLATED_DOCUMENTS_ROOT + data.ID);
				}
				catch
				{
				}

				return data;
			}
		}

		#endregion

		public int GetDeclarationDocumentsCount(ListFilter filter)
		{
			using (var db = new LogistoDb())
			{
				var query = from d in db.Documents
							from a in db.Accountings.Where(w => w.ID == d.AccountingId).DefaultIfEmpty()
							from o in db.Orders.Where(w => w.ID == a.OrderId).DefaultIfEmpty()
							from c in db.Contracts.Where(w => w.ID == o.ContractId).DefaultIfEmpty()
							from l in db.Legals.Where(w => w.ID == c.LegalId).DefaultIfEmpty()
							from o2 in db.Orders.Where(w => w.ID == d.OrderId).DefaultIfEmpty()
							from w in db.Participants.Where(w => w.OrderId == o.ID).DefaultIfEmpty()
							from w2 in db.Participants.Where(w => w.OrderId == o2.ID).DefaultIfEmpty()
							where (w.UserId == filter.UserId
									|| w2.UserId == filter.UserId)
								&& d.DocumentTypeId == 20    // ДТ
							select new { d, l };

				// условия поиска

				if (!string.IsNullOrWhiteSpace(filter.Context))
					query = query.Where(w => w.d.Number.Contains(filter.Context) ||
						w.l.DisplayName.Contains(filter.Context)
						);

				query = query.GroupBy(g => g.d.Number).Select(s => s.First());

				return query.Count();
			}
		}

		public IEnumerable<Document> GetDeclarationDocuments(ListFilter filter)
		{
			using (var db = new LogistoDb())
			{
				var query = from d in db.Documents
							from a in db.Accountings.Where(w => w.ID == d.AccountingId).DefaultIfEmpty()
							from o in db.Orders.Where(w => w.ID == a.OrderId).DefaultIfEmpty()
							from c in db.Contracts.Where(w => w.ID == o.ContractId).DefaultIfEmpty()
							from l in db.Legals.Where(w => w.ID == c.LegalId).DefaultIfEmpty()
							from o2 in db.Orders.Where(w => w.ID == d.OrderId).DefaultIfEmpty()
							from w in db.Participants.Where(w => w.OrderId == o.ID).DefaultIfEmpty()
							from w2 in db.Participants.Where(w => w.OrderId == o2.ID).DefaultIfEmpty()
							where (w.UserId == filter.UserId
									|| w2.UserId == filter.UserId)
								&& d.DocumentTypeId == 20    // ДТ
							select new { d, l };

				// условия поиска

				if (!string.IsNullOrWhiteSpace(filter.Context))
					query = query.Where(w => w.d.Number.Contains(filter.Context) ||
						w.l.DisplayName.Contains(filter.Context)
						);


				var list = query.Distinct().Select(s => new ViewModels.DeclarationViewModel { DocumentId = s.d.ID, DeclarationNumber = s.d.Number, IsWeekend = s.d.IsWeekend, WeekendMarkDate = s.d.WeekendMarkDate, WeekendMarkUserId = s.d.WeekendMarkUserId }).ToList();
				// fill
				foreach (var item in list)
				{
					if (item.WeekendMarkUserId.HasValue)
						item.WeekendMarkUser = (from p in db.Persons join u in db.Users on p.ID equals u.PersonId where u.ID == item.WeekendMarkUserId select p.DisplayName).FirstOrDefault();

					var doc = db.Documents.First(w => w.ID == item.DocumentId);

					Order order = null;
					if (doc.OrderId.HasValue)
						order = db.Orders.First(w => w.ID == doc.OrderId.Value);

					if (doc.AccountingId.HasValue)
						order = db.Orders.First(w => w.ID == db.Accountings.First(wa => wa.ID == doc.AccountingId.Value).OrderId);

					var contract = db.Contracts.First(w => w.ID == order.ContractId);
					var legal = db.Legals.First(w => w.ID == contract.LegalId);

					item.ContractId = contract.ID;
					item.ContractNumber = contract.Number;
					item.ContractType = db.ContractTypes.Where(w => w.ID == contract.ContractTypeId).Select(s => s.Display).First();
					item.LegalName = legal.DisplayName;
					item.MotivationDate = order.ClosedDate;
					item.OrderId = order.ID;
					item.OrderNumber = order.Number;
					item.OrderStatus = db.OrderStatuses.Where(w => w.ID == order.OrderStatusId).Select(s => s.Display).First();
				}

				// сортировка
				if (!string.IsNullOrWhiteSpace(filter.Sort))
				{
					if (string.IsNullOrWhiteSpace(filter.SortDirection))
						filter.SortDirection = "Asc";

					switch (filter.Sort)
					{
						case "OrderNumber":
							if (filter.SortDirection == "Asc")
								list = list.OrderBy(o => o.OrderNumber).ToList();
							else
								list = list.OrderByDescending(o => o.OrderNumber).ToList();

							break;

						case "OrderStatus":
							if (filter.SortDirection == "Asc")
								list = list.OrderBy(o => o.OrderStatus).ToList();
							else
								list = list.OrderByDescending(o => o.OrderStatus).ToList();

							break;

						case "MotivationDate":
							if (filter.SortDirection == "Asc")
								list = list.OrderBy(o => o.MotivationDate).ToList();
							else
								list = list.OrderByDescending(o => o.MotivationDate).ToList();

							break;

						case "WeekendMarkUser":
							if (filter.SortDirection == "Asc")
								list = list.OrderBy(o => o.WeekendMarkUser).ToList();
							else
								list = list.OrderByDescending(o => o.WeekendMarkUser).ToList();

							break;

						case "LegalName":
							if (filter.SortDirection == "Asc")
								list = list.OrderBy(o => o.LegalName).ToList();
							else
								list = list.OrderByDescending(o => o.LegalName).ToList();

							break;

						case "ContractNumber":
							if (filter.SortDirection == "Asc")
								list = list.OrderBy(o => o.ContractNumber).ToList();
							else
								list = list.OrderByDescending(o => o.ContractNumber).ToList();

							break;

						case "ContractType":
							if (filter.SortDirection == "Asc")
								list = list.OrderBy(o => o.ContractType).ToList();
							else
								list = list.OrderByDescending(o => o.ContractType).ToList();

							break;

						case "DeclarationNumber":
							if (filter.SortDirection == "Asc")
								list = list.OrderBy(o => o.DeclarationNumber).ToList();
							else
								list = list.OrderByDescending(o => o.DeclarationNumber).ToList();

							break;

						case "IsWeekend":
							if (filter.SortDirection == "Asc")
								list = list.OrderBy(o => o.IsWeekend).ToList();
							else
								list = list.OrderByDescending(o => o.IsWeekend).ToList();

							break;

						case "WeekendMarkDate":
							if (filter.SortDirection == "Asc")
								list = list.OrderBy(o => o.WeekendMarkDate).ToList();
							else
								list = list.OrderByDescending(o => o.WeekendMarkDate).ToList();

							break;
					}
				}

				// пейджинг

				if (filter.PageNumber > 0)
					list = list.Skip(filter.PageNumber * filter.PageSize).ToList();

				if (filter.PageSize > 0)
					list = list.Take(filter.PageSize).ToList();

				var ids = list.Select(s => s.DocumentId).ToList();

				var result = new List<Document>(ids.Count);
				foreach (var id in ids)
					result.Add(db.Documents.First(w => w.ID == id));

				return result;
			}
		}

		//public IEnumerable<Document> GetDeclarationDocuments(ListFilter filter)
		//{
		//	using (var db = new LogistoDb())
		//	{
		//		var query = from d in db.Documents
		//					from a in db.Accountings.Where(w => w.ID == d.OrderAccountingId).DefaultIfEmpty()
		//					from o in db.Orders.Where(w => w.ID == a.OrderId).DefaultIfEmpty()
		//					from c in db.Contracts.Where(w => w.ID == o.ContractId).DefaultIfEmpty()
		//					from l in db.Legals.Where(w => w.ID == c.LegalId).DefaultIfEmpty()
		//					from o2 in db.Orders.Where(w => w.ID == d.OrderId).DefaultIfEmpty()
		//					from w in db.Participants.Where(w => w.OrderId == o.ID).DefaultIfEmpty()
		//					from w2 in db.Participants.Where(w => w.OrderId == o2.ID).DefaultIfEmpty()
		//					where (w.UserId == filter.UserId
		//							|| w2.UserId == filter.UserId)
		//						&& d.DocumentTypeId == 20    // ДТ
		//					select new { d, l,o };

		//		// условия поиска

		//		if (!string.IsNullOrWhiteSpace(filter.Context))
		//			query = query.Where(w => w.d.Number.Contains(filter.Context) ||
		//				w.l.DisplayName.Contains(filter.Context)
		//				);

		//		//query = query.GroupBy(g => g.d.Number).Select(s => s.First());

		//		query = query.Distinct();

		//		// сортировка
		//		if (!string.IsNullOrWhiteSpace(filter.Sort))
		//		{
		//			if (string.IsNullOrWhiteSpace(filter.SortDirection))
		//				filter.SortDirection = "Asc";

		//			switch (filter.Sort)
		//			{
		//				case "OrderNumber":
		//					if (filter.SortDirection == "Asc")
		//						query = query.OrderBy(o => o.o.Number);
		//					else
		//						query = query.OrderByDescending(o => o.o.Number);

		//					break;

		//				case "OrderStatus":
		//					if (filter.SortDirection == "Asc")
		//						query = query.OrderBy(o => o.o.OrderStatusId);
		//					else
		//						query = query.OrderByDescending(o => o.o.OrderStatusId);

		//					break;

		//				case "MotivationDate":
		//					if (filter.SortDirection == "Asc")
		//						query = query.OrderBy(o => o.o.ClosedDate);
		//					else
		//						query = query.OrderByDescending(o => o.o.ClosedDate);

		//					break;

		//				case "Balance":
		//					if (filter.SortDirection == "Asc")
		//						query = query.OrderBy(o => o.Balance);
		//					else
		//						query = query.OrderByDescending(o => o.Balance);

		//					break;
		//			}
		//		}
		//		// пейджинг

		//		if (filter.PageNumber > 0)
		//			query = query.Skip(filter.PageNumber * filter.PageSize);

		//		if (filter.PageSize > 0)
		//			query = query.Take(filter.PageSize);

		//		return query.Select(s => s.d).ToList();
		//	}
		//}
	}
}