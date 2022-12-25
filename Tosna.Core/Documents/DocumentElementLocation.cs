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
		
		public static bool operator ==(DocumentElementLocation location1, DocumentElementLocation location2)
		{
			return Equals(location1, location2);
		}

		public static bool operator !=(DocumentElementLocation location1, DocumentElementLocation location2)
		{
			return !(location1 == location2);
		}
	}
}