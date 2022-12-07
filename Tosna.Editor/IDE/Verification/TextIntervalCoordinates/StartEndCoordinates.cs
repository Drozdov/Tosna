namespace Tosna.Editor.IDE.Verification.TextIntervalCoordinates
{
	public class StartEndCoordinates : ITextIntervalCoordinates
	{
		public int StartLineNumber { get; }

		public int StartPosition { get; }

		public int EndLineNumber { get; }

		public int EndPosition { get; }

		public StartEndCoordinates(int startLineNumber, int startPosition, int endLineNumber, int endPosition)
		{
			StartLineNumber = startLineNumber;
			StartPosition = startPosition;
			EndLineNumber = endLineNumber;
			EndPosition = endPosition;
		}

		public void Accept(ITextIntervalCoordinatesVisitor visitor)
		{
			visitor.Visit(this);
		}

		protected bool Equals(StartEndCoordinates other)
		{
			return StartLineNumber == other.StartLineNumber && StartPosition == other.StartPosition &&
			       EndLineNumber == other.EndLineNumber && EndPosition == other.EndPosition;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((StartEndCoordinates) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = StartLineNumber;
				hashCode = (hashCode * 397) ^ StartPosition;
				hashCode = (hashCode * 397) ^ EndLineNumber;
				hashCode = (hashCode * 397) ^ EndPosition;
				return hashCode;
			}
		}
	}
}