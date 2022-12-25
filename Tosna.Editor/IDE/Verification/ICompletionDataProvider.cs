using Tosna.Editor.IDE.Verification.CompletionDataProviders;

namespace Tosna.Editor.IDE.Verification
{
	public interface ICompletionDataProvider
	{
		void Accept(ICompletionDataProviderVisitor visitor);
	}
	
	public interface ICompletionDataProviderVisitor
	{
		void Visit(NoneCompletionDataProvider provider);

		void Visit(MissingMembersCompletionDataProvider provider);

		void Visit(UnfinishedTypeCompletionDataProvider provider);

		void Visit(InvalidClosingTagCompletionProvider provider);
	}
}