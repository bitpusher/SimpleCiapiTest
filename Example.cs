using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using CIAPI;
using CIAPI.DTO;
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

		public static void SubscribeToStreams()
		{
			var adapter = new MyLoggerFactoryAdapter(null) { OnMessage = AddLogMessage };
			LogManager.Adapter = adapter;

			var client = new CIAPI.Rpc.Client(RPC_URI);
			client.LogIn(USERNAME, PASSWORD);

			var streamingClient = StreamingClientFactory.CreateStreamingClient(STREAMING_URI, USERNAME, client.Session);

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
			client.LogOut();
			client.Dispose();
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
	}
}
