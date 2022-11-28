using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Tosna.Editor.Wpf.Converters
{
	public class BoolToVisibilityConverter : IValueConverter
	{
		public static BoolToVisibilityConverter Instance { get; } = new BoolToVisibilityConverter();

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value is bool b && b ? Visibility.Visible : Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value?.Equals(Visibility.Visible);
		}
	}
}