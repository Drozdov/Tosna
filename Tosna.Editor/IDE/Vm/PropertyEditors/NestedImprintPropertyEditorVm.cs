using System.Linq;
using Tosna.Editor.IDE.FieldsConfigurator;
using Tosna.Editor.IDE.FieldsConfigurator.ConfigurableFields;

namespace Tosna.Editor.IDE.Vm.PropertyEditors
{
	public class NestedImprintPropertyEditorVm : IPropertyEditorVm
	{
		private readonly NestedImprintConfigurableField nestedImprintConfigurableField;

		public NestedImprintPropertyEditorVm(NestedImprintConfigurableField nestedImprintConfigurableField, string publicName)
		{
			this.nestedImprintConfigurableField = nestedImprintConfigurableField;

			PublicName = publicName;
			AvailableStamps =
				nestedImprintConfigurableField.AvailableImprints.Select(imprint => new DescriptedImprintFieldItemVm(imprint)).ToArray();
		}

		public string PublicName { get; }

		public DescriptedImprintFieldItemVm SelectedStamp
		{
			get => AvailableStamps.FirstOrDefault(stamp => stamp.Imprint == nestedImprintConfigurableField.SelectedImprint);
			set => nestedImprintConfigurableField.Select(value.Imprint);
		}

		public DescriptedImprintFieldItemVm[] AvailableStamps { get; }
	}
}
