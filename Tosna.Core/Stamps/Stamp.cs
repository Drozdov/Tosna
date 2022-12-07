using System;
using System.IO;
using Tosna.Core.SerializationInterfaces;

namespace Tosna.Core.Stamps
{
	public class Stamp
	{
		public Type Type { get; }

		public IStampProperty[] Properties { get; }

		public object SimpleTypeValue { get; }

		public StampInlinePolicy InlinePolicy { get; set; }

		public string UserId { get; set; }
		
		public string UserPublicName { get; set; }

		public FileInfo UserFilePath { get; set; }

		public Stamp UserParent { get; set; }

		public Stamp(Type type, IStampProperty[] properties)
		{
			Type = type;
			Properties = properties;
			InlinePolicy = type.IsValueType ? StampInlinePolicy.Always : StampInlinePolicy.Dynamic;
		}

		public Stamp(object simpleTypeValue)
		{
			Type = simpleTypeValue.GetType();
			SimpleTypeValue = simpleTypeValue;
			Properties = new IStampProperty[0];
		}
	}

	public enum StampInlinePolicy
	{
		Always,
		Dynamic,
		Never
	}

	public interface IStampProperty
	{
		SerializingElement SerializingElement { get; }

		void Visit(IStampPropertyVisitor visitor);
	}

	public interface IStampPropertyVisitor
	{
		void Visit(SimpleTypeProperty simpleTypeProperty);

		void Visit(StampProperty stampProperty);

		void Visit(ArrayStampProperty arrayStampProperty);
	}

	public class SimpleTypeProperty : IStampProperty
	{
		public SerializingElement SerializingElement { get; }

		public object Value { get; }

		public SimpleTypeProperty(SerializingElement serializingElement, object value)
		{
			SerializingElement = serializingElement;
			Value = value;
		}

		public void Visit(IStampPropertyVisitor visitor)
		{
			visitor.Visit(this);
		}
	}

	public class StampProperty : IStampProperty
	{
		public SerializingElement SerializingElement { get; }

		public Stamp Value { get; }

		public StampProperty(SerializingElement serializingElement, Stamp value)
		{
			SerializingElement = serializingElement;
			Value = value;
		}

		public void Visit(IStampPropertyVisitor visitor)
		{
			visitor.Visit(this);
		}
	}

	public class ArrayStampProperty : IStampProperty
	{
		public SerializingElement SerializingElement { get; }

		public Stamp[] Values { get; }

		public ArrayStampProperty(SerializingElement serializingElement, Stamp[] values)
		{
			SerializingElement = serializingElement;
			Values = values;
		}

		public void Visit(IStampPropertyVisitor visitor)
		{
			visitor.Visit(this);
		}

	}
}
