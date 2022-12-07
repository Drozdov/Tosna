using System.Collections.Generic;
using Tosna.Core.SerializationInterfaces;

namespace Tosna.Core.Imprints.Fields
{
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
}