namespace Tosna.Editor.IDE.Verification
{
	public abstract class VerificationNotification
	{
		public string FileName { get; }

		public ITextIntervalCoordinates Coordinates { get; }

		public string Message { get; }

		public ICompletionDataProvider CompletionDataProvider { get; }

		public abstract VerificationNotificationType NotificationType { get; }

		protected VerificationNotification(string fileName, ITextIntervalCoordinates coordinates, string message, ICompletionDataProvider completionDataProvider)
		{
			FileName = fileName;
			Coordinates = coordinates;
			Message = message;
			CompletionDataProvider = completionDataProvider;
		}
	}
}
