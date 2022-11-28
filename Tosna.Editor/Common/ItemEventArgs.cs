using System;

namespace Tosna.Editor.Common
{
	public class ItemEventArgs<T> : EventArgs
	{
		public T Item { get; }

		public ItemEventArgs(T item)
		{
			Item = item;
		}
	}
}