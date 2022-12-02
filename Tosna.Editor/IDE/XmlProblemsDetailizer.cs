using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using Tosna.Core.Common;
using Tosna.Core.Common.Imprints;
using Tosna.Core.Common.Problems;
using Tosna.Core.SerializationInterfaces;
using Tosna.Editor.IDE.Reserved;
using Tosna.Editor.IDE.Verification;

namespace Tosna.Editor.IDE
{
	public class XmlProblemsDetailizer
	{
		private readonly ImprintsSerializer serializer;
		private readonly ISerializingElementsManager serializingElementsManager;
		private readonly ISerializingTypesResolver serializingTypesResolver;

		private readonly Regex unfinishedRegex = new Regex(@"^(.*)(<([\w\d\\.]*))(\s*(<.*)?)$");

		public XmlProblemsDetailizer(ImprintsSerializer serializer, ISerializingElementsManager serializingElementsManager, ISerializingTypesResolver serializingTypesResolver)
		{
			this.serializer = serializer;
			this.serializingElementsManager = serializingElementsManager;
			this.serializingTypesResolver = serializingTypesResolver;
		}

		public bool TryCreateVerificationError(string content, string fileName, XmlException e, out VerificationError verificationError)
		{
			var lines = Regex.Split(content, "\r\n|\r|\n");

			// Пытаемся найти незавершенный тип вида <SomeUnfinishedType (без />) и собрать по нему информацию 

			// Строки нумеруются с 1!
			for (var lineNumber = e.LineNumber; lineNumber > 0; lineNumber--)
			{
				// Находим строчку, в которой не закрыт тег 
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

				// Чтобы убедиться, что, дописав правильный тип, проблема решится, а также определив, какой именно тип нам нужен,
				// повтороно прогоняем весь цикл чтения, но вместо нечитаемого фрагмента подставляем заглушку для типа Empty
				var correctedContent = string.Join(Environment.NewLine,
					lines.Select((l, index) => index == lineNumber - 1 ? replacement : l));
				var xDocument = XDocument.Parse(correctedContent, LoadOptions.SetLineInfo);
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

				// Информация о проблеме для дальнейших дествий: какой тег не закрыт, какие типы туда можно подставить, строки, столбцы,
				// способы сериализации полей и типов
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
