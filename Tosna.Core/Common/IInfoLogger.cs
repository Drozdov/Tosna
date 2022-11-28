using System;

namespace Tosna.Core.Common
{
	public interface IInfoLogger
	{
		void LogMessage(LogMessageType messageType, string message);
		
		void LogMessage(LogMessageType messageType, string message, Exception e);
	}

	public enum LogMessageType
	{
		Debug,
		Info,
		Warning,
		Error
	}

	public static class InfoLoggerExtensions
	{
		public static void LogDebug(this IInfoLogger logger, string message)
		{
			logger.LogMessage(LogMessageType.Debug, message);
		}
		
		public static void LogInfo(this IInfoLogger logger, string message)
		{
			logger.LogMessage(LogMessageType.Info, message);
		}
		
		public static void LogWarning(this IInfoLogger logger, string message)
		{
			logger.LogMessage(LogMessageType.Warning, message);
		}

		public static void LogError(this IInfoLogger logger, string message)
		{
			logger.LogMessage(LogMessageType.Error, message);
		}
		
		public static void LogError(this IInfoLogger logger, string message, Exception e)
		{
			logger.LogMessage(LogMessageType.Error, message, e);
		}
	}
}