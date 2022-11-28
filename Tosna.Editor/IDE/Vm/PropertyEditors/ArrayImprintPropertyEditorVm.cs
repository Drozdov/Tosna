using System.Collections.Generic;
using System.Linq;
using Tosna.Editor.IDE.Vm.CollectionsEditing;

namespace Tosna.Editor.IDE.Vm.PropertyEditors
{
	public class ArrayImprintPropertyEditorVm : CollectionEditorVm, IPropertyEditorVm
	{
		private readonly ArrayImprintDescriptedField arrayImprintDescriptedField;

		public ArrayImprintPropertyEditorVm(ArrayImprintDescriptedField arrayImprintDescriptedField, string publicName)
		{
			this.arrayImprintDescriptedField = arrayImprintDescriptedField;

			PublicName = publicName;

			var availableItems =
				arrayImprintDescriptedField.AvailableImprints.Select(imprint => new DescriptedImprintFieldItemVm(imprint)).ToArray();
			var allowedItems = arrayImprintDescriptedField.SelectedImprints
				.Select(imprint => availableItems.FirstOrDefault(stamp => stamp.Imprint == imprint)).Where(item => item != null).ToArray();
			var forbiddenItems = availableItems.Except(allowedItems).ToArray();
			Init(allowedItems, forbiddenItems);
		}

		public string PublicName { get; }

		protected override void OnUpdate(IReadOnlyCollection<ICollectionItemVm> allowedItems, IReadOnlyCollection<ICollectionItemVm> forbiddenItems)
		{
			arrayImprintDescriptedField.Select(allowedItems.Cast<DescriptedImprintFieldItemVm>().Select(item => item.Imprint).ToArray());
		}
	}
}
