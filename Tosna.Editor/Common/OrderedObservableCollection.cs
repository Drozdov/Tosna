using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace Tosna.Editor.Common
{
	public class OrderedObservableCollection<T> : ObservableCollection<T> where T : IComparable<T>
	{
		private bool suspendCollectionChangeNotification;

		protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			if (suspendCollectionChangeNotification)
			{
				return;
			}

			if (e.Action == NotifyCollectionChangedAction.Add && this.Any())
			{
				suspendCollectionChangeNotification = true;
				var items = this.ToArray().Union(e.NewItems.OfType<T>()).OrderBy(t => t);
				ClearItems();
				foreach (var item in items)
				{
					Add(item);
				}
				base.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
				suspendCollectionChangeNotification = false;
				return;
			}

			base.OnCollectionChanged(e);
		}
	}
}
