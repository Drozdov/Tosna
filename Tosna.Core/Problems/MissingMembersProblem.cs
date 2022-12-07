using System;
using Tosna.Core.SerializationInterfaces;

namespace Tosna.Core.Problems
{
	public class MissingMembersProblem : IComplexSerializerProblem
	{
		public int Line { get; }

		public int Column { get; }

		public string FieldName { get; }

		public Type Type { get; }

		public ISerializingElementsManager SerializingElementsManager { get; }

		public ISerializingTypesResolver TypesResolver { get; }

		public MissingMembersProblem(int line, int column, string fieldName, Type type, ISerializingElementsManager serializingElementsManager, ISerializingTypesResolver serializingTypesResolver)
		{
			Line = line;
			Column = column;
			FieldName = fieldName;
			Type = type;
			SerializingElementsManager = serializingElementsManager;
			TypesResolver = serializingTypesResolver;
		}

		public void Accept(IComplexSerializerProblemVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}
