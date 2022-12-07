using System.Linq;
using System.Windows.Controls;
using Tosna.Editor.Helpers.Vm;

namespace Tosna.Editor.Wpf.Common
{
	public partial class CollectionEditorControl : UserControl
	{
		public CollectionEditorControl()
		{
			InitializeComponent();
		}

		private void OnAllowedSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var listView = (ListView)sender;
			if (DataContext is CollectionEditorVm vm)
			{
				vm.SelectedAllowedItems = listView.SelectedItems.OfType<ICollectionItemVm>().ToArray();
			}
		}

		private void OnForbiddenSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var listView = (ListView)sender;
			if (DataContext is CollectionEditorVm vm)
			{
				vm.SelectedForbiddenItems = listView.SelectedItems.OfType<ICollectionItemVm>().ToArray();
			}
		}
	}
}