using LinqToDB;
using Logisto.Models;

namespace Logisto.Data
{
	public class LogistoDb : LinqToDB.Data.DataConnection
	{
		public LogistoDb()
			: base("DefaultConnection")
		{ }

		public ITable<CrmCall> CrmCalls { get { return GetTable<CrmCall>(); } }
		public ITable<CrmManager> CrmManagers { get { return GetTable<CrmManager>(); } }
		public ITable<CrmLegal> CrmLegals { get { return GetTable<CrmLegal>(); } }

		public ITable<Bank> Banks { get { return GetTable<Bank>(); } }
		public ITable<User> Users { get { return GetTable<User>(); } }
		public ITable<Legal> Legals { get { return GetTable<Legal>(); } }
		public ITable<Phone> Phones { get { return GetTable<Phone>(); } }
		public ITable<Order> Orders { get { return GetTable<Order>(); } }
		public ITable<Person> Persons { get { return GetTable<Person>(); } }
		public ITable<Request> Requests { get { return GetTable<Request>(); } }
		public ITable<Contract> Contracts { get { return GetTable<Contract>(); } }
		public ITable<Employee> Employees { get { return GetTable<Employee>(); } }
		public ITable<OurLegal> OurLegals { get { return GetTable<OurLegal>(); } }
		public ITable<CargoSeat> CargoSeats { get { return GetTable<CargoSeat>(); } }
		public ITable<SyncQueue> SyncQueue { get { return GetTable<SyncQueue>(); } }
		public ITable<PayMethod> PayMethods { get { return GetTable<PayMethod>(); } }
		public ITable<Accounting> Accountings { get { return GetTable<Accounting>(); } }
		public ITable<Contractor> Contractors { get { return GetTable<Contractor>(); } }
		public ITable<RoutePoint> RoutePoints { get { return GetTable<RoutePoint>(); } }
		public ITable<BankAccount> BankAccounts { get { return GetTable<BankAccount>(); } }
		public ITable<Participant> Participants { get { return GetTable<Participant>(); } }
		public ITable<AutoExpense> AutoExpenses { get { return GetTable<AutoExpense>(); } }
		public ITable<OrderRentability> OrdersRentability { get { return GetTable<OrderRentability>(); } }
		public ITable<TemplatedDocument> TemplatedDocuments { get { return GetTable<TemplatedDocument>(); } }
		public ITable<ContractorEmployeeSettings> ContractorEmployeeSettings { get { return GetTable<ContractorEmployeeSettings>(); } }
		public ITable<DocumentTypeProductPrint> DocumentTypeProductPrints { get { return GetTable<DocumentTypeProductPrint>(); } }

		public ITable<LegalPhone> LegalPhones { get { return GetTable<LegalPhone>(); } }
		public ITable<PersonPhone> PersonPhones { get { return GetTable<PersonPhone>(); } }
		public ITable<RouteContact> RouteContacts { get { return GetTable<RouteContact>(); } }
		public ITable<RouteSegment> RouteSegments { get { return GetTable<RouteSegment>(); } }
		public ITable<CurrencyRate> CurrencyRates { get { return GetTable<CurrencyRate>(); } }
		public ITable<SystemSetting> SystemSettings { get { return GetTable<SystemSetting>(); } }
		public ITable<CurrencyRateDiff> CurrencyRateDiff { get { return GetTable<CurrencyRateDiff>(); } }
		public ITable<TemplatedDocumentData> TemplatedDocumentData { get { return GetTable<TemplatedDocumentData>(); } }
		public ITable<OrderAccountingRouteSegment> OrderAccountingRouteSegments { get { return GetTable<OrderAccountingRouteSegment>(); } }

		public ITable<Country> Countries { get { return GetTable<Country>(); } }
		public ITable<Region> Regions { get { return GetTable<Region>(); } }
		public ITable<SubRegion> SubRegions { get { return GetTable<SubRegion>(); } }
		public ITable<Place> Places { get { return GetTable<Place>(); } }

		public ITable<Service> Services { get { return GetTable<Service>(); } }
		public ITable<Payment> Payments { get { return GetTable<Payment>(); } }
		public ITable<Template> Templates { get { return GetTable<Template>(); } }
		public ITable<Document> Documents { get { return GetTable<Document>(); } }
		public ITable<UserSetting> UserSettings { get { return GetTable<UserSetting>(); } }
		public ITable<DocumentData> DocumentData { get { return GetTable<DocumentData>(); } }
		public ITable<ContractMark> ContractMarks { get { return GetTable<ContractMark>(); } }
		public ITable<TemplateField> TemplateFields { get { return GetTable<TemplateField>(); } }
		public ITable<AccountingMark> AccountingMarks { get { return GetTable<AccountingMark>(); } }
		public ITable<OrderTemplate> OrderTemplates { get { return GetTable<OrderTemplate>(); } }
		public ITable<OrderEvent> OrderEvents { get { return GetTable<OrderEvent>(); } }

		public ITable<Price> Prices { get { return GetTable<Price>(); } }
		public ITable<PriceKind> PriceKinds { get { return GetTable<PriceKind>(); } }
		public ITable<Models.Action> Actions { get { return GetTable<Models.Action>(); } }
		public ITable<Operation> Operations { get { return GetTable<Operation>(); } }
		public ITable<Pricelist> Pricelists { get { return GetTable<Pricelist>(); } }
		public ITable<OperationKind> OperationKinds { get { return GetTable<OperationKind>(); } }
		public ITable<OrderOperation> OrderOperations { get { return GetTable<OrderOperation>(); } }
		public ITable<OperationStatus> OperationStatuses { get { return GetTable<OperationStatus>(); } }
		public ITable<ContractCurrency> ContractCurrencies { get { return GetTable<ContractCurrency>(); } }
		public ITable<OrderStatusHistory> OrderStatusHistory { get { return GetTable<OrderStatusHistory>(); } }
		public ITable<AccountingMatching> AccountingMatchings { get { return GetTable<AccountingMatching>(); } }
		public ITable<ContractMarksHistory> ContractMarksHistory { get { return GetTable<ContractMarksHistory>(); } }
		public ITable<ParticipantPermission> ParticipantPermissions { get { return GetTable<ParticipantPermission>(); } }
		public ITable<OrderTemplateOperation> OrderTemplateOperations { get { return GetTable<OrderTemplateOperation>(); } }

		public ITable<Schedule> Schedule { get { return GetTable<Schedule>(); } }
		public ITable<Job> Job { get { return GetTable<Job>(); } }
		public ITable<MailingLog> MailingLog { get { return GetTable<MailingLog>(); } }
		public ITable<DocumentLog> DocumentLog { get { return GetTable<DocumentLog>(); } }

		// справочники
		public ITable<VAT> Vats { get { return GetTable<VAT>(); } }
		public ITable<Role> Roles { get { return GetTable<Role>(); } }
		public ITable<Event> Events { get { return GetTable<Event>(); } }
		public ITable<Product> Products { get { return GetTable<Product>(); } }
		public ITable<TaxType> TaxTypes { get { return GetTable<TaxType>(); } }
		public ITable<Measure> Measures { get { return GetTable<Measure>(); } }
		public ITable<Currency> Currencies { get { return GetTable<Currency>(); } }
		public ITable<PhoneType> PhoneTypes { get { return GetTable<PhoneType>(); } }
		public ITable<OrderType> OrderTypes { get { return GetTable<OrderType>(); } }
		public ITable<PaymentTerm> PaymentTerms { get { return GetTable<PaymentTerm>(); } }
		public ITable<ServiceKind> ServiceKinds { get { return GetTable<ServiceKind>(); } }
		public ITable<ServiceType> ServiceTypes { get { return GetTable<ServiceType>(); } }
		public ITable<PackageType> PackageTypes { get { return GetTable<PackageType>(); } }
		public ITable<OrderStatus> OrderStatuses { get { return GetTable<OrderStatus>(); } }
		public ITable<FinRepCenter> FinRepCenters { get { return GetTable<FinRepCenter>(); } }
		public ITable<ContractRole> ContractRoles { get { return GetTable<ContractRole>(); } }
		public ITable<ContractType> ContractTypes { get { return GetTable<ContractType>(); } }
		public ITable<DocumentType> DocumentTypes { get { return GetTable<DocumentType>(); } }
		public ITable<InsuranceType> InsuranceTypes { get { return GetTable<InsuranceType>(); } }
		public ITable<TransportType> TransportTypes { get { return GetTable<TransportType>(); } }
		public ITable<RoutePointType> RoutePointTypes { get { return GetTable<RoutePointType>(); } }
		public ITable<EmployeeStatus> EmployeeStatuses { get { return GetTable<EmployeeStatus>(); } }
		public ITable<ParticipantRole> ParticipantRoles { get { return GetTable<ParticipantRole>(); } }
		public ITable<CurrencyRateUse> CurrencyRateUses { get { return GetTable<CurrencyRateUse>(); } }
		public ITable<UninsuranceType> UninsuranceTypes { get { return GetTable<UninsuranceType>(); } }
		public ITable<VolumetricRatio> VolumetricRatios { get { return GetTable<VolumetricRatio>(); } }
		public ITable<CargoDescription> CargoDescriptions { get { return GetTable<CargoDescription>(); } }
		public ITable<PositionTemplate> PositionTemplates { get { return GetTable<PositionTemplate>(); } }
		public ITable<SigningAuthority> SigningAuthorities { get { return GetTable<SigningAuthority>(); } }
		public ITable<ContractServiceType> ContractServiceTypes { get { return GetTable<ContractServiceType>(); } }
		public ITable<AccountingPaymentType> AccountingPaymentTypes { get { return GetTable<AccountingPaymentType>(); } }
		public ITable<AccountingDocumentType> AccountingDocumentTypes { get { return GetTable<AccountingDocumentType>(); } }
		public ITable<AccountingPaymentMethod> AccountingPaymentMethods { get { return GetTable<AccountingPaymentMethod>(); } }

		// asp_identity
		public ITable<asp_IdentityUser> asp_IdentityUsers { get { return GetTable<asp_IdentityUser>(); } }
		public ITable<asp_IdentityRole> asp_IdentityRoles { get { return GetTable<asp_IdentityRole>(); } }
		public ITable<asp_IdentityProfile> asp_IdentityProfiles { get { return GetTable<asp_IdentityProfile>(); } }
		public ITable<asp_IdentityMembership> asp_IdentityMemberships { get { return GetTable<asp_IdentityMembership>(); } }
		public ITable<asp_IdentityUserInRole> asp_IdentityUserInRoles { get { return GetTable<asp_IdentityUserInRole>(); } }
		public ITable<asp_IdentityApplication> asp_IdentityApplications { get { return GetTable<asp_IdentityApplication>(); } }
		public ITable<asp_IdentityRolePermission> asp_IdentityRolePermissions { get { return GetTable<asp_IdentityRolePermission>(); } }

		// Identity
		public ITable<IdentityUser> IdentityUsers { get { return GetTable<IdentityUser>(); } }
		public ITable<IdentityRole> IdentityRoles { get { return GetTable<IdentityRole>(); } }
		public ITable<IdentityAccount> IdentityAccounts { get { return GetTable<IdentityAccount>(); } }
		public ITable<IdentityUserInRole> IdentityUserInRoles { get { return GetTable<IdentityUserInRole>(); } }

	}
}

