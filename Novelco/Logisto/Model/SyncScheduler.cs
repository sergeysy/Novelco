using Logisto.Controllers;
using System;
using System.Threading;

namespace Logisto.Model
{
	public class SyncScheduler
	{
		public void Start()
		{
			while (true)
			{
				Thread.Sleep(TimeSpan.FromMinutes(1));

				new SyncController().FinSyncronize();
				new DataController().RefreshRates();
				new DataController().RecalculateContractors();

				Thread.Sleep(TimeSpan.FromHours(1));
			}
		}
	}
}