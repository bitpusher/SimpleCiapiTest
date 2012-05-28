using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using CIAPI.Rpc;

namespace SimpleCiapiTest
{
	// repro for https://github.com/cityindex/CIAPI.CS/issues/164
	static class Repro164
	{
		public static void Test()
		{
			while (true)
			{
				DoPolling();
			}
		}

		private static void DoPolling()
		{
			var client = new Client(Const.RPC_URI, Const.STREAMING_URI, "Test.{B4E415A7-C453-4867-BDD1-C77ED345777B}");
			try
			{
				client.AppKey = "Test";
				client.StartMetrics();

				client.LogIn(Const.USERNAME, Const.PASSWORD);

				for (int i = 0; i < 10; i++)
				{
					var accountInfo = client.AccountInformation.GetClientAndTradingAccount();
					client.TradesAndOrders.ListOpenPositions(accountInfo.CFDAccount.TradingAccountId);
					Thread.Sleep(1000);
				}

				client.LogOut();
			}
			catch (Exception exc)
			{
				Trace.WriteLine(exc);
			}
			finally
			{
				client.Dispose();
			}
		}
	}
}
