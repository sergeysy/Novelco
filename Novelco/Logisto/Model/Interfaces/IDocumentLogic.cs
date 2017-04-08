using System.Collections.Generic;
using Logisto.Models;

namespace Logisto.BusinessLogic
{
	public interface IDocumentLogic
	{
		#region documents

		int CreateDocument(Document document);
		Document GetDocument(int documentId);
		void UpdateDocument(Document document, int userId);
		void DeleteDocument(int documentId, int userId);

		IEnumerable<Document> GetDocuments(ListFilter filter);

		/// <summary>
		/// Получить список документов для записи учета по доход/расходу
		/// </summary>
		IEnumerable<Document> GetDocumentsByAccounting(int accontingId);

		/// <summary>
		/// Получить список документов для заказа
		/// </summary>
		IEnumerable<Document> GetDocumentsByOrder(int orderId);

		/// <summary>
		/// Получить список документов для договора
		/// </summary>
		IEnumerable<Document> GetDocumentsByContract(int contractId);
		
		int CreateDocumentData(DocumentData documentData);
		DocumentData GetDocumentDataByDocument(int documentId);
		void UpdateDocumentData(DocumentData documentData);

		int CreateOrUpdateDocumentLog(DocumentLog entity);

		#endregion

		#region Templated Document

		int CreateTemplatedDocument(TemplatedDocument entity);
		TemplatedDocument GetTemplatedDocument(int id);
		void UpdateTemplatedDocument(TemplatedDocument entity);
		void DeleteTemplatedDocument(int id);

		IEnumerable<TemplatedDocument> GetTemplatedDocuments(ListFilter filter);

		/// <summary>
		/// Получить документы для доход/расхода
		/// </summary>
		IEnumerable<TemplatedDocument> GetTemplatedDocumentsByAccounting(int accountingId);

		/// <summary>
		/// Получить документы по номеру заказа
		/// </summary>
		IEnumerable<TemplatedDocument> GetTemplatedDocumentsByOrder(int orderId);

		/// <summary>
		/// Получить все документы по номеру заказа, включая доход/расходы
		/// </summary>
		IEnumerable<TemplatedDocument> GetAllTemplatedDocumentsByOrder(int orderId);

		/// <summary>
		/// Удалить данные (TemplatedDocumentData) для этого документа
		/// </summary>
		void ClearTemplatedDocument(int id);

		#endregion

		// templated document data
		int CreateTemplatedDocumentData(TemplatedDocumentData data);
		TemplatedDocumentData GetTemplatedDocumentData(int id);
		void UpdateTemplatedDocumentData(TemplatedDocumentData data);
		TemplatedDocumentData GetTemplatedDocumentData(int documentId, string type);

		int GetDeclarationDocumentsCount(ListFilter filter);
		IEnumerable<Document> GetDeclarationDocuments(ListFilter filter);

	}
}