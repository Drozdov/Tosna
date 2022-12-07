using Tosna.Editor.IDE.Verification.CompletionDataProviders;

namespace Tosna.Editor.IDE.Verification
{
	public class VerificationError : VerificationNotification
	{
		public VerificationError(string fileName, ITextIntervalCoordinates coordinates, string message,
			ICompletionDataProvider completionDataProvider = null) : base(fileName, coordinates, message, completionDataProvider ?? new NoneCompletionDataProvider())
		{
		}

		public override VerificationNotificationType NotificationType => VerificationNotificationType.Error;
	}
}
