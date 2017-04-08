using System.Collections.Generic;
using Logisto.Models;

namespace Logisto.BusinessLogic
{
	public interface IRequestLogic
	{
		#region Request

		int CreateRequest(Request request);
		Request GetRequest(int id);
		void UpdateRequest(Request request);
		void DeleteRequest(int id);

		int GetRequestsCount(ListFilter filter);
		IEnumerable<Request> GetRequests(ListFilter filter);
				
		#endregion
	}
}