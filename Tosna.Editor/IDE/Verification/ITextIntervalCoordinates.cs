using Tosna.Editor.IDE.Verification.TextIntervalCoordinates;

namespace Tosna.Editor.IDE.Verification
{
	public interface ITextIntervalCoordinates
	{
		void Accept(ITextIntervalCoordinatesVisitor visitor);
	}

	public interface ITextIntervalCoordinatesVisitor
	{
		void Visit(FullDocumentCoordinates coordinates);

		void Visit(FullLineCoordinates coordinates);

		void Visit(StartEndCoordinates coordinates);
	}
}
