using Tosna.Core.SerializationInterfaces;

namespace Tosna.Core.Stamps
{
	public class ArrayStampField : IStampField
	{
		public SerializingElement SerializingElement { get; }

		public Stamp[] Values { get; }

		public ArrayStampField(SerializingElement serializingElement, Stamp[] values)
		{
			SerializingElement = serializingElement;
			Values = values;
		}

		public void Accept(IStampFieldVisitor visitor)
		{
			visitor.Visit(this);
		}

	}
}