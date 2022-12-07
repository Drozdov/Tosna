using System;
using System.Diagnostics.Contracts;

namespace Tosna.Core.Imprints
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

		public abstract void Accept(IImprintVisitor visitor);
	}

	public interface IImprintVisitor
	{
		void Visit(SimpleTypeImprint imprint);

		void Visit(AggregateImprint imprint);

		void Visit(ReferenceImprint imprint);
	}
}
