using Tosna.Core.SerializationInterfaces;

namespace Tosna.Core.Stamps
{
	public class StampField : IStampField
	{
		public SerializingElement SerializingElement { get; }

		public Stamp Value { get; }

		public StampField(SerializingElement serializingElement, Stamp value)
		{
			SerializingElement = serializingElement;
			Value = value;
		}

		public void Accept(IStampFieldVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}