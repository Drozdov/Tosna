using System;
using Tosna.Core.SerializationInterfaces;

namespace Tosna.Editor.IDE.Verification.CompletionDataProviders
{
	public class MissingMembersCompletionDataProvider : ICompletionDataProvider
	{
		public int Line { get; }

		public int Column { get; }

		public Type Type { get; }

		public ISerializingElementsManager SerializingElementsManager { get; }

		public ISerializingTypesResolver TypesResolver { get; }

		public MissingMembersCompletionDataProvider(int line, int column, Type type, ISerializingElementsManager serializingElementsManager, ISerializingTypesResolver typesResolver)
		{
			Line = line;
			Column = column;
			Type = type;
			SerializingElementsManager = serializingElementsManager;
			TypesResolver = typesResolver;
		}

		void ICompletionDataProvider.Accept(ICompletionDataProviderVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}