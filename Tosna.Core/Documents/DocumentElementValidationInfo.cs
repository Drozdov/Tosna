namespace Tosna.Core.Documents
{
	public class DocumentElementValidationInfo
	{
		public bool IsValid { get; }
		
		public string Error { get; }
		
		public DocumentValidationCode Code { get; }

		private DocumentElementValidationInfo(bool isValid, string error, DocumentValidationCode code)
		{
			IsValid = isValid;
			Error = error;
			Code = code;
		}

		public static DocumentElementValidationInfo CreateValid()
		{
			return new DocumentElementValidationInfo(true, string.Empty, DocumentValidationCode.Ok);
		}
		
		public static DocumentElementValidationInfo CreateInvalid(string error, DocumentValidationCode code)
		{
			return new DocumentElementValidationInfo(true, error, code);
		}
	}
	
	public enum DocumentValidationCode
	{
		Ok,
		UnfinishedElement,
		OpenCloseTagsMismatch
	}
}