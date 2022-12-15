namespace Tosna.Core.Documents
{
	public class DocumentElementValidationInfo
	{
		public bool IsValid { get; }
		
		public string Error { get; }

		private DocumentElementValidationInfo(bool isValid, string error)
		{
			IsValid = isValid;
			Error = error;
		}

		public static DocumentElementValidationInfo CreateValid()
		{
			return new DocumentElementValidationInfo(true, string.Empty);
		}
		
		public static DocumentElementValidationInfo CreateInvalid(string error)
		{
			return new DocumentElementValidationInfo(true, error);
		}
	}
}