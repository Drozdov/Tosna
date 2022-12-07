using Tosna.Core.Imprints.Fields;

namespace Tosna.Editor.IDE.FieldsConfigurator.ConfigurableFields
{
	public class SimpleTypeConfigurableField : ConfigurableField
	{
		private readonly FieldsConfiguratorManager fieldsConfiguratorManager;
		private SimpleTypeImprintField imprintField;

		public SimpleTypeConfigurableField(string publicName, FieldsConfiguratorManager fieldsConfiguratorManager, SimpleTypeImprintField imprintField) : base(publicName)
		{
			this.fieldsConfiguratorManager = fieldsConfiguratorManager;
			this.imprintField = imprintField;
		}

		public object Value
		{
			get => imprintField.Value;
			set
			{
				if (Value == value)
				{
					return;
				}
				fieldsConfiguratorManager.Edit(imprintField,
					imprintField = new SimpleTypeImprintField(imprintField.Info, value));
			}
		}
	}
}