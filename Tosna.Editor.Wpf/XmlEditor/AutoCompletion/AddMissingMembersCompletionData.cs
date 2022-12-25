using System;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using Tosna.Editor.IDE.Verification.CompletionDataProviders;

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

		public string Text => "Add missing members";

		public object Content => "Add missing members";

		public object Description => "Add missing members";

		public double Priority => 1;
	}
}
