using System.Collections.Generic;
using Tosna.Editor.Helpers;
using Tosna.Editor.IDE.Verification;

namespace Tosna.Editor.Wpf.XmlEditor.Tooltips
{
	public enum NotificationType
	{
		None = 0,
		Warning = 1,
		Error = 2
	}

	public class NotificationMeta
	{
		public NotificationType NotificationNotificationType { get; }

		public ITextIntervalCoordinates Coordinates { get; }

		public IReadOnlyCollection<string> Messages { get; }

		public IReadOnlyCollection<ActionCommand> Actions { get; }

		public NotificationMeta(NotificationType notificationNotificationType, ITextIntervalCoordinates coordinates, IReadOnlyCollection<string> messages, IReadOnlyCollection<ActionCommand> actions)
		{
			NotificationNotificationType = notificationNotificationType;
			Coordinates = coordinates;
			Messages = messages;
			Actions = actions;
		}
	}
}
