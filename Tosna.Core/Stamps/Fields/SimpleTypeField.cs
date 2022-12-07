using Tosna.Core.SerializationInterfaces;

namespace Tosna.Core.Stamps
{
	public class SimpleTypeField : IStampField
	{
		public SerializingElement SerializingElement { get; }

		public object Value { get; }

		public SimpleTypeField(SerializingElement serializingElement, object value)
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