namespace Tosna.Core.Documents
{
	public struct DocumentElementLocation
	{
		public int LineStart { get; }
		
		public int ColumnStart { get; }
		
		public int LineEnd { get; }
		
		public int ColumnEnd { get; }

		public DocumentElementLocation(int lineStart, int columnStart, int lineEnd, int columnEnd)
		{
			LineStart = lineStart;
			ColumnStart = columnStart;
			LineEnd = lineEnd;
			ColumnEnd = columnEnd;
		}

		public static DocumentElementLocation Unknown = new DocumentElementLocation(-1, -1, -1, -1);
	}
}