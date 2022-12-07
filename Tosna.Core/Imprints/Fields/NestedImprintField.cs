using Tosna.Core.SerializationInterfaces;

namespace Tosna.Core.Imprints.Fields
{
	public sealed class NestedImprintField : ImprintField
	{
		public Imprint NestedItem { get; }

		public NestedImprintField(SerializingElement info, Imprint nestedItem) : base(info)
		{
			NestedItem = nestedItem;
		}

		public override void Accept(IImprintFieldVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}