using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using CIAPI;
using CIAPI.DTO;
using CIAPI.Rpc;
using Salient.ReflectiveLoggingAdapter;

namespace SimpleCiapiTest
{
	class Example
	{
		private static readonly Uri RPC_URI = new Uri("https://ciapi.cityindex.com/TradingAPI");
		private static readonly Uri STREAMING_URI = new Uri("https://push.cityindex.com/");
		private const string USERNAME = "DM593504";
		private const string PASSWORD = "qweqwe";

		public void Login()
		{
			MyLogger.OnMessage = AddLogMessage;
			// demonstrates how to inject an external logger
			LogManager.CreateInnerLogger = (logName, logLevel, showLevel, showDateTime, showLogName, dateTimeFormat) =>
			{
				// create external logger implementation and return instance.
				// this will be called whenever CIAPI requires a logger
				return new MyLogger(logName, logLevel, showLevel, showDateTime, showLogName, dateTimeFormat);
			};

			_client = new Client(RPC_URI, STREAMING_URI, "");
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
			var streamingClient = _client.CreateStreamingClient();

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
			const int maxResults = 10;

			int tradingAccountId = accounts.SpreadBettingAccount.TradingAccountId;
			var response = _client.TradesAndOrders.ListTradeHistory(tradingAccountId, maxResults);

			//tradingAccountId = accounts.SpreadBettingAccount.TradingAccountId;
			//var response2 = _client.TradesAndOrders.ListTradeHistory(tradingAccountId, maxResults);

			var items = new List<ApiTradeHistoryDTO>(response.TradeHistory);
			items.Sort((x, y) => y.LastChangedDateTimeUtc.CompareTo(x.LastChangedDateTimeUtc));

			//var buf = new StringBuilder();

			//Console.WriteLine("\r\n\r\n");
			//foreach (var cur in items)
			//{
			//    var text = string.Format("{0}\t{1}\t\t{2}\t{3}\t{4}\t{5}", cur.LastChangedDateTimeUtc, cur.MarketName, cur.OrderId, 
			//        cur.Direction, cur.OriginalQuantity, cur.Price);
			//    buf.AppendLine(text);
			//    Console.WriteLine(text);
			//}
		}

		public void TestAddStopOrder()
		{
			var accountInfo = _client.AccountInformation.GetClientAndTradingAccount();
			var orders = _client.TradesAndOrders.ListActiveStopLimitOrders(accountInfo.CFDAccount.TradingAccountId);

			const int orderId = 472307456;
			var order = _client.TradesAndOrders.GetOrder(orderId.ToString()).StopLimitOrder;

			var request = new NewStopLimitOrderRequestDTO
							{
								OrderId = orderId,
								TradingAccountId = order.TradingAccountId,
								Direction = "sell",
								MarketId = order.MarketId,
								Quantity = order.Quantity,
								OfferPrice = 5720,
								TriggerPrice = 5720,
							};
			var resp = _client.TradesAndOrders.Order(request);
			_client.MagicNumberResolver.ResolveMagicNumbers(resp);
		}

		public void TestUpdateOrder()
		{
			var accountInfo = _client.AccountInformation.GetClientAndTradingAccount();
			var openPositions = _client.TradesAndOrders.ListOpenPositions(accountInfo.CFDAccount.TradingAccountId);
			var stopLimitOrders = _client.TradesAndOrders.ListActiveStopLimitOrders(accountInfo.CFDAccount.TradingAccountId);

			const int orderId = 472307456;
			var order = _client.TradesAndOrders.GetActiveStopLimitOrder(orderId.ToString()).ActiveStopLimitOrder;

			var request = new UpdateStopLimitOrderRequestDTO
			{
				OrderId = orderId,
				TradingAccountId = (int)order.TradingAccountId,
				Direction = order.Direction,
				MarketId = order.MarketId,
				Quantity = order.Quantity,
				OfferPrice = order.TriggerPrice * 0.9M,
				TriggerPrice = order.TriggerPrice,
				BidPrice = order.TriggerPrice * 1.1M,
			};
			request.IfDone = new[]
			                 	{
			                 		new ApiIfDoneDTO
			                 			{
			                 				Limit = new ApiStopLimitOrderDTO { TriggerPrice = order.TriggerPrice * 0.9M, }, 
											Stop = new ApiStopLimitOrderDTO { TriggerPrice = order.TriggerPrice * 1.1M, }
			                 			}
			                 	};

			var resp = _client.TradesAndOrders.UpdateOrder(request);
			_client.MagicNumberResolver.ResolveMagicNumbers(resp);

			var order2 = _client.TradesAndOrders.GetActiveStopLimitOrder(orderId.ToString()).ActiveStopLimitOrder;
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

			Console.WriteLine(string.Format("{0} {1}", DateTime.UtcNow, text));
		}

		private Client _client;
	}
}
