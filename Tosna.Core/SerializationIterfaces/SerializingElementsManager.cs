using System;
using System.Collections.Generic;

namespace Tosna.Core.SerializationIterfaces
{
	public class SerializingElementsManager : ISerializingElementsManager
	{
		public IEnumerable<SerializingElement> GetAllElements(Type type)
		{
			var fields = type.GetFields();
			// TODO: backing fields
			foreach (var field in fields)
			{
				yield return new SerializingElement(field.SetValue, field.GetValue, field.FieldType, field.Name, true,
					GetDefault(field.FieldType));
			}
		}
		
		private static object GetDefault(Type type)
		{
			return type.IsValueType ? Activator.CreateInstance(type) : null;
		}
	}
}