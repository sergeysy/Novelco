using System;
using System.Collections.Generic;
using System.Linq;
using Logisto.Data;
using Logisto.Models;
using LinqToDB;

namespace Logisto.BusinessLogic
{
	public class DataLogic : IDataLogic
	{
		#region справочники

		public IEnumerable<VAT> GetVats()
		{
			using (var db = new LogistoDb())
			{ return db.Vats.ToList(); }
		}

		public IEnumerable<Role> GetRoles()
		{
			using (var db = new LogistoDb())
			{ return db.Roles.ToList(); }
		}

		public IEnumerable<Event> GetEvents()
		{
			using (var db = new LogistoDb())
			{ return db.Events.OrderBy(o => o.Name).ToList(); }
		}

		public IEnumerable<OperationKind> GetOperationKinds()
		{
			using (var db = new LogistoDb())
			{ return db.OperationKinds.ToList(); }
		}

		public IEnumerable<Measure> GetMeasures()
		{
			using (var db = new LogistoDb())
			{ return db.Measures.ToList(); }
		}

		public IEnumerable<TaxType> GetTaxTypes()
		{
			using (var db = new LogistoDb())
			{ return db.TaxTypes.ToList(); }
		}

		public IEnumerable<Product> GetProducts()
		{
			using (var db = new LogistoDb())
			{ return db.Products.ToList(); }
		}

		public IEnumerable<Template> GetTemplates()
		{
			using (var db = new LogistoDb())
			{ return db.Templates.ToList(); }
		}

		public IEnumerable<Currency> GetCurrencies()
		{
			using (var db = new LogistoDb())
			{ return db.Currencies.ToList(); }
		}

		public IEnumerable<PhoneType> GetPhoneTypes()
		{
			using (var db = new LogistoDb())
			{ return db.PhoneTypes.ToList(); }
		}

		public IEnumerable<FinRepCenter> GetFinRepCenters()
		{
			using (var db = new LogistoDb())
			{ return db.FinRepCenters.ToList(); }
		}

		public IEnumerable<EmployeeStatus> GetEmployeeStatuses()
		{
			using (var db = new LogistoDb())
			{ return db.EmployeeStatuses.ToList(); }
		}

		public IEnumerable<ParticipantRole> GetParticipantRoles()
		{
			using (var db = new LogistoDb())
			{ return db.ParticipantRoles.ToList(); }
		}

		public IEnumerable<OrderType> GetOrderTypes()
		{
			using (var db = new LogistoDb())
			{ return new List<OrderType> { new OrderType { ID = 0, Display = "" } }.Union(db.OrderTypes.ToList()).ToList(); }
		}

		public IEnumerable<ServiceType> GetServiceTypes()
		{
			using (var db = new LogistoDb())
			{ return new List<ServiceType> { new ServiceType { ID = 0, Name = "" } }.Union(db.ServiceTypes.ToList()).OrderBy(o => o.Name).ToList(); }
		}

		public IEnumerable<ServiceKind> GetServiceKinds()
		{
			using (var db = new LogistoDb())
			{ return db.ServiceKinds.OrderBy(o => o.Name).ToList(); }
		}

		public IEnumerable<PackageType> GetPackageTypes()
		{
			using (var db = new LogistoDb())
			{ return db.PackageTypes.OrderBy(o => o.Display).ToList(); }
		}

		public IEnumerable<OrderStatus> GetOrderStatuses()
		{
			using (var db = new LogistoDb())
			{ return db.OrderStatuses.Where(w => w.ID <= 5).Concat(db.OrderStatuses.Where(w => w.ID == 9)).Concat(db.OrderStatuses.Where(w => w.ID > 5 && w.ID < 9)).ToList(); }
		}

		public IEnumerable<ContractType> GetContractTypes()
		{
			using (var db = new LogistoDb())
			{ return db.ContractTypes.ToList(); }
		}

		public IEnumerable<ContractRole> GetContractRoles()
		{
			using (var db = new LogistoDb())
			{ return db.ContractRoles.OrderBy(o => o.Display).ToList(); }
		}

		public IEnumerable<InsuranceType> GetInsuranceTypes()
		{
			using (var db = new LogistoDb())
			{ return db.InsuranceTypes.ToList(); }
		}

		public IEnumerable<RoutePointType> GetRoutePointTypes()
		{
			using (var db = new LogistoDb())
			{ return db.RoutePointTypes.ToList(); }
		}

		public IEnumerable<UninsuranceType> GetUninsuranceTypes()
		{
			using (var db = new LogistoDb())
			{ return db.UninsuranceTypes.ToList(); }
		}

		public IEnumerable<VolumetricRatio> GetVolumetricRatios()
		{
			using (var db = new LogistoDb())
			{ return new List<VolumetricRatio> { new VolumetricRatio { ID = 0, Display = "" } }.Union(db.VolumetricRatios.ToList()).ToList(); }
		}

		public IEnumerable<CargoDescription> GetCargoDescriptions()
		{
			using (var db = new LogistoDb())
			{ return db.CargoDescriptions.OrderBy(o => o.Display).ToList(); }
		}

		public IEnumerable<SigningAuthority> GetSigningAuthorities()
		{
			using (var db = new LogistoDb())
			{ return db.SigningAuthorities.ToList(); }
		}

		public IEnumerable<ContractServiceType> GetContractServiceTypes()
		{
			using (var db = new LogistoDb())
			{ return db.ContractServiceTypes.ToList(); }
		}

		public IEnumerable<AccountingDocumentType> GetAccountingDocumentTypes()
		{
			using (var db = new LogistoDb())
			{ return db.AccountingDocumentTypes.ToList(); }
		}

		public int CreateDocumentTypeProductPrint(DocumentTypeProductPrint entity)
		{
			using (var db = new LogistoDb())
			{
				return Convert.ToInt32(db.InsertWithIdentity(entity));
			}
		}

		public void DeleteDocumentTypeProductPrint(int id)
		{
			using (var db = new LogistoDb())
			{ db.DocumentTypeProductPrints.Delete(w => w.ID == id); }
		}

		public IEnumerable<DocumentTypeProductPrint> GetDocumentTypePrints(int productId)
		{
			using (var db = new LogistoDb())
			{
				return db.DocumentTypeProductPrints.Where(w => w.ProductId == productId).ToList();
			}
		}

		public IEnumerable<DocumentTypeProductPrint> GetDocumentTypeProductPrints(int documentTypeId)
		{
			using (var db = new LogistoDb())
			{
				return db.DocumentTypeProductPrints.Where(w => w.DocumentTypeId == documentTypeId).ToList();
			}
		}

		public IEnumerable<DocumentType> GetDocumentTypes()
		{
			using (var db = new LogistoDb())
			{ return db.DocumentTypes.OrderBy(o => o.Display).ToList(); }
		}

		public IEnumerable<TemplateField> GetTemplateFields()
		{
			using (var db = new LogistoDb())
			{ return db.TemplateFields.ToList(); }
		}

		public IEnumerable<OrderTemplate> GetOrderTemplates()
		{
			using (var db = new LogistoDb())
			{ return db.OrderTemplates.ToList(); }
		}

		public IEnumerable<OrderTemplate> GetOrderTemplatesByContract(int contractId)
		{
			using (var db = new LogistoDb())
			{ return db.OrderTemplates.Where(w => w.ContractId == contractId).ToList(); }
		}

		public IEnumerable<OrderTemplateOperation> GetOrderTemplateOperations()
		{
			using (var db = new LogistoDb())
			{ return db.OrderTemplateOperations.ToList(); }
		}

		public IEnumerable<OrderOperation> GetOrderOperations()
		{
			using (var db = new LogistoDb())
			{
				return (from op in db.OrderOperations
						from o in db.OperationKinds.Where(w => w.ID == op.OperationKindId).DefaultIfEmpty()
						orderby o.Name, op.Name
						select op).ToList();
			}
		}

		public IEnumerable<OrderOperation> GetOrderOperationsByTemplate(int templateId)
		{
			using (var db = new LogistoDb())
			{
				return (from t in db.OrderTemplateOperations
						join o in db.OrderOperations on t.OrderOperationId equals o.ID
						where t.OrderTemplateId == templateId
						orderby t.No
						select o).ToList();
			}
		}

		public IEnumerable<OperationStatus> GetOperationStatuses()
		{
			using (var db = new LogistoDb())
			{
				var list = db.OperationStatuses.ToList();
				var item = list[2];
				list.RemoveAt(2);
				list.Insert(0, item);
				return list;
			}
		}

		public IEnumerable<AccountingPaymentType> GetAccountingPaymentTypes()
		{
			using (var db = new LogistoDb())
			{ return db.AccountingPaymentTypes.ToList(); }
		}

		public IEnumerable<AccountingPaymentMethod> GetAccountingPaymentMethods()
		{
			using (var db = new LogistoDb())
			{ return db.AccountingPaymentMethods.ToList(); }
		}

		public IEnumerable<TransportType> GetTransportTypes()
		{
			using (var db = new LogistoDb())
			{ return db.TransportTypes.ToList(); }
		}

		public Template GetTemplate(int id)
		{
			using (var db = new LogistoDb())
			{ return db.Templates.FirstOrDefault(w => w.ID == id); }
		}

		public void UpdateTemplate(Template template)
		{
			using (var db = new LogistoDb())
			{
				db.Templates.Where(w => w.ID == template.ID)
					.Set(s => s.Data, template.Data)
					.Set(s => s.Filename, template.Filename)
					.Set(s => s.FileSize, template.FileSize)
					.Set(s => s.Name, template.Name)
					//.Set(s => s.SqlDataSource, template.SqlDataSource)
					.Set(s => s.xlfColumns, template.xlfColumns)
					.Set(s => s.Suffix, template.Suffix)
					.Set(s => s.ListFirstColumn, template.ListFirstColumn)
					.Set(s => s.ListLastColumn, template.ListLastColumn)
					.Set(s => s.ListRow, template.ListRow)
					.Update();
			}
		}

		public TemplateField GetTemplateField(int id)
		{
			using (var db = new LogistoDb())
			{ return db.TemplateFields.FirstOrDefault(w => w.ID == id); }
		}

		public void UpdateTemplateField(TemplateField field)
		{
			using (var db = new LogistoDb())
			{
				db.TemplateFields.Where(w => w.ID == field.ID)
					.Set(s => s.Name, field.Name)
					.Set(s => s.FieldName, field.FieldName)
					.Set(s => s.IsAtable, field.IsAtable)
					.Set(s => s.Range, field.Range)
					.Update();
			}
		}

		#region dynamic

		public IEnumerable<DynamicDictionary> GetUsers()
		{
			using (var db = new LogistoDb())
			{
				return (from u in db.Users
						join p in db.Persons on u.PersonId equals p.ID
						select new DynamicDictionary { ID = u.ID, Display = p.DisplayName }).ToList();
			}
		}

		public IEnumerable<DynamicDictionary> GetActiveUsers()
		{
			using (var db = new LogistoDb())
			{
				return (from u in db.Users
						from p in db.Persons.Where(w => w.ID == u.PersonId)
						from e in db.Employees.Where(w => w.PersonId == u.PersonId)
						where e.EmployeeStatusId == 1
						select new DynamicDictionary { ID = u.ID, Display = p.DisplayName }).Distinct().OrderBy(o => o.Display).ToList();
			}
		}

		public IEnumerable<DynamicDictionary> GetPersons()
		{
			using (var db = new LogistoDb())
			{
				return db.Persons.Select(s => new DynamicDictionary { ID = s.ID, Display = s.DisplayName }).ToList();
			}
		}

		public IEnumerable<DynamicDictionary> GetOrders()
		{
			using (var db = new LogistoDb())
			{
				return db.Orders.Select(s => new DynamicDictionary { ID = s.ID, Display = s.Number }).ToList();
			}
		}

		public IEnumerable<DynamicDictionary> GetContracts()
		{
			using (var db = new LogistoDb())
			{
				return db.Contracts.Select(s => new DynamicDictionary { ID = s.ID, Display = s.Number }).ToList();
			}
		}

		public IEnumerable<DynamicDictionary> GetContractors()
		{
			using (var db = new LogistoDb())
			{
				return db.Contractors.OrderBy(o => o.Name).Select(s => new DynamicDictionary { ID = s.ID, Display = s.Name }).ToList();
			}
		}

		public IEnumerable<DynamicDictionary> GetLegals()
		{
			using (var db = new LogistoDb())
			{
				return db.Legals.OrderBy(o => o.DisplayName).Select(s => new DynamicDictionary { ID = s.ID, Display = s.DisplayName }).ToList();
			}
		}

		public IEnumerable<DynamicDictionary> GetEmployees()
		{
			using (var db = new LogistoDb())
			{
				return db.Employees.Select(s => new DynamicDictionary { ID = s.ID, Display = s.Position }).ToList();
			}
		}

		public IEnumerable<DynamicDictionary> GetBanks()
		{
			using (var db = new LogistoDb())
			{
				return db.Banks.Select(s => new DynamicDictionary { ID = s.ID, Display = s.Name }).ToList();
			}
		}

		public IEnumerable<DynamicDictionary> GetOurLegalLegals()
		{
			using (var db = new LogistoDb())
			{
				var query = from ol in db.OurLegals
							join l in db.Legals on ol.LegalId equals l.ID
							select new DynamicDictionary { ID = ol.ID, Display = l.DisplayName };

				return query.ToList();
			}
		}

		public IEnumerable<DynamicDictionary> GetOurLegals()
		{
			using (var db = new LogistoDb())
			{
				return db.OurLegals.Select(s => new DynamicDictionary { ID = s.ID, Display = s.Name }).ToList();
			}
		}

		public IEnumerable<DynamicDictionary> GetLegalProviders()
		{
			using (var db = new LogistoDb())
			{
				return (from l in db.Legals
						join c in db.Contracts on l.ID equals c.LegalId
						where c.ContractServiceTypeId == 2 || c.ContractServiceTypeId == 3
						orderby l.DisplayName
						select new DynamicDictionary { ID = l.ID, Display = l.DisplayName }).Distinct().ToList();
			}
		}

		public IEnumerable<DynamicDictionary> GetLegalsByContract()
		{
			using (var db = new LogistoDb())
			{
				return (from l in db.Legals
						join c in db.Contracts on l.ID equals c.LegalId
						select new DynamicDictionary { ID = c.ID, Display = l.DisplayName }).ToList();
			}
		}

		public IEnumerable<DynamicDictionary> GetContractorsByOrder()
		{
			using (var db = new LogistoDb())
			{
				return (from cn in db.Contractors
						join l in db.Legals on cn.ID equals l.ContractorId
						join c in db.Contracts on l.ID equals c.LegalId
						join o in db.Orders on c.ID equals o.ContractId
						select new DynamicDictionary { ID = o.ID, Display = cn.Name }).ToList();
			}
		}

		public IEnumerable<DynamicTrictionary> GetContractorsByContract()
		{
			using (var db = new LogistoDb())
			{
				return (from cn in db.Contractors
						join l in db.Legals on cn.ID equals l.ContractorId
						join c in db.Contracts on l.ID equals c.LegalId
						select new DynamicTrictionary { ID = c.ID, TargetId = cn.ID, Display = cn.Name }).ToList();
			}
		}

		#endregion

		#endregion

		public List<CurrencyRate> GetCurrencyRates(DateTime date)
		{
			List<CurrencyRate> result = new List<CurrencyRate>();
			using (var db = new LogistoDb())
			{
				var currencies = db.Currencies.Select(s => s.ID);

				foreach (var currency in currencies.ToList())
				{
					var rate = db.CurrencyRates.Where(w => w.CurrencyId == currency && w.Date <= date).OrderByDescending(o => o.Date).FirstOrDefault();
					if (rate != null)
						result.Add(rate);
				}
			}

			return result;
		}

		public Dictionary<DateTime, List<float>> GetCurrenciesRates(ListFilter filter)
		{
			var result = new Dictionary<DateTime, List<float>>();
			using (var db = new LogistoDb())
			{
				var currencies = db.Currencies.Select(s => s.ID);

				var currentDate = filter.From.Value.Date;
				while (currentDate.Date < filter.To.Value.Date)
				{
					var rates = new List<float>();
					var rateData = db.CurrencyRates.Where(w => w.Date.Value.Date == currentDate).ToList();

					foreach (var currency in currencies)
					{
						var crate = rateData.FirstOrDefault(w => w.CurrencyId == currency);
						if (crate != null)
							rates.Add(Convert.ToSingle(crate.Rate));
						else
							rates.Add(1);
					}

					result.Add(currentDate, rates);
					currentDate = currentDate.AddDays(1);
				}
			}

			return result;
		}

		public Dictionary<string, object> GetDataCounters()
		{
			using (var db = new LogistoDb())
			{
				var model = new Dictionary<string, object>();

				model.Add("TemplatesCount", db.Templates.Count());
				model.Add("CargoDescriptionsCount", db.CargoDescriptions.Count());
				model.Add("PackageTypesCount", db.PackageTypes.Count());
				model.Add("BanksCount", db.Banks.Count());
				model.Add("PaymentTermsCount", db.PaymentTerms.Count());
				model.Add("ContractRolesCount", db.ContractRoles.Count());
				model.Add("ContractTypesCount", db.ContractTypes.Count());
				model.Add("OurLegalsCount", db.OurLegals.Count());
				model.Add("UsersCount", db.Users.Count());
				model.Add("RolesCount", db.IdentityRoles.Count());
				model.Add("OrderOperationsCount", db.OrderOperations.Count());
				model.Add("OrderTemplatesCount", db.OrderTemplates.Count());
				model.Add("PositionTemplatesCount", db.PositionTemplates.Count());
				model.Add("FinRepCentersCount", db.FinRepCenters.Count());

				return model;
			}
		}


		public List<SystemSetting> GetSystemSettings()
		{
			using (var db = new LogistoDb())
			{ return db.SystemSettings.ToList(); }
		}

		public SystemSetting GetSystemSetting(string name)
		{
			using (var db = new LogistoDb())
			{ return db.SystemSettings.FirstOrDefault(w => w.Name == name); }
		}

		public void UpdateSystemSetting(SystemSetting entity)
		{
			using (var db = new LogistoDb())
			{
				db.SystemSettings.Where(w => w.ID == entity.ID)
					.Set(s => s.Value, entity.Value)
					.Update();
			}
		}

		//
		public List<SyncQueue> GetSyncQueue()
		{
			using (var db = new LogistoDb())
			{ return db.SyncQueue.ToList(); }
		}

		public int CreateSyncQueue(SyncQueue entity)
		{
			using (var db = new LogistoDb())
			{
				var sq = db.SyncQueue.FirstOrDefault(w => w.AccountingId == entity.AccountingId);
				if (sq == null)
					return Convert.ToInt32(db.InsertWithIdentity(entity));
				else
					return sq.ID;
			}
		}

		public void DeleteSyncQueue(int id)
		{
			using (var db = new LogistoDb())
			{ db.SyncQueue.Delete(w => w.ID == id); }
		}

		public void UpdateSyncQueue(SyncQueue entity)
		{
			using (var db = new LogistoDb())
			{
				db.SyncQueue.Where(w => w.ID == entity.ID)
					.Set(s => s.Error, entity.Error)
					.Update();
			}
		}

		//
		public CargoDescription GetCargoDescription(int id)
		{
			using (var db = new LogistoDb())
			{ return db.CargoDescriptions.FirstOrDefault(w => w.ID == id); }
		}

		public int CreateCargoDescription(CargoDescription entity)
		{
			using (var db = new LogistoDb())
			{
				return Convert.ToInt32(db.InsertWithIdentity(entity));
			}
		}

		public void UpdateCargoDescription(CargoDescription entity)
		{
			using (var db = new LogistoDb())
			{
				db.CargoDescriptions.Where(w => w.ID == entity.ID)
					.Set(s => s.Display, entity.Display)
					.Set(s => s.EnDisplay, entity.EnDisplay)
					.Update();
			}
		}

		//
		public PackageType GetPackageType(int id)
		{
			using (var db = new LogistoDb())
			{ return db.PackageTypes.FirstOrDefault(w => w.ID == id); }
		}

		public void UpdatePackageType(PackageType entity)
		{
			using (var db = new LogistoDb())
			{
				db.PackageTypes.Where(w => w.ID == entity.ID)
					.Set(s => s.Display, entity.Display)
					.Set(s => s.EnDisplay, entity.EnDisplay)
					.Update();
			}
		}

		//
		public FinRepCenter GetFinRepCenter(int id)
		{
			using (var db = new LogistoDb())
			{ return db.FinRepCenters.FirstOrDefault(w => w.ID == id); }
		}

		public int CreateFinRepCenter(FinRepCenter entity)
		{
			using (var db = new LogistoDb())
			{
				return Convert.ToInt32(db.InsertWithIdentity(entity));
			}
		}

		public void UpdateFinRepCenter(FinRepCenter entity)
		{
			using (var db = new LogistoDb())
			{
				db.FinRepCenters.Where(w => w.ID == entity.ID)
					.Set(s => s.Name, entity.Name)
					.Set(s => s.Code, entity.Code)
					.Set(s => s.Description, entity.Description)
					.Set(s => s.OurLegalId, entity.OurLegalId)
					.Update();
			}
		}

		//
		public PaymentTerm GetPaymentTerm(int id)
		{
			using (var db = new LogistoDb())
			{ return db.PaymentTerms.FirstOrDefault(w => w.ID == id); }
		}

		public int CreatePaymentTerm(PaymentTerm entity)
		{
			using (var db = new LogistoDb())
			{
				return Convert.ToInt32(db.InsertWithIdentity(entity));
			}
		}

		public void UpdatePaymentTerm(PaymentTerm entity)
		{
			using (var db = new LogistoDb())
			{
				db.PaymentTerms.Where(w => w.ID == entity.ID)
					.Set(s => s.Display, entity.Display)
					.Set(s => s.Condition1_BankDays, entity.Condition1_BankDays)
					.Set(s => s.Condition1_Days, entity.Condition1_Days)
					.Set(s => s.Condition1_From, entity.Condition1_From)
					.Set(s => s.Condition1_OrdersFrom, entity.Condition1_OrdersFrom)
					.Set(s => s.Condition1_OrdersTo, entity.Condition1_OrdersTo)
					.Set(s => s.Condition1_Percent, entity.Condition1_Percent)
					.Set(s => s.Condition2_BankDays, entity.Condition2_BankDays)
					.Set(s => s.Condition2_Days, entity.Condition2_Days)
					.Set(s => s.Condition2_From, entity.Condition2_From)
					.Set(s => s.Condition2_OrdersFrom, entity.Condition2_OrdersFrom)
					.Set(s => s.Condition2_OrdersTo, entity.Condition2_OrdersTo)
					.Set(s => s.Condition2_Percent, entity.Condition2_Percent)
					.Update();
			}
		}

		public IEnumerable<PaymentTerm> GetPaymentTerms()
		{
			using (var db = new LogistoDb())
			{ return db.PaymentTerms.OrderBy(o => o.Display).ToList(); }
		}

		//
		public OrderRentability GetOrderRentability(int id)
		{
			using (var db = new LogistoDb())
			{ return db.OrdersRentability.FirstOrDefault(w => w.ID == id); }
		}

		public int CreateOrderRentability(OrderRentability entity)
		{
			using (var db = new LogistoDb())
			{
				return Convert.ToInt32(db.InsertWithIdentity(entity));
			}
		}

		public void UpdateOrderRentability(OrderRentability entity)
		{
			using (var db = new LogistoDb())
			{
				db.OrdersRentability.Where(w => w.ID == entity.ID)
					.Set(s => s.OrderTemplateId, entity.OrderTemplateId)
					.Set(s => s.FinRepCenterId, entity.FinRepCenterId)
					.Set(s => s.Rentability, entity.Rentability)
					.Set(s => s.ProductId, entity.ProductId)
					.Set(s => s.Year, entity.Year)
					.Update();
			}
		}

		public IEnumerable<OrderRentability> GetOrdersRentability()
		{
			using (var db = new LogistoDb())
			{
				return db.OrdersRentability.ToList();
			}
		}

		//
		public ContractRole GetContractRole(int id)
		{
			using (var db = new LogistoDb())
			{ return db.ContractRoles.FirstOrDefault(w => w.ID == id); }
		}

		public int CreateContractRole(ContractRole entity)
		{
			using (var db = new LogistoDb())
			{
				return Convert.ToInt32(db.InsertWithIdentity(entity));
			}
		}

		public void UpdateContractRole(ContractRole entity)
		{
			using (var db = new LogistoDb())
			{
				db.ContractRoles.Where(w => w.ID == entity.ID)
					.Set(s => s.Display, entity.Display)
					.Set(s => s.AblativeName, entity.AblativeName)
					.Set(s => s.DativeName, entity.DativeName)
					.Set(s => s.EnName, entity.EnName)
					.Update();
			}
		}

		public void DeleteContractRole(int id)
		{
			using (var db = new LogistoDb())
			{
				db.ContractRoles.Delete(w => w.ID == id);
			}
		}

		//
		public ContractType GetContractType(int id)
		{
			using (var db = new LogistoDb())
			{ return db.ContractTypes.FirstOrDefault(w => w.ID == id); }
		}

		public int CreateContractType(ContractType entity)
		{
			using (var db = new LogistoDb())
			{
				return Convert.ToInt32(db.InsertWithIdentity(entity));
			}
		}

		public void UpdateContractType(ContractType entity)
		{
			using (var db = new LogistoDb())
			{
				db.ContractTypes.Where(w => w.ID == entity.ID)
					.Set(s => s.Display, entity.Display)
					.Set(s => s.ContractRoleId, entity.ContractRoleId)
					.Set(s => s.OurContractRoleId, entity.OurContractRoleId)
					.Update();
			}
		}

		//
		public Product GetProduct(int id)
		{
			using (var db = new LogistoDb())
			{ return db.Products.FirstOrDefault(w => w.ID == id); }
		}

		public int CreateProduct(Product entity)
		{
			using (var db = new LogistoDb())
			{
				return Convert.ToInt32(db.InsertWithIdentity(entity));
			}
		}

		public void UpdateProduct(Product entity)
		{
			using (var db = new LogistoDb())
			{
				db.Products.Where(w => w.ID == entity.ID)
					.Set(s => s.Display, entity.Display)
					.Set(s => s.VolumetricRatioId, entity.VolumetricRatioId)
					.Set(s => s.IsWorking, entity.IsWorking)
					.Set(s => s.ManagerUserId, entity.ManagerUserId)
					.Set(s => s.DeputyUserId, entity.DeputyUserId)
					.Update();
			}
		}

		//
		public OrderOperation GetOrderOperation(int id)
		{
			using (var db = new LogistoDb())
			{ return db.OrderOperations.FirstOrDefault(w => w.ID == id); }
		}

		public int CreateOrderOperation(OrderOperation entity)
		{
			using (var db = new LogistoDb())
			{
				return Convert.ToInt32(db.InsertWithIdentity(entity));
			}
		}

		public void UpdateOrderOperation(OrderOperation entity)
		{
			using (var db = new LogistoDb())
			{
				db.OrderOperations.Where(w => w.ID == entity.ID)
					.Set(s => s.Name, entity.Name)
					.Set(s => s.EnName, entity.EnName)
					.Set(s => s.OperationKindId, entity.OperationKindId)
					.Set(s => s.StartFactEventId, entity.StartFactEventId)
					.Set(s => s.FinishFactEventId, entity.FinishFactEventId)
					.Update();
			}
		}

		//
		public OrderTemplate GetOrderTemplate(int id)
		{
			using (var db = new LogistoDb())
			{ return db.OrderTemplates.FirstOrDefault(w => w.ID == id); }
		}

		public int CreateOrderTemplate(OrderTemplate entity)
		{
			using (var db = new LogistoDb())
			{
				return Convert.ToInt32(db.InsertWithIdentity(entity));
			}
		}

		public void UpdateOrderTemplate(OrderTemplate entity)
		{
			using (var db = new LogistoDb())
			{
				db.OrderTemplates.Where(w => w.ID == entity.ID)
					.Set(s => s.Name, entity.Name)
					.Set(s => s.ProductId, entity.ProductId)
					.Update();
			}
		}

		public void DeleteOrderTemplate(int id)
		{
			using (var db = new LogistoDb())
			{
				db.OrderTemplateOperations.Delete(w => w.OrderTemplateId == id);
				db.OrderTemplates.Delete(w => w.ID == id);
			}
		}

		//
		public OrderTemplateOperation GetOrderTemplateOperation(int orderTemplateId, int orderOperationId)
		{
			using (var db = new LogistoDb())
			{ return db.OrderTemplateOperations.FirstOrDefault(w => w.OrderTemplateId == orderTemplateId && w.OrderOperationId == orderOperationId); }
		}

		public void CreateOrderTemplateOperation(OrderTemplateOperation entity)
		{
			using (var db = new LogistoDb())
			{
				db.InsertWithIdentity(entity);
			}
		}

		public void UpdateOrderTemplateOperation(OrderTemplateOperation entity)
		{
			using (var db = new LogistoDb())
			{
				db.OrderTemplateOperations.Where(w => w.OrderTemplateId == entity.OrderTemplateId && w.OrderOperationId == entity.OrderOperationId)
					.Set(s => s.No, entity.No)
					.Update();
			}
		}

		public void DeleteOrderTemplateOperation(int orderTemplateId, int orderOperationId)
		{
			using (var db = new LogistoDb())
			{
				db.OrderTemplateOperations.Delete(w => w.OrderTemplateId == orderTemplateId && w.OrderOperationId == orderOperationId);
			}
		}

		//
		public ServiceKind GetServiceKind(int id)
		{
			using (var db = new LogistoDb())
			{ return db.ServiceKinds.FirstOrDefault(w => w.ID == id); }
		}

		public int CreateServiceKind(ServiceKind entity)
		{
			using (var db = new LogistoDb())
			{
				return Convert.ToInt32(db.InsertWithIdentity(entity));
			}
		}

		public void UpdateServiceKind(ServiceKind entity)
		{
			using (var db = new LogistoDb())
			{
				db.ServiceKinds.Where(w => w.ID == entity.ID)
					.Set(s => s.Name, entity.Name)
					.Set(s => s.EnName, entity.EnName)
					.Set(s => s.VatId, entity.VatId)
					.Update();
			}
		}

		//
		public ServiceType GetServiceType(int id)
		{
			using (var db = new LogistoDb())
			{ return db.ServiceTypes.FirstOrDefault(w => w.ID == id); }
		}

		public int CreateServiceType(ServiceType entity)
		{
			using (var db = new LogistoDb())
			{
				return Convert.ToInt32(db.InsertWithIdentity(entity));
			}
		}

		public void UpdateServiceType(ServiceType entity)
		{
			using (var db = new LogistoDb())
			{
				db.ServiceTypes.Where(w => w.ID == entity.ID)
					.Set(s => s.Name, entity.Name)
					.Set(s => s.Count, entity.Count)
					.Set(s => s.MeasureId, entity.MeasureId)
					.Set(s => s.Price, entity.Price)
					.Update();
			}
		}

		//
		public DocumentType GetDocumentType(int id)
		{
			using (var db = new LogistoDb())
			{ return db.DocumentTypes.FirstOrDefault(w => w.ID == id); }
		}

		public int CreateDocumentType(DocumentType entity)
		{
			using (var db = new LogistoDb())
			{
				return Convert.ToInt32(db.InsertWithIdentity(entity));
			}
		}

		public void UpdateDocumentType(DocumentType entity)
		{
			using (var db = new LogistoDb())
			{
				db.DocumentTypes.Where(w => w.ID == entity.ID)
					.Set(s => s.Description, entity.Description)
					.Set(s => s.Display, entity.Display)
					.Set(s => s.EnDescription, entity.EnDescription)
					.Set(s => s.IsNipVisible, entity.IsNipVisible)
					.Update();
			}
		}

		//
		public int GetCountriesCount(ListFilter filter)
		{
			using (var db = new LogistoDb())
			{
				var query = db.Countries.AsQueryable();

				// условия поиска

				if (!string.IsNullOrWhiteSpace(filter.Context))
					query = query.Where(w => w.Name.Contains(filter.Context) ||
										w.EnName.Contains(filter.Context)
										);

				return query.Count();
			}
		}

		public IEnumerable<Country> GetCountries(ListFilter filter)
		{
			using (var db = new LogistoDb())
			{
				var query = db.Countries.AsQueryable();

				// условия поиска

				if (!string.IsNullOrWhiteSpace(filter.Context))
					query = query.Where(w => w.Name.Contains(filter.Context) ||
										w.EnName.Contains(filter.Context)
										);

				// сортировка
				if (!string.IsNullOrWhiteSpace(filter.Sort))
				{
					if (string.IsNullOrWhiteSpace(filter.SortDirection))
						filter.SortDirection = "Asc";

					switch (filter.Sort)
					{
						case "ID":
							if (filter.SortDirection == "Asc")
								query = query.OrderBy(o => o.ID);
							else
								query = query.OrderByDescending(o => o.ID);

							break;

						case "Name":
							if (filter.SortDirection == "Asc")
								query = query.OrderBy(o => o.Name);
							else
								query = query.OrderByDescending(o => o.Name);

							break;
					}
				}

				// пейджинг

				if (filter.PageNumber > 0)
					query = query.Skip(filter.PageNumber * filter.PageSize);

				if (filter.PageSize > 0)
					query = query.Take(filter.PageSize);

				return query.ToList();
			}
		}

		public Country GetCountry(int id)
		{
			using (var db = new LogistoDb())
			{ return db.Countries.FirstOrDefault(w => w.ID == id); }
		}

		public void UpdateCountry(Country entity)
		{
			using (var db = new LogistoDb())
			{
				db.Countries.Where(w => w.ID == entity.ID)
					.Set(s => s.Name, entity.Name)
					.Set(s => s.EnName, entity.EnName)
					.Set(s => s.IsoCode, entity.IsoCode)
					.Update();
			}
		}

		//
		public int GetRegionsCount(ListFilter filter)
		{
			using (var db = new LogistoDb())
			{
				var query = db.Regions.AsQueryable();

				// условия поиска

				if (!string.IsNullOrWhiteSpace(filter.Context))
					query = query.Where(w => w.Name.Contains(filter.Context) ||
										w.EnName.Contains(filter.Context)
										);

				if (filter.ParentId > 0)
					query = query.Where(w => w.CountryId == filter.ParentId);

				return query.Count();
			}
		}

		public IEnumerable<Region> GetRegions(ListFilter filter)
		{
			using (var db = new LogistoDb())
			{
				var query = db.Regions.AsQueryable();

				// условия поиска

				if (!string.IsNullOrWhiteSpace(filter.Context))
					query = query.Where(w => w.Name.Contains(filter.Context) ||
										w.EnName.Contains(filter.Context)
										);

				if (filter.ParentId > 0)
					query = query.Where(w => w.CountryId == filter.ParentId);


				// сортировка
				if (!string.IsNullOrWhiteSpace(filter.Sort))
				{
					if (string.IsNullOrWhiteSpace(filter.SortDirection))
						filter.SortDirection = "Asc";

					switch (filter.Sort)
					{
						case "ID":
							if (filter.SortDirection == "Asc")
								query = query.OrderBy(o => o.ID);
							else
								query = query.OrderByDescending(o => o.ID);

							break;

						case "Name":
							if (filter.SortDirection == "Asc")
								query = query.OrderBy(o => o.Name);
							else
								query = query.OrderByDescending(o => o.Name);

							break;
					}
				}

				// пейджинг

				if (filter.PageNumber > 0)
					query = query.Skip(filter.PageNumber * filter.PageSize);

				if (filter.PageSize > 0)
					query = query.Take(filter.PageSize);

				return query.ToList();
			}
		}

		public Region GetRegion(int id)
		{
			using (var db = new LogistoDb())
			{ return db.Regions.FirstOrDefault(w => w.ID == id); }
		}

		public void UpdateRegion(Region entity)
		{
			using (var db = new LogistoDb())
			{
				db.Regions.Where(w => w.ID == entity.ID)
					.Set(s => s.Name, entity.Name)
					.Set(s => s.EnName, entity.EnName)
					.Set(s => s.IsoCode, entity.IsoCode)
					.Update();
			}
		}

		//
		public int GetSubRegionsCount(ListFilter filter)
		{
			using (var db = new LogistoDb())
			{
				var query = db.SubRegions.AsQueryable();

				// условия поиска

				if (!string.IsNullOrWhiteSpace(filter.Context))
					query = query.Where(w => w.Name.Contains(filter.Context) ||
										w.EnName.Contains(filter.Context)
										);

				if (filter.ParentId > 0)
					query = query.Where(w => w.RegionId == filter.ParentId);

				return query.Count();
			}
		}

		public IEnumerable<SubRegion> GetSubRegions(ListFilter filter)
		{
			using (var db = new LogistoDb())
			{
				var query = db.SubRegions.AsQueryable();

				// условия поиска

				if (!string.IsNullOrWhiteSpace(filter.Context))
					query = query.Where(w => w.Name.Contains(filter.Context) ||
										w.EnName.Contains(filter.Context)
										);

				if (filter.ParentId > 0)
					query = query.Where(w => w.RegionId == filter.ParentId);


				// сортировка
				if (!string.IsNullOrWhiteSpace(filter.Sort))
				{
					if (string.IsNullOrWhiteSpace(filter.SortDirection))
						filter.SortDirection = "Asc";

					switch (filter.Sort)
					{
						case "ID":
							if (filter.SortDirection == "Asc")
								query = query.OrderBy(o => o.ID);
							else
								query = query.OrderByDescending(o => o.ID);

							break;

						case "Name":
							if (filter.SortDirection == "Asc")
								query = query.OrderBy(o => o.Name);
							else
								query = query.OrderByDescending(o => o.Name);

							break;
					}
				}

				// пейджинг

				if (filter.PageNumber > 0)
					query = query.Skip(filter.PageNumber * filter.PageSize);

				if (filter.PageSize > 0)
					query = query.Take(filter.PageSize);

				return query.ToList();
			}
		}

		public SubRegion GetSubRegion(int id)
		{
			using (var db = new LogistoDb())
			{ return db.SubRegions.FirstOrDefault(w => w.ID == id); }
		}

		public void UpdateSubRegion(SubRegion entity)
		{
			using (var db = new LogistoDb())
			{
				db.SubRegions.Where(w => w.ID == entity.ID)
					.Set(s => s.Name, entity.Name)
					.Set(s => s.EnName, entity.EnName)
					.Update();
			}
		}

		//
		public int GetPlacesCount(PlaceListFilter filter)
		{
			using (var db = new LogistoDb())
			{
				var query = db.Places.AsQueryable();

				// условия поиска

				if (!string.IsNullOrWhiteSpace(filter.Context))
					query = query.Where(w => w.Name.Contains(filter.Context) ||
										w.EnName.Contains(filter.Context)
										);

				if (filter.SubRegionId > 0)
					query = query.Where(w => w.SubRegionId == filter.SubRegionId);

				if (filter.RegionId > 0)
					query = query.Where(w => w.RegionId == filter.RegionId);

				if (filter.CountryId > 0)
					query = query.Where(w => w.CountryId == filter.CountryId);

				return query.Count();
			}
		}

		public IEnumerable<Place> GetPlaces(PlaceListFilter filter)
		{
			using (var db = new LogistoDb())
			{
				var query = db.Places.AsQueryable();

				// условия поиска

				if (!string.IsNullOrWhiteSpace(filter.Context))
					query = query.Where(w => w.Name.Contains(filter.Context) ||
										w.EnName.Contains(filter.Context)
										);

				if (filter.SubRegionId > 0)
					query = query.Where(w => w.SubRegionId == filter.SubRegionId);

				if (filter.RegionId > 0)
					query = query.Where(w => w.RegionId == filter.RegionId);

				if (filter.CountryId > 0)
					query = query.Where(w => w.CountryId == filter.CountryId);

				// сортировка
				if (!string.IsNullOrWhiteSpace(filter.Sort))
				{
					if (string.IsNullOrWhiteSpace(filter.SortDirection))
						filter.SortDirection = "Asc";

					switch (filter.Sort)
					{
						case "ID":
							if (filter.SortDirection == "Asc")
								query = query.OrderBy(o => o.ID);
							else
								query = query.OrderByDescending(o => o.ID);

							break;

						case "Name":
							if (filter.SortDirection == "Asc")
								query = query.OrderBy(o => o.Name);
							else
								query = query.OrderByDescending(o => o.Name);

							break;
					}
				}

				// пейджинг

				if (filter.PageNumber > 0)
					query = query.Skip(filter.PageNumber * filter.PageSize);

				if (filter.PageSize > 0)
					query = query.Take(filter.PageSize);

				return query.ToList();
			}
		}

		public Place GetPlace(int id)
		{
			using (var db = new LogistoDb())
			{ return db.Places.FirstOrDefault(w => w.ID == id); }
		}

		public int CreatePlace(Place entity)
		{
			using (var db = new LogistoDb())
			{
				return Convert.ToInt32(db.InsertWithIdentity(entity));
			}
		}

		public void UpdatePlace(Place entity)
		{
			using (var db = new LogistoDb())
			{
				db.Places.Where(w => w.ID == entity.ID)
					.Set(s => s.Name, entity.Name)
					.Set(s => s.Airport, entity.Airport)
					.Set(s => s.EnName, entity.EnName)
					.Set(s => s.IataCode, entity.IataCode)
					.Set(s => s.IcaoCode, entity.IcaoCode)
					.Update();
			}
		}

		public IEnumerable<Place> SearchPlaces(string term)
		{
			using (var db = new LogistoDb())
			{
				return db.Places.Where(w => w.Name.Contains(term) || w.EnName.Contains(term)).ToList();
			}
		}

		//
		public IEnumerable<CurrencyRateUse> GetCurrencyRateUses()
		{
			using (var db = new LogistoDb())
			{
				return db.CurrencyRateUses.ToList();
			}
		}

		public int CreateCurrencyRateUse(CurrencyRateUse entity)
		{
			using (var db = new LogistoDb())
			{
				return Convert.ToInt32(db.InsertWithIdentity(entity));
			}
		}

		public CurrencyRateUse GetCurrencyRateUse(int id)
		{
			using (var db = new LogistoDb())
			{ return db.CurrencyRateUses.FirstOrDefault(w => w.ID == id); }
		}

		public void UpdateCurrencyRateUse(CurrencyRateUse entity)
		{
			using (var db = new LogistoDb())
			{
				db.CurrencyRateUses.Where(w => w.ID == entity.ID)
					.Set(s => s.Display, entity.Display)
					.Set(s => s.Value, entity.Value)
					.Set(s => s.IsDocumentDate, entity.IsDocumentDate)
					.Update();
			}
		}

		//
		public IEnumerable<CurrencyRateDiff> GetCurrencyRateDiffs()
		{
			using (var db = new LogistoDb())
			{
				return db.CurrencyRateDiff.ToList();
			}
		}

		public int CreateCurrencyRateDiff(CurrencyRateDiff entity)
		{
			using (var db = new LogistoDb())
			{
				return Convert.ToInt32(db.InsertWithIdentity(entity));
			}
		}

		public CurrencyRateDiff GetCurrencyRateDiff(int id)
		{
			using (var db = new LogistoDb())
			{ return db.CurrencyRateDiff.FirstOrDefault(w => w.ID == id); }
		}

		public void UpdateCurrencyRateDiff(CurrencyRateDiff entity)
		{
			using (var db = new LogistoDb())
			{
				db.CurrencyRateDiff.Where(w => w.ID == entity.ID)
					.Set(s => s.From, entity.From)
					.Set(s => s.To, entity.To)
					.Set(s => s.CNY, entity.CNY)
					.Set(s => s.EUR, entity.EUR)
					.Set(s => s.GBP, entity.GBP)
					.Set(s => s.USD, entity.USD)
					.Update();
			}
		}

		//
		public IEnumerable<PositionTemplate> GetPositionTemplates()
		{
			using (var db = new LogistoDb())
			{
				return db.PositionTemplates.OrderBy(o => o.Position).ToList();
			}
		}

		public int CreatePositionTemplate(PositionTemplate entity)
		{
			using (var db = new LogistoDb())
			{
				return Convert.ToInt32(db.InsertWithIdentity(entity));
			}
		}

		public PositionTemplate GetPositionTemplate(int id)
		{
			using (var db = new LogistoDb())
			{ return db.PositionTemplates.FirstOrDefault(w => w.ID == id); }
		}

		public void UpdatePositionTemplate(PositionTemplate entity)
		{
			using (var db = new LogistoDb())
			{
				db.PositionTemplates.Where(w => w.ID == entity.ID)
					.Set(s => s.Basis, entity.Basis)
					.Set(s => s.EnBasis, entity.EnBasis)
					.Set(s => s.EnPosition, entity.EnPosition)
					.Set(s => s.GenitivePosition, entity.GenitivePosition)
					.Set(s => s.Position, entity.Position)
					.Set(s => s.Department, entity.Department)
					.Update();
			}
		}

		public void DeletePositionTemplate(int id)
		{
			using (var db = new LogistoDb())
			{
				db.PositionTemplates.Delete(w => w.ID == id);
			}
		}

		public RouteSegment GetLastRouteSegment(int fromPlaceId, int toPlaceId)
		{
			using (var db = new LogistoDb())
			{
				return (from s in db.RouteSegments
						from rpf in db.RoutePoints.Where(wrpf => wrpf.ID == s.FromRoutePointId).DefaultIfEmpty()
						from pf in db.Places.Where(w => w.ID == rpf.PlaceId).DefaultIfEmpty()
						from rpt in db.RoutePoints.Where(wrpt => wrpt.ID == s.ToRoutePointId).DefaultIfEmpty()
						from pt in db.Places.Where(wpt => wpt.ID == rpt.PlaceId).DefaultIfEmpty()
						where pf.ID == fromPlaceId && pt.ID == toPlaceId && s.Length > 0
						orderby s.ID descending
						select s).FirstOrDefault();
			}
		}

		//
		public VolumetricRatio GetVolumetricRatio(int id)
		{
			using (var db = new LogistoDb())
			{ return db.VolumetricRatios.FirstOrDefault(w => w.ID == id); }
		}

		public int CreateVolumetricRatio(VolumetricRatio entity)
		{
			using (var db = new LogistoDb())
			{
				return Convert.ToInt32(db.InsertWithIdentity(entity));
			}
		}

		public void UpdateVolumetricRatio(VolumetricRatio entity)
		{
			using (var db = new LogistoDb())
			{
				db.VolumetricRatios.Where(w => w.ID == entity.ID)
					.Set(s => s.Value, entity.Value)
					.Set(s => s.Display, entity.Display)
					.Update();
			}
		}

		public void DeleteVolumetricRatio(int id)
		{
			using (var db = new LogistoDb())
			{
				db.VolumetricRatios.Delete(w => w.ID == id);
			}
		}

		public int CreateParticipantRole(ParticipantRole entity)
		{
			using (var db = new LogistoDb())
			{
				return Convert.ToInt32(db.InsertWithIdentity(entity));
			}
		}


		//
		public IEnumerable<AutoExpense> GetAutoExpenses()
		{
			using (var db = new LogistoDb())
			{
				return db.AutoExpenses.ToList();
			}
		}

		public int CreateAutoExpense(AutoExpense entity)
		{
			using (var db = new LogistoDb())
			{
				return Convert.ToInt32(db.InsertWithIdentity(entity));
			}
		}

		public AutoExpense GetAutoExpense(int id)
		{
			using (var db = new LogistoDb())
			{
				return db.AutoExpenses.FirstOrDefault(w => w.ID == id);
			}
		}

		public void UpdateAutoExpense(AutoExpense entity)
		{
			using (var db = new LogistoDb())
			{
				db.AutoExpenses.Where(w => w.ID == entity.ID)
					.Set(s => s.From, entity.From)
					.Set(s => s.To, entity.To)
					.Set(s => s.CNY, entity.CNY)
					.Set(s => s.EUR, entity.EUR)
					.Set(s => s.GBP, entity.GBP)
					.Set(s => s.USD, entity.USD)
					.Update();
			}
		}


		//
		public IEnumerable<PayMethod> GetPayMethods()
		{
			using (var db = new LogistoDb())
			{
				return db.PayMethods.ToList();
			}
		}

		public int CreatePayMethod(PayMethod entity)
		{
			using (var db = new LogistoDb())
			{
				return Convert.ToInt32(db.InsertWithIdentity(entity));
			}
		}

		public PayMethod GetPayMethod(int id)
		{
			using (var db = new LogistoDb())
			{
				return db.PayMethods.FirstOrDefault(w => w.ID == id);
			}
		}

		public void UpdatePayMethod(PayMethod entity)
		{
			using (var db = new LogistoDb())
			{
				db.PayMethods.Where(w => w.ID == entity.ID)
					.Set(s => s.From, entity.From)
					.Set(s => s.To, entity.To)
					.Set(s => s.Display, entity.Display)
					.Update();
			}
		}


		//
		public IEnumerable<Schedule> GetSchedules()
		{
			using (var db = new LogistoDb())
			{
				return db.Schedule.ToList();
			}
		}

		public int CreateSchedule(Schedule entity)
		{
			using (var db = new LogistoDb())
			{
				return Convert.ToInt32(db.InsertWithIdentity(entity));
			}
		}

		public Schedule GetSchedule(int id)
		{
			using (var db = new LogistoDb())
			{
				return db.Schedule.FirstOrDefault(w => w.ID == id);
			}
		}

		public void UpdateSchedule(Schedule entity)
		{
			using (var db = new LogistoDb())
			{
				db.Schedule.Where(w => w.ID == entity.ID)
					.Set(s => s.Weekday, entity.Weekday)
					.Set(s => s.Hour, entity.Hour)
					.Set(s => s.Minute, entity.Minute)
					.Set(s => s.ReportName, entity.ReportName)
					.Update();
			}
		}

		public void DeleteSchedule(int id)
		{
			using (var db = new LogistoDb())
			{
				db.Schedule.Delete(w => w.ID == id);
			}
		}

		//
		public Event GetEvent(int id)
		{
			using (var db = new LogistoDb())
			{
				return db.Events.FirstOrDefault(w => w.ID == id);
			}
		}


		//
		public IEnumerable<MailingLog> GetMailingLog(string email)
		{
			using (var db = new LogistoDb())
			{
				return db.MailingLog.Where(w => w.To.Contains(email)).OrderByDescending(o => o.Date).Take(1000).ToList();
			}
		}

		public int CreateMailingLog(MailingLog entity)
		{
			using (var db = new LogistoDb())
			{
				return Convert.ToInt32(db.InsertWithIdentity(entity));
			}
		}

		public MailingLog GetMailingLog(int id)
		{
			using (var db = new LogistoDb())
			{
				return db.MailingLog.FirstOrDefault(w => w.ID == id);
			}
		}
	}
}