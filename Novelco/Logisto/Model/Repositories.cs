using System.Data;
using Logisto.Models;
using MicroOrm.Dapper.Repositories;
using MicroOrm.Dapper.Repositories.SqlGenerator;

namespace Logisto.Data
{

	public partial class SubRegionRepository : DapperRepository<SubRegion>
	{
		public SubRegionRepository(IDbConnection connection, ISqlGenerator<SubRegion> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class ContactRepository : DapperRepository<Contractor>
	{
		public ContactRepository(IDbConnection connection, ISqlGenerator<Contractor> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class RoleRepository : DapperRepository<Role>
	{
		public RoleRepository(IDbConnection connection, ISqlGenerator<Role> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class UserRepository : DapperRepository<User>
	{
		public UserRepository(IDbConnection connection, ISqlGenerator<User> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class udalittblEdinicyIzmereniyaRepository : DapperRepository<udalittblEdinicyIzmereniya>
	{
		public udalittblEdinicyIzmereniyaRepository(IDbConnection connection, ISqlGenerator<udalittblEdinicyIzmereniya> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class BanksRFswiftRepository : DapperRepository<BanksRFswift>
	{
		public BanksRFswiftRepository(IDbConnection connection, ISqlGenerator<BanksRFswift> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class ZakazEconomicaDocLangRepository : DapperRepository<ZakazEconomicaDocLang>
	{
		public ZakazEconomicaDocLangRepository(IDbConnection connection, ISqlGenerator<ZakazEconomicaDocLang> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class ContactManagerRepository : DapperRepository<ContactManager>
	{
		public ContactManagerRepository(IDbConnection connection, ISqlGenerator<ContactManager> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class RolePermissionRepository : DapperRepository<RolePermission>
	{
		public RolePermissionRepository(IDbConnection connection, ISqlGenerator<RolePermission> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class PlaceRepository : DapperRepository<Place>
	{
		public PlaceRepository(IDbConnection connection, ISqlGenerator<Place> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class BanksRFRepository : DapperRepository<Bank>
	{
		public BanksRFRepository(IDbConnection connection, ISqlGenerator<Bank> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class qC1forOLAPRepository : DapperRepository<qC1forOLAP>
	{
		public qC1forOLAPRepository(IDbConnection connection, ISqlGenerator<qC1forOLAP> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class ZakazDoc1Repository : DapperRepository<ZakazDoc1>
	{
		public ZakazDoc1Repository(IDbConnection connection, ISqlGenerator<ZakazDoc1> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class ZakazEconomOplataTypeRepository : DapperRepository<ZakazEconomOplataType>
	{
		public ZakazEconomOplataTypeRepository(IDbConnection connection, ISqlGenerator<ZakazEconomOplataType> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class udalittblKontragentTypeRepository : DapperRepository<udalittblKontragentType>
	{
		public udalittblKontragentTypeRepository(IDbConnection connection, ISqlGenerator<udalittblKontragentType> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class qTempBankRepository : DapperRepository<qTempBank>
	{
		public qTempBankRepository(IDbConnection connection, ISqlGenerator<qTempBank> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class qDohodRashodRepository : DapperRepository<qDohodRashod>
	{
		public qDohodRashodRepository(IDbConnection connection, ISqlGenerator<qDohodRashod> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class ZakazDoc2Repository : DapperRepository<ZakazDoc2>
	{
		public ZakazDoc2Repository(IDbConnection connection, ISqlGenerator<ZakazDoc2> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class qDohodRashod1Repository : DapperRepository<qDohodRashod1>
	{
		public qDohodRashod1Repository(IDbConnection connection, ISqlGenerator<qDohodRashod1> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class CurrencyRepository : DapperRepository<Currency>
	{
		public CurrencyRepository(IDbConnection connection, ISqlGenerator<Currency> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class ZakazDoc3Repository : DapperRepository<ZakazDoc3>
	{
		public ZakazDoc3Repository(IDbConnection connection, ISqlGenerator<ZakazDoc3> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class KontragentDocRepository : DapperRepository<KontragentDoc>
	{
		public KontragentDocRepository(IDbConnection connection, ISqlGenerator<KontragentDoc> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class DorovoryRepository : DapperRepository<Dorovory>
	{
		public DorovoryRepository(IDbConnection connection, ISqlGenerator<Dorovory> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class ZakazEconomOplataMetodRepository : DapperRepository<ZakazEconomOplataMetod>
	{
		public ZakazEconomOplataMetodRepository(IDbConnection connection, ISqlGenerator<ZakazEconomOplataMetod> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class udalytTempRepository : DapperRepository<udalytTemp>
	{
		public udalytTempRepository(IDbConnection connection, ISqlGenerator<udalytTemp> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class KontragentDocTypeRepository : DapperRepository<KontragentDocType>
	{
		public KontragentDocTypeRepository(IDbConnection connection, ISqlGenerator<KontragentDocType> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class qZakazStrahRepository : DapperRepository<qZakazStrah>
	{
		public qZakazStrahRepository(IDbConnection connection, ISqlGenerator<qZakazStrah> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class CountryRepository : DapperRepository<Country>
	{
		public CountryRepository(IDbConnection connection, ISqlGenerator<Country> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class OurULRepository : DapperRepository<OurUL>
	{
		public OurULRepository(IDbConnection connection, ISqlGenerator<OurUL> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class qTempBezRegionovPlaceRepository : DapperRepository<qTempBezRegionovPlace>
	{
		public qTempBezRegionovPlaceRepository(IDbConnection connection, ISqlGenerator<qTempBezRegionovPlace> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class ZakazEconomUslugaMainRepository : DapperRepository<ZakazEconomUslugaMain>
	{
		public ZakazEconomUslugaMainRepository(IDbConnection connection, ISqlGenerator<ZakazEconomUslugaMain> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class ZakazEconomicaRepository : DapperRepository<ZakazEconomica>
	{
		public ZakazEconomicaRepository(IDbConnection connection, ISqlGenerator<ZakazEconomica> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class ZakazMarshrutTransportRepository : DapperRepository<ZakazMarshrutTransport>
	{
		public ZakazMarshrutTransportRepository(IDbConnection connection, ISqlGenerator<ZakazMarshrutTransport> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class ZakazEconomicaMetkiRepository : DapperRepository<ZakazEconomicaMetki>
	{
		public ZakazEconomicaMetkiRepository(IDbConnection connection, ISqlGenerator<ZakazEconomicaMetki> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class DogovorOplataRepository : DapperRepository<DogovorOplata>
	{
		public DogovorOplataRepository(IDbConnection connection, ISqlGenerator<DogovorOplata> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class SysXLSformRepository : DapperRepository<SysXLSform>
	{
		public SysXLSformRepository(IDbConnection connection, ISqlGenerator<SysXLSform> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class PravoPodpisiRepository : DapperRepository<PravoPodpisi>
	{
		public PravoPodpisiRepository(IDbConnection connection, ISqlGenerator<PravoPodpisi> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class ReestrOfPayRepository : DapperRepository<ReestrOfPay>
	{
		public ReestrOfPayRepository(IDbConnection connection, ISqlGenerator<ReestrOfPay> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class ZakazEconomDocDocTypeRepository : DapperRepository<ZakazEconomDocDocType>
	{
		public ZakazEconomDocDocTypeRepository(IDbConnection connection, ISqlGenerator<ZakazEconomDocDocType> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class ZakazEconomDocRepository : DapperRepository<ZakazEconomDoc>
	{
		public ZakazEconomDocRepository(IDbConnection connection, ISqlGenerator<ZakazEconomDoc> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class DogTypeRepository : DapperRepository<DogType>
	{
		public DogTypeRepository(IDbConnection connection, ISqlGenerator<DogType> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class PhoneTypeRepository : DapperRepository<PhoneType>
	{
		public PhoneTypeRepository(IDbConnection connection, ISqlGenerator<PhoneType> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class ULRepository : DapperRepository<Legal>
	{
		public ULRepository(IDbConnection connection, ISqlGenerator<Legal> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class PayImportDataRepository : DapperRepository<PayImportData>
	{
		public PayImportDataRepository(IDbConnection connection, ISqlGenerator<PayImportData> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class ZakazMarshrutKontaktRepository : DapperRepository<ZakazMarshrutKontakt>
	{
		public ZakazMarshrutKontaktRepository(IDbConnection connection, ISqlGenerator<ZakazMarshrutKontakt> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class ZakazEconomUslugaListRepository : DapperRepository<ZakazEconomUslugaList>
	{
		public ZakazEconomUslugaListRepository(IDbConnection connection, ISqlGenerator<ZakazEconomUslugaList> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class LegalPhoneRepository : DapperRepository<LegalPhone>
	{
		public LegalPhoneRepository(IDbConnection connection, ISqlGenerator<LegalPhone> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class qAdmZakazDogIdToEconomRepository : DapperRepository<qAdmZakazDogIdToEconom>
	{
		public qAdmZakazDogIdToEconomRepository(IDbConnection connection, ISqlGenerator<qAdmZakazDogIdToEconom> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class ZakazEconomDocPrintTableRepository : DapperRepository<ZakazEconomDocPrintTable>
	{
		public ZakazEconomDocPrintTableRepository(IDbConnection connection, ISqlGenerator<ZakazEconomDocPrintTable> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class qAdmZakazDogIdToEconomTempRepository : DapperRepository<qAdmZakazDogIdToEconomTemp>
	{
		public qAdmZakazDogIdToEconomTempRepository(IDbConnection connection, ISqlGenerator<qAdmZakazDogIdToEconomTemp> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class qZakazUslugiRepository : DapperRepository<qZakazUslugi>
	{
		public qZakazUslugiRepository(IDbConnection connection, ISqlGenerator<qZakazUslugi> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class DogDocRepository : DapperRepository<DogDoc>
	{
		public DogDocRepository(IDbConnection connection, ISqlGenerator<DogDoc> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class qSumRepository : DapperRepository<qSum>
	{
		public qSumRepository(IDbConnection connection, ISqlGenerator<qSum> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class qTempXLSformRepository : DapperRepository<qTempXLSform>
	{
		public qTempXLSformRepository(IDbConnection connection, ISqlGenerator<qTempXLSform> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class VATRepository : DapperRepository<VAT>
	{
		public VATRepository(IDbConnection connection, ISqlGenerator<VAT> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class TempEconomDocsToRepairRepository : DapperRepository<TempEconomDocsToRepair>
	{
		public TempEconomDocsToRepairRepository(IDbConnection connection, ISqlGenerator<TempEconomDocsToRepair> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class ZakazMarshrutPunktRepository : DapperRepository<ZakazMarshrutPunkt>
	{
		public ZakazMarshrutPunktRepository(IDbConnection connection, ISqlGenerator<ZakazMarshrutPunkt> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class FlPhoneRepository : DapperRepository<PersonPhone>
	{
		public FlPhoneRepository(IDbConnection connection, ISqlGenerator<PersonPhone> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class qZakazStrahwithtblKurRepository : DapperRepository<qZakazStrahwithtblKur>
	{
		public qZakazStrahwithtblKurRepository(IDbConnection connection, ISqlGenerator<qZakazStrahwithtblKur> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class ZakazUchastnikiRepository : DapperRepository<ZakazUchastniki>
	{
		public ZakazUchastnikiRepository(IDbConnection connection, ISqlGenerator<ZakazUchastniki> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class vwaspnetApplicationRepository : DapperRepository<vwaspnetApplication>
	{
		public vwaspnetApplicationRepository(IDbConnection connection, ISqlGenerator<vwaspnetApplication> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class vwaspnetUserRepository : DapperRepository<vwaspnetUser>
	{
		public vwaspnetUserRepository(IDbConnection connection, ISqlGenerator<vwaspnetUser> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class qZakazSchetRashodRepository : DapperRepository<qZakazSchetRashod>
	{
		public qZakazSchetRashodRepository(IDbConnection connection, ISqlGenerator<qZakazSchetRashod> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class CurrencyForInvoiceRepository : DapperRepository<CurrencyForInvoice>
	{
		public CurrencyForInvoiceRepository(IDbConnection connection, ISqlGenerator<CurrencyForInvoice> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class UdalittblPerevozchikRepository : DapperRepository<UdalittblPerevozchik>
	{
		public UdalittblPerevozchikRepository(IDbConnection connection, ISqlGenerator<UdalittblPerevozchik> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class ZakazXLSformRepository : DapperRepository<ZakazXLSform>
	{
		public ZakazXLSformRepository(IDbConnection connection, ISqlGenerator<ZakazXLSform> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class ZakazRepository : DapperRepository<Zakaz>
	{
		public ZakazRepository(IDbConnection connection, ISqlGenerator<Zakaz> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class ZakazTypeRepository : DapperRepository<ZakazType>
	{
		public ZakazTypeRepository(IDbConnection connection, ISqlGenerator<ZakazType> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class AccountRepository : DapperRepository<BankAccount>
	{
		public AccountRepository(IDbConnection connection, ISqlGenerator<BankAccount> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class ZakazStatuRepository : DapperRepository<ZakazStatus>
	{
		public ZakazStatuRepository(IDbConnection connection, ISqlGenerator<ZakazStatus> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class UdalittblCargoRepository : DapperRepository<UdalittblCargo>
	{
		public UdalittblCargoRepository(IDbConnection connection, ISqlGenerator<UdalittblCargo> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class ZakazProductRepository : DapperRepository<ZakazProduct>
	{
		public ZakazProductRepository(IDbConnection connection, ISqlGenerator<ZakazProduct> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class ZakazXLSdataRepository : DapperRepository<ZakazXLSdata>
	{
		public ZakazXLSdataRepository(IDbConnection connection, ISqlGenerator<ZakazXLSdata> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class DogVidRepository : DapperRepository<DogVid>
	{
		public DogVidRepository(IDbConnection connection, ISqlGenerator<DogVid> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class qvpKursEURRepository : DapperRepository<qvpKursEUR>
	{
		public qvpKursEURRepository(IDbConnection connection, ISqlGenerator<qvpKursEUR> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class qvpKursUSDRepository : DapperRepository<qvpKursUSD>
	{
		public qvpKursUSDRepository(IDbConnection connection, ISqlGenerator<qvpKursUSD> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class ULtypicalPositionSetRepository : DapperRepository<ULtypicalPositionSet>
	{
		public ULtypicalPositionSetRepository(IDbConnection connection, ISqlGenerator<ULtypicalPositionSet> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class qKursRateRepository : DapperRepository<qKursRate>
	{
		public qKursRateRepository(IDbConnection connection, ISqlGenerator<qKursRate> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class ZakazGruzDescriptionRepository : DapperRepository<ZakazGruzDescription>
	{
		public ZakazGruzDescriptionRepository(IDbConnection connection, ISqlGenerator<ZakazGruzDescription> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class ZakazEconomDocsFileRepository : DapperRepository<ZakazEconomDocsFile>
	{
		public ZakazEconomDocsFileRepository(IDbConnection connection, ISqlGenerator<ZakazEconomDocsFile> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class ZakazGruzKoefRepository : DapperRepository<ZakazGruzKoef>
	{
		public ZakazGruzKoefRepository(IDbConnection connection, ISqlGenerator<ZakazGruzKoef> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class udalittblKontragentRepository : DapperRepository<udalittblKontragent>
	{
		public udalittblKontragentRepository(IDbConnection connection, ISqlGenerator<udalittblKontragent> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class KontragentUrTypeRepository : DapperRepository<KontragentUrType>
	{
		public KontragentUrTypeRepository(IDbConnection connection, ISqlGenerator<KontragentUrType> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class qTempBezRegionovRepository : DapperRepository<qTempBezRegionov>
	{
		public qTempBezRegionovRepository(IDbConnection connection, ISqlGenerator<qTempBezRegionov> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class ZakazStrahTypeRepository : DapperRepository<ZakazStrahType>
	{
		public ZakazStrahTypeRepository(IDbConnection connection, ISqlGenerator<ZakazStrahType> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class XLSFormsListRepository : DapperRepository<XLSFormsList>
	{
		public XLSFormsListRepository(IDbConnection connection, ISqlGenerator<XLSFormsList> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class ZakazBezStrahRepository : DapperRepository<ZakazBezStrah>
	{
		public ZakazBezStrahRepository(IDbConnection connection, ISqlGenerator<ZakazBezStrah> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class vwaspnetMembershipUserRepository : DapperRepository<vwaspnetMembershipUser>
	{
		public vwaspnetMembershipUserRepository(IDbConnection connection, ISqlGenerator<vwaspnetMembershipUser> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class qDohodRashodPaymentRepository : DapperRepository<qDohodRashodPayment>
	{
		public qDohodRashodPaymentRepository(IDbConnection connection, ISqlGenerator<qDohodRashodPayment> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class qZakazEconomicaPaymentRepository : DapperRepository<qZakazEconomicaPayment>
	{
		public qZakazEconomicaPaymentRepository(IDbConnection connection, ISqlGenerator<qZakazEconomicaPayment> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class UdalittblGorodaRepository : DapperRepository<UdalittblGoroda>
	{
		public UdalittblGorodaRepository(IDbConnection connection, ISqlGenerator<UdalittblGoroda> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class qZakazStraholdRepository : DapperRepository<qZakazStrahold>
	{
		public qZakazStraholdRepository(IDbConnection connection, ISqlGenerator<qZakazStrahold> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class qZakazClientRepository : DapperRepository<qZakazClient>
	{
		public qZakazClientRepository(IDbConnection connection, ISqlGenerator<qZakazClient> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class PayImportSheetRepository : DapperRepository<PayImportSheet>
	{
		public PayImportSheetRepository(IDbConnection connection, ISqlGenerator<PayImportSheet> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class UdalittblPerevozkiRepository : DapperRepository<UdalittblPerevozki>
	{
		public UdalittblPerevozkiRepository(IDbConnection connection, ISqlGenerator<UdalittblPerevozki> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class ZakazGruzUpakovkaTypeRepository : DapperRepository<ZakazGruzUpakovkaType>
	{
		public ZakazGruzUpakovkaTypeRepository(IDbConnection connection, ISqlGenerator<ZakazGruzUpakovkaType> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class PaymentRepository : DapperRepository<Payment>
	{
		public PaymentRepository(IDbConnection connection, ISqlGenerator<Payment> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class KurRepository : DapperRepository<Kur>
	{
		public KurRepository(IDbConnection connection, ISqlGenerator<Kur> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class vwaspnetProfileRepository : DapperRepository<vwaspnetProfile>
	{
		public vwaspnetProfileRepository(IDbConnection connection, ISqlGenerator<vwaspnetProfile> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class ZakazMarshrutRepository : DapperRepository<ZakazMarshrut>
	{
		public ZakazMarshrutRepository(IDbConnection connection, ISqlGenerator<ZakazMarshrut> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class ZakazMarshrutPunktTypeRepository : DapperRepository<ZakazMarshrutPunktType>
	{
		public ZakazMarshrutPunktTypeRepository(IDbConnection connection, ISqlGenerator<ZakazMarshrutPunktType> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class qC1forOLAPbackUpRepository : DapperRepository<qC1forOLAPbackUp>
	{
		public qC1forOLAPbackUpRepository(IDbConnection connection, ISqlGenerator<qC1forOLAPbackUp> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class View1Repository : DapperRepository<View1>
	{
		public View1Repository(IDbConnection connection, ISqlGenerator<View1> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class qC1forGGiRepository : DapperRepository<qC1forGGi>
	{
		public qC1forGGiRepository(IDbConnection connection, ISqlGenerator<qC1forGGi> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class qC1forGGIrashodRepository : DapperRepository<qC1forGGIrashod>
	{
		public qC1forGGIrashodRepository(IDbConnection connection, ISqlGenerator<qC1forGGIrashod> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class TempqUlByDogVidRepository : DapperRepository<TempqUlByDogVid>
	{
		public TempqUlByDogVidRepository(IDbConnection connection, ISqlGenerator<TempqUlByDogVid> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class RegionRepository : DapperRepository<Region>
	{
		public RegionRepository(IDbConnection connection, ISqlGenerator<Region> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class EmployeeRepository : DapperRepository<Employee>
	{
		public EmployeeRepository(IDbConnection connection, ISqlGenerator<Employee> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class ZakazEconomicaMarshrutRepository : DapperRepository<ZakazEconomicaMarshrut>
	{
		public ZakazEconomicaMarshrutRepository(IDbConnection connection, ISqlGenerator<ZakazEconomicaMarshrut> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class PostalCodeRepository : DapperRepository<PostalCode>
	{
		public PostalCodeRepository(IDbConnection connection, ISqlGenerator<PostalCode> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class OKEIRepository : DapperRepository<OKEI>
	{
		public OKEIRepository(IDbConnection connection, ISqlGenerator<OKEI> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class SysVariableRepository : DapperRepository<SysVariable>
	{
		public SysVariableRepository(IDbConnection connection, ISqlGenerator<SysVariable> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class UdalittblZakazGruzRepository : DapperRepository<UdalittblZakazGruz>
	{
		public UdalittblZakazGruzRepository(IDbConnection connection, ISqlGenerator<UdalittblZakazGruz> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class qZakazRashodRepository : DapperRepository<qZakazRashod>
	{
		public qZakazRashodRepository(IDbConnection connection, ISqlGenerator<qZakazRashod> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class vwaspnetRoleRepository : DapperRepository<vwaspnetRole>
	{
		public vwaspnetRoleRepository(IDbConnection connection, ISqlGenerator<vwaspnetRole> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class vwaspnetUsersInRoleRepository : DapperRepository<vwaspnetUsersInRole>
	{
		public vwaspnetUsersInRoleRepository(IDbConnection connection, ISqlGenerator<vwaspnetUsersInRole> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class qVATRepository : DapperRepository<qVAT>
	{
		public qVATRepository(IDbConnection connection, ISqlGenerator<qVAT> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class SysProcessRepository : DapperRepository<SysProcess>
	{
		public SysProcessRepository(IDbConnection connection, ISqlGenerator<SysProcess> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class ZakazGruzMestaRepository : DapperRepository<ZakazGruzMesta>
	{
		public ZakazGruzMestaRepository(IDbConnection connection, ISqlGenerator<ZakazGruzMesta> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class qTempIATARepository : DapperRepository<qTempIATA>
	{
		public qTempIATARepository(IDbConnection connection, ISqlGenerator<qTempIATA> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class SysProcessLogRepository : DapperRepository<SysProcessLog>
	{
		public SysProcessLogRepository(IDbConnection connection, ISqlGenerator<SysProcessLog> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class PersonRepository : DapperRepository<Person>
	{
		public PersonRepository(IDbConnection connection, ISqlGenerator<Person> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class ZakazEconomUslugaRepository : DapperRepository<ZakazEconomUsluga>
	{
		public ZakazEconomUslugaRepository(IDbConnection connection, ISqlGenerator<ZakazEconomUsluga> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class ZakazEconomicaXLSformRepository : DapperRepository<ZakazEconomicaXLSform>
	{
		public ZakazEconomicaXLSformRepository(IDbConnection connection, ISqlGenerator<ZakazEconomicaXLSform> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class ULnalogFormRepository : DapperRepository<ULnalogForm>
	{
		public ULnalogFormRepository(IDbConnection connection, ISqlGenerator<ULnalogForm> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class qGruzInfoRepository : DapperRepository<qGruzInfo>
	{
		public qGruzInfoRepository(IDbConnection connection, ISqlGenerator<qGruzInfo> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class qGruzUpakovkaRepository : DapperRepository<qGruzUpakovka>
	{
		public qGruzUpakovkaRepository(IDbConnection connection, ISqlGenerator<qGruzUpakovka> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class SysXLSformsRefRepository : DapperRepository<SysXLSformsRef>
	{
		public SysXLSformsRefRepository(IDbConnection connection, ISqlGenerator<SysXLSformsRef> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class PhoneRepository : DapperRepository<Phone>
	{
		public PhoneRepository(IDbConnection connection, ISqlGenerator<Phone> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class ZakazEconomDocTypeRepository : DapperRepository<ZakazEconomDocType>
	{
		public ZakazEconomDocTypeRepository(IDbConnection connection, ISqlGenerator<ZakazEconomDocType> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class ZakazEconomicaXLSdataRepository : DapperRepository<ZakazEconomicaXLSdata>
	{
		public ZakazEconomicaXLSdataRepository(IDbConnection connection, ISqlGenerator<ZakazEconomicaXLSdata> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class ZakazEconomicaTypeRepository : DapperRepository<ZakazEconomicaType>
	{
		public ZakazEconomicaTypeRepository(IDbConnection connection, ISqlGenerator<ZakazEconomicaType> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}


	public partial class qZakazSchetRepository : DapperRepository<qZakazSchet>
	{
		public qZakazSchetRepository(IDbConnection connection, ISqlGenerator<qZakazSchet> sqlGenerator)
			: base(connection, sqlGenerator)
		{ }
	}

}