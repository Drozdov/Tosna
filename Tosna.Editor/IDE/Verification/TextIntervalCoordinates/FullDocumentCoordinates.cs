namespace Tosna.Editor.IDE.Verification.TextIntervalCoordinates
{
	public class FullDocumentCoordinates : ITextIntervalCoordinates
	{
		public void Accept(ITextIntervalCoordinatesVisitor visitor)
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
}