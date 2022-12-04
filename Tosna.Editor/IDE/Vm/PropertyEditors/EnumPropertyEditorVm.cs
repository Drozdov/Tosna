using System;
using Tosna.Editor.IDE.FieldsConfigurator;
using Tosna.Editor.IDE.FieldsConfigurator.ConfigurableFields;

namespace Tosna.Editor.IDE.Vm.PropertyEditors
{
	public class EnumPropertyEditorVm : IPropertyEditorVm
	{
		private readonly SimpleTypeConfigurableField property;

		public EnumPropertyEditorVm(SimpleTypeConfigurableField property, string publicName)
		{
			this.property = property;
			PublicName = publicName;

			Values = Enum.GetValues(property.Value.GetType());
		}

		public string PublicName { get; set; }

		public object Values { get; }

		public object Value
		{
			get => property.Value;
			set => property.Value = value;
		}
	}
}
