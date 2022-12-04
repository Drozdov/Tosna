using Tosna.Editor.IDE.FieldsConfigurator;
using Tosna.Editor.IDE.FieldsConfigurator.ConfigurableFields;

namespace Tosna.Editor.IDE.Vm.PropertyEditors
{
	public class DoublePropertyEditorVm : IPropertyEditorVm
	{
		private readonly SimpleTypeConfigurableField property;

		public DoublePropertyEditorVm(SimpleTypeConfigurableField property, string publicName)
		{
			this.property = property;
			PublicName = publicName;
		}

		public double DoubleValue
		{
			get => (double)property.Value;
			set => property.Value = value;
		}

		public string PublicName { get; }
	}
}
