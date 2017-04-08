using System.Collections.Generic;
using Logisto.Models;

namespace Logisto.BusinessLogic
{
	public interface ICrmLogic
	{
		int GetCallsCount(ListFilter filter);
		IEnumerable<CrmCall> GetCalls(ListFilter filter);
		bool IsFirstCall(CrmCall call);

		IEnumerable<CrmManager> GetAllManagers();

        int GetLegalsCount(ListFilter filter);
        IEnumerable<CrmLegal> GetLegals(ListFilter filter);
    }
}