using System;
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
			Example.SubscribeToStreams();
		}

		//Action action =
		//    () => Thread.Sleep(Timeout.Infinite);

		//for (int i = 0; i < 50; i++)
		//{
		//    action.BeginInvoke(null, null);
		//}
	}
}
