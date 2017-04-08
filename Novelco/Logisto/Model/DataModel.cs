using System;
using System.ComponentModel.DataAnnotations.Schema;
using LinqToDB.Mapping;
using Microsoft.AspNet.Identity;
using Column = LinqToDB.Mapping.ColumnAttribute;
using Table = LinqToDB.Mapping.TableAttribute;

namespace Logisto.Models
{
	#region Identity

	//[Table("Users")]
	[Table("tblUsers")]
	public partial class IdentityUser : IUser<int>
	{
		[PrimaryKey]
		[Identity]
		[Column("IdUsr")]
		public int Id { get; set; }
		[Column("usrLogin")]
		public string Login { get; set; }
		[Column("Name")]
		public string Name { get; set; }
		[Column("Email")]
		public string Email { get; set; }
		[Column("usrFLId")]
		public int PersonId { get; set; }
		[Column]
		public int? CrmId { get; set; }
		[Column]
		public byte[] Photo { get; set; }

		[Column("usrLogin", SkipOnUpdate = true, SkipOnInsert = true)]
		public string UserName { get; set; }
	}

	[Table("IdentityRoles")]
	public partial class IdentityRole : IRole<int>
	{
		[PrimaryKey]
		[Identity]
		[Column("Id")]
		public int Id { get; set; }
		[Column("Name")]
		public string Name { get; set; }
		[Column("Description")]
		public string Description { get; set; }
	}

	[Table("IdentityUsersInRoles")]
	public partial class IdentityUserInRole
	{
		[Column("UserId")]
		public int UserId { get; set; }
		[Column("RoleId")]
		public int RoleId { get; set; }
	}

	[Table("IdentityAccounts")]
	public partial class IdentityAccount
	{
		[PrimaryKey]
		//[Identity]
		[Column("UserId")]
		public int UserId { get; set; }
		[Column("Password")]
		public string Password { get; set; }
		[Column("IsApproved")]
		public bool IsApproved { get; set; }
		[Column("IsBlocked")]
		public bool IsBlocked { get; set; }
		[Column("CreatedDate")]
		public DateTime CreatedDate { get; set; }
		[Column("FailedPasswordAttemptCount")]
		public int FailedPasswordAttemptCount { get; set; }
		[Column("ResetToken")]
		public string ResetToken { get; set; }
	}

	#endregion

	#region Справочники

	[Table("EmployeeStatuses")]
	public partial class EmployeeStatus
	{
		[PrimaryKey]
		[Identity]
		[Column]
		public int ID { get; set; }
		[Column]
		public string Name { get; set; }
	}

	[Table("ParticipantRoles")]
	public partial class ParticipantRole
	{
		[PrimaryKey]
		[Identity]
		[Column]
		public int ID { get; set; }
		[Column]
		public string Name { get; set; }
		[Column]
		public string Description { get; set; }
	}

	[Table("FinRepCenters")]
	public partial class FinRepCenter
	{
		[PrimaryKey]
		[Identity]
		[Column]
		public int ID { get; set; }
		[Column]
		public string Name { get; set; }
		[Column]
		public string Code { get; set; }
		[Column]
		public int OurLegalId { get; set; }
		[Column]
		public string Description { get; set; }
	}

	/// <summary>
	/// Справочник статуса заказа
	/// </summary>
	[Table("tblZakazStatus")]
	public partial class OrderStatus
	{
		[PrimaryKey]
		[Identity]
		[Column("IdZKS")]
		public int ID { get; set; }
		[Column("zksStatus")]
		public string Display { get; set; }
	}

	/// <summary>
	/// Справочник типа заказа
	/// </summary>
	[Table("tblZakazType")]
	public partial class OrderType
	{
		[PrimaryKey]
		[Identity]
		[Column("IdZKT")]
		public int ID { get; set; }
		[Column("zktType")]
		public string Display { get; set; }
	}

	/// <summary>
	/// Тип страхования груза
	/// </summary>
	[Table("tblZakazStrahType")]
	public partial class InsuranceType
	{
		[PrimaryKey]
		[Identity]
		[Column("IdZST")]
		public int ID { get; set; }
		[Column("zstName")]
		public string Display { get; set; }
	}

	/// <summary>
	/// Причины отсутствия страховки
	/// </summary>
	[Table("tblZakazBezStrah")]
	public partial class UninsuranceType
	{
		[PrimaryKey]
		[Identity]
		[Column("IdZBS")]
		public int ID { get; set; }
		[Column("zbsName")]
		public string Display { get; set; }
	}

	/// <summary>
	/// Валюта
	/// </summary>
	[Table("tblCurrency")]
	public partial class Currency
	{
		[PrimaryKey]
		[Identity]
		[Column("IdCur")]
		public int ID { get; set; }
		[Column("curNameRU")]
		public string Name { get; set; }
		[Column("curNameEN")]
		public string EnName { get; set; }
		[Column("curCode")]
		public string Display { get; set; }
		[Column("curNum")]
		public string Code { get; set; }
	}

	/// <summary>
	/// Коэффициент объемного веса
	/// </summary>
	[Table("tblZakazGruzKoef")]
	public partial class VolumetricRatio
	{
		[PrimaryKey]
		[Identity]
		[Column("IdZGK")]
		public int ID { get; set; }
		[Column("zgkName")]
		public string Display { get; set; }
		[Column("zgkValue")]
		public double Value { get; set; }
	}

	/// <summary>
	/// Тип упаковки (груза)
	/// </summary>
	[Table("tblZakazGruzUpakovkaType")]
	public partial class PackageType
	{
		[PrimaryKey]
		[Identity]
		[Column("IdZGU")]
		public int ID { get; set; }
		[Column("zguDescriptionRU")]
		public string Display { get; set; }
		[Column("zguDescriptionENG")]
		public string EnDisplay { get; set; }
	}

	/// <summary>
	/// Описание груза
	/// </summary>
	[Table("tblZakazGruzDescription")]
	public partial class CargoDescription
	{
		[PrimaryKey]
		[Identity]
		[Column("IdZGO")]
		public int ID { get; set; }
		[Column("zgoDescriptionRU")]
		public string Display { get; set; }
		[Column("zgoDescriptionENG")]
		public string EnDisplay { get; set; }
	}

	/// <summary>
	/// Право подписи
	/// </summary>
	[Table("tblPravoPodpisi")]
	public partial class SigningAuthority
	{
		[PrimaryKey]
		[Identity]
		[Column("IdPPD")]
		public int ID { get; set; }
		[Column("ppdName")]
		public string Display { get; set; }
	}

	/// <summary>
	/// Тип телефонного номера
	/// </summary>
	[Table("tblPhoneTypes")]
	public partial class PhoneType
	{
		[PrimaryKey]
		[Identity]
		[Column("IdPTP")]
		public int ID { get; set; }
		[Column("ptpName")]
		public string Display { get; set; }
	}

	/// <summary>
	/// Тип договора
	/// </summary>
	[Table("tblDogType")]
	public partial class ContractType
	{
		[PrimaryKey]
		[Identity]
		[Column("IdDGT")]
		public int ID { get; set; }
		[Column("dgtName")]
		public string Display { get; set; }
		[Column("dgtOurSide")]
		public int? OurContractRoleId { get; set; }
		[Column("dgtKntSide")]
		public int? ContractRoleId { get; set; }
	}

	/// <summary>
	/// Вид договора
	/// </summary>
	[Table("tblDogVid")]
	public partial class ContractServiceType
	{
		[PrimaryKey]
		[Identity]
		[Column("IdDGV")]
		public int ID { get; set; }
		[Column("dgvDogVid")]
		public string Display { get; set; }
	}

	/// <summary>
	/// Налоговая форма
	/// </summary>
	[Table("tblULnalogForm")]
	public partial class TaxType
	{
		[PrimaryKey]
		[Identity]
		[Column("IdUNF")]
		public int ID { get; set; }
		[Column("unfName")]
		public string Display { get; set; }
	}

	/// <summary>
	/// Тип доход/расхода
	/// </summary>
	[Table("tblZakazEconomDocType")]
	public partial class AccountingDocumentType
	{
		[PrimaryKey]
		[Identity]
		[Column("IdEDT")]
		public int ID { get; set; }
		[Column("edtName")]
		public string Display { get; set; }
		[Column]
		public string EnName { get; set; }
	}


	/// <summary>
	/// Роль пользователя
	/// </summary>
	[Table("tblRoles")]
	public partial class Role
	{
		[PrimaryKey]
		[Identity]
		[Column("IdROL")]
		public int ID { get; set; }
		[Column("rolName")]
		public string Display { get; set; }
		[Column("rolCode")]
		public string Code { get; set; }
		[Column("rolNameEng")]
		public string EnName { get; set; }
		[Column("rolComm")]
		public string rolComm { get; set; }
	}

	/// <summary>
	/// Шаблон должности
	/// </summary>
	[Table("tblULtypicalPositionSet")]
	public partial class PositionTemplate
	{
		[PrimaryKey]
		[Identity]
		[Column("IdTPS")]
		public int ID { get; set; }
		[Column("tpsDoljnost")]
		public string Position { get; set; }
		[Column("tpsDoljnosnRP")]
		public string GenitivePosition { get; set; }
		[Column("tpsDoljnostEN")]
		public string EnPosition { get; set; }
		[Column("tpsDeystvuetNa")]
		public string Basis { get; set; }
		[Column("tpsDeystvuetNaEN")]
		public string EnBasis { get; set; }
		[Column]
		public string Department { get; set; }
	}

	/// <summary>
	/// Условия оплаты
	/// </summary>
	[Table("tblDogovorOplata")]
	public partial class PaymentTerm
	{
		[PrimaryKey]
		[Identity]
		[Column("IdDGO")]
		public int ID { get; set; }
		[Column("dgoOplataType")]
		public string Display { get; set; }

		[Column]
		public byte Condition1_Percent { get; set; }
		[Column]
		public int? Condition1_From { get; set; }
		[Column]
		public byte Condition1_Days { get; set; }
		[Column]
		public bool Condition1_BankDays { get; set; }
		[Column]
		public int? Condition1_OrdersFrom { get; set; }
		[Column]
		public int? Condition1_OrdersTo { get; set; }

		[Column]
		public byte Condition2_Percent { get; set; }
		[Column]
		public int? Condition2_From { get; set; }
		[Column]
		public byte Condition2_Days { get; set; }
		[Column]
		public bool Condition2_BankDays { get; set; }
		[Column]
		public int? Condition2_OrdersFrom { get; set; }
		[Column]
		public int? Condition2_OrdersTo { get; set; }
	}

	/// <summary>
	/// Роль стороны в договоре
	/// </summary>
	[Table("tblKontragentUrType")]
	public partial class ContractRole
	{
		[PrimaryKey]
		[Identity]
		[Column("IdKUT")]
		public int ID { get; set; }
		[Column("kutName")]
		public string Display { get; set; }
		[Column("kutNameTvorP")]
		public string AblativeName { get; set; }
		[Column("kutNameDatP")]
		public string DativeName { get; set; }
		[Column("kutNameENG")]
		public string EnName { get; set; }
	}

	/// <summary>
	/// Продукт-услуга предоставляемая контрагенту
	/// </summary>
	[Table("tblZakazProduct")]
	public partial class Product
	{
		[PrimaryKey]
		[Identity]
		[Column("IdZKP")]
		public int ID { get; set; }
		[Column("zkpProduct")]
		public string Display { get; set; }
		[Column("zkpIsWorking")]
		public bool? IsWorking { get; set; }
		[Column]
		public int? VolumetricRatioId { get; set; }
		[Column]
		public int? ManagerUserId { get; set; }
		[Column]
		public int? DeputyUserId { get; set; }
	}

	[Table("OrderTemplates")]
	public partial class OrderTemplate
	{
		[PrimaryKey]
		[Identity]
		[Column]
		public int ID { get; set; }
		[Column]
		public int? ProductId { get; set; }
		[Column]
		public int? ContractId { get; set; }
		[Column]
		public string Name { get; set; }
	}


	/// <summary>
	/// Тип точки маршрута
	/// </summary>
	[Table("tblZakazMarshrutPunktType")]
	public partial class RoutePointType
	{
		[PrimaryKey]
		[Identity]
		[Column("IdMPT")]
		public int ID { get; set; }
		[Column("mptName")]
		public string Display { get; set; }
		[Column("mptNameEN")]
		public string EnDisplay { get; set; }
	}

	/// <summary>
	/// Тип документа
	/// </summary>
	[Table("tblZakazEconomDocDocTypes")]
	public partial class DocumentType
	{
		[PrimaryKey]
		[Identity]
		[Column("IdDCT")]
		public int ID { get; set; }
		[Column("dctNаme")]
		public string Display { get; set; }
		[Column]
		public string Description { get; set; }
		[Column]
		public string EnDescription { get; set; }
		[Column]
		public bool IsNipVisible { get; set; }
	}

	[Table("DocumentTypeProductPrints")]
	public partial class DocumentTypeProductPrint
	{
		[PrimaryKey]
		[Identity]
		[Column]
		public int ID { get; set; }
		[Column]
		public int DocumentTypeId { get; set; }
		[Column]
		public int ProductId { get; set; }
	}

	/// <summary>
	/// Вид оплаты
	/// </summary>
	[Table("tblZakazEconomOplataType")]
	public partial class AccountingPaymentType
	{
		[PrimaryKey]
		[Identity]
		[Column("IdZOT")]
		public int ID { get; set; }
		[Column("zotName")]
		public string Display { get; set; }
		[Column]
		public string EnName { get; set; }
	}

	/// <summary>
	/// Метод оплаты (нал/безнал)
	/// </summary>
	[Table("tblZakazEconomOplataMetod")]
	public partial class AccountingPaymentMethod
	{
		[PrimaryKey]
		[Identity]
		[Column("IdEOM")]
		public int ID { get; set; }
		[Column("eomName")]
		public string Display { get; set; }
		[Column]
		public string EnName { get; set; }
	}

	/// <summary>
	/// Вид транспорта
	/// </summary>
	[Table("tblZakazMarshrutTransport")]
	public partial class TransportType
	{
		[PrimaryKey]
		[Identity]
		[Column("IdTRS")]
		public int ID { get; set; }
		[Column("trsNameRU")]
		public string Display { get; set; }
		[Column("trsNameEN")]
		public string EnDisplay { get; set; }
	}

	/// <summary>
	/// Единицы измерения
	/// </summary>
	[Table("tblOKEI")]
	public partial class Measure
	{
		[PrimaryKey]
		[Identity]
		[Column("IdOKE")]
		public int ID { get; set; }
		[Column("okeKod")]
		public int? Code { get; set; }
		[Column("okeName")]
		public string Name { get; set; }
		[Column("okeShortRU")]
		public string Display { get; set; }
		[Column("okeShortEN")]
		public string EnDisplay { get; set; }
	}

	/// <summary>
	/// Типы НДС
	/// </summary>
	[Table("tblVAT")]
	public partial class VAT
	{
		[PrimaryKey]
		[Identity]
		[Column("IdVAT")]
		public int ID { get; set; }
		[Column("vatName")]
		public string Display { get; set; }
		[Column("vatPercent")]
		public int Percent { get; set; }
	}

	/// <summary>
	/// Применимость курса валюты
	/// </summary>
	[Table("CurrencyRateUses")]
	public partial class CurrencyRateUse
	{
		[PrimaryKey]
		[Identity]
		[Column("Id")]
		public int ID { get; set; }
		[Column("Display")]
		public string Display { get; set; }
		[Column("Value")]
		public float Value { get; set; }
		[Column("IsDocumentDate")]
		public bool IsDocumentDate { get; set; }
	}

	[Table]
	public partial class CurrencyRateDiff
	{
		[PrimaryKey]
		[Identity]
		[Column]
		public int ID { get; set; }
		[Column]
		public DateTime From { get; set; }
		[Column]
		public DateTime To { get; set; }
		[Column]
		public float USD { get; set; }
		[Column]
		public float GBP { get; set; }
		[Column]
		public float EUR { get; set; }
		[Column]
		public float CNY { get; set; }
	}

	[Table("Operations")]
	public partial class Operation
	{
		[PrimaryKey]
		[Identity]
		[Column]
		public int ID { get; set; }
		[Column]
		public int OrderId { get; set; }
		[Column]
		public int? OperationStatusId { get; set; }
		[Column]
		public int OrderOperationId { get; set; }
		[Column]
		public int? ResponsibleUserId { get; set; }
		[Column]
		public int No { get; set; }
		[Column]
		public string Name { get; set; }
		[Column]
		public DateTime? StartPlanDate { get; set; }
		[Column]
		public DateTime? FinishPlanDate { get; set; }
		[Column]
		public DateTime? StartFactDate { get; set; }
		[Column]
		public DateTime? FinishFactDate { get; set; }
	}

	[Table("OperationKinds")]
	public partial class OperationKind
	{
		[PrimaryKey]
		[Identity]
		[Column]
		public int ID { get; set; }
		[Column]
		public string Name { get; set; }
		[Column]
		public string EnName { get; set; }
	}

	[Table("OrderOperations")]
	public partial class OrderOperation
	{
		[PrimaryKey]
		[Identity]
		[Column]
		public int ID { get; set; }
		[Column]
		public string Name { get; set; }
		[Column]
		public string EnName { get; set; }
		[Column]
		public int? OperationKindId { get; set; }
		[Column]
		public int? StartFactEventId { get; set; }
		[Column]
		public int? FinishFactEventId { get; set; }
	}

	[Table("OperationStatuses")]
	public partial class OperationStatus
	{
		[PrimaryKey]
		[Identity]
		[Column]
		public int ID { get; set; }
		[Column]
		public string Name { get; set; }
	}

	[Table("Events")]
	public partial class Event
	{
		[PrimaryKey]
		[Identity]
		[Column]
		public int ID { get; set; }
		[Column]
		public string Name { get; set; }
		[Column]
		public string EnName { get; set; }
		[Column]
		public bool IsExternal { get; set; }
	}

	[Table("OrderEvents")]
	public partial class OrderEvent
	{
		[PrimaryKey]
		[Identity]
		[Column]
		public int ID { get; set; }
		[Column]
		public int OrderId { get; set; }
		[Column]
		public int EventId { get; set; }
		[Column]
		public string City { get; set; }
		[Column]
		public string Comment { get; set; }
		[Column]
		public DateTime Date { get; set; }
		[Column]
		public DateTime CreatedDate { get; set; }
		[Column]
		public bool IsExternal { get; set; }
	}

	/// <summary>
	/// Вид услуги (родитель для ServiceType)
	/// </summary>
	[Table("tblZakazEconomUslugaMain")]
	public partial class ServiceKind
	{
		[PrimaryKey]
		[Identity]
		[Column("IdEUM")]
		public int ID { get; set; }
		[Column("eumProductId")]
		public int ProductId { get; set; }
		[Column("eumName")]
		public string Name { get; set; }
		[Column("eumNameEN")]
		public string EnName { get; set; }
		[Column("eumVatID")]
		public int? VatId { get; set; }
	}

	/// <summary>
	/// Тип конкретной услуги
	/// </summary>
	[Table("tblZakazEconomUslugaList")]
	public partial class ServiceType
	{
		[PrimaryKey]
		[Identity]
		[Column("IdZEU")]
		public int ID { get; set; }
		[Column("zeuUslugaBase")]
		public int ServiceKindId { get; set; }
		[Column("zeuOKEIid")]
		public int? MeasureId { get; set; }
		[Column("zeuUslugaName")]
		public string Name { get; set; }
		[Column]
		public string EnName { get; set; }
		[Column("zeuKolvo")]
		public int? Count { get; set; }
		[Column("zeuPrice")]
		public double? Price { get; set; }
	}

	#endregion

	#region Сущности

	/// <summary>
	/// Банк
	/// </summary>
	[Table("Banks")]
	public partial class Bank
	{
		[PrimaryKey]
		[Identity]
		[Column("IdBnk")]
		public int ID { get; set; }
		[Column("NAMEP")]
		public string Name { get; set; }
		[Column("NEWNUM")]
		public string BIC { get; set; }
		[Column("PZN")]
		public int? PZN { get; set; }
		[Column("UER")]
		public int? UER { get; set; }
		[Column("TNP")]
		public int? TNP { get; set; }
		[Column("NNP")]
		public string NNP { get; set; }
		[Column("KSNP")]
		public string KSNP { get; set; }
		[Column("SWIFT")]
		public string SWIFT { get; set; }
		[Column]
		public string EnName { get; set; }
		[Column]
		public string Address { get; set; }
		[Column]
		public string EnAddress { get; set; }
	}

	/// <summary>
	/// Банковский счет
	/// </summary>
	[Table("tblAccounts")]
	public partial class BankAccount
	{
		[PrimaryKey]
		[Identity]
		[Column("IdAcc")]
		public int ID { get; set; }
		[Column("accUlId")]
		public int? LegalId { get; set; }
		[Column("accNumber")]
		public string Number { get; set; }
		[Column("accCurrencyId")]
		public int? CurrencyId { get; set; }
		[Column("accBankId")]
		public int? BankId { get; set; }
		[Column("accKorrBank")]
		public string CoBankName { get; set; }
		[Column("accKorrAccount")]
		public string CoBankAccount { get; set; }
		[Column("accKorrSWIFT")]
		public string CoBankSWIFT { get; set; }
		[Column]
		public string CoBankIBAN { get; set; }
		[Column("Address")]
		public string CoBankAddress { get; set; }
	}

	/// <summary>
	/// Пользователь
	/// </summary>
	[Table("tblUsers")]
	public partial class User
	{
		[PrimaryKey]
		[Identity]
		[Column("IdUsr")]
		public int ID { get; set; }
		[Column("usrFLId")]
		public int? PersonId { get; set; }
		[Column("usrRolId")]
		public int? RoleId { get; set; }
		[Column("usrLogin")]
		public string Login { get; set; }
		[Column("usrPass")]
		public string Pass { get; set; }
		[Column]
		public int? CrmId { get; set; }
		[Column]
		public byte[] Photo { get; set; }
	}

	[Table("UserSettings")]
	public partial class UserSetting
	{
		[PrimaryKey]
		[Identity]
		[Column]
		public int ID { get; set; }
		[Column]
		public int UserId { get; set; }
		[Column]
		public string Key { get; set; }
		[Column]
		public string Value { get; set; }
	}

	/// <summary>
	/// Договор
	/// </summary>
	[Table("Contracts")]
	public partial class Contract
	{
		[PrimaryKey]
		[Identity]
		[Column("IdDOG")]
		public int ID { get; set; }
		[Column("dogClientId")]
		public int LegalId { get; set; }
		[Column("dogOurUlId")]
		public int OurLegalId { get; set; }
		[Column("dogOurBankShetId")]
		public int? OurBankAccountId { get; set; }
		// равно null только при создании
		[Column("dogVidId")]
		public int? ContractServiceTypeId { get; set; }
		// равно null только при создании
		[Column("dogTypeId")]
		public int? ContractTypeId { get; set; }
		[Column]
		public int? PayMethodId { get; set; }
		[Column("dogOurDogKntUrTypeId")]
		public int? OurContractRoleId { get; set; }
		[Column("dogKontDogKntUrTypeId")]
		public int? ContractRoleId { get; set; }
		[Column]
		public int? CurrencyRateUseId { get; set; }
		[Column("dogUslOplatyId")]
		public int? PaymentTermsId { get; set; }
		[Column("dogBankSchetId")]
		public int? BankAccountId { get; set; }
		[Column("dogUrTypeInDogNotChanged")]
		public bool? IsFixed { get; set; }
		[Column("dogIsProlong")]
		public bool IsProlongation { get; set; }
		[Column("dogDate")]
		public DateTime? Date { get; set; }
		[Column("dogBeginDate")]
		public DateTime? BeginDate { get; set; }
		[Column("dogEndDate")]
		public DateTime? EndDate { get; set; }
		[Column("dogNum")]
		public string Number { get; set; }
		[Column("dogComm")]
		public string Comment { get; set; }
		[Column]
		public double AgentPercentage { get; set; }
	}

	/// <summary>
	/// Контрагент
	/// </summary>
	[Table("tblContact")]
	public partial class Contractor
	{
		[PrimaryKey]
		[Identity]
		[Column("IdCNT")]
		public int ID { get; set; }
		[Column("cntName")]
		public string Name { get; set; }
		[Column("cntCreator")]
		public int? CreatedBy { get; set; }
		[Column("cntCreatedAt")]
		public DateTime? CreatedDate { get; set; }
		[Column("cntIsBlocked")]
		public bool IsLocked { get; set; }
		[Column("cntBalance")]
		public double? Balance { get; set; }
		[Column("cntDohod")]
		public double? Income { get; set; }
		[Column("cntRashod")]
		public double? Expense { get; set; }
		[Column("cntPostuplenie")]
		public double? PaymentIncome { get; set; }
		[Column("cntOplata")]
		public double? PaymentExpense { get; set; }
		[Column("cntBalancePP")]
		public double? PaymentBalance { get; set; }
	}

	[Table]
	public partial class ContractorEmployeeSettings
	{
		[PrimaryKey]
		[Identity]
		[Column]
		public int ID { get; set; }
		[Column]
		public int ContractorId { get; set; }
		[Column]
		public int? EmployeeId { get; set; }
		[Column]
		public string Password { get; set; }
		[Column]
		public bool IsEnUI { get; set; }
		[Column]
		public bool NotifyEventCreated { get; set; }
		[Column]
		public bool NotifyStatusChanged { get; set; }
		[Column]
		public bool NotifyDocumentCreated { get; set; }
		[Column]
		public bool NotifyDocumentChanged { get; set; }
		[Column]
		public bool NotifyDocumentDeleted { get; set; }
		[Column]
		public bool NotifyTemplatedDocumentCreated { get; set; }
		[Column]
		public bool NotifyTemplatedDocumentChanged { get; set; }
	}

	/// <summary>
	/// Сотрудник
	/// </summary>
	[Table("tblEmployees")]
	public partial class Employee
	{
		[PrimaryKey]
		[Identity]
		[Column("IdlEmp")]
		public int ID { get; set; }
		[Column]
		public int EmployeeStatusId { get; set; }
		[Column]
		public int? FinRepCenterId { get; set; }
		[Column("empFlId")]
		public int? PersonId { get; set; }
		[Column("empUlId")]
		public int? LegalId { get; set; }
		[Column]
		public int? ContractorId { get; set; }
		[Column("empOtdel")]
		public string Department { get; set; }
		[Column("empDoljnost")]
		public string Position { get; set; }
		[Column("empDoljnostRP")]
		public string GenitivePosition { get; set; }
		[Column("empComm")]
		public string Comment { get; set; }
		[Column("empBeginDate")]
		public DateTime? BeginDate { get; set; }
		[Column("empEndDate")]
		public DateTime? EndDate { get; set; }
		[Column("empOsnovanie")]
		public string Basis { get; set; }
		[Column("empDoljnostEN")]
		public string EnPosition { get; set; }
		[Column("empOsnovanieEN")]
		public string EnBasis { get; set; }
		[Column("empPodpis")]
		public byte[] Signature { get; set; }
		[Column]
		public string PositionCode { get; set; }
		[Column]
		public DateTime? PositionStartDate { get; set; }
		[Column]
		public DateTime? PositionEndDate { get; set; }
		[Column]
		public string PositionHistory { get; set; }
		[Column]
		public string Password { get; set; }
	}

	/// <summary>
	/// Юридическое лицо
	/// </summary>
	[Table("Legals")]
	public partial class Legal
	{
		[PrimaryKey]
		[Identity]
		[Column("IdULC")]
		public int ID { get; set; }
		[Column("ulcContactId")]
		public int? ContractorId { get; set; }
		[Column("ulcVidULId")]
		public int? TaxTypeId { get; set; }
		[Column("ulcGenDir")]
		public int? DirectorId { get; set; }
		[Column("ulcGlavBuh")]
		public int? AccountantId { get; set; }

		[Column("ulcFullRU")]
		public string Name { get; set; }

		[Column("ulcShortRU")]
		public string DisplayName { get; set; }
		[Column("ulcFullENG")]
		public string EnName { get; set; }
		[Column("ulcShortENG")]
		public string EnShortName { get; set; }
		[Column("ulcINN")]
		public string TIN { get; set; }
		[Column("ulcOGRN")]
		public string OGRN { get; set; }
		[Column("ulcKPP")]
		public string KPP { get; set; }
		[Column("ulcOKPO")]
		public string OKPO { get; set; }
		[Column("ulcOKVED")]
		public string OKVED { get; set; }
		[Column("ulcAdrRU")]
		public string Address { get; set; }
		[Column("ulcAdrENG")]
		public string EnAddress { get; set; }
		[Column("ulcAdrFactRU")]
		public string AddressFact { get; set; }
		[Column("ulcAdrFactENG")]
		public string EnAddressFact { get; set; }
		[Column]
		public string PostAddress { get; set; }
		[Column]
		public string EnPostAddress { get; set; }
		[Column("ulcRabTime")]
		public string WorkTime { get; set; }
		[Column("ulcCreatedBy")]
		public int? CreatedBy { get; set; }
		[Column("ulcCreatedAt")]
		public DateTime? CreatedDate { get; set; }
		[Column("ulcChangedBy")]
		public int? UpdatedBy { get; set; }
		[Column("ulcChangedAt")]
		public DateTime? UpdatedDate { get; set; }

		[Column("ulcIsNerezident")]
		public bool IsNotResident { get; set; }

		[Column("ulcDohod")]
		public double? Income { get; set; }
		[Column("ulcRashod")]
		public double? Expense { get; set; }
		[Column("ulcBalance")]
		public double? Balance { get; set; }
		[Column("ulcPostuplenie")]
		public double? PaymentIncome { get; set; }
		[Column("ulcOplata")]
		public double? PaymentExpense { get; set; }
		[Column("ulcBalancePP")]
		public double? PaymentBalance { get; set; }
		[Column]
		public string TimeZone { get; set; }
	}

	/// <summary>
	/// Пункт маршрута
	/// </summary>
	[Table("tblZakazMarshrutPunkts")]
	public partial class RoutePoint
	{
		[PrimaryKey]
		[Identity]
		[Column("IdPNT")]
		public int ID { get; set; }
		[Column("ontZakazId")]
		public int OrderId { get; set; }
		[Column("pntNumber")]
		public int? No { get; set; }
		[Column("pntPunktTypeId")]
		public int? RoutePointTypeId { get; set; }
		[Column("pntPlaceId")]
		public int? PlaceId { get; set; }
		[Column("pntDatePlan")]
		public DateTime PlanDate { get; set; }
		[Column("pntDateFact")]
		public DateTime? FactDate { get; set; }
		[Column("pntAdresRU")]
		public string Address { get; set; }
		[Column("pntAdresEN")]
		public string EnAddress { get; set; }
		[Column("pntContactRU")]
		public string Contact { get; set; }
		[Column("ontContactEN")]
		public string EnContact { get; set; }
		[Column("pntGPOulID")]
		public int? ParticipantLegalId { get; set; }
		[Column("pntGPOcomment")]
		public string ParticipantComment { get; set; }
		[Column("pntGPOcontactID")]
		public int? RouteContactID { get; set; }
		[Column("pntGPOcommentEN")]
		public string EnParticipantComment { get; set; }
	}

	/// <summary>
	/// Плечо маршрута
	/// </summary>
	[Table("tblZakazMarshrut")]
	public partial class RouteSegment
	{
		[PrimaryKey]
		[Identity]
		[Column("IdMST")]
		public int ID { get; set; }
		[Column("mstZakazId")]
		public int OrderId { get; set; }
		[Column("mstSectionNum")]
		public int No { get; set; }
		[Column("mstFromId")]
		public int FromRoutePointId { get; set; }
		[Column("mstToId")]
		public int ToRoutePointId { get; set; }
		[Column("mstTransportId")]
		public int? TransportTypeId { get; set; }
		[Column("mstPosleGranicy")]
		public bool IsAfterBorder { get; set; }
		[Column("mstRastoyanie")]
		public double? Length { get; set; }
		[Column("mstMarkaTS")]
		public string Vehicle { get; set; }
		[Column("mstTSnumber")]
		public string VehicleNumber { get; set; }
		[Column("mstVoditelFIO")]
		public string DriverName { get; set; }
		[Column("mstVoditelPhone")]
		public string DriverPhone { get; set; }
	}

	/// <summary>
	/// Места в грузе (заказа)
	/// </summary>
	[Table("tblZakazGruzMesta")]
	public partial class CargoSeat
	{
		[PrimaryKey]
		[Identity]
		[Column("IdZGM")]
		public int ID { get; set; }
		[Column("zgmZakazGruzId")]
		public int? OrderId { get; set; }
		[Column("zgmGruzDescriptionId")]
		public int? CargoDescriptionId { get; set; }
		[Column("zgmUpakovkaTypeId")]
		public int? PackageTypeId { get; set; }
		[Column("zgmKolvoMest")]
		public int? SeatCount { get; set; }
		[Column("zgmDlinaCM")]
		public double? Length { get; set; }
		[Column("zgmShirinaCM")]
		public double? Width { get; set; }
		[Column("zgmVysotaCM")]
		public double? Height { get; set; }
		[Column("zgmObemM3")]
		public double? Volume { get; set; }
		[Column("zgmVesBrutto")]
		public double? GrossWeight { get; set; }
		[Column]
		public bool IsStacking { get; set; }
	}

	/// <summary>
	/// A class which represents the tblZakaz table.
	/// </summary>
	[Table("Orders")]
	public partial class Order
	{
		[PrimaryKey]
		[Identity]
		[Column("IdZKZ")]
		public int ID { get; set; }
		[Column("zkzTypeId")]
		public int? OrderTypeId { get; set; }
		[Column("zkzStatusId")]
		public int OrderStatusId { get; set; }
		[Column("zkzDogovorId")]
		public int? ContractId { get; set; }
		[Column("zkzProductId")]
		public int ProductId { get; set; }
		[Column]
		public int? OrderTemplateId { get; set; }
		[Column]
		public int? FinRepCenterId { get; set; }
		[Column("zkzOtpravitelId")]
		public int? SenderLegalId { get; set; }
		[Column("zkzPoluchId")]
		public int? ReceiverLegalId { get; set; }
		[Column("zkzGruzStrahTypeId")]
		public int? InsuranceTypeId { get; set; }
		[Column("zkzGruzBezStrahId")]
		public int? UninsuranceTypeId { get; set; }
		[Column("zkzInvoiceCurr")]
		public int? InvoiceCurrencyId { get; set; }
		[Column("zkzGruzKoefId")]
		public int? VolumetricRatioId { get; set; }

		[Column("zkzCreatedDate")]
		public DateTime? CreatedDate { get; set; }
		[Column]
		public DateTime? InvoiceDate { get; set; }
		[Column("zkzLoadingDate")]
		public DateTime? LoadingDate { get; set; }
		[Column("zkzUnloadingDate")]
		public DateTime? UnloadingDate { get; set; }
		[Column("zkzZayavkaDate")]
		public DateTime RequestDate { get; set; }
		[Column("zkzClosedate")]
		public DateTime? ClosedDate { get; set; }

		[Column("zkzNum")]
		public string Number { get; set; }
		[Column("zkzGruzInfo")]
		public string CargoInfo { get; set; }
		[Column("zkzFrom")]
		public string From { get; set; }
		[Column("zkzTo")]
		public string To { get; set; }
		[Column("zkzTSnum")]
		public string VehicleNumbers { get; set; }
		[Column("zkzZayavkaNum")]
		public string RequestNumber { get; set; }
		[Column]
		public string InvoiceNumber { get; set; }
		[Column("zkzGruzSpecHran")]
		public string SpecialCustody { get; set; }
		[Column("zkzGruzOpasnost")]
		public string Danger { get; set; }
		[Column("zkzGruzTempRezh")]
		public string TemperatureRegime { get; set; }
		[Column("zkzInvoiceCurrVal")]
		public string InvoiceCurrencyDisplay { get; set; }
		[Column]
		public string Cost { get; set; }

		[Column]
		public double? InvoiceSum { get; set; }
		[Column("zkzGruzVesNettoKG")]
		public double? NetWeight { get; set; }
		[Column("zkzGruzVesBruttoKG")]
		public double? GrossWeight { get; set; }
		[Column("zkzGruzOplachVesKG")]
		public double? PaidWeight { get; set; }
		[Column("zkzGruzObjemM3")]
		public double? Volume { get; set; }
		[Column("zkzGruzPrice")]
		public double? CargoPrice { get; set; }
		[Column("zkzEconomicaBalanceRUR")]
		public double? Balance { get; set; }
		[Column("zkzEconomicaRashod")]
		public double? Expense { get; set; }
		[Column("zkzEconomicaDohod")]
		public double? Income { get; set; }
		[Column("zkzForDetailsBeforBoardKM")]
		public double? RouteLengthBeforeBoard { get; set; }
		[Column("zkzForDetailsAfterBoardKM")]
		public double? RouteLengthAfterBoard { get; set; }

		[Column("zkzOtpravitelContactID")]
		public int? zkzOtpravitelContactID { get; set; }
		[Column("zkzPoluchContactID")]
		public int? zkzPoluchContactID { get; set; }
		[Column("zkzGruzKolvoMest")]
		public int? SeatsCount { get; set; }
		[Column("zkzGruzPriceCurrID")]
		public int? zkzGruzPriceCurrID { get; set; }
		[Column("zkzCreatedBy")]
		public int? CreatedBy { get; set; }

		[Column("zkzPrintDetails")]
		public bool? IsPrintDetails { get; set; }
		[Column("zkzGruzSpecHranEN")]
		public string EnSpecialCustody { get; set; }
		[Column("zkzGruzOpasnostEN")]
		public string EnDanger { get; set; }
		[Column("zkzGruzTempRezhEN")]
		public string EnTemperatureRegime { get; set; }
		[Column("CargoInfoEn")]
		public string EnCargoInfo { get; set; }
		[Column("zkzKomment")]
		public string Comment { get; set; }
		[Column("zkzForDetailsBeforBoard")]
		public string RouteBeforeBoard { get; set; }
		[Column("zkzForDetailsAfterBoard")]
		public string RouteAfterBoard { get; set; }
		[Column("zkzInsPolicy")]
		public string InsurancePolicy { get; set; }

		[Column("zkzForDetailsVATSumBefore")]
		public double? zkzForDetailsVATSumBefore { get; set; }
		[Column("zkzForDetailsVATSumAfter")]
		public double? zkzForDetailsVATSumAfter { get; set; }
		[Column("zkzForDetailsVAT")]
		public int? zkzForDetailsVAT { get; set; }
		[Column("zkzInsurUpak")]
		public string zkzInsurUpak { get; set; }

		[Column("zkzEconomicaBalance")]
		public double? zkzEconomicaBalance { get; set; }
		[Column("zkzInvoiceCurrNew")]
		public int? zkzInvoiceCurrNew { get; set; }
		[Column("zkzZayavkaID")]
		public int? zkzZayavkaID { get; set; }
		[Column("zkzGOnameINN")]
		public string zkzGOnameINN { get; set; }
		[Column("zkzGOcontact")]
		public string zkzGOcontact { get; set; }
		[Column("zkzGOcontTel")]
		public string zkzGOcontTel { get; set; }
		[Column("zkzGOtime")]
		public string zkzGOtime { get; set; }
		[Column("zkzGOcomment")]
		public string zkzGOcomment { get; set; }
		[Column("zkzGPnameINN")]
		public string zkzGPnameINN { get; set; }
		[Column("zkzGPcontact")]
		public string zkzGPcontact { get; set; }
		[Column("zkzGPconTel")]
		public string zkzGPconTel { get; set; }
		[Column("zkzGPtime")]
		public string zkzGPtime { get; set; }
		[Column("zkzGPcomment")]
		public string zkzGPcomment { get; set; }
		[Column("zkzGOname")]
		public string zkzGOname { get; set; }
		[Column("zkzGPname")]
		public string zkzGPname { get; set; }
		[Column("zkzMarshrut")]
		public string zkzMarshrut { get; set; }
		[Column("zkzExpTO")]
		public string zkzExpTO { get; set; }
		[Column("zkzImpTO")]
		public string zkzImpTO { get; set; }
		[Column("zkzKursValue")]
		public double? zkzKursValue { get; set; }
	}

	/// <summary>
	/// Физическое лицо
	/// </summary>
	[Table("tblFL")]
	public partial class Person
	{
		[PrimaryKey]
		[Identity]
		[Column("IdFLC")]
		public int ID { get; set; }
		[Column("flcF")]
		public string Family { get; set; }
		[Column("flcI")]
		public string Name { get; set; }
		[Column("flcO")]
		public string Patronymic { get; set; }
		[Column("flcInitials")]
		public string Initials { get; set; }
		[Column("flcFamIO")]
		public string DisplayName { get; set; }
		[Column("flcFrp")]
		public string GenitiveFamily { get; set; }
		[Column("flcIrp")]
		public string GenitiveName { get; set; }
		[Column("flcOrp")]
		public string GenitivePatronymic { get; set; }
		[Column("flcAdr")]
		public string Address { get; set; }
		[Column("flcEmail")]
		public string Email { get; set; }
		[Column("flcComment")]
		public string Comment { get; set; }
		[Column("flcENGname")]
		public string EnName { get; set; }
		[Column("flcIsNerez")]
		public bool IsNotResident { get; set; }
		[Column("flcIsSubscribe")]
		public bool IsSubscribed { get; set; }
		[Column("Birthday")]
		public DateTime? Birthday { get; set; }
	}

	/// <summary>
	/// Телефон
	/// </summary>
	[Table("tblPhones")]
	public partial class Phone
	{
		[PrimaryKey]
		[Identity]
		[Column("IdPHN")]
		public int ID { get; set; }
		[Column("phnName")]
		public string Name { get; set; }
		[Column("phnTypeId")]
		public int? TypeId { get; set; }
		[Column("phnNumber")]
		public string Number { get; set; }
	}

	/// <summary>
	/// Файлы документов, сформированные по шаблонам
	/// </summary>
	[Table("tblZakazEconomicaXLSforms")]
	public partial class TemplatedDocument
	{
		[PrimaryKey]
		[Identity]
		[Column("IdZEF")]
		public int ID { get; set; }
		[Column("zefZakazEconomicaId")]
		public int? AccountingId { get; set; }
		[Column("zafXLSFormId")]
		public int? TemplateId { get; set; }
		[Column("zefXLSname")]
		public string Filename { get; set; }
		[Column("zefCreatedAt")]
		public DateTime? CreatedDate { get; set; }
		[Column("zefLastSavedAt")]
		public DateTime? ChangedDate { get; set; }
		[Column("zefCreatedBy")]
		public int? CreatedBy { get; set; }
		[Column("zafChangedBy")]
		public int? ChangedBy { get; set; }
		[Column("zefZakazId")]
		public int? OrderId { get; set; }
		[Column]
		public DateTime? OriginalSentDate { get; set; }
		[Column]
		public int? OriginalSentUserId { get; set; }
		[Column]
		public DateTime? OriginalReceivedDate { get; set; }
		[Column]
		public int? OriginalReceivedUserId { get; set; }
		[Column]
		public string ReceivedBy { get; set; }
		[Column]
		public string ReceivedNumber { get; set; }
	}

	[Table("TemplatedDocumentData")]
	public partial class TemplatedDocumentData
	{
		[PrimaryKey]
		[Identity]
		[Column]
		public int ID { get; set; }
		[Column]
		public int TemplatedDocumentId { get; set; }
		[Column]
		public string Type { get; set; }
		//[Column]
		public byte[] Data { get; set; }
	}

	/// <summary>
	/// Прикрепленные файлы документов
	/// </summary>
	[Table("tblZakazEconomDocs")]
	public partial class Document
	{
		[PrimaryKey]
		[Identity]
		[Column("IdZDC")]
		public int ID { get; set; }
		[Column]
		public int? OrderId { get; set; }
		[Column]
		public int? ContractId { get; set; }
		[Column("zdcEconomicaId")]
		public int? AccountingId { get; set; }
		[Column("zdcTypeId")]
		public int? DocumentTypeId { get; set; }
		[Column("zdcNum")]
		public string Number { get; set; }
		[Column("zdcDate")]
		public DateTime? Date { get; set; }
		[Column("zdcFileName")]
		public string Filename { get; set; }
		[Column("zdcFileAddedAt")]
		public DateTime? UploadedDate { get; set; }
		[Column("zdcFileAddedBy")]
		public int? UploadedBy { get; set; }
		[Column("zdcFileIsPrinted")]
		public bool? IsPrint { get; set; }
		[Column("zdcFileSize")]
		public string FileSize { get; set; }
		[Column]
		public bool IsNipVisible { get; set; }

		[Column]
		public DateTime? OriginalSentDate { get; set; }
		[Column]
		public int? OriginalSentUserId { get; set; }
		[Column]
		public DateTime? OriginalReceivedDate { get; set; }
		[Column]
		public int? OriginalReceivedUserId { get; set; }
		[Column]
		public string ReceivedBy { get; set; }
		[Column]
		public string ReceivedNumber { get; set; }

		[Column]
		public bool IsWeekend { get; set; }
		[Column]
		public DateTime? WeekendMarkDate { get; set; }
		[Column]
		public int? WeekendMarkUserId { get; set; }
	}

	/// <summary>
	/// Содержимое документа (байты)
	/// </summary>
	[Table("tblZakazEconomDocsFiles")]
	public partial class DocumentData
	{
		[PrimaryKey]
		[Identity]
		[Column("IdEDF")]
		public int ID { get; set; }
		[Column("edfEconomDocId")]
		public int? DocumentId { get; set; }
		//[Column("edfData")]
		public byte[] Data { get; set; }
	}

	[Table]
	public partial class DocumentLog
	{
		[PrimaryKey]
		[Identity]
		[Column]
		public int ID { get; set; }
		[Column]
		public int OrderId { get; set; }
		[Column]
		public int ContractorId { get; set; }
		[Column]
		public int DocumentId { get; set; }
		[Column]
		public int DocumentTypeId { get; set; }
		[Column]
		public string DocumentNumber { get; set; }
		[Column]
		public DateTime DocumentDate { get; set; }
		[Column]
		public DateTime Date { get; set; }
		[Column]
		public int UserId { get; set; }
		[Column]
		public string Action { get; set; }
	}

	#endregion

	#region Связи

	[Table("ParticipantPermissions")]
	public partial class ParticipantPermission
	{
		[Column]
		public int ParticipantRoleId { get; set; }
		[Column]
		public int ActionId { get; set; }
	}

	/// <summary>
	/// Телефон юрлица (связка)
	/// </summary>
	[Table("tblUlPhones")]
	public partial class LegalPhone
	{
		[PrimaryKey]
		[Identity]
		[Column("IdPUL")]
		public int ID { get; set; }
		[Column("pulPhnID")]
		public int? PhoneId { get; set; }
		[Column("pulULID")]
		public int? LegalId { get; set; }
	}

	/// <summary>
	/// Телефон физического лица (связка)
	/// </summary>
	[Table("tblFlPhones")]
	public partial class PersonPhone
	{
		[PrimaryKey]
		[Identity]
		[Column("IdPFL")]
		public int ID { get; set; }
		[Column("pflPhnID")]
		public int? PhoneId { get; set; }
		[Column("pflFLID")]
		public int? PersonId { get; set; }
	}

	/// <summary>
	/// Рабочая группа по контрагенту
	/// </summary>
	[Table("tblContactManagers")]
	public partial class Participant
	{
		[PrimaryKey]
		[Identity]
		[Column("IdCNM")]
		public int ID { get; set; }
		[Column("cnmContactId")]
		public int? ContractorId { get; set; }
		[Column]
		public int? OrderId { get; set; }
		[Column]
		public int? ParticipantRoleId { get; set; }
		[Column("cnmUserId")]
		public int? UserId { get; set; }
		[Column("cnmUserIsFirst")]
		public bool IsDeputy { get; set; }
		[Column("cnmPeriodFrom")]
		public DateTime? FromDate { get; set; }
		[Column("cnmPeriodTo")]
		public DateTime? ToDate { get; set; }
		[Column]
		public bool IsResponsible { get; set; }
	}


	/// <summary>
	/// Пункты сегментов маршрута в расходе
	/// </summary>
	[Table("tblZakazEconomicaMarshrut")]
	public partial class OrderAccountingRouteSegment
	{
		[PrimaryKey]
		[Identity]
		[Column("IdEZM")]
		public int ID { get; set; }
		[Column("ezmEconomicaId")]
		public int? AccountingId { get; set; }
		[Column("ezmMarshrutId")]
		public int? RouteSegmentId { get; set; }
	}

	[Table("OrderTemplateOperations")]
	public class OrderTemplateOperation
	{
		[Column]
		public int OrderTemplateId { get; set; }
		[Column]
		public int OrderOperationId { get; set; }
		[Column]
		public int No { get; set; }
	}

	[Table("ContractCurrencies")]
	public class ContractCurrency
	{
		[Column]
		public int ContractId { get; set; }
		[Column]
		public int CurrencyId { get; set; }
		[Column]
		public int OurBankAccountId { get; set; }
		[Column]
		public int BankAccountId { get; set; }
	}

	#endregion

	[Table("ContractMarks")]
	public partial class ContractMark
	{
		[PrimaryKey]
		[Identity]
		[Column]
		public int ID { get; set; }
		[Column]
		public int ContractId { get; set; }
		[Column]
		public bool IsContractOk { get; set; }
		[Column]
		public DateTime? ContractOkDate { get; set; }
		[Column]
		public int? ContractOkUserId { get; set; }
		[Column]
		public bool IsContractChecked { get; set; }
		[Column]
		public DateTime? ContractCheckedDate { get; set; }
		[Column]
		public int? ContractCheckedUserId { get; set; }
		[Column]
		public bool IsContractRejected { get; set; }
		[Column]
		public DateTime? ContractRejectedDate { get; set; }
		[Column]
		public int? ContractRejectedUserId { get; set; }
		[Column]
		public string ContractRejectedComment { get; set; }
		[Column]
		public bool IsContractBlocked { get; set; }
		[Column]
		public DateTime? ContractBlockedDate { get; set; }
		[Column]
		public int? ContractBlockedUserId { get; set; }
		[Column]
		public string ContractBlockedComment { get; set; }
	}

	[Table("AccountingMarks")]
	public partial class AccountingMark
	{
		[PrimaryKey]
		[Identity]
		[Column]
		public int ID { get; set; }
		[Column]
		public int AccountingId { get; set; }
		// Invoice
		[Column]
		public bool IsInvoiceOk { get; set; }
		[Column]
		public DateTime? InvoiceOkDate { get; set; }
		[Column]
		public int? InvoiceOkUserId { get; set; }
		[Column]
		public bool IsInvoiceChecked { get; set; }
		[Column]
		public DateTime? InvoiceCheckedDate { get; set; }
		[Column]
		public int? InvoiceCheckedUserId { get; set; }
		[Column]
		public bool IsInvoiceRejected { get; set; }
		[Column]
		public DateTime? InvoiceRejectedDate { get; set; }
		[Column]
		public int? InvoiceRejectedUserId { get; set; }
		[Column]
		public string InvoiceRejectedComment { get; set; }
		// Act
		[Column]
		public bool IsActOk { get; set; }
		[Column]
		public DateTime? ActOkDate { get; set; }
		[Column]
		public int? ActOkUserId { get; set; }
		[Column]
		public bool IsActChecked { get; set; }
		[Column]
		public DateTime? ActCheckedDate { get; set; }
		[Column]
		public int? ActCheckedUserId { get; set; }
		[Column]
		public bool IsActRejected { get; set; }
		[Column]
		public DateTime? ActRejectedDate { get; set; }
		[Column]
		public int? ActRejectedUserId { get; set; }
		[Column]
		public string ActRejectedComment { get; set; }
		// Accounting
		[Column]
		public bool IsAccountingOk { get; set; }
		[Column]
		public DateTime? AccountingOkDate { get; set; }
		[Column]
		public int? AccountingOkUserId { get; set; }
		[Column]
		public bool IsAccountingChecked { get; set; }
		[Column]
		public DateTime? AccountingCheckedDate { get; set; }
		[Column]
		public int? AccountingCheckedUserId { get; set; }
		[Column]
		public bool IsAccountingRejected { get; set; }
		[Column]
		public DateTime? AccountingRejectedDate { get; set; }
		[Column]
		public int? AccountingRejectedUserId { get; set; }
		[Column]
		public string AccountingRejectedComment { get; set; }
	}

	#region asp_identity

	/// <summary>
	/// Приложение?
	/// </summary>
	[Table("aspnet_Applications")]
	public partial class asp_IdentityApplication
	{
		[PrimaryKey]
		[Identity]
		[Column("ApplicationId")]
		public Guid ApplicationId { get; set; }
		[Column("ApplicationName")]
		public string ApplicationName { get; set; }
		[Column("LoweredApplicationName")]
		public string LoweredApplicationName { get; set; }
		[Column("Description")]
		public string Description { get; set; }
	}

	/// <summary>
	/// Членство?
	/// </summary>
	[Table("aspnet_Membership")]
	public partial class asp_IdentityMembership
	{
		[PrimaryKey]
		[Identity]
		[Column("UserId")]
		public Guid UserId { get; set; }
		[Column("ApplicationId")]
		public Guid ApplicationId { get; set; }

		[Column("Password")]
		public string Password { get; set; }
		[Column("PasswordFormat")]
		public int PasswordFormat { get; set; }

		[Column("PasswordQuestion")]
		public string PasswordQuestion { get; set; }
		[Column("PasswordAnswer")]
		public string PasswordAnswer { get; set; }
		[Column("PasswordSalt")]
		public string PasswordSalt { get; set; }

		[Column("Email")]
		public string Email { get; set; }
		[Column("IsApproved")]
		public bool IsApproved { get; set; }
		[Column("IsLockedOut")]
		public bool IsLockedOut { get; set; }
		[Column("CreateDate")]
		public DateTime CreateDate { get; set; }
		[Column("LastLoginDate")]
		public DateTime LastLoginDate { get; set; }
		[Column("LastPasswordChangedDate")]
		public DateTime LastPasswordChangedDate { get; set; }
		[Column("LastLockoutDate")]
		public DateTime LastLockoutDate { get; set; }
		[Column("FailedPasswordAttemptCount")]
		public int FailedPasswordAttemptCount { get; set; }
		[Column("FailedPasswordAttemptWindowStart")]
		public DateTime FailedPasswordAttemptWindowStart { get; set; }
		[Column("FailedPasswordAnswerAttemptCount")]
		public int FailedPasswordAnswerAttemptCount { get; set; }
		[Column("FailedPasswordAnswerAttemptWindowStart")]
		public DateTime FailedPasswordAnswerAttemptWindowStart { get; set; }
	}

	/// <summary>
	/// Профайл?
	/// </summary>
	[Table("aspnet_Profile")]
	public partial class asp_IdentityProfile
	{
		[PrimaryKey]
		[Identity]
		[Column("UserId")]
		public Guid UserId { get; set; }
		[Column("PropertyNames")]
		public string PropertyNames { get; set; }
		[Column("PropertyValuesString")]
		public string PropertyValuesString { get; set; }

		[Column("LastUpdatedDate")]
		public DateTime LastUpdatedDate { get; set; }
	}

	/// <summary>
	/// Роль?
	/// </summary>
	[Table("aspnet_Roles")]
	public partial class asp_IdentityRole
	{
		[PrimaryKey]
		[Identity]
		[Column("RoleId")]
		public Guid RoleId { get; set; }
		[Column("ApplicationId")]
		public Guid ApplicationId { get; set; }
		[Column("RoleName")]
		public string RoleName { get; set; }
		[Column("LoweredRoleName")]
		public string LoweredRoleName { get; set; }
		[Column("Description")]
		public string Description { get; set; }
	}

	/// <summary>
	/// Назначение ролей
	/// </summary>
	[Table("aspnet_UsersInRoles")]
	public partial class asp_IdentityUserInRole
	{
		[Column("UserId")]
		public Guid UserId { get; set; }
		[Column("RoleId")]
		public Guid RoleId { get; set; }
	}

	/// <summary>
	/// Пользователь
	/// </summary>
	[Table("aspnet_Users")]
	public partial class asp_IdentityUser
	{
		[PrimaryKey]
		[Identity]
		[Column("UserId")]
		public Guid UserId { get; set; }
		[Column("ApplicationId")]
		public Guid ApplicationId { get; set; }
		[Column("UserName")]
		public string UserName { get; set; }
		[Column("LoweredUserName")]
		public string LoweredUserName { get; set; }
		[Column("MobileAlias")]
		public string MobileAlias { get; set; }
		[Column("IsAnonymous")]
		public bool IsAnonymous { get; set; }
		[Column("LastActivityDate")]
		public DateTime LastActivityDate { get; set; }
	}

	/// <summary>
	/// Разрешения
	/// </summary>
	[Table("RolePermissions")]
	public partial class asp_IdentityRolePermission
	{
		[PrimaryKey(1)]
		[Column("RoleName")]
		public string RoleName { get; set; }
		[PrimaryKey(2)]
		[Column("PermissionId")]
		public string PermissionId { get; set; }
	}

	#endregion

	/// <summary>
	/// Оказанная услуга
	/// </summary>
	[Table("tblZakazEconomUsluga")]
	public partial class Service
	{
		[PrimaryKey]
		[Identity]
		[Column("IdECU")]
		public int ID { get; set; }
		[Column("ecuEconomId")]
		public int? AccountingId { get; set; }
		[Column("ecuUslugaId")]
		public int? ServiceTypeId { get; set; }
		[Column("ecuCurID")]
		public int? CurrencyId { get; set; }
		[Column("ecuVATid")]
		public int? VatId { get; set; }
		[Column("ecuKolvo")]
		public double? Count { get; set; }
		[Column("ecuPrice")]
		public double? Price { get; set; }
		[Column("ecuSummRUR")]
		public double? Sum { get; set; }
		[Column("ecuSummVAL")]
		public double? OriginalSum { get; set; }
		[Column]
		public bool IsForDetalization { get; set; }
	}

	/// <summary>
	/// Наше юрлицо
	/// </summary>
	[Table("tblOurUL")]
	public partial class OurLegal
	{
		[PrimaryKey]
		[Identity]
		[Column("IdOUL")]
		public int ID { get; set; }
		[Column("oulUlId")]
		public int? LegalId { get; set; }
		[Column("oulName")]
		public string Name { get; set; }
		[Column("oulPechat")]
		public byte[] Sign { get; set; }
	}

	/// <summary>
	/// Обменные курсы валют
	/// </summary>
	[Table("tblKurs")]
	public partial class CurrencyRate
	{
		[PrimaryKey]
		[Identity]
		[Column("IdKRS")]
		public int ID { get; set; }
		[Column("krsDate")]
		public DateTime? Date { get; set; }
		[Column("krsCurrId")]
		public int? CurrencyId { get; set; }
		[Column("krsKurs")]
		public double? Rate { get; set; }
	}

	/// <summary>
	/// Учетные данные заказа (экономика)
	/// </summary>
	[Table("Accountings")]
	public partial class Accounting
	{
		[PrimaryKey]
		[Identity]
		[Column("IdECN")]
		public int ID { get; set; }
		[Column("ecnZakazId")]
		public int OrderId { get; set; }
		[Column]
		public bool IsIncome { get; set; }
		[Column("ecnDocTypeId")]
		public int? AccountingDocumentTypeId { get; set; }
		[Column("encOplataTypeId")]
		public int? AccountingPaymentTypeId { get; set; }
		[Column("encOplataMetodId")]
		public int? AccountingPaymentMethodId { get; set; }
		[Column]
		public int? PayMethodId { get; set; }
		[Column("ecnKntDogId")]
		public int? ContractId { get; set; }
		[Column("ecn2podpisID")]
		public int? SecondSignerEmployeeId { get; set; }
		[Column("ecnGruzoperevozchikID")]
		public int? CargoLegalId { get; set; }

		[Column("ecnNumberR")]
		public int SameDirectionNo { get; set; }
		[Column("OurLegalId")]
		public int? OurLegalId { get; set; }
		[Column("LegalId")]
		public int? LegalId { get; set; }

		[Column("ecnNum")]
		public string Number { get; set; }
		[Column("ecnSchetNum")]
		public string InvoiceNumber { get; set; }
		[Column]
		public string VatInvoiceNumber { get; set; }
		[Column("ecnAktNum")]
		public string ActNumber { get; set; }
		[Column("ecnMarshrut")]
		public string Route { get; set; }
		[Column("ecnKomment")]
		public string Comment { get; set; }
		[Column("ecn2PodpisFIO")]
		public string SecondSignerName { get; set; }
		[Column("ecn2PodpisDoljn")]
		public string SecondSignerPosition { get; set; }
		[Column("ecn2podpisIO")]
		public string SecondSignerInitials { get; set; }
		[Column("ecnTransport")]
		public string ecnTransport { get; set; }
		[Column("ecnTS")]
		public string ecnTS { get; set; }
		[Column("ecnImportTO")]
		public string ecnImportTO { get; set; }
		[Column("ecnExportTO")]
		public string ecnExportTO { get; set; }
		[Column("ecnGOnameINN")]
		public string ecnGOnameINN { get; set; }
		[Column("ecnGOcontact")]
		public string ecnGOcontact { get; set; }
		[Column("ecnGOcontTel")]
		public string ecnGOcontTel { get; set; }
		[Column("ecnGOtime")]
		public string ecnGOtime { get; set; }
		[Column("ecnGOcomment")]
		public string ecnGOcomment { get; set; }
		[Column("ecnGPnameINN")]
		public string ecnGPnameINN { get; set; }
		[Column("ecnGPcontact")]
		public string ecnGPcontact { get; set; }
		[Column("ecnGPconTel")]
		public string ecnGPconTel { get; set; }
		[Column("ecnGPtime")]
		public string ecnGPtime { get; set; }
		[Column("ecnGPcomment")]
		public string ecnGPcomment { get; set; }
		[Column("ecnMarshrutEN")]
		public string ecnMarshrutEN { get; set; }
		[Column("ecnExportTOEN")]
		public string ecnExportTOEN { get; set; }
		[Column("ecnImportTOEN")]
		public string ecnImportTOEN { get; set; }
		[Column("ecnKommentEN")]
		public string ecnKommentEN { get; set; }
		[Column("ecnGOnameINNEN")]
		public string ecnGOnameINNEN { get; set; }
		[Column("ecnGOcontactEN")]
		public string ecnGOcontactEN { get; set; }
		[Column("ecnGOcommentEN")]
		public string ecnGOcommentEN { get; set; }
		[Column("ecnGPnameINNEN")]
		public string ecnGPnameINNEN { get; set; }
		[Column("ecnGPcontactEN")]
		public string ecnGPcontactEN { get; set; }
		[Column("ecnGPcommentEN")]
		public string ecnGPcommentEN { get; set; }
		[Column("strGOcontTelEN")]
		public string strGOcontTelEN { get; set; }
		[Column("strGPcontTelEN")]
		public string strGPcontTelEN { get; set; }
		[Column("ecnGOtimeEN")]
		public string ecnGOtimeEN { get; set; }
		[Column("ecnGPtimeEN")]
		public string ecnGPtimeEN { get; set; }
		[Column]
		public string RejectHistory { get; set; }

		[Column("ecnCreatedAt")]
		public DateTime CreatedDate { get; set; }
		[Column("ecnSchetDate")]
		public DateTime? InvoiceDate { get; set; }
		[Column("ecnActDate")]
		public DateTime? ActDate { get; set; }
		[Column("ecnUchetDate")]
		public DateTime? AccountingDate { get; set; }
		[Column("ecnLoadingDate")]
		public DateTime? ecnLoadingDate { get; set; }
		[Column("ecnUnloadingDate")]
		public DateTime? ecnUnloadingDate { get; set; }
		[Column("PaymentPlanDate")]
		public DateTime? PaymentPlanDate { get; set; }
		[Column]
		public DateTime? RequestDate { get; set; }

		[Column("ecnSum")]
		public double? OriginalSum { get; set; }
		[Column]
		public double? OriginalVat { get; set; }
		[Column]
		public double? CurrencyRate { get; set; }
		[Column("ecnSumRUR")]
		public double? Sum { get; set; }
		[Column("ecnSumVAT")]
		public double? Vat { get; set; }
		[Column("ecnPayment")]
		public double? Payment { get; set; }
		[Column("ecnForDetailsSumBefore")]
		public double? RouteLengthBeforeBorderForDetails { get; set; }
		[Column("ecnForDetailsSumAfter")]
		public double? RouteLengthAfterBorderForDetails { get; set; }
	}

	/// <summary>
	/// Страна
	/// </summary>
	[Table("tblCountries")]
	public partial class Country
	{
		[PrimaryKey]
		[Identity]
		[Column("IdCountry")]
		public int ID { get; set; }
		[Column("countryNameRU")]
		public string Name { get; set; }
		[Column("countryNameEN")]
		public string EnName { get; set; }
		[Column("countryISO")]
		public string IsoCode { get; set; }
	}

	/// <summary>
	/// Географический регион
	/// </summary>
	[Table("tblRegions")]
	public partial class Region
	{
		[PrimaryKey]
		[Identity]
		[Column("IdRegion")]
		public int ID { get; set; }
		[Column("IdCountry")]
		public int? CountryId { get; set; }
		[Column("iso31662code")]
		public string IsoCode { get; set; }
		[Column("regionNameRU")]
		public string Name { get; set; }
		[Column("regionNameEN")]
		public string EnName { get; set; }
		[Column("regionNameLOCAL1")]
		public string regionNameLOCAL1 { get; set; }
		[Column("regionNameLOCAL2")]
		public string regionNameLOCAL2 { get; set; }
	}

	/// <summary>
	/// Географическая область
	/// </summary>
	[Table("tblSubRegions")]
	public partial class SubRegion
	{
		[PrimaryKey]
		[Identity]
		[Column("IdSubRegion")]
		public int ID { get; set; }
		[Column("IdCountry")]
		public int? CountryId { get; set; }
		[Column("IdRegion")]
		public int? RegionId { get; set; }
		[Column("subRegionNameRU")]
		public string Name { get; set; }
		[Column("subRegionNameEN")]
		public string EnName { get; set; }
	}

	/// <summary>
	/// Географическое место
	/// </summary>
	[Table("tblPlaces")]
	public partial class Place
	{
		[PrimaryKey]
		[Identity]
		[Column("IdPlace")]
		public int ID { get; set; }
		[Column("IdCountry")]
		public int? CountryId { get; set; }
		[Column("IdRegion")]
		public int? RegionId { get; set; }
		[Column("IdSubRegion")]
		public int? SubRegionId { get; set; }
		[Column("cityNameRU")]
		public string Name { get; set; }
		[Column("cityNameEN")]
		public string EnName { get; set; }
		[Column("IATAcode")]
		public string IataCode { get; set; }
		[Column("ICAOcode")]
		public string IcaoCode { get; set; }
		[Column("Airport")]
		public string Airport { get; set; }
	}

	/// <summary>
	/// Глобальные переменные и настройки системы
	/// </summary>
	[Table("tblSysVariables")]
	public partial class SystemSetting
	{
		[PrimaryKey]
		[Identity]
		[Column("IdVRB")]
		public int ID { get; set; }
		[Column("vrbName")]
		public string Name { get; set; }
		[Column("vrbType")]
		public string Type { get; set; }
		[Column("vrbValue")]
		public string Value { get; set; }

	}

	/// <summary>
	/// Шаблон для формирования файлов
	/// </summary>
	[Table("tblSysXLSforms")]
	public partial class Template
	{
		[PrimaryKey]
		[Identity]
		[Column("IdXLF")]
		public int ID { get; set; }
		[Column("xlfFormName")]
		public string Filename { get; set; }
		[Column("xlfFormInfo")]
		public string FileSize { get; set; }
		[Column("xlfData")]
		public byte[] Data { get; set; }

		[Column("xlfDocName")]
		public string Name { get; set; }
		[Column("xlfDocIDX")]
		public string Suffix { get; set; }
		[Column("xlfTblFirstRow")]
		public int? ListRow { get; set; }
		[Column("xlfFirstColumn")]
		public string ListFirstColumn { get; set; }
		[Column("xlfLastColumn")]
		public string ListLastColumn { get; set; }
		[Column("xlfColumns")]
		public string xlfColumns { get; set; }
	}

	/// <summary>
	/// Поле подстановки для шаблона
	/// </summary>
	[Table("tblSysXLSformsRef")]
	public partial class TemplateField
	{
		[PrimaryKey]
		[Identity]
		[Column("IdXFR")]
		public int ID { get; set; }
		[Column("xfrXLSformId")]
		public int? TemplateId { get; set; }
		[Column("xlfRefName")]
		public string Name { get; set; }
		[Column("xlfQFieldName")]
		public string FieldName { get; set; }
		[Column("xlfXLSRange")]
		public string Range { get; set; }
		[Column("xlfIsAtable")]
		public bool? IsAtable { get; set; }
	}

	/// <summary>
	/// Платеж (ПП)
	/// </summary>
	[Table("Payments")]
	public partial class Payment
	{
		[PrimaryKey]
		[Identity]
		[Column]
		public int ID { get; set; }
		[Column]
		public int? AccountingId { get; set; }
		[Column]
		public string Number { get; set; }
		[Column]
		public DateTime Date { get; set; }
		[Column]
		public double Sum { get; set; }
		[Column]
		public int? CurrencyId { get; set; }
		[Column]
		public string FinReference { get; set; }
		[Column]
		public string Description { get; set; }
		[Column]
		public bool IsIncome { get; set; }
		[Column]
		public bool IsMarkingRemoval { get; set; }
		[Column]
		public string BankAccount { get; set; }
		[Column]
		public string BIC_Swift { get; set; }
		[Column]
		public string BaseNumber { get; set; }
		[Column]
		public string KPP { get; set; }
		[Column]
		public string TIN { get; set; }
	}

	/// <summary>
	/// Контактная информация для узлов маршрута?
	/// </summary>
	[Table("tblZakazMarshrutKontakt")]
	public partial class RouteContact
	{
		[PrimaryKey]
		[Identity]
		[Column("IdZMK")]
		public int ID { get; set; }
		[Column("zmkPlaceID")]
		public int? PlaceId { get; set; }
		[Column("zmkULid")]
		public int? LegalId { get; set; }
		[Column("zmkName")]
		public string Name { get; set; }
		[Column("zmkKontakt")]
		public string Contact { get; set; }
		[Column("zmkPhones")]
		public string Phones { get; set; }
		[Column("zmkKontaktEN")]
		public string EnContact { get; set; }
		[Column("zmkEmail")]
		public string Email { get; set; }
		[Column]
		public string Address { get; set; }
		[Column]
		public string EnAddress { get; set; }
	}

	[Table]
	public partial class OrderStatusHistory
	{
		[PrimaryKey]
		[Identity]
		[Column]
		public int ID { get; set; }
		[Column]
		public int OrderId { get; set; }
		[Column]
		public int OrderStatusId { get; set; }
		[Column]
		public int UserId { get; set; }
		[Column]
		public DateTime Date { get; set; }
		[Column]
		public string Reason { get; set; }
	}

	[Table]
	public partial class ContractMarksHistory
	{
		[PrimaryKey]
		[Identity]
		[Column]
		public int ID { get; set; }
		[Column]
		public int ContractId { get; set; }
		[Column]
		public string Text { get; set; }
		[Column]
		public int UserId { get; set; }
		[Column]
		public DateTime Date { get; set; }
		[Column]
		public string Reason { get; set; }
	}

	[Table("Requests")]
	public partial class Request
	{
		[PrimaryKey]
		[Identity]
		[Column]
		public int ID { get; set; }
		[Column]
		public int? SalesUserId { get; set; }
		[Column]
		public int? AccountUserId { get; set; }
		[Column]
		public int? ProductId { get; set; }
		[Column]
		public int? ContractorId { get; set; }
		[Column]
		public string ClientName { get; set; }
		[Column]
		public string Text { get; set; }
		[Column]
		public string CargoInfo { get; set; }
		[Column]
		public string Route { get; set; }
		[Column]
		public string Contacts { get; set; }
		[Column]
		public DateTime Date { get; set; }
		[Column]
		public DateTime? ResponseDate { get; set; }
		[Column]
		public string Comment { get; set; }
		[Column]
		public string Filename { get; set; }
	}

	[Table]
	public partial class SyncQueue
	{
		[PrimaryKey]
		[Identity]
		[Column]
		public int ID { get; set; }
		[Column]
		public int AccountingId { get; set; }
		[Column]
		public string Error { get; set; }
	}

	[Table("Actions")]
	public partial class Action
	{
		[PrimaryKey]
		[Identity]
		[Column]
		public int ID { get; set; }
		[Column]
		public string Name { get; set; }
		[Column]
		public string Hint { get; set; }
	}

	[Table("CrmCalls")]
	public partial class CrmCall
	{
		[PrimaryKey]
		[Identity]
		[Column]
		public int ID { get; set; }
		[Column("DATE_CAL")]
		public DateTime Date { get; set; }
		[Column("TIME_START")]
		public DateTime From { get; set; }
		[Column("TIME_FINAL")]
		public DateTime? To { get; set; }
		[Column("TEL_NUMBER")]
		public string Number { get; set; }
		[Column]
		public int ID_COMPANY { get; set; }
		[Column]
		public int ID_CONTACT_MAN { get; set; }
		[Column("PRIM")]
		public string Direction { get; set; }
		[Column]
		public int ID_MANAGER { get; set; }
		[Column]
		public string MANAGER_NAME { get; set; }
		[Column("SAVE_FILE")]
		public string Filename { get; set; }
		[Column("IS_CANCEL")]
		public bool? IsCancelled { get; set; }

		// TEMP:
		public int? UserId { get; set; }
		public string Duration { get; set; }
	}

	[Table("CrmManagers")]
	public partial class CrmManager
	{
		[PrimaryKey]
		[Identity]
		[Column]
		public int ID { get; set; }
		[Column]
		public int ID_COMPANY { get; set; }
		[Column("MANAGER_NAME")]
		public string Name { get; set; }

		/*[ID]
		  ,[MANAGER_NAME]
		  ,[DATA_INPUT]
		  ,[DATA_HAPY]
		  ,[TELEPHON_HOME]
		  ,[TELEPHON_JOB]
		  ,[ADRES_HOME]
		  ,[PASWORD]
		  ,[LOGIN]
		  ,[TIP]
		  ,[e_mail]
		  ,[id_group]
		  ,[ONLINE]
		  ,[REGONLINE]
		  ,[EXCEL_SAVE_LIST_COMPANY]
		  ,[EXCEL_SAVE_REPORT]
		  ,[SAVE_REPORT_COMPANY]
		  ,[VER]
		  ,[ID_LIST_SIP_CHANEL]
		  ,[SIP_LOGIN]
		  ,[SIP_PASW]
		  ,[SIP_USER]
		  ,[ID_GROUP_COMPANY]
		  ,[ENABLED]
		  ,[SHOW_PHONE_PANEL]
		  ,[SAVE_ALL_CALL]
		  ,[COLOR_SCHLURER_MARKER]
		  ,[TIME_ZONE_OFFSET]
		  ,[TIME_SUMMER]
		  ,[ID_SIP_ACCOUNT]
		  ,[ID_COMPANY]
		  ,[HIDDEN_MODULES]
		  ,[COMPANY_DATA_ID]
		  ,[ID_KPI]
		  ,[AVATAR_URL]
		  ,[EMAIL_NOTIFICATION]
		  ,[AVATAR_URL_KPI]
		  ,[COVER_URL]
		  ,[GOOGLEDISC_LOGIN]
		  ,[GOOGLEDISC_PASS]
		  ,[DROPBOX_LOGIN]
		  ,[DROPBOX_PASS]
		  ,[ID_CONTACT_MAN]
		  ,[IS_NOT_FIRST_LOGIN]
		  ,[latest_version]
		  ,[WEBINAR_NOTIFICATION]
		  ,[DOUBLE_LOGIN]
		  ,[VOXIMPLANT_USERNAME]
		  ,[MANAGER_POSITION]
		  ,[has_iron_phone]
		  ,[IS_OUTER_CHAT_ON]
		FROM[dbNVLC].[dbo].[CrmManagers]*/
	}

    [Table("CrmLegals")]
    public class CrmLegal
    {
        [PrimaryKey]
        [Identity]
        [Column]
        public int ID { get; set; }
        [Column]
        public string CompanyName { get; set; }
        [Column]
        public string CompanyFullName { get; set; }
        [Column]
        public string Address { get; set; }
        [Column]
        public string LegalAddress { get; set; }
        [Column]
        public string PostAddress { get; set; }
        [Column]
        public string TIN { get; set; }
        [Column]
        public string KPP { get; set; }
        [Column]
        public string OGRN { get; set; }
    }

    [Table("OrdersRentability")]
	public class OrderRentability
	{
		[PrimaryKey]
		[Identity]
		[Column]
		public int ID { get; set; }
		[Column]
		public int ProductId { get; set; }
		[Column]
		public int OrderTemplateId { get; set; }
		[Column]
		public int Year { get; set; }
		[Column]
		public int FinRepCenterId { get; set; }
		[Column]
		public float Rentability { get; set; }
	}

	[Table("AutoExpenses")]
	public class AutoExpense
	{
		[PrimaryKey]
		[Identity]
		[Column]
		public int ID { get; set; }
		[Column]
		public DateTime From { get; set; }
		[Column]
		public DateTime To { get; set; }
		[Column]
		public float USD { get; set; }
		[Column]
		public float EUR { get; set; }
		[Column]
		public float GBP { get; set; }
		[Column]
		public float CNY { get; set; }
	}

	[Table("Pricelists")]
	public class Pricelist
	{
		[PrimaryKey]
		[Identity]
		[Column]
		public int ID { get; set; }
		[Column]
		public string Name { get; set; }
		[Column]
		public DateTime? From { get; set; }
		[Column]
		public DateTime? To { get; set; }
		[Column]
		public int? FinRepCenterId { get; set; }
		[Column]
		public int? ProductId { get; set; }
		[Column]
		public int? ContractId { get; set; }
		[Column]
		public byte[] Data { get; set; }
		[Column]
		public string Filename { get; set; }
		[Column]
		public string Comment { get; set; }
	}

	[Table("Prices")]
	public class Price
	{
		[PrimaryKey]
		[Identity]
		[Column]
		public int ID { get; set; }
		[Column]
		public double Sum { get; set; }
		[Column]
		public double Count { get; set; }
		[Column]
		public int VatId { get; set; }
		[Column]
		public int CurrencyId { get; set; }
		[Column]
		public int PricelistId { get; set; }
		[Column]
		public int ServiceId { get; set; }
	}

	[Table("PriceKinds")]
	public class PriceKind
	{
		[PrimaryKey]
		[Identity]
		[Column]
		public int ID { get; set; }
		[Column]
		public string Name { get; set; }
		[Column]
		public string EnName { get; set; }
		[Column]
		public int PricelistId { get; set; }
		[Column]
		public int ServiceKindId { get; set; }
	}

	[Table("PayMethods")]
	public class PayMethod
	{
		[PrimaryKey]
		[Identity]
		[Column]
		public int ID { get; set; }
		[Column]
		public string Display { get; set; }
		[Column]
		public DateTime From { get; set; }
		[Column]
		public DateTime To { get; set; }
	}

	[Table("AccountingMatchings")]
	public class AccountingMatching
	{
		[PrimaryKey]
		[Identity]
		[Column]
		public int ID { get; set; }
		[Column]
		public int ExpenseAccountingId { get; set; }
		[Column]
		public int IncomeAccountingId { get; set; }
		[Column]
		public double Sum { get; set; }
	}

	[Table]
	public class Schedule
	{
		[PrimaryKey]
		[Identity]
		[Column]
		public int ID { get; set; }
		[Column]
		public string ReportName { get; set; }
		[Column]
		public byte Hour { get; set; }
		[Column]
		public byte Minute { get; set; }
		[Column]
		public byte Weekday { get; set; }
	}

	[Table]
	public class Job
	{
		[PrimaryKey]
		[Identity]
		[Column]
		public int ID { get; set; }
		[Column]
		public int ScheduleId { get; set; }
		[Column]
		public DateTime Date { get; set; }
		[Column]
		public bool IsDone { get; set; }
	}

	[Table("EmployeeSettings")]
	public partial class EmployeeSetting
	{
		[PrimaryKey]
		[Identity]
		[Column]
		public int ID { get; set; }
		[Column]
		public int EmployeeId { get; set; }
		[Column]
		public string Key { get; set; }
		[Column]
		public string Value { get; set; }
	}

	[Table]
	public partial class MailingLog
	{
		[PrimaryKey]
		[Identity]
		[Column]
		public int ID { get; set; }
		[Column]
		public DateTime Date { get; set; }
		[Column]
		public string To { get; set; }
		[Column]
		public string Subject { get; set; }
		[Column]
		public string Text { get; set; }
	}
}
