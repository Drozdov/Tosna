using System;
using System.Collections.Generic;
using Tosna.Core.Documents;
using Tosna.Core.SerializationInterfaces;

namespace Tosna.Core.Problems
{
	public class ParsingProblem : IComplexSerializerProblem
	{
		public string Description { get; }

		public DocumentElementLocation Location { get; }

		public DocumentErrorCode Code { get; }

		public Type ExpectedType { get; }

		public IReadOnlyList<string> ErrorParameters { get; }

		public ISerializingElementsManager SerializingElementsManager { get; }

		public ISerializingTypesResolver TypesResolver { get; }

		public bool IsCritical => true;

		public ParsingProblem(string description, DocumentElementLocation location, DocumentErrorCode code,
			Type expectedType, IReadOnlyList<string> errorParameters,
			ISerializingElementsManager serializingElementsManager, ISerializingTypesResolver typesResolver)
		{
			Description = description;
			Location = location;
			Code = code;
			ExpectedType = expectedType;
			ErrorParameters = errorParameters;
			SerializingElementsManager = serializingElementsManager;
			TypesResolver = typesResolver;
		}
	}
}