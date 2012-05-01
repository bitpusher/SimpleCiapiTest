using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Salient.ReflectiveLoggingAdapter;

namespace SimpleCiapiTest
{
	class MyLogger : AbstractAppender
	{
		public MyLogger(string logName, LogLevel logLevel, bool showLevel, bool showDateTime, bool showLogName, string dateTimeFormat)
			: base(logName, logLevel, showLevel, showDateTime, showLogName, dateTimeFormat)
		{
		}
		public static Action<LogLevel, object, Exception> OnMessage;
		protected override void WriteInternal(LogLevel level, object message, Exception exception)
		{
			OnMessage(level, message, exception);
		}
	}
}
