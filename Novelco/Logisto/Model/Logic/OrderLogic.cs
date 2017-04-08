using System;
using System.Collections.Generic;
using System.Linq;
using Logisto.Data;
using Logisto.Models;
using LinqToDB;
using Logisto.ViewModels;

namespace Logisto.BusinessLogic
{
	public class OrderLogic : IOrderLogic
	{
		#region orders

		public IEnumerable<Order> GetAllOrders()
		{
			using (var db = new LogistoDb())
			{
				return db.Orders.ToList();
			}
		}

		public int GetOrdersCount(ListFilter filter)
		{
			using (var db = new LogistoDb())
			{
				var query = from o in db.Orders
							join w in db.Participants on o.ID equals w.OrderId
							where w.UserId == filter.UserId
							select o;

				// условия поиска

				if (!string.IsNullOrWhiteSpace(filter.Context))
				{
					var contracts = db.Contracts.Where(w => w.Number.Contains(filter.Context)).Select(s => s.ID).ToList();
					var products = db.Products.Where(w => w.Display.Contains(filter.Context)).Select(s => s.ID).ToList();

					// по юрлицу и контрагенту
					contracts = contracts.Union((from cn in db.Contractors
												 join l in db.Legals on cn.ID equals l.ContractorId
												 join c in db.Contracts on l.ID equals c.LegalId
												 where cn.Name.Contains(filter.Context) || l.DisplayName.Contains(filter.Context)
												 select c.ID)).ToList();

					query = query.Where(w => w.CargoInfo.Contains(filter.Context) ||
						w.Number.Contains(filter.Context) ||
						w.Comment.Contains(filter.Context) ||
						w.From.Contains(filter.Context) ||
						w.To.Contains(filter.Context) ||
						// экстра-поиск
						contracts.Contains(w.ContractId.Value) ||
						products.Contains(w.ProductId)
						);
				}

				if (filter.Statuses.Count > 0)
					query = query.Where(w => filter.Statuses.Contains(w.OrderStatusId));


				return query.Distinct().Count();
			}
		}

		public IEnumerable<Order> GetOrders(ListFilter filter)
		{
			using (var db = new LogistoDb())
			{
				var query = from o in db.Orders
							join w in db.Participants on o.ID equals w.OrderId
							from c in db.Contracts.Where(wc => wc.ID == o.ContractId).DefaultIfEmpty()
							from l in db.Legals.Where(wl => wl.ID == c.LegalId).DefaultIfEmpty()
							from cn in db.Contractors.Where(wcn => wcn.ID == l.ContractorId).DefaultIfEmpty()
							from p in db.Products.Where(wp => wp.ID == o.ProductId).DefaultIfEmpty()
							where w.UserId == filter.UserId
							select new { o, c, p, l, cn };

				// условия поиска

				if (!string.IsNullOrWhiteSpace(filter.Context))
				{
					var contracts = db.Contracts.Where(w => w.Number.Contains(filter.Context)).Select(s => s.ID).ToList();

					// по юрлицу и контрагенту
					contracts = contracts.Union((from cn in db.Contractors
												 join l in db.Legals on cn.ID equals l.ContractorId
												 join c in db.Contracts on l.ID equals c.LegalId
												 where cn.Name.Contains(filter.Context) || l.DisplayName.Contains(filter.Context)
												 select c.ID)).ToList();

					query = query.Where(w => w.o.CargoInfo.Contains(filter.Context) ||
						w.o.Number.Contains(filter.Context) ||
						w.o.Comment.Contains(filter.Context) ||
						w.o.From.Contains(filter.Context) ||
						w.o.To.Contains(filter.Context) ||
						// экстра-поиск
						w.cn.Name.Contains(filter.Context) ||
						w.c.Number.Contains(filter.Context) ||
						w.l.DisplayName.Contains(filter.Context) ||
						w.p.Display.Contains(filter.Context)
						);
				}

				if (filter.Statuses.Count > 0)
					query = query.Where(w => filter.Statuses.Contains(w.o.OrderStatusId));

				// сортировка

				if (!string.IsNullOrWhiteSpace(filter.Sort))
				{
					if (string.IsNullOrWhiteSpace(filter.SortDirection))
						filter.SortDirection = "Asc";

					switch (filter.Sort)
					{
						case "Number":  // TEMP:
						case "ID":
							if (filter.SortDirection == "Asc")
								query = query.OrderBy(o => o.o.ID);
							else
								query = query.OrderByDescending(o => o.o.ID);

							break;

						case "OrderStatusId":
							if (filter.SortDirection == "Asc")
								query = query.OrderBy(o => o.o.OrderStatusId);
							else
								query = query.OrderByDescending(o => o.o.OrderStatusId);

							break;


						case "ClosedDate":
							if (filter.SortDirection == "Asc")
								query = query.OrderBy(o => o.o.ClosedDate);
							else
								query = query.OrderByDescending(o => o.o.ClosedDate);

							break;

						case "CreatedDate":
							if (filter.SortDirection == "Asc")
								query = query.OrderBy(o => o.o.CreatedDate);
							else
								query = query.OrderByDescending(o => o.o.CreatedDate);

							break;

						case "Balance":
							if (filter.SortDirection == "Asc")
								query = query.OrderBy(o => o.o.Balance);
							else
								query = query.OrderByDescending(o => o.o.Balance);

							break;

						case "Volume":
							if (filter.SortDirection == "Asc")
								query = query.OrderBy(o => o.o.Volume);
							else
								query = query.OrderByDescending(o => o.o.Volume);

							break;

						case "GrossWeight":
							if (filter.SortDirection == "Asc")
								query = query.OrderBy(o => o.o.GrossWeight);
							else
								query = query.OrderByDescending(o => o.o.GrossWeight);

							break;

						case "ProductId":
							if (filter.SortDirection == "Asc")
								query = query.OrderBy(o => o.p.Display);
							else
								query = query.OrderByDescending(o => o.p.Display);

							break;

						case "Contractor":
							if (filter.SortDirection == "Asc")
								query = query.OrderBy(o => o.cn.Name);
							else
								query = query.OrderByDescending(o => o.cn.Name);

							break;

						case "Legal":
							if (filter.SortDirection == "Asc")
								query = query.OrderBy(o => o.l.DisplayName);
							else
								query = query.OrderByDescending(o => o.l.DisplayName);

							break;

						case "ContractId":
							if (filter.SortDirection == "Asc")
								query = query.OrderBy(o => o.c.Number);
							else
								query = query.OrderByDescending(o => o.c.Number);

							break;

						case "From":
							if (filter.SortDirection == "Asc")
								query = query.OrderBy(o => o.o.From);
							else
								query = query.OrderByDescending(o => o.o.From);

							break;

						case "To":
							if (filter.SortDirection == "Asc")
								query = query.OrderBy(o => o.o.To);
							else
								query = query.OrderByDescending(o => o.o.To);

							break;
					}
				}

				query = query.Distinct();

				// пейджинг

				if (filter.PageNumber > 0)
					query = query.Skip(filter.PageNumber * filter.PageSize);

				if (filter.PageSize > 0)
					query = query.Take(filter.PageSize);

				return query.Select(s => s.o).ToList();
			}
		}

		#region motivation

		public int GetMotivationOrdersCount(ListFilter filter)
		{
			using (var db = new LogistoDb())
			{
				var query = from o in db.Orders
							join w in db.Participants on o.ID equals w.OrderId
							where o.OrderStatusId == 7  // мотивация
									&& w.UserId == filter.UserId
							select new { o, w };

				// условия поиска

				if (!string.IsNullOrWhiteSpace(filter.Context))
				{
					var contracts = db.Contracts.Where(w => w.Number.Contains(filter.Context)).Select(s => s.ID).ToList();
					var products = db.Products.Where(w => w.Display.Contains(filter.Context)).Select(s => s.ID).ToList();

					// по юрлицу и контрагенту
					contracts = contracts.Union((from cn in db.Contractors
												 join l in db.Legals on cn.ID equals l.ContractorId
												 join c in db.Contracts on l.ID equals c.LegalId
												 where cn.Name.Contains(filter.Context) || l.DisplayName.Contains(filter.Context)
												 select c.ID)).ToList();

					query = query.Where(w => w.o.CargoInfo.Contains(filter.Context) ||
						w.o.Number.Contains(filter.Context) ||
						w.o.Comment.Contains(filter.Context) ||
						w.o.From.Contains(filter.Context) ||
						w.o.To.Contains(filter.Context) ||
						// экстра-поиск
						contracts.Contains(w.o.ContractId.Value) ||
						products.Contains(w.o.ProductId)
						);
				}

				// даты

				if (filter.From.HasValue)
					query = query.Where(w => w.o.ClosedDate > filter.From.Value.Date);

				if (filter.To.HasValue)
					query = query.Where(w => w.o.ClosedDate < filter.To.Value.Date.AddDays(1));

				// роли

				if (filter.From.HasValue)
					query = query.Where(w => w.o.ClosedDate > filter.From.Value.Date);

				if (filter.To.HasValue)
					query = query.Where(w => w.o.ClosedDate < filter.To.Value.Date.AddDays(1));

				// роли

				if (filter.Statuses.Count == 0)
					query = query.Where(w => ((w.w.ParticipantRoleId == 3) && w.w.IsResponsible) || (w.w.ParticipantRoleId == 1) || (w.w.ParticipantRoleId == 5));

				if (filter.Statuses.Count == 1)
				{
					if (filter.Statuses.First() == 3)
						query = query.Where(w => ((w.w.ParticipantRoleId == 3) && w.w.IsResponsible));
					else
						query = query.Where(w => (w.w.ParticipantRoleId == filter.Statuses.First()));
				}

				if (filter.Statuses.Count > 1)
				{
					if (filter.Statuses.Contains(3))
					{
						var st = filter.Statuses.Except(new int[] { 3 });
						query = query.Where(w => ((w.w.ParticipantRoleId == 3) && w.w.IsResponsible) || st.Contains(w.w.ParticipantRoleId.Value));
					}
					else
						query = query.Where(w => filter.Statuses.Contains(w.w.ParticipantRoleId.Value));
				}


				return query.Distinct().Count();
			}
		}

		public double GetMotivationOrdersSum(ListFilter filter)
		{
			using (var db = new LogistoDb())
			{
				var query = from o in db.Orders
							join w in db.Participants on o.ID equals w.OrderId
							where o.OrderStatusId == 7  // мотивация
									&& w.UserId == filter.UserId
							select new { o, w };

				// условия поиска

				if (!string.IsNullOrWhiteSpace(filter.Context))
				{
					var contracts = db.Contracts.Where(w => w.Number.Contains(filter.Context)).Select(s => s.ID).ToList();
					var products = db.Products.Where(w => w.Display.Contains(filter.Context)).Select(s => s.ID).ToList();

					// по юрлицу и контрагенту
					contracts = contracts.Union((from cn in db.Contractors
												 join l in db.Legals on cn.ID equals l.ContractorId
												 join c in db.Contracts on l.ID equals c.LegalId
												 where cn.Name.Contains(filter.Context) || l.DisplayName.Contains(filter.Context)
												 select c.ID)).ToList();

					query = query.Where(w => w.o.CargoInfo.Contains(filter.Context) ||
						w.o.Number.Contains(filter.Context) ||
						w.o.Comment.Contains(filter.Context) ||
						w.o.From.Contains(filter.Context) ||
						w.o.To.Contains(filter.Context) ||
						// экстра-поиск
						contracts.Contains(w.o.ContractId.Value) ||
						products.Contains(w.o.ProductId)
						);
				}

				// даты

				if (filter.From.HasValue)
					query = query.Where(w => w.o.ClosedDate > filter.From.Value.Date);

				if (filter.To.HasValue)
					query = query.Where(w => w.o.ClosedDate < filter.To.Value.Date.AddDays(1));

				// роли

				if (filter.Statuses.Count == 0)
					query = query.Where(w => ((w.w.ParticipantRoleId == 3) && w.w.IsResponsible) || (w.w.ParticipantRoleId == (int)ParticipantRoles.SM) || (w.w.ParticipantRoleId == (int)ParticipantRoles.SL));

				if (filter.Statuses.Count == 1)
				{
					if (filter.Statuses.First() == 3)
						query = query.Where(w => ((w.w.ParticipantRoleId == 3) && w.w.IsResponsible));
					else
						query = query.Where(w => (w.w.ParticipantRoleId == filter.Statuses.First()));
				}

				if (filter.Statuses.Count > 1)
				{
					if (filter.Statuses.Contains(3))
					{
						var st = filter.Statuses.Except(new int[] { 3 });
						query = query.Where(w => ((w.w.ParticipantRoleId == 3) && w.w.IsResponsible) || st.Contains(w.w.ParticipantRoleId.Value));
					}
					else
						query = query.Where(w => filter.Statuses.Contains(w.w.ParticipantRoleId.Value));
				}


				return query.Distinct().Sum(w => w.o.Balance ?? 0);
			}
		}

		public IEnumerable<Order> GetMotivationOrders(ListFilter filter)
		{
			using (var db = new LogistoDb())
			{
				var query = from o in db.Orders
							join w in db.Participants on o.ID equals w.OrderId
							from c in db.Contracts.Where(wc => wc.ID == o.ContractId).DefaultIfEmpty()
							from l in db.Legals.Where(wl => wl.ID == c.LegalId).DefaultIfEmpty()
							from cn in db.Contractors.Where(wcn => wcn.ID == l.ContractorId).DefaultIfEmpty()
							from p in db.Products.Where(wp => wp.ID == o.ProductId).DefaultIfEmpty()
							where o.OrderStatusId == 7  // мотивация
									&& w.UserId == filter.UserId
							select new { o, c, p, l, cn, w };

				// условия поиска

				if (!string.IsNullOrWhiteSpace(filter.Context))
				{
					query = query.Where(w => w.o.CargoInfo.Contains(filter.Context) ||
						w.o.Number.Contains(filter.Context) ||
						w.o.Comment.Contains(filter.Context) ||
						w.o.From.Contains(filter.Context) ||
						w.o.To.Contains(filter.Context) ||
						// экстра-поиск
						w.cn.Name.Contains(filter.Context) ||
						w.c.Number.Contains(filter.Context) ||
						w.l.DisplayName.Contains(filter.Context) ||
						w.p.Display.Contains(filter.Context)
						);
				}

				// даты

				if (filter.From.HasValue)
					query = query.Where(w => w.o.ClosedDate > filter.From.Value.Date);

				if (filter.To.HasValue)
					query = query.Where(w => w.o.ClosedDate < filter.To.Value.Date.AddDays(1));

				// роли

				if (filter.Statuses.Count == 0)
					query = query.Where(w => ((w.w.ParticipantRoleId == 3) && w.w.IsResponsible) || (w.w.ParticipantRoleId == 1) || (w.w.ParticipantRoleId == 5));

				if (filter.Statuses.Count == 1)
				{
					if (filter.Statuses.First() == 3)
						query = query.Where(w => ((w.w.ParticipantRoleId == 3) && w.w.IsResponsible));
					else
						query = query.Where(w => (w.w.ParticipantRoleId == filter.Statuses.First()));
				}

				if (filter.Statuses.Count > 1)
				{
					if (filter.Statuses.Contains(3))
					{
						var st = filter.Statuses.Except(new int[] { 3 });
						query = query.Where(w => ((w.w.ParticipantRoleId == 3) && w.w.IsResponsible) || st.Contains(w.w.ParticipantRoleId.Value));
					}
					else
						query = query.Where(w => filter.Statuses.Contains(w.w.ParticipantRoleId.Value));
				}

				// сортировка

				if (!string.IsNullOrWhiteSpace(filter.Sort))
				{
					if (string.IsNullOrWhiteSpace(filter.SortDirection))
						filter.SortDirection = "Asc";

					switch (filter.Sort)
					{
						case "Number":  // TEMP:
						case "ID":
							if (filter.SortDirection == "Asc")
								query = query.OrderBy(o => o.o.ID);
							else
								query = query.OrderByDescending(o => o.o.ID);

							break;

						case "OrderStatusId":
							if (filter.SortDirection == "Asc")
								query = query.OrderBy(o => o.o.OrderStatusId);
							else
								query = query.OrderByDescending(o => o.o.OrderStatusId);

							break;


						case "ClosedDate":
							if (filter.SortDirection == "Asc")
								query = query.OrderBy(o => o.o.ClosedDate);
							else
								query = query.OrderByDescending(o => o.o.ClosedDate);

							break;

						case "CreatedDate":
							if (filter.SortDirection == "Asc")
								query = query.OrderBy(o => o.o.CreatedDate);
							else
								query = query.OrderByDescending(o => o.o.CreatedDate);

							break;

						case "Balance":
							if (filter.SortDirection == "Asc")
								query = query.OrderBy(o => o.o.Balance);
							else
								query = query.OrderByDescending(o => o.o.Balance);

							break;

						case "Volume":
							if (filter.SortDirection == "Asc")
								query = query.OrderBy(o => o.o.Volume);
							else
								query = query.OrderByDescending(o => o.o.Volume);

							break;

						case "GrossWeight":
							if (filter.SortDirection == "Asc")
								query = query.OrderBy(o => o.o.GrossWeight);
							else
								query = query.OrderByDescending(o => o.o.GrossWeight);

							break;

						case "ProductId":
							if (filter.SortDirection == "Asc")
								query = query.OrderBy(o => o.p.Display);
							else
								query = query.OrderByDescending(o => o.p.Display);

							break;

						case "Contractor":
							if (filter.SortDirection == "Asc")
								query = query.OrderBy(o => o.cn.Name);
							else
								query = query.OrderByDescending(o => o.cn.Name);

							break;

						case "Legal":
							if (filter.SortDirection == "Asc")
								query = query.OrderBy(o => o.l.DisplayName);
							else
								query = query.OrderByDescending(o => o.l.DisplayName);

							break;

						case "ContractId":
							if (filter.SortDirection == "Asc")
								query = query.OrderBy(o => o.c.Number);
							else
								query = query.OrderByDescending(o => o.c.Number);

							break;

						case "From":
							if (filter.SortDirection == "Asc")
								query = query.OrderBy(o => o.o.From);
							else
								query = query.OrderByDescending(o => o.o.From);

							break;

						case "To":
							if (filter.SortDirection == "Asc")
								query = query.OrderBy(o => o.o.To);
							else
								query = query.OrderByDescending(o => o.o.To);

							break;
					}
				}

				query = query.Distinct();

				// пейджинг

				if (filter.PageNumber > 0)
					query = query.Skip(filter.PageNumber * filter.PageSize);

				if (filter.PageSize > 0)
					query = query.Take(filter.PageSize);

				return query.Select(s => s.o).ToList();
			}
		}

		#endregion

		public int CreateOrder(Order order)
		{
			using (var db = new LogistoDb())
			{
				return Convert.ToInt32(db.InsertWithIdentity(order));
			}
		}

		public Order GetOrder(int id)
		{
			using (var db = new LogistoDb())
			{
				return db.Orders.FirstOrDefault(w => w.ID == id);
			}
		}

		public void UpdateOrder(Order order)
		{
			using (var db = new LogistoDb())
			{
				db.Orders.Where(w => w.ID == order.ID)
				.Set(u => u.Number, order.Number)
				.Set(u => u.ProductId, order.ProductId)
				.Set(u => u.ContractId, order.ContractId)
				.Set(u => u.RequestNumber, order.RequestNumber)
				.Set(u => u.RequestDate, order.RequestDate)
				.Set(u => u.OrderTypeId, order.OrderTypeId > 0 ? order.OrderTypeId : null)
				.Set(u => u.SpecialCustody, order.SpecialCustody)
				.Set(u => u.EnSpecialCustody, order.EnSpecialCustody)
				.Set(u => u.VolumetricRatioId, order.VolumetricRatioId > 0 ? order.VolumetricRatioId : null)
				.Set(u => u.Danger, order.Danger)
				.Set(u => u.EnDanger, order.EnDanger)
				.Set(u => u.PaidWeight, order.PaidWeight)
				.Set(u => u.TemperatureRegime, order.TemperatureRegime)
				.Set(u => u.EnTemperatureRegime, order.EnTemperatureRegime)
				.Set(u => u.InsurancePolicy, order.InsurancePolicy)
				.Set(u => u.InsuranceTypeId, order.InsuranceTypeId)
				.Set(u => u.UninsuranceTypeId, order.UninsuranceTypeId > 0 ? order.UninsuranceTypeId : null)
				.Set(u => u.InvoiceNumber, order.InvoiceNumber)
				.Set(u => u.InvoiceDate, order.InvoiceDate)
				.Set(u => u.InvoiceSum, order.InvoiceSum)
				.Set(u => u.InvoiceCurrencyId, order.InvoiceCurrencyId)
				.Set(u => u.IsPrintDetails, order.IsPrintDetails)
				.Set(u => u.RouteBeforeBoard, order.RouteBeforeBoard)
				.Set(u => u.RouteAfterBoard, order.RouteAfterBoard)
				.Set(u => u.RouteLengthBeforeBoard, order.RouteLengthBeforeBoard)
				.Set(u => u.RouteLengthAfterBoard, order.RouteLengthAfterBoard)
				.Set(u => u.Balance, order.Balance)
				.Set(u => u.CargoInfo, order.CargoInfo)
				.Set(u => u.EnCargoInfo, order.EnCargoInfo)
				.Set(u => u.Comment, order.Comment)
				.Set(u => u.Expense, order.Expense)
				.Set(u => u.From, order.From)
				.Set(u => u.GrossWeight, order.GrossWeight)
				.Set(u => u.Income, order.Income)
				.Set(u => u.InvoiceCurrencyDisplay, order.InvoiceCurrencyDisplay)
				.Set(u => u.LoadingDate, order.LoadingDate)
				.Set(u => u.NetWeight, order.NetWeight)
				.Set(u => u.OrderStatusId, order.OrderStatusId)
				.Set(u => u.ClosedDate, order.ClosedDate)
				.Set(u => u.ReceiverLegalId, order.ReceiverLegalId)
				.Set(u => u.SeatsCount, order.SeatsCount)
				.Set(u => u.SenderLegalId, order.SenderLegalId)
				.Set(u => u.To, order.To)
				.Set(u => u.UnloadingDate, order.UnloadingDate)
				.Set(u => u.VehicleNumbers, order.VehicleNumbers)
				.Set(u => u.Volume, order.Volume)
				.Set(u => u.FinRepCenterId, order.FinRepCenterId)
				.Set(u => u.OrderTemplateId, order.OrderTemplateId)
				.Set(u => u.Cost, order.Cost)
				.Set(u => u.zkzEconomicaBalance, order.zkzEconomicaBalance)
				.Set(u => u.zkzExpTO, order.zkzExpTO)
				.Set(u => u.zkzForDetailsVAT, order.zkzForDetailsVAT)
				.Set(u => u.zkzForDetailsVATSumAfter, order.zkzForDetailsVATSumAfter)
				.Set(u => u.zkzForDetailsVATSumBefore, order.zkzForDetailsVATSumBefore)
				.Set(u => u.zkzGOcomment, order.zkzGOcomment)
				.Set(u => u.zkzGOcontact, order.zkzGOcontact)
				.Set(u => u.zkzGOcontTel, order.zkzGOcontTel)
				.Set(u => u.zkzGOname, order.zkzGOname)
				.Set(u => u.zkzGOnameINN, order.zkzGOnameINN)
				.Set(u => u.zkzGOtime, order.zkzGOtime)
				.Set(u => u.zkzGPcomment, order.zkzGPcomment)
				.Set(u => u.zkzGPcontact, order.zkzGPcontact)
				.Set(u => u.zkzGPconTel, order.zkzGPconTel)
				.Set(u => u.zkzGPname, order.zkzGPname)
				.Set(u => u.zkzGPnameINN, order.zkzGPnameINN)
				.Set(u => u.zkzGPtime, order.zkzGPtime)
				.Set(u => u.zkzGruzPriceCurrID, order.zkzGruzPriceCurrID)
				.Set(u => u.zkzImpTO, order.zkzImpTO)
				.Set(u => u.zkzInsurUpak, order.zkzInsurUpak)
				.Set(u => u.zkzInvoiceCurrNew, order.zkzInvoiceCurrNew)
				.Set(u => u.zkzKursValue, order.zkzKursValue)
				.Set(u => u.zkzMarshrut, order.zkzMarshrut)
				.Set(u => u.zkzOtpravitelContactID, order.zkzOtpravitelContactID)
				.Set(u => u.zkzPoluchContactID, order.zkzPoluchContactID)
				.Set(u => u.zkzZayavkaID, order.zkzZayavkaID)
				.Update();
			}
		}

		public Order GetOrderByAccounting(int accountingId)
		{
			using (var db = new LogistoDb())
			{
				var query = from o in db.Orders
							join c in db.Accountings on o.ID equals c.OrderId
							where c.ID == accountingId
							select o;

				return query.FirstOrDefault();
			}
		}

		public IEnumerable<Order> GetOrdersByLegal(int legalId)
		{
			using (var db = new LogistoDb())
			{
				return (from o in db.Orders
						join c in db.Contracts on o.ContractId equals c.ID
						where c.LegalId == legalId
						select o).ToList();
			}
		}

		public int GetOrdersCountByContractor(int contractorId)
		{
			using (var db = new LogistoDb())
			{
				return (from o in db.Orders
						join c in db.Contracts on o.ContractId equals c.ID
						join l in db.Legals on c.LegalId equals l.ID
						where c.LegalId == l.ID && l.ContractorId == contractorId
						select o).Count();
			}
		}

		public IEnumerable<Order> GetOrdersByContractor(int contractorId)
		{
			using (var db = new LogistoDb())
			{
				return (from o in db.Orders
						join c in db.Contracts on o.ContractId equals c.ID
						join l in db.Legals on c.LegalId equals l.ID
						where c.LegalId == l.ID && l.ContractorId == contractorId
						select o).ToList();
			}
		}

		public IEnumerable<Order> GetOrdersByContract(int contractId)
		{
			using (var db = new LogistoDb())
			{
				return db.Orders.Where(w => w.ContractId == contractId).ToList();
			}
		}

		#endregion

		public Dictionary<string, object> GetOrderInfo(int id)
		{
			using (var db = new LogistoDb())
			{
				var entity = new Dictionary<string, object>();
				var order = db.Orders.First(w => w.ID == id);

				// основные поля
				entity.Add("ID", order.ID);
				entity.Add("Number", order.Number);
				entity.Add("Comment", order.Comment);
				entity.Add("CreatedDate", order.CreatedDate);
				entity.Add("LoadingDate", order.LoadingDate);
				entity.Add("UnloadingDate", order.UnloadingDate);
				entity.Add("From", order.From);
				entity.Add("To", order.To);
				entity.Add("CargoInfo", order.CargoInfo);
				entity.Add("NetWeight", order.NetWeight);
				entity.Add("SeatsCount", order.SeatsCount);
				entity.Add("Danger", order.Danger);
				entity.Add("SpecialCustody", order.SpecialCustody);
				entity.Add("TemperatureRegime", order.TemperatureRegime);
				entity.Add("InsurancePolicy", order.InsurancePolicy);
				entity.Add("InvoiceDate", order.InvoiceDate);
				entity.Add("InvoiceNumber", order.InvoiceNumber);
				entity.Add("InvoiceSum", order.InvoiceSum);
				entity.Add("RequestDate", order.RequestDate);
				entity.Add("RequestNumber", order.RequestNumber);

				// поля из справочников
				entity.Add("Status", db.OrderStatuses.Where(w => w.ID == order.OrderStatusId).Select(s => s.Display).FirstOrDefault());
				var contract = db.Contracts.First(w => w.ID == order.ContractId);
				var ourLegal = db.OurLegals.First(w => w.ID == contract.OurLegalId);
				entity.Add("OurLegal", db.Legals.Where(w => w.ID == ourLegal.LegalId).Select(s => s.DisplayName).FirstOrDefault());
				entity.Add("Legal", db.Legals.Where(w => w.ID == contract.LegalId).Select(s => s.DisplayName).FirstOrDefault());
				entity.Add("OurBankAccount", db.BankAccounts.Where(w => w.ID == contract.OurBankAccountId).Select(s => s.Number).FirstOrDefault());
				entity.Add("BankAccount", db.BankAccounts.Where(w => w.ID == contract.BankAccountId).Select(s => s.Number).FirstOrDefault());
				entity.Add("OurContractRole", db.ContractRoles.Where(w => w.ID == contract.OurContractRoleId).Select(s => s.Display).FirstOrDefault());
				entity.Add("ContractRole", db.ContractRoles.Where(w => w.ID == contract.ContractRoleId).Select(s => s.Display).FirstOrDefault());
				entity.Add("ContractServiceType", db.ContractServiceTypes.Where(w => w.ID == contract.ContractServiceTypeId).Select(s => s.Display).FirstOrDefault());
				entity.Add("ContractType", db.ContractTypes.Where(w => w.ID == contract.ContractTypeId).Select(s => s.Display).FirstOrDefault());
				entity.Add("PaymentTerm", db.PaymentTerms.Where(w => w.ID == contract.PaymentTermsId).Select(s => s.Display).FirstOrDefault());
				entity.Add("CurrencyRateUse", db.CurrencyRateUses.Where(w => w.ID == contract.CurrencyRateUseId).Select(s => s.Display).FirstOrDefault());

				var q = from cc in db.ContractCurrencies
						from c in db.Currencies.Where(w => w.ID == cc.CurrencyId).DefaultIfEmpty()
						from oa in db.BankAccounts.Where(woa => woa.ID == cc.OurBankAccountId).DefaultIfEmpty()
						from oab in db.Banks.Where(woab => woab.ID == oa.BankId).DefaultIfEmpty()
						from a in db.BankAccounts.Where(wa => wa.ID == cc.BankAccountId).DefaultIfEmpty()
						from ab in db.Banks.Where(wab => wab.ID == a.BankId).DefaultIfEmpty()
						where cc.ContractId == id
						select new { Display = c.Display, OurBankAccount = oa.Number, OurBank = oab.Name, BankAccount = a.Number, Bank = ab.Name ?? a.CoBankName };

				entity.Add("Currencies", q.ToList());

				var personByUsers = (from u in db.Users
									 from p in db.Persons.Where(w => w.ID == u.PersonId).DefaultIfEmpty()
									 select new { u.ID, p.DisplayName }).ToList();

				var marks = db.ContractMarks.Where(w => w.ContractId == id).Select(s => new
				{
					s.IsContractOk,
					s.ContractOkDate,
					s.ContractOkUserId,
					ContractOkUser = personByUsers.Where(pbuw => pbuw.ID == s.ContractOkUserId).Select(pbu => pbu.DisplayName).FirstOrDefault(),

					s.IsContractChecked,
					s.ContractCheckedDate,
					s.ContractCheckedUserId,
					ContractCheckedUser = personByUsers.Where(pbuw => pbuw.ID == s.ContractCheckedUserId).Select(pbu => pbu.DisplayName).FirstOrDefault(),

					s.IsContractRejected,
					s.ContractRejectedDate,
					s.ContractRejectedUserId,
					ContractRejectedUser = personByUsers.Where(pbuw => pbuw.ID == s.ContractRejectedUserId).Select(pbu => pbu.DisplayName).FirstOrDefault(),
					s.ContractRejectedComment,

					s.IsContractBlocked,
					s.ContractBlockedDate,
					s.ContractBlockedUserId,
					ContractBlockedUser = personByUsers.Where(pbuw => pbuw.ID == s.ContractBlockedUserId).Select(pbu => pbu.DisplayName).FirstOrDefault(),
					s.ContractBlockedComment
				}).FirstOrDefault();

				entity.Add("ContractMarks", marks);

				// места
				var descriptions = db.CargoDescriptions.ToList();
				var packages = db.PackageTypes.ToList();
				var seats = db.CargoSeats.Where(w => w.OrderId == id).Select(s => new
				{
					s.GrossWeight,
					s.Height,
					s.IsStacking,
					s.Length,
					s.SeatCount,
					s.Volume,
					s.Width,
					Description = descriptions.Where(w => w.ID == s.CargoDescriptionId).Select(sd => sd.Display).FirstOrDefault(),
					Package = packages.Where(w => w.ID == s.PackageTypeId).Select(sd => sd.Display).FirstOrDefault()
				}).ToList();

				entity.Add("Seats", seats);

				// события
				var eventTypes = db.Events.ToList();
				var events = db.OrderEvents.Where(w => w.OrderId == id).Select(s => new
				{
					s.City,
					s.Comment,
					s.Date,
					s.IsExternal,
					Event = eventTypes.Where(w => w.ID == s.EventId).Select(sd => sd.Name).FirstOrDefault()
				}).ToList();

				entity.Add("Events", events);

				// документы
				var docTypes = db.DocumentTypes.ToList();
				var tdocTypes = db.Templates.ToList();
				var accountingNumbers = db.Accountings.Select(s => new { s.ID, s.Number }).ToList();
				var docQuery = from d in db.Documents
							   from a in db.Accountings.Where(w => w.ID == d.AccountingId).DefaultIfEmpty()
							   where a.OrderId == id || d.OrderId == id
							   select d;

				var documents = docQuery.Select(s => new
				{
					IsDocument = true,
					ID = s.ID,
					Date = s.Date,
					Number = s.Number,
					UploadedDate = s.UploadedDate,
					DocumentType = docTypes.Where(w => w.ID == s.DocumentTypeId).Select(sd => sd.Display).FirstOrDefault(),
					//IsPrint = s.IsPrint,
					IsNipVisible = s.IsNipVisible,
					//OriginalSentDate = s.OriginalSentDate,
					//OriginalSentBy = s.OriginalSentUserId,
					//OriginalReceivedDate = s.OriginalReceivedDate,
					//OriginalReceivedBy = s.OriginalReceivedUserId,
					//OrderAccountingId = s.OrderAccountingId,
					ReceivedBy = s.ReceivedBy,
					ReceivedNumber = s.ReceivedNumber,
					Filename = s.Filename,
					FileSize = s.FileSize
				}).ToList();

				var tempdocQuery = from d in db.TemplatedDocuments
								   from a in db.Accountings.Where(w => w.ID == d.AccountingId).DefaultIfEmpty()
								   where a.OrderId == id || d.OrderId == id
								   select d;

				documents.AddRange(tempdocQuery.Select(s => new
				{
					IsDocument = false,
					ID = s.ID,
					Date = (DateTime?)null,
					Number = accountingNumbers.Where(w => w.ID == s.AccountingId).Select(sd => sd.Number).FirstOrDefault(),
					UploadedDate = s.ChangedDate ?? s.CreatedDate,
					DocumentType = tdocTypes.Where(w => w.ID == s.TemplateId).Select(sd => sd.Name).FirstOrDefault(),
					IsNipVisible = true,
					//OriginalSentDate = s.OriginalSentDate,
					//OriginalSentBy = s.OriginalSentUserId,
					//OriginalReceivedDate = s.OriginalReceivedDate,
					//OriginalReceivedBy = s.OriginalReceivedUserId,
					//OrderAccountingId = s.OrderAccountingId,
					ReceivedBy = s.ReceivedBy,
					ReceivedNumber = s.ReceivedNumber,
					Filename = s.Filename,
					FileSize = ""
				}));

				entity.Add("Documents", documents);


				return entity;
			}
		}

		public void CalculateOrderBalance(int orderId)
		{
			using (var db = new LogistoDb())
			{
				var income = db.Accountings.Where(w => w.OrderId == orderId && w.IsIncome).Sum(s => s.Sum ?? 0);
				var expense = db.Accountings.Where(w => w.OrderId == orderId && !w.IsIncome).Sum(s => s.Sum ?? 0);
				var balance = income - expense;
				db.Orders.Where(w => w.ID == orderId)
					.Set(u => u.Income, income)
					.Set(u => u.Expense, expense)
					.Set(u => u.Balance, balance)
					.Update();
			}
		}

		#region route point

		public int CreateRoutePoint(RoutePoint point)
		{
			using (var db = new LogistoDb())
			{
				var id = Convert.ToInt32(db.InsertWithIdentity(point));
				return id;
			}
		}

		public RoutePoint GetRoutePoint(int pointId)
		{
			using (var db = new LogistoDb())
			{
				return db.RoutePoints.FirstOrDefault(w => w.ID == pointId);
			}
		}

		public void UpdateRoutePoint(RoutePoint point)
		{
			using (var db = new LogistoDb())
			{
				db.RoutePoints.Where(w => w.ID == point.ID)
				.Set(u => u.No, point.No)
				.Set(u => u.RoutePointTypeId, point.RoutePointTypeId)
				.Set(u => u.PlaceId, point.PlaceId)
				.Set(u => u.PlanDate, point.PlanDate)
				.Set(u => u.FactDate, point.FactDate)
				.Set(u => u.Address, point.Address)
				.Set(u => u.EnAddress, point.EnAddress)
				.Set(u => u.Contact, point.Contact)
				.Set(u => u.EnContact, point.EnContact)
				.Set(u => u.ParticipantLegalId, point.ParticipantLegalId)
				.Set(u => u.ParticipantComment, point.ParticipantComment)
				.Set(u => u.RouteContactID, point.RouteContactID)
				.Set(u => u.EnParticipantComment, point.EnParticipantComment)
				.Update();
			}
		}

		public void DeleteRoutePoint(int routePointId)
		{
			using (var db = new LogistoDb())
			{
				var segments = db.RouteSegments.Where(w => w.FromRoutePointId == routePointId || w.ToRoutePointId == routePointId).ToList();
				foreach (var item in segments)
				{
					db.OrderAccountingRouteSegments.Delete(w => w.RouteSegmentId == item.ID);
					db.RouteSegments.Delete(w => w.ID == item.ID);
				}

				db.RoutePoints.Delete(w => w.ID == routePointId);
			}
		}

		public IEnumerable<RoutePoint> GetRoutePoints(int orderId)
		{
			using (var db = new LogistoDb())
			{
				return db.RoutePoints.Where(w => w.OrderId == orderId).OrderBy(o => o.No).ToList();
			}
		}

		#endregion

		#region route segment

		public int CreateRouteSegment(RouteSegment segment)
		{
			using (var db = new LogistoDb())
			{
				return Convert.ToInt32(db.InsertWithIdentity(segment));
			}
		}

		public RouteSegment GetRouteSegment(int segmentId)
		{
			using (var db = new LogistoDb())
			{
				return db.RouteSegments.FirstOrDefault(w => w.ID == segmentId);
			}
		}

		public void UpdateRouteSegment(RouteSegment segment)
		{
			using (var db = new LogistoDb())
			{
				db.RouteSegments.Where(w => w.ID == segment.ID)
				.Set(u => u.No, segment.No)
				.Set(u => u.FromRoutePointId, segment.FromRoutePointId)
				.Set(u => u.ToRoutePointId, segment.ToRoutePointId)
				.Set(u => u.TransportTypeId, segment.TransportTypeId)
				.Set(u => u.IsAfterBorder, segment.IsAfterBorder)
				.Set(u => u.Length, segment.Length)
				.Set(u => u.Vehicle, segment.Vehicle)
				.Set(u => u.VehicleNumber, segment.VehicleNumber)
				.Set(u => u.DriverName, segment.DriverName)
				.Set(u => u.DriverPhone, segment.DriverPhone)
				.Update();
			}
		}

		public void DeleteRouteSegment(int routeSegmentId)
		{
			using (var db = new LogistoDb())
			{
				db.RouteSegments.Delete(w => w.ID == routeSegmentId);
			}
		}

		public IEnumerable<RouteSegment> GetRouteSegments(int orderId)
		{
			using (var db = new LogistoDb())
			{
				return db.RouteSegments.Where(w => w.OrderId == orderId).OrderBy(o => o.No);
			}
		}

		#endregion

		#region cargo seats

		public int CreateCargoSeat(CargoSeat seat)
		{
			using (var db = new LogistoDb())
			{
				return Convert.ToInt32(db.InsertWithIdentity(seat));
			}
		}

		public CargoSeat GetCargoSeat(int cargoSeatId)
		{
			using (var db = new LogistoDb())
			{
				return db.CargoSeats.FirstOrDefault(w => w.ID == cargoSeatId);
			}
		}

		public void UpdateCargoSeat(CargoSeat seat)
		{
			using (var db = new LogistoDb())
			{
				db.CargoSeats.Where(w => w.ID == seat.ID)
				.Set(u => u.CargoDescriptionId, seat.CargoDescriptionId)
				.Set(u => u.GrossWeight, seat.GrossWeight)
				.Set(u => u.Height, seat.Height)
				.Set(u => u.Length, seat.Length)
				.Set(u => u.PackageTypeId, seat.PackageTypeId)
				.Set(u => u.SeatCount, seat.SeatCount)
				.Set(u => u.Volume, seat.Volume)
				.Set(u => u.Width, seat.Width)
				.Set(u => u.IsStacking, seat.IsStacking)
				.Update();
			}
		}

		public void DeleteCargoSeat(int cargoSeatId)
		{
			using (var db = new LogistoDb())
			{
				db.CargoSeats.Delete(w => w.ID == cargoSeatId);
			}
		}

		public int GetCargoSeatsCount(int orderId)
		{
			// TODO:
			return GetCargoSeats(orderId).Count();
		}

		public IEnumerable<CargoSeat> GetCargoSeats(int orderId)
		{
			using (var db = new LogistoDb())
			{
				return db.CargoSeats.Where(w => w.OrderId == orderId).ToList();
			}
		}

		#endregion

		public void CalculateCargoSeats(int orderId)
		{
			double gross = 0;
			double volume = 0;
			int seatsCount = 0;

			using (var db = new LogistoDb())
			{
				var order = db.Orders.First(w => w.ID == orderId);
				var seats = db.CargoSeats.Where(w => w.OrderId == orderId).Select(s => new { SeatCount = s.SeatCount, GrossWeight = s.GrossWeight, Volume = s.Volume }).ToList();
				foreach (var seat in seats)
				{
					seatsCount += seat.SeatCount ?? 0;
					gross += seat.GrossWeight ?? 0;
					volume += seat.Volume ?? 0;
				}

				order.SeatsCount = seatsCount;
				order.GrossWeight = gross;
				order.Volume = volume;
				if (order.VolumetricRatioId.HasValue)
				{
					var coeff = db.VolumetricRatios.First(w => w.ID == order.VolumetricRatioId).Value;
					double volumetric = order.Volume.Value * coeff;
					order.PaidWeight = Math.Round(Math.Max(gross, volumetric), 2);
				}
				else
					order.PaidWeight = gross;

				// https://pyrus.com/t#id13217361
				//if (gross == 0 || volume == 0)
				//	order.PaidWeight = 0;

				db.Orders.Where(w => w.ID == orderId)
					.Set(u => u.GrossWeight, order.GrossWeight)
					.Set(u => u.SeatsCount, order.SeatsCount)
					.Set(u => u.PaidWeight, order.PaidWeight)
					.Set(u => u.Volume, order.Volume)
					.Update();
			}
		}

		#region operations

		public int CreateOperation(Operation entity)
		{
			using (var db = new LogistoDb())
			{
				return Convert.ToInt32(db.InsertWithIdentity(entity));
			}
		}

		public Operation GetOperation(int id)
		{
			using (var db = new LogistoDb())
			{
				return db.Operations.FirstOrDefault(w => w.ID == id);
			}
		}

		public void UpdateOperation(Operation entity)
		{
			using (var db = new LogistoDb())
			{
				db.Operations.Where(w => w.ID == entity.ID)
				.Set(u => u.No, entity.No)
				.Set(u => u.Name, entity.Name)
				.Set(u => u.OperationStatusId, entity.OperationStatusId)
				.Set(u => u.ResponsibleUserId, entity.ResponsibleUserId)
				.Set(u => u.FinishFactDate, entity.FinishFactDate)
				.Set(u => u.FinishPlanDate, entity.FinishPlanDate)
				.Set(u => u.StartFactDate, entity.StartFactDate)
				.Set(u => u.StartPlanDate, entity.StartPlanDate)
				.Update();
			}
		}

		public void DeleteOperation(int id)
		{
			using (var db = new LogistoDb())
			{
				db.Operations.Delete(w => w.ID == id);
			}
		}

		public int GetOperationsCount(OperationsFilter filter)
		{
			using (var db = new LogistoDb())
			{
				var query = from o in db.Operations
							from oo in db.OrderOperations.Where(w => w.ID == o.OrderOperationId).DefaultIfEmpty()
							from ok in db.OperationKinds.Where(w => w.ID == oo.OperationKindId).DefaultIfEmpty()
							from ord in db.Orders.Where(w => w.ID == o.OrderId)
							select new { o, oo, ok, ord };

				if (!string.IsNullOrWhiteSpace(filter.OrderNumber))
					query = query.Where(w => w.ord.Number.Contains(filter.OrderNumber));

				if (!string.IsNullOrWhiteSpace(filter.Context))
					query = query.Where(w => w.o.Name.Contains(filter.Context) || w.ok.Name.Contains(filter.Context));

				if (filter.Statuses.Count > 0)
					query = query.Where(w => filter.Statuses.Contains(w.o.OperationStatusId.Value));

				if (filter.Responsibles.Count > 0)
					query = query.Where(w => filter.Responsibles.Contains(w.o.ResponsibleUserId.Value));

				if (filter.StartPlanFrom.HasValue)
					query = query.Where(w => w.o.StartPlanDate > filter.StartPlanFrom);

				if (filter.StartPlanTo.HasValue)
					query = query.Where(w => w.o.StartPlanDate < filter.StartPlanTo);

				if (filter.StartFactFrom.HasValue)
					query = query.Where(w => w.o.StartFactDate > filter.StartFactFrom);

				if (filter.StartFactTo.HasValue)
					query = query.Where(w => w.o.StartFactDate < filter.StartFactTo);

				if (filter.FinishPlanFrom.HasValue)
					query = query.Where(w => w.o.FinishPlanDate > filter.FinishPlanFrom);

				if (filter.FinishPlanTo.HasValue)
					query = query.Where(w => w.o.FinishPlanDate < filter.FinishPlanTo);

				if (filter.FinishFactFrom.HasValue)
					query = query.Where(w => w.o.FinishFactDate > filter.FinishFactFrom);

				if (filter.FinishFactTo.HasValue)
					query = query.Where(w => w.o.FinishFactDate < filter.FinishFactTo);

				return query.Count();
			}
		}

		public IEnumerable<Operation> GetOperations(OperationsFilter filter)
		{
			using (var db = new LogistoDb())
			{
				var query = from o in db.Operations
							from oo in db.OrderOperations.Where(w => w.ID == o.OrderOperationId).DefaultIfEmpty()
							from ok in db.OperationKinds.Where(w => w.ID == oo.OperationKindId).DefaultIfEmpty()
							from ord in db.Orders.Where(w => w.ID == o.OrderId)
							select new { o, oo, ok, ord };

				if (!string.IsNullOrWhiteSpace(filter.OrderNumber))
					query = query.Where(w => w.ord.Number.Contains(filter.OrderNumber));

				if (!string.IsNullOrWhiteSpace(filter.Context))
					query = query.Where(w => w.o.Name.Contains(filter.Context) || w.ok.Name.Contains(filter.Context));

				if (filter.Statuses.Count > 0)
					query = query.Where(w => filter.Statuses.Contains(w.o.OperationStatusId.Value));

				if (filter.Responsibles.Count > 0)
					query = query.Where(w => filter.Responsibles.Contains(w.o.ResponsibleUserId.Value));

				if (filter.StartPlanFrom.HasValue)
					query = query.Where(w => w.o.StartPlanDate > filter.StartPlanFrom);

				if (filter.StartPlanTo.HasValue)
					query = query.Where(w => w.o.StartPlanDate < filter.StartPlanTo);

				if (filter.StartFactFrom.HasValue)
					query = query.Where(w => w.o.StartFactDate > filter.StartFactFrom);

				if (filter.StartFactTo.HasValue)
					query = query.Where(w => w.o.StartFactDate < filter.StartFactTo);

				if (filter.FinishPlanFrom.HasValue)
					query = query.Where(w => w.o.FinishPlanDate > filter.FinishPlanFrom);

				if (filter.FinishPlanTo.HasValue)
					query = query.Where(w => w.o.FinishPlanDate < filter.FinishPlanTo);

				if (filter.FinishFactFrom.HasValue)
					query = query.Where(w => w.o.FinishFactDate > filter.FinishFactFrom);

				if (filter.FinishFactTo.HasValue)
					query = query.Where(w => w.o.FinishFactDate < filter.FinishFactTo);

				// пейджинг

				if (filter.PageNumber > 0)
					query = query.Skip(filter.PageNumber * filter.PageSize);

				if (filter.PageSize > 0)
					query = query.Take(filter.PageSize);

				return query.Select(s => s.o).ToList();
			}
		}

		public IEnumerable<Operation> GetOperationsByOrder(int orderId)
		{
			using (var db = new LogistoDb())
			{
				return db.Operations.Where(w => w.OrderId == orderId).ToList();
			}
		}

		#endregion

		#region order events

		public int CreateOrderEvent(OrderEvent entity)
		{
			using (var db = new LogistoDb())
			{
				return Convert.ToInt32(db.InsertWithIdentity(entity));
			}
		}

		public OrderEvent GetOrderEvent(int id)
		{
			using (var db = new LogistoDb())
			{
				return db.OrderEvents.FirstOrDefault(w => w.ID == id);
			}
		}

		public OrderEvent GetOrderEvent(int orderId, int eventId)
		{
			using (var db = new LogistoDb())
			{
				return db.OrderEvents.FirstOrDefault(w => (w.OrderId == orderId) && (w.EventId == eventId));
			}
		}

		public void UpdateOrderEvent(OrderEvent entity)
		{
			using (var db = new LogistoDb())
			{
				db.OrderEvents.Where(w => w.ID == entity.ID)
				.Set(u => u.IsExternal, entity.IsExternal)
				.Set(u => u.Comment, entity.Comment)
				// TODO:
				.Update();
			}
		}

		public void DeleteOrderEvent(int id)
		{
			using (var db = new LogistoDb())
			{
				db.OrderEvents.Delete(w => w.ID == id);
			}
		}

		public IEnumerable<OrderEvent> GetOrderEvents(int orderId)
		{
			using (var db = new LogistoDb())
			{
				return db.OrderEvents.Where(w => w.OrderId == orderId).OrderBy(o => o.Date).ToList();
			}
		}

		#endregion

		#region order statis history

		public int CreateOrderStatusHistory(OrderStatusHistory entity)
		{
			using (var db = new LogistoDb())
			{
				return Convert.ToInt32(db.InsertWithIdentity(entity));
			}
		}

		public IEnumerable<OrderStatusHistory> GetOrderStatusHistory(int orderId)
		{
			using (var db = new LogistoDb())
			{
				return db.OrderStatusHistory.Where(w => w.OrderId == orderId).OrderBy(o => o.ID).ToList();
			}
		}

		#endregion
	}
}