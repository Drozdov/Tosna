namespace Tosna.Core.Common.Problems
{
	public class ComplexSerializerSolution
	{
		public string NewContent { get; }

		public int LineStart { get; }

		public int PositionStart { get; }

		public int LineEnd { get; }

		public int PositionEnd { get; }

		public ComplexSerializerSolution(string newContent, int lineStart, int positionStart, int lineEnd, int positionEnd)
		{
			NewContent = newContent;
			LineStart = lineStart;
			PositionStart = positionStart;
			LineEnd = lineEnd;
			PositionEnd = positionEnd;
		}
	}
}