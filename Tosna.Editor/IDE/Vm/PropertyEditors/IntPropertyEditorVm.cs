using Tosna.Editor.IDE.FieldsConfigurator;
using Tosna.Editor.IDE.FieldsConfigurator.ConfigurableFields;

namespace Tosna.Editor.IDE.Vm.PropertyEditors
{
	public class IntPropertyEditorVm : IPropertyEditorVm
	{
		private readonly SimpleTypeConfigurableField property;

		public IntPropertyEditorVm(SimpleTypeConfigurableField property, string publicName)
		{
			this.property = property;
			PublicName = publicName;
		}

		public int IntValue
		{
			get => (int)property.Value;
			set => property.Value = value;
		}

		public string PublicName { get; }
	}
}
