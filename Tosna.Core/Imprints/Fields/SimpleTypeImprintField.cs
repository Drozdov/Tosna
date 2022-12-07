using Tosna.Core.SerializationInterfaces;

namespace Tosna.Core.Imprints.Fields
{
	public sealed class SimpleTypeImprintField : ImprintField
	{
		public object Value { get; }

		public SimpleTypeImprintField(SerializingElement info, object value) : base(info)
		{
			Value = value;
		}

		public override void Accept(IImprintFieldVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}