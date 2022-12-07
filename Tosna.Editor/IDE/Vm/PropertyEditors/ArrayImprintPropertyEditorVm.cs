using System.Collections.Generic;
using System.Linq;
using Tosna.Editor.Helpers.Vm;
using Tosna.Editor.IDE.FieldsConfigurator;
using Tosna.Editor.IDE.FieldsConfigurator.ConfigurableFields;

namespace Tosna.Editor.IDE.Vm.PropertyEditors
{
	public class ArrayImprintPropertyEditorVm : CollectionEditorVm, IPropertyEditorVm
	{
		private readonly ArrayImprintConfigurableField arrayImprintConfigurableField;

		public ArrayImprintPropertyEditorVm(ArrayImprintConfigurableField arrayImprintConfigurableField, string publicName)
		{
			this.arrayImprintConfigurableField = arrayImprintConfigurableField;

			PublicName = publicName;

			var availableItems =
				arrayImprintConfigurableField.AvailableImprints.Select(imprint => new DescriptedImprintFieldItemVm(imprint)).ToArray();
			var allowedItems = arrayImprintConfigurableField.SelectedImprints
				.Select(imprint => availableItems.FirstOrDefault(stamp => stamp.Imprint == imprint)).Where(item => item != null).ToArray();
			var forbiddenItems = availableItems.Except(allowedItems).ToArray();
			Init(allowedItems, forbiddenItems);
		}

		public string PublicName { get; }

		protected override void OnUpdate(IReadOnlyCollection<ICollectionItemVm> allowedItems, IReadOnlyCollection<ICollectionItemVm> forbiddenItems)
		{
			arrayImprintConfigurableField.Select(allowedItems.Cast<DescriptedImprintFieldItemVm>().Select(item => item.Imprint).ToArray());
		}
	}
}
