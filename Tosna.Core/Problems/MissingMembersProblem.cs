using System;
using Tosna.Core.Documents;
using Tosna.Core.SerializationInterfaces;

namespace Tosna.Core.Problems
{
	public class MissingMembersProblem : IComplexSerializerProblem
	{
		public string FieldName { get; }

		public Type Type { get; }
				
		public DocumentElementLocation Location { get; }
		
		public ISerializingElementsManager SerializingElementsManager { get; }

		public ISerializingTypesResolver TypesResolver { get; }

		public string Description => $"Missing field {FieldName} for {Type.FullName}";
		
		public bool IsCritical => true;

		public MissingMembersProblem(string fieldName, Type type, DocumentElementLocation location, ISerializingElementsManager serializingElementsManager, ISerializingTypesResolver typesResolver)
		{
			FieldName = fieldName;
			Type = type;
			Location = location;
			SerializingElementsManager = serializingElementsManager;
			TypesResolver = typesResolver;
		}
	}
}
