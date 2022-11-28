namespace Tosna.Editor.IDE.Verification
{
	public interface ITextIntervalCoordinates
	{
		void Visit(IVerificationErrorCoordinatesVisitor visitor);
	}

	public interface IVerificationErrorCoordinatesVisitor
	{
		void Visit(FullDocumentCoordinates coordinates);

		void Visit(FullLineCoordinates coordinates);

		void Visit(StartEndCoordinates coordinates);
	}

	public class FullDocumentCoordinates : ITextIntervalCoordinates
	{
		public void Visit(IVerificationErrorCoordinatesVisitor visitor)
		{
			visitor.Visit(this);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return true;
		}

		public override int GetHashCode()
		{
			return 561527617;
		}
	}

	public class FullLineCoordinates : ITextIntervalCoordinates
	{
		public FullLineCoordinates(int lineNumber)
		{
			LineNumber = lineNumber;
		}

		public int LineNumber { get; }

		public void Visit(IVerificationErrorCoordinatesVisitor visitor)
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

		public void Visit(IVerificationErrorCoordinatesVisitor visitor)
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
