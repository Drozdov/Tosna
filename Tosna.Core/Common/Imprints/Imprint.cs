using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using Tosna.Core.Common.Problems;
using Tosna.Core.Helpers;
using Tosna.Core.SerializationInterfaces;

namespace Tosna.Core.Common.Imprints
{
	public abstract class Imprint
	{
		public Type Type { get; }

		private readonly string publicName;
		private readonly string id;
		private readonly ImprintInfo info;

		public string FilePath
		{
			get
			{
				if (TryGetInfo(out var imprintInfo))
				{
					return imprintInfo.FilePath;
				}

				throw new Exception("No info");
			}
		}

		protected Imprint(Type type, string publicName, string id, ImprintInfo info)
		{
			Contract.Requires(Type != null);

			Type = type;
			this.publicName = publicName;
			this.id = id;
			this.info = info;
		}

		// ReSharper disable once ParameterHidesMember
		public bool TryGetPublicName(out string publicName)
		{
			publicName = this.publicName;
			return publicName != null;
		}
		
		// ReSharper disable once ParameterHidesMember
		public bool TryGetId(out string id)
		{
			id = this.id;
			return id != null;
		}

		// ReSharper disable once ParameterHidesMember
		public bool TryGetInfo(out ImprintInfo info)
		{
			info = this.info;
			return info != null;
		}

		public abstract void Visit(IImprintVisitor visitor);
	}

	public interface IImprintVisitor
	{
		void Visit(SimpleTypeImprint imprint);

		void Visit(AggregateImprint imprint);

		void Visit(ReferenceImprint imprint);
	}

	public sealed class SimpleTypeImprint : Imprint
	{
		public object Value { get; }

		public SimpleTypeImprint(Type type, string publicName, string id, ImprintInfo info, object value) : base(type, publicName, id, info)
		{
			Value = value;
		}

		public override void Visit(IImprintVisitor visitor)
		{
			visitor.Visit(this);
		}
	}

	public sealed class AggregateImprint : Imprint
	{
		public IReadOnlyCollection<ImprintField> Fields { get; }

		public AggregateImprint(Type type, string publicName, string id, ImprintInfo info, IReadOnlyCollection<ImprintField> fields) : base(type, publicName, id, info)
		{
			Fields = fields;
		}

		public override void Visit(IImprintVisitor visitor)
		{
			visitor.Visit(this);
		}
	}

	public sealed class ReferenceImprint : Imprint
	{
		public string ReferenceId { get; }

		public string ReferenceRelativePath { get; }

		public ReferenceImprint(Type type, string publicName, string id, ImprintInfo info, string referenceId, string referenceRelativePath) : base(type, publicName, id, info)
		{
			ReferenceId = referenceId;
			ReferenceRelativePath = referenceRelativePath;
		}

		public override void Visit(IImprintVisitor visitor)
		{
			visitor.Visit(this);
		}

		public string AbsoluteReferencePath
		{
			get
			{
				if (!TryGetInfo(out var info))
				{
					throw new InvalidOperationException("Cannot get absolute path for reference. No info");
				}

				if (string.IsNullOrEmpty(ReferenceRelativePath))
				{
					return info.FilePath;
				}

				var directoryName = Path.GetDirectoryName(info.FilePath);

				if (directoryName == null)
				{
					throw new InvalidOperationException("Cannot get directory name form imprint info.");
				}

				return PathUtils.GetAbsolutePath(ReferenceRelativePath, new DirectoryInfo(directoryName));
			}
		}
	}

	public abstract class ImprintField
	{
		protected ImprintField(SerializingElement info)
		{
			Info = info;
		}

		public SerializingElement Info { get; }

		public abstract void Visit(IImprintFieldVisitor visitor);
	}

	public interface IImprintFieldVisitor
	{
		void Visit(SimpleTypeImprintField field);

		void Visit(NestedImprintField field);

		void Visit(ArrayImprintField field);
	}

	public sealed class SimpleTypeImprintField : ImprintField
	{
		public object Value { get; }

		public SimpleTypeImprintField(SerializingElement info, object value) : base(info)
		{
			Value = value;
		}

		public override void Visit(IImprintFieldVisitor visitor)
		{
			visitor.Visit(this);
		}
	}

	public sealed class NestedImprintField : ImprintField
	{
		public Imprint NestedItem { get; }

		public NestedImprintField(SerializingElement info, Imprint nestedItem) : base(info)
		{
			NestedItem = nestedItem;
		}

		public override void Visit(IImprintFieldVisitor visitor)
		{
			visitor.Visit(this);
		}
	}

	public sealed class ArrayImprintField : ImprintField
	{
		public IReadOnlyList<Imprint> Items { get; }

		public ArrayImprintField(SerializingElement info, IReadOnlyList<Imprint> items) : base(info)
		{
			Items = items;
		}

		public override void Visit(IImprintFieldVisitor visitor)
		{
			visitor.Visit(this);
		}
	}

	public sealed class ImprintInfo
	{
		public string FilePath { get; }

		public string TypeName { get; }

		public int Line { get; }

		public int Column { get; }

		public IReadOnlyCollection<IComplexSerializerProblem> Problems { get; }

		public ImprintInfo(string filePath, string typeName, int line, int column, params IComplexSerializerProblem[] problems)
		{
			FilePath = filePath;
			TypeName = typeName;
			Line = line;
			Column = column;
			Problems = problems;
		}
	}
}
