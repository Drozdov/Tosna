using System.Linq;
using Tosna.Core.Documents;
using Tosna.Core.Problems;
using Tosna.Editor.IDE.Verification;
using Tosna.Editor.IDE.Verification.CompletionDataProviders;

namespace Tosna.Editor.IDE
{
	public static class ComplexSerializerProviderFactory
	{
		public static ICompletionDataProvider GetProvider(IComplexSerializerProblem problem)
		{
			switch (problem)
			{
				case MissingMembersProblem missingMembersProblem:
					return new MissingMembersCompletionDataProvider(
						missingMembersProblem.Location.LineStart,
						missingMembersProblem.Location.ColumnStart,
						missingMembersProblem.Type,
						missingMembersProblem.SerializingElementsManager,
						missingMembersProblem.TypesResolver);
				
				case ParsingProblem parsingProblem:

					var unfinishedToken = parsingProblem.ErrorParameters.FirstOrDefault();
					if (parsingProblem.Code == DocumentValidationCode.XmlUnfinishedElement && unfinishedToken != null)
					{
						return new UnfinishedTypeCompletionDataProvider(
							parsingProblem.Location.LineStart,
							parsingProblem.Location.ColumnStart,
							parsingProblem.Location.ColumnEnd,
							unfinishedToken,
							parsingProblem.ExpectedType,
							parsingProblem.SerializingElementsManager,
							parsingProblem.TypesResolver
							);
					}

					return new NoneCompletionDataProvider();
				
				default:
					return new NoneCompletionDataProvider();
			}
		}

	}
}
