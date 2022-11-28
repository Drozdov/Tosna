namespace Tosna.Editor.IDE.Verification
{
	public class VerificationWarning : VerificationNotification
	{
		public VerificationWarning(string fileName, ITextIntervalCoordinates coordinates, string message,
			ICompletionDataProvider completionDataProvider) : base(fileName, coordinates, message, completionDataProvider)
		{
		}

		public override VerificationNotificationType NotificationType => VerificationNotificationType.Warning;
	}
}
