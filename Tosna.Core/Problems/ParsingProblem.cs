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

		public DocumentValidationCode Code { get; }

		public Type ExpectedType { get; }

		public IReadOnlyCollection<string> ErrorParameters { get; }

		public ISerializingElementsManager SerializingElementsManager { get; }

		public ISerializingTypesResolver TypesResolver { get; }

		public bool IsCritical => true;

		public ParsingProblem(string description, DocumentElementLocation location, DocumentValidationCode code,
			Type expectedType, IReadOnlyCollection<string> errorParameters,
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