namespace Tosna.Editor.IDE.Verification.CompletionDataProviders
{
	public class InvalidClosingTagCompletionProvider : ICompletionDataProvider
	{
		public int Line { get; }

		public int ColumnStart { get; }

		public int ColumnEnd { get; }

		public string OpenTagName { get; }

		public string ClosingTagName { get; }
		
		public InvalidClosingTagCompletionProvider(int line, int columnStart, int columnEnd, string openTagName, string closingTagName)
		{
			Line = line;
			ColumnStart = columnStart;
			ColumnEnd = columnEnd;
			OpenTagName = openTagName;
			ClosingTagName = closingTagName;
		}

		public void Accept(ICompletionDataProviderVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}