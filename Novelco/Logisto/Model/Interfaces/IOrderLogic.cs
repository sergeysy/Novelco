using System.Collections.Generic;
using Logisto.Models;

namespace Logisto.BusinessLogic
{
	public interface IOrderLogic
	{
		#region Order

		int CreateOrder(Order order);

		/// <summary>
		/// Получить заказ
		/// </summary>
		Order GetOrder(int id);

		/// <summary>
		/// Обновить данные заказа
		/// </summary>
		void UpdateOrder(Order order);

		IEnumerable<Order> GetAllOrders();

		/// <summary>
		/// Получить список заказов с учетом фильтра
		/// </summary>
		IEnumerable<Order> GetOrders(ListFilter filter);

		/// <summary>
		/// Получить значение количества заказов с учетом фильтра
		/// </summary>
		int GetOrdersCount(ListFilter filter);

		#region motivation

		/// <summary>
		/// Получить список заказов с учетом фильтра
		/// </summary>
		IEnumerable<Order> GetMotivationOrders(ListFilter filter);

		/// <summary>
		/// Получить значение количества заказов с учетом фильтра
		/// </summary>
		int GetMotivationOrdersCount(ListFilter filter);
		double GetMotivationOrdersSum(ListFilter filter);

		#endregion

		/// <summary>
		/// Получить заказ
		/// </summary>
		Order GetOrderByAccounting(int accountingId);

		/// <summary>
		/// Получить список заказов для юрлица
		/// </summary>
		IEnumerable<Order> GetOrdersByLegal(int legalId);

		/// <summary>
		/// Получить список заказов для контрагента
		/// </summary>
		IEnumerable<Order> GetOrdersByContractor(int contractorId);

		/// <summary>
		/// Получить число заказов для контрагента
		/// </summary>
		int GetOrdersCountByContractor(int contractorId);

		#endregion

		Dictionary<string, object> GetOrderInfo(int id);
		void CalculateOrderBalance(int orderId);

		#region Cargo seat

		/// <summary>
		/// Создать позицию мест
		/// </summary>
		int CreateCargoSeat(CargoSeat seat);

		/// <summary>
		/// Получить позицию мест
		/// </summary>
		CargoSeat GetCargoSeat(int cargoSeatId);

		/// <summary>
		/// Обновить позицию мест
		/// </summary>
		void UpdateCargoSeat(CargoSeat seat);

		/// <summary>
		/// Удалить позицию мест
		/// </summary>
		void DeleteCargoSeat(int cargoSeatId);

		/// <summary>
		/// Получить число мест для заказа
		/// </summary>
		int GetCargoSeatsCount(int orderId);

		/// <summary>
		/// Получить список мест для заказа
		/// </summary>
		IEnumerable<CargoSeat> GetCargoSeats(int orderId);

		void CalculateCargoSeats(int orderId);

		#endregion
				
		#region route point

		/// <summary>
		/// Создать маршрутную точку
		/// </summary>
		int CreateRoutePoint(RoutePoint point);

		/// <summary>
		/// Получить точку маршрута
		/// </summary>
		RoutePoint GetRoutePoint(int pointId);

		/// <summary>
		/// Обновить маршрутную точку
		/// </summary>
		void UpdateRoutePoint(RoutePoint point);

		/// <summary>
		/// Удалить маршрутную точку
		/// </summary>
		void DeleteRoutePoint(int pointId);

		/// <summary>
		/// Получить список маршрутных пунктов для заказа
		/// </summary>
		IEnumerable<RoutePoint> GetRoutePoints(int orderId);

		#endregion

		#region route segment

		/// <summary>
		/// Создать 
		/// </summary>
		int CreateRouteSegment(RouteSegment segment);

		/// <summary>
		/// Получить 
		/// </summary>
		RouteSegment GetRouteSegment(int segmentId);

		/// <summary>
		/// Обновить 
		/// </summary>
		void UpdateRouteSegment(RouteSegment segment);

		/// <summary>
		/// Удалить 
		/// </summary>
		void DeleteRouteSegment(int segmentId);

		/// <summary>
		/// Получить список сегментов маршрута заказа
		/// </summary>
		IEnumerable<RouteSegment> GetRouteSegments(int orderId);

		#endregion

		/// <summary>
		/// Получить список заказов для договора
		/// </summary>
		IEnumerable<Order> GetOrdersByContract(int contractId);

		#region Operation

		int CreateOperation(Operation operation);
		Operation GetOperation(int id);
		void UpdateOperation(Operation operation);
		void DeleteOperation(int id);

		int GetOperationsCount(OperationsFilter filter);
		IEnumerable<Operation> GetOperations(OperationsFilter filter);
		IEnumerable<Operation> GetOperationsByOrder(int orderId);
						
		#endregion

		#region order Events

		int CreateOrderEvent(OrderEvent entity);
		OrderEvent GetOrderEvent(int id);
		OrderEvent GetOrderEvent(int orderId, int eventId);
		void UpdateOrderEvent(OrderEvent entity);
		void DeleteOrderEvent(int id);

		IEnumerable<OrderEvent> GetOrderEvents(int orderId);

		#endregion

		#region order status history

		int CreateOrderStatusHistory(OrderStatusHistory entity);
		IEnumerable<OrderStatusHistory> GetOrderStatusHistory(int orderId);

		#endregion
	}
}