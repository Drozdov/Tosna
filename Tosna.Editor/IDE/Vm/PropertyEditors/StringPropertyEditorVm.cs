using Tosna.Editor.IDE.FieldsConfigurator;
using Tosna.Editor.IDE.FieldsConfigurator.ConfigurableFields;

namespace Tosna.Editor.IDE.Vm.PropertyEditors
{
	public class StringPropertyEditorVm : IPropertyEditorVm
	{
		private readonly SimpleTypeConfigurableField property;

		public StringPropertyEditorVm(SimpleTypeConfigurableField property, string publicName)
		{
			this.property = property;
			PublicName = publicName;
		}

		public string StringValue
		{
			get => (string)property.Value;
			set => property.Value = value;
		}

		public string PublicName { get; }
	}
}
