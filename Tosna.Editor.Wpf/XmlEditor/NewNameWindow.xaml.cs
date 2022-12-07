using System.Windows;
using System.Windows.Input;

namespace Tosna.Editor.Wpf.XmlEditor
{
	public partial class NewNameWindow
	{
		public NewNameWindow()
		{
			InitializeComponent();

			Height = 120;
			Width = 340;

			TextBox.Focus();
		}
		
		private void OnApplyButtonClicked(object sender, RoutedEventArgs e)
		{
			ApplyImpl();
		}

		private void OnCancelButtonClicked(object sender, RoutedEventArgs e)
		{
			CancelImpl();
		}

		private void OnKeyUp(object sender, KeyEventArgs e)
		{
			switch (e.Key)
			{
				case Key.Enter:
					ApplyImpl();
					return;
				
				case Key.Escape:
					CancelImpl();
					return;
			}
		}

		private void ApplyImpl()
		{
			DialogResult = true;
			Close();
		}
		private void CancelImpl()
		{
			Close();
		}
	}
}
