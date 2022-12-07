using ICSharpCode.AvalonEdit.Editing;
using Tosna.Editor.Helpers;
using Tosna.Editor.IDE.Verification;
using Tosna.Editor.IDE.Verification.CompletionDataProviders;

namespace Tosna.Editor.Wpf.XmlEditor.Tooltips
{
	public class ActionsFactory : ICompletionDataProviderVisitor
	{
		private readonly TextArea textArea;

		private ActionCommand command;

		private ActionsFactory(TextArea textArea)
		{
			this.textArea = textArea;
		}

		public static bool TryCreateActionCommand(ICompletionDataProvider completionDataProvider, TextArea textArea,
			out ActionCommand command)
		{
			var visitor = new ActionsFactory(textArea);
			completionDataProvider.Accept(visitor);
			command = visitor.command;
			return command != null;
		}

		public void Visit(NoneCompletionDataProvider provider)
		{
		}

		public void Visit(MissingMembersCompletionDataProvider provider)
		{
			command = new ActionCommand(() => MissingMembersResolver.Complete(provider, textArea), () => true, "Add missing members");
		}

		public void Visit(UnfinishedTypeCompletionDataProvider provider)
		{
		}
	}
}
