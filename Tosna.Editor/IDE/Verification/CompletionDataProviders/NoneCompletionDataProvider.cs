namespace Tosna.Editor.IDE.Verification.CompletionDataProviders
{
	public class NoneCompletionDataProvider : ICompletionDataProvider
	{
		void ICompletionDataProvider.Accept(ICompletionDataProviderVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}