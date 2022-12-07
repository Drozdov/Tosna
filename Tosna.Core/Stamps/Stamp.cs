using System;
using System.IO;

namespace Tosna.Core.Stamps
{
	public class Stamp
	{
		public Type Type { get; }

		public IStampField[] Properties { get; }

		public object SimpleTypeValue { get; }

		public StampInlinePolicy InlinePolicy { get; set; }

		public string UserId { get; set; }
		
		public string UserPublicName { get; set; }

		public FileInfo UserFilePath { get; set; }

		public Stamp UserParent { get; set; }

		public Stamp(Type type, IStampField[] properties)
		{
			Type = type;
			Properties = properties;
			InlinePolicy = type.IsValueType ? StampInlinePolicy.Always : StampInlinePolicy.Dynamic;
		}

		public Stamp(object simpleTypeValue)
		{
			Type = simpleTypeValue.GetType();
			SimpleTypeValue = simpleTypeValue;
			Properties = new IStampField[0];
		}
	}
}
