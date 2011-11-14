using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using CIAPI;
using CIAPI.DTO;
using CIAPI.Rpc;
using CIAPI.Streaming;
using Common.Logging;

namespace SimpleCiapiTest
{
	class Example
	{
		private static readonly Uri RPC_URI = new Uri("https://ciapi.cityindex.com/TradingAPI");
		private static readonly Uri STREAMING_URI = new Uri("https://push.cityindex.com/");
		private const string USERNAME = "";
		private const string PASSWORD = "";

		public void Login()
		{
			var adapter = new MyLoggerFactoryAdapter(null) { OnMessage = AddLogMessage };
			LogManager.Adapter = adapter;

			_client = new Client(RPC_URI);
			_client.LogIn(USERNAME, PASSWORD);
		}

		public void Logout()
		{
			_client.LogOut();
			_client.Dispose();
			_client = null;
		}

		public void SubscribeToStreams()
		{
			var streamingClient = StreamingClientFactory.CreateStreamingClient(STREAMING_URI, USERNAME, _client.Session);

			var marginListener = streamingClient.BuildClientAccountMarginListener();
			marginListener.MessageReceived +=
				(s, e) => Console.WriteLine("\tequity: {0}", e.Data.NetEquity);

			var topics = new[] { 99498, 99500 };
			var pricesListener = streamingClient.BuildPricesListener(topics);
			pricesListener.MessageReceived +=
				(s, e) => Console.WriteLine("{0} -> {1}", e.Data.MarketId, e.Data.Price);

			Console.ReadKey();

			streamingClient.TearDownListener(pricesListener);
			streamingClient.TearDownListener(marginListener);
			streamingClient.Dispose();
		}

		public void TestTradeHistory()
		{
			var accounts = _client.AccountInformation.GetClientAndTradingAccount();
			const int maxResults = 100;

			int tradingAccountId = accounts.CFDAccount.TradingAccountId;
			var response = _client.TradesAndOrders.ListTradeHistory(tradingAccountId, maxResults);

			tradingAccountId = accounts.SpreadBettingAccount.TradingAccountId;
			var response2 = _client.TradesAndOrders.ListTradeHistory(tradingAccountId, maxResults);
		}

		static void AddLogMessage(LogLevel level, object message, Exception exception)
		{
			AddLogMessage(message, level);
			AddLogMessage(exception, LogLevel.Error);
		}

		static void AddLogMessage(object val, LogLevel level)
		{
			if (val == null)
				return;

			var text = val.ToString() + Environment.NewLine;

			Trace.WriteLine(string.Format("{0} {1}", DateTime.UtcNow, text));
		}

		private Client _client;
	}
}
