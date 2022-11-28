using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Tosna.Editor.Common;

namespace Tosna.Editor.IDE.Vm.CollectionsEditing
{
	public abstract class CollectionEditorVm
	{
		private IReadOnlyCollection<ICollectionItemVm> selectedAllowedItems = new ICollectionItemVm[] { };
		private IReadOnlyCollection<ICollectionItemVm> selectedForbiddenItems = new ICollectionItemVm[] { };
		public ObservableCollection<ICollectionItemVm> AllowedItems { get; } = new ObservableCollection<ICollectionItemVm>();

		public IReadOnlyCollection<ICollectionItemVm> SelectedAllowedItems
		{
			get => selectedAllowedItems;
			set
			{
				selectedAllowedItems = value;
				AllowSelectedCommand.RaiseCanExecuteChanged();
				ForbidSelectedCommand.RaiseCanExecuteChanged();
			}
		}

		public ObservableCollection<ICollectionItemVm> ForbiddenItems { get; } = new ObservableCollection<ICollectionItemVm>();

		public IReadOnlyCollection<ICollectionItemVm> SelectedForbiddenItems
		{
			get => selectedForbiddenItems;
			set
			{
				selectedForbiddenItems = value;
				AllowSelectedCommand.RaiseCanExecuteChanged();
				ForbidSelectedCommand.RaiseCanExecuteChanged();
			}
		}

		public ActionCommand AllowSelectedCommand { get; }
		
		public ActionCommand ForbidSelectedCommand { get; }

		protected CollectionEditorVm()
		{
			AllowSelectedCommand = new ActionCommand(AllowSelectedImpl, CanAllowSelected);
			ForbidSelectedCommand = new ActionCommand(ForbidSelectedImpl, CanForbidSelectedCommand);
		}
		
		private void AllowSelectedImpl()
		{
			foreach (var allowedItemVm in SelectedForbiddenItems.ToArray())
			{
				ForbiddenItems.Remove(allowedItemVm);
				AllowedItems.Add(allowedItemVm);
			}
			
			OnUpdate(AllowedItems, ForbiddenItems);
		}
		
		private bool CanAllowSelected()
		{
			return SelectedForbiddenItems?.Any() ?? false;
		}
		
		private void ForbidSelectedImpl()
		{
			foreach (var forbiddenItemVm in SelectedAllowedItems.ToArray())
			{
				AllowedItems.Remove(forbiddenItemVm);
				ForbiddenItems.Add(forbiddenItemVm);
			}
			
			OnUpdate(AllowedItems, ForbiddenItems);
		}
		
		private bool CanForbidSelectedCommand()
		{
			return SelectedAllowedItems?.Any() ?? false;
		}

		protected void Init(IEnumerable<ICollectionItemVm> allowedItems,
			IEnumerable<ICollectionItemVm> forbiddenItems)
		{
			foreach (var allowedItem in allowedItems)
			{
				AllowedItems.Add(allowedItem);
			}

			foreach (var forbiddenItem in forbiddenItems)
			{
				ForbiddenItems.Add(forbiddenItem);
			}
			
		}

		protected abstract void OnUpdate(IReadOnlyCollection<ICollectionItemVm> allowedItems, IReadOnlyCollection<ICollectionItemVm> forbiddenItems);
	}

	public interface ICollectionItemVm
	{
		string PublicName { get; }
	}
}