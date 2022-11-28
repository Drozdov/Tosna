using System;
using System.Collections.Generic;
using Tosna.Core.Common.Imprints;

namespace Tosna.Core.Common.Attributes
{

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
	public class ControllerDescriptorAttribute : Attribute
	{
		public Type ContextType { get; }

		public ControllerDescriptorAttribute(Type contextType)
		{
			ContextType = contextType;
		}
	}

	public abstract class ControllerDescriptorContext
	{
		private readonly IDictionary<string, string> publicNames;

		protected ControllerDescriptorContext(IDictionary<string, string> publicNames)
		{
			this.publicNames = publicNames;
		}

		public IEnumerable<NamedImprintField> GetProperties(AggregateImprint imprint)
		{
			foreach (var field in imprint.Fields)
			{
				if (publicNames.TryGetValue(field.Info.Name, out var result))
				{
					yield return new NamedImprintField(field, result);
				}
			}
		}
	}

	public class NamedImprintField
	{
		public ImprintField Field { get; }

		public string PublicName { get; }

		public NamedImprintField(ImprintField field, string publicName)
		{
			Field = field;
			PublicName = publicName;
		}
	}

}
