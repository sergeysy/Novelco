using System;
using System.Collections.Generic;
using Logisto.Models;

namespace Logisto.BusinessLogic
{
	public interface IDataLogic
	{
		IEnumerable<VAT> GetVats();
		IEnumerable<Role> GetRoles();
		IEnumerable<Event> GetEvents();
		IEnumerable<TaxType> GetTaxTypes();
		IEnumerable<Product> GetProducts();
		IEnumerable<Measure> GetMeasures();
		IEnumerable<Template> GetTemplates();
		IEnumerable<Currency> GetCurrencies();
		IEnumerable<OrderType> GetOrderTypes();
		IEnumerable<PhoneType> GetPhoneTypes();
		IEnumerable<PaymentTerm> GetPaymentTerms();
		IEnumerable<PackageType> GetPackageTypes();
		IEnumerable<ServiceType> GetServiceTypes();
		IEnumerable<ServiceKind> GetServiceKinds();
		IEnumerable<OrderStatus> GetOrderStatuses();
		IEnumerable<ContractRole> GetContractRoles();
		IEnumerable<ContractType> GetContractTypes();
		IEnumerable<InsuranceType> GetInsuranceTypes();
		IEnumerable<TransportType> GetTransportTypes();
		IEnumerable<TemplateField> GetTemplateFields();
		IEnumerable<OrderTemplate> GetOrderTemplates();
		IEnumerable<OperationKind> GetOperationKinds();
		IEnumerable<OrderTemplate> GetOrderTemplatesByContract(int contractId);
		IEnumerable<OrderOperation> GetOrderOperations();
		IEnumerable<OrderOperation> GetOrderOperationsByTemplate(int templateId);
		IEnumerable<RoutePointType> GetRoutePointTypes();
		IEnumerable<EmployeeStatus> GetEmployeeStatuses();
		IEnumerable<UninsuranceType> GetUninsuranceTypes();
		IEnumerable<OperationStatus> GetOperationStatuses();
		IEnumerable<VolumetricRatio> GetVolumetricRatios();
		IEnumerable<CargoDescription> GetCargoDescriptions();
		IEnumerable<SigningAuthority> GetSigningAuthorities();
		IEnumerable<ContractServiceType> GetContractServiceTypes();
		IEnumerable<AccountingPaymentType> GetAccountingPaymentTypes();
		IEnumerable<AccountingDocumentType> GetAccountingDocumentTypes();
		IEnumerable<OrderTemplateOperation> GetOrderTemplateOperations();
		IEnumerable<AccountingPaymentMethod> GetAccountingPaymentMethods();

		int CreateDocumentTypeProductPrint(DocumentTypeProductPrint entity);
		void DeleteDocumentTypeProductPrint(int id);
		IEnumerable<DocumentTypeProductPrint> GetDocumentTypePrints(int productId);
		IEnumerable<DocumentTypeProductPrint> GetDocumentTypeProductPrints(int documentTypeId);

		Template GetTemplate(int id);
		TemplateField GetTemplateField(int id);
		void UpdateTemplate(Template template);
		void UpdateTemplateField(TemplateField entity);

		#region извращенство

		IEnumerable<DynamicDictionary> GetLegalProviders();
		IEnumerable<DynamicDictionary> GetLegalsByContract();
		IEnumerable<DynamicDictionary> GetContractorsByOrder();
		IEnumerable<DynamicTrictionary> GetContractorsByContract();

		#endregion

		#region dynamic

		IEnumerable<DynamicDictionary> GetOrders();
		IEnumerable<DynamicDictionary> GetUsers();
		IEnumerable<DynamicDictionary> GetActiveUsers();
		IEnumerable<DynamicDictionary> GetContracts();
		IEnumerable<DynamicDictionary> GetContractors();
		IEnumerable<DynamicDictionary> GetLegals();
		IEnumerable<DynamicDictionary> GetEmployees();
		IEnumerable<DynamicDictionary> GetBanks();
		IEnumerable<DynamicDictionary> GetOurLegalLegals();
		IEnumerable<DynamicDictionary> GetOurLegals();
		IEnumerable<DynamicDictionary> GetPersons();

		#endregion

		Dictionary<string, object> GetDataCounters();

		List<CurrencyRate> GetCurrencyRates(DateTime date);

		Dictionary<DateTime, List<float>> GetCurrenciesRates(ListFilter filter);

		List<SystemSetting> GetSystemSettings();
		SystemSetting GetSystemSetting(string name);
		void UpdateSystemSetting(SystemSetting entity);

		List<SyncQueue> GetSyncQueue();
		int CreateSyncQueue(SyncQueue entity);
		void UpdateSyncQueue(SyncQueue entity);
		void DeleteSyncQueue(int id);

		CargoDescription GetCargoDescription(int id);
		int CreateCargoDescription(CargoDescription cargoDescription);
		void UpdateCargoDescription(CargoDescription cargoDescription);

		PackageType GetPackageType(int id);
		void UpdatePackageType(PackageType entity);

		int CreateFinRepCenter(FinRepCenter entity);
		FinRepCenter GetFinRepCenter(int id);
		void UpdateFinRepCenter(FinRepCenter entity);
		IEnumerable<FinRepCenter> GetFinRepCenters();

		PaymentTerm GetPaymentTerm(int id);
		int CreatePaymentTerm(PaymentTerm entity);
		void UpdatePaymentTerm(PaymentTerm entity);

		OrderRentability GetOrderRentability(int id);
		int CreateOrderRentability(OrderRentability entity);
		void UpdateOrderRentability(OrderRentability entity);
		IEnumerable<OrderRentability> GetOrdersRentability();

		ContractRole GetContractRole(int id);
		int CreateContractRole(ContractRole entity);
		void UpdateContractRole(ContractRole entity);
		void DeleteContractRole(int id);

		ContractType GetContractType(int id);
		int CreateContractType(ContractType entity);
		void UpdateContractType(ContractType entity);

		Product GetProduct(int id);
		int CreateProduct(Product entity);
		void UpdateProduct(Product entity);

		OrderOperation GetOrderOperation(int id);
		int CreateOrderOperation(OrderOperation entity);
		void UpdateOrderOperation(OrderOperation entity);

		OrderTemplate GetOrderTemplate(int id);
		int CreateOrderTemplate(OrderTemplate entity);
		void UpdateOrderTemplate(OrderTemplate entity);
		void DeleteOrderTemplate(int id);

		OrderTemplateOperation GetOrderTemplateOperation(int orderTemplateId, int orderOperationId);
		void CreateOrderTemplateOperation(OrderTemplateOperation entity);
		void UpdateOrderTemplateOperation(OrderTemplateOperation entity);
		void DeleteOrderTemplateOperation(int orderTemplateId, int orderOperationId);

		ServiceKind GetServiceKind(int id);
		int CreateServiceKind(ServiceKind entity);
		void UpdateServiceKind(ServiceKind entity);

		ServiceType GetServiceType(int id);
		int CreateServiceType(ServiceType entity);
		void UpdateServiceType(ServiceType entity);

		int GetCountriesCount(ListFilter listFilter);
		IEnumerable<Country> GetCountries(ListFilter listFilter);
		Country GetCountry(int id);
		void UpdateCountry(Country entity);

		int GetRegionsCount(ListFilter listFilter);
		IEnumerable<Region> GetRegions(ListFilter listFilter);
		Region GetRegion(int id);
		void UpdateRegion(Region entity);

		int GetSubRegionsCount(ListFilter listFilter);
		IEnumerable<SubRegion> GetSubRegions(ListFilter listFilter);
		SubRegion GetSubRegion(int id);
		void UpdateSubRegion(SubRegion entity);

		int GetPlacesCount(PlaceListFilter listFilter);
		IEnumerable<Place> GetPlaces(PlaceListFilter listFilter);
		Place GetPlace(int id);
		int CreatePlace(Place entity);
		void UpdatePlace(Place entity);

		IEnumerable<DocumentType> GetDocumentTypes();
		DocumentType GetDocumentType(int id);
		int CreateDocumentType(DocumentType entity);
		void UpdateDocumentType(DocumentType entity);

		/// <summary>
		/// Поиск мест по подстроке
		/// </summary>
		IEnumerable<Place> SearchPlaces(string term);

		// 
		IEnumerable<CurrencyRateUse> GetCurrencyRateUses();
		int CreateCurrencyRateUse(CurrencyRateUse entity);
		CurrencyRateUse GetCurrencyRateUse(int id);
		void UpdateCurrencyRateUse(CurrencyRateUse entity);
		
		// 
		IEnumerable<CurrencyRateDiff> GetCurrencyRateDiffs();
		int CreateCurrencyRateDiff(CurrencyRateDiff entity);
		CurrencyRateDiff GetCurrencyRateDiff(int id);
		void UpdateCurrencyRateDiff(CurrencyRateDiff entity);

		IEnumerable<PositionTemplate> GetPositionTemplates();
		int CreatePositionTemplate(PositionTemplate entity);
		PositionTemplate GetPositionTemplate(int id);
		void UpdatePositionTemplate(PositionTemplate entity);
		void DeletePositionTemplate(int id);

		RouteSegment GetLastRouteSegment(int fromPlaceId, int toPlaceId);

		VolumetricRatio GetVolumetricRatio(int id);
		int CreateVolumetricRatio(VolumetricRatio entity);
		void UpdateVolumetricRatio(VolumetricRatio entity);
		void DeleteVolumetricRatio(int id);

		IEnumerable<ParticipantRole> GetParticipantRoles();
		int CreateParticipantRole(ParticipantRole entity);

		// 
		IEnumerable<AutoExpense> GetAutoExpenses();
		int CreateAutoExpense(AutoExpense entity);
		AutoExpense GetAutoExpense(int id);
		void UpdateAutoExpense(AutoExpense entity);

		// 
		IEnumerable<PayMethod> GetPayMethods();
		int CreatePayMethod(PayMethod entity);
		PayMethod GetPayMethod(int id);
		void UpdatePayMethod(PayMethod entity);
	
		// 
		IEnumerable<Schedule> GetSchedules();
		int CreateSchedule(Schedule entity);
		Schedule GetSchedule(int id);
		void UpdateSchedule(Schedule entity);
		void DeleteSchedule(int id);

		//
		Event GetEvent(int id);

		// 
		IEnumerable<MailingLog> GetMailingLog(string email);
		int CreateMailingLog(MailingLog entity);
		MailingLog GetMailingLog(int id);
	}
}