using System;
using System.IO;
using System.Linq;
using Tosna.Core.SerializationIterfaces;

namespace Tosna.Core.Common.Stamps
{
	public class Stamp
	{
		public Type Type { get; }

		public IStampProperty[] Properties { get; }

		public object SimpleTypeValue { get; }

		public StampInlinePolicy InlinePolicy { get; set; }

		public string UserId { get; set; }

		public FileInfo UserFilePath { get; set; }

		public Stamp UserParent { get; set; }

		public Stamp(Type type, IStampProperty[] properties)
		{
			Type = type;
			Properties = properties;
			InlinePolicy = type.IsValueType ? StampInlinePolicy.Always : StampInlinePolicy.Dynamic;

			foreach (var property in Properties)
			{
				property.Changed += (sender, args) => Changed(this, args);
			}
		}

		public Stamp(object simpleTypeValue)
		{
			Type = simpleTypeValue.GetType();
			SimpleTypeValue = simpleTypeValue;
			Properties = new IStampProperty[0];
		}

		public event EventHandler Changed = delegate { };
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

		event EventHandler Changed;
	}

	public interface IStampPropertyVisitor
	{
		void Visit(SimpleTypeProperty simpleTypeProperty);

		void Visit(StampProperty stampProperty);

		void Visit(ArrayStampProperty arrayStampProperty);
	}

	public class SimpleTypeProperty : IStampProperty
	{
		private object value;

		public SerializingElement SerializingElement { get; }

		public object Value
		{
			get => value;
			set
			{
				this.value = value;
				Changed(this, EventArgs.Empty);
			}
		}

		public SimpleTypeProperty(SerializingElement serializingElement, object value)
		{
			SerializingElement = serializingElement;
			Value = value;
		}

		public void Visit(IStampPropertyVisitor visitor)
		{
			visitor.Visit(this);
		}

		public event EventHandler Changed = delegate { };
	}

	public class StampProperty : IStampProperty
	{
		private Stamp value;

		public SerializingElement SerializingElement { get; }

		public Stamp Value
		{
			get => value;
			set
			{
				if (this.value == value)
				{
					return;
				}
				this.value = value;
				Changed(this, EventArgs.Empty);
			}
		}

		public StampProperty(SerializingElement serializingElement, Stamp value)
		{
			SerializingElement = serializingElement;
			Value = value;
		}

		public void Visit(IStampPropertyVisitor visitor)
		{
			visitor.Visit(this);
		}

		public event EventHandler Changed = delegate { };
	}

	public class ArrayStampProperty : IStampProperty
	{
		private Stamp[] values;
		public SerializingElement SerializingElement { get; }

		public Stamp[] Values
		{
			get => values;
			set
			{
				if (value != null && values != null && value.SequenceEqual(values))
				{
					return;
				}

				values = value; 
				Changed(this, EventArgs.Empty);
			}
		}

		public ArrayStampProperty(SerializingElement serializingElement, Stamp[] values)
		{
			SerializingElement = serializingElement;
			Values = values;
		}

		public void Visit(IStampPropertyVisitor visitor)
		{
			visitor.Visit(this);
		}

		public event EventHandler Changed = delegate { };
	}
}
