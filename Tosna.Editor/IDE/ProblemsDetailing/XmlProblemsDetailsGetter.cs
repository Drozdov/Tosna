using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using Tosna.Core;
using Tosna.Core.Imprints;
using Tosna.Core.Problems;
using Tosna.Core.SerializationInterfaces;
using Tosna.Editor.IDE.Verification;

namespace Tosna.Editor.IDE.ProblemsDetailing
{
	public class XmlProblemsDetailsGetter
	{
		private readonly ISerializingElementsManager serializingElementsManager;
		private readonly ISerializingTypesResolver serializingTypesResolver;

		private readonly Regex unfinishedRegex = new Regex(@"^(.*)(<([\w\d\\.]*))(\s*(<.*)?)$");

		public XmlProblemsDetailsGetter(ISerializingElementsManager serializingElementsManager, ISerializingTypesResolver serializingTypesResolver)
		{
			this.serializingElementsManager = serializingElementsManager;
			this.serializingTypesResolver = serializingTypesResolver;
		}

		public bool TryCreateVerificationError(string content, string fileName, XmlException e, out VerificationError verificationError)
		{
			var lines = Regex.Split(content, "\r\n|\r|\n");

			// Looking for unfinished types like <SomeUnfinishedType (without />) 

			// Lines numbering starts from 1!
			for (var lineNumber = e.LineNumber; lineNumber > 0; lineNumber--)
			{
				// Locating the line with unfinished tag 
				var line = lines[lineNumber - 1];
				var match = unfinishedRegex.Match(line);
				if (!match.Success)
				{
					continue;
				}

				var prefixGroup = match.Groups[1];
				var unfinishedPatternGroup = match.Groups[2];
				var nameGroup = match.Groups[3];
				var suffixGroup = match.Groups[4];

				var prefix = prefixGroup.Value;
				var suffix = suffixGroup.Value;
				var replacement = $"{prefix}<{Empty.SerializationLabel}/>{suffix}";

				// To make sure, that this problem will be solved if a write type is written and to get the proper type
				// we repeat all reading cycle but with <Empty> stub instead of unreadable fragment
				// and with a patched serializing types resolver
				var correctedContent = string.Join(Environment.NewLine,
					lines.Select((l, index) => index == lineNumber - 1 ? replacement : l));
				var xDocument = XDocument.Parse(correctedContent, LoadOptions.SetLineInfo);

				var serializer = new ImprintsSerializer(serializingElementsManager,
					new PatchedToReservedResolver(serializingTypesResolver));
				var imprints = serializer.LoadRootImprints(xDocument, fileName).GetNestedImprintsRecursively();

				var problems = GetAllProblems(imprints).ToArray();

				var invalidCastProblem = problems.OfType<InvalidCastProblem>()
					.FirstOrDefault(castProblem => castProblem.ActualType == typeof(Empty));
				if (invalidCastProblem == null && problems.Any())
				{
					verificationError = null;
					return false;
				}
				var expectedType = invalidCastProblem != null ? invalidCastProblem.DestinationType : typeof(object);

				// Information about the problem: which tag is not completed, what types can be passed,
				// column and line numbers
				var provider = new UnfinishedTypeCompletionDataProvider(line: lineNumber,
					columnStart: unfinishedPatternGroup.Index + 1,
					columnEnd: unfinishedPatternGroup.Index + unfinishedPatternGroup.Length + 1,
					unfinishedPrefix: nameGroup.Value,
					type: expectedType,
					serializingElementsManager: serializingElementsManager,
					typesResolver: serializingTypesResolver);

				verificationError = new VerificationError(fileName,
					new StartEndCoordinates(provider.Line, provider.ColumnStart, lineNumber, provider.ColumnEnd),
					$"Unfinished item '{nameGroup.Value}'", provider);
				return true;
			}

			verificationError = null;
			return false;
		}

		private static IEnumerable<IComplexSerializerProblem> GetAllProblems(IEnumerable<Imprint> imprints)
		{
			return imprints.GetNestedImprintsRecursively().SelectMany(imprint =>
				imprint.TryGetInfo(out var info) ? info.Problems : Enumerable.Empty<IComplexSerializerProblem>());
		}
	}
}
