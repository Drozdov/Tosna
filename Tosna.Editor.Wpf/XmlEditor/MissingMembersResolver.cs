using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using Tosna.Core.Common.Problems;
using Tosna.Core.Helpers.Xml;
using Tosna.Editor.IDE.Verification;

namespace Tosna.Editor.Wpf.XmlEditor
{
	public static class MissingMembersResolver
	{
		public static void Complete(MissingMembersCompletionDataProvider missingMemberProblem, TextArea textArea)
		{
			var completor = new XmlCompletor(missingMemberProblem.SerializingElementsManager, missingMemberProblem.TypesResolver);
			if (!XmlUtils.TryReadXElement(textArea.Document.Text, missingMemberProblem.Line, 0, out var xElement, out var info))
			{
				return;
			}

			var newValue = completor.GetFilledContent(xElement, missingMemberProblem.Type, new string(' ', missingMemberProblem.Column)).TrimStart();
			var solution = new ComplexSerializerSolution(newValue, info.LineStart, info.ColumnStart, info.LineEnd, info.ColumnEnd);

			var startOffset = textArea.Document.GetOffset(new TextLocation(solution.LineStart, solution.PositionStart));
			var endOffset = textArea.Document.GetOffset(new TextLocation(solution.LineEnd, solution.PositionEnd + 1));

			textArea.Document.Replace(new TextSegment
			{
				StartOffset = startOffset,
				EndOffset = endOffset
			}, solution.NewContent);
		}
	}
}
