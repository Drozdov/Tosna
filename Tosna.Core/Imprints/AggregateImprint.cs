using System;
using System.Collections.Generic;

namespace Tosna.Core.Imprints
{
	public sealed class AggregateImprint : Imprint
	{
		public IReadOnlyCollection<ImprintField> Fields { get; }

		public AggregateImprint(Type type, string publicName, string id, ImprintInfo info,
			IReadOnlyCollection<ImprintField> fields) : base(type, publicName, id, info)
		{
			Fields = fields;
		}

		public override void Accept(IImprintVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}