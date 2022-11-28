using System.Linq;

namespace Tosna.Editor.IDE.Vm.PropertyEditors
{
	public class NestedImprintPropertyEditorVm : IPropertyEditorVm
	{
		private readonly NestedImprintDescriptedField nestedImprintDescriptedField;

		public NestedImprintPropertyEditorVm(NestedImprintDescriptedField nestedImprintDescriptedField, string publicName)
		{
			this.nestedImprintDescriptedField = nestedImprintDescriptedField;

			PublicName = publicName;
			AvailableStamps =
				nestedImprintDescriptedField.AvailableImprints.Select(imprint => new DescriptedImprintFieldItemVm(imprint)).ToArray();
		}

		public string PublicName { get; }

		public DescriptedImprintFieldItemVm SelectedStamp
		{
			get => AvailableStamps.FirstOrDefault(stamp => stamp.Imprint == nestedImprintDescriptedField.SelectedImprint);
			set => nestedImprintDescriptedField.Select(value.Imprint);
		}

		public DescriptedImprintFieldItemVm[] AvailableStamps { get; }
	}
}
