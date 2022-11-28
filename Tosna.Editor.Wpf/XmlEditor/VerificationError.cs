using ICSharpCode.AvalonEdit.Document;
using Tosna.Editor.IDE.Verification;
using Tosna.Editor.Wpf.XmlEditor.TextMarkers;

namespace Tosna.Editor.Wpf.XmlEditor
{

	public class TextMarkerFactory : IVerificationErrorCoordinatesVisitor
	{
		private readonly ITextMarkerService textMarkerService;
		private readonly TextDocument textDocument;

		private ITextMarker marker;

		private TextMarkerFactory(ITextMarkerService textMarkerService, TextDocument textDocument)
		{
			this.textMarkerService = textMarkerService;
			this.textDocument = textDocument;
		}

		public static ITextMarker CreateMarker(ITextIntervalCoordinates coordinates, ITextMarkerService textMarkerService,
			TextDocument textDocument)
		{
			var factory = new TextMarkerFactory(textMarkerService, textDocument);
			coordinates.Visit(factory);
			return factory.marker;
		}

		public void Visit(FullDocumentCoordinates coordinates)
		{
			marker = textMarkerService.Create(0, textDocument.TextLength);
		}

		public void Visit(FullLineCoordinates coordinates)
		{
			var line = textDocument.Lines[coordinates.LineNumber - 1];
			marker = textMarkerService.Create(line.Offset, line.Length);
		}

		public void Visit(StartEndCoordinates coordinates)
		{
			var startOffset = textDocument.GetOffset(coordinates.StartLineNumber, coordinates.StartPosition);
			var endOffset = textDocument.GetOffset(coordinates.EndLineNumber, coordinates.EndPosition);
			marker = textMarkerService.Create(startOffset, endOffset - startOffset);
		}
	}

	public class TextIntervalCoordinatesChecker : IVerificationErrorCoordinatesVisitor
	{
		private readonly TextLocation location;

		private bool result;

		private TextIntervalCoordinatesChecker(TextLocation location)
		{
			this.location = location;
		}

		public static bool Contains(ITextIntervalCoordinates coordinates, TextLocation position)
		{
			var checker = new TextIntervalCoordinatesChecker(position);
			coordinates.Visit(checker);
			return checker.result;
		}

		public void Visit(FullDocumentCoordinates coordinates)
		{
			result = true;
		}

		public void Visit(FullLineCoordinates coordinates)
		{
			result = location.Line == coordinates.LineNumber;
		}

		public void Visit(StartEndCoordinates coordinates)
		{
			if (coordinates.StartLineNumber == coordinates.EndLineNumber)
			{
				result =
					location.Line == coordinates.StartLineNumber && coordinates.StartPosition <= location.Column && location.Column <= coordinates.EndPosition;
			}
			else
			{
				result =
					location.Line == coordinates.StartLineNumber && location.Column >= coordinates.StartPosition ||
					coordinates.StartLineNumber < location.Line && location.Line < coordinates.EndLineNumber ||
					location.Line == coordinates.EndLineNumber && location.Column <= coordinates.EndPosition;
			}
		}
	}
}
