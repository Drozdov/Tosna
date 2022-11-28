using System;

namespace Tosna.Core.SerializationIterfaces
{
	public class SerializingElement
	{
		private readonly Action<object, object> setValueAction;
		private readonly Func<object, object> getValueFunc; 

		public Type Type { get; }
		public string Name { get; }
		public bool IsMandatory { get; }
		public object DefaultValue { get; }

		public SerializingElement(Action<object, object> setValueAction, Func<object, object> getValueFunc, Type type, string name, bool isMandatory, object defaultValue)
		{
			this.setValueAction = setValueAction;
			this.getValueFunc = getValueFunc;
			Type = type;
			Name = name;
			IsMandatory = isMandatory;
			DefaultValue = defaultValue;
		}

		public void SetValue(object o, object value)
		{
			setValueAction(o, value);
		}

		public object GetValue(object o)
		{
			return getValueFunc(o);
		}
	}
}