using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using Tosna.Core.Problems;

namespace Tosna.Editor.Wpf.XmlEditor
{
	public static class SolutionsWorker
	{
		
		public static void ApplySolution(ComplexSerializerSolution solution, TextArea textArea)
		{
			var startOffset = textArea.Document.GetOffset(new TextLocation(solution.LineStart, solution.PositionStart));
			var endOffset = textArea.Document.GetOffset(new TextLocation(solution.LineEnd, solution.PositionEnd));

			textArea.Document.Replace(new TextSegment
			{
				StartOffset = startOffset,
				EndOffset = endOffset
			}, solution.NewContent);
		}
	}
}