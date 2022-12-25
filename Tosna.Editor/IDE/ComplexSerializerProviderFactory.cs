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

					switch (parsingProblem.Code)
					{
						case DocumentValidationCode.XmlUnfinishedElement when parsingProblem.ErrorParameters.Count == 1:
						{
							var unfinishedToken = parsingProblem.ErrorParameters[0];

							return new UnfinishedTypeCompletionDataProvider(
								line: parsingProblem.Location.LineStart,
								columnStart: parsingProblem.Location.ColumnStart,
								columnEnd: parsingProblem.Location.ColumnEnd,
								unfinishedPrefix: unfinishedToken,
								type: parsingProblem.ExpectedType,
								serializingElementsManager: parsingProblem.SerializingElementsManager,
								typesResolver: parsingProblem.TypesResolver
							);
						}
						
						case DocumentValidationCode.XmlOpenCloseTagsMismatch when parsingProblem.ErrorParameters.Count == 2:
						{
							var openTagName = parsingProblem.ErrorParameters[0];
							var closingTagName = parsingProblem.ErrorParameters[1];

							return new InvalidClosingTagCompletionProvider(
								line: parsingProblem.Location.LineStart,
								columnStart: parsingProblem.Location.ColumnStart,
								columnEnd: parsingProblem.Location.ColumnEnd,
								openTagName: openTagName,
								closingTagName: closingTagName);
						}
						
						default:
							return new NoneCompletionDataProvider();
					}

				default:
					return new NoneCompletionDataProvider();
			}
		}

	}
}
