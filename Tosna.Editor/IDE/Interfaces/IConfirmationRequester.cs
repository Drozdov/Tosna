namespace Tosna.Editor.IDE.Interfaces
{
	public interface IConfirmationRequester
	{
		ConfirmationAnswer ConfirmOperation(string question);
	}

	public enum ConfirmationAnswer
	{
		Yes,
		No,
		Cancel
	}
}
