using System;
using Tosna.Core.SerializationIterfaces;

namespace Tosna.Editor.IDE.Verification
{
	public abstract class VerificationNotification
	{
		public string FileName { get; }

		public ITextIntervalCoordinates Coordinates { get; }

		public string Message { get; }

		public ICompletionDataProvider CompletionDataProvider { get; }

		public abstract VerificationNotificationType NotificationType { get; }

		protected VerificationNotification(string fileName, ITextIntervalCoordinates coordinates, string message, ICompletionDataProvider completionDataProvider)
		{
			FileName = fileName;
			Coordinates = coordinates;
			Message = message;
			CompletionDataProvider = completionDataProvider;
		}
	}

	public enum VerificationNotificationType
	{
		Error,
		Warning
	}

	public interface ICompletionDataProvider
	{
		void Visit(ICompletionDataProviderVisitor visitor);
	}

	public interface ICompletionDataProviderVisitor
	{
		void Visit(NoneCompletionDataProvider provider);

		void Visit(MissingMembersCompletionDataProvider provider);

		void Visit(UnfinishedTypeCompletionDataProvider provider);
	}

	public class NoneCompletionDataProvider : ICompletionDataProvider
	{
		void ICompletionDataProvider.Visit(ICompletionDataProviderVisitor visitor)
		{
			visitor.Visit(this);
		}
	}

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

		void ICompletionDataProvider.Visit(ICompletionDataProviderVisitor visitor)
		{
			visitor.Visit(this);
		}
	}

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

		void ICompletionDataProvider.Visit(ICompletionDataProviderVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}
