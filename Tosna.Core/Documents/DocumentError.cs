namespace Tosna.Core.Documents
{
	public class DocumentError
	{
		public string Error { get; }

		public DocumentErrorCode Code { get; }

		public string[] ErrorParameters { get; }
		
		public DocumentElementLocation ProblemLocation { get; }

		public DocumentError(string error, DocumentErrorCode code,
			DocumentElementLocation problemLocation, params string[] errorParameters)
		{
			Error = error;
			Code = code;
			ProblemLocation = problemLocation;
			ErrorParameters = errorParameters;
		}
	}

	public enum DocumentErrorCode
	{
		LexerProblem,
		ParsingProblem,

		// XML specific
		XmlUnfinishedElement,
		XmlOpenCloseTagsMismatch
	}
}