using System;

namespace Logisto.Models
{
	/// <summary>
	/// Роль участника рабочей группы. На основе справочника из БД, актуальные данные там!
	/// </summary>
	public enum ParticipantRoles
	{
		SM = 1, //	Applicant
		OPER = 2,
		AM = 3, // Order manager
		BUH = 4,
		SL = 5,
		GM = 6  // General Manager	
	}

	/// <summary>
	/// На основе справочника из БД, актуальные данные там!
	/// </summary>
	public enum AccountingDirection
	{
		Income = 1,
		Expense = 2,
	}

	public enum TemplatedDocumentFormat
	{
		Pdf,
		CleanPdf,
		CutPdf
	}

	public enum OrderStatuses
	{
		Rejected = 1,
		Created = 2,
		InProgress = 3,
		Completed = 4,
		CostsEntered = 5,
		Checked = 6,
		Motivation = 7,
		Correcting = 8,
		Closed = 9
	}
}
