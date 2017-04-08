using System.Collections.Generic;
using Logisto.Models;

namespace Logisto.BusinessLogic
{
	public interface IPricelistLogic
	{
		#region Pricelist

		int CreatePricelist(Pricelist entity);
		Pricelist GetPricelist(int id);
		void UpdatePricelist(Pricelist entity);
		void DeletePricelist(int id);

		int GetPricelistsCount(ListFilter filter);
		IEnumerable<Pricelist> GetPricelists(ListFilter filter);
		IEnumerable<Pricelist> GetPricelists(int legalId);
		IEnumerable<Pricelist> GetValidPricelists();

		void ClearPricelist(int id);
		void InitPricelist(int id, int productId);
		void FixPricelistKinds(int id, int productId);
		void ImportPricelist(int fromPricelistId, int toPricelistId);
		void UpdatePricelistData(Pricelist entity);

		#endregion

		Price GetPrice(int id);
		void UpdatePrice(Price entity);
		IEnumerable<Price> GetPrices(int pricelistId);

		PriceKind GetPriceKind(int id);
		void UpdatePriceKind(PriceKind entity);
		IEnumerable<PriceKind> GetPriceKinds(int pricelistId);

	}
}