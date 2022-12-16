namespace Tosna.Core.Documents
{
	public class DocumentElementValidationInfo
	{
		public bool IsValid { get; }

		public string Error { get; }

		public DocumentValidationCode Code { get; }

		public string[] ErrorParameters { get; }

		private DocumentElementValidationInfo(bool isValid, string error, DocumentValidationCode code,
			string[] errorParameters)
		{
			IsValid = isValid;
			Error = error;
			Code = code;
			ErrorParameters = errorParameters;
		}

		public static DocumentElementValidationInfo CreateValid()
		{
			return new DocumentElementValidationInfo(true, string.Empty, DocumentValidationCode.Ok, new string[] { });
		}

		public static DocumentElementValidationInfo CreateInvalid(string error, DocumentValidationCode code,
			params string[] errorParameters)
		{
			return new DocumentElementValidationInfo(false, error, code, errorParameters);
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