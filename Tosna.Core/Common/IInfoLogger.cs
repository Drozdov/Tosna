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

		public static string GetMessageTypeStr(this LogMessageType messageType)
		{
			switch (messageType)
			{
				case LogMessageType.Debug:
					return "D";

				case LogMessageType.Info:
					return "I";
				
				case LogMessageType.Warning:
					return "W";
				
				case LogMessageType.Error:
					return "E";
				
				default:
					throw new ArgumentOutOfRangeException(nameof(messageType), messageType, null);
			}
		}
	}
}