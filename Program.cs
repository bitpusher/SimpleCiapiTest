﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace SimpleCiapiTest
{
	class Program
	{
		static void Main(string[] args)
		{
			try
			{
				//var example = new Example();
				//example.Login();

				//example.TestTradeHistory();
				//example.SubscribeToStreams();
				//example.TestUpdateOrder();

				//example.Logout();

				Repro164.Test();
			}
			catch (Exception exc)
			{
				Console.WriteLine(exc);
			}
		}
	}
}
