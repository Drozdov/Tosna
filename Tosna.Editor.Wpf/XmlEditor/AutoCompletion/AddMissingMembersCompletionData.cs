using System;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using Tosna.Editor.IDE.Verification.CompletionDataProviders;
using Tosna.Editor.Wpf.Properties;

namespace Tosna.Editor.Wpf.XmlEditor.AutoCompletion
{
	public class AddMissingMembersCompletionData : ICompletionData
	{
		private readonly MissingMembersCompletionDataProvider missingMemberProblem;

		public AddMissingMembersCompletionData(MissingMembersCompletionDataProvider missingMemberProblem)
		{
			this.missingMemberProblem = missingMemberProblem;
		}

		public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
		{
			if (missingMemberProblem.TryGetSolution(textArea, out var solution))
			{
				SolutionsWorker.ApplySolution(solution, textArea);
			}
		}

		public ImageSource Image => null;

		public string Text => Resources.AddMissingMembersAction;

		public object Content => Resources.AddMissingMembersAction;

		public object Description => Resources.AddMissingMembersAction;

		public double Priority => 1;
	}
}
