using Tosna.Editor.IDE.FieldsConfigurator;
using Tosna.Editor.IDE.FieldsConfigurator.ConfigurableFields;

namespace Tosna.Editor.IDE.Vm.PropertyEditors
{
	public class BoolPropertyEditorVm : IPropertyEditorVm
	{
		private readonly SimpleTypeConfigurableField property;
		
		public BoolPropertyEditorVm(SimpleTypeConfigurableField property, string publicName)
		{
			this.property = property;
			PublicName = publicName;
		}

		public bool BoolValue
		{
			get => (bool) property.Value;
			set => property.Value = value;
		}

		public string PublicName { get; }
	}
}
