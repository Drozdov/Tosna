using System.Windows;
using System.Windows.Input;
using Tosna.Editor.IDE.Vm;

namespace Tosna.Editor.Wpf.IDE
{
	public partial class FilesManagerControl
	{
		public FilesManagerControl()
		{
			InitializeComponent();
		}

		protected override void OnPreviewMouseDoubleClick(MouseButtonEventArgs e)
		{
			if (DataContext is FilesHierarchyVm vm)
			{
				vm.LaunchEditorCommand.Execute(null);
			}
			e.Handled = true;
		}

		protected override void OnSelectedItemChanged(RoutedPropertyChangedEventArgs<object> e)
		{
			if (!(DataContext is FilesHierarchyVm vm)) return;

			if (SelectedItem is IFileSystemItemVm selectedItem)
			{
				vm.SelectedItems = new[] {selectedItem};
			}
			else
			{
				vm.SelectedItems = new IFileSystemItemVm[] { };
			}
		}
	}
}
