namespace Tosna.Editor.IDE.Verification.TextIntervalCoordinates
{
	public class FullLineCoordinates : ITextIntervalCoordinates
	{
		public FullLineCoordinates(int lineNumber)
		{
			LineNumber = lineNumber;
		}

		public int LineNumber { get; }

		public void Accept(ITextIntervalCoordinatesVisitor visitor)
		{
			visitor.Visit(this);
		}

		protected bool Equals(FullLineCoordinates other)
		{
			return LineNumber == other.LineNumber;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((FullLineCoordinates) obj);
		}

		public override int GetHashCode()
		{
			return LineNumber;
		}
	}
}