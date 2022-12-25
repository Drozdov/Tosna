namespace Tosna.Core.Documents
{
	public class DocumentElementValidationInfo
	{
		public bool IsValid { get; }

		public string Error { get; }

		public DocumentValidationCode Code { get; }

		public string[] ErrorParameters { get; }
		
		public DocumentElementLocation ProblemLocation { get; }

		private DocumentElementValidationInfo(bool isValid, string error, DocumentValidationCode code,
			DocumentElementLocation problemLocation, string[] errorParameters)
		{
			IsValid = isValid;
			Error = error;
			Code = code;
			ProblemLocation = problemLocation;
			ErrorParameters = errorParameters;
		}

		public static DocumentElementValidationInfo CreateValid()
		{
			return new DocumentElementValidationInfo(true, string.Empty, DocumentValidationCode.Ok,
				DocumentElementLocation.Unknown, new string[] { });
		}

		public static DocumentElementValidationInfo CreateInvalid(string error, DocumentValidationCode code,
			DocumentElementLocation problemLocation, params string[] errorParameters)
		{
			return new DocumentElementValidationInfo(false, error, code, problemLocation, errorParameters);
		}
	}

	public enum DocumentValidationCode
	{
		Ok,
		ParsingProblem,
		
		// XML specific
		XmlUnfinishedElement,
		XmlOpenCloseTagsMismatch
	}
}