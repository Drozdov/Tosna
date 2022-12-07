using System;
using Tosna.Core.SerializationInterfaces;

namespace Tosna.Editor.IDE.Verification.CompletionDataProviders
{
	public class UnfinishedTypeCompletionDataProvider : ICompletionDataProvider
	{
		public int Line { get; }

		public int ColumnStart { get; }

		public int ColumnEnd { get; }

		public string UnfinishedPrefix { get; }

		public Type Type { get; }

		public ISerializingElementsManager SerializingElementsManager { get; }

		public ISerializingTypesResolver TypesResolver { get; }

		public UnfinishedTypeCompletionDataProvider(int line, int columnStart, int columnEnd, string unfinishedPrefix,
			Type type, ISerializingElementsManager serializingElementsManager, ISerializingTypesResolver typesResolver)
		{
			Line = line;
			ColumnStart = columnStart;
			ColumnEnd = columnEnd;
			UnfinishedPrefix = unfinishedPrefix;
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