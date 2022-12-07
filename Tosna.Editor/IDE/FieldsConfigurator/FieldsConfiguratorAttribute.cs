using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Tosna.Core.Imprints;

namespace Tosna.Editor.IDE.FieldsConfigurator
{

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
	public class FieldsConfiguratorAttribute : Attribute
	{
		public Type ContextType { get; }

		/// <summary>
		/// This attribute enables GUI editor for a specific class or struct 
		/// </summary>
		/// <param name="contextType">Subclass of <see cref="FieldsConfiguratorContext"/> with parameterless constructor</param>
		public FieldsConfiguratorAttribute(Type contextType)
		{
			Contract.Requires(contextType.IsSubclassOf(typeof(FieldsConfiguratorContext)));
			
			ContextType = contextType;
		}
	}
	
	public abstract class FieldsConfiguratorContext
	{
		private readonly IDictionary<string, string> publicNames;

		protected FieldsConfiguratorContext(IDictionary<string, string> publicNames)
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
